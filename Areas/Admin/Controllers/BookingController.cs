using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using LinqToExcel;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class BookingController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IBookingService _bookingService;
        private readonly ICountryService _countryService;

        public BookingController(IBookingService bookingService, ICountryService _countryService)
        {
            this._bookingService = bookingService;
            this._countryService = _countryService;
        }

        // GET: Admin/Booking
        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.ExcelUploadErrorMessage = TempData["ExcelUploadErrorMessage"];

            return View();
        }

        public ActionResult List()
        {
            var bookings = _bookingService.GetBookings();
            return PartialView(bookings);
        }
        public ActionResult StatusChange(long id)
        {
            Booking order = _bookingService.GetBooking((long)id);
            return View(order);
        }
        [HttpPost]
        public ActionResult StatusChange(Booking orderID, string status)
        {
            string message = string.Empty;
            Booking order = _bookingService.GetBooking((long)orderID.ID);
            order.Status = status;
            if (_bookingService.UpdateBooking(ref order, ref message))
            {
                //if (order.Status == "Delivered")
                //{
                //    _vendorWalletShareService.UpdateVendorEarning(orderID.ID);
                //}

                //if (order.Status == "Confirmed")
                //{
                //    /*Order Email*/
                //    var OrderModel = _orderService.GetOrder((long)orderID.ID);
                //    IEnumerable<SP_GetOrderDetails_Result> OrderDetails = _orderService.GetOrderByOrderID((Int16)OrderModel.ID);
                //    OrderDetailViewModel Details = new OrderDetailViewModel();

                //    Details.CreatedOn = (DateTime)OrderModel.CreatedOn;
                //    Details.OrderNo = OrderModel.OrderNo;
                //    Details.DeliveryAddress = OrderModel.DeliveryAddress;
                //    Details.Status = OrderModel.Status;
                //    Details.ShipmentStatus = OrderModel.ShipmentStatus;
                //    Details.CustomerName = OrderModel.Customer.Name;
                //    Details.Currency = OrderModel.Currency;

                //    Details.Amount = OrderModel.Amount.HasValue ? OrderModel.Amount.Value : 0;

                //    Details.OrderTaxPercent = OrderModel.TaxPercent.HasValue ? OrderModel.TaxPercent.Value : 0;
                //    Details.OrderTaxAmount = OrderModel.TaxAmount.HasValue ? OrderModel.TaxAmount.Value : 0;

                //    Details.Shipping = OrderModel.DeliveryCharges.HasValue ? OrderModel.DeliveryCharges.Value : 0;
                //    Details.CouponDiscount = OrderModel.CouponDiscount.HasValue ? OrderModel.CouponDiscount.Value : 0m;
                //    Details.CouponCode = OrderModel.CouponCode != null ? OrderModel.CouponCode : "-";
                //    Details.RedeemAmount = OrderModel.RedeemAmount.HasValue ? OrderModel.RedeemAmount.Value : 0m;

                //    Details.TotalAmount = OrderModel.TotalAmount.HasValue ? OrderModel.TotalAmount.Value : 0;

                //    var deliveryAddress = OrderModel.OrderDeliveryAddresses.FirstOrDefault();
                //    if (deliveryAddress != null)
                //    {
                //        Details.Country = deliveryAddress.Country != null ? deliveryAddress.Country.Name : "-";
                //        Details.City = deliveryAddress.City != null ? deliveryAddress.City.Name : "-";
                //        Details.Area = deliveryAddress.Area != null ? deliveryAddress.Area.Name : "-";
                //        Details.Address = deliveryAddress.Address;
                //    }

                //    Details.orderdetails = OrderDetails;

                //    var body = ViewToStringRenderer.RenderViewToString(this.ControllerContext, "~/Views/Orders/Details.cshtml", Details);

                //    _orderService.SendOrderEmail(order.Customer.Email, "NowBuySell | Order Placed", body);
                //}

                //Notification not = new Notification();

                //if (order.Status == "Pending")
                //{
                //    not.Title = "Order Placed";
                //    not.TitleAr = "Order Placed";
                //    not.Description = string.Format("Your order # {0} has been placed. You can check the order status via order tracker", order.OrderNo);
                //    not.DescriptionAr = string.Format("Your order # {0} has been placed. You can check the order status via order tracker", order.OrderNo);

                //}
                //else if (order.Status == "Confirmed")
                //{

                //    not.Title = "Order Confirmed";
                //    not.TitleAr = "Order Confirmed";
                //    not.Description = string.Format("Your order # {0} has been confirmed. You can check the order status via order tracker", order.OrderNo);
                //    not.DescriptionAr = string.Format("Your order # {0} has been confirmed. You can check the order status via order tracker", order.OrderNo);

                //}
                //else if (order.Status == "Processing")
                //{
                //    not.Title = "Order Processed";
                //    not.TitleAr = "Order Processed";
                //    not.Description = string.Format("Your order # {0} has been processed. You can check the order status via order tracker", order.OrderNo);
                //    not.DescriptionAr = string.Format("Your order # {0} has been processed. You can check the order status via order tracker", order.OrderNo);
                //}
                //else if (order.Status == "Completed")
                //{
                //    not.Title = "Order Ready For Delivery";
                //    not.TitleAr = "Order Ready For Delivery";
                //    not.Description = string.Format("Your order # {0} is ready for delivery. You can check the order status via order tracker", order.OrderNo);
                //    not.DescriptionAr = string.Format("Your order # {0} is ready for delivery. You can check the order status via order tracker", order.OrderNo);

                //}

                //else if (order.Status == "Dispatched")
                //{
                //    not.Title = "Order Out For Delivery";
                //    not.TitleAr = "Order Out For Delivery";
                //    not.Description = string.Format("Your order # {0} has been dispatched. You can check the order status via order tracker", order.OrderNo);
                //    not.DescriptionAr = string.Format("Your order # {0} has been dispatched. You can check the order status via order tracker", order.OrderNo);

                //}
                //else if (order.Status == "Delivered")
                //{
                //    not.Title = "Order Delivered";
                //    not.TitleAr = "Order Delivered";
                //    not.Description = string.Format("Your order # {0} has been delivered. You can check the order status via order tracker", order.OrderNo);
                //    not.DescriptionAr = string.Format("Your order # {0} has been delivered. You can check the order status via order tracker", order.OrderNo);

                //}
                //else if (order.Status == "Canceled")
                //{
                //    not.Title = "Order Canceled";
                //    not.TitleAr = "Order Canceled";
                //    not.Description = string.Format("Your order # {0} has been canceled. You can check the order status via order tracker", order.OrderNo);
                //    not.DescriptionAr = string.Format("Your order # {0} has been canceled. You can check the order status via order tracker", order.OrderNo);

                //}
                //else if (order.Status == "Returned")
                //{
                //    not.Title = "Order Returned";
                //    not.TitleAr = "Order Returned";
                //    not.Description = string.Format("Your order # {0} has been returned. You can check the order status via order tracker", order.OrderNo);
                //    not.DescriptionAr = string.Format("Your order # {0} has been returned. You can check the order status via order tracker", order.OrderNo);
                //}

                //not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                //not.OriginatorName = Session["UserName"].ToString();
                //not.Module = "Booking";
                //not.OriginatorType = "Admin";
                //not.RecordID = order.ID;
                //if (_notificationService.CreateNotification(not, ref message))
                //{
                //    NotificationReceiver notRec = new NotificationReceiver();
                //    notRec.ReceiverID = order.CustomerID;
                //    notRec.ReceiverType = "Customer";
                //    notRec.NotificationID = not.ID;
                //    if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                //    {
                //        var tokens = _customerSessionService.GetCustomerSessionFirebaseTokens((long)order.CustomerID);
                //        var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
                //        {
                //            Module = "Booking",
                //            RecordID = order.ID,
                //            NotificationID = notRec.ID
                //        });
                //    }
                //}
                return Json(new
                {
                    success = true,
                    url = "/Admin/Booking/Index",
                    message = "Status updated successfully ...",
                    data = new
                    {
                        ID = order.ID,
                        Date = order.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        Car= order.Car.Name,
                        Customer = new { Name = order.Customer != null ? order.Customer.Name : null, Contact = order.Customer != null ? order.Customer.Contact : null },
                        Vendor = order.Vendor.Name,
                        Status = order.Status,
                    }
                });
            }
            return View("Index");
        }
        public ActionResult Details(long id)
        {
            Booking order = _bookingService.GetBooking((long)id);

            return View(order);
        }

    }
}