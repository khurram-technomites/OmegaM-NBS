using NowBuySell.Data;
using System.Collections.Generic;

namespace NowBuySell.Web.Areas.Admin.ViewModels
{
	public class DashboardStatsViewModel
	{
		public SP_GetAdminDashboardStats_Result Stats { get; set; }
		public SP_GetAdminDashboardStatsByRange_Result StatusByRange { get; set; }
		public List<SP_GetAdminDashboardCharts_Result> GetAdminDashboardCharts { get; set; }
		public List<SP_GetAdminDashboardChartsForItemsSold_Result> GetAdminDashboardChartsForItemsSold { get; set; }
		public List<SP_GetAdminDashboardChartForReturn_Result> GetAdminDashboardChartForReturn { get; set; }
		public List<SP_GetTopCustomers_Result> TopCustomers { get; set; }
		public List<SP_GetTopCategories_Result> TopCategories { get; set; }
		public List<SP_GetTopVendors_Result> TopVendors { get; set; }
		public List<SP_GetLastWeekEarning_Result> GetNetSalesChartValues { get; set; }
		
	}

	public class DashboardListViewModel
	{
		public IEnumerable<Order> Orders { get; set; }
		public List<SP_GetDashboardSalesChart_Result> GetAdminDashboardChartForWeeklySalesWise { get; set; }
		public List<SP_GetTopVendors_Result> TopVendors { get; set; }
		public List<SP_GetTopCustomers_Result> TopCustomers { get; set; }
		public List<SP_GetTopCategories_Result> TopCategories { get; set; }
		public List<SP_GetTopCars_Result> TopCars { get; set; }
		public List<SP_GetTopCoupons_Result> TopCoupons { get; set; }
	}
}