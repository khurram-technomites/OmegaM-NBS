using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.MaruCompare
{
	public class LeadRequestViewModel
	{
		[Required]
		public string FullName { get; set; }

		[Required]
		public string Email { get; set; }

		[Required]
		public string Phone { get; set; }

		[Required]
		public string Nationality { get; set; }

		[Required]
		public string Address { get; set; }
	}
}