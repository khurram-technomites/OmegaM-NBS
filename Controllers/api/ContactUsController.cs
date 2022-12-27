using NowBuySell.Service;
using NowBuySell.Service.Helpers;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.ViewModels;
using NowBuySell.Web.ViewModels.Api.Coupon;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class ContactUsController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMail _mail;
        private readonly IBusinessSettingService _businessSettingService;

        public ContactUsController(IBusinessSettingService businessSettingService, IMail mail)
        {
            this._businessSettingService = businessSettingService;
            this._mail = mail;
        }

        [HttpPost]
        [Route("ContactUs")]
        public HttpResponseMessage Create(ContactUsViewModel contactus, string lang = "en")
        {
            try
            {
                //var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                //var claims = identity.Claims;
                //long customerId;
                //if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                //{
                string message = string.Empty;
                if (ModelState.IsValid)
                {

                    var path = HttpContext.Current.Server.MapPath("~/");
                    if (_mail.SendContactUsMail(contactus.Name, contactus.Email, contactus.Subject, contactus.Contact, contactus.Message))
                    {

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = lang == "en" ? "Thank you! Your message has been successfully sent." : ArabicDictionary.Translate("Thank you! Your message has been successfully sent.", false),
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                        {
                            status = "failure",
                            message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "error",
                        message = lang == "en" ? "Bad request!" : ArabicDictionary.Translate("Bad request!", false),
                        description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
                    });
                }
                //}
                //else
                //{
                //    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                //    {
                //        status = "error",
                //        message = "Session invalid or expired !"
                //    });
                //}
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                });
            }
        }

        [HttpGet]
        [Route("ContactUs")]
        public HttpResponseMessage GetClientConfiguration(string lang = "en")
        {
            try
            {
                var businesssettingSetting = _businessSettingService.GetDefaultBusinessSetting();
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",
                    config = new
                    {
                        BusinessSetting = new
                        {
                            WhatsappDetails = new
                            {
                                Title = businesssettingSetting.Title,
                                WhatsappNo = businesssettingSetting.Whatsapp,
                                FirstMessage = businesssettingSetting.FirstMessage,
                            },
                            ContactDetails = new
                            {
                                ContactNo = businesssettingSetting.Contact,
                                Email = businesssettingSetting.Email,
                                Address = businesssettingSetting.StreetAddress,
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
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                });
            }
        }

    }
}
