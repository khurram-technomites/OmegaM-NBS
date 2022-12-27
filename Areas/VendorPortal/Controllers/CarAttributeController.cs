using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
	[AuthorizeVendor]
	public class CarAttributeController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly ICarAttributeService _carAttributeService;
		private readonly IAttributeService _attributeService;

		public CarAttributeController(ICarAttributeService carAttributeService, IAttributeService attributeService)
		{
			this._carAttributeService = carAttributeService;
			this._attributeService = attributeService;
		}

		[HttpGet]
		public ActionResult GetCarAttributes(long id)
		{
			var carAttributes = _carAttributeService.GetCarAttributes(id).Select(i => new
			{
				id = i.ID.ToString(),
				value = i.Value,
				attributeId = i.AttributeID
			}).ToList();

			return Json(new
			{
				success = true,
				message = "Data recieved successfully!",
				carAttributes = carAttributes
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(CarAttribute carAttribute)
		{
			try
			{
				string message = string.Empty;
				if (ModelState.IsValid)
				{
					bool isAlreadyExist = false;
					if (_carAttributeService.CreateCarAttribute(ref carAttribute, ref message, ref isAlreadyExist))
					{
						return Json(new
						{
							success = true,
							data = carAttribute.ID,
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

			try
			{
				string message = string.Empty;
				if (_carAttributeService.DeleteCarAttribute(id, ref message))
				{
					return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
				}
				return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
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

		[HttpPost, ActionName("DeleteValue")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteValue(CarAttribute carAttribute)
		{
			try
			{
				string message = string.Empty;
				long id = 0;
				if (_carAttributeService.DeleteCarAttribute(carAttribute, ref message, ref id))
				{
					return Json(new
					{
						success = true,
						data = id,
						message = message
					}, JsonRequestBehavior.AllowGet);
				}
				return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);

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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteAll(CarAttribute carAttribute)
		{
			try
			{
				string message = string.Empty;
				if (_carAttributeService.DeleteCarAttributes(carAttribute, ref message))
				{
					return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
				}
				return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);

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