using NowBuySell.Service;
using NowBuySell.Web.Areas.Admin.ViewModels;
using NowBuySell.Web.AuthorizationProvider;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NowBuySell.Data;
using NowBuySell.Web.Helpers.PushNotification;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class DashboardController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IOrderService _orderService;
        private readonly IOrderDetailsService _orderDetailsService;
        private readonly ISPService _spService;
        private readonly INotificationService _notificationService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly ICustomerSessionService _customerSessionService;

        public DashboardController(IOrderService orderService, IOrderDetailsService orderDetailsService, ISPService spService, INotificationService notificationService, INotificationReceiverService notificationReceiverService, ICustomerSessionService customerSessionService)
        {
            this._orderService = orderService;
            this._orderDetailsService = orderDetailsService;
            this._spService = spService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
            this._customerSessionService = customerSessionService;
        }

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            ViewBag.Role = (long)Session["AdminUserID"];
            ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
            ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-30).ToString("MM/dd/yyyy");
            DashboardStatsViewModel ObjDashboardStatsViewModel = new DashboardStatsViewModel()
            {
                Stats = _spService.GetAdminDashboardStats(),
                StatusByRange = _spService.GetAdminDashboardStatusByRange(Helpers.TimeZone.GetLocalDateTime().AddDays(-30), Helpers.TimeZone.GetLocalDateTime()),
                //Orders = _spService.GetOrdersDateWise(Helpers.TimeZone.GetLocalDateTime().AddDays(-100), Helpers.TimeZone.GetLocalDateTime()),
                GetAdminDashboardCharts = _spService.GetAdminDashboardCharts(Helpers.TimeZone.GetLocalDateTime().AddDays(-30), Helpers.TimeZone.GetLocalDateTime()),
                GetAdminDashboardChartsForItemsSold = _spService.GetAdminDashboardChartsForItemsSold(Helpers.TimeZone.GetLocalDateTime().AddDays(-30), Helpers.TimeZone.GetLocalDateTime()),
                GetAdminDashboardChartForReturn = _spService.GetAdminDashboardChartForReturn(Helpers.TimeZone.GetLocalDateTime().AddDays(-30), Helpers.TimeZone.GetLocalDateTime()),
                TopCustomers = _spService.GetTopCustomers(Helpers.TimeZone.GetLocalDateTime().AddDays(-30), Helpers.TimeZone.GetLocalDateTime(), null),
                TopCategories = _spService.GetTopCategories(Helpers.TimeZone.GetLocalDateTime().AddDays(-30), Helpers.TimeZone.GetLocalDateTime(), null),
                TopVendors = _spService.GetTopVendors(Helpers.TimeZone.GetLocalDateTime().AddDays(-30), Helpers.TimeZone.GetLocalDateTime(), null),
                GetNetSalesChartValues = _spService.GetLastWeekEarning(Helpers.TimeZone.GetLocalDateTime().AddDays(-7), Helpers.TimeZone.GetLocalDateTime(), null)
            };
            return View(ObjDashboardStatsViewModel);
        }

        public ActionResult List()
        {
            long? vendorId = null;
            DateTime ToDate = Helpers.TimeZone.GetLocalDateTime();
            DateTime FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-7);

            DashboardListViewModel dashboardListViewModel = new DashboardListViewModel()
            {
                TopCustomers = _spService.GetTopCustomers(FromDate, ToDate, vendorId),
                TopCategories = _spService.GetTopCategories(FromDate, ToDate, vendorId),
                TopCars = _spService.GetTopCars(FromDate, ToDate, vendorId),
                TopCoupons = _spService.GetTopCoupons(FromDate, ToDate),
            };
            return PartialView(dashboardListViewModel);
        }

        [HttpPost]
        public ActionResult List(DateTime FromDate, DateTime ToDate)
        {
            long? vendorId = null;
            DateTime EndDate = ToDate.AddMinutes(1439);
            DashboardListViewModel dashboardListViewModel = new DashboardListViewModel()
            {
                TopCustomers = _spService.GetTopCustomers(FromDate, EndDate, vendorId),
                TopCategories = _spService.GetTopCategories(FromDate, EndDate, vendorId),
                TopCars = _spService.GetTopCars(FromDate, EndDate, vendorId),
                TopCoupons = _spService.GetTopCoupons(FromDate, EndDate),
            };
            return PartialView(dashboardListViewModel);
        }

        public ActionResult OrderList()
        {
            long? vendorId = null;
            DateTime ToDate = Helpers.TimeZone.GetLocalDateTime().AddMinutes(1439);
            DateTime FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-7);
            var orders = _orderService.GetFilteredOrders(FromDate, ToDate, vendorId, string.Empty, null);

            return PartialView();
        }


        [HttpPost]
        public ActionResult OrderList(DateTime FromDate, DateTime ToDate)
        {
            long? vendorId = null;
            DateTime EndDate = ToDate.AddMinutes(1439);
            var orders = _orderService.GetFilteredOrders(FromDate, EndDate, vendorId, "OnGoing", null);
            return PartialView(orders);
        }

        public ActionResult Filter(DateTime sd, DateTime ed)
        {
            ed = ed.AddMinutes(1439);
            DashboardStatsViewModel ObjDashboardStatsViewModel = new DashboardStatsViewModel()
            {
                Stats = _spService.GetAdminDashboardStats(),
                StatusByRange = _spService.GetAdminDashboardStatusByRange(sd, ed),
                GetAdminDashboardCharts = _spService.GetAdminDashboardCharts(sd, ed),
                GetAdminDashboardChartsForItemsSold = _spService.GetAdminDashboardChartsForItemsSold(sd, ed),
                GetAdminDashboardChartForReturn = _spService.GetAdminDashboardChartForReturn(sd, ed),
                TopCustomers = _spService.GetTopCustomers(sd, ed, null),
                TopCategories = _spService.GetTopCategories(sd, ed, null),
                TopVendors = _spService.GetTopVendors(sd, ed, null),
                GetNetSalesChartValues = _spService.GetLastWeekEarning(sd, ed, null)
            };
            return Json(new { success = true, data = ObjDashboardStatsViewModel });
        }

        [HttpGet]
        public ActionResult GetNetSalesDetails(long id)
        {
            var data = _spService.GetAdminDashboardWeeklySalesChart(Helpers.TimeZone.GetLocalDateTime().AddDays(-7), Helpers.TimeZone.GetLocalDateTime());


            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                data = data,
            }, JsonRequestBehavior.AllowGet);
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


                not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                not.OriginatorName = Session["UserName"].ToString();
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
                    url = "/Admin/Dashboard/Index",
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
            long? vendorId = null;
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
            long? vendorId = null;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TopCustomersReport(DateTime FromDate, DateTime ToDate)
        {
            long? vendorId = null;
            DateTime EndDate = ToDate.AddMinutes(1439);
            var getAllCustomers = _spService.GetTopCustomers(FromDate, EndDate, vendorId);
            if (getAllCustomers.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("TopCustomers");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Customer"
                        ,"Total Orders"
                        ,"Total Spend"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["TopCustomers"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllCustomers)
                    {
                        cellData.Add(new object[] {
                        !string.IsNullOrEmpty(i.Name) ? i.Name : "-"
                        ,i.Orders ?? 0
                        ,!string.IsNullOrEmpty(i.Currency) ? i.Currency + " " + (i.TotalSpend ?? 0) : "-"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Top Customers Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TopCouponsReport(DateTime FromDate, DateTime ToDate)
        {
            DateTime EndDate = ToDate.AddMinutes(1439);
            var getAllCoupons = _spService.GetTopCoupons(FromDate, EndDate);
            if (getAllCoupons.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("TopCoupons");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Coupon Code"
                        ,"Name"
                        ,"Type"
                        ,"Total Orders"
                        ,"Total Discount"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["TopCoupons"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllCoupons)
                    {
                        cellData.Add(new object[] {
                        !string.IsNullOrEmpty(i.CouponCode) ? i.CouponCode : "-"
                        ,!string.IsNullOrEmpty(i.CouponName) ? i.CouponName : "-"
                        ,!string.IsNullOrEmpty(i.Type) ? i.Type : "-"
                        ,i.TotalOrders ?? 0
                        ,i.TotalCouponDiscount ?? 0
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Top Coupons Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

        #endregion


    }
}