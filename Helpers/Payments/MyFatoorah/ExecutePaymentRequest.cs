using System;
using System.Collections.Generic;

namespace NowBuySell.Web.Helpers.Payments.MyFatoorah
{
	public class CustomerAddress
	{
		public string Street { get; set; }
	}

	public class InvoiceItem
	{
		public string ItemName { get; set; }
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
	}

	public class ExecutePaymentRequest
	{
		public int PaymentMethodId { get; set; }
		public string CustomerName { get; set; }
		public string DisplayCurrencyIso { get; set; }
		public string MobileCountryCode { get; set; }
		public string CustomerMobile { get; set; }
		public string CustomerEmail { get; set; }
		public decimal InvoiceValue { get; set; }
		public string CallBackUrl { get; set; }
		public string ErrorUrl { get; set; }
		public string Language { get; set; }
		public CustomerAddress CustomerAddress { get; set; }
		public List<InvoiceItem> InvoiceItems { get; set; }
	}
}