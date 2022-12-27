using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System.Web.Mvc;


namespace NowBuySell.Web.Areas.Admin.Controllers
{

	[AuthorizeAdmin]
	public class MyFatoorahPaymentGatewaySettingsController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IMyFatoorahPaymentGatewaySettingsService _myFatoorahpaymentgatewaysettingService;

		public MyFatoorahPaymentGatewaySettingsController(IMyFatoorahPaymentGatewaySettingsService myFatoorahpaymentgatewaysettingService)
		{
			this._myFatoorahpaymentgatewaysettingService = myFatoorahpaymentgatewaysettingService;
		}

		public ActionResult Index()
		{
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			MyFatoorahPaymentGatewaySetting myFatoorahpaymentGatewaySetting = _myFatoorahpaymentgatewaysettingService.GetDefaultPaymentGatewaySetting();

			return View(myFatoorahpaymentGatewaySetting);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Update(long? id, MyFatoorahPaymentGatewaySetting paymentGatewaySetting)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (id.HasValue && id > 0)
				{
					if (_myFatoorahpaymentgatewaysettingService.UpdateMyFatoorahPaymentGatewaySetting(ref paymentGatewaySetting, ref message))
					{
						TempData["SuccessMessage"] = message;
						return RedirectToAction("Index");
					}
				}
				else
				{
					if (_myFatoorahpaymentgatewaysettingService.CreateMyFatoorahPaymentGatewaySetting(paymentGatewaySetting, ref message))
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
			return View("Index", paymentGatewaySetting);
		}
	}
}