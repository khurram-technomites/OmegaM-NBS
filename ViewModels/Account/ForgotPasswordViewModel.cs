
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Account
{
	public class ForgotPasswordViewModel
	{
		[Display(Name = "Email address")]
		[Required(ErrorMessage = "The email address is required")]
		[EmailAddress(ErrorMessage = "Invalid Email Address")]
		public string EmailAddress { get; set; }
	}
}