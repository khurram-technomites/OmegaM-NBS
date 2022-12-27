using System;
using System.Collections.Generic;

namespace NowBuySell.Web.ViewModels.Api.Car
{
    public class FilterViewModelTest
    {
        public string search { get; set; }
        public Nullable<long> categoryID { get; set; }
        public Nullable<long> vendorID { get; set; }
        public Nullable<long> brandID { get; set; }
        public Nullable<decimal> minPrice { get; set; }
        public Nullable<decimal> maxPrice { get; set; }
        public List<FilterAttributesViewModel> attributes { get; set; }
        public Nullable<int> pageSize { get; set; }
        public Nullable<int> pageNumber { get; set; }
        public Nullable<int> sortBy { get; set; }
        public Nullable<bool> isFeatured { get; set; }
        public Nullable<bool> isSale { get; set; }
        public Nullable<bool> isRecommended { get; set; }
    }

    public class FilterViewModel
	{
		public List<string> categories { get; set; }
		public Nullable<decimal> latitude { get; set; }
		public Nullable<decimal> longitude { get; set; }
		public Nullable<System.DateTime> startDate { get; set; }
		public Nullable<System.DateTime> endDate { get; set; }
		public Nullable<long> packageID { get; set; }
		public List<string> makes { get; set; }
		public string year { get; set; }
		public Nullable<decimal> minPrice { get; set; }
		public Nullable<decimal> maxPrice { get; set; }
		public Nullable<double> rating { get; set; }
		public List<string> features { get; set; }
		public Nullable<bool> isFeatured { get; set; }
		public Nullable<long> vendorID { get; set; }
		public string search { get; set; }
		public Nullable<int> pageSize { get; set; }
		public Nullable<int> pageNumber { get; set; }
		public Nullable<int> sortBy { get; set; }
		public string lang { get; set; }
		public Nullable<long> modelID { get; set; }
		public Nullable<long> typeID { get; set; }
		public Nullable<bool> isSale { get; set; }
	}

	public class FilterAttributesViewModel
	{
		public Nullable<long> attributeID { get; set; }
		public List<string> values { get; set; }
	}

    //public class FilterBrandViewModel
    //{
    //    public string BrandID { get; set; }

    //}

    //public class FilterFeaturesViewModel
    //{
    //    public string BrandID { get; set; }

    //}
}