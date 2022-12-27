using System;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.CustomRequest
{
    public class CarRequestViewModel
    {

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Category is required")]
        public long CategoryID { get; set; }

        public Nullable<long> MakeID { get; set; }
        public Nullable<long> ModelID { get; set; }
        public string Color { get; set; }
        public string Doors { get; set; }
        public string Cylinders { get; set; }
        public string Transmission { get; set; }

        public string MinYear { get; set; }
        public string MaxYear { get; set; }

        public Nullable<double> MinKilometers { get; set; }
        public Nullable<double> MaxKilometers { get; set; }

        public Nullable<decimal> MinPrice { get; set; }
        public Nullable<decimal> MaxPrice { get; set; }

        public string RegionalSpecification { get; set; }
        public Nullable<bool> Warranty { get; set; }
    }
}