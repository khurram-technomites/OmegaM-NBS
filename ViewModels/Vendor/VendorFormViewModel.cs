using System;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Vendor
{
	public class VendorFormViewModel
	{
		//[Required]
		public long ID { get; set; }
		[Required]
		public string VendorCode { get; set; }
		[Required]
		public string Name { get; set; }
		public string NameAr { get; set; }
		[Required]
		public string Slug { get; set; }
		[Required]
		public string Email { get; set; }
		//[Required]
		public string Logo { get; set; }

		public string PassportNo { get; set; }
        //[Required]
        //public string CoverImage { get; set; }
        [Required]
		public string Contact { get; set; }
		[Required]
		public string Mobile { get; set; }
		[Required]
		public string Address { get; set; }
		[Required]
		public string IDNo { get; set; }
		[Required]
		public string TRN { get; set; }
		[Required]
		public string Website { get; set; }
		public decimal Commission { get; set; }
		[Required]
		public string License { get; set; }
		public string FAX { get; set; }

		public int? RERANo { get; set; }
		public int? DEDNo { get; set; }

		public int? PermitNo { get; set; }


		public string About { get; set; }

		public string AboutAr { get; set; }
		[Required]
		public Nullable<long> CountryID { get; set; }
		[Required]
		public Nullable<long> CityID { get; set; }
        [Required]
        public string UserEmail { get; set; }
        [Required]
        public string UserPassword { get; set; }
		[Required]
        public int VendorPackageID { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public String OpeningTime { get; set; }
        public String ClosingTime { get; set; }
		public string TermsAndConditionWebEn { get; set; }
		public string TermsAndConditionWebAr { get; set; }
		public string Facebook { get; set; }
		public string Instagram { get; set; }
		public string Youtube { get; set; }
		public string Twitter { get; set; }
		public string Snapchat { get; set; }
		public string LinkedIn { get; set; }
		public string Whatsapp { get; set; }
		public string TikTok { get; set; }
		public double ServingKilometer { get; set; }
        public Nullable<bool> IsSocialManagment { get; set; }


    }
}