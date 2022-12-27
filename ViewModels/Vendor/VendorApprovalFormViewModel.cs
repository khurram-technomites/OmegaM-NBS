using System.ComponentModel.DataAnnotations;
using static NowBuySell.Web.Helpers.Enumerations.Enumeration;

namespace NowBuySell.Web.ViewModels.Car
{
	public class CarApprovalFormViewModel
	{
		[Required]
		public long ID { get; set; }
		[Required]
		public bool IsApproved { get; set; }
		[Required]
		public string Remarks { get; set; }
	}
}