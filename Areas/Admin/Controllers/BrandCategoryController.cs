using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System.Linq;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class BrandCategoryController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IBrandCategoryService _brandCategoryService;
		private readonly IBrandsService _brandsService;
		private readonly ICategoryService _categoryService;

		public BrandCategoryController(IBrandCategoryService brandCategoryService, IBrandsService brandsService, ICategoryService categoryService)
		{
			this._brandCategoryService = brandCategoryService;
			this._brandsService = brandsService;
			this._categoryService = categoryService;
		}

		public ActionResult Index(long id)
		{
			ViewBag.BrandID = id;
			return View();
		}

		[HttpGet]
		public ActionResult GetAll(long id)
		{
			var brands = _categoryService.GetCarCategories().Select(i => new
			{
				id = i.ID.ToString(),
				value = i.CategoryName
			}).ToList();

			var brandCategories = _brandCategoryService.GetBrandCategories(id).Select(i => new
			{
				id = i.CategoryID.ToString(),
				value = i.Category.CategoryName,
				brandCategoryId = i.ID
			}).ToList();

			return Json(new
			{
				success = true,
				message = "Data recieved successfully!",
				brands = brands,
				brandCategories = brandCategories
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(BrandCategory brandCategory)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (_brandCategoryService.CreateBrandCategory(ref brandCategory, ref message))
				{
					return Json(new
					{
						success = true,
						data = brandCategory.ID,
						message = "Brand category assigned.",
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
		public ActionResult DeleteConfirmed(BrandCategory brandCategory)
		{
			string message = string.Empty;
			if (_brandCategoryService.DeleteBrandCategory(brandCategory, ref message))
			{
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}
	}
}