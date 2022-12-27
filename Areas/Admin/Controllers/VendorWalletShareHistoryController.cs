using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using LinqToExcel;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class VendorWalletShareHistoryController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IVendorWalletShareHistoryService _vendorwalletsharehistoryService;
		private readonly IVendorWalletShareService _VendorWalletShareService;
		private readonly IVendorService _vendorService;
		private readonly IVendorWithdrawalRequestService _VendorWithdrawalRequestService;
		private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;
		public VendorWalletShareHistoryController(IVendorWalletShareHistoryService vendorwalletsharehistoryService, IVendorWalletShareService VendorWalletShareService, IVendorService vendorService, IVendorWithdrawalRequestService VendorWithdrawalRequestService, INotificationService notificationService, INotificationReceiverService notificationReceiverService)
		{
			this._vendorwalletsharehistoryService = vendorwalletsharehistoryService;
			this._VendorWalletShareService = VendorWalletShareService;
			this._vendorService = vendorService;
			this._VendorWithdrawalRequestService = VendorWithdrawalRequestService;
			this._notificationService = notificationService;
			this._notificationReceiverService = notificationReceiverService;
		}

		public ActionResult Index()
		{
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			ViewBag.ErrorMessage = TempData["ErrorMessage"];
			return View();
		}

		[HttpPost]
		public ActionResult TransferAmount(long VendorID, decimal TransferedAmount)
		{
			string message = null;
			try
			{
				var VendorDetail = _VendorWalletShareService.GetDetails(VendorID);
				VendorDetail.TransferedAmount += TransferedAmount;
				VendorDetail.PendingAmount -= TransferedAmount;
				if (_VendorWalletShareService.UpdateVendorWalletShare(ref VendorDetail, ref message))
				{
					VendorWalletShareHistory walletDetail = new VendorWalletShareHistory();
					walletDetail.VendorID = VendorID;
					walletDetail.Amount = TransferedAmount;
					walletDetail.Description = "Funds Transfer";
					walletDetail.Type = 1;
					if (_vendorwalletsharehistoryService.CreateVendorWalletShareHistory(ref walletDetail, ref message))
					{
						TempData["SuccessMessage"] = " Successfull Transfered";
					}

					#region Notification

					Notification not = new Notification();
					not.Title = "Wallet Transfer";
					not.TitleAr = "تحويل المحفظة";
					not.Description = "Amount of " + TransferedAmount + " have been transfered to your wallet ";
					not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
					not.OriginatorName = Session["UserName"].ToString();
					not.Url = "/Vendor/VendorWalletHistory/Index";
					not.Module = "VendorWalletHistory";
					not.OriginatorType = "Admin";
					not.RecordID = VendorDetail.ID;
					if (_notificationService.CreateNotification(not, ref message))
					{
						NotificationReceiver notRec = new NotificationReceiver();
						notRec.ReceiverID = VendorDetail.VendorID;
						notRec.ReceiverType = "Vendor";
						notRec.NotificationID = not.ID;
						if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
						{
						}
					}

					#endregion
				}

				return RedirectToAction("Index", new { id = VendorID });
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "Error";
				return View("Index", new { id = VendorID });
			}

		}

		public ActionResult List()
		{
			var vendorwalletsharehistories = _vendorwalletsharehistoryService.GetVendorWalletShareHistories();
			return PartialView(vendorwalletsharehistories);
		}

		public ActionResult VendorWallet()
		{
			ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
			ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-30).ToString("MM/dd/yyyy");
			ViewBag.VendorID = new SelectList(_vendorService.GetVendorsForDropDown(), "value", "text");
			return View();
		}

		public ActionResult Create()
		{
			ViewBag.VendorID = new SelectList(_vendorService.GetVendorsForDropDown(), "value", "text");
			return View();
		}

		public JsonResult getVendorDetail(int VendorID)
		{

			if (VendorID > 0)
			{
				var qry = _VendorWalletShareService.GetDetails(VendorID);
				return Json(new { qry.PendingAmount, qry.TotalEarning, qry.TransferedAmount });
			}
			else
			{
				var qry = 1;
				return Json(qry);
			}
		}

		//public ActionResult WalletHitoryList()
		//{
		//    var vendorwalletsharehistories = _vendorwalletsharehistoryService.GetVendorWalletShareHistories();
		//    return View(vendorwalletsharehistories);
		//}

		public ActionResult WalletHitoryList(long id)
		{
			ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("dd/mm/yyyy");
			ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-30).ToString("dd/mm/yyyy");
			var vendorwalletsharehistories = _vendorwalletsharehistoryService.GetVendorWalletShareHistorybyVendorID(id);
			return View(vendorwalletsharehistories);
		}

		[HttpPost]
		public ActionResult ListReport()
		{
			var vendorwalletsharehistories = _vendorwalletsharehistoryService.GetVendorWalletShareHistories();
			return View(vendorwalletsharehistories);
		}

		public ActionResult Details(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			VendorWalletShareHistory vendorwalletsharehistory = _vendorwalletsharehistoryService.GetVendorWalletShareHistory((Int16)id);
			if (vendorwalletsharehistory == null)
			{
				return HttpNotFound();
			}
			return View(vendorwalletsharehistory);
		}

		public ActionResult PendingReuests()
		{
			var VendorRequests = _VendorWithdrawalRequestService.GetVendorWithdrawalRequest();
			return View(VendorRequests);
		}
		public ActionResult AcceptRequest(long id, bool approvalStatus)
		{
			ViewBag.BuildingID = id;
			ViewBag.ApprovalStatus = approvalStatus;

			var req = _VendorWithdrawalRequestService.GetWithdrawalRequest((long)id);
			if (approvalStatus == true)
			{
				req.Status = "Processed";
			}
			else
			{
				req.Status = "Rejected";
			}
			return View(req);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult AcceptRequest(VendorWithdrawalRequest ApprovalRequest)
		{
			string message = string.Empty;
			try
			{
				if (_VendorWithdrawalRequestService.UpdateRequest(ref ApprovalRequest, ref message, true))
				{
					if (ApprovalRequest.Status == "Processed")
					{
						var VendorDetail = _VendorWalletShareService.GetDetails((long)ApprovalRequest.VendorID);
						VendorDetail.TransferedAmount += ApprovalRequest.Amount;
						VendorDetail.PendingAmount -= ApprovalRequest.Amount;
						if (_VendorWalletShareService.UpdateVendorWalletShare(ref VendorDetail, ref message))
						{

							VendorWalletShareHistory walletDetail = new VendorWalletShareHistory();
							walletDetail.VendorID = ApprovalRequest.VendorID;
							walletDetail.Amount = ApprovalRequest.Amount;
							walletDetail.Description = " Funds Transfer";
							walletDetail.Type = 1;
							if (_vendorwalletsharehistoryService.CreateVendorWalletShareHistory(ref walletDetail, ref message))
							{
								TempData["SuccessMessage"] = " Successfully Transfered";
							}
						}
					}
					else
					{
						TempData["SuccessMessage"] = "Requested rejected successfully";
					}

					#region Notification

					Notification not = new Notification();
					not.Title = "Wallet Request";
					not.TitleAr = "طلب المحفظة";
					if (ApprovalRequest.Status == "Processed")
					{
						not.Description = "Your request for amount of " + ApprovalRequest.Amount + " have been approved ";
					}
					else
					{
						not.Description = "Your request for amount of " + ApprovalRequest.Amount + " have been rejected ";
					}
					not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
					not.OriginatorName = Session["UserName"].ToString();
					not.Url = "/Vendor/VendorWalletHistory/Index";
					not.Module = "VendorWalletHistory";
					not.OriginatorType = "Admin";
					not.RecordID = ApprovalRequest.ID;
					if (_notificationService.CreateNotification(not, ref message))
					{
						NotificationReceiver notRec = new NotificationReceiver();
						notRec.ReceiverID = ApprovalRequest.VendorID;
						notRec.ReceiverType = "Vendor";
						notRec.NotificationID = not.ID;
						if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
						{
						}
					}

					#endregion
				}
				return RedirectToAction("Index", new { id = ApprovalRequest.VendorID });
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "Error";
				return View("Index", new { id = ApprovalRequest.VendorID });
			}
		}

		[HttpPost]
		public ActionResult WalletHitoryList(DateTime sd, DateTime ed, long VendorID)
		{
			DateTime EndDate = ed.AddMinutes(1439);
			var history = _vendorwalletsharehistoryService.GetHistoryDateWise(sd, EndDate, VendorID);
			return PartialView(history);
		}


		[HttpPost]
		public ActionResult GetVendorHistory(long vendorId, DateTime startDate, DateTime endDate)
		{
			try
			{
				endDate = endDate.AddMinutes(1439);
				var history = _vendorwalletsharehistoryService.GetHistoryDateWise(startDate, endDate, vendorId);

				return Json(new
				{
					success = true,
					data = history.Select(i => new
					{
						i.ID,
						i.CreatedOn,
						i.Description,
						i.Amount,
						i.Type
					})
				});
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}
	}
}