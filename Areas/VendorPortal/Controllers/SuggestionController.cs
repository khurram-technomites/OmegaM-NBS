using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    [AuthorizeVendor]
    public class SuggestionController : Controller
    {
        private readonly IVendorSuggestionService _suggestionService;
        private readonly INotificationService _notificationService;
        private readonly INotificationReceiverService _notificationReceiverService;
        // GET: VendorPortal/Suggestion
        public SuggestionController(IVendorSuggestionService suggestionService , INotificationService notificationService, INotificationReceiverService notificationReceiverService)
        {
            this._suggestionService = suggestionService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
        }
        public ActionResult Index()
        {
            var suggestion = _suggestionService.GetAll();
            return View(suggestion);
        }
         
        public ActionResult SuggestionList()
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            var suggestion = _suggestionService.GetListByVendor(vendorId);
            return PartialView(suggestion);
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VendorSuggestion suggestion = _suggestionService.GetSuggestion((long)id);
            if (suggestion == null)
            {
                return HttpNotFound();
            }

            TempData["vendorID"] = id;
            return View(suggestion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VendorSuggestion tag)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                    if (_suggestionService.Updatesuggestion(ref tag, ref message))
                    {
                        return Json(new
                        {
                            success = true,
                            url = "/Vendor/Suggestion/Index",
                            message = "Suggestion updated successfully ...",
                            data = new
                            {
                                ID = tag.ID,
                                Date = tag.CreatedOn.ToString("dd MMM yyyy, h:mm tt"),
                                Suggestion = tag.Suggestion

                            }
                        });
                    }
                
            }
            else
            {
                message = "Please fill the form properly ...";
            }
            return Json(new { success = false, message = message });
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(long id)
        {
            string message = string.Empty;
            if (_suggestionService.DeleteSuggestion((int)id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Suggestion()
        {
            return PartialView();
        }
        public ActionResult SuggestionById (int id)
        {
            var suggestion = _suggestionService.GetById(id);
            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                getintouch = suggestion
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VendorSuggestion Entity)
        {
            string message = string.Empty;
            int vendorId = Convert.ToInt32(Session["VendorID"]);

            Entity.VendorID = vendorId;
            Entity.IsDeleted = false;
            Entity.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();

            if (_suggestionService.AddSuggestion(ref Entity, ref message))
            {
                Notification not = new Notification();
                not.Title = "Sent Suggestion";
                not.Description = "New Suggestion For Admin ";
                not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                not.OriginatorName = Session["VendorUserName"].ToString();
                not.Url = "/Admin/VendorSuggestion/Index";
                not.Module = "Suggestion";
                not.OriginatorType = "Vendor";
                not.RecordID = Entity.ID;
                if (_notificationService.CreateNotification(not, ref message))
                {
                    if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", not.OriginatorID))
                    {
                    }
                }
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        suggestion = Entity.Suggestion,
                        createdOn = Entity.CreatedOn
                        
                    },
                    message = "Suggestion sent successfully",
                });


            }
            return Json(new
            {
                success = false,
                
            });

        }
 
    }
}