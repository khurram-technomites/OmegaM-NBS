﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Rating
{
    public class RatingFilterViewModel
    {
		public long CarID { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int SortBy { get; set; }
		public string Lang { get; set; }
	}
}