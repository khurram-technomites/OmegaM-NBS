using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Vendor
{
	public class VendorSignupFormViewModel
	{
		[Required]
		public string VendorName { get; set; }
		[Required]
		public string VendorMobile { get; set; }
		[Required]
		public string VendorEmail { get; set; }
		[Required]
		public string VendorPassword { get; set; }
		[Required]
		public string VendorLogo { get; set; }
	}
}