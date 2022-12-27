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
    public class PropertyRequestsController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IPropertyRequestsService _service;
        private readonly IVendorRequestsService _vendorRequestService;

        public PropertyRequestsController(IPropertyRequestsService service, IVendorRequestsService vendorRequestService)
        {
            _service = service;
            _vendorRequestService = vendorRequestService;
        }
        // GET: VendorPortal/PropertyRequests
        public ActionResult Index()
        {
            List<PropertyRequest> model = new List<PropertyRequest>();
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            model = _service.GetRequestListByVendor(vendorId).ToList();
            return View(model);
        }
        public ActionResult Details(long RequestId)
        {
            List<VendorRequest> model = new List<VendorRequest>();
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            var propertyrequest = _service.GetRequestByID((int)RequestId);            
            model = _vendorRequestService.GetPropertyRequestByVendor(vendorId).ToList();

            if(model.Where(x=>x.PropertyID == propertyrequest.ID).Count() > 0)
            {
                ViewBag.IsFlagAllow = false;
            }
            else
            {
                ViewBag.IsFlagAllow = true;
            }

            return PartialView(propertyrequest);
        }
        [HttpPost]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                long Id;
                PropertyRequest proprequest = _service.GetRequestByID((int)id);
                proprequest.IsFulFilled = true;
                if (_service.UpdatePropertyRequests(ref proprequest, ref message))
                {

                    return Json(new
                    {

                        success = true,
                        url = "/Vendor/CarRequests/Index",
                        message = message,
                        data = new
                        {
                            Date = proprequest.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            ID = proprequest.ID,
                            Title = proprequest.Title,
                            CategoryName = proprequest.Category.CategoryName,
                            State = proprequest.State,
                            Area = proprequest.Area,
                            Description = proprequest.Description,
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
        public ActionResult Flag(int id)
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            string message = string.Empty;

            VendorRequest model = new VendorRequest();

            model.PropertyID = id;
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
        public ActionResult UnFlag(int Requestid)
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            string message = string.Empty;

            var request = _vendorRequestService.GetPropertyRequestByVendor(vendorId).Where(x => x.ID == Requestid).FirstOrDefault();

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
            model = _vendorRequestService.GetPropertyRequestByVendor(vendorId).ToList();
            return View(model);
        }
        public ActionResult MyRequestDetails(long RequestId)
        {
            var propertyrequest = _service.GetRequestByID((int)RequestId);
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            ViewBag.RequestID = propertyrequest.VendorRequests.Where(x => x.VendorID == vendorId).FirstOrDefault().ID;
            return PartialView(propertyrequest);
        }
    }
}