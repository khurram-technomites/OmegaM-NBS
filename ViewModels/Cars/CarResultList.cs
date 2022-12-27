using NowBuySell.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Cars
{
    public class CarResultList
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public int Brand { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public long VendorID { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public string Thumbnail { get; set; }
        public string VendorThumbnail { get; set; }
        public string VendorName { get; set; }
        public string VendorMobile { get; set; }
        public string VendorContact { get; set; }
        public bool IsFeatured { get; set; }
        public string BodyType { get; set; }
        public string SteeringSide { get; set; }
        public string MechanicalCondition { get; set; }
        public bool ServiceHistory { get; set; }
        public string RegionalSpecification { get; set; }
        public string Mileage { get; set; }
        public bool Warranty { get; set; }
        public string AdsReferenceCode { get; set; }

        public int? RERANo { get; set; }
        public int? DEDNo { get; set; }
        public int? PermitNo {get;set;}
        public string Color { get; set; }
        public string LicensePlate { get; set; }
        public string Year { get; set; }
        public string Country { get; set; }
        public string Door { get; set; }
        public string Cylinder { get; set; }
        public string HorsePower { get; set; }
        public string Slug { get; set; }
        public string Capacity { get; set; }
        public string Transmission { get; set; }
        public long WishlistId { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Area { get; set; }
        public string Condition { get; set; }
        public string FuelType { get; set; }
        public string WarrentyEndDate { get; set; }
        public string Garage { get; set; }
        public string GarageSize { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Wheels { get; set; }
        public string EngineDisplacement { get; set; }
        public string Video { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public int CountryId { get; set; }
        public int CityId { get; set; }
        public int CategoryId { get; set; }
        public string AdPostedDate { get; set; }
        public string LastUpdationDate { get; set; }
        public string VendorFacebook { get; set; }
        public string VendorTwitter { get; set; }
        public string VendorInstagram { get; set; }
        public string VendorLinkedin { get; set; }
        public string VendorWhatsapp { get; set; }
        public string VendorYoutube { get; set; }
        public string VendorSnapchat{ get; set; }
        public string VendorTikTok { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsPremium { get; set; }
        public bool? IsSold { get; set; }
        public List<Featues> Features { get; set; } = new List<Featues>();
        public List<string> Images { get; set; } = new List<string>();
        public List<CarImages> CarImagespath { get; set; }
        public string ListingType { get; set; }

        public string SoldDate { get; set; }
        public string carInspection { get; set; }
        public string carInspectionName { get; set; }
    }
    public class Featues
    {
        public string Name { get; set; }
        public string Icon { get; set; }
    }
    public class CarImages
    {
        public long imageID { get; set; }
        public string imagePath { get; set; }
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