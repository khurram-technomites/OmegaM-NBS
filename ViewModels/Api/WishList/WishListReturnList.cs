using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.WishList
{
    public class WishListReturnList
    {
        public long wishlistId { get; set; }
        public long ProductID { get; set; }
        public string Thumbnail { get; set; }
        public string Address { get; set; }
        public double oldPrice { get; set; }
        public double price { get; set; }
        public string Title { get; set; }
        public string rooms { get; set; }
        public string baths { get; set; }
        public string NoOfGarage { get; set; }
        public string Size { get; set; }
        public string Transmission { get; set; }
        public string NoOfDoors { get; set; }
        public string HorsePower { get; set; }
        public string Cylinders { get; set; }

        public string Status { get; set; }
        public string VendorThumbnail { get; set; }
        public string Wheels { get; set; }
        public string Door { get; set; }
        public long? CityID { get; set; }
        public long? CountryID { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public string Garage { get; set; }
        public string EngineDisplacement { get; set; }
        public bool IsSold { get; set; }
        public bool IsVerified { get; set; }
        public bool IsPremium { get; set; }
        public long VendorID { get; set; }
        public string VendorName { get; set; }
        public string Mileage { get; set; }
        public List<string> Images { get; set; } = new List<string>();
    }
}