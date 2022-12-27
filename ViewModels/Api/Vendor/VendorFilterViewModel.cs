using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Vendor
{
    public class VendorFilterViewModel
    {
        [Required]
        public int pgno { get; set; }
        public Nullable<int> PageSize { get; set; }
        public string Search { get; set; }
        public string VendorType { get; set; }
    }
}