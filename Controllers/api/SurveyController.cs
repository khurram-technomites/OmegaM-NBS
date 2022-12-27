using NowBuySell.Data;
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
    public class SurveyController : ApiController
    {
        private readonly ISurveyService _service;

        public SurveyController(ISurveyService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("survey")]
        public HttpResponseMessage Add(Survey Model)
        {
            string message = string.Empty;
            if(Model != null)
            {
                Model.CreatedON = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
                if (_service.AddSurvey(Model, ref message))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = "Record created successfully"
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "failed",
                        message = "Ops! something went wrong."
                    });
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                status = "failed",
                message = "Please fill the form properly."
            });
        }
    }
}
