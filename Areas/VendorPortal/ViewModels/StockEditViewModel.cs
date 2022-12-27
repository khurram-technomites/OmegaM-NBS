using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.Areas.VendorPortal.ViewModels
{
	public class StockEditViewModel
	{
		public long ID { get; set; }
		public string Name{ get; set; }
		public decimal RegularPrice{ get; set; }
		public decimal SalePrice { get; set; }
		public int Stock { get; set; }
	}
}