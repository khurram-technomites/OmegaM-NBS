using NowBuySell.Service;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NowBuySell.Web.AuthorizationProvider
{
    public class AuthorizeVendor : AuthorizeAttribute
    {
        public string AccessLevel { get; set; }
        public bool IsAuthorized { get; set; }
        //private readonly IVendorUserService _vendorUserService;

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            IsAuthorized = true;
            var AccessToken = httpContext.Request.Cookies["Vendor-Session"] != null ? httpContext.Request.Cookies["Vendor-Session"]["Access-Token"] : null;

            string UserRole = httpContext.Session["Role"] != null ? httpContext.Session["Role"].ToString() : string.Empty;
            var VendorID = httpContext.Session["VendorID"] != null ? httpContext.Session["VendorID"].ToString() : null;

            if (httpContext.Session["VendorUserID"] != null && httpContext.Session["VendorUserName"] != null)
            {
                var request = httpContext.Request;
                string controller = request.RequestContext.RouteData.Values["controller"].ToString();
                string action = request.RequestContext.RouteData.Values["action"].ToString();

                string currentRequest = controller + "/" + action;
                string adminRoutesPath = UserRole.Equals("Administrator") ? "/AuthorizationProvider/Privileges/Vendor/Administrator.txt" : string.Concat("/AuthorizationProvider/Privileges/Vendor/", VendorID, "/", UserRole, ".txt");
                string[] adminRoutes = File.ReadAllLines(httpContext.Server.MapPath(adminRoutesPath));
                string[] generalRoutes = File.ReadAllLines(httpContext.Server.MapPath("/AuthorizationProvider/Privileges/Vendor/GeneralLinksAdmin.txt"));
                if (adminRoutes.Contains(currentRequest.ToLower()) | generalRoutes.Contains(currentRequest.ToLower()))
                {
                    return true;
                }
                else
                {
                    IsAuthorized = false;
                    return false;
                }
            }
            else if (AccessToken != null && string.IsNullOrEmpty(AccessToken) && AccessToken != "")
            {
                var _vendorUserService = DependencyResolver.Current.GetService<IVendorUserService>();
                var user = _vendorUserService.GetByAuthCode(AccessToken);
                if (user != null)
                {
                    var request = httpContext.Request;
                    UserRole = user.VendorUserRole.Name;
                    string controller = request.RequestContext.RouteData.Values["controller"].ToString();
                    string action = request.RequestContext.RouteData.Values["action"].ToString();

                    string currentRequest = controller + "/" + action;

                    string adminRoutesPath = UserRole.Equals("Administrator") ? "/AuthorizationProvider/Privileges/Vendor/Administrator.txt" : string.Concat("/AuthorizationProvider/Privileges/Vendor/", user.VendorID, "/", UserRole, ".txt");
                    string[] adminRoutes = File.ReadAllLines(httpContext.Server.MapPath(adminRoutesPath));
                    string[] generalRoutes = File.ReadAllLines(httpContext.Server.MapPath("/AuthorizationProvider/Privileges/Vendor/GeneralLinksAdmin.txt"));

                    if (adminRoutes.Contains(currentRequest.ToLower()) | generalRoutes.Contains(currentRequest.ToLower()))
                    {
                        httpContext.Session["VendorUserID"] = user.ID;
                        httpContext.Session["VendorUserName"] = user.Name;
                        httpContext.Session["Role"] = UserRole;
                        httpContext.Session["Email"] = user.EmailAddress;
                        httpContext.Session["VendorID"] = user.VendorID;

                        httpContext.Response.Cookies["Vendor-Session"]["Access-Token"] = user.AuthorizationCode;
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
            else if (httpContext.Session["VendorIdle"] != null)
            {
                var request = httpContext.Request;
                string controller = request.RequestContext.RouteData.Values["controller"].ToString();
                string action = request.RequestContext.RouteData.Values["action"].ToString();

                string currentRequest = controller + "/" + action;

                if (currentRequest.ToLower() == "account/profilemanagement" || currentRequest.ToLower() == "account/packagesubscription" || 
                    currentRequest.ToLower() == "account/createdocuments" || currentRequest.ToLower() == "account/deletecardocument" || currentRequest.ToLower() == "account/getdocuments")
                    return true;

                return false;
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
                    filterContext.HttpContext.Response.StatusCode = 401;
                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            Error = "NotAuthorized",
                            LogOnUrl = urlHelper.Action("Login", "Account", new { area = "VendorPortal" })
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
                                area = "VendorPortal"
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
                                                    area = "VendorPortal"
                                                })
                                            );
                }
            }
        }
    }
}