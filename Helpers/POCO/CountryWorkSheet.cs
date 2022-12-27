using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.Helpers.POCO
{
    public class CountryWorkSheet
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string NameAr { get; set; }
    }
}