using NowBuySell.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class MakeController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICarMakeService _carMakeService;

        public MakeController(ICarMakeService carMakeService)
        {
            this._carMakeService = carMakeService;
        }

        [HttpGet]
        [Route("{lang}/makes")]
        public HttpResponseMessage GetMakes(string lang)
        {
            try
            {
                var makes = _carMakeService.GetCarMake().Select(i => new
                {
                    id = i.ID,
                    name = lang == "en" ? i.Name : i.NameAR
                });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",
                    makes = makes
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
