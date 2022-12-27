using NowBuySell.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;

namespace NowBuySell.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //enabling attribute routing
            routes.MapMvcAttributeRoutes();
            AreaRegistration.RegisterAllAreas();

            routes.MapRoute(
                            name: "Default",
                            url: "{controller}/{action}/{id}",
                            defaults: new { controller = "Default", action = "Index", id = UrlParameter.Optional },
                            namespaces: new[] { "NowBuySell.Web.Controllers" }
                        );
        }
    }
}
