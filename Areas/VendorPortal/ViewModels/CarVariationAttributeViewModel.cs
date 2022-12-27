using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.Areas.VendorPortal.ViewModels
{
	public class CarVariationAttributeViewModel
	{
		[Required]
		public long CarId { get; set; }
		public long CarVariationId { get; set; }
		[MustHaveOneElementAttribute(ErrorMessage = "At least a attribute is required")]
		public List<long> CarAttributes { get; set; }
	}
	public class MustHaveOneElementAttribute : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			var list = value as IList;
			if (list != null)
			{
				return list.Count > 0;
			}
			return false;
		}
	}
}