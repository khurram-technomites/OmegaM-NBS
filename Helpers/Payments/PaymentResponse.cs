using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.Helpers.Payments
{
	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
	public class CnpPaymentLink
	{
		public string href { get; set; }
	}

	public class PaymentAuthorization
	{
		public string href { get; set; }
	}

	public class Self
	{
		public string href { get; set; }
	}

	public class TenantBrand
	{
		public string href { get; set; }
	}

	public class Payment
	{
		public string href { get; set; }
		public string _id { get; set; }
		public Links _links { get; set; }
		public string state { get; set; }
		public Amount amount { get; set; }
		public DateTime updateDateTime { get; set; }
		public string outletId { get; set; }
		public string orderReference { get; set; }
	}

	public class MerchantBrand
	{
		public string href { get; set; }
	}

	public class Links
	{
		[JsonProperty("cnp:payment-link")]
		public CnpPaymentLink CnpPaymentLink { get; set; }

		[JsonProperty("payment-authorization")]
		public PaymentAuthorization PaymentAuthorization { get; set; }
		public Self self { get; set; }

		[JsonProperty("tenant-brand")]
		public TenantBrand TenantBrand { get; set; }
		public Payment payment { get; set; }

		[JsonProperty("merchant-brand")]
		public MerchantBrand MerchantBrand { get; set; }

		[JsonProperty("payment:card")]
		public PaymentCard PaymentCard { get; set; }
		public List<Cury> curies { get; set; }
	}

	public class MerchantDefinedData
	{
	}


	public class PaymentMethods
	{
		public List<string> card { get; set; }
	}

	public class FormattedOrderSummary
	{
	}

	public class PaymentCard
	{
		public string href { get; set; }
	}

	public class Cury
	{
		public string name { get; set; }
		public string href { get; set; }
		public bool templated { get; set; }
	}

	public class Embedded
	{
		public List<Payment> payment { get; set; }
	}

	public class PaymentResponse
	{
		public string _id { get; set; }
		public Links _links { get; set; }
		public string type { get; set; }
		public MerchantDefinedData merchantDefinedData { get; set; }
		public string action { get; set; }
		public Amount amount { get; set; }
		public string language { get; set; }
		public MerchantAttributes merchantAttributes { get; set; }
		public string reference { get; set; }
		public string outletId { get; set; }
		public DateTime createDateTime { get; set; }
		public PaymentMethods paymentMethods { get; set; }
		public string referrer { get; set; }
		public string formattedAmount { get; set; }
		public FormattedOrderSummary formattedOrderSummary { get; set; }
		public Embedded _embedded { get; set; }
	}


}