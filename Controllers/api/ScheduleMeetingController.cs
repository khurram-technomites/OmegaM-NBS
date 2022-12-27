using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.ViewModels.Api.ScheduleMeeting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class ScheduleMeetingController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IScheduleMeetingService _meetingService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;
        private readonly IVendorUserService _vendorUserService;

        public ScheduleMeetingController(IScheduleMeetingService meetingService, INotificationReceiverService notificationReceiverService,
            INotificationService notificationService, IPropertyRequestsService propRequestService, IVendorUserService vendorUserService)
        {
            _meetingService = meetingService;
            _notificationReceiverService = notificationReceiverService;
            _notificationService = notificationService;
            _vendorUserService = vendorUserService;
        }

        [HttpPost]
        [Route("ScheduleMeeting")]
        public HttpResponseMessage CreateCarRequest(ScheduleMeetingViewModel meetingtViewModel, string lang = "en")
        {
            string message = string.Empty;
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims.ToList();
            long customerId;
            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
            {
                if (ModelState.IsValid)
                {
                    ScheduleMeeting ScheduleMeeting = new ScheduleMeeting()
                    {
                        VendorID = meetingtViewModel.VendorID,
                        CustomerID = customerId,
                        CarID = meetingtViewModel.CarID,
                        PropertyID = meetingtViewModel.PropertyID,
                        Message = meetingtViewModel.Message,
                        Status = "PENDING",
                        MeetingDate = meetingtViewModel.MeetingDate,
                        CreatedOn = Helpers.TimeZone.GetLocalDateTime()
                    };

                    if (_meetingService.AddMeeting(ScheduleMeeting, ref message))
                    {

                        Notification not = new Notification();
                        not.Title = "Meeting Request";
                        not.Description = "New meeting added for request ";
                        not.OriginatorID = customerId;
                        not.OriginatorName = "";
                        not.Url = "/Vendor/Meeting/Index";
                        not.Module = "MeetingRequest";
                        not.OriginatorType = "Customer";
                        not.RecordID = ScheduleMeeting.ID;
                        if (_notificationService.CreateNotification(not, ref message))
                        {
                            var vendorUser = _vendorUserService.GetUserByRole(meetingtViewModel.VendorID, "Administrator");
                            NotificationReceiver notRec = new NotificationReceiver();
                            notRec.ReceiverID = vendorUser.ID;
                            notRec.ReceiverType = "Vendor";
                            notRec.NotificationID = not.ID;
                            _notificationReceiverService.CreateNotificationReceiver(notRec, ref message);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = lang == "en" ? "Meeting Request Sent successfully" : ArabicDictionary.Translate("Meeting Request Sent successfully", false)
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
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "failed",
                    message = lang == "en" ? message : ArabicDictionary.Translate(message, false)
                });
            }

            return Request.CreateResponse(HttpStatusCode.Unauthorized, new
            {
                status = "failed",
                message = lang == "en" ? "Authorization failed for current request" : ArabicDictionary.Translate("Authorization failed for current request", false)
            });
        }

    }
}
