using NowBuySell.Service;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NowBuySell.Data;
using NowBuySell.Web.ViewModels.Api.GetInTouch;
using NowBuySell.Web.Helpers.PushNotification;
using NowBuySell.Web.Helpers;
using System;
using System.Security.Claims;
using System.Threading;
using System.Linq;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class GetInTouchController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGetInTouchService _service;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;
        private readonly IVendorSessionService _vendorSessionService;
        private readonly IVendorUserService _vendorUserService;
        public GetInTouchController(IGetInTouchService service
            , INotificationReceiverService notificationReceiverService
            , INotificationService notificationService
            , IVendorSessionService vendorSessionService
            ,IVendorUserService vendorUserService )
        {
            _service = service;
            _notificationReceiverService = notificationReceiverService;

            _notificationService = notificationService;
            _vendorSessionService = vendorSessionService;
            _vendorUserService = vendorUserService;
        }

        [HttpPost]
        [Route("getInTouch")]
        public HttpResponseMessage Post(GetInTouchViewModel Model, string lang = "en")
        {
            if (ModelState.IsValid)
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId=0;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                   
                }

                    GetInTouch getInTouch = new GetInTouch();
                string message = string.Empty, message2 = string.Empty;

                getInTouch.Name = Model.Name;
                getInTouch.PhoneNo = Model.PhoneNo;
                getInTouch.Email = Model.Email;
                getInTouch.Comments = Model.Comments;
                getInTouch.MarkRead = false;
                getInTouch.VendorID = Model.VendorID;
                getInTouch.CarID = Model.CarID;
                getInTouch.PropertyID = Model.PropertyID;
                getInTouch.CreatedOn = Helpers.TimeZone.GetLocalDateTime();
                getInTouch.CustomerID = customerId;

                Model = null;

                if (_service.Add(getInTouch, ref message))
                {
                    Notification not = new Notification();
                    not.Title = getInTouch.CarID.HasValue ? "Car Request" : "Property Request";
                    not.Description = getInTouch.CarID.HasValue ? string.Format("{0} is interested in your motor ", getInTouch.Name) : string.Format("{0} is interested in your property ", getInTouch.Name);
                    //not.OriginatorID = customerId;
                    not.OriginatorName = "";
                    not.Url = getInTouch.CarID.HasValue ? "/Vendor/CarRequests/Index" : "/Vendor/PropertyRequests/Index";
                    //not.Module = getInTouch.CarID.HasValue ? "GetInTouchRequest" : "GetInTouchRequest";
                    not.Module = "GetInTouchRequest";
                    not.OriginatorType = "Customer";
                    not.RecordID = getInTouch.ID;

                    if (_notificationService.CreateNotification(not, ref message2))
                    {
                        var vendorUser = _vendorUserService.GetUserByRole((long)getInTouch.VendorID, "Administrator");
                        NotificationReceiver notRec = new NotificationReceiver();
                        notRec.ReceiverID = vendorUser.ID;
                        notRec.ReceiverType = "Vendor";
                        notRec.NotificationID = not.ID;
                        if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message2))
                        {
                            var tokens = _vendorSessionService.GetVendorSessionFirebaseTokens(getInTouch.VendorID.Value);
                            if (tokens.Length > 0)
                            {
                                var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
                                {
                                    Module = not.Module,
                                    RecordID = getInTouch.ID,
                                    NotificationID = notRec.ID
                                }, false);
                            }
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = true,
                        message = lang == "en" ? message : ArabicDictionary.Translate(message, false)
                    });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = true,
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    message = lang == "en" ? "Please fill the form properly." : ArabicDictionary.Translate("Please fill the form properly.", false)
                });
            }

        }
    }
}
