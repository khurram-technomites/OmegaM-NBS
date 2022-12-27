using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.Helpers.POCO
{
    public class CarMakeSheet
    {
        [Required]
        public string Make { get; set; }

        [Required]
        public string MakeAr { get; set; }
        
        public string Image { get; set; }
    }
}