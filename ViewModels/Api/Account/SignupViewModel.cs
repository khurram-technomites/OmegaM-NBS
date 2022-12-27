using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Account
{
	public class SignupViewModel
	{
		
		public string Email { get; set; }
        public string Name { get; set; }

        public string UserName { get; set; }
	
		public string Password { get; set; }
	
		public string Contact { get; set; }

        public long? ReferralID { get; set; }
	}
}