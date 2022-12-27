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
	public class CarCategoryController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly ICarCategoryService _carCategoryService;
		private readonly ICategoryService _categoryService;

		public CarCategoryController(ICarCategoryService carCategoryService, ICategoryService categoryService)
		{
			this._carCategoryService = carCategoryService;
			this._categoryService = categoryService;
		}

		[HttpGet]
		public ActionResult GetCarCategories(long id)
		{
			var category = _categoryService.GetCategories(string.Empty, "en").Select(i => new { id = i.ID, name = i.Name, ParentId = i.ParentID, hasChilds = i.hasChilds });
			var carCategory = _carCategoryService.GetCarCategories(id).Select(i => new { id = i.ID, categoryId = i.CarCategoryID , module = i.Category.Module}).ToList();

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
		public ActionResult Create(CarCategory carCategory)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (_carCategoryService.CreateCarCategory(ref carCategory, ref message))
				{
					return Json(new
					{
						success = true,
						data = carCategory.ID,
						message = "Car category assigned.",
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
			if (_carCategoryService.DeleteCarCategory((Int16)id, ref message))
			{
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}
	}
}