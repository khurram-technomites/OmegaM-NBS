using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Net;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class TaxSettingController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly ITaxSettingService _taxsettingService;


		public TaxSettingController(ITaxSettingService taxsettingService)
		{
			this._taxsettingService = taxsettingService;
		}

		public ActionResult Index()
		{
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			ViewBag.ErrorMessage = TempData["ErrorMessage"];
			return View();
		}

		public ActionResult List()
		{
			var taxsettings = _taxsettingService.GetTaxSettings();
			return PartialView(taxsettings);
		}

		public ActionResult ListReport()
		{
			var taxsettings = _taxsettingService.GetTaxSettings();
			return View(taxsettings);
		}

		public ActionResult Details(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			TaxSetting taxsetting = _taxsettingService.GetTaxSetting((Int16)id);
			if (taxsetting == null)
			{
				return HttpNotFound();
			}
			return View(taxsetting);
		}

		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(TaxSetting taxsetting)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (_taxsettingService.CreateTaxSetting(taxsetting, ref message))
				{
					return Json(new
					{
						success = true,
						url = "/Admin/TaxSetting/Index",
						message = message,
						data = new
						{
							ID = taxsetting.ID,
							Date = taxsetting.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
							TaxName = taxsetting.TaxName,
							//TaxNameAr = taxsetting.TaxNameAr,
							TaxPercentage = taxsetting.TaxPercentage,
							IsActive = taxsetting.IsActive.HasValue ? taxsetting.IsActive.Value.ToString() : bool.FalseString
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
			TaxSetting taxsetting = _taxsettingService.GetTaxSetting((long)id);
			if (taxsetting == null)
			{
				return HttpNotFound();
			}
			TempData["TaxSettingID"] = id;
			return View(taxsetting);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(TaxSetting taxsetting)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				long Id;
				if (TempData["TaxSettingID"] != null && Int64.TryParse(TempData["TaxSettingID"].ToString(), out Id) && taxsetting.ID == Id)
				{
					if (_taxsettingService.UpdateTaxSetting(ref taxsetting, ref message))
					{
						return Json(new
						{
							success = true,
							url = "/Admin/TaxSetting/Index",
							message = "TaxSetting updated successfully ...",
							data = new
							{
								ID = taxsetting.ID,
								Date = taxsetting.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
								TaxName = taxsetting.TaxName,
								TaxNameAr = taxsetting.TaxNameAr,
								TaxPercentage = taxsetting.TaxPercentage,
								IsActive = taxsetting.IsActive.HasValue ? taxsetting.IsActive.Value.ToString() : bool.FalseString
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


		public ActionResult Activate(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			var taxsetting = _taxsettingService.GetTaxSetting((long)id);
			if (taxsetting == null)
			{
				return HttpNotFound();
			}

			if (!(bool)taxsetting.IsActive)
				taxsetting.IsActive = true;
			else
			{
				taxsetting.IsActive = false;
			}
			string message = string.Empty;
			if (_taxsettingService.UpdateTaxSetting(ref taxsetting, ref message))
			{
				SuccessMessage = "TaxSetting " + ((bool)taxsetting.IsActive ? "activated" : "deactivated") + "  successfully ...";
				return Json(new
				{
					success = true,
					message = SuccessMessage,
					data = new
					{
						ID = taxsetting.ID,
						Date = taxsetting.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
						TaxName = taxsetting.TaxName,
						TaxPercentage = taxsetting.TaxPercentage,
						IsActive = taxsetting.IsActive.HasValue ? taxsetting.IsActive.Value.ToString() : bool.FalseString
					}
				}, JsonRequestBehavior.AllowGet);
			}
			else
			{
				ErrorMessage = "Oops! Something went wrong. Please try later.";
			}

			return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Delete(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			TaxSetting taxsetting = _taxsettingService.GetTaxSetting((Int16)id);
			if (taxsetting == null)
			{
				return HttpNotFound();
			}
			TempData["TaxSettingID"] = id;
			return View(taxsetting);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(long id)
		{
			string message = string.Empty;
			if (_taxsettingService.DeleteTaxSetting((Int16)id, ref message))
			{
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}
	}
}