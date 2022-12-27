using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Suggestion
{
	public class FeedbackViewModel
	{
		[Required]
		public double? FeedbackRating { get; set; }
		public string FeedbackExperience { get; set; }
		[Required]
		public string FeedbackName { get; set; }
		[Required]
		public string FeedbackEmail { get; set; }
		public string FeedbackMessage { get; set; }
	}
}