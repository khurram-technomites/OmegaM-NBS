using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.Areas.Admin.ViewModels
{
	public class UserRolePrivilegeFormViewModel
	{
		[Required]
		public long UserRoleId { get; set; }
		public List<RoutesFormViewModel> Routes { get; set; }
	}

	public class RoutesFormViewModel
	{
		public long Id { get; set; }
		public string Url { get; set; }
	}
}