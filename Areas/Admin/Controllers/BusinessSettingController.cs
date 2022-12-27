using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class BusinessSettingController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IBusinessSettingService _businessSettingService;
		private readonly ICouponService _couponService;

		public BusinessSettingController(IBusinessSettingService businessSettingService, ICouponService couponService)
		{
			this._businessSettingService = businessSettingService;
			this._couponService = couponService;
		}

		public ActionResult Index()
		{
			BusinessSetting business = new BusinessSetting();
			ViewBag.SuccessMessage = TempData["SuccessMessage"];

            var businesssettingSetting = _businessSettingService.GetDefaultBusinessSetting();
            //ViewBag.CouponID = new SelectList(_couponService.GetCouponsForDropDown(false), "value", "text", businesssettingSetting.CouponID);
            //if (businesssettingSetting != null)
            business = businesssettingSetting;

            return View(business);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		public ActionResult Update(long? id, BusinessSetting businessSetting)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (id.HasValue && id > 0)
				{
					if (_businessSettingService.UpdateBusinessSetting(ref businessSetting, ref message))
					{
						TempData["SuccessMessage"] = message;
						return RedirectToAction("Index");
					}
				}
				else
				{
					if (_businessSettingService.CreateBusinessSetting(businessSetting, ref message))
					{
						TempData["SuccessMessage"] = message;
						return RedirectToAction("Index");
					}
				}
			}
			else
			{
				message = "Please fill the form properly ...";
			}
			ViewBag.ErrorMessage = message;
			return View("Index", businessSetting);
		}
	}
}
