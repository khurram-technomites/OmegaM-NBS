using Newtonsoft.Json;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.ViewModels.Vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api.vendorApi
{
    [Authorize]
    [RoutePrefix("api/v1/vendor")]
    public class VendorPackagesController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IVendorPackagesService _vendorPackagesService;
        private readonly IVendorService _vendorService;
        private readonly IVendorUserService _vendorUserService;
        private readonly IMyFatoorahPaymentGatewaySettingsService _myFatoorahPaymentGatewaySettingsService;
        private readonly IVendorTransactionHistoryService _vendorTransactionHistory;
        private readonly ITransactionService _transactionService;
        private readonly INotificationService _notificationService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly IPropertyService _propertyService;
        private readonly ICarService _carService;

        public VendorPackagesController(IVendorPackagesService vendorPackagesService, IVendorService vendorService,
            IMyFatoorahPaymentGatewaySettingsService myFatoorahPaymentGatewaySettingsService, IVendorTransactionHistoryService vendorTransactionHistory,
            ITransactionService transactionService, INotificationService notificationService, INotificationReceiverService notificationReceiverService,
            IVendorUserService vendorUserService, IPropertyService propertyService, ICarService carService)
        {
            _vendorPackagesService = vendorPackagesService;
            _vendorService = vendorService;
            _myFatoorahPaymentGatewaySettingsService = myFatoorahPaymentGatewaySettingsService;
            _vendorTransactionHistory = vendorTransactionHistory;

            _transactionService = transactionService;
            _notificationService = notificationService;
            _notificationReceiverService = notificationReceiverService;
            _vendorUserService = vendorUserService;
            _propertyService = propertyService;
            _carService = carService;
        }

        [HttpGet]
        [Route("packages")]
        public HttpResponseMessage GetPackages()
        {
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims;
            long userId;
            long VendorID = 0, VendorPackageID = 0;
            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out userId))
            {
                string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
                if (ReceiverType == "Vendor")
                {
                    VendorID = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
                }

                var vendor = _vendorService.GetVendor(VendorID);

                if (vendor.VendorPackageID.HasValue)
                    VendorPackageID = vendor.VendorPackageID.Value;

                try
                {
                    var packages = _vendorPackagesService.GetAll(true).Select(i => new
                    {
                        i.ID,
                        i.Name,
                        i.Description,
                        i.Price,
                        i.BillingPeriod,
                        IsSelected = i.ID == VendorPackageID ? true : false
                    });

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        packages = packages
                    });

                }
                catch (Exception ex)
                {
                    log.Error("Error", ex);
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                    {
                        status = "failure",
                        message = "Oops! Something went wrong. Please try later."
                    });
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                {
                    status = "failure",
                    message = "Authorization failed for current user"
                });
            }
        }

        [HttpPut]
        [Route("package/paid/{PackageId}")]

        public HttpResponseMessage Paid(int PackageId, decimal CompensationAmount = 0, string invoiceId = null)
        {
            VendorTransactionHistory transactionHistory = new VendorTransactionHistory();
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserID, vendorID = 0;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserID))
                {
                    string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
                    if (ReceiverType == "Vendor")
                    {
                        vendorID = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
                    }

                    var Package = _vendorPackagesService.GetById((int)PackageId);
                    Vendor vendorModel = _vendorService.GetVendor(vendorID);
                    if (Package != null)
                    {
                        if(Package.IsFree.HasValue && Package.IsFree.Value)
                        {
                            string message = string.Empty;

                            transactionHistory.IsSuccess = true;
                            transactionHistory.Status = "Paid";
                            transactionHistory.VendorID = vendorID;
                            transactionHistory.VendorPackageID = PackageId;
                            transactionHistory.Price = 0;
                            transactionHistory.CompensationAmount = CompensationAmount;
                            transactionHistory.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();

                            if (_vendorTransactionHistory.AddVendorTransactionHistory(transactionHistory, ref message))
                            {
                                Vendor vendor = new Vendor();

                                vendor = _vendorService.GetVendor(vendorID);

                                vendor.VendorPackageID = PackageId;
                                vendor.PackageStartDate = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
                                vendor.PackageEndDate = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime().AddMonths(Package.MonthCount.Value);

                                if (_vendorService.UpdateVendor(ref vendor, ref message))
                                {
                                    //TempData["IsPaid"] = string.Format("Payment of AED {0} recieved against your selected package", Payment.InvoiceValue);
                                }

                                Transaction transaction = new Transaction()
                                {
                                    VendorID = vendorID,
                                    PaymentRef = vendorModel.VendorCode,
                                    NameOnCard = vendorModel.Name,
                                    MaskCardNo = string.Empty,
                                    TransactionStatus = "Success",
                                    Amount = 0
                                };

                                _transactionService.CreateTransaction(transaction, ref message);

                                Notification not = new Notification();
                                not.Title = "Package Invoiced";
                                not.Description = string.Format("Order {0} payment recieved against package from {1}", Package.Name, vendor.Name);
                                not.Url = "/Admin/Transaction/Index";
                                not.Module = "Invoiced";
                                not.OriginatorType = "Vendor";
                                not.RecordID = Package.ID;

                                if (_notificationService.CreateNotification(not, ref message))
                                {
                                    if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", null))
                                    {
                                    }
                                }
                                return Request.CreateResponse(HttpStatusCode.OK, new
                                {
                                    status = "success",
                                    message = string.Format("Payment against Package {0} CAPTURED", Package.Name)
                                });
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new
                                {
                                    status = "failed",
                                    message = "Oops! Something went wrong. Please try later."
                                });
                            }
                        }
                        else if (!string.IsNullOrEmpty(invoiceId))
                        {
                            try
                            {
                                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                                string TestAPIKey = "rLtt6JWvbUHDDhsZnfpAhpYk4dxYDQkbcPTyGaKp2TYqQgG7FGZ5Th_WD53Oq8Ebz6A53njUoo1w3pjU1D4vs_ZMqFiz_j0urb_BH9Oq9VZoKFoJEDAbR" +
                                    "ZepGcQanImyYrry7Kt6MnMdgfG5jn4HngWoRdKduNNyP4kzcp3mRv7x00ahkm9LAK7ZRieg7k1PDAnBIOG3EyVSJ5kK4WLMvYr7sCwHbHcu4A5WwelxYK0GMJy37bNAarSJDFQsJ2ZvJj" +
                                    "vMDmfWwDVFEVe_5tOomfVNt6bOg9mexbGjMrnHBnKnZR1vQbBtQieDlQepzTZMuQrSuKn-t5XZM7V6fCW7oP-uXGX-sMOajeX65JOf6XVpk29DP6ro8WTAflCDANC193yof8-f5_EY" +
                                    "Y-3hXhJj7RBXmizDpneEQDSaSz5sFk0sV5qPcARJ9zGG73vuGFyenjPPmtDtXtpx35A-BVcOSBYVIWe9kndG3nclfefjKEuZ3m4jL9Gg1h2JBvmXSMYiZtp9MR5I6pvbvylU_PP5xJF" +
                                    "SjVTIz7IQSjcVGO41npnwIxRXNRxFOdIUHn0tjQ-7LwvEcTXyPsHXcMD8WtgBh-wxR8aKX7WPSsT1O8d8reb2aR7K3rkV3K82K_0OgawImEpwSvp9MNKynEAJQS6ZHe_J_l77652xwPNxMRTMASk1ZsJL";

                                var myFatoorahSetting = _myFatoorahPaymentGatewaySettingsService.GetDefaultPaymentGatewaySetting();

                                string Endpoint = myFatoorahSetting.IsLive.HasValue && myFatoorahSetting.IsLive.Value ? myFatoorahSetting.LiveEndpoint : myFatoorahSetting.TestEndpoint;
                                string APIKey = myFatoorahSetting.IsLive.HasValue && myFatoorahSetting.IsLive.Value ? myFatoorahSetting.APIKey : TestAPIKey;

                                using (var client = new HttpClient())
                                {
                                    /*Fetch  Access Token From N-Genius*/
                                    var body = new
                                    {
                                        Key = invoiceId,
                                        KeyType = "InvoiceId"
                                    };

                                    client.BaseAddress = new Uri(Endpoint);
                                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + APIKey);

                                    var json = JsonConvert.SerializeObject(body);

                                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                                    var response = client.PostAsync("/v2/GetPaymentStatus", content).Result;

                                    if (response.IsSuccessStatusCode)
                                    {
                                        var paymentResponse = JsonConvert.DeserializeObject<NowBuySell.Web.Helpers.Payments.MyFatoorah.Capture.PaymentInquiryResponse>(response.Content.ReadAsStringAsync().Result);
                                        if (paymentResponse.IsSuccess)
                                        {
                                            string message = string.Empty;

                                            var Payment = paymentResponse.Data;

                                            if ((Payment != null && Payment.InvoiceStatus == "Paid"))
                                            {
                                                var InvoiceTransaction = Payment.InvoiceTransactions.Where(i => i.TransactionStatus == "Succss").OrderByDescending(i => i.TransactionDate).FirstOrDefault();

                                                transactionHistory.IsSuccess = true;
                                                transactionHistory.Status = Payment.InvoiceStatus;
                                                transactionHistory.VendorID = vendorID;
                                                transactionHistory.VendorPackageID = PackageId;
                                                transactionHistory.Price = Convert.ToDecimal(Payment.InvoiceValue);
                                                transactionHistory.CompensationAmount = CompensationAmount;
                                                transactionHistory.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();

                                                if (_vendorTransactionHistory.AddVendorTransactionHistory(transactionHistory, ref message))
                                                {
                                                    Vendor vendor = new Vendor();

                                                    vendor = _vendorService.GetVendor(vendorID);

                                                    vendor.VendorPackageID = PackageId;
                                                    vendor.PackageStartDate = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
                                                    vendor.PackageEndDate = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime().AddMonths(Package.MonthCount.Value);

                                                    if (_vendorService.UpdateVendor(ref vendor, ref message))
                                                    {
                                                        //TempData["IsPaid"] = string.Format("Payment of AED {0} recieved against your selected package", Payment.InvoiceValue);
                                                    }

                                                    Transaction transaction = new Transaction()
                                                    {
                                                        VendorID = vendorID,
                                                        PaymentRef = vendorModel.VendorCode,
                                                        NameOnCard = Payment.CustomerName,
                                                        MaskCardNo = InvoiceTransaction.CardNumber,
                                                        TransactionStatus = InvoiceTransaction.TransactionStatus,
                                                        Amount = Convert.ToDecimal(InvoiceTransaction.TransationValue)
                                                    };

                                                    _transactionService.CreateTransaction(transaction, ref message);

                                                    Notification not = new Notification();
                                                    not.Title = "Package Invoiced";
                                                    not.Description = string.Format("Order {0} payment recieved against package from {1}", Package.Name, vendor.Name);
                                                    not.Url = "/Admin/Transaction/Index";
                                                    not.Module = "Invoiced";
                                                    not.OriginatorType = "Vendor";
                                                    not.RecordID = Package.ID;

                                                    if (_notificationService.CreateNotification(not, ref message))
                                                    {
                                                        if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", null))
                                                        {
                                                        }
                                                    }
                                                    return Request.CreateResponse(HttpStatusCode.OK, new
                                                    {
                                                        status = "success",
                                                        message = string.Format("Payment against Package {0} CAPTURED", Package.Name)
                                                    });
                                                }
                                                else
                                                {
                                                    return Request.CreateResponse(HttpStatusCode.OK, new
                                                    {
                                                        status = "failed",
                                                        message = "Oops! Something went wrong. Please try later."
                                                    });
                                                }
                                            }
                                            else if ((Payment != null && Payment.InvoiceStatus == "Pending"))
                                            {
                                                var InvoiceTransaction = Payment.InvoiceTransactions.Where(i => i.TransactionStatus == "Succss").OrderByDescending(i => i.TransactionDate).FirstOrDefault();

                                                transactionHistory.IsSuccess = false;
                                                transactionHistory.Status = Payment.InvoiceStatus;
                                                transactionHistory.VendorID = vendorID;
                                                transactionHistory.VendorPackageID = PackageId;
                                                transactionHistory.Price = Convert.ToDecimal(Payment.InvoiceValue);
                                                transactionHistory.CompensationAmount = CompensationAmount;
                                                transactionHistory.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();

                                                if (_vendorTransactionHistory.AddVendorTransactionHistory(transactionHistory, ref message))
                                                {
                                                    return Request.CreateResponse(HttpStatusCode.OK, new
                                                    {
                                                        status = "success",
                                                        message = string.Format("Payment PENDING against Package {0}", Package.Name)
                                                    });
                                                }
                                                else
                                                {
                                                    return Request.CreateResponse(HttpStatusCode.OK, new
                                                    {
                                                        status = "failed",
                                                        message = "Oops! Something went wrong. Please try later."
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                var InvoiceTransaction = Payment.InvoiceTransactions.Where(i => i.TransactionStatus == "Failed").OrderByDescending(i => i.TransactionDate).FirstOrDefault();

                                                if (InvoiceTransaction != null)
                                                {
                                                    transactionHistory.IsSuccess = false;
                                                    transactionHistory.Status = "Failed";
                                                    transactionHistory.VendorID = vendorID;
                                                    transactionHistory.VendorPackageID = PackageId;
                                                    transactionHistory.Price = Convert.ToDecimal(Payment.InvoiceValue);
                                                    transactionHistory.CompensationAmount = CompensationAmount;
                                                    transactionHistory.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();

                                                    if (_vendorTransactionHistory.AddVendorTransactionHistory(transactionHistory, ref message))
                                                    {
                                                        return Request.CreateResponse(HttpStatusCode.OK, new
                                                        {
                                                            status = "failed",
                                                            message = string.Format("Payment FAILED against Package {0}", Package.Name)
                                                        });
                                                    }
                                                    else
                                                    {
                                                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                                                        {
                                                            status = "failed",
                                                            message = "Oops! Something went wrong while processing for payment. Please try later."
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                                                    {
                                                        status = "failed",
                                                        message = "Oops! Something went wrong while processing for payment. Please try later."
                                                    });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                                            {
                                                status = "failed",
                                                message = "Oops! Something went wrong while processing for payment. Please try later."
                                            });
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                                        {
                                            status = "failed",
                                            message = "Oops! Something went wrong while processing for payment. Please try later."
                                        });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                                {
                                    status = "failed",
                                    message = "Oops! Something went wrong while processing for payment. Please try later."
                                });
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, new
                            {
                                status = "failed",
                                message = "Oops! Something went wrong while processing for payment. Please try later."
                            });
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new
                        {
                            status = "failed",
                            message = "Oops! Something went wrong while processing for payment. Please try later."
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "failed",
                        message = "Authorization failed for current user"
                    });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failed",
                    message = "Oops! Something went wrong while processing for payment. Please try later."
                });
            }

        }

        [HttpGet]
        [Route("package/{PackageId}/IsAllowed")]
        public HttpResponseMessage IsPackageAllowed(int PackageId)
        {
            try
            {
                VendorPackageViewModel result = new VendorPackageViewModel();
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserID, vendorID = 0;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserID))
                {
                    string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
                    if (ReceiverType == "Vendor")
                    {
                        vendorID = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
                    }

                    DateTime currentDateTime = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
                    int PropertyListingCount = _propertyService.GetVendorLimit(vendorID);
                    int CarListingCount = _carService.GetVendorLimit(vendorID);
                    int noOfDaysLeft = 0, totalNumberOfDays = 0;
                    decimal costPerDay = 0, costForDaysLeft = 0, packageCost = 0;
                    Vendor vendor = _vendorService.GetVendor((long)vendorID);
                    var list = _vendorPackagesService.GetAll(true).Where(x => x.IsActive == true && x.ID == PackageId).FirstOrDefault();
                    var VendorPackage = _vendorPackagesService.GetAll().Where(x =>  x.ID == vendor.VendorPackageID).FirstOrDefault();

                    if (vendor.PackageEndDate.HasValue && vendor.PackageEndDate.Value > currentDateTime)
                    {
                        noOfDaysLeft = (vendor.PackageEndDate.Value.Date - currentDateTime.Date).Days;
                        totalNumberOfDays = (vendor.PackageEndDate.Value - vendor.PackageStartDate.Value).Days;
                        costPerDay = VendorPackage.Price.Value / totalNumberOfDays;
                        costForDaysLeft = noOfDaysLeft * costPerDay;

                    }
                    packageCost = list.Price.Value - costForDaysLeft;
                    packageCost = packageCost < 0 ? list.Price.Value : packageCost;
                    //1/7 < 3/7

                    if (vendor.PackageEndDate.HasValue && vendor.PackageEndDate.Value.Date > Helpers.TimeZone.GetLocalDateTime().Date)
                    {
                        if ((noOfDaysLeft > 0 && VendorPackage.Price.Value > list.Price.Value) ||
                        (vendor.VendorPackageID.HasValue && list.ID == vendor.VendorPackageID) ||
                        vendor.VendorPackageID.HasValue && list.IsFree.HasValue && list.IsFree.Value)
                            result.IsAllowed = false;
                    }


                    //if (list.PropertyLimit.HasValue && PropertyListingCount > list.PropertyLimit)
                    //{
                    //    result.PropOverflowMessage = "* Your current property listing count exceeds the listing limit for this package, delete atleast " + (PropertyListingCount - list.PropertyLimit).ToString() +
                    //        " properties to aquire this package";

                    //    result.IsAllowed = false;
                    //}

                    //if (list.MotorLimit.HasValue && CarListingCount > list.MotorLimit)
                    //{
                    //    result.CarOverflowMessage = "* Your current motor listing count exceeds the listing limit for this package, delete atleast " + (CarListingCount - list.MotorLimit).ToString() +
                    //        " motors to aquire this package";

                    //    result.IsAllowed = false;
                    //}

                    result = MapVendorPackages(list, result);

                    result.CostForDaysLeft = costForDaysLeft.ToString("n2");
                    result.CostPerDay = costPerDay.ToString("n2");
                    result.TotalNumberOfDays = totalNumberOfDays;
                    result.NoOfDaysLeft = noOfDaysLeft;
                    result.PriceToPay = packageCost.ToString("n2");
                    result.PackagePrice = list.Price.Value;

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = string.Empty,
                        package = result
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "failed",
                        message = "Authorization failed for current user"
                    });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Something went wrong. Please try again.",
                    package = new { }
                });
            }
        }

        private VendorPackageViewModel MapVendorPackages(VendorPackage Model, VendorPackageViewModel result)
        {
            result.ID = Model.ID;
            result.Name = Model.Name;
            result.NameAr = Model.NameAr;
            result.Price = Model.Price;
            result.Description = Model.Description;
            result.DescriptionAr = Model.DescriptionAr;
            result.BillingPeriod = Model.BillingPeriod;
            result.hasMotorModule = Model.hasMotorModule;
            result.hasPropertyModule = Model.hasPropertyModule;
            result.IsActive = Model.IsActive;
            result.IsDeleted = Model.IsDeleted;
            result.CreatedOn = Model.CreatedOn;
            result.MotorLimit = Model.MotorLimit;
            result.PropertyLimit = Model.PropertyLimit;
            result.IsFree = Model.IsFree;
            result.MonthCount = Model.MonthCount;

            return result;
        }
    }
}
