using NowBuySell.Service;
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

	//[Authorize]
	[RoutePrefix("api/v1")]
	public class DeliveryChargesController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IDeliveryChargesService _deliveryChargesService;

		public DeliveryChargesController(IDeliveryChargesService deliveryChargesService)
		{
			this._deliveryChargesService = deliveryChargesService;
		}

		[HttpGet]
		[Route("areas/{areaId}/deliverycharges")]
		public HttpResponseMessage GetDeliveryCharges(long areaId)
		{
			try
			{
                //var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                //var claims = identity.Claims;
                //long customerId;
                //if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                //{
                    var deliveryCharges = _deliveryChargesService.GetDeliveryChargesByArea((long)areaId);
					if (deliveryCharges != null)
					{
						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "success",
							deliveryCharges = new
							{
								id = deliveryCharges.ID,
								area = deliveryCharges.Area != null ? new
								{
									id = deliveryCharges.AreaID,
									name = deliveryCharges.Area.Name
								} : null,
								minOrder = deliveryCharges.MinOrder,
								charges = deliveryCharges.Charges,
							}
						});
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "error",
							message = "Delivery not available!"
						});
					}
				//}
				//else
				//{
				//	return Request.CreateResponse(HttpStatusCode.Unauthorized, new
				//	{
				//		status = "error",
				//		message = "Session invalid or expired !"
				//	});
				//}
			}
			catch (Exception ex)
			{
				//log.Error("Error", ex);

				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}


	}
}
