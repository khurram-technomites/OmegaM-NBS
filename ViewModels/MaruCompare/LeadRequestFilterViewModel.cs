using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.MaruCompare
{
    public class LeadRequestFilterViewModel
    {
        public string Status { get; set; }
        public int PageNumber { get; set; }
        public int SortBy { get; set; }
    }
}