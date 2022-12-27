using NowBuySell.Service;
using System;
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
	public class CustomerCouponsController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICustomerCouponsService _customerCouponsService;

		public CustomerCouponsController(ICustomerCouponsService customerCouponsService)
		{
			this._customerCouponsService = customerCouponsService;
		}

		[HttpGet]
		[Route("coupons")]
		public HttpResponseMessage GetAll(int pg = 1)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					var coupons = _customerCouponsService.GetCustomerCoupons(customerId);

					return Request.CreateResponse(HttpStatusCode.OK, new
					{
						status = "success",
						coupons = coupons.Select(i => new
						{
							id = i.ID,
							name = i.Name,
							promoCode = i.CouponCode,
							i.DicountAmount,
							i.DicountPercentage,
							i.Expiry,
							i.CoverImage,
							i.CreatedOn
						})
					});
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				//Logs.Write(ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}
	}
}
