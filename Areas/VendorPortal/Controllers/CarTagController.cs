using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Linq;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
	[AuthorizeVendor]
	public class CarTagController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly ICarTagService _carTagService;
		private readonly ITagService _tagService;

		public CarTagController(ICarTagService carTagService, ITagService tagService)
		{
			this._carTagService = carTagService;
			this._tagService = tagService;
		}

		[HttpGet]
		public ActionResult GetCarTags(long id)
		{
			var tags = _tagService.GetTags().Select(i => new
			{
				id = i.ID.ToString(),
				value = i.Name
			}).ToList();
			var carTags = _carTagService.GetCarTags(id).Select(i => new
			{
				id = i.TagID.ToString(),
				value = i.Tag.Name,
				cartagId = i.ID
			}).ToList();

			return Json(new
			{
				success = true,
				message = "Data recieved successfully!",
				tags = tags,
				carTags = carTags
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(CarTag carTag)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (_carTagService.CreateCarTag(ref carTag, ref message))
				{
					return Json(new
					{
						success = true,
						data = carTag.ID,
						message = "Car tag assigned.",
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
		public ActionResult DeleteConfirmed(CarTag carTag)
		{
			string message = string.Empty;
			if (_carTagService.DeleteCarTag(carTag, ref message))
			{
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}
	}
}