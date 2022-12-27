using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    [AuthorizeVendor]
    public class GetInTouchController : Controller
    {
        private readonly IGetInTouchService _getInTouchService;
        private readonly IVendorUserService _VendoruserService;
        private readonly INotificationService _notificationService;
        private readonly IGetInTouchRemarkService _IGetInTouchRemarkService;
        private readonly INotificationReceiverService _notificationReceiverService;
        // GET: VendorPortal/GetInTouch
        public GetInTouchController(IGetInTouchService getInTouchService, IVendorUserService VendoruserService, INotificationService notificationService, INotificationReceiverService notificationReceiverService, IGetInTouchRemarkService iGetInTouchRemarkService)
        {
            this._getInTouchService = getInTouchService;
            this._VendoruserService = VendoruserService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
            this._IGetInTouchRemarkService = iGetInTouchRemarkService;
        }
        [HttpGet]
        public ActionResult GetInToucha(int Id)
        {
            var vendorId = Convert.ToInt64(Session["VendorID"]);
            var getintouch = _getInTouchService.GetById(Id);
            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                getintouch = getintouch
            }, JsonRequestBehavior.AllowGet);
            /*return View();*/
        }
        public ActionResult Index()
        {
            var getintouch = _getInTouchService.GetAll();
            /*return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                getintouch = getintouch
            }, JsonRequestBehavior.AllowGet);*/
            return View(getintouch);
        }
        public ActionResult IndexMotors()
        {
            var getintouch = _getInTouchService.GetAll();
            /*return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                getintouch = getintouch
            }, JsonRequestBehavior.AllowGet);*/
            return View(getintouch);
        }
        public ActionResult ForDashboard()
        {
            var getintouch = _getInTouchService.GetAll();
            /*return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                getintouch = getintouch
            }, JsonRequestBehavior.AllowGet);*/
            return View(getintouch);
        }

        public ActionResult Comments(long Id)
        {
            ViewBag.BuildingID = Id;
            var getintouch = _getInTouchService.GetById((int)Id);
            return PartialView(getintouch);
        }
        public ActionResult ReadMark(long id)
        {
            ViewBag.BuildingID = id;
            var getintouch = _getInTouchService.GetById((int)id);
            return PartialView();
        }

        [HttpGet]
        public ActionResult Update(long id)
        {
            bool status;

            GetInTouch Model = _getInTouchService.GetById((int)id);

            if (!Model.MarkRead.HasValue)
                Model.MarkRead = true;
            else
                Model.MarkRead = Model.MarkRead == true ? false : true;

            status = _getInTouchService.UpdateMarkRead(Model);
            var Data = _getInTouchService.GetById((int)id);
            //return Json(new
            //{
            //    success = true,
            //    message = "Status updated successfully ...",
            //    data = new
            //    {
            //        ID = Data.ID,
            //        Date = Data.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
            //        Name = Data.Name,
            //        Email = Data.Email,
            //        PhoneNo = Data.PhoneNo,
            //        CarID = Data.CarID,
            //        CarName = Data.Car.Name,
            //        CarThumbnail = Data.Car.Thumbnail,
            //        CarAddress = Data.Car.Address,
            //        PropertyID = Data.PropertyID,
            //        CarName = Data.Property.Title,
            //        IsActive = data.IsActive.HasValue ? data.IsActive.Value.ToString() : bool.FalseString,
            //    }
            //});
           // Old Code
            return Json(new
            {
                success = true,
                message = "Status updated successfully ...",

            }, JsonRequestBehavior.AllowGet);
        }
        //Properties
        public ActionResult GetInTouchList()
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            long userId = (long)Session["VendorUserID"];
            var role = Session["Role"].ToString();
            ViewBag.Role = role;
            if(role == "Administrator")
            {
                var getintouch = _getInTouchService.GetListByVendorAndProperty(vendorId, 0);
                return PartialView(getintouch);
            }
            else
            {
                var getintouch = _getInTouchService.GetListByVendorAndProperty(vendorId, (int)userId);
                return PartialView(getintouch);
            }
            
        }
        public ActionResult AssignUser(long Id)
        {
            TempData["GetInTouchId"] = Id;
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            ViewBag.UserID = new SelectList(_VendoruserService.GetVendorUserByRoleId(vendorId,"Lead Manager"), "value", "text");
            return PartialView();
        }

        [HttpPost]
        public ActionResult AssignUser(long UserID, string message)
        {
            long GetInTouchId = (long)TempData["GetInTouchId"];
            var GetInTouch = _getInTouchService.GetById((int)GetInTouchId);
            GetInTouch.VendorUserId = UserID;

            if (_getInTouchService.UpdateAssignUser(GetInTouch))
            {
                Notification not = new Notification();
                not.Title = GetInTouch.CarID == null? "Property Enquires Assign To " + GetInTouch.VendorUser.Name : "Motor Enquires Assign To " + GetInTouch.VendorUser.Name;
                not.Description = "New Customer Enquires";
                not.OriginatorID = Convert.ToInt64(Session["VendorUserID"]);
                not.OriginatorName = Session["VendorUserName"].ToString();
                not.Url = GetInTouch.CarID == null ? "/Vendor/GetInTouch/Index" : "/Vendor/GetInTouch/IndexMotors";
                not.Module = "GetInTouch";
                not.OriginatorType = "Vendor";
                not.RecordID = GetInTouch.ID;
                if (_notificationService.CreateNotification(not, ref message))
                {
                    NotificationReceiver notificationReceiver = new NotificationReceiver();
                    notificationReceiver.NotificationID = not.ID;
                    notificationReceiver.ReceiverType = "Vendor";
                    notificationReceiver.ReceiverID = UserID;
                    if (_notificationReceiverService.CreateNotificationReceiver(notificationReceiver, ref message))
                    {
                    }
                }
                return Json(new
                {
                    success = true,
                    message = "User assign successfully ...",
                });

            }

            else
            {
                message = "Please select the User properly ...";
            }

            return Json(new { success = false, message = message });
        }
        //Motors
        public ActionResult GetInTouchListMotors()
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            long userId = (long)Session["VendorUserID"];
            var role = Session["Role"].ToString();
            ViewBag.Role = role;
            if (role == "Administrator")
            {
                var getintouch = _getInTouchService.GetListByVendorAndMotor(vendorId, 0);
                return PartialView(getintouch);
            }
            else
            {
                var getintouch = _getInTouchService.GetListByVendorAndMotor(vendorId, (int)userId);
                return PartialView(getintouch);
            }
        }
        public ActionResult GetInTouchListfordashboard()
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            long userId = (long)Session["VendorUserID"];
            var role = Session["Role"].ToString();
            ViewBag.Role = role;
            if (role == "Administrator")
            {
                var getintouch = _getInTouchService.GetListByVendorUser(vendorId,0);
                return PartialView(getintouch);
            }
            else
            {   
                var getintouch = _getInTouchService.GetListByVendorUser(vendorId,(int)userId);
                return PartialView(getintouch);
            }
            
        }

        public ActionResult Remarks(long Id)
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            ViewBag.GetInTouchId = Id;
            var remarks =  _IGetInTouchRemarkService.GetByGetInTouchId(Id);    
            return PartialView(remarks);
        }
        [HttpPost]
        public ActionResult Remarks(GetInTouchRemark getInTouchRemark)
        {
            string message = string.Empty;
            object data = null;
            if (getInTouchRemark.Remarks != null)
            {
                GetInTouchRemark CreateRemark = new GetInTouchRemark();
                CreateRemark.Remarks = getInTouchRemark.Remarks;
                CreateRemark.GetInTouchID = getInTouchRemark.GetInTouchID;
                CreateRemark.VendorUserID = Convert.ToInt32(Session["VendorUserID"]);
                CreateRemark.CreatedOn = Helpers.TimeZone.GetLocalDateTime();

                if (_IGetInTouchRemarkService.AddRemarks(ref CreateRemark, ref message))
                {
                    var Vendor = _VendoruserService.GetVendorUserByVendorID((long)CreateRemark.VendorUserID);

                    data = new
                    {
                        ID = CreateRemark.ID,
                        Date = CreateRemark.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                        VendorUserName = Vendor.Name,
                        Remarks = CreateRemark.Remarks,
                    };
                    return Json(new { success = true, message, data }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false, message = message });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int Id)
        {
            string message = string.Empty;
            if (_getInTouchService.DeleteGetInTouch(Id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}