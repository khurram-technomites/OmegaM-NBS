using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CarFeatureController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ICarFeatureService _carFeatureService;
        private readonly IFeatureService _featureService;

        public CarFeatureController(ICarFeatureService carFeatureService, IFeatureService featureService)
        {
            _carFeatureService = carFeatureService;
            _featureService = featureService;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetCarFeatures(long id)
        {
            var category = _featureService.GetAllCarFeature().Select(i => new { id = i.ID, name = i.Name });
            var carCategory = _carFeatureService.GetCarFeature(id).Select(i => new { id = i.ID, FeatureID = i.FeatureID }).ToList();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                categories = category,
                carCategories = carCategory
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CarFeature carFeature)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_carFeatureService.CreateCarFeature(ref carFeature, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        data = carFeature.ID,
                        message = "Motor feature assigned.",
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
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_carFeatureService.DeleteCarFeature((Int16)id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}