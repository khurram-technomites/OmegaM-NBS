using System;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.Helpers.POCO
{
    public class CouponsWorkSheet
    {
		[Required]
		public string Name { get; set; }
		[Required]
		public string CouponCode { get; set; }
		[Required]
		public int Frequency { get; set; }
		[Required]
		public string Type { get; set; }
		[Required]
		public decimal Value { get; set; }

		public decimal MaxAmount { get; set; }
		[Required]
		public DateTime Expiry { get; set; }

		[Required]
		public string IsOpenToAll { get; set; }
	}
}