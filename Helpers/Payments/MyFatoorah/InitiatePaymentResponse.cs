using System.Collections.Generic;

namespace NowBuySell.Web.Helpers.Payments.MyFatoorah.Initiate
{
	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
	public class PaymentMethod
	{
		public int PaymentMethodId { get; set; }
		public string PaymentMethodAr { get; set; }
		public string PaymentMethodEn { get; set; }
		public string PaymentMethodCode { get; set; }
		public bool IsDirectPayment { get; set; }
		public double ServiceCharge { get; set; }
		public double TotalAmount { get; set; }
		public string CurrencyIso { get; set; }
		public string ImageUrl { get; set; }
	}

	public class Data
	{
		public List<PaymentMethod> PaymentMethods { get; set; }
	}

	public class InitiatePaymentResponse
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public object ValidationErrors { get; set; }
		public Data Data { get; set; }
	}
	
}