using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.PushNotification;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.Order;
using NowBuySell.Web.ViewModels.Api.Vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1/vendor")]
    public class VendorBookingController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IOrderService _orderService;
        private readonly ISPService _spService;
        private readonly ICustomerService _customerService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;
        private readonly ICustomerSessionService _customerSessionService;
        private readonly IVendorUserService _vendorUserService;
        private readonly IVendorWalletShareService _vendorWalletShareService;


        public VendorBookingController(IOrderService orderService, IOrderDetailsService orderDetailsService, ISPService sPService, ICustomerService customerService, INotificationReceiverService notificationReceiverService, INotificationService notificationService, ICustomerSessionService customerSessionService, IVendorUserService vendorUserService, IVendorWalletShareService vendorWalletShareService)
        {
            this._orderService = orderService;
            this._spService = sPService;
            this._customerService = customerService;
            this._notificationReceiverService = notificationReceiverService;
            this._notificationService = notificationService;
            this._customerSessionService = customerSessionService;
            this._vendorUserService = vendorUserService;
            this._vendorWalletShareService = vendorWalletShareService;
        }

        [HttpPost]
        [Route("bookings")]
        public HttpResponseMessage GetAllVendorBooking(OrderFilterViewModel orderFilter)
        {
            //string status = string.Empty;
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {

                    var orders = _orderService.GetFilteredOrders(orderFilter.StartDate, orderFilter.EndDate, vendorId, orderFilter.Status, orderFilter.PageNumber.HasValue ? orderFilter.PageNumber.Value : 1, orderFilter.SortBy.HasValue ? orderFilter.SortBy.Value : 1);

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        orders = orders.Select(i => new
                        {
                            orderID = i.ID,
                            bookingNo = i.OrderNo,
                            carName = i.CarName,
                            status = i.Status
                        }),
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
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }


        [HttpGet]
        [Route("stats")]
        public HttpResponseMessage DashboardStats()
        {
            //string status = string.Empty;
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {
                    var stats = _spService.GetVendorDashboardStats(vendorId);
                    var statusByRange = _spService.GetVendorDashboardStatusByRange(Helpers.TimeZone.GetLocalDateTime().AddDays(-30), Helpers.TimeZone.GetLocalDateTime().AddMinutes(1439), vendorId);

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        ordersStats = new
                        {
                            totalSales = statusByRange.TotalSale,
                            netSales = statusByRange.NetSale,
                            totalUsers = stats.NoOfUsers,
                            noOfCustomer = statusByRange.ActiveCustomer,
                            totalCars = stats.NoOfCars,
                            pendingBookings = statusByRange.PendingOrders,
                            completedBookings = statusByRange.CompletedOrders,
                            canceledBookings = statusByRange.CanceledOrders,
                            carApproval = stats.CarApprovals,
                            transferredAmount = statusByRange.TransferedAmountWallet,
                            pendingAmount = statusByRange.PendingAmountWallet

                        }

                    });

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }



        [HttpGet]
        [Route("{lang}/bookings/{orderId}")]
        public HttpResponseMessage GetOrderDetails(long orderId, string lang = "en")
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long staffId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out staffId))
                {

                    Order order = _orderService.GetOrder(orderId);

                    var orderDetails = _orderService.GetOrderByOrderID(orderId, lang);

                    var orderDetail = _orderService.GetOrderByOrderID(orderId).FirstOrDefault();

                    var customer = _customerService.GetCustomer((long)order.CustomerID);

                    if (order != null)
                    {
                        string ImageServer = CustomURL.GetImageServer();

                        var currentOrder = new
                        {
                            order.ID,
                            order.OrderNo,
                            order.Status,
                            car = new
                            {
                                id = orderDetail.CarID,
                                thumbnail = !string.IsNullOrEmpty(orderDetail.Thumbnail) ? ImageServer + orderDetail.Thumbnail : null,
                                name = orderDetail.CarName,
                                licensePlate = orderDetail.LicensePlate,
                                sku = orderDetail.SKU,
                            },

                            customer = new
                            {
                                id = customer.ID,
                                name = customer.Name,
                                image = !string.IsNullOrEmpty(customer.Logo) ? ImageServer + customer.Logo : null,
                                contact = customer.Contact
                            },


                            deliveryAddress = order.OrderDeliveryAddresses.Select(j => new
                            {
                                latitude = j.Latitude,
                                longitude = j.Longitude,
                                address = j.Address
                            }).FirstOrDefault(),

                            bookingInfo = new
                            {
                                fromDate = orderDetail.StartDateTime,
                                toDate = orderDetail.EndDateTime,
                                DeliveryMethod = order.SelfPickUp.HasValue && order.SelfPickUp.Value ? "Self Pickup" : "Delivery",
                                ExtraKilometers = orderDetail.ExtraKilometer,
                                Package = new
                                {
                                    name = orderDetail.PackageName,
                                },
                            },
                            paymentInfo = new
                            {
                                Method = order.PaymentMethod,
                                Status = order.IsPaid.HasValue && order.IsPaid.Value ? "Paid" : "Unpaid",
                                PaymentCaptured = order.PaymentCaptured.HasValue ? order.PaymentCaptured.Value : false,
                                charges = new
                                {
                                    RentalFee = orderDetail.Price,
                                    DeliveryCharges = order.DeliveryCharges,
                                    ExtraKilometerPrice = orderDetail.ExtraKilometerPrice,
                                    SubTotal = orderDetail.TotalPrice,
                                    CouponCode = order.CouponCode,
                                    CouponDiscount = order.CouponDiscount,
                                    RedeemAmount = order.RedeemAmount,
                                    Total = order.TotalAmount
                                }
                            },
                            orderDetail.Rating,
                            orderDetail.Remarks,
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            order = currentOrder
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { status = "error", message = "Invalid order id !" });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

        //[HttpGet]
        //[Route("bookings/{orderId}")]
        //public HttpResponseMessage GetOrderDetails(long orderId)
        //{
        //    try
        //    {
        //        var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
        //        var claims = identity.Claims;
        //        long staffId;
        //        if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out staffId))
        //        {

        //            Order order = _orderService.GetOrder(orderId);

        //            var orderDetails = _orderService.GetOrderByOrderID(orderId);


        //            if (order != null)
        //            {
        //                string ImageServer = CustomURL.GetImageServer();

        //                var currentOrder = new
        //                {
        //                    order.ID,
        //                    order.OrderNo,
        //                    order.Status,

        //                    details = orderDetails.Select(i => new
        //                    {
        //                        id = i.DetailID,
        //                        vendor = new
        //                        {
        //                            id = i.VendorID,
        //                            code = i.VendorCode,
        //                            name = i.VendorName,
        //                            logo = ImageServer + i.VendorLogo,
        //                        },

        //                        car = new
        //                        {
        //                            id = i.CarID,
        //                            name = i.CarName,
        //                            licensePlate = i.LicensePlate,
        //                            color = i.Color,
        //                            thumbnail = ImageServer + i.Thumbnail,
        //                            sku = i.SKU,

        //                        },

        //                        insurance = new
        //                        {
        //                            InsuranceId = i.InsuranceID,
        //                            InsuranceName = i.InsuranceName,
        //                            InsurancePrice = i.InsurancePrice,

        //                        },

        //                        i.Status,
        //                        i.PackageName,
        //                        i.StartDateTime,
        //                        i.EndDateTime,
        //                        i.Remarks,
        //                        i.Rating,
        //                        i.ExtraKilometer,
        //                        i.ExtraKilometerPrice,
        //                        i.PackagePrice,
        //                        TotalPrice = i.PackagePrice + i.ExtraKilometerPrice,

        //                    }),
        //                    DeliveryAddress = order.OrderDeliveryAddresses.Select(j => new
        //                    {
        //                        country = j.Area != null ? new
        //                        {
        //                            j.Country.ID,
        //                            j.Country.Name
        //                        } : null,
        //                        City = j.Area != null ? new
        //                        {
        //                            j.City.ID,
        //                            j.City.Name
        //                        } : null,
        //                        Area = j.Area != null ? new
        //                        {
        //                            j.Area.ID,
        //                            j.Area.Name
        //                        } : null,
        //                        j.Contact,
        //                        j.Address
        //                    }).FirstOrDefault()
        //                };

        //                return Request.CreateResponse(HttpStatusCode.OK, new
        //                {
        //                    status = "success",
        //                    order = currentOrder
        //                });
        //            }
        //            else
        //            {
        //                return Request.CreateResponse(HttpStatusCode.BadRequest, new { status = "error", message = "Invalid order id !" });
        //            }
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("Error", ex);
        //        //Logs.Write(ex);
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
        //    }
        //}


        [HttpPut]
        [Route("booking/status")]
        public HttpResponseMessage UpdateStatus(VendorBookingStatusViewModel vendorBookingStatusViewModel)
        {
            try
            {

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {

                    string message = string.Empty;

                    var currentOrder = _orderService.GetOrder(vendorBookingStatusViewModel.OrderID);
                    var vendor = _vendorUserService.GetVendorUserByVendorID(vendorId);


                    if (currentOrder != null)
                    {

                        currentOrder.Status = vendorBookingStatusViewModel.Status;



                        if (_orderService.UpdateOrder(ref currentOrder, ref message))
                        {
                            if (vendorBookingStatusViewModel.Status == "Completed")
                            {
                                _vendorWalletShareService.UpdateVendorEarning(currentOrder.ID);
                            }

                            if (vendorBookingStatusViewModel.Status == "Confirmed")
                            {
                                /*Order Email*/

                                IEnumerable<SP_GetOrderDetails_Result> OrderDetails = _orderService.GetOrderByOrderID((Int16)currentOrder.ID);
                                NowBuySell.Web.ViewModels.Order.OrderDetailViewModel Details = new NowBuySell.Web.ViewModels.Order.OrderDetailViewModel();

                                Details.CreatedOn = (DateTime)currentOrder.CreatedOn;
                                Details.OrderNo = currentOrder.OrderNo;
                                Details.DeliveryAddress = currentOrder.DeliveryAddress;
                                Details.Status = currentOrder.Status;
                                Details.ShipmentStatus = currentOrder.ShipmentStatus;
                                Details.CustomerName = currentOrder.Customer.Name;
                                Details.Currency = currentOrder.Currency;

                                Details.Amount = currentOrder.Amount.HasValue ? currentOrder.Amount.Value : 0;

                                Details.OrderTaxPercent = currentOrder.TaxPercent.HasValue ? currentOrder.TaxPercent.Value : 0;
                                Details.OrderTaxAmount = currentOrder.TaxAmount.HasValue ? currentOrder.TaxAmount.Value : 0;

                                Details.Shipping = currentOrder.DeliveryCharges.HasValue ? currentOrder.DeliveryCharges.Value : 0;
                                Details.CouponDiscount = currentOrder.CouponDiscount.HasValue ? currentOrder.CouponDiscount.Value : 0m;
                                Details.CouponCode = currentOrder.CouponCode != null ? currentOrder.CouponCode : "-";
                                Details.RedeemAmount = currentOrder.RedeemAmount.HasValue ? currentOrder.RedeemAmount.Value : 0m;

                                Details.TotalAmount = currentOrder.TotalAmount.HasValue ? currentOrder.TotalAmount.Value : 0;

                                var deliveryAddress = currentOrder.OrderDeliveryAddresses.FirstOrDefault();
                                if (deliveryAddress != null)
                                {
                                    Details.Country = deliveryAddress.Country != null ? deliveryAddress.Country.Name : "-";
                                    Details.City = deliveryAddress.City != null ? deliveryAddress.City.Name : "-";
                                    Details.Area = deliveryAddress.Area != null ? deliveryAddress.Area.Name : "-";
                                    Details.Address = deliveryAddress.Address;
                                }

                                Details.orderdetails = OrderDetails;

                                var body = ViewToStringRenderer.RenderViewToString("Orders", "Details", Details);

                                _orderService.SendOrderEmail("asim.technomites@gmail.com", "NowBuySell | Booking Placed", body);
                            }
                            Notification not = new Notification();

                            if (vendorBookingStatusViewModel.Status == "Pending")
                            {
                                not.Title = "Bookings Placed";
                                not.TitleAr = "Bookings Placed";
                                not.Description = string.Format("Your booking # {0} has been placed. You can check the booking status via booking details", currentOrder.OrderNo);
                                not.DescriptionAr = string.Format("Your booking # {0} has been placed. You can check the booking status via booking details", currentOrder.OrderNo);

                            }
                            else if (vendorBookingStatusViewModel.Status == "Confirmed")
                            {

                                not.Title = "Bookings Confirmed";
                                not.TitleAr = "Bookings Confirmed";
                                not.Description = string.Format("Your booking # {0} has been confirmed. You can check the booking status via booking details", currentOrder.OrderNo);
                                not.DescriptionAr = string.Format("Your booking # {0} has been confirmed. You can check the booking status via booking details", currentOrder.OrderNo);

                            }
                            else if (vendorBookingStatusViewModel.Status == "Processing")
                            {
                                not.Title = "Bookings Processed";
                                not.TitleAr = "Bookings Processed";
                                not.Description = string.Format("Your booking # {0} has been processed. You can check the booking status via booking details", currentOrder.OrderNo);
                                not.DescriptionAr = string.Format("Your booking # {0} has been processed. You can check the booking status via booking details", currentOrder.OrderNo);
                            }
                            else if (vendorBookingStatusViewModel.Status == "Completed")
                            {
                                not.Title = "Bookings Completed";
                                not.TitleAr = "Bookings Completed";
                                not.Description = string.Format("Your booking # {0} is completed. You can check the booking status via booking details", currentOrder.OrderNo);
                                not.DescriptionAr = string.Format("Your booking # {0} is completed. You can check the booking status via booking details", currentOrder.OrderNo);

                            }
                            else if (vendorBookingStatusViewModel.Status == "Canceled")
                            {
                                not.Title = "Bookings Canceled";
                                not.TitleAr = "Bookings Canceled";
                                not.Description = string.Format("Your booking # {0} has been canceled. You can check the booking status via booking details", currentOrder.OrderNo);
                                not.DescriptionAr = string.Format("Your booking # {0} has been canceled. You can check the booking status via booking details", currentOrder.OrderNo);

                            }

                            not.OriginatorID = vendorId;
                            not.OriginatorName = vendor.Name.ToString();
                            not.Module = "Booking";
                            not.OriginatorType = "Vendor";
                            not.RecordID = currentOrder.ID;
                            if (_notificationService.CreateNotification(not, ref message))
                            {
                                NotificationReceiver notRec = new NotificationReceiver();
                                notRec.ReceiverID = currentOrder.CustomerID;
                                notRec.ReceiverType = "Customer";
                                notRec.NotificationID = not.ID;
                                if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                                {
                                    var tokens = _customerSessionService.GetCustomerSessionFirebaseTokens((long)currentOrder.CustomerID, true, null);
                                    var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
                                    {
                                        Module = "Booking",
                                        RecordID = currentOrder.ID,
                                        OrderNo = currentOrder.OrderNo,
                                        NotificationID = notRec.ID
                                    });
                                }
                            }

                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = "Booking status updated!",

                            });

                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = "error",
                                message = "Something went wrong!"
                            });
                        }

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Conflict, new
                        {
                            status = "error",
                            message = "Invalid order id!"
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




    }
}
