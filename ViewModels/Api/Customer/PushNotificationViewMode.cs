using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Customer
{
	public class PushNotificationViewMode
	{
	
		public bool AllowPushNotification { get; set; }

		public bool AllowBookingNotification { get; set; }
	}
}