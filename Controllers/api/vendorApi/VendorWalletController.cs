using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.ViewModels.Api.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1/vendor")]
    public class VendorWalletController : ApiController
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		
		private readonly IVendorService _vendorService;
		private readonly IVendorWalletShareHistoryService _vendorWalletShareHistoryService;
		private readonly IVendorWalletShareService _vendorWalletShareService;
		private readonly IVendorWithdrawalRequestService _vendorWithdrawalRequestService;
		private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;
		



		public VendorWalletController( IVendorService vendorService ,IVendorWalletShareHistoryService vendorWalletShareHistoryService , IVendorWithdrawalRequestService vendorWithdrawalRequestService , INotificationService notificationService, INotificationReceiverService notificationReceiverService, IVendorWalletShareService vendorWalletShareService)
		{
			
			this._vendorService = vendorService;
			this._vendorWalletShareHistoryService = vendorWalletShareHistoryService;
			this._vendorWithdrawalRequestService = vendorWithdrawalRequestService;
			this._vendorWalletShareService = vendorWalletShareService;
			this._notificationService = notificationService;
			this._notificationReceiverService = notificationReceiverService;

		}

		[HttpPost]
		[Route("walletsharehistory")]
		public HttpResponseMessage GetVendorWallet(VendorWalletShareHistoryViewModel walletShareHistory)
		{
			//string status = string.Empty;
			try
			{

				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long vendorId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
				{ 

					var vendorwalletsharehistories = _vendorWalletShareHistoryService.GetHistoryDateWise(walletShareHistory.startDate , walletShareHistory.endDate , vendorId);

					return Request.CreateResponse(HttpStatusCode.OK, new
						{


							status = "success",
						    
						    transactionHistory = vendorwalletsharehistories.Select(i => new { 

							    id = i.ID,
								amount = i.Amount,
								type = i.Type,
								description = i.Description,
								date = i.CreatedOn
								
							}).OrderByDescending(i => i.id).ToList().Skip(20 * (walletShareHistory.pgno - 1)).Take(20),
					   });

			    }

				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				//Logs.Write(ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}

		[HttpGet]
		[Route("withdrawalhistory")]
		public HttpResponseMessage GetVendorWithdrawalRequest(int pgno)
		{
			//string status = string.Empty;
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long vendorId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
				{

					var vendorWithdrawalRequest = _vendorWithdrawalRequestService.GetVendorWithdrawalRequestByVendorID(vendorId);

					return Request.CreateResponse(HttpStatusCode.OK, new
					{
						status = "success",

						withdrawalrequestHistory = vendorWithdrawalRequest.Select(i => new {

							id = i.ID,
							amount = i.Amount,
							status = i.Status,
							remarks = i.Remarks,
							description = i.Description,
							date = i.CreatedOn

						}).OrderByDescending(i => i.id).ToList().Skip(20 * (pgno - 1)).Take(20),
					});

				}

				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				//Logs.Write(ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}


		[HttpPost]
		[Route("withdrawal")]
		public HttpResponseMessage WithdrawalRequest(VendorWithdrawalRequest data)
		{
			//string status = string.Empty;
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long vendorId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
				{
					    string messageNotify = string.Empty;
					    string message = string.Empty; 
						data.VendorID = vendorId;
						data.Status = "Pending";
					    var vendor = _vendorService.GetVendor(vendorId);
						if (_vendorWithdrawalRequestService.CreateRequest(data, ref message))
						{

						Notification not = new Notification();
						not.Title = "Wallet Request";
						not.TitleAr = "طلب المحفظة";
						not.Description = $"Amount of {data.Amount} requested for withdrawal";
						not.OriginatorID = vendorId;
						not.OriginatorName = vendor.Name;
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

						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "success",
							message = message

						});

					    }
                       else
                       {
						  return Request.CreateResponse(HttpStatusCode.BadRequest, new
						 {
							status = "error",
							message = "Something went wrong"

						  });
					   }
				}

				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				//Logs.Write(ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}

		[HttpGet]
		[Route("walletshare")]
		public HttpResponseMessage GetVendorWalletShare()
		{
			//string status = string.Empty;
			try
			{

				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long vendorId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
				{
					var vendorwalletsharehistories = _vendorWalletShareService.GetWalletShareByVendor(vendorId);
					if (vendorwalletsharehistories != null)
                    {
						return Request.CreateResponse(HttpStatusCode.OK, new
						{


							status = "success",
							totalEarning = vendorwalletsharehistories.TotalEarning,
							pendingAmount = vendorwalletsharehistories.PendingAmount,
							transferredAmount = vendorwalletsharehistories.TransferedAmount,
							commission = vendorwalletsharehistories.Commission,


						});
					}

                    else
                    {
						return Request.CreateResponse(HttpStatusCode.OK, new
						{


							status = "error",
							message = "There is no wallet share"
							


						});
					}
					

					

				}

				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				//Logs.Write(ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}




	}
}
