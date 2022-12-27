using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Account
{
	public class LogoutViewModel
	{
		[Required]
		public string DeviceID { get; set; }
	}
}