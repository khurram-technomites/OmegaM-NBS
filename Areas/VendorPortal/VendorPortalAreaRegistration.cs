using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal
{
	public class VendorPortalAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "VendorPortal";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
			   "VendorPortal_login",
			   "vendor",
			   new { controller = "Account", action = "Login" }
			   );

			context.MapRoute(
				"VendorPortal_default",
				"Vendor/{controller}/{action}/{id}",
				new { action = "Index", id = UrlParameter.Optional },
				namespaces: new[] { "NowBuySell.Web.Areas.VendorPortal.Controllers" }
			);
		}
	}
}