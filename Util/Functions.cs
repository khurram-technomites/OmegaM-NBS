using NowBuySell.Web.Hubs;
using Microsoft.AspNet.SignalR;

namespace NowBuySell.Web.Util
{
	public class Functions
	{
		public static void SendProgress(string progressMessage, int progressCount, int totalItems)
		{
			//IN ORDER TO INVOKE SIGNALR FUNCTIONALITY DIRECTLY FROM SERVER SIDE WE MUST USE THIS
			var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();

			//CALCULATING PERCENTAGE BASED ON THE PARAMETERS SENT
			var percentage = (progressCount * 100) / totalItems;

			//PUSHING DATA TO ALL CLIENTS
			hubContext.Clients.All.AddProgress(progressMessage, percentage + "%", progressCount, totalItems);
		}

		public static void SendProgress(string connectionId, string progressMessage, int progressCount, int totalItems)
		{

			//IN ORDER TO INVOKE SIGNALR FUNCTIONALITY DIRECTLY FROM SERVER SIDE WE MUST USE THIS
			var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();

			//CALCULATING PERCENTAGE BASED ON THE PARAMETERS SENT
			var percentage = (progressCount * 100) / totalItems;

			//PUSHING DATA TO ALL CLIENTS
			hubContext.Clients.Client(connectionId).AddProgress(progressMessage, percentage + "%", progressCount, totalItems);
		}

		public static void SendGalleryProgress(string connectionId, string progressMessage, int progressCount, int totalItems, string name, string path)
		{

			//IN ORDER TO INVOKE SIGNALR FUNCTIONALITY DIRECTLY FROM SERVER SIDE WE MUST USE THIS
			var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();

			//CALCULATING PERCENTAGE BASED ON THE PARAMETERS SENT
			var percentage = (progressCount * 100) / totalItems;

			//PUSHING DATA TO ALL CLIENTS
			hubContext.Clients.Client(connectionId).AddGalleryProgress(progressMessage, percentage + "%", progressCount, totalItems, name, path);
		}
	}
}