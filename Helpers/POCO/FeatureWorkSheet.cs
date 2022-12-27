using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.Helpers.POCO
{
    public class FeatureWorkSheet
    {
        [Required]
        public string Feature { get; set; }

        [Required]
        public string FeatureAr { get; set; }
    }
}