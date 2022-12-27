using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySelll.Web.ViewModels.Ticket
{
    public class TicketConversationViewModel
    {
        public string senderName { get; set; }
        public string message { get; set; }
        public string senderType { get; set; }
        public DateTime datetime { get; set; }
        public long id { get; set; }
    }
}