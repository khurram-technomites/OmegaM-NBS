using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Account
{
	public class LoginContactViewModel
	{
		[Display(Name = "Mobile Number")]
		[Required(ErrorMessage = "The Mobile Number is required")]
		public string Contact { get; set; }

		[Required(ErrorMessage = "The Password is required")]
		[Display(Name = "Password")]
		public string Password { get; set; }
	}
}