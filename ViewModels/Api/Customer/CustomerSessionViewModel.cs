using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Customer
{
	public class CustomerSessionViewModel
	{
		[Required]
		public string FirebaseToken { get; set; }
		[Required]
		public string DeviceID { get; set; }

		[Required]
		public string AccessToken { get; set; }
	}
}