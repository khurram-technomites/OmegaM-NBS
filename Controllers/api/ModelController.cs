using NowBuySell.Service;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class ModelController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICarModelService _modelService;

        public ModelController(ICarModelService modelService)
        {
            this._modelService = modelService;
        }

        [HttpGet]
        [Route("{lang}/model")]
        public HttpResponseMessage GetModels(string lang)
        {
            try
            {
                var model = _modelService.GetCarModel().Select(i => new
                {
                    id = i.ID,
                    name = lang == "en" ? i.Name : i.NameAR,
                    carmakeID = i.CarMake_ID
                });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",
                    model = model
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

        [HttpGet]
        [Route("{lang}/makes/{makeId}/models")]
        public HttpResponseMessage GetMakeModels(long makeId, string lang)
        {
            try
            {
                var model = _modelService.GetCarModel().Where(i => i.CarMake_ID == makeId).Select(i => new
                {
                    id = i.ID,
                    name = lang == "en" ? i.Name : i.NameAR,
                });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",
                    model = model
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
