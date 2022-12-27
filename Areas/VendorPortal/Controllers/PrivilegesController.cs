using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
	public class PrivilegesController : Controller
	{
		// GET: VendorPortal/Privileges
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult UnAuthorize()
		{
			return View();
		}

		public ActionResult UnAuthorizeAJAX()
		{
			return View();
		}
	}
}