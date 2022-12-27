using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.Car;
using System;
using System.Collections.Generic;
using System.IO;
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

	public class listingController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly ICarService _carService;
		private readonly IPropertyService _propertyService;
		private readonly ICustomerService _customerService;

		public listingController(ICarService carService , IPropertyService propertyService , ICustomerService customerService)
		{
			this._carService = carService;
			this._propertyService = propertyService;
			this._customerService = customerService;
		}

		[HttpGet]
		[Route("listingStats")]
		public HttpResponseMessage GetListing()
		{
			try
			{
				var cars = _carService.GetCarsCount();
				var propSale = _propertyService.PropertiesForSale();
				var propRent = _propertyService.PropertiesForRent();
				var customers = _customerService.GetCustomers();
				return Request.CreateResponse(HttpStatusCode.OK, new
				{
					status = "success",
					Stats = new
					{
								carsForSale = cars.Count(),
								propertiesForSale = propSale.Count(),
								propertiesForRent = propRent.Count(),
								CommunityMembers = customers.Count()
                    },

				});
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

	}
}
