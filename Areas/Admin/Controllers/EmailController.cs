using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class EmailController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IEmailService _emailService;

		public EmailController(IEmailService emailService)
		{
			this._emailService = emailService;
		}

		public ActionResult Index()
		{
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			Email email = _emailService.GetDefaultEmailSetting();

			return View(email);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Update(long? id, Email email)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (id.HasValue && id > 0)
				{
					if (_emailService.UpdateEmail(ref email, ref message))
					{
						TempData["SuccessMessage"] = message;
						return RedirectToAction("Index");
					}
				}
				else
				{
					if (_emailService.CreateEmail(email, ref message))
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
			return View("Index", email);
		}

	}
}