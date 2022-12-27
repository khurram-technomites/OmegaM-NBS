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
	public class CarAttributeController : Controller
	{
		// GET: Admin/CarAttribute
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
			string message = string.Empty;
			bool isAlreadyExist = false;

			if (ModelState.IsValid)
			{
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


		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(long id)
		{
			string message = string.Empty;
			if (_carAttributeService.DeleteCarAttribute(id, ref message))
			{
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost, ActionName("DeleteValue")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteValue(CarAttribute carAttribute)
		{
			string message = string.Empty;
			long id = 0;
			if (_carAttributeService.DeleteCarAttribute(carAttribute, ref message, ref id))
			{
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteAll(CarAttribute carAttribute)
		{
			string message = string.Empty;
			if (_carAttributeService.DeleteCarAttributes(carAttribute, ref message))
			{
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}
	}
}