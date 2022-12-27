using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.CarReturn;
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
	public class CarReturnController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICarReturnService _carReturnService;
		private readonly ICarReturnImageService _carReturnImageService;
		private readonly INumberRangeService _numberRangeService;
		private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;

		public CarReturnController(ICarReturnService carReturnService, ICarReturnImageService carReturnImageService, INumberRangeService numberRangeService, INotificationService notificationService, INotificationReceiverService notificationReceiverService)
		{
			this._carReturnService = carReturnService;
			this._carReturnImageService = carReturnImageService;
			this._numberRangeService = numberRangeService;
			this._notificationService = notificationService;
			this._notificationReceiverService = notificationReceiverService;
		}

		[HttpPost]
		[Authorize]
		[Route("returns")]
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
					string filePath = "/Assets/AppFiles/Return";
					string root = HttpContext.Current.Server.MapPath(filePath);
					var provider = new CustomMultipartFormDataStreamProvider(root);

					CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);

					CarReturnViewModel carReturnViewModel = JsonConvert.DeserializeObject<CarReturnViewModel>(provider.FormData.GetValues("return").FirstOrDefault());


					CarReturn carReturn = new CarReturn()
					{
						ReturnCode = _numberRangeService.GetNextValueFromNumberRangeByName("CAR_RETURN"),
						OrderDetailID = carReturnViewModel.OrderDetailID,
						CarID = carReturnViewModel.CarID,
						CustomerID = customerId,
						ReturnMethod = carReturnViewModel.ReturnMethod,
						Reason = carReturnViewModel.Reason,
						Status = "Pending"
					};

					if (_carReturnService.CreateCarReturn(ref carReturn, ref message))
					{

						string path = string.Empty;
						if (file.FileData.Count() > 0)
						{
							var name = file.FileData[0].LocalFileName;
							path = filePath + "/" + name.Substring(name.IndexOf("Return\\") + 7);
							CarReturnImage carReturnImage = new CarReturnImage()
							{
								Image = path,
								CarReturnID = carReturn.ID,
							};

							_carReturnImageService.CreateCarReturnImage(ref carReturnImage, ref message);

						}
						if (file.FileData.Count() > 1)
						{
							var name = file.FileData[1].LocalFileName;
							path = filePath + "/" + name.Substring(name.IndexOf("Return\\") + 7);
							CarReturnImage carReturnImage = new CarReturnImage()
							{
								Image = path,
								CarReturnID = carReturn.ID,
							};

							_carReturnImageService.CreateCarReturnImage(ref carReturnImage, ref message);
						}

						/*Order Notification For Admin & Vendor*/
						Notification not = new Notification();
						not.Title = "Car Return";
						not.TitleAr = "تصنيف المنتج";
						not.Description = $"New request for car return ";
						not.DescriptionAr = $"New request for car return ";
						not.Url = "/Admin/CarReturn/Index";
						not.Module = "CarReturn";
						not.OriginatorType = "Customer";
						not.RecordID = carReturn.ID;
						if (_notificationService.CreateNotification(not, ref message))
						{
							if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", null))
							{
							}
						}

						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "success",
							message = "Car return captured!"
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
		[Route("returns")]
		public HttpResponseMessage GetAll(string status = null, int pg = 1)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					var returns = _carReturnService.GetCustomerReturns(customerId, status, pg, 1);

					return Request.CreateResponse(HttpStatusCode.OK, new
					{
						status = "success",
						totalRecords = returns.Count() > 0 ? returns.FirstOrDefault().TotalRecords : 0,
						filteredRecords = returns.Count() > 0 ? returns.FirstOrDefault().filteredRecords : 0,
						returns = returns.Select(i => new
						{
							id = i.ID,
							date = i.Date,
							returnCode = i.ReturnCode,
							orderNo = i.OrderNo,
							car = i.Car,
							returnMethod = i.ReturnMethod,
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
		[Route("returns/{returnId}")]
		public HttpResponseMessage GetDetails(long returnId)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long staffId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out staffId))
				{
					var carReturn = _carReturnService.GetCarReturn(returnId);
					if (carReturn != null)
					{
						string ImageServer = CustomURL.GetImageServer();
						var carReturnRequest = new
						{
							carReturn.ID,
							carReturn.ReturnCode,
							carReturn.ReturnMethod,
							carReturn.Reason,
							carReturn.Status,
							images = carReturn.CarReturnImages.Select(i => ImageServer + i.Image).ToList(),
							order = new
							{
								id = carReturn.OrderDetail.OrderID,
								orderNumber = carReturn.OrderDetail.Order.OrderNo,
							},
							car = new
							{
								id = carReturn.CarID,
								name = carReturn.Car.Name,
								thumbnail = ImageServer + carReturn.Car.Thumbnail,
							},
						};

						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "success",
							carReturnRequest = carReturnRequest
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
