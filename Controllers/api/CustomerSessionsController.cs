using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.ViewModels.Api.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
	[Authorize]
	[RoutePrefix("api/v1")]
	public class CustomerSessionsController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICustomerSessionService _customerSessionService;


		public CustomerSessionsController(ICustomerSessionService customerSessionService)
		{
			this._customerSessionService = customerSessionService;
		}

		[HttpPost]
		[Route("sessions")]
		public HttpResponseMessage SaveDeviceToken(CustomerSessionViewModel customerSessionViewModel)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					string message = string.Empty;
					string status = string.Empty;

                    _customerSessionService.ExpireSession(customerId, customerSessionViewModel.DeviceID);

                    CustomerSession customerSession = new CustomerSession()
					{
						CustomerID = customerId,
						DeviceID = customerSessionViewModel.DeviceID,
						FirebaseToken = customerSessionViewModel.FirebaseToken,
						AccessToken = customerSessionViewModel.AccessToken,
					};

					if (_customerSessionService.CreateCustomerSession(ref customerSession, ref message, ref status))
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
