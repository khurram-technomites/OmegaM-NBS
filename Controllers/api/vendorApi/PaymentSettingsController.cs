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
    public class PaymentSettingsController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMyFatoorahPaymentGatewaySettingsService _paymentSetting;

        public PaymentSettingsController(IMyFatoorahPaymentGatewaySettingsService paymentSetting)
        {
            this._paymentSetting = paymentSetting;
        }


        [HttpGet]
        [Route("paymentsettings")]
        public HttpResponseMessage GetClientConfiguration()
        {
            try
            {
                var paymentSetting = _paymentSetting.GetDefaultPaymentGatewaySetting();
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",
                    paymentSetting = paymentSetting != null ? new
                    {
                        paymentSetting.IsLive,
                        paymentSetting.LiveEndpoint,
                        paymentSetting.TestEndpoint,
                        paymentSetting.APIKey,
                    } : null
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
