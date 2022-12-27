using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.ViewModels.Api.Vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api.vendorApi
{

    [Authorize]
    [RoutePrefix("api/v1/vendor")]
    public class VendorSessionController : ApiController
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IVendorSessionService _vendorSessionService;


        public VendorSessionController(IVendorSessionService vendorSessionService)
        {
            this._vendorSessionService = vendorSessionService;
        }

		[HttpPost]
		[Route("sessions")]
		public HttpResponseMessage SaveDeviceToken(VendorSessionViewModel vendorSessionViewModel)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long vendorId;
				if (claims.Count() == 4 && Int64.TryParse(claims.FirstOrDefault().Value, out vendorId))
				{
					string message = string.Empty;
					string status = string.Empty;
					
					VendorSession vendorSession = new VendorSession()
					{
						VendorID = vendorId,
						DeviceID = vendorSessionViewModel.DeviceID,
						FirebaseToken = vendorSessionViewModel.FirebaseToken,
						AccessToken = vendorSessionViewModel.AccessToken,
					};

					if (_vendorSessionService.CreateVendorSession(ref vendorSession, ref message, ref status))
					{
						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "success",
							message = "Session captured!"
						});
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.OK, new { status = status, message = message });
					}
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}

			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}

	}
}
