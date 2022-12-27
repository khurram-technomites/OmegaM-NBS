using NowBuySell.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Ticket
{
	public class TicketDetailsViewModel
	{

		public long TicketID { get; set; }
		public string TicketNo { get; set; }
		public string Description { get; set; }
		public string Status { get; set; }
		public string Priority { get; set; }
		public DateTime CreatedOn { get; set; }

		public List<SP_GetTicketConversation_Result> ticketConversation { get; set; }
	}
}