using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Areas.VendorPortal.ViewModels;
using NowBuySell.Web.AuthorizationProvider;
using System.Linq;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    public class CarVariationAttributeController : Controller
    {
        // GET: Admin/CarVariationAttribute
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ICarVariationAttributeService _carVariationAttributeService;

        public CarVariationAttributeController(ICarVariationAttributeService carVariationAttributeService)
        {
            this._carVariationAttributeService = carVariationAttributeService;
        }


        [HttpGet]
        public ActionResult GetVariationAttributes(long id)
        {
            var carVariationAttributes = _carVariationAttributeService.GetCarVariationAttributes(id).Select(i => new
            {
                id = i.ID.ToString(),
                carVariationID = i.CarVariationID,
                carAttributeID = i.CarAttributeID
            }).ToList();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                variationAttributes = carVariationAttributes
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCarVariationAttributes(long id)
        {
            var carVariationAttributes = _carVariationAttributeService.GetVariationAttributesByCar(id).Select(i => new
            {
                id = i.ID.ToString(),
                carVariationID = i.CarVariationID,
                carAttributeID = i.CarAttributeID
            }).ToList();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                carVariationAttributes = carVariationAttributes
            }, JsonRequestBehavior.AllowGet);
        }


        //[HttpGet]
        //public ActionResult CheckDublicate(CarVariationAttributeViewModel carVariationAttributeViewModel)
        //{
        //	var carVariationAttributes = _carVariationAttributeService.GetVariationAttributesByCar(id).Select(i => new
        //	{
        //		id = i.ID.ToString(),
        //		carVariationID = i.CarVariationID,
        //		carAttributeID = i.CarAttributeID
        //	}).ToList();

        //	return Json(new
        //	{
        //		success = true,
        //		message = "Data recieved successfully!",
        //		carVariationAttributes = carVariationAttributes
        //	}, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CarVariationAttribute carVariationAttribute)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_carVariationAttributeService.CreateCarVariationAttribute(ref carVariationAttribute, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            carVariationAttribute.ID,
                            carVariationAttribute.CarVariationID,
                            carVariationAttribute.CarAttributeID
                        },
                        message = "Car variation attribute assigned.",
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
            if (_carVariationAttributeService.DeleteCarVariationAttribute(id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("DeleteValue")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteValue(CarVariationAttribute carVariationAttribute)
        {
            string message = string.Empty;
            if (_carVariationAttributeService.DeleteCarVariationAttribute(carVariationAttribute, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAll(long id)
        {
            string message = string.Empty;
            if (_carVariationAttributeService.DeleteCarVariationAttributes(id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}