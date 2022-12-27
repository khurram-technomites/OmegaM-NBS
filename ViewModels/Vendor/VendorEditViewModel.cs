using System;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Vendor
{
	public class VendorEditViewModel
	{
		[Required]
		public long ID { get; set; }
		[Required]
		public string VendorCode { get; set; }
		[Required]
		public string Name { get; set; }
		
		public string NameAr { get; set; }
		public string Slug { get; set; }
		[Required]
		public string Email { get; set; }
		public string PassportNo { get; set; }
		public string Logo { get; set; }
		public string CoverImage { get; set; }
		public string Contact { get; set; }
		public string Mobile { get; set; }
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

		public int VendorPackageID { get; set; }

		
		public string BankName { get; set; }
		
		public string BankAccountNumber { get; set; }
		/*public string PassportNo { get; set; }*/
		public Nullable<decimal> Longitude { get; set; }
		
		public Nullable<decimal> Latitude { get; set; }
		
		[Required]
		public Nullable<long> CountryID { get; set; }
		[Required]
		public Nullable<long> CityID { get; set; }
		[Required]
		public string ClosingTime { get; set; }
		[Required]
		public string OpeningTime { get; set; }
		public string TermsAndConditionWebEn {get; set;}
		public string TermsAndConditionWebAr { get; set; }		
		public double ServingKilometer { get; set; }

		public bool? IsApproved { get; set; }
		public string Facebook { get; set; }
		public string Instagram { get; set; }
		public string Youtube { get; set; }
		public string Twitter { get; set; }
		public string Snapchat { get; set; }
		public string LinkedIn { get; set; }
		public string Whatsapp { get; set; }
		public string TikTok { get; set; }
        public Nullable<bool> IsSocialManagment { get; set; }
    }
}