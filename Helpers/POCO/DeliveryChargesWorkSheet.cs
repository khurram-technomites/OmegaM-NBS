using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.Helpers.POCO
{
    public class DeliveryChargesWorkSheet
    {
        [Required]
        public string AreaName { get; set; }
        [Required]
        public decimal MinOrder { get; set; }
        [Required]
        public decimal Charges { get; set; }
    }
}