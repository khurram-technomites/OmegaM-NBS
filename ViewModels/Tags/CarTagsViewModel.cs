using NowBuySell.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Tags
{
    public class CarTagsViewModel
    {
        public long ID { get; set; }
        public bool IsPublished { get; set; }
        public bool IsRecommended { get; set; }
        public bool IsTaxInclusive { get; set; }
        public bool IsFeatured { get; set; }

        public List<CarTag> tag { get; set; }
    }
}