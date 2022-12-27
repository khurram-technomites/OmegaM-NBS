using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               "Admin_login",
               "admin",
               new { controller = "Account", action = "Login" }
               );

            context.MapRoute(
             "Admin_default",
             "Admin/{controller}/{action}/{id}",
             new { action = "Index", id = UrlParameter.Optional }

         );           
        }
    }
}