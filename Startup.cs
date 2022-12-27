using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System.Web.Http;
using System;
using NowBuySell.Web.AuthorizationProvider.OAuth;

[assembly: OwinStartupAttribute(typeof(NowBuySell.Web.Startup))]
namespace NowBuySell.Web
{
	public partial class Startup
	{

		public void Configuration(IAppBuilder app)
		{
			var config = new HttpConfiguration();
			//other configurations

			ConfigureOAuth(app);
			app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
			app.UseWebApi(config);
			app.MapSignalR();
		}

		public void ConfigureOAuth(IAppBuilder app)
		{
			var oAuthServerOptions = new OAuthAuthorizationServerOptions()
			{
				TokenEndpointPath = new PathString("/api/security/token"),
				AccessTokenExpireTimeSpan = TimeSpan.FromDays(30),
				Provider = new ApplicationOAuthAuthorizationServerProvider(),

				// Only do this for demo!!
				AllowInsecureHttp = true,
				RefreshTokenProvider = new ApplicationRefreshTokenProvider()
			};

			app.UseOAuthAuthorizationServer(oAuthServerOptions);
			app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
		}
	}
}