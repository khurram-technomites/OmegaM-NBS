using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    [AuthorizeVendor]
    public class MeetingController : Controller
    {
        private readonly IScheduleMeetingService _meetingService;

        public MeetingController(IScheduleMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        // GET: VendorPortal/Meeting
        //Property
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult List()
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);

            var Meeting = _meetingService.GetListByVendorAndProperty(vendorId).OrderByDescending(x => x.ID);
            return PartialView(Meeting);
        }
        //Motors
        public ActionResult MotorIndex()
        {
            return View();
        }
        public ActionResult MotorList()
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            var Meeting = _meetingService.GetListByVendorAndMotors(vendorId).OrderByDescending(x => x.ID);
            return PartialView(Meeting);
        }
        public ActionResult Message(int ID)
        {
            var Meeting = _meetingService.GetByID(ID);

            ViewBag.Date = GetDate(Meeting.MeetingDate);
            return PartialView(Meeting);
        }

        public ActionResult StatusChange(int ID)
        {
            ScheduleMeeting scheduleMeeting = _meetingService.GetByID(ID);
            TempData["MeetingID"] = ID;
            return PartialView(scheduleMeeting);
        }

        [HttpPost]
        public ActionResult StatusChange(ScheduleMeeting scheduleMeeting, string status)
        {
            string message = string.Empty;
            ScheduleMeeting current = _meetingService.GetByID(scheduleMeeting.ID);
            current.Status = status;
            if (_meetingService.UpdateMeeting(ref current, ref message))
            {

                var VendorID = Convert.ToInt64(Session["VendorID"]);
                return Json(new
                {
                    success = true,
                    url = "/Vendor/Meeting/Index",
                    message = "Status updated successfully ...",
                    data = new
                    {
                        Date = current.CreatedOn.ToString("dd MMM yyyy, h:mm tt"),
                        Customer = current.Customer.Logo + "|" + current.Customer.Name + "|" + current.Customer.Email+ "|" + current.Customer.Contact,
                        For = current.CarID != null ? current.CarID + "^" + current.Car.Name + "^" + current.Car.Thumbnail + "^" + current.Car.Address + "^" + "Car" : current.PropertyID + "^" + current.Property.Title + "^" + current.Property.Thumbnail + "^" + current.Property.Address + "^" + "Property",
                        MeetingDate = current.MeetingDate.Value.ToString("dd MMM yyyy, h:mm tt"),
                        Status = current.Status,
                        ID = current.ID
                    }
                });
            }
            else

                return Json(new
                {
                    success = false,
                    message = "Ooops! something went wrong..."
                });
        }

        private string GetDate(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "-";

            string Month = dateTime.Value.ToString("MMM");

            return dateTime.Value.Day + " " + Month + " " + dateTime.Value.Year;
        }
    }
}