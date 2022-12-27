using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.Vendor
{
    public class VendorProfileEditViewModel
    {
			[Required]
			public long ID { get; set; }
			[Required]
			public string VendorCode { get; set; }
			[Required]
			public string Name { get; set; }
			[Required]
			public string NameAr { get; set; }
			[Required]
			public string Slug { get; set; }
			[Required]
			public string Email { get; set; }
			public string Logo { get; set; }
			public string CoverImage { get; set; }
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
			[Required]
			public decimal Commission { get; set; }
			[Required]
			public string License { get; set; }

			public Nullable<decimal> Longitude { get; set; }

			public Nullable<decimal> Latitude { get; set; }
			[Required]
			public string FAX { get; set; }
			[Required]
			public string About { get; set; }
			[Required]
			public string AboutAr { get; set; }
			[Required]
			public Nullable<long> CountryID { get; set; }
			[Required]
			public Nullable<long> CityID { get; set; }
			public string ClosingTime { get; set; }
			public string OpeningTime { get; set; }
			public string TermAndConditionWebEn { get; set; }
			public string TermAndConditionWebAr { get; set; }
			public double ServingKilometer { get; set; }
		
	}
}