using System;

namespace NowBuySell.Web.ViewModels.Api.Order
{
	public class OrderFilterViewModel
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		
		public string Status { get; set; }
		public int? PageNumber { get; set; }
		public int? SortBy { get; set; }
	}
}