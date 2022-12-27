using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class PaymentGatewaySettingController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IPaymentGatewaySettingService _paymentgatewaysettingService;

		public PaymentGatewaySettingController(IPaymentGatewaySettingService paymentgatewaysettingService)
		{
			this._paymentgatewaysettingService = paymentgatewaysettingService;
		}

		public ActionResult Index()
		{
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			PaymentGatewaySetting paymentGatewaySetting = _paymentgatewaysettingService.GetDefaultPaymentGatewaySetting();

			return View(paymentGatewaySetting);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Update(long? id, PaymentGatewaySetting paymentGatewaySetting)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (id.HasValue && id > 0)
				{
					if (_paymentgatewaysettingService.UpdatePaymentGatewaySetting(ref paymentGatewaySetting, ref message))
					{
						TempData["SuccessMessage"] = message;
						return RedirectToAction("Index");
					}
				}
				else
				{
					if (_paymentgatewaysettingService.CreatePaymentGatewaySetting(paymentGatewaySetting, ref message))
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