using NowBuySell.Data;
using System;
using System.Collections.Generic;

namespace NowBuySell.Web.ViewModels.Order
{
	public class OrderDetailViewModel
	{
		public IEnumerable<SP_GetOrderDetails_Result> orderdetails = new List<SP_GetOrderDetails_Result>();
		public long ID { get; set; }
		public string OrderNo { get; set; }

		public long CustomerID { get; set; }
		public string DeliveryAddress { get; set; }
		public string Status { get; set; }
		public string ShipmentStatus { get; set; }
		public string CustomerName { get; set; }
		public string Currency { get; set; }

		public string CancelationReason { get; set; }

		public decimal Amount { get; set; }
		public decimal OrderTaxAmount { get; set; }
		public decimal OrderTaxPercent { get; set; }
		public decimal Shipping { get; set; }

		public string CouponCode { get; set; }
		public decimal CouponDiscount { get; set; }

		public string CarModel { get; set; }

		public decimal RedeemAmount { get; set; }
		public decimal TotalAmount { get; set; }

		public string Vendor { get; set; }
		public string Country { get; set; }
		public string City { get; set; }
		public string Area { get; set; }
		public string Address { get; set; }
		public string Contact { get; set; }

		public Nullable<decimal> Latitude { get; set; }
		public Nullable<decimal> Longitude { get; set; }

		public decimal DiscountAmount { get; set; }
		public decimal DiscountPercent { get; set; }

		public Nullable<DateTime> DeliveryDate { get; set; }
		public Nullable<DateTime> CreatedOn { get; set; }
		public Nullable<DateTime> StartDateTime { get; set; }
		public Nullable<DateTime> EndDateTime { get; set; }
		public Decimal ExtraKilometer { get; set; }
		public Decimal ExtraKilometerPrice { get; set; }
		public Decimal InsurancePrice { get; set; }
		public string PackageName { get; set; }
		public string InsuranceName { get; set; }
		public string DeliveryMethod { get; set; }
		public decimal? DeliveryCharges { get; set; }
		public string PaymentMethod { get; set; }
		public string IsPaid { get; set; }
		public bool PaymentCaptured { get; set; }

		public long orderID { get; set; } 

		
	}
}