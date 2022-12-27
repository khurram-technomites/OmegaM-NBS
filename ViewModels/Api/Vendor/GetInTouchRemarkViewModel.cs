using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.Vendor
{
    public class GetInTouchRemarkViewModel
    {
        public long ID { get; set; }
        public Nullable<long> VendorUserID { get; set; }
        public Nullable<int> GetInTouchID { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }

        [Required(ErrorMessage = "Remarks is required")]
        public string Remarks { get; set; }

    }
}