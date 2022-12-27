using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.Helpers.Payments
{
	public class ATResponse
	{
		public string access_token { get; set; }
		public string refresh_token { get; set; }
		public int expires_in { get; set; }
		public int refresh_expires_in { get; set; }
		public string token_type { get; set; }
	}

	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
	public class Amount
	{
		public string currencyCode { get; set; }
		public Int64 value { get; set; }
	}

	public class MerchantAttributes
	{
		public string redirectUrl { get; set; }
	}

	public class BillingAddress
	{
		public string firstName { get; set; }
		public string lastName { get; set; }
		public string address1 { get; set; }
		public string city { get; set; }
		public string countryCode { get; set; }
	}

	public class ExecutePaymentRequest
	{
		public string action { get; set; }
		public Amount amount { get; set; }
		public MerchantAttributes merchantAttributes { get; set; }
		public string emailAddress { get; set; }
		public BillingAddress billingAddress { get; set; }
	}
}