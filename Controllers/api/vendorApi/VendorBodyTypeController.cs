using NowBuySell.Service;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{

    [RoutePrefix("api/v1/vendor")]
    public class VendorBodyTypeController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IBodyTypeService _bodyTypeService;

        public VendorBodyTypeController(IBodyTypeService bodyTypeService)
        {
            this._bodyTypeService = bodyTypeService;
        }

        [HttpGet]
        [Route("{lang}/bodytype")]
        public HttpResponseMessage GetBodyType(string lang)
        {
            try
            {
                var bodyType = _bodyTypeService.GetBodyType().Select(i => new
                {
                    id = i.ID,
                    name = lang == "en" ? i.Name : i.NameAR

                });

                return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", bodyType = bodyType });

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
