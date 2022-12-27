using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NowBuySell.Web.AuthorizationProvider.OAuth
{
	public class ApplicationRefreshTokenProvider : IAuthenticationTokenProvider
	{
		private static ConcurrentDictionary<string, AuthenticationTicket> _refreshTokens = new ConcurrentDictionary<string, AuthenticationTicket>();

		public async Task CreateAsync(AuthenticationTokenCreateContext context)
		{
			var guid = Guid.NewGuid().ToString();

			//copy properties and set the desired lifetime of refresh token
			var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
			{
				IssuedUtc = context.Ticket.Properties.IssuedUtc,
				ExpiresUtc = DateTime.UtcNow.AddHours(12)

				//SET DATETIME to 5 Minutes
				//ExpiresUtc = DateTime.UtcNow.AddMonths(3) 
			};

			/*CREATE A NEW TICKET WITH EXPIRATION TIME OF 5 MINUTES 
			 *INCLUDING THE VALUES OF THE CONTEXT TICKET: SO ALL WE 
			 *DO HERE IS TO ADD THE PROPERTIES IssuedUtc and 
			 *ExpiredUtc to the TICKET*/
			var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties);

			//saving the new refreshTokenTicket to a local var of Type ConcurrentDictionary<string,AuthenticationTicket>
			// consider storing only the hash of the handle
			_refreshTokens.TryAdd(guid, refreshTokenTicket);
			context.SetToken(guid);
		}


		public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
		{
			AuthenticationTicket ticket;

			if (_refreshTokens.TryRemove(context.Token, out ticket))
			{
				context.SetTicket(ticket);
			}
		}

		public void Create(AuthenticationTokenCreateContext context)
		{
			throw new NotImplementedException();
		}

		public void Receive(AuthenticationTokenReceiveContext context)
		{
			throw new NotImplementedException();
		}
	}
}