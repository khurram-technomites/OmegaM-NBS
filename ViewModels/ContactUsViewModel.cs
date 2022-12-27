using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels
{
	public class ContactUsViewModel
	{
		[Required]
		public string Name { get; set; }
		[Required]
		public string Email { get; set; }
		[Required]
		public string Contact { get; set; }
		[Required]
		public string Message { get; set; }
		[Required]
		public string Subject { get; set; }
	}
}