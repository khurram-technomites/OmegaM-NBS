using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.Prescription;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class PrescriptionController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPrescriptionService _prescriptionService;
        private readonly IPrescriptionImagesService _prescriptionImagesService;
		private readonly INumberRangeService _numberRangeService;
        private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;

        public PrescriptionController(IPrescriptionService prescriptionService, IPrescriptionImagesService prescriptionImagesService, INumberRangeService numberRangeService, INotificationService notificationService, INotificationReceiverService notificationReceiverService)
        {
            this._prescriptionService = prescriptionService;
            this._prescriptionImagesService = prescriptionImagesService;
			this._numberRangeService = numberRangeService;
            this._notificationService = notificationService;
			this._notificationReceiverService = notificationReceiverService;
        }

		[HttpPost]
		[Authorize]
		[Route("prescription")]
		public async Task<HttpResponseMessage> Create()
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					if (!Request.Content.IsMimeMultipartContent())
					{
						throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
					}
					string message = string.Empty;
					string status = string.Empty;
					string filePath = "/Assets/AppFiles/Prescription";
					string root = HttpContext.Current.Server.MapPath(filePath);
					var provider = new CustomMultipartFormDataStreamProvider(root);

					CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);

					PrescriptionViewModel prescriptionViewModel = JsonConvert.DeserializeObject<PrescriptionViewModel>(provider.FormData.GetValues("prescription").FirstOrDefault());


					Prescription prescription = new Prescription()
					{
						PrescriptionCode = _numberRangeService.GetNextValueFromNumberRangeByName("PRESCRIPTION"),
						Description = prescriptionViewModel.Description,
						CustomerID = customerId,
						Status = "Pending",
					};

					if (_prescriptionService.CreatePrescription(ref prescription, ref message))
					{
						string path = string.Empty;
						for (int i = 0; i < file.FileData.Count; i++)
						{
							var name = file.FileData[i].LocalFileName;
							path = filePath + "/" + name.Substring(name.IndexOf("Prescription\\") + 13);
							PrescriptionImage prescriptionImage = new PrescriptionImage()
							{
								Image = path,
								PrescriptionID = prescription.ID,
							};

							_prescriptionImagesService.CreatePrescriptionImage(ref prescriptionImage, ref message);
						}
					
						/*Prescription Notification For Admin*/
						Notification not = new Notification();
						not.Title = "Prescription";
						not.TitleAr = "";
						not.Description = $"New request for prescription ";
						not.DescriptionAr = $"";
						not.Url = "/Admin/Prescription/Index";
						not.Module = "Prescription";
						not.OriginatorType = "Customer";
						not.RecordID = prescription.ID;
						if (_notificationService.CreateNotification(not, ref message))
						{
							if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", null))
							{
							}
						}

						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "success",
							message = "Prescription captured!"
						});
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.InternalServerError, new
						{
							status = "error",
							message = message
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
		[Authorize]
		[Route("prescription")]
		public HttpResponseMessage GetAll(string status = null, int pg = 1)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					var prescription = _prescriptionService.GetCustomerPrescriptions(customerId, status, pg, 1);

					return Request.CreateResponse(HttpStatusCode.OK, new
					{
						status = "success",
						totalRecords = prescription.Count() > 0 ? prescription.FirstOrDefault().TotalRecords : 0,
						filteredRecords = prescription.Count() > 0 ? prescription.FirstOrDefault().filteredRecords : 0,
						returns = prescription.Select(i => new
						{
							id = i.ID,
							prescriptionNo = i.PrescriptionCode,
							description = i.Description,
							date = i.Date,
							status = i.Status
						})
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
		[Authorize]
		[Route("prescription/{prescriptionId}")]
		public HttpResponseMessage GetDetails(long prescriptionId)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					var prescription = _prescriptionService.GetPrescription(prescriptionId);
					if (prescription != null)
					{
						string ImageServer = CustomURL.GetImageServer();
						var prescriptionRequest = new
						{
							prescription.ID,
							prescriptionNo = prescription.PrescriptionCode,
							prescription.Description,
							date = prescription.CreatedOn,
							prescription.Status,
							prescription.Remarks,
							images = prescription.PrescriptionImages.Select(i => ImageServer + i.Image).ToList(),
						};

						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "success",
							prescriptionRequest = prescriptionRequest
						});
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.BadRequest, new { status = "error", message = "Invalid order id !" });
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