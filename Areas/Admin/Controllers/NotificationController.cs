using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Web.Mvc;
using NowBuySell.Web.ViewModels.CustomNotification;
using NowBuySell.Web.Helpers.PushNotification;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerSessionService _customerSessionService;
        private readonly ICarService _carService;
        private readonly IPropertyService _propService;
        private readonly IVendorService _vendorService;
        private readonly IVendorSessionService _vendorSessionService;

        public NotificationController(INotificationService notificationService
            , INotificationReceiverService notificationReceiverService
            , ICustomerService customerService
            , ICustomerSessionService customerSessionService
            , ICarService carService
            , IPropertyService propService
            , IVendorService vendorService
            , IVendorSessionService vendorSessionService)
        {
            this._customerSessionService = customerSessionService;
            this._customerService = customerService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
            this._carService = carService;
            _propService = propService;
            this._vendorService = vendorService;
            this._vendorSessionService = vendorSessionService;
        }

        // GET: Admin/Notification
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetNotifications()
        {

            if (Session["ReceiverType"] != null)
            {
                string ReceiverType = "Admin";
                Int64 UserId = 0;
                Int64 clientid = Convert.ToInt64(Session["AdminUserID"].ToString());
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
                string ReceiverType = "Admin";
                Int64 UserId = 0;
                Int64 clientid = Convert.ToInt64(Session["AdminUserID"].ToString());

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
        public ActionResult MarkNotificationsAsDelivered(string id)
        {
            if (Session["ReceiverType"] != null)
            {
                string ReceiverType = "Admin";
                Int64 UserId = Convert.ToInt64(Session["AdminUserID"].ToString());

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

        #region Push Notifications for Mobile App

        [AuthorizeAdmin]
        public ActionResult SendNotification()
        {
            var AdminID = Convert.ToInt64(Session["AdminID"]);
            ViewBag.CustomerID = new SelectList(_customerService.GetCustomersForDropDown(), "value", "text");
            ViewBag.CarID = new SelectList(_carService.GetCarsForDropDown(), "value", "text");
            ViewBag.PropertyID = new SelectList(_propService.GetPropertiesForDropDown(), "value", "text");
            ViewBag.VendorID = new SelectList(_vendorService.GetVendorsForDropDown(), "value", "text");
            return View();
        }

        [HttpPost]
        public ActionResult SendNotification(CustomNotificationViewModel notificationModel)
        {
            try
            {
                string message = string.Empty;
                for (int i = 0; i < notificationModel.Customers.Count; i++)
                {
                    var tokens = _customerSessionService.GetCustomerSessionFirebaseTokens(notificationModel.Customers[i], null, true);

                    Notification not = new Notification();
                    not.Title = notificationModel.Title;
                    not.TitleAr = notificationModel.Title;
                    not.Description = notificationModel.Body;
                    not.DescriptionAr = notificationModel.Body;
                    not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                    not.OriginatorName = Session["UserName"].ToString();
                    not.Module = notificationModel.Module;
                    not.OriginatorType = "Admin";
                    not.RecordID = notificationModel.CarID == 0 ? notificationModel.PropertyID : notificationModel.CarID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        NotificationReceiver notRec = new NotificationReceiver();
                        notRec.ReceiverID = notificationModel.Customers[i];
                        notRec.ReceiverType = "Customer";
                        notRec.NotificationID = not.ID;
                        if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                        {
                            if (tokens.Length > 0)
                            {
                                var response = PushNotification.SendPushNotification(tokens, notificationModel.Title, notificationModel.Body, new
                                {
                                    Module = notificationModel.Module,
                                    RecordID = notificationModel.CarID,
                                    NotificationID = notRec.ID
                                });
                            }
                        }
                    }
                }
                return Json(new { success = true, message = "Notification sent sucessfully... " }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "Error !" }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult SendVendorNotification(CustomNotificationViewModel notificationModel)
        {
            try
            {
                string message = string.Empty;
                for (int i = 0; i < notificationModel.Vendors.Count; i++)
                {                    

                    Notification not = new Notification();
                    not.Title = notificationModel.Title;
                    not.TitleAr = notificationModel.Title;
                    not.Description = notificationModel.Body;
                    not.DescriptionAr = notificationModel.Body;
                    not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                    not.OriginatorName = Session["UserName"].ToString();
                    not.Module = notificationModel.Module;
                    not.OriginatorType = "Admin";
                    //not.RecordID = notificationModel.FacilityID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        NotificationReceiver notRec = new NotificationReceiver();
                        notRec.ReceiverID = notificationModel.Vendors[i];
                        notRec.ReceiverType = "Vendor";
                        notRec.NotificationID = not.ID;
                        if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                        {
                            var tokens = _vendorSessionService.GetVendorSessionFirebaseTokens(notificationModel.Vendors[i]);
                            if (tokens.Length > 0)
                            {
                                var response = PushNotification.SendPushNotification(tokens, notificationModel.Title, notificationModel.Body, new
                                {
                                    Module = notificationModel.Module,
                                    RecordID = notificationModel.PropertyID,
                                    NotificationID = notRec.ID
                                }, false);
                            }
                        }
                    }
                }
                return Json(new { success = true, message = "Vendor notification sent successfully ..." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "Error !" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}