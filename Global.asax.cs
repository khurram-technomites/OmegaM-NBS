using NowBuySell.Web.Controllers;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace NowBuySell.Web
{
	public class MvcApplication : System.Web.HttpApplication 
	{
		protected void Application_Start()
		{

			GlobalConfiguration.Configure(WebApiConfig.Register);
			//AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			log4net.Config.XmlConfigurator.Configure();

			// Autofac and Automapper configurations
			Bootstrapper.Run();
		}

		protected void Application_EndRequest()
		{
			//if (Context.Response.StatusCode == 404)
			//{
			//	Response.Clear();

			//	var routeData = new RouteData();
			//	HttpContextBase currentContext = new HttpContextWrapper(HttpContext.Current);
			//	var culture = RouteTable.Routes.GetRouteData(currentContext).Values["culture"];
			//	routeData.Values["culture"] = culture;
			//	routeData.Values["controller"] = "Error";
			//	routeData.Values["action"] = "PageNotFound";

			//	IController customErrorController = new ErrorController();
			//	customErrorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
			//}
			//if (Context.Response.StatusCode == 500)
			//{
			//	Response.Clear();

			//	var routeData = new RouteData();
			//	HttpContextBase currentContext = new HttpContextWrapper(HttpContext.Current);
			//	var culture = RouteTable.Routes.GetRouteData(currentContext).Values["culture"];
			//	routeData.Values["culture"] = culture;
			//	routeData.Values["controller"] = "Error";
			//	routeData.Values["action"] = "InternalServerError";

			//	IController customErrorController = new ErrorController();
			//	customErrorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
			//}
		}
	}

}
