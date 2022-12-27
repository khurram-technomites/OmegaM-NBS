using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.Vendor.Property
{
    public class PropertyFeatureViewModel
    {
        public long FeatureID { get; set; }
    }
    public  class NearByPlaceViewModel
    {
        public long ID { get; set; }
        public Nullable<long> PropertyID { get; set; }
        public Nullable<long> NearByPlacesCategoryID { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Distance { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
    }

    public class PropertyViewModel
    {
        public long? ID { get; set; }
        [Required(ErrorMessage = "Category is required")]
        public long CategoryID { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Title (ar) is required")]
        public string TitleAr { get; set; }
        public double OldPrice { get; set; }
        [Required(ErrorMessage = "Price is required")]
        public double Price { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public Nullable<long> CountryID { get; set; }
        public long CityID { get; set; }
        public string Area { get; set; }
        public string State { get; set; }
        public string Address { get; set; }
        public List<PropertyFeatureViewModel> PropertyFeatures { get; set; }
        public List<NearByPlaceViewModel> NearByPlace { get; set; }
        public int BuildYear { get; set; }
        public int Size { get; set; }
        public string NoOfRooms { get; set; }
        public string NoOfDinings { get; set; }
        public string NoOfBaths { get; set; }
        public string NoOfLaundry { get; set; }
        public string NoOfGarage { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool IsFurnished { get; set; }

        public string ZipCode { get; set; }
    }


}