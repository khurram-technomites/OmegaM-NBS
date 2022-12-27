using NowBuySell.Data;
using NowBuySell.Service;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class ConfigController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ITaxSettingService _taxSettingService;
        private readonly IBusinessSettingService _businessSettingService;
        private readonly IMyFatoorahPaymentGatewaySettingsService _paymentSetting;

        public ConfigController(ITaxSettingService taxSettingService, IBusinessSettingService businessSettingService, IMyFatoorahPaymentGatewaySettingsService paymentSetting)
        {
            this._taxSettingService = taxSettingService;
            this._businessSettingService = businessSettingService;
            this._paymentSetting = paymentSetting;
        }

        [HttpGet]
        [Route("configuration")]
        public HttpResponseMessage GetClientConfiguration()
        {
            try
            {
                var paymentSetting = _paymentSetting.GetDefaultPaymentGatewaySetting();
                var taxPercent = _taxSettingService.GetTotalTax();
                var businesssettingSetting = _businessSettingService.GetDefaultBusinessSetting();
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",
                    config = new
                    {
                        TaxPercentage = taxPercent,
                        BusinessSetting = new
                        {
                            WhatsappDetails = new
                            {
                                Title = businesssettingSetting.Title,
                                TitleAr = businesssettingSetting.TitleAr,
                                WhatsappNo = businesssettingSetting.Whatsapp,
                                FirstMessage = businesssettingSetting.FirstMessage,
                                FirstMessageAr = businesssettingSetting.FirstMessageAr,
                            },
                            ContactDetails = new
                            {
                                ContactNo = businesssettingSetting.Contact,
                                Email = businesssettingSetting.Email,
                                Address = businesssettingSetting.StreetAddress,
                            },
                            HoursOfOperation = new
                            {
                                Days = businesssettingSetting.Days
                            },
                            SocialMediaLinks = new
                            {
                                Facebook = businesssettingSetting.Facebook,
                                Instagram = businesssettingSetting.Instagram,
                                Youtube = businesssettingSetting.Youtube,
                                Twitter = businesssettingSetting.Twitter,
                                Snapchat = businesssettingSetting.Snapchat,
                                LinkedIn = businesssettingSetting.LinkedIn,
                                Behance = businesssettingSetting.Behance,
                                Pinterest = businesssettingSetting.Pinterest,

                            },
                            TaxAndCompare = new
                            {
                                businesssettingSetting.IsTaxInclusive,
                                businesssettingSetting.IsMaruCompare,
                            },
                            LoyaltyAndRedemption = new
                            {
                                enabled = businesssettingSetting.IsLoyaltyEnabled
                            },

                            paymentSetting = paymentSetting != null ? new
                            {
                                paymentSetting.IsLive,
                                paymentSetting.LiveEndpoint,
                                paymentSetting.TestEndpoint,
                                paymentSetting.APIKey,
                            } : null
                        },
                    }
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
