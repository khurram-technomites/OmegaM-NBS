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
    public class BrandsController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IBrandsService _brandsService;

        public BrandsController(IBrandsService brandsService)
        {
            this._brandsService = brandsService;
        }

        [HttpGet]
        [Route("{lang}/brands")]
        public HttpResponseMessage GetCountries(string lang = "en")
        {
            try
            {
                var countries = _brandsService.GetBrands().Select(i => new
                {
                    id = i.ID,
                    name = lang == "en" ? i.Name : i.NameAr
                });

                return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", countries = countries });
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
