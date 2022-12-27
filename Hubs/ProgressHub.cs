using Microsoft.AspNet.SignalR;

namespace NowBuySell.Web.Hubs
{
	public class ProgressHub : Hub
	{
		public string GetConnectionId()
		{
			return this.Context.ConnectionId;
		}
	}
}