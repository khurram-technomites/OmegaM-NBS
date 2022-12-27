using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.CarReturn
{
	public class CarReturnViewModel
	{
		[Required]
		public long CarID { get; set; }
		[Required]
		public long OrderDetailID { get; set; }
		[Required]
		public string Reason { get; set; }
		[Required]
		public string ReturnMethod { get; set; }
	}
}