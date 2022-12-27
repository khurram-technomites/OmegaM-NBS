using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Areas.Admin.ViewModels.VendorWalletShare;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizationProvider.AuthorizeAdmin]
	public class VendorWalletShareController : Controller
	{

		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IVendorWalletShareHistoryService _vendorwalletsharehistoryService;
		private readonly IVendorWalletShareService _VendorWalletShareService;
		private readonly IVendorService _vendorService;
		private readonly IVendorWithdrawalRequestService _VendorWithdrawalRequestService;
		private readonly IVendorWalletShareHistoryOrdersService _vendorWalletShareHistoryOrdersService;
		private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;
		private readonly IOrderService _orderService;

		public VendorWalletShareController(IVendorWalletShareHistoryService vendorwalletsharehistoryService, IVendorWalletShareService VendorWalletShareService, IVendorService vendorService, IVendorWithdrawalRequestService VendorWithdrawalRequestService, IVendorWalletShareHistoryOrdersService vendorWalletShareHistoryOrdersService, INotificationService notificationService, INotificationReceiverService notificationReceiverService, IOrderService orderService)
		{
			this._vendorwalletsharehistoryService = vendorwalletsharehistoryService;
			this._VendorWalletShareService = VendorWalletShareService;
			this._vendorService = vendorService;
			this._VendorWithdrawalRequestService = VendorWithdrawalRequestService;
			this._notificationService = notificationService;
			this._notificationReceiverService = notificationReceiverService;
			this._vendorWalletShareHistoryOrdersService = vendorWalletShareHistoryOrdersService;
			this._orderService = orderService;
		}

		public ActionResult Index()
		{
			ViewBag.VendorID = new SelectList(_vendorService.GetVendorsForDropDown(), "value", "text");
			ViewBag.EndDate = Helpers.TimeZone.GetLocalDateTime().ToString("dd/mm/yyyy");
			ViewBag.StartDate= Helpers.TimeZone.GetLocalDateTime().AddDays(-30).ToString("dd/mm/yyyy");
			return View();
		}

		public ActionResult Details(long id)
		{
			try
			{
				var qry = _VendorWalletShareService.GetDetails(id);
				return Json(new
				{
					success = true,
					data = new
					{
						qry.PendingAmount,
						qry.TotalEarning,
						qry.TransferedAmount
					}
				}, JsonRequestBehavior.AllowGet);
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

		[HttpPost]
		public ActionResult BookingReconciliation(long vendorId, List<BookingReconciliationViewModel> BookingReconciliationViewModel)
		{
			try
			{
				string message = string.Empty;
				decimal totalTransferedAmount = 0;
				List<long> ReconciledOrders = new List<long>();

				var vendor = _vendorService.GetVendor(vendorId);

				if (vendor == null)
				{
					return Json(new
					{
						success = false,
						message = "Oops! Vendor not found. Please try later."
					});
				}

				foreach (var item in BookingReconciliationViewModel)
				{
					var order = _orderService.GetOrder(item.BookingID);

					if (order != null)
					{
						var vendorWalletShareHistory = new VendorWalletShareHistory()
						{
							VendorID = vendor.ID,
							Description = string.Format("Booking no {0} amount tranfered to wallet.", order.OrderNo),
							Amount = (order.TotalAmount - ((order.TotalAmount * vendor.Commission) / 100)),
							Type = 2,

						};

						if (_vendorwalletsharehistoryService.CreateVendorWalletShareHistory(ref vendorWalletShareHistory, ref message))
						{
							var vendorWalletShareHistoryOrder = new VendorWalletShareHistoryOrder()
							{
								VendorWalletShareHistoryID = vendorWalletShareHistory.ID,
								OrderID = order.ID
							};

							_vendorWalletShareHistoryOrdersService.CreateVendorWalletShareHistoryOrder(vendorWalletShareHistoryOrder, ref message);

							order.IsEarningCaptured = true;
							if (_orderService.UpdateOrder(ref order, ref message))
							{
								ReconciledOrders.Add(item.BookingID);
								totalTransferedAmount += vendorWalletShareHistory.Amount.Value;
							}
						}
					}
				}

				if (totalTransferedAmount > 0)
				{
					var VendorWalletShare = _VendorWalletShareService.GetWalletShareByVendor(vendorId);

					VendorWalletShare.TotalEarning += totalTransferedAmount;


					if (_VendorWalletShareService.UpdateVendorWalletShare(ref VendorWalletShare, ref message))
					{
						var vendorWalletShareHistory = new VendorWalletShareHistory()
						{
							VendorID = vendor.ID,
							Description = string.Format("AED {0} withdrawed from wallet and will be transfered to {1}.", totalTransferedAmount, vendor.BankAccountNumber),
							Amount = totalTransferedAmount,
							Type = 1
						};

						if (_vendorwalletsharehistoryService.CreateVendorWalletShareHistory(ref vendorWalletShareHistory, ref message))
						{
							foreach (var orderId in ReconciledOrders)
							{
								var vendorWalletShareHistoryOrder = new VendorWalletShareHistoryOrder()
								{
									VendorWalletShareHistoryID = vendorWalletShareHistory.ID,
									OrderID = orderId
								};

								_vendorWalletShareHistoryOrdersService.CreateVendorWalletShareHistoryOrder(vendorWalletShareHistoryOrder, ref message);
							}

							return Json(new
							{
								success = true,
								message = "Selected bookings reconciled"
							}, JsonRequestBehavior.AllowGet);
						}
					}
				}

				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
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