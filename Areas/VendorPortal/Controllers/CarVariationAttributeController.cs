using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Areas.VendorPortal.ViewModels;
using NowBuySell.Web.AuthorizationProvider;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
	[AuthorizeVendor]
	public class CarVariationAttributeController : Controller
	{
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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Update(CarVariationAttributeViewModel carVariationAttributeViewModel)
		{

			string message = string.Empty;
			var carVariationAttributes = _carVariationAttributeService.GetVariationAttributesByCar((long)carVariationAttributeViewModel.CarId);

			foreach (var item in carVariationAttributes.Select(i => i.CarVariationID).Distinct().ToArray())
			{
				IEnumerable<long> var_attr = carVariationAttributes.Where(i => i.CarVariationID == item).Select(i => i.CarAttributeID.Value).Distinct().ToList();

				bool isEqual = var_attr.SequenceEqual(carVariationAttributeViewModel.CarAttributes);
				if (isEqual && item != carVariationAttributeViewModel.CarVariationId)
				{
					return Json(new
					{
						success = false,
						message = "Variation with same attribute combination already exist.",
					});
				}
			}
			_carVariationAttributeService.DeleteCarVariationAttributes(carVariationAttributeViewModel.CarVariationId, ref message);
			var attributes = new List<long>();
			foreach (var carAttribute in carVariationAttributeViewModel.CarAttributes)
			{
				CarVariationAttribute carVariationAttribute = new CarVariationAttribute()
				{
					CarID = carVariationAttributeViewModel.CarId,
					CarVariationID = carVariationAttributeViewModel.CarVariationId,
					CarAttributeID = carAttribute
				};
				if (_carVariationAttributeService.CreateCarVariationAttribute(ref carVariationAttribute, ref message));
				{
					attributes.Add(carAttribute);
				}
			}
			return Json(new
			{
				success = true,
				data = carVariationAttributeViewModel.CarVariationId,
				carVariation = new
				{
					id = carVariationAttributeViewModel.CarVariationId,
					attributes = attributes
				},
				message = "Car variation attributes updated.",
			}, JsonRequestBehavior.AllowGet);
		}

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