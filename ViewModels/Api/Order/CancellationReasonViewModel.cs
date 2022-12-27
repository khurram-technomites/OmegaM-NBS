using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Order
{
	public class CancellationReasonViewModel
	{
		[Required]
		public string Reason { get; set; }
	}
}