using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    [AuthorizeVendor]
    public class CustomerController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IOrderDetailsService _orderdetailService;
        private readonly INotificationService _notificationService;
        private readonly INotificationReceiverService _notificationReceiverService;

        public CustomerController(ICustomerService customerService, IOrderService orderService, IOrderDetailsService orderdetailService, INotificationService notificationService, INotificationReceiverService notificationReceiverService)
        {
            this._customerService = customerService;
            this._orderService = orderService;
            this._orderdetailService = orderdetailService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
        }

        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        public ActionResult List()
        {
            var VendorID = Convert.ToInt64(Session["VendorID"]);
            var customers = _customerService.GetVendorCustomers(VendorID);
            return PartialView(customers);
        }

        [HttpGet]
        public ActionResult CustomerBooking(long id)
        {
            ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
            ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-7).ToString("MM/dd/yyyy");
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            Session["CustomerID"] = id;
            return View();
        }

        public ActionResult CustomerBookingList()
        {
            ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
            ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-7).ToString("MM/dd/yyyy");
            long customerID = (long)Session["CustomerID"];
            var customers = _customerService.GetCustomerOrders(customerID,null,"en");
            return PartialView(customers);
        }

        [HttpPost]
        public ActionResult CustomerBookingList(DateTime sd, DateTime ed)
        {
            DateTime EndDate = ed.AddMinutes(1439);
            long customerID = (long)Session["CustomerID"];
            var orders = _customerService.GetCustomerOrdersDateWise(customerID,"en", sd, EndDate);
            return PartialView(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomersOrdersReport()
        {
            var customerId = (long)Session["CustomerID"];
            var getAllCustomers = _customerService.GetCustomerOrders(customerId,null,"en").ToList();
            if (getAllCustomers.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("CustomersReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Booking No"
                        ,"Car Name"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["CustomersReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllCustomers)
                    {
                        var customerDetails = new Customer();
                        customerDetails = _customerService.GetCustomer(i.ID);
                        if (customerDetails == null)
                            customerDetails = new Customer();

                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.OrderNo) ? i.OrderNo : "-"
                        ,!string.IsNullOrEmpty(i.CarName) ? i.CarName: "-"
                        ,!string.IsNullOrEmpty(i.Status) ? i.Status : "-"
                       
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Customers Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = _customerService.GetCustomer((Int16)id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = _customerService.GetCustomer((long)id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            if (!(bool)customer.IsActive)
                customer.IsActive = true;
            else
            {
                customer.IsActive = false;
            }
            string message = string.Empty;
            string status = string.Empty;
            if (_customerService.UpdateCustomer(ref customer, ref message, ref status))
            {
                SuccessMessage = "Customer " + ((bool)customer.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        ID = customer.ID,
                        Date = customer.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        Name = customer.Name,
                        Contact = customer.Contact,
                        Email = customer.Email,
                        Address = customer.Address,
                        IsActive = customer.IsActive.HasValue ? customer.IsActive.Value.ToString() : bool.FalseString
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomersReport()
        {
            var VendorID = Convert.ToInt64(Session["VendorID"]);
            var getAllCustomers = _customerService.GetVendorCustomers(VendorID).ToList();
            if (getAllCustomers.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("CustomersReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Name"
                        ,"Contact"
                        ,"Email"
                        ,"Country"
                        ,"City"
                        ,"Area"
                        ,"Status"
                        ,"Address"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["CustomersReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllCustomers)
                    {
                        var customerDetails = new Customer();
                        customerDetails = _customerService.GetCustomer(i.ID);
                        if (customerDetails == null)
                            customerDetails = new Customer();

                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
                        ,!string.IsNullOrEmpty(i.Contact) ? i.Contact: "-"
                        ,!string.IsNullOrEmpty(i.Email) ? i.Email : "-"
                        ,customerDetails.Country != null ? (!string.IsNullOrEmpty(customerDetails.Country.Name) ? customerDetails.Country.Name : "-") : "-"
                        ,customerDetails.City != null ? (!string.IsNullOrEmpty(customerDetails.City.Name) ? customerDetails.City.Name : "-") : "-"
                        ,customerDetails.Area != null ? (!string.IsNullOrEmpty(customerDetails.Area.Name) ? customerDetails.Area.Name : "-") : "-"
                        ,i.IsActive == true ? "Active" : "InActive"
                        ,!string.IsNullOrEmpty(i.Address) ? i.Address : "-"
                        
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Customers Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }


        public ActionResult StatusChange(long ID)

        {
            Order order = _orderService.GetOrder((long)ID);
            return View(order);
        }

        [HttpPost]
        public ActionResult StatusChange(Order orderID, string status)
        {
            string message = string.Empty;
            Order order = _orderService.GetOrder((long)orderID.ID);
            order.Status = status;
            if (_orderService.UpdateOrder(ref order, ref message))
            {
                Notification not = new Notification();
                if (order.Status == "Pending")
                {
                    not.Title = "Booking Placed";
                    not.TitleAr = "Booking Placed";
                    not.Description = string.Format("Your booking # {0} has been placed. You can check the booking status via booking details", order.OrderNo);
                    not.DescriptionAr = string.Format("Your booking # {0} has been placed. You can check the booking status via booking details", order.OrderNo);

                }
                else if (order.Status == "Confirmed")
                {

                    not.Title = "Booking Confirmed";
                    not.TitleAr = "Booking Confirmed";
                    not.Description = string.Format("Your booking # {0} has been confirmed. You can check the booking status via booking details", order.OrderNo);
                    not.DescriptionAr = string.Format("Your booking # {0} has been confirmed. You can check the booking status via booking details", order.OrderNo);

                }
                else if (order.Status == "Processing")
                {
                    not.Title = "Booking Processed";
                    not.TitleAr = "Booking Processed";
                    not.Description = string.Format("Your booking # {0} has been processed. You can check the booking status via booking details", order.OrderNo);
                    not.DescriptionAr = string.Format("Your booking # {0} has been processed. You can check the booking status via booking details", order.OrderNo);
                }
                else if (order.Status == "Completed")
                {
                    not.Title = "Booking Ready For Delivery";
                    not.TitleAr = "Booking Ready For Delivery";
                    not.Description = string.Format("Your booking # {0} is ready for delivery. You can check the booking status via booking details", order.OrderNo);
                    not.DescriptionAr = string.Format("Your booking # {0} is ready for delivery. You can check the booking status via booking details", order.OrderNo);

                }

                else if (order.Status == "Dispatched")
                {
                    not.Title = "Booking Out For Delivery";
                    not.TitleAr = "Booking Out For Delivery";
                    not.Description = string.Format("Your booking # {0} has been dispatched. You can check the booking status via booking details", order.OrderNo);
                    not.DescriptionAr = string.Format("Your booking # {0} has been dispatched. You can check the booking status via booking details", order.OrderNo);

                }
                else if (order.Status == "Delivered")
                {
                    not.Title = "Booking Delivered";
                    not.TitleAr = "Booking Delivered";
                    not.Description = string.Format("Your booking # {0} has been delivered. You can check the booking status via booking details", order.OrderNo);
                    not.DescriptionAr = string.Format("Your booking # {0} has been delivered. You can check the booking status via booking details", order.OrderNo);

                }
                else if (order.Status == "Canceled")
                {
                    not.Title = "Booking Canceled";
                    not.TitleAr = "Booking Canceled";
                    not.Description = string.Format("Your booking # {0} has been canceled. You can check the booking status via booking details", order.OrderNo);
                    not.DescriptionAr = string.Format("Your booking # {0} has been canceled. You can check the booking status via booking details", order.OrderNo);

                }
                else if (order.Status == "Returned")
                {
                    not.Title = "Booking Returned";
                    not.TitleAr = "Booking Returned";
                    not.Description = string.Format("Your booking # {0} has been returned. You can check the booking status via booking details", order.OrderNo);
                    not.DescriptionAr = string.Format("Your booking # {0} has been returned. You can check the booking status via booking details", order.OrderNo);
                }

                not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                not.OriginatorName = Session["VendorUserName"].ToString();
                not.Module = "Booking";
                not.OriginatorType = "Vendor";
                not.RecordID = order.ID;
                if (_notificationService.CreateNotification(not, ref message))
                {
                    NotificationReceiver notRec = new NotificationReceiver();
                    notRec.ReceiverID = order.CustomerID;
                    notRec.ReceiverType = "Customer";
                    notRec.NotificationID = not.ID;
                    if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                    {

                    }

                }
                var VendorID = Convert.ToInt64(Session["VendorID"]);
                return Json(new
                {
                    success = true,
                    url = "/Vendor/Order/Index",
                    message = "Status updated successfully ...",
                    data = new
                    {
                        ID = order.ID,
                        Date = order.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        OrderNo = order.OrderNo,
                        //Customer = new { Name = order.Customer.Name, Contact = order.Customer.Contact },
                        Car= order.OrderDetails.FirstOrDefault().Car.Name, 
                       
                        Status = order.Status,
                        
                    }
                });
            }
            return View("Index");
        }
    }
}