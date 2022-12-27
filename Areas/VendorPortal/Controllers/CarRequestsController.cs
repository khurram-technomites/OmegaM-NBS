using LinqToExcel;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.POCO;
using NowBuySell.Web.Helpers.Routing;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    [AuthorizeVendor]
    public class CarRequestsController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;
        private readonly ICarRequestsService _service;
        private readonly IVendorRequestsService _vendorRequestService;
        public CarRequestsController(ICarRequestsService service, IVendorRequestsService vendorRequestService)
        {
            _service = service;
            _vendorRequestService = vendorRequestService;
        }
        // GET: VendorPortal/CarRequests
        
        public ActionResult Index()
        {
            List<CarRequest> model = new List<CarRequest>();
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            model = _service.GetRequestListByVendor(vendorId).ToList();
            return View(model);
        }
        public ActionResult Details(long RequestId)
        {
            List<VendorRequest> model = new List<VendorRequest>();
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            var carrequest = _service.GetRequestByID((int)RequestId);
            model = _vendorRequestService.GetCarRequestByVendor(vendorId).ToList();

            if (model.Where(x => x.CarID == carrequest.ID).Count() > 0)
            {
                ViewBag.IsFlagAllow = false;
            }
            else
            {
                ViewBag.IsFlagAllow = true;
            }
            return PartialView(carrequest);
        }
     
        [HttpPost]
        public ActionResult Edit (long id)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                long Id;
                CarRequest carrequest = _service.GetRequestByID((int)id);
                carrequest.IsFulFilled = true;
                if (_service.UpdateCarRequests(ref carrequest, ref message))
                {
                    
                    return Json(new
                    {

                        success = true,
                        url = "/Vendor/CarRequests/Index",
                        message = message,
                        data = new
                        {
                            Date = carrequest.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            ID = carrequest.ID,
                            Title = carrequest.Title,
                            CategoryName = carrequest.Category.CategoryName,
                            Transmission = carrequest.Transmission,
                            Horsepower = carrequest.Horsepower,
                            Description = carrequest.Description,
                        }
                    });

                }
            }
            else
            {
                message = "Your Request Has Not Been Flagged";
            }
            return Json(new { success = false, message = message });
        }
        [HttpPost]
        public ActionResult Flag(int id)
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            string message = string.Empty;

            VendorRequest model = new VendorRequest();

            model.CarID = id;
            model.VendorID = vendorId;
            model.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();

            if (_vendorRequestService.AddVendorRequest(model, ref message))
            {
                return Json(new
                {
                    success = true,
                    message = "Request Flagged Successfully",
                });
            }

            return Json(new
            {
                success = false,
                message = "Ops ! Something Wnet Wrong",
            });
        }
        [HttpPost]
        public ActionResult UnFlag(int Requestid)
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            string message = string.Empty;

            var request = _vendorRequestService.GetCarRequestByVendor(vendorId).Where(x => x.ID == Requestid).FirstOrDefault();

            if (_vendorRequestService.Delete(request))
            {
                return Json(new
                {
                    success = true,
                    message = "Request UnFlagged Successfully",
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Ops ! Something Wnet Wrong",
                });
            }

           
        }

        public ActionResult MyRequest()
        {
            List<VendorRequest> model = new List<VendorRequest>();
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            model = _vendorRequestService.GetCarRequestByVendor(vendorId).ToList();
            return View(model);
        }
        public ActionResult MyRequestDetails(long RequestId)
        {
            var carrequest = _service.GetRequestByID((int)RequestId);
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            ViewBag.RequestID = carrequest.VendorRequests.Where(x => x.VendorID == vendorId).FirstOrDefault().ID;
            return PartialView(carrequest);
        }
    }
}