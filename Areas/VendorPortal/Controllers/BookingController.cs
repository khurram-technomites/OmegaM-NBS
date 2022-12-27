using NowBuySell.Data;
using NowBuySell.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    public class BookingController : Controller
    {
        // GET: VendorPortal/Booking
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IBookingService _bookingService;


        public BookingController(IBookingService bookingService)
        {
            this._bookingService = bookingService;


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
            var bookings = _bookingService.GetBookingsByVendor(VendorID);
            return PartialView(bookings);
        }

        public ActionResult StatusChange(long id)
        {
            Booking order = _bookingService.GetBooking((long)id);
            return View(order);
        }

        public ActionResult Details(long id)
        {
            Booking order = _bookingService.GetBooking((long)id);
            return View();
        }

        [HttpPost]
        public ActionResult StatusChange(Booking orderID, string status)
        {
            string message = string.Empty;
            Booking order = _bookingService.GetBooking((long)orderID.ID);
            order.Status = status;
            if (_bookingService.UpdateBooking(ref order, ref message))
            {

                return Json(new
                {
                    success = true,
                    url = "/Admin/Booking/Index",
                    message = "Status updated successfully ...",
                    data = new
                    {
                        ID = order.ID,
                        Date = order.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        Car = order.Car.Name,
                        Customer = new { Name = order.Customer != null ? order.Customer.Name : null, Contact = order.Customer != null ? order.Customer.Contact : null },
                        Vendor = order.Vendor.Name,
                        //TotalAmount = order.TotalAmount,
                        // Currency = order.Currency,
                        //ShipmentStatus = order.ShipmentStatus,
                        Status = order.Status,


                    }
                });
            }
            return View("Index");
        }
    }
}