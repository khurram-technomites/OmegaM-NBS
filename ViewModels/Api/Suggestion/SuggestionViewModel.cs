using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Suggestion
{
	public class SuggestionViewModel
	{
		[Required]
		public string Suggestion { get; set; }
		[Required]
		public float Rating { get; set; }
	}
}