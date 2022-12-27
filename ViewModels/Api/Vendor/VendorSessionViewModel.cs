using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.Vendor
{
    public class VendorSessionViewModel
    {
		[Required]
		public string FirebaseToken { get; set; }
		[Required]
		public string DeviceID { get; set; }

		[Required]
		public string AccessToken { get; set; }
	}
}