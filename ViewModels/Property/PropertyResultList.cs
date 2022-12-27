using NowBuySell.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Property
{
    public class PropertyResultList
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string Rooms { get; set; }
        public string Baths { get; set; }
        public string Garages { get; set; }
        public string Dinings { get; set; }
        public string Laundries { get; set; }
        public string Size { get; set; }
        public double Price { get; set; }
        public double OldPrice { get; set; }
        public string Thumbnail { get; set; }
        public string VendorThumbnail { get; set; }
        public string VendorName { get; set; }
        public string VendorContact { get; set; }
        public string VendorMobile { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsFurnished { get; set; }
        public string AdPostedDate { get; set; }
        public string LastUpdationDate { get; set; }
        public string AdsReferenceCode { get; set; }
        public int? RERANo { get; set; }
        public int? DEDNo { get; set; }
        public int? PermitNo { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Area { get; set; }
        public string Country { get; set; }
        public string Zipcode { get; set; }
        public string State { get; set; }
        public int BuildYear { get; set; }
        public string Slug { get; set; }
        public string VendorFacebook { get; set; }
        public string VendorTwitter { get; set; }
        public string VendorInstagram { get; set; }
        public string VendorLinkedin { get; set; }
        public string VendorWhatsapp { get; set; }
        public string VendorYoutube { get; set; }
        public string VendorSnapchat { get; set; }

        public string VendorTikTok { get; set; }
        public long VendorID { get; set; }
        public long WishlistId { get; set; }
        public string Video { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string PropertyType { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsPremium { get; set; }
        public bool? IsSold { get; set; }
        public bool ForSale { get; set; }
        public long CountryId { get; set; }
        public long CityId { get; set; }
        public long CategoryId { get; set; }
        public List<Featues> Features { get; set; } = new List<Featues>();
        public List<GetPropetyAvgPriceByArea_Result> TrendPrice { get; set; }
        public List<NearByPlaces> NearByPlaces { get; set; } = new List<NearByPlaces>();
        public List<string> Images { get; set; } = new List<string>();
        public List<string> FloorPlans { get; set; } = new List<string>();
        public string ListingType { get; set; }
        public string SoldDate { get; set; }
        public string propertyInspectionUrl { get; set; }
        public string propertyInspectionName { get; set; }
        public double AnnualPrice { get; set; }
        
    }

    public class Featues
    {
        public string Name { get; set; }
        public string Icon { get; set; }
    }
    public class NearByPlaces
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string CategoryName { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Distance { get; set; }
    }
    public class VendorSocialLinks
    {
        public string Facebook { get; set; }
        public string Instagram { get; set; }
        public string Snapchat { get; set; }
        public string LinkedIn { get; set; }
        public string Twitter { get; set; }
        public string Youtube { get; set; }
        public string Whatsapp { get; set; }
        public string TikTok { get; set; }
    }
}