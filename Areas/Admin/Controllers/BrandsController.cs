using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Order;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class BrandsController : Controller
	{
		private readonly IBrandsService _brandsService;

		public BrandsController(IBrandsService brandsService)
		{
			this._brandsService = brandsService;

		}
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;
		// GET: Admin/Brands
		public ActionResult Index()
		{
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			ViewBag.ErrorMessage = TempData["ErrorMessage"];
			ViewBag.ExcelUploadErrorMessage = TempData["ExcelUploadErrorMessage"];
			return View();
		}
		public ActionResult List()
		{
			var brands = _brandsService.GetBrands();
			return PartialView(brands);

		}
		public ActionResult Create()
		{
			return View();
		}
		[HttpPost]
		public ActionResult Create(string Name, string NameAr, string Slug)
		{
			string message = string.Empty;

			if (ModelState.IsValid)
			{
				//string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/Brands/"), Name.Replace(" ", "_"), "/Image");

				string absolutePath = Server.MapPath("~");
				string relativePath = string.Format("/Assets/AppFiles/Images/Brands/{0}/", Name.Replace(" ", "_"));
				Brand data = new Brand();
				data.Name = Name;
				data.NameAr = NameAr;
				data.Slug = Slug;

				data.Logo = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "logo", ref message, "Image");
				if (_brandsService.Createbrands(data, ref message))
				{
					return Json(new
					{
						success = true,
						url = "/Admin/Brands/Index",
						message = message,
						data = new
						{
							Date = data.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
							Name = data.Logo + "|" + data.Name,
							IsActive = data.IsActive.HasValue ? data.IsActive.Value.ToString() : bool.FalseString,
							ID = data.ID
						}
					});
				}
			}
			else
			{
				message = "Please fill the form properly ...";
			}

			return Json(new { success = false, message = message });
		}
		public ActionResult Edit(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Brand brand = _brandsService.GetBrand((long)id);
			if (brand == null)
			{
				return HttpNotFound();
			}

			TempData["brandID"] = id;
			return View(brand);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(long id, string Name, string NameAr, string Slug)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				long Id;
				if (TempData["brandID"] != null && Int64.TryParse(TempData["brandID"].ToString(), out Id) && id == Id)
				{
					Brand data = _brandsService.GetBrand(id);
					data.Name = Name;
					data.NameAr = NameAr;
					data.Slug = Slug;

					if (Request.Files["Image"] != null)
					{
						//string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/Brands/"), Name.Replace(" ", "_"), "/Image");

						string absolutePath = Server.MapPath("~");
						string relativePath = string.Format("/Assets/AppFiles/Images/Brands/{0}/", Name.Replace(" ", "_"));
						data.Logo = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "logo", ref message, "Image");
					}


					if (_brandsService.UpdateBrand(ref data, ref message))
					{
						return Json(new
						{
							success = true,
							url = "/Admin/Brands/Index",
							message = "Brand updated successfully ...",
							data = new
							{
								Date = data.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
								Name = data.Logo + "|" + data.Name,
								IsActive = data.IsActive.HasValue ? data.IsActive.Value.ToString() : bool.FalseString,
								ID = data.ID
							}
						});
					}

				}
				else
				{
					message = "Oops! Something went wrong. Please try later.";
				}
			}
			else
			{
				message = "Please fill the form properly ...";
			}
			return Json(new { success = false, message = message });
		}
		public ActionResult Delete(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Brand brand = _brandsService.GetBrand((Int16)id);
			if (brand == null)
			{
				return HttpNotFound();
			}
			TempData["brandID"] = id;
			return View(brand);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(long id)
		{
			string message = string.Empty;
			if (_brandsService.DeleteBrand((Int16)id, ref message))
			{
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}
		public ActionResult Activate(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			var brand = _brandsService.GetBrand((long)id);
			if (brand == null)
			{
				return HttpNotFound();
			}

			if (!(bool)brand.IsActive)
				brand.IsActive = true;
			else
			{
				brand.IsActive = false;
			}
			string message = string.Empty;
			if (_brandsService.UpdateBrand(ref brand, ref message))
			{
				SuccessMessage = "Brand " + ((bool)brand.IsActive ? "activated" : "deactivated") + "  successfully ...";
				return Json(new
				{
					success = true,
					message = SuccessMessage,
					data = new
					{
						Date = brand.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
						Name = brand.Logo + "|" + brand.Name,
						IsActive = brand.IsActive.HasValue ? brand.IsActive.Value.ToString() : bool.FalseString,
						ID = brand.ID
					}
				}, JsonRequestBehavior.AllowGet);
			}
			else
			{
				ErrorMessage = "Oops! Something went wrong. Please try later.";
			}

			return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
		}
		public ActionResult Details(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Brand brand = _brandsService.GetBrand((Int16)id);
			if (brand == null)
			{
				return HttpNotFound();
			}
			return View(brand);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult BrandsReport()
		{
			string ImageServer = CustomURL.GetImageServer();
			var getAllBrands = _brandsService.GetBrands().ToList();
			if (getAllBrands.Count() > 0)
			{
				using (ExcelPackage excel = new ExcelPackage())
				{
					excel.Workbook.Worksheets.Add("BrandsReport");

					var headerRow = new List<string[]>()
					{
					new string[] {
						"Creation Date"
						,"Name"
						,"Slug"
						,"Logo"
						,"Status"
						}
					};

					// Determine the header range (e.g. A1:D1)
					string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

					// Target a worksheet
					var worksheet = excel.Workbook.Worksheets["BrandsReport"];

					// Popular header row data
					worksheet.Cells[headerRange].LoadFromArrays(headerRow);

					var cellData = new List<object[]>();

					if (getAllBrands.Count != 0)
						getAllBrands = getAllBrands.OrderByDescending(x => x.ID).ToList();

					foreach (var i in getAllBrands)
					{
						cellData.Add(new object[] {
						i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
						,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
						,!string.IsNullOrEmpty(i.Slug) ? i.Slug : "-"
						,!string.IsNullOrEmpty(i.Logo) ? (ImageServer + i.Logo) : "-"
						,i.IsActive == true ? "Active" : "InActive"
						});
					}

					worksheet.Cells[2, 1].LoadFromArrays(cellData);

					return File(excel.GetAsByteArray(), "application/msexcel", "Brands Report.xlsx");
				}
			}
			return RedirectToAction("Index");
		}
	}
}