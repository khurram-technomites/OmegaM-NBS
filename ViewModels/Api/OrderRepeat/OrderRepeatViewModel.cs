using System;
using System.Collections.Generic;

namespace NowBuySell.Web.ViewModels.Api.OrderRepeat
{
	public class OrderRepeatViewModel
	{
		public long ID { get; set; }
		public string SKU { get; set; }
		public string Title { get; set; }
		public string Type { get; set; }
		public decimal? RegularPrice { get; set; }
		public decimal? SalePrice { get; set; }
		public DateTime? SalePriceFrom { get; set; }
		public DateTime? SalePriceTo { get; set; }
		public bool? IsManageStock { get; set; }
		public int Stock { get; set; }
		public bool? IsSaleAvailable { get; set; }

		public Nullable<bool> IsSoldIndividually { get; set; }
		public string Thumbnail { get; set; }
		public int? StockStatus { get; set; }
		public List<OrderRepeatAttributesViewModel> attributes { get; set; }
		public List<OrderRepeatVariationsViewModel> variations { get; set; }
		public OrderRepeatVendorViewModel vendor { get; set; }
	}

	public class OrderRepeatAttributesViewModel
	{
		public long? ID { get; set; }
		public string Name { get; set; }
		public string[] Options { get; set; }
	}

	public class OrderRepeatVariationsViewModel
	{
		public long ID { get; set; }
		public string SKU { get; set; }
		public string Thumbnail { get; set; }
		public decimal? RegularPrice { get; set; }
		public decimal? SalePrice { get; set; }
		public DateTime? SalePriceFrom { get; set; }
		public DateTime? SalePriceTo { get; set; }
		public bool? IsSaleAvailable { get; set; }
		public bool? IsManageStock { get; set; }
		public int? Stock { get; set; }
		public string StockStatus { get; set; }
		public bool? SoldIndividually { get; set; }
		public string[] attributes { get; set; }

	}

	public class OrderRepeatVendorViewModel
	{
		public long? ID { get; set; }
	}
}