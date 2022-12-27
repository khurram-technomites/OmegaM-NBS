using System;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Order
{
	public class OrderDetailViewModel
	{
		public long? ID { get; set; }
		public long VendorID { get; set; }
		public long CarID { get; set; }
		public long InsuranceID { get; set; }
		public long PackageID { get; set; }
		public string PackageName { get; set; }
		public decimal Price { get; set; }
		public Nullable<DateTime> StartDateTime { get; set; }
		public Nullable<DateTime> EndDateTime { get; set; }
		public decimal InsuranceAmount { get; set; }
		public decimal ExtraKilometer { get; set; }
		public decimal ExtraKilometerPrice { get; set; }
		



		[Required]
		public int[] Attributes { get; set; }
	}
}