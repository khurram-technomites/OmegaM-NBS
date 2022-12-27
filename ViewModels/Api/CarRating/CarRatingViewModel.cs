using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.CarRating
{
	public class CarRatingViewModel
	{
		[Required]
		public long CarID { get; set; }
		[Required]
		public long OrderID { get; set; }
		[Required]
		public float Rating { get; set; }
		[Required]
		public string Remarks { get; set; }
	}
}