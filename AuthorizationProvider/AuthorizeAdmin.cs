using NowBuySell.Service;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NowBuySell.Web.AuthorizationProvider
{
    public class AuthorizeAdmin : AuthorizeAttribute
    {
        public string AccessLevel { get; set; }
        public bool IsAuthorized { get; set; }
		private readonly IUserService _userService;

		protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            IsAuthorized = true;
            var AccessToken = httpContext.Request.Cookies["Admin-Session"] != null ? httpContext.Request.Cookies["Admin-Session"]["Access-Token"] : null;

            string UserRole = httpContext.Session["Role"] != null ? httpContext.Session["Role"].ToString() : string.Empty;

            if (httpContext.Session["AdminUserID"] != null && httpContext.Session["UserName"] != null)
            {
                var request = httpContext.Request;
                string controller = request.RequestContext.RouteData.Values["controller"].ToString();
                string action = request.RequestContext.RouteData.Values["action"].ToString();

                string currentRequest = controller + "/" + action;
                string[] adminRoutes = File.ReadAllLines(httpContext.Server.MapPath("/AuthorizationProvider/Privileges/Admin/" + UserRole + ".txt"));
                string[] generalRoutes = File.ReadAllLines(httpContext.Server.MapPath("/AuthorizationProvider/Privileges/Admin/GeneralLinksAdmin.txt"));
                if (adminRoutes.Contains(currentRequest.ToLower()) || generalRoutes.Contains(currentRequest.ToLower()))
                {
                    return true;
                }
                else
                {
                    IsAuthorized = false;
                    return false;
                }
            }
            else if (AccessToken != null && string.IsNullOrEmpty(AccessToken))
            {
                var user = _userService.GetByAuthCode(AccessToken);
                if (user != null)
                {
                    var request = httpContext.Request;
                    string controller = request.RequestContext.RouteData.Values["controller"].ToString();
                    string action = request.RequestContext.RouteData.Values["action"].ToString();

                    string currentRequest = controller + "/" + action;

                    string[] adminRoutes = File.ReadAllLines(httpContext.Server.MapPath("/AuthorizationProvider/Privileges/Admin/" + user.UserRole.RoleName + ".txt"));
                    string[] generalRoutes = File.ReadAllLines(httpContext.Server.MapPath("/AuthorizationProvider/Privileges/Admin/GeneralLinksAdmin.txt"));

                    if (adminRoutes.Contains(currentRequest) | generalRoutes.Contains(currentRequest.ToLower()))
                    {
                        httpContext.Session["AdminUserID"] = user.ID;
                        httpContext.Session["UserName"] = user.Name;
                        httpContext.Session["Role"] = user.UserRole.RoleName;
                        httpContext.Session["Email"] = user.EmailAddress;

                        httpContext.Response.Cookies["Admin-Session"]["Access-Token"] = user.AuthorizationCode;
                        return true;
                    }
                    else
                    {
                        IsAuthorized = false;
                        return false;
                    }
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
            if (IsAuthorized)
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
                            LogOnUrl = urlHelper.Action("Login", "Account", new { area = "Admin", referrer = filterContext.RequestContext.HttpContext.Request.UrlReferrer })
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                controller = "Account",
                                action = "Login",
                                area = "Admin"
                            })
                        );
                }
            }
            else
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    var urlHelper = new UrlHelper(filterContext.RequestContext);
                    filterContext.HttpContext.Response.StatusCode = 403;
                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            Error = "Access Denied",
                            Message = "Your are not authorize to perform this action, For further details please contact administrator !"
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(
                                            new RouteValueDictionary(
                                                new
                                                {
                                                    controller = "Privileges",
                                                    action = "UnAuthorize",
                                                    area = "Admin",
                                                    referrer = filterContext.RequestContext.HttpContext.Request.UrlReferrer
                                                })
                                            );
                }
            }
        }
    }
}