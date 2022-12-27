using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.Wallet
{
    public class VendorWalletShareHistoryViewModel
    {
        [Required]
        public int pgno { get; set; }

        public Nullable<DateTime> startDate { get; set; }

        public Nullable<DateTime> endDate{ get; set; }
        
    }
}