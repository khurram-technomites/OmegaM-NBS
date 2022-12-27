using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Areas.VendorPortal.ViewModels.Car;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
	[AuthorizeVendor]
	public class CarVariationController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly ICarVariationService _carVariationService;
		private readonly ICarVariationAttributeService _carVariationAttributeService;

		public CarVariationController(ICarVariationService carVariationService, ICarVariationAttributeService carVariationAttributeService)
		{
			this._carVariationService = carVariationService;
			this._carVariationAttributeService = carVariationAttributeService;
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
		public ActionResult Create(CarVariation carVariation, CarVariationAttributesViewModel variationAttributes)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				var carVariationAttributes = _carVariationAttributeService.GetVariationAttributesByCar((long)carVariation.CarID);

				foreach (var item in carVariationAttributes.Select(i => i.CarVariationID).Distinct().ToArray())
				{
					IEnumerable<long> var_attr = carVariationAttributes.Where(i => i.CarVariationID == item).Select(i => i.CarAttributeID.Value).Distinct().ToList();

					bool isEqual = var_attr.SequenceEqual(variationAttributes.CarAttributes);
					if (isEqual)
					{
						return Json(new
						{
							success = false,
							message = "Variation with same attribute combination already exist.",
						});
					}
				}

				if (_carVariationService.CreateCarVariation(ref carVariation, ref message))
				{
					var attributes = new List<long>();
					foreach (var carAttribute in variationAttributes.CarAttributes)
					{
						CarVariationAttribute carVariationAttribute = new CarVariationAttribute()
						{
							CarID = carVariation.CarID,
							CarVariationID = carVariation.ID,
							CarAttributeID = carAttribute
						};
						if (_carVariationAttributeService.CreateCarVariationAttribute(ref carVariationAttribute, ref message))
						{
							attributes.Add(carAttribute);
						}
					}
					return Json(new
					{
						success = true,
						data = carVariation.ID,
						carVariation = new
						{
							id = carVariation.ID,
							attributes = attributes
						},
						message = "Variation created successfully.",
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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Thumbnail(long? id)
		{
			try
			{
				if (id == null)
				{
					return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
				}
				var carVariation = _carVariationService.GetCarVariation((long)id);
				if (carVariation == null)
				{
					return HttpNotFound();
				}
				string filePath = !string.IsNullOrEmpty(carVariation.Thumbnail) ? carVariation.Thumbnail : string.Empty;

				string message = string.Empty;

				string absolutePath = Server.MapPath("~");
				string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/Variations/{1}/", carVariation.Car.SKU.Replace(" ", "_"), carVariation.SKU.Replace(" ", "_"));

				carVariation.Thumbnail = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "Thumbnail", ref message, "Image");

				if (_carVariationService.UpdateCarVariation(ref carVariation, ref message, false))
				{
					if (!string.IsNullOrEmpty(filePath))
					{
						System.IO.File.Delete(Server.MapPath(filePath));
					}
					return Json(new
					{
						success = true,
						message = string.Format("{0} thumbnail image uploaded.", carVariation.SKU),
						data = carVariation.Thumbnail
					});
				}
				return Json(new { success = false, message = message });
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}
	}
}