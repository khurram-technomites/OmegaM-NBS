using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Account
{
    public class VerifyOTPViewModel
    {
        [Required(ErrorMessage = "The contact number is required")]
        public string Contact { get; set; }

        [Required(ErrorMessage = "The otp code is required")]
        public int otp { get; set; }
    }
}