using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels
{
    public class SubscriberMailViewModel
    {
		[Required]
		public List<int> Email { get; set; }
		[Required]
		public string Contact { get; set; }
		[Required]
		public string Message { get; set; }
		[Required]
		public string Subject { get; set; }
	}
}