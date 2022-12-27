using System;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.Helpers.POCO
{
	public class CarWorkSheet
	{
		//[Required]
		public string ChasisNo { get; set; }
		public string SKU { get; set; }
		public string Thumbnail { get; set; }
		public string Slug { get; set; }
		public string Name { get; set; }
		public string NameAr { get; set; }
		public string Specification { get; set; }
		public string SpecificationAr { get; set; }
		public string Category { get; set; }
		public string Make { get; set; }
		public string BodyType { get; set; }
		public string Model { get; set; }
		public string IsTaxInclusive { get; set; }
		public string IsRecommended { get; set; }
		public string InsuranceName { get; set; }
		public string InsuranceNameAr { get; set; }
		public string InsuranceDetails { get; set; }
		public string InsuranceDetailsAr { get; set; }
		public string IsFeatured { get; set; }
		public string Features { get; set; }
		public string Images { get; set; }
		public string Doors { get; set; }
		public string Cylinders { get; set; }
		public string HorsePower { get; set; }
		public string RegionalSpecification { get; set; }
		public string Year { get; set; }
		public string Capacity { get; set; }
		public string Transmission { get; set; }
		public string KilometersDriven { get; set; }
		public string RegularPrice { get; set; }
		public string Price { get; set; }
		public string Description { get; set; }
		public string DescriptionAr { get; set; }
		public string AdsReferenceCode { get; set; }
		public string ServiceHistory { get; set; }
		public string MechanicalCondition { get; set; }
		public string Warranty { get; set; }
		public string SteeringSide { get; set; }
		public string Wheels { get; set; }
		public string EngineCC { get; set; }
		public string BodyCondition { get; set; }
		public string Address { get; set; }
		public string FuelType { get; set; }
		public string Documents { get; set; }
		public string Video { get; set; }
		public string Area { get; set; }
		public string Latitude { get; set; }
		public string Longitude { get; set; }
		public string City { get; set; }

	}
}