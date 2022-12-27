using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.Account
{
    public class ChangePasswordViewModel
    {

		[Required(ErrorMessage = "The old Password is required")]
		public string CurrentPassword { get; set; }

		[Required(ErrorMessage = "The New Password is required")]
		public string NewPassword { get; set; }

	}
}