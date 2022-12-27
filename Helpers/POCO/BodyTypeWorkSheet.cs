using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.Helpers.POCO
{
    public class BodyTypeWorkSheet
    {
        [Required]
        public string BodyType { get; set; }

        [Required]
        public string BodyTypeAr { get; set; }
    }
}