using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Account
{
	public class LoginViewModel
	{
		[Display(Name = "Email address")]
		[Required(ErrorMessage = "The email address is required")]
		[EmailAddress(ErrorMessage = "Invalid Email Address")]
		public string EmailAddress { get; set; }

		[Required(ErrorMessage = "The Password is required")]
		[Display(Name = "Password")]
		public string Password { get; set; }
	}
}