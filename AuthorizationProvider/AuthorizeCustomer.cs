using NowBuySell.Service;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NowBuySell.Web.AuthorizationProvider
{
	public class AuthorizeCustomer : AuthorizeAttribute
	{
		public string AccessLevel { get; set; }
		private readonly ICustomerService _customerService;

		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			var AccessToken = httpContext.Request.Cookies["Admin-Session"] != null ? httpContext.Request.Cookies["Admin-Session"]["Access-Token"] : null;


			if (httpContext.Session["CustomerID"] != null && httpContext.Session["CustomerName"] != null)
			{
				return true;

			}
			else if (AccessToken != null && string.IsNullOrEmpty(AccessToken))
			{
				var customer = _customerService.GetByAuthCode(AccessToken);
				if (customer != null)
				{
					httpContext.Session["CustomerID"] = customer.ID;
					httpContext.Session["CustomerName"] = customer.Name;
					httpContext.Session["Email"] = customer.Email;

					httpContext.Response.Cookies["Customer-Session"]["Access-Token"] = customer.AuthorizationCode;

					return true;
				}
				else
				{
					return false;
				}
			}
			return false;
		}

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
		{
			if (filterContext.HttpContext.Request.IsAjaxRequest())
			{
				var urlHelper = new UrlHelper(filterContext.RequestContext);
				filterContext.HttpContext.Response.StatusCode = 403;
				filterContext.Result = new JsonResult
				{
					Data = new
					{
						Error = "NotAuthorized",
						//LogOnUrl = urlHelper.Action("Login", "Account", new { area = "CustomerPortal" })
						LogOnUrl = urlHelper.Action("Index", "Home", new { area = "" })
					},
					JsonRequestBehavior = JsonRequestBehavior.AllowGet
				};
			}
			else
			{
				filterContext.Result = new RedirectToRouteResult(
					//new RouteValueDictionary(new { controller = "Account", action = "Login", area = "CustomerPortal" })
					new RouteValueDictionary(new { controller = "Home", action = "Index", area = "" })
					);
			}

		}
	}
}