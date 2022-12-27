using NowBuySell.Web.ViewModels.Api.Order;
using System;
using System.ComponentModel.DataAnnotations;


namespace NowBuySell.Web.ViewModels.Order
{
    public class OrderViewModel
    {
        public long VendorID { get; set; }
        public long CarID { get; set; }
        public long PackageID { get; set; }

        public DateTime StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan EndTime { get; set; }

        public decimal? ExtraKilometers { get; set; }
        public decimal? ExtraKilometersPrice { get; set; }

        public bool? DocumentAtPickUp { get; set; }
        public bool? SelfPickUp { get; set; }

        public decimal? DeliveryCharges { get; set; }

        public string CouponCode { get; set; }
        public decimal? CouponDiscount { get; set; }

        [Required(ErrorMessage = "The PaymentMethod is required")]
        public string PaymentMethod { get; set; }

        [Required(ErrorMessage = "The DeliveryAddress is required")]
        public OrderDeliveryAddressViewModel DeliveryAddress { get; set; }

    }
}