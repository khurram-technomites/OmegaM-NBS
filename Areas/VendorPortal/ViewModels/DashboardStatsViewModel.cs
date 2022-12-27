using NowBuySell.Data;
using System.Collections.Generic;

namespace NowBuySell.Web.Areas.VendorPortal.ViewModels
{
	public class DashboardStatsViewModel
	{
		public SP_GetVendorDashboardStats_Result Stats { get; set; }
		public SP_GetVendorDashboardStatsByRange_Result StatusByRange { get; set; }
		public IEnumerable<SP_GetVendorOrders_Result> Orders { get; set; }
		public List<SP_GetTopCustomers_Result> TopCustomers { get; set; }
		public List<SP_GetTopCategories_Result> TopCategories { get; set; }
		public List<SP_GetTopVendors_Result> TopVendors { get; set; }
		//public List<SP_GetDashboardSalesChart_Result> GetNetSalesChartValues { get; set; }
		public List<SP_GetLastWeekEarning_Result> GetNetSalesChartValues { get; set; }
        public int TotalProperties { get; set; }
        public int PropertyApprovals { get; set; }

    }
	public class DashboardListViewModel
	{
		public List<SP_GetTopCustomers_Result> TopCustomers { get; set; }
		public List<SP_GetTopCategories_Result> TopCategories { get; set; }
		public List<SP_GetTopCars_Result> TopCars { get; set; }
	}
}