using NowBuySell.Data;
using NowBuySell.Data.Infrastructure;
using NowBuySell.Data.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NowBuySell.Web.Helpers.POCO
{
    public class CarModelWorkSheet
    {
        [Required]
        public string CarModel { get; set; }
        [Required]
        public string CarMake { get; set; }
        [Required]
        public string CarModelAr { get; set; }
        
    }
}