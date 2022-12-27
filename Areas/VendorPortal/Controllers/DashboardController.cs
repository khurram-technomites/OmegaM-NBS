using NowBuySell.Service;
using NowBuySell.Web.Areas.VendorPortal.ViewModels;
using NowBuySell.Web.AuthorizationProvider;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NowBuySell.Data;
using NowBuySell.Web.Helpers.PushNotification;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    [AuthorizeVendor]
    public class DashboardController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ISPService _spService;
        private readonly INotificationService _notificationService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly ICustomerSessionService _customerSessionService;
        private readonly IPropertyService _propService;
        private readonly IOrderService _orderService;
        private readonly IVendorService _vendorService;

        public DashboardController(IOrderService orderService
            , ISPService spService
            , INotificationService notificationService
            , INotificationReceiverService notificationReceiverService
            , ICustomerSessionService customerSessionService
            , IPropertyService propService
            , IVendorService vendorService)
        {
            this._orderService = orderService;
            this._spService = spService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
            this._customerSessionService = customerSessionService;
            this._propService = propService;
            this._vendorService = vendorService;
        }

        public ActionResult Index()
        {
            ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
            ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-30).ToString("MM/dd/yyyy");
            var VendorID = Convert.ToInt64(Session["VendorID"]);
            var vendor = _vendorService.GetVendor(VendorID);
            ViewBag.ExceedMessage = vendor.IsAdExceeds;
            ViewBag.hasMotorModule = vendor.VendorPackage != null ? vendor.VendorPackage.hasMotorModule : false;
            ViewBag.hasPropertyModule = vendor.VendorPackage != null ? vendor.VendorPackage.hasPropertyModule : false;

            DashboardStatsViewModel ObjDashboardStatsViewModel = new DashboardStatsViewModel()
            {
                Stats = _spService.GetVendorDashboardStats(VendorID),
                StatusByRange = _spService.GetVendorDashboardStatusByRange(Helpers.TimeZone.GetLocalDateTime().AddDays(-30), Helpers.TimeZone.GetLocalDateTime().AddMinutes(1439), VendorID),
                Orders = _orderService.GetVendorOrders("Pending", VendorID),
                TopCustomers = _spService.GetTopCustomers(Helpers.TimeZone.GetLocalDateTime().AddDays(-30), Helpers.TimeZone.GetLocalDateTime(), null),
                TopCategories = _spService.GetTopCategories(Helpers.TimeZone.GetLocalDateTime().AddDays(-30), Helpers.TimeZone.GetLocalDateTime(), null),
                GetNetSalesChartValues = _spService.GetLastWeekEarning(Helpers.TimeZone.GetLocalDateTime().AddDays(-7), Helpers.TimeZone.GetLocalDateTime(), VendorID),
                TotalProperties = _propService.GetTotalPropertyCount((long)VendorID),
                PropertyApprovals = _propService.GetTotalPropertyApprovalCount((long)VendorID),
            };
            return View(ObjDashboardStatsViewModel);
        }

        public ActionResult OnGoingOrders()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult OnGoingOrders(DateTime FromDate, DateTime ToDate)
        {
            long? vendorId = Convert.ToInt64(Session["VendorID"]); ;
            DateTime EndDate = ToDate.AddMinutes(1439);
            var orders = _orderService.GetFilteredOrders(FromDate, EndDate, vendorId, string.Empty, null);
            return PartialView(orders);
        }

        public ActionResult Filter(DateTime sd, DateTime ed)
        {
            var VendorID = Convert.ToInt64(Session["VendorID"]);
            DashboardStatsViewModel ObjDashboardStatsViewModel = new DashboardStatsViewModel()
            {
                Stats = _spService.GetVendorDashboardStats(VendorID),
                StatusByRange = _spService.GetVendorDashboardStatusByRange(sd, ed.AddMinutes(1439), VendorID),
                Orders = null,
                TopCustomers = _spService.GetTopCustomers(sd, ed.AddMinutes(1439), VendorID),
                TopCategories = _spService.GetTopCategories(sd, ed.AddMinutes(1439), VendorID)
            };
            return Json(new { success = true, data = ObjDashboardStatsViewModel });
        }

        [HttpPost]
        public ActionResult StatusChange(long orderID, string status)
        {
            string message = string.Empty;
            Order order = _orderService.GetOrder((long)orderID);
            order.Status = status;
            if (_orderService.UpdateOrder(ref order, ref message))
            {

                Notification not = new Notification();

                if (order.Status == "InProcess")
                {

                    not.Title = "Order Processed";
                    not.TitleAr = "Order Processed";
                    not.Description = string.Format("Your order # {0} has been processed. You can check the order status via order tracker", order.OrderNo);
                    not.DescriptionAr = string.Format("Your order # {0} has been processed. You can check the order status via order tracker", order.OrderNo);
                }
                else if (order.Status == "Canceled")
                {
                    not.Title = "Order Canceled";
                    not.TitleAr = "Order Canceled";
                    not.Description = string.Format("Your order # {0} has been canceled. You can check the order status via order tracker", order.OrderNo);
                    not.DescriptionAr = string.Format("Your order # {0} has been canceled. You can check the order status via order tracker", order.OrderNo);

                }


                not.OriginatorID = Convert.ToInt64(Session["VendorUserID"]);
                not.OriginatorName = Session["VendorUserName"].ToString();
                not.Module = "Booking";
                not.OriginatorType = "Admin";
                not.RecordID = order.ID;
                if (_notificationService.CreateNotification(not, ref message))
                {
                    NotificationReceiver notRec = new NotificationReceiver();
                    notRec.ReceiverID = order.CustomerID;
                    notRec.ReceiverType = "Customer";
                    notRec.NotificationID = not.ID;
                    if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                    {
                        var tokens = _customerSessionService.GetCustomerSessionFirebaseTokens((long)order.CustomerID, true, null);
                        var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
                        {
                            Module = "Booking",
                            RecordID = order.ID,
                            OrderNo = order.OrderNo,
                            NotificationID = notRec.ID
                        });
                    }
                }
                string car = "";
                string vendor = "";
                if (order.OrderDetails.Count > 0)
                {
                    car = order.OrderDetails.FirstOrDefault().Car != null ? order.OrderDetails.FirstOrDefault().Car.Name : null;
                    vendor = order.OrderDetails.FirstOrDefault().Vendor != null ? order.OrderDetails.FirstOrDefault().Vendor.Name : null;
                }

                return Json(new
                {
                    success = true,
                    url = "/Vendor/Dashboard/Index",
                    message = "Status updated successfully ...",
                    data = new
                    {
                        ID = order.ID,
                        Date = order.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        BookingNo = order.OrderNo,
                        Status = order.Status,
                        IsPaid = order.IsPaid,
                        Customer = new
                        {
                            Logo = order.Customer != null ? order.Customer.Logo : null,
                            Name = order.Customer != null ? order.Customer.Name : null,
                            Address = order.OrderDeliveryAddresses.Count() > 0 ? order.OrderDeliveryAddresses.FirstOrDefault().Address : null,
                        },
                        Car = car,
                        Vendor = vendor,
                    }
                });
            }
            else
            {
                message = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = message });
        }

        #region Reports

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TopCategoriesReport(DateTime FromDate, DateTime ToDate)
        {
            long? vendorId = Convert.ToInt64(Session["VendorID"]);
            DateTime EndDate = ToDate.AddMinutes(1439);
            var getAllCategories = _spService.GetTopCategories(FromDate, EndDate, vendorId);
            if (getAllCategories.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("TopCategories");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Category"
                        ,"Items Sold"
                        ,"Net Sales"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["TopCategories"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllCategories)
                    {
                        cellData.Add(new object[] {
                        !string.IsNullOrEmpty(i.Category) ? i.Category : "-"
                        ,i.ItemsSold ?? 0
                        ,!string.IsNullOrEmpty(i.Currency) ? i.Currency + " " + (i.NetSales ?? 0) : "-"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Top Categories Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TopCarsReport(DateTime FromDate, DateTime ToDate)
        {
            long? vendorId = Convert.ToInt64(Session["VendorID"]);
            DateTime EndDate = ToDate.AddMinutes(1439);
            var getAllCars = _spService.GetTopCars(FromDate, EndDate, vendorId);
            if (getAllCars.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("TopCars");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Car"
                        ,"Items Sold"
                        ,"Net Sales"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["TopCars"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllCars)
                    {
                        cellData.Add(new object[] {
                        !string.IsNullOrEmpty(i.Name) ? i.Name : "-"
                        ,i.ItemsSold ?? 0
                        ,!string.IsNullOrEmpty(i.Currency) ? i.Currency + " " + (i.NetSales ?? 0) : "-"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Top Cars Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

        #endregion
    }
}