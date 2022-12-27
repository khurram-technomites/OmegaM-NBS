using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Service.Helpers;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    [AuthorizeVendor]
    public class TestRunCustomerController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ITestRunCustomerService _testRunCustomerService;
        private readonly IMail _email;

        public TestRunCustomerController(ITestRunCustomerService testRunCustomerService, IMail email)
        {
            this._testRunCustomerService = testRunCustomerService;
            this._email = email;
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
            var type = "Property";
            var customers = _testRunCustomerService.GetTestRunCustomers(VendorID, type).OrderByDescending(x => x.ID);
            return PartialView(customers);
        }
        public ActionResult MotorTrialIndex()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        public ActionResult MotorTrialList()
        {
            var VendorID = Convert.ToInt64(Session["VendorID"]);
            var type = "Motor";
            var customers = _testRunCustomerService.GetTestRunCustomers(VendorID, type).OrderByDescending(x => x.ID);
            return PartialView(customers);
        }
        public ActionResult Details(long Id)
        {
            ViewBag.BuildingID = Id;
            var details = _testRunCustomerService.GetTestRunCustomer((int)Id);
            return PartialView(details);
        }
        public ActionResult StatusChange(long id)
        {
            TestRunCustomer trialbooking= _testRunCustomerService.GetTestRunCustomer((long)id);
            return View(trialbooking);
        }

        [HttpPost]
        public ActionResult StatusChange(TestRunCustomer orderID, string status)
        {
            string message = string.Empty;
            string TypeName = string.Empty;
            TestRunCustomer order = _testRunCustomerService.GetTestRunCustomer((long)orderID.ID);
            order.Status = status;
            if (_testRunCustomerService.UpdateTestRunCustomer(ref order, ref message))
            {
                TypeName = order.Property != null ? order.Property.Title : order.Car.Name;
                var path = Server.MapPath("~/");
                if (_email.SendTrialBookingMail(order.TrialBookingNo, order.CustomerName, TypeName, order.CustomerEmail, order.Status, path))
                {
                    
                }

                var VendorID = Convert.ToInt64(Session["VendorID"]);
                return Json(new
                {
                    success = true,
                    url = "/Vendor/TestRunCustomer/Index",
                    message = "Status updated successfully ...",
                    data = new
                    {
                        ID = order.ID,
                        Date = order.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                        TrialBookingNo = order.TrialBookingNo,
                        Customer = new { CustomerName = order.CustomerName, CustomerContact = order.CustomerContact,CustomerEmail=order.CustomerEmail, CustomerLogo = order.Customer.Logo },
                        Property = order.Type=="Property"? order.Property.Title: order.Car.Name,
                        Status = order.Status,
                        BookingDate=order.BookedDate.Value.ToString("dd MMM yyyy"),
                        BookingTime=order.BookedTime.ToString()
                    }
                });;
            }
            return View("Index");
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_testRunCustomerService.DeleteTestRunCustomer((Int16)id, ref message,true))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}