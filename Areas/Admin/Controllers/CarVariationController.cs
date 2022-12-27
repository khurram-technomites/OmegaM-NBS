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
    public class CarVariationController : Controller
    {
        // GET: Admin/CarVariation
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ICarVariationService _carVariationService;

        public CarVariationController(ICarVariationService carVariationService)
        {
            this._carVariationService = carVariationService;
        }

        [HttpGet]
        public ActionResult GetCarVariation(long id)
        {

            var carVariation = _carVariationService.GetCarVariations(id).Select(i => new
            {
                i.ID,
                i.CarID,
                i.SKU,
                i.RegularPrice,
                i.SalePrice,
                i.SalePriceFrom,
                i.SalePriceTo,
                i.Stock,
                i.Threshold,
                i.StockStatus,
                i.Thumbnail,
                i.Weight,
                i.Length,
                i.Width,
                i.Height,
                i.Description,
                i.DescriptionAr,
                i.IsManageStock,
                i.SoldIndividually,
                i.IsActive
            }).ToList();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                carVariation = carVariation
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CarVariation carVariation)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_carVariationService.CreateCarVariation(ref carVariation, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        data = carVariation.ID,
                        message = "Motor attribute assigned.",
                    });
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }
            return Json(new { success = false, message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(CarVariation carVariation)
        {
            string message = string.Empty;
            if (_carVariationService.UpdateCarVariation(ref carVariation, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_carVariationService.DeleteCarVariation(id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("DeleteValue")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteValue(CarVariation carVariation)
        {
            string message = string.Empty;
            if (_carVariationService.DeleteCarVariation(carVariation, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}