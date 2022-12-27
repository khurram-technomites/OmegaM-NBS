using NowBuySell.Service;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{

	[RoutePrefix("api/v1")]
	public class CountriesController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICountryService _countryService;
		private readonly ICityService _cityService;

		public CountriesController(ICountryService countryService, ICityService cityService)
		{
			this._countryService = countryService;
			this._cityService = cityService;
		}

		[HttpGet]
		[Route("{lang}/countries")]
		public HttpResponseMessage GetCountries(string lang = "en")
		{
			try
			{
				var countries = _countryService.GetCountries().Select(i => new
				{
					id = i.ID,
					name = lang == "en" ? i.Name : i.NameAr
				});

				return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", countries = countries });
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}

		[HttpGet]
		[Route("{lang}/country/{countryId}/cities")]
		public HttpResponseMessage GetCountryStates(long countryId, string lang = "en")
		{
			try
			{
				var cities = _cityService.GetCities(countryId).Select(i => new
				{
					id = i.ID,
					name = lang == "en" ? i.Name : i.NameAR
				});

				return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", cities = cities });
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}
	}
}
