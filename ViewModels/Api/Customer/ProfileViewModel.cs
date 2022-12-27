using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Customer
{
	public class ProfileViewModel
	{

		[Required(ErrorMessage = "The Name is required")]
		public string Name { get; set; }
		public string Contact { get; set; }
		public string Email { get; set; }


		//[Required(ErrorMessage = "The Country is required")]
		public long? CountryId { get; set; }

		//[Required(ErrorMessage = "The City is required")]
		public long? CityId { get; set; }

		//[Required(ErrorMessage = "The Area is required")]
		public long? AreaId { get; set; }

		//[Required(ErrorMessage = "The Address is required")]
		public string Address { get; set; }
	}
}