using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.Helpers.POCO
{
    public class AreaWorkSheet
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string NameAR { get; set; }

        [Required]
        public string CountryName { get; set; }
        
        [Required]
        public string CityName { get; set; }
    }
}