using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.ViewModels.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
	[AuthorizeVendor]
	public class VendorWalletHistoryController : Controller
	{
		private readonly IVendorWalletShareHistoryService _vendorwalletsharehistoryService;
		private readonly IVendorWalletShareService _VendorWalletShareService;
		private readonly IVendorWithdrawalRequestService _VendorWithdrawalRequestService;
		private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;

		public VendorWalletHistoryController(IVendorWalletShareService VendorWalletShareService, IVendorWalletShareHistoryService vendorwalletsharehistoryService, IVendorWithdrawalRequestService VendorWithdrawalRequestService, INotificationService notificationService, INotificationReceiverService notificationReceiverService)
		{
			this._VendorWalletShareService = VendorWalletShareService;
			this._vendorwalletsharehistoryService = vendorwalletsharehistoryService;
			this._VendorWithdrawalRequestService = VendorWithdrawalRequestService;
			this._notificationService = notificationService;
			this._notificationReceiverService = notificationReceiverService;
		}

		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;
		public ActionResult Index()
		{
			ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
			ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-7).ToString("MM/dd/yyyy");
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			ViewBag.ErrorMessage = TempData["ErrorMessage"];
			var VendorID = Convert.ToInt64(Session["VendorID"]);
			var qry = _VendorWalletShareService.GetDetails(VendorID);
			return View(qry);
		}
		public ActionResult VendorHistor()
		{
			var VendorID = Convert.ToInt64(Session["VendorID"]);
			var vendorwalletsharehistories = _vendorwalletsharehistoryService.GetVendorWalletShareHistorybyVendorID(VendorID);
			return PartialView(vendorwalletsharehistories);
		}
		[HttpPost]
		public ActionResult VendorHistor(DateTime fromDate, DateTime ToDate)
		{
			DateTime EndDate = ToDate.AddMinutes(1439);
			var VendorID = Convert.ToInt64(Session["VendorID"]);
			var vendorwalletsharehistories = _vendorwalletsharehistoryService.GetVendorWalletShareHistorybyVendorIDDateWise(VendorID, fromDate, EndDate);
			return PartialView(vendorwalletsharehistories);
		}
		public ActionResult RequestList()
		{
			var VendorID = Convert.ToInt64(Session["VendorID"]);
			var VendorRequests = _VendorWithdrawalRequestService.GetVendorWithdrawalRequestByVendorID(VendorID);
			return View(VendorRequests);

		}
		public ActionResult WithdrawalRequest()
		{
			var VendorID = Convert.ToInt64(Session["VendorID"]);
			return View();
		}
		[HttpPost]
		public ActionResult WithdrawalRequest(VendorWithdrawalRequest data)
		{
			string messageNotify = string.Empty;
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				var VendorID = Convert.ToInt64(Session["VendorID"]);
				data.VendorID = VendorID;
				data.Status = "Pending";
				if (_VendorWithdrawalRequestService.CreateRequest(data, ref message))
				{
					Notification not = new Notification();
					not.Title = "Wallet Request";
					not.TitleAr = "طلب المحفظة";
					not.Description = $"Amount of {data.Amount} requested for withdrawal";
					not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
					not.OriginatorName = Session["VendorUserName"].ToString();
					not.Url = "/Admin/VendorWalletShareHistory/Index";
					not.Module = "VendorWallet";
					not.OriginatorType = "Vendor";
					not.RecordID = data.ID;
					if (_notificationService.CreateNotification(not, ref messageNotify))
					{
						if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", not.OriginatorID))
						{
						}
					}
                    //SuccessMessage = message;
                    TempData["SuccessMessage"] = message;
					ViewBag.SuccessMessage = TempData["SuccessMessage"];
					//return View("Index");
				}
			}

			return RedirectToAction("Index");
		}
	}
}