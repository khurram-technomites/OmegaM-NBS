using System;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Vendor
{
	public class VendorRegisterViewModel
	{
		[Required]
		public long ID { get; set; }
		[Required]
		public string VendorCode { get; set; }
		[Required]
		public string Name { get; set; }
		[Required]
		public string Slug { get; set; }
		[Required]
		public string Email { get; set; }
		[Required]
		public string Logo { get; set; }
		[Required]
		public string Contact { get; set; }
		[Required]
		public string Address { get; set; }
		[Required]
		public string IDNo { get; set; }
		[Required]
		public string TRN { get; set; }
		[Required]
		public string Website { get; set; }
		[Required]
		public string License { get; set; }
		[Required]
		public string FAX { get; set; }
		[Required]
		public string About { get; set; }
		[Required]
		public Nullable<long> CountryID { get; set; }
		[Required]
		public Nullable<long> CityID { get; set; }
		public Nullable<bool> IsSrilankan { get; set; }

		[Required]
		public string UserEmail { get; set; }
		[Required]
		public string UserPassword { get; set; }
	}
}