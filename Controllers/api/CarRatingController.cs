using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.CarRating;
using Newtonsoft.Json;
using System;
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
	public class CarRatingController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICarService _carService;
		private readonly ICarRatingService _carRatingService;
		private readonly ICarRatingImageService _carRatingImageService;
		private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;
		private readonly IOrderService _orderService;

		public CarRatingController(ICarRatingService carRatingService, ICarRatingImageService carRatingImageService, INotificationService notificationService, INotificationReceiverService notificationReceiverService, ICarService carService, IOrderService orderService)
		{
			this._carRatingService = carRatingService;
			this._carRatingImageService = carRatingImageService;
			this._notificationService = notificationService;
			this._notificationReceiverService = notificationReceiverService;
			this._carService = carService;
			this._orderService = orderService;
		}

		[HttpGet]
		[Route("cars/{carId}/ratings")]
		public HttpResponseMessage GetCarRatings(long carId, string lang = "en")
		{
			try
			{
				string ImageServer = CustomURL.GetImageServer();
				var ratings = _carRatingService.GetCarRatings(carId, ImageServer);
				return Request.CreateResponse(HttpStatusCode.OK, new
				{
					status = "success",
					ratings = ratings.Select(i => new
					{
						id = i.ID,
						date = i.Date,
						customer = new
						{
							image = i.CustomerImage,
							name = i.Name
						},
						rating = i.Rating,
						remarks = i.Remarks,
					})
				});
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}

		[HttpPost]
		[Authorize]
		[Route("ratings")]
		public HttpResponseMessage Create(CarRatingViewModel carRatingViewModel)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{

					string message = string.Empty;
					string status = string.Empty;

					Order order = _orderService.GetOrder(carRatingViewModel.OrderID);

					CarRating carRating = new CarRating()
					{
						OrderDetailID = order.OrderDetails.FirstOrDefault().ID,
						CarID = carRatingViewModel.CarID,
						CustomerID = customerId,
						Rating = carRatingViewModel.Rating,
						Remarks = carRatingViewModel.Remarks
					};

					if (_carRatingService.CreateCarRating(ref carRating, ref message))
					{
						/*Order Notification For Admin & Vendor*/
						Car car = _carService.GetCar((Int64)carRating.CarID);

						Notification not = new Notification();
						not.Title = "Car Rating";
						not.TitleAr = "تصنيف المنتج";
						not.Description = $"New car rating for {car.Name} ";
						not.DescriptionAr = $"New car rating for {car.Name} ";
						not.Url = "/Admin/CustomerRating/Index";
						not.Module = "Rating";
						not.OriginatorType = "Customer";
						not.RecordID = carRating.ID;
						if (_notificationService.CreateNotification(not, ref message))
						{
							if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", null))
							{
							}
						}

						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "success",
							message = "Car rating captured!"
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
	}
}
