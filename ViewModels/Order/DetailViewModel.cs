using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Order
{
	public class DetailViewModel
	{
		public long? ID { get; set; }
		public long VendorID { get; set; }
		public long CarID { get; set; }
		public long? CarVaraiationID { get; set; }
		public int Quantity { get; set; }

		public string CustomNote { get; set; }

		public List<AttributesViewModel> Attributes { get; set; }
	}
}