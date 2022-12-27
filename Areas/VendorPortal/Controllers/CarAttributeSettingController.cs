using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System.Linq;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
	[AuthorizeVendor]
	public class CarAttributeSettingController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly ICarAttributeSettingService _carAttributeSettingService;

		public CarAttributeSettingController(ICarAttributeSettingService carAttributeSettingService)
		{
			this._carAttributeSettingService = carAttributeSettingService;
		}

		[HttpGet]
		public ActionResult GetCarAttributeSetting(long id)
		{
			var carAttributeSetting = _carAttributeSettingService.GetCarAttributesSetting(id).Select(i => new
			{
				id = i.ID.ToString(),
				attributeId = i.AttributeID,
				attributeName = i.Attribute.Name,
				i.CarPageVisiblity,
				i.VariationUsage
			}).ToList();

			return Json(new
			{
				success = true,
				message = "Data recieved successfully!",
				carAttributeSetting = carAttributeSetting
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(CarAttributeSetting carAttributeSetting)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (_carAttributeSettingService.CreateCarAttributeSetting(ref carAttributeSetting, ref message))
				{
					return Json(new
					{
						success = true,
						data = carAttributeSetting.ID,
						message = "Car attribute assigned.",
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
		public ActionResult Update(CarAttributeSetting carAttributeSetting)
		{
			string message = string.Empty;
			if (_carAttributeSettingService.UpdateCarAttributeSetting(ref carAttributeSetting, ref message))
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
			if (_carAttributeSettingService.DeleteCarAttributeSetting(id, ref message))
			{
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost, ActionName("DeleteValue")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteValue(CarAttributeSetting carAttributeSetting)
		{
			string message = string.Empty;
			if (_carAttributeSettingService.DeleteCarAttributeSetting(carAttributeSetting, ref message))
			{
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}
	}
}