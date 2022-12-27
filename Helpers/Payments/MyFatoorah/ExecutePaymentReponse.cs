using System.Collections.Generic;

namespace NowBuySell.Web.Helpers.Payments.MyFatoorah
{

	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
	public class ValidationError
	{
		public string Name { get; set; }
		public string Error { get; set; }
	}

	public class Data
	{
		public int InvoiceId { get; set; }
		public bool IsDirectPayment { get; set; }
		public string PaymentURL { get; set; }
		public string CustomerReference { get; set; }
		public string UserDefinedField { get; set; }
		public string RecurringId { get; set; }
	}

	public class ExecutePaymentReponse
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public List<ValidationError> ValidationErrors { get; set; }
		public Data Data { get; set; }
	}


}