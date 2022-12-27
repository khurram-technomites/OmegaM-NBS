using NowBuySell.Service;
using NowBuySell.Web.Helpers.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
	[RoutePrefix("api/v1")]
	public class CitiesController : ApiController
	{
		string ImageServer = string.Empty;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICityService _cityService;
		private readonly IPropertyService _propService;
		private readonly IAreaService _areaService;
		private readonly ICarService _carService;

		public CitiesController(ICityService cityService, IAreaService areaService, IPropertyService propService, ICarService carService)
		{
			this._cityService = cityService;
			this._areaService = areaService;
			_propService = propService;
			this._carService = carService;
			ImageServer = CustomURL.GetImageServer();
		}

		[HttpGet]
		[Route("{lang}/cities")]
		public HttpResponseMessage GetCities(string lang = "en")
		{
			try
			{
				var cities = _cityService.GetCities().Select(i => new
				{
					id = i.ID,
					name = lang == "en" ? i.Name : i.NameAR,
					PlaceId = i.PlaceId,
					thumbnail = ImageServer + "/" + i.Image,
					icon = ImageServer + "/" + i.Icon,
					PropertyCount = new { Sale = _propService.GetCountByCity((int)i.ID, true), Rent = _propService.GetCountByCity((int)i.ID, false) },
					VehicleCount = _carService.GetCountByCity((int)i.ID)
				});

				return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", cities = cities });

			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}

		[HttpGet]
		[Route("{lang}/cities/{cityId}/areas")]
		public HttpResponseMessage GetStateAreas(long cityId, string lang = "en")
		{
			try
			{
				var areas = _areaService.GetAreas(cityId).Select(i => new
				{
					id = i.ID,
					name = lang == "en" ? i.Name : i.NameAR
				});

				return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", areas = areas });

			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}
	}
}
