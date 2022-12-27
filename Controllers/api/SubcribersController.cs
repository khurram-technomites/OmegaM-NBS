using System;
using NowBuySell.Service;

using System.Net;
using System.Net.Http;
using System.Web.Http;
using NowBuySell.Data;
using NowBuySell.Web.Helpers;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class SubcribersController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISubscribersService _service;

        public SubcribersController(ISubscribersService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("newsletter")]
        public HttpResponseMessage Post(Subscriber model, string lang = "en")
        {
            string message = string.Empty;
            var subscriber = new Subscriber();

            if (ModelState.IsValid)
            {
                subscriber.CreatedOn = DateTime.Now;
                subscriber.EmailID = model.EmailID;
                if (_service.CreateSubscriber(ref subscriber, ref message))
                {
                    
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = lang == "en" ? message : ArabicDictionary.Translate(message)});
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = lang == "en" ? message : ArabicDictionary.Translate(message) });
        }

    }
}
