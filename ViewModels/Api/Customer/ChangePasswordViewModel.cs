using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Customer
{
	public class ChangePasswordViewModel
	{
		[Required(ErrorMessage = "The Current Password is required")]
		public string CurrentPassword { get; set; }

		[Required(ErrorMessage = "The New Password is required")]
		public string NewPassword { get; set; }

		[Required(ErrorMessage = "The Confirm Password is required")]
		[Compare("NewPassword", ErrorMessage = "Confirm password doesn't match, Type again !")]
		public string ConfirmPassword { get; set; }
	}
}