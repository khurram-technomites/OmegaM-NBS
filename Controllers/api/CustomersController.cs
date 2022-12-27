using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Service.Helpers;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Account;
using NowBuySell.Web.ViewModels.Api.Customer;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [Authorize]
    [RoutePrefix("api/v1")]
    public class CustomersController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICustomerService _customerService;
        private readonly ICustomerLoyaltySettingService _customerLoyaltySettingService;
        private readonly ICustomerDeliveryAddressService _customerDeliveryAddressService;
        private readonly ICustomerDocumentService _customerDocumentService;
        private readonly ICarRequestsService _carRequestsService;
        private readonly IPropertyRequestsService _propertyRequestsService;
        private readonly IMail _email;


        public CustomersController(ICustomerService customerService, ICustomerLoyaltySettingService customerLoyaltySettingService, ICustomerDeliveryAddressService customerDeliveryAddressService, ICustomerDocumentService customerDocumentService, ICarRequestsService carRequestsService, IPropertyRequestsService propertyRequestsService, IMail email)
        {
            this._customerService = customerService;
            this._customerLoyaltySettingService = customerLoyaltySettingService;
            this._customerDeliveryAddressService = customerDeliveryAddressService;
            this._customerDocumentService = customerDocumentService;
            this._carRequestsService = carRequestsService;
            this._propertyRequestsService = propertyRequestsService;

            _email = email;
        }

        [HttpPost]
        [Route("customer/changepassword")]
        public HttpResponseMessage ChangePassword(NowBuySell.Web.ViewModels.Api.Customer.ChangePasswordViewModel changePasswordViewModel, string lang = "en")
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                    var claims = identity.Claims;
                    long customerId;
                    if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                    {
                        string status = string.Empty;
                        string message = string.Empty;
                        if (_customerService.ChangePassword(changePasswordViewModel.CurrentPassword, changePasswordViewModel.NewPassword, customerId, ref status, ref message))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = status,
                                message = lang == "en" ? message : ArabicDictionary.Translate(message, false)
                            });
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = status,
                            message = lang == "en" ? message : ArabicDictionary.Translate(message, false)
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                        {
                            status = "error",
                            message = lang == "en" ? "Session invalid or expired" : ArabicDictionary.Translate("Session invalid or expired", false)
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "error",
                        message = lang == "en" ? "Bad request!" : ArabicDictionary.Translate("Bad request!", false),
                        description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
                    });
                }
            }
            catch (Exception ex)
            {
                //log.Error("Error", ex);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                });
            }
        }

        [HttpGet]
        [Route("customer/profile")]
        public HttpResponseMessage CustomerProfile()
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    Customer customer = _customerService.GetCustomer((long)customerId);
                    var customerLoyaltySetting = _customerLoyaltySettingService.GetCustomerLoyaltySetting(customer.AccountType);
                    /*var defaultAddress = _customerDeliveryAddressService.GetCustomerDefaultDeliveryAddresses(customerId);*/
                    var document = _customerDocumentService.GetCustomerDocumentByCustomerID(customerId);

                    string ImageServer = CustomURL.GetImageServer();
                    if (customer != null)
                    {
                        var customerModel = new
                        {
                            id = customer.ID,
                            name = customer.Name == null ? "" : customer.Name,
                            logo = ImageServer + customer.Logo,
                            email = customer.Email == null ? "" : customer.Email,
                            contact = customer.Contact == null ? "" : customer.Contact,
                            newContact = customer.TempNumber == null ? "" : customer.TempNumber,
                            isContactUpdated = customer.TempNumber == null ? false : true,
                            country = (customer != null && customer.Country != null) ? new { id = customer.Country.ID, name = customer.Country.Name } : null,
                            state = (customer != null && customer.City != null) ? new { id = customer.City.ID, name = customer.City.Name } : null,
                            area = (customer != null && customer.Area != null) ? new { id = customer.Area.ID, name = customer.Area.Name } : null,
                            address = customer != null ? customer.Address : null,

                        };

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            customer = customerModel
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                        {
                            status = "error",
                            message = "Session invalid or expired !"
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = "Session invalid or expired !"
                    });
                }
            }
            catch (Exception ex)
            {
                //log.Error("Error", ex);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpPut]
        [Route("customer/profile")]
        public async Task<HttpResponseMessage> CustomerProfile(ProfileViewModel viewModel, string lang = "en")
        {
            try
            {
                if (ModelState.IsValid)
                {
                    NowBuySell.Web.Helpers.Encryption.Crypto objCrypto = new NowBuySell.Web.Helpers.Encryption.Crypto();

                    bool IsNumberChanged = false, IsEmailChanged = false;
                    var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                    var claims = identity.Claims;
                    long customerId;
                    if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                    {
                        string message = string.Empty;
                        string status = string.Empty;
                        Customer customer = _customerService.GetCustomer(customerId);

                        if (customer != null)
                        {
                            customer.Name = viewModel.Name;

                            customer.CityID = viewModel.CityId;
                            customer.AreaID = viewModel.AreaId;
                            customer.CountryID = viewModel.CountryId;
                            customer.Address = viewModel.Address;
                            if ((customer.Contact == viewModel.Contact) || viewModel.Contact == null)
                            {
                                IsNumberChanged = false;
                                customer.TempNumber = null;
                            }
                            else if (!_customerService.ValidateContactNumber(viewModel.Contact))
                            {
                                IsNumberChanged = true;
                                customer.TempNumber = viewModel.Contact;
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.BadRequest, new
                                {
                                    status = "false",
                                    message = lang == "en" ? "Contact number is being used by another account" : ArabicDictionary.Translate("Contact number is being used by another account", false)
                                });
                            }

                            if (viewModel.Email != customer.Email)
                            {
                                customer.IsEmailVerified = false;
                                customer.AuthorizationCode = objCrypto.Random(225);
                                IsEmailChanged = true;
                            }

                            customer.Email = viewModel.Email;

                            if (_customerService.UpdateCustomer(ref customer, ref message, ref status))
                            {
                                if (IsNumberChanged)
                                {
                                    bool isSent = await _customerService.SendOTP(customer.TempNumber);
                                }

                                if (IsEmailChanged)
                                {
                                    var path = System.Web.Hosting.HostingEnvironment.MapPath("~/");
                                    bool emailSend = _email.SendVerificationMail(customer.Name, customer.Email, CustomURL.GetFormatedURL("/Customer/Verify?auth=" + customer.AuthorizationCode), path);
                                }

                                return Request.CreateResponse(HttpStatusCode.OK, new
                                {
                                    status = "success",
                                    message = lang == "en" ? "Profile updated!" : ArabicDictionary.Translate("Profile updated!", false),
                                    newNumber = customer.TempNumber,
                                    IsNumberChanged = IsNumberChanged,
                                    customer = new
                                    {
                                        id = customer.ID,
                                        name = customer.Name,
                                        email = customer.Email,
                                        contact = customer.Contact,
                                        accountType = customer.AccountType,
                                        country = customer.Country != null ? new { id = customer.Country.ID, name = customer.Country.Name } : null,
                                        state = customer.City != null ? new { id = customer.City.ID, name = customer.City.Name } : null,
                                        area = customer.Area != null ? new { id = customer.Area.ID, name = customer.Area.Name } : null,
                                        address = customer.Address
                                    }
                                });
                            }
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = status,
                                message = lang == "en" ? message : ArabicDictionary.Translate(message)
                            });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.Conflict, new
                            {
                                status = "error",
                                message = lang == "en" ? "Account already exist!" : ArabicDictionary.Translate("Account already exist!", false)
                            });
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                        {
                            status = "error",
                            message = lang == "en" ? "Session invalid or expired !" : ArabicDictionary.Translate("Session invalid or expired", false)
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "error",
                        message = lang == "en" ? "Bad request!" : ArabicDictionary.Translate("Bad request!", false),
                        description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                });
            }
        }

        [HttpPut]
        [Route("customer/pushnotification")]
        public HttpResponseMessage CustomerPushNotification(PushNotificationViewMode pushNotificationViewMode)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                    var claims = identity.Claims;
                    long customerId;
                    if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                    {
                        string message = string.Empty;
                        string status = string.Empty;
                        Customer customer = _customerService.GetCustomer(customerId);
                        if (pushNotificationViewMode.AllowPushNotification != customer.IsPushNotificationAllowed || pushNotificationViewMode.AllowBookingNotification != customer.IsBookingNoticationAllowed)
                        {
                            customer.IsPushNotificationAllowed = pushNotificationViewMode.AllowPushNotification;
                            customer.IsBookingNoticationAllowed = pushNotificationViewMode.AllowBookingNotification;
                            if (_customerService.UpdateCustomer(ref customer, ref message, ref status))
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new
                                {
                                    status = "success",
                                    message = "Notification settings updated!",
                                    customer = new
                                    {
                                        id = customer.ID,
                                        name = customer.Name,
                                        email = customer.Email,
                                        contact = customer.Contact,
                                        allowPushNotifications = customer.IsPushNotificationAllowed,
                                        country = customer.Country != null ? new { id = customer.Country.ID, name = customer.Country.Name } : null,
                                        state = customer.City != null ? new { id = customer.City.ID, name = customer.City.Name } : null,
                                        area = customer.Area != null ? new { id = customer.Area.ID, name = customer.Area.Name } : null,
                                        address = customer.Address
                                    }
                                });
                            }
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = status,
                                message = message
                            });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = status,
                                message = "Setting already exist"
                            });

                        }


                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                        {
                            status = "error",
                            message = "Session invalid or expired !"
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "error",
                        message = "Bad request !",
                        description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
                    });
                }
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

        [HttpPut]
        [Route("customer/profile/photo")]
        public async Task<HttpResponseMessage> UpdateProfilePhoto()
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string message = string.Empty;
                    string status = string.Empty;
                    string filePath = "/Assets/AppFiles/Customer";
                    string root = HttpContext.Current.Server.MapPath(filePath);
                    var provider = new CustomMultipartFormDataStreamProvider(root);

                    CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);

                    string path = string.Empty;
                    if (file.Contents.Count() > 0)
                    {
                        path = filePath + "/" + file.filePath;

                        var customer = _customerService.GetCustomer(customerId);
                        customer.Logo = path;

                        if (_customerService.UpdateCustomer(ref customer, ref message, ref status))
                        {
                            string ImageServer = CustomURL.GetImageServer();
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = "Profile image updated!",
                                image = ImageServer + customer.Logo
                            });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = status,
                                message = message
                            });
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new
                        {
                            status = "error",
                            message = "Bad request !",
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = "Session invalid or expired !"
                    });
                }
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

        [HttpPost]
        [Route("customer/carrequests")]
        public HttpResponseMessage Carrequest(CarRequest Model)
        {

            string message = string.Empty;
            Model.CreatedOn = DateTime.Now;
            if (_carRequestsService.AddCarRequests(Model, ref message))
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = message });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "An unknown error occured, please try again" });
        }
        [HttpPost]
        [Route("customer/propertyrequests")]
        public HttpResponseMessage Propertyrequest(PropertyRequest Model)
        {

            string message = string.Empty;
            Model.CreatedOn = DateTime.Now;
            if (_propertyRequestsService.AddPropertyRequests(Model, ref message))
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = message });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "An unknown error occured, please try again" });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("customer/contact")]
        public async Task<HttpResponseMessage> GetCustomerContact(LoginViewModel Model, string lang = "en")
        {
            if (Model == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Email or password is missing" });
            }
            else
            {
                var customer = _customerService.GetCustomerByEmail(Model.EmailAddress);
                if (customer != null)
                {
                    if (customer.IsActive.HasValue && customer.IsActive.Value)
                    {
                        string RetrievedPass = new NowBuySell.Web.Helpers.Encryption.Crypto().RetrieveHash(Model.Password, Convert.ToString(customer.Salt.Value));

                        if (RetrievedPass.Equals(customer.Password))
                        {
                            bool isSent = await _customerService.SendOTP(customer.Contact);

                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                success = true,
                                message = customer.Contact,
                                isSent
                            });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = lang == "en" ? "Incorrect password" : ArabicDictionary.Translate("Incorrect password", false) });
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = lang == "en" ? "Account suspended!" : ArabicDictionary.Translate("Account suspended!", false) });
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = lang == "en" ? "Account suspended!" : ArabicDictionary.Translate("Account suspended!", false) });
            }
        }
    }
}
