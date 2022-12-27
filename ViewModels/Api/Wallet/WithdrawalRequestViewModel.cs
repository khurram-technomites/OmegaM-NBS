using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.Wallet
{
    public class WithdrawalRequestViewModel
    {
        [Required]
        public Nullable<decimal> Amount { get; set; }

        [Required]
        public string Description { get; set; }
     
    }
}