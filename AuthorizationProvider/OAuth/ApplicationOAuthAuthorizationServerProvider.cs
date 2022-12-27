using NowBuySell.Service;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NowBuySell.Web.AuthorizationProvider.OAuth
{
    public class ApplicationOAuthAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            await Task.Run(() =>
            {
                context.Validated();
            });
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            string Message = string.Empty;
            try
            {
                var deviceId = (await context.Request.ReadFormAsync())["deviceId"];
                var type = (await context.Request.ReadFormAsync())["type"];

                if (String.IsNullOrEmpty(deviceId))
                {
                    context.SetError("Invalid Access", "Device Id required.");
                    return;
                }

                var identity = new ClaimsIdentity(context.Options.AuthenticationType);

                if (type == "Customer")
                {
                    var _customerService = DependencyResolver.Current.GetService<ICustomerService>();

                    if (_customerService.Authenticate(context.UserName, context.Password, ref identity, ref Message))
                    {
                        //roles example
                        var rolesTechnicalNamesUser = new string[] { "Branch" };

                        var principal = new GenericPrincipal(identity, rolesTechnicalNamesUser);
                        Thread.CurrentPrincipal = principal;

                        context.Validated(identity);
                    }
                    else
                    {
                        context.SetError("Invalid Access", Message);
                        return;
                    }
                }

                else if (type == "Vendor")
                {
                    var _vendorUserService = DependencyResolver.Current.GetService<IVendorUserService>();


                    if (_vendorUserService.Authenticate(context.UserName, context.Password, ref identity, ref Message,true))
                    {
                        //roles example
                        var rolesTechnicalNamesUser = new string[] { "Branch" };

                        var principal = new GenericPrincipal(identity, rolesTechnicalNamesUser);
                        Thread.CurrentPrincipal = principal;

                        context.Validated(identity);
                    }
                    else
                    {
                        context.SetError("Invalid Access", Message);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                context.SetError("Invalid Access", "Internal server error.");
            }
        }
    }
}