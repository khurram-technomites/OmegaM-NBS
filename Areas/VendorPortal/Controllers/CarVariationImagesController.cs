using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
	[AuthorizeVendor]
	public class CarVariationImagesController : Controller
	{

		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly ICarVariationImageService _carVariationImageService;
		private readonly ICarVariationService _carVariationService;

		public CarVariationImagesController(ICarVariationImageService carVariationImageService, ICarVariationService carVariationService)
		{
			this._carVariationImageService = carVariationImageService;
			this._carVariationService = carVariationService;
		}

		[HttpGet]
		public ActionResult GetCarVariationImages(long id)
		{
			var carImages = _carVariationImageService.GetCarVariationImages(id).Select(i => new
			{
				id = i.ID,
				i.Title,
				Image = i.Image,
				position = i.Position,
			}).ToList();

			return Json(new
			{
				success = true,
				message = "Data recieved successfully!",
				carImages = carImages
			}, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Create(long? id, int count)
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

				string message = string.Empty;

				string absolutePath = Server.MapPath("~");
				string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/Variations/{1}/Gallery/", carVariation.Car.SKU.Replace(" ", "_"), carVariation.SKU.Replace(" ", "_"));

				List<string> Pictures = new List<string>();

				Dictionary<long, string> data = new Dictionary<long, string>();
				Uploader.UploadImages(Request.Files, absolutePath, relativePath, "PVGI", ref Pictures, ref message, "CarVariationGalleryImages");
				foreach (var item in Pictures)
				{
					CarVariationImage carVariationImage = new CarVariationImage();
					carVariationImage.CarID = carVariation.CarID;
					carVariationImage.CarVariationID = carVariation.ID;
					carVariationImage.Image = item;
					carVariationImage.Position = ++count;

					if (_carVariationImageService.CreateCarVariationImage(ref carVariationImage, ref message))
					{
						data.Add(carVariationImage.ID, item);
					}
				}

				return Json(new
				{
					success = true,
					message = message,
					data = data.ToList()
				});
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

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(long id)
		{
			string message = string.Empty;
			string filePath = string.Empty;
			if (_carVariationImageService.DeleteCarVariationImage(id, ref message, ref filePath))
			{
				System.IO.File.Delete(Server.MapPath(filePath));
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}
	}
}