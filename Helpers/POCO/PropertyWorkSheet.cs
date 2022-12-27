using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.Helpers.POCO
{
    public class PropertyWorkSheet
    {
		
		public string Slug { get; set; }
		public string Thumbnail { get; set; }
		public string Title { get; set; }
		public string TitleAr { get; set; }
		public decimal Price { get; set; }
		public decimal Size { get; set; }
		public string Type { get; set; }
		public int NoOfGarage { get; set; }
		public int NoOfRooms { get; set; }
		public int NoOfBaths { get; set; }
		public int NoOfDinings { get; set; }
		public int NoOfLanudry { get; set; }
		public int BuildYear { get; set; }
		public string Furnished { get; set; }
		public string Description { get; set; }
		public string DescriptionAr { get; set; }
		public string City { get; set; }
		public string Category { get; set; }
		public string Features { get; set; }
		public string Address { get; set; }
		public string Images { get; set; }
		public string FloorPlan { get; set; }
		public string Video { get; set; }
		public string AdsReferenceCode { get; set; }
		public string ForSale { get; set; }
		public string Latitude { get; set; }
		public string Longitude { get; set; }
		public string Area { get; set; }
	}
}