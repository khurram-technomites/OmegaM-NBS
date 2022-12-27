using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.MaruCompare;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;


namespace NowBuySell.Web.Controllers.api
{
	//[Authorize]
	[RoutePrefix("api/v1")]
	public class MaruCompareController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IServicesCategoryService _servicesCategoryService;
		private readonly IServicesService _servicesService;
		private readonly IServicesCarService _servicesCarService;
		private readonly ILeadRequestService _leadRequestService;
		private readonly INumberRangeService _numberRangeService;

		public MaruCompareController(IServicesCategoryService servicesCategoryService, IServicesService servicesService, IServicesCarService servicesCarService, ILeadRequestService leadRequestService, INumberRangeService numberRangeService)
		{
			this._servicesCategoryService = servicesCategoryService;
			this._servicesService = servicesService;
			this._servicesCarService = servicesCarService;
			this._servicesCarService = servicesCarService;
			this._leadRequestService = leadRequestService;
			this._numberRangeService = numberRangeService;
		}

		[Route("{lang}/services")]
		public HttpResponseMessage GetServiceCategories(string lang = "en")
		{
			try
			{
				var AllServices = _servicesService.GetServices().ToList();
				string ImageServer = CustomURL.GetImageServer();
				var ServiceCategories = _servicesCategoryService.GetServicesCategories().Select(i =>
				new
				{
					i.ID,
					i.Name,

					services = AllServices.Where(j => j.CategoryID == i.ID).Select(m => new
					{
						m.ID,
						m.Name,
						image = ImageServer + m.Image,
					})
				}

				);
				return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", ServiceCategories = ServiceCategories });
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}


		[Route("{lang}/services/{serviceID}")]
		public HttpResponseMessage GetServiceDetails(string lang = "en", long? serviceID = null)
		{

			try
			{
				if (serviceID == null)
				{
					throw new Exception("Invalid Service ID");
				}
				string ImageServer = CustomURL.GetImageServer();
				var ServiceDetails = _servicesService.GetService((long)serviceID);
				if (ServiceDetails == null)
				{
					throw new Exception("Invalid Service ID");
				}
				var ServiceCars = _servicesCarService.GetServiceCars().Where(i => i.ServiceID == serviceID).Select(s =>
					new
					{
						s.ID,
						s.Title,
						s.Fee,
						image = ImageServer + s.Image,

					});
				var service = new
				{
					ServiceDetails.ID,
					ServiceDetails.Name,
					image = ImageServer + ServiceDetails.Image,
					cars = ServiceCars

				};
				return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", service = service });
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				if (ex.Message == "Invalid Service ID")
				{
					return Request.CreateResponse(HttpStatusCode.NotFound, new { status = "failure", message = ex.Message });

				}
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}


		[Route("{lang}/services/car/{carID}")]
		public HttpResponseMessage GetCarDetails(string lang = "en", long? carID = null)
		{

			try
			{
				if (carID == null)
				{
					throw new Exception("Invalid Car ID");
				}
				string ImageServer = CustomURL.GetImageServer();

				var ServiceCar = _servicesCarService.GetServiceCar((long)carID);

				ServiceCar.Image = ImageServer + ServiceCar.Image;

				var CarAttributes = _servicesCarService.GetCarAttribute((long)carID).Select(i => new
				{
					i.ID,
					i.Name,
					i.Value
				});

				var CarDetails = new
				{
					ServiceCar.ID,
					ServiceCar.Title,
					ServiceCar.Image,
					ServiceCar.Fee,
					ServiceCar.MobileDescription,
					attributes = CarAttributes
				};

				return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", car = CarDetails });
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				if (ex.Message == "Invalid Car ID")
				{
					return Request.CreateResponse(HttpStatusCode.NotFound, new { status = "failure", message = ex.Message });

				}
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}

		[HttpPost]
		[Route("services/car/{carID}")]
		public HttpResponseMessage PostLeadRequest(long? carID, LeadRequestViewModel leadRequestViewModel)
		{

			try
			{
				if (carID == null)
				{
					throw new Exception("Invalid Car ID");
				}
				if (!ModelState.IsValid)
				{
					throw new Exception("Bad Request");
				}



				LeadRequest leadRequest = new LeadRequest();
				leadRequest.RequestNo = _numberRangeService.GetNextValueFromNumberRangeByName("LEAD_REQUEST");
				leadRequest.Name = leadRequestViewModel.FullName;
				leadRequest.Phone = leadRequestViewModel.Phone;
				leadRequest.ServiceCarID = carID;
				leadRequest.Nationality = leadRequestViewModel.Nationality;
				leadRequest.Email = leadRequestViewModel.Email;
				leadRequest.Address = leadRequestViewModel.Address;


				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					leadRequest.CustomerID = customerId;
				}

				string message = "";
				string error = "";

				if (_leadRequestService.CreateRequest(ref leadRequest, ref message, ref error))
				{
					return Request.CreateResponse(HttpStatusCode.OK, new
					{
						status = "success",
						message = "Lead Request created succesfully"
					});
				}
				else
				{
					throw new Exception(error);
				}
				//var leadRequest = _leadRequestService.CreateRequest()

			}
			catch (Exception ex)
			{
				if (ex.Message == "Bad Request")
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, new
					{
						status = "failure",
						message = ex.Message,
						description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
					});

				}

				if (ex.Message == "Invalid Car ID")
				{
					return Request.CreateResponse(HttpStatusCode.NotFound, new
					{
						status = "failure",
						message = ex.Message
					});

				}
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		[HttpGet]
		[Authorize]
		[Route("services/requests")]
		public HttpResponseMessage GetAll(int pg = 1)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{

					var requests = _leadRequestService.GetLeadRequestsByCustomer(customerId, pg).Select(i => new
					{
						i.ID,
						RequestNo = "REQ00000001",
						i.Name,
						i.Phone,
						i.CreatedOn,
						status = i.ApprovalStatus,
						serviceCar = new
						{
							id = i.ServiceCarID,
							title = i.ServiceCar.Title,
						}
					});

					return Request.CreateResponse(HttpStatusCode.OK, new
					{
						status = "success",
						requests = requests
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
		[Route("services/requests/{requestId}")]
		public HttpResponseMessage GetRequestDetails(long requestId)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{

					var requestDetails = _leadRequestService.GetLeadRequest(requestId);
					if (requestDetails != null)
					{
						string ImageServer = CustomURL.GetImageServer();

						var serviceCar = requestDetails.ServiceCar;
						var service = serviceCar != null ? requestDetails.ServiceCar.ServiceCompare : null;
						var serviceCategory = service != null ? service.ServiceCategory : null;

						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "success",
							requests = new
							{
								requestDetails.ID,
								requestDetails.RequestNo,
								requestDetails.Name,
								requestDetails.Email,
								requestDetails.Company,
								requestDetails.Nationality,
								requestDetails.Phone,
								requestDetails.Address,
								requestDetails.Remarks,
								requestDetails.CreatedOn,
								status = requestDetails.ApprovalStatus,
								serviceCar = serviceCar != null ? new
								{
									id = serviceCar.ID,
									Image = ImageServer + serviceCar.Image,
									title = serviceCar.Title,
									description = serviceCar.MobileDescription,

								} : null,
								Service = service != null ?
									new
									{
										id = service.ID,
										Image = ImageServer + service.Image,
										title = service.Name,
										description = service.Description,
									} : null,
								serviceCategory = serviceCategory != null ?
									new
									{
										id = serviceCategory.ID,
										Image = ImageServer + serviceCategory.Image,
										title = serviceCategory.Name,
										description = serviceCategory.Description,
									} : null,

							}
						});
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.NotFound, new
						{
							status = "error",
							message = "Request Not Found!"
						});
					}
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new
					{
						status = "error",
						message = "Session invalid or expired !"
					});
				}
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				//Logs.Write(ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		//[HttpGet]
		//[Route("services/requests")]
		//public HttpResponseMessage GetAll(int pg = 1)
		//{
		//	try
		//	{
		//		var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
		//		var claims = identity.Claims;
		//		long customerId;
		//		if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
		//		{
		//			var requests = _leadRequestService.GetLeadRequestsByCustomer(customerId, pg).Select(i => new
		//			{
		//				i.ID,
		//				RequestNo = "REQ00000001",
		//				i.Name,
		//				i.Phone,
		//				i.CreatedOn,
		//				status = i.ApprovalStatus
		//			});
		//			return Request.CreateResponse(HttpStatusCode.OK, new
		//			{
		//				status = "success",
		//				requests = requests
		//			});
		//		}
		//		else
		//		{
		//			return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		log.Error("Error", ex);
		//		//Logs.Write(ex);
		//		return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
		//	}
		//}
	}
}
