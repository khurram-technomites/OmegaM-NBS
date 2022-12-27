using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Account
{
	public class RegisterViewModel
	{
		[Display(Name = "First Name")]
		[Required(ErrorMessage = "The First Name is required")]
		public string FirstName { get; set; }


		[Display(Name = "Last Name")]
		[Required(ErrorMessage = "The Last Name is required")]
		public string LastName { get; set; }

		[Display(Name = "Email address")]
		[Required(ErrorMessage = "The Email Address is required")]
		[EmailAddress(ErrorMessage = "Invalid Email Address")]
		public string EmailAddress { get; set; }

		[Display(Name = "Password")]
		[Required(ErrorMessage = "The Password is required")]
		public string Password { get; set; }

		[Required(ErrorMessage = "The Confirm Password is required")]
		[Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again !")]
		public string ConfirmPassword { get; set; }

		[Display(Name = "Account Type")]
		[Required(ErrorMessage = "The Account Type is required")]
		public string AccountType { get; set; }
	}
}