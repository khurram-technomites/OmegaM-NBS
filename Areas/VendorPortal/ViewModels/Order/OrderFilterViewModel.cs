using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.Areas.VendorPortal.ViewModels.Order
{
	public class OrderFilterViewModel
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public long VendorId { get; set; }
		public string Status { get; set; }
		public int? PageNumber { get; set; }
		public int? SortBy { get; set; }
	}
}