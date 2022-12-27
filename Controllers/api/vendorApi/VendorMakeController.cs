using NowBuySell.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api.vendorApi
{
	[RoutePrefix("api/v1/vendor")]
	public class VendorMakeController : ApiController
    {

	       private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

			private readonly ICarMakeService _carMakeService;

			public VendorMakeController(ICarMakeService carMakeService)
			{
				this._carMakeService = carMakeService;
			}

			[HttpGet]
			[Route("car/{lang}/make")]
			public HttpResponseMessage GetMake(string lang)
			{
				try
				{
					var make = _carMakeService.GetCarMake().Select(i => new
					{
						id = i.ID,
						name = lang == "en" ? i.Name : i.NameAR

					});

					return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", CarMake = make });

				}
				catch (Exception ex)
				{
					log.Error("Error", ex);
					return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
				}
			}
		}

	}

