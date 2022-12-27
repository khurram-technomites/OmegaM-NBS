using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Order
{
	public class OrderViewModel
	{

		[Required(ErrorMessage = "The DeliveryCharges is required")]
		public decimal DeliveryCharges { get; set; }

		[Required(ErrorMessage = "The TaxAmount is required")]
		public decimal TaxAmount { get; set; }

		[Required(ErrorMessage = "The TaxPercent is required")]
		public decimal TaxPercent { get; set; }

		public decimal DiscountAmount { get; set; }
		public decimal DiscountPercent { get; set; }
		public bool DocumentAtPickUp { get; set; }
		public bool SelfPickUp { get; set; }



		public string CouponCode { get; set; }
		public decimal CouponDiscount { get; set; }

		public decimal RedeemAmount { get; set; }

		[Required(ErrorMessage = "The PaymentMethod is required")]
		public string PaymentMethod { get; set; }

		public string Note { get; set; }

		public DateTime? DeliveryDate { get; set; }
		public string OrderRef { get; set; }

		[Required(ErrorMessage = "The DeliveryAddress is required")]
		public OrderDeliveryAddressViewModel DeliveryAddress { get; set; }

		[EnsureOneElement(ErrorMessage = "At least onr item is required")]
		public List<OrderDetailViewModel> OrderDetails { get; set; }

		public GuestDetailsViewModel GuestDetails { get; set; }

		
	}

	public class EnsureOneElementAttribute : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			var list = value as IList;
			if (list != null)
			{
				return list.Count > 0;
			}
			return false;
		}
	}
}