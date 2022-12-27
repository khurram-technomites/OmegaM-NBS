using System;

namespace NowBuySell.Web.ViewModels.NewsFeed
{
	public class FilterViewModel
	{
		public string search { get; set; }
		public Nullable<int> pageSize { get; set; }
		public Nullable<int> pageNumber { get; set; }
		public Nullable<int> sortBy { get; set; }
	}
}