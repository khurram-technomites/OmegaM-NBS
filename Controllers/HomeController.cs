using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Service.Helpers;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.ViewModels.Account;
using NowBuySell.Web.ViewModels.Vendor;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Controllers
{
	public class HomeController : Controller
	{

		public HomeController() { 
		}

		public ActionResult Index()
		{
			return View();
		}

		[Route("about-us", Name = "about-us")]
		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		
		[Route("about-us/{id}")]
		public ActionResult AboutUS(int id)
		{
			ViewBag.Message = "Your application description page.";
			ViewBag.id = id;
			return View();
		}
		
		[Route("about-us/Edit/{id}")]
		public ActionResult AboutUSEdit(string id)
		{
			ViewBag.Message = "Your application description page.";
			ViewBag.id = id;
			return View();
		}


		[Route("privacy-policy", Name = "privacy-policy")]
		public ActionResult PrivacyPolicy()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}


		[Route("terms-and-conditions", Name = "terms-and-conditions")]
		public ActionResult TermandCondition()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		[Route("frequently-asked-questions", Name = "frequently-asked-questions")]
		public ActionResult FAQS()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

	}
}