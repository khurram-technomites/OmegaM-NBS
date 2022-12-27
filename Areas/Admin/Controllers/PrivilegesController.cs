using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	public class PrivilegesController : Controller
	{
		// GET: Admin/Privileges
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