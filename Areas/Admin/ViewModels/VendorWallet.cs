using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.Areas.Admin.ViewModels
{
    public class VendorWallet
    {
        public decimal ToatalEarning { get; set; }
        public decimal PendingAmount { get; set; }
        public int TransferedAmount{ get; set; }
    }
}