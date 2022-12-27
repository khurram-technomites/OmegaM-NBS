using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Customer
{
	public class ProfilePhotoViewModel
	{
		[Required(ErrorMessage = "The Image is required")]
		public string Image { get; set; }
	}
}