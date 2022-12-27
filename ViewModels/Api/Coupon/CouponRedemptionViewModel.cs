using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Coupon
{
	public class CouponRedemptionViewModel
	{
		[Required(ErrorMessage = "The Coupon Code is required")]
		public string CouponCode { get; set; }
	}
}