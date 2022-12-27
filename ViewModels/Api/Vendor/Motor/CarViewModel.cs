using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.Vendor.Motor
{

    public class CarFeatureViewModel
    {
        public int FeatureID { get; set; }
    }

    public class CarViewModel
    {
        [Required(ErrorMessage = "Category is required")]
        public int CategoryID { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Name (ar) is required")]
        public string NameAr { get; set; }
        public double SellerTransactionFee { get; set; }
        public double BuyerTransactionFee { get; set; }
        public int CarMakeID { get; set; }
        public int CarModelID { get; set; }
        [Required(ErrorMessage = "Regular price is required")]
        public int RegularPrice { get; set; }
        public int SalePrice { get; set; }
        public string LongDescription { get; set; }
        public string LongDescriptionAr { get; set; }
        public List<CarFeatureViewModel> CarFeatures { get; set; }
        
        public string SKU { get; set; }
       
        public string LicensePlate { get; set; }
        public int BodyTypeID { get; set; }
        public string Year { get; set; }
        public string Doors { get; set; }
        public string Cylinders { get; set; }
        public string HorsePower { get; set; }
        public string Capacity { get; set; }
        public string RegionalSpecification { get; set; }
        public string Transmission { get; set; }
        public string FuelEconomy { get; set; }
        public string ChesisNumber { get; set; }
        public int CountryID { get; set; }
        public int CityID { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Area { get; set; }
        public string Zipcode { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public string Condition { get; set; }
        public string MechanicalCondition { get; set; }
        public string FuelType { get; set; }
        public string EngineDisplacementVolumes { get; set; }
        public string Wheels { get; set; }
        public string SteeringSide { get; set; }
        public bool ServiceHistory { get; set; }
        public bool Warranty { get; set; }
    }


}