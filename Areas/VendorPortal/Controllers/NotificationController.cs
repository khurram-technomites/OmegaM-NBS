using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using System;
using System.Net;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly INotificationReceiverService _notificationReceiverService;

        public NotificationController(INotificationService notificationService, INotificationReceiverService notificationReceiverService)
        {
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
        }
        // GET: VendorPortal/Notification
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetNotifications()
        {

            if (Session["ReceiverType"] != null)
            {
                string ReceiverType = "Vendor";
                Int64 UserId = 0;
                Int64 clientid = Convert.ToInt64(Session["VendorID"].ToString());
                int pageNo = 1;
                string lang = "en";

                var Notifications = _notificationService.GetNotifications((Int64)clientid, ReceiverType, pageNo, lang);
                _notificationReceiverService.MarkNotificationsAsDelivered((Int64)clientid, ReceiverType);


                return Json(new
                {
                    success = true,
                    message = "Data retrieved successfully !",
                    data = Notifications
                }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, message = "Authorization failed!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult LoadNotifications(int pageNo, string lang = "en")
        {
            if (Session["ReceiverType"] != null)
            {
                string ReceiverType = "Vendor";
                Int64 UserId = 0;
                Int64 clientid = Convert.ToInt64(Session["VendorID"].ToString());
                long vendoruserid = Convert.ToInt64(Session["VendorUserID"].ToString());

                var Notifications = _notificationService.GetNotifications((Int64)clientid, ReceiverType, pageNo, lang, vendoruserid);

                _notificationReceiverService.MarkNotificationsAsDelivered((Int64)clientid, ReceiverType);
                return Json(new
                {
                    success = true,
                    message = "Data retrieved successfully !",
                    data = Notifications

                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Authorization failed!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult MarkNotificationsAsDelivered(string id)
        {
            if (Session["ReceiverType"] != null)
            {
                string ReceiverType = "Vendor";
                Int64 UserId = Convert.ToInt64(Session["VendorID"].ToString());

                if (_notificationReceiverService.MarkNotificationsAsDelivered((Int64)UserId, ReceiverType))
                {
                    return Json(new { success = true, message = "Notification delivered successfully !" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = "Error !" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, message = "Authorization failed!" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult MarkNotificationAsRead(long notificationId)
        {
            if (notificationId != null)
            {
                if (_notificationReceiverService.MarkNotificationAsRead(notificationId))
                {
                    return Json(new { success = true, message = "Notification read successfully !" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = "Error !" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Error !" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult MarkNotificationsAsSeen(long receiverId)
        {
            if (Session["ReceiverType"] != null)
            {
                if (_notificationReceiverService.MarkNotificationsAsSeen(receiverId, Session["ReceiverType"].ToString()))
                {
                    return Json(new { success = true, message = "Notification seen successfully !" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = "Error !" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Authorization failed!" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult NotificationsReadAll(long receiverId)
        {
            if (Session["ReceiverType"] != null)
            {
                if (_notificationReceiverService.MarkNotificationsAsRead(receiverId, Session["ReceiverType"].ToString()))
                {
                    return Json(new { success = true, message = "All notification read successfully !" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = "Error !" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Authorization failed!" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}