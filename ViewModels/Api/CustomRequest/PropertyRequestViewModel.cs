using System;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.CustomRequest
{
    public class PropertyRequestViewModel
    {

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Category is required")]
        public long CategoryID { get; set; }
        [Required(ErrorMessage = "Property Type is required")]
        public string PropertyType { get; set; }

        public Nullable<decimal> MinPrice { get; set; }
        public Nullable<decimal> MaxPrice { get; set; }
        public Nullable<double> Size { get; set; }
        public Nullable<int> NoOfRooms { get; set; }
        public Nullable<int> NoOfBathRooms { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Address { get; set; }
    }
}