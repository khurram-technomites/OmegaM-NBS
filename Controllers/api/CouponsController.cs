using NowBuySell.Service;
using NowBuySell.Web.ViewModels.Api.Coupon;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
	[RoutePrefix("api/v1")]
	public class CouponsController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICouponService _couponService;
		private readonly ICouponRedemptionService _couponRedemptionService;
		private readonly ICustomerCouponsService _customerCouponsService;

		public CouponsController(ICouponService couponService, ICouponRedemptionService couponRedemptionService, ICustomerCouponsService customerCouponsService)
		{
			this._couponService = couponService;
			this._couponRedemptionService = couponRedemptionService;
			this._customerCouponsService = customerCouponsService;
		}

		[HttpPost]
		[Route("coupons/redeem")]
		public HttpResponseMessage CouponRedemption(CouponRedemptionViewModel couponRedemptionViewModel)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					string message = string.Empty;
					if (ModelState.IsValid)
					{
						var coupon = _couponService.GetCoupon(couponRedemptionViewModel.CouponCode);

						if (coupon != null)
						{
							if (coupon.Expiry >= Helpers.TimeZone.GetLocalDateTime())
							{
								if (!(coupon.IsOpenToAll.HasValue && coupon.IsOpenToAll.Value))
								{
									var customercoupon = _customerCouponsService.GetCoupon(customerId, coupon.ID);
									if (customercoupon == null)
									{
										return Request.CreateResponse(HttpStatusCode.OK, new
										{
											status = "error",
											message = "Invalid coupon code !"
										});
									}
								}
								var couponRedemptions = _couponRedemptionService.GetCouponRedemptions(coupon.ID, customerId);
								if (couponRedemptions.Count() <= coupon.Frequency)
								{
									return Request.CreateResponse(HttpStatusCode.OK, new
									{
										status = "success",
										message = "Coupon Code is valid !",
										coupon.Type,
										coupon.Value,
										coupon.MaxAmount,
										categories = coupon.CouponCategories.Select(i => i.CategoryID).ToArray()
									});
								}
								else
								{
									return Request.CreateResponse(HttpStatusCode.OK, new
									{
										status = "error",
										message = "Coupon already used !"
									});
								}
							}
							else
							{
								return Request.CreateResponse(HttpStatusCode.OK, new
								{
									status = "error",
									message = "Coupon expired !"
								});
							}
						}
						else
						{
							return Request.CreateResponse(HttpStatusCode.OK, new
							{
								status = "error",
								message = "Invalid coupon code !"
							});
						}
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.BadRequest, new
						{
							status = "error",
							message = "Bad request !",
							description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
						});
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
