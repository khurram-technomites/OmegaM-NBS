using Newtonsoft.Json;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Service.Helpers;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Payments;
using NowBuySell.Web.Helpers.Payments.MyFatoorah;
using NowBuySell.Web.Helpers.Payments.MyFatoorah.Initiate;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Account;
using NowBuySell.Web.ViewModels.Vendor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    public class AccountController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IVendorService _vendorService;
        private readonly IVendorUserService _vendorUserService;
        private readonly IMail _email;
        private readonly ICityService _cityService;
        private readonly ICountryService _countryService;
        private readonly IVendorDocumentService _vendorDocumentService;
        private readonly IVendorPackagesService _vendorPackageService;
        private readonly INotificationService _notificationService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly IMyFatoorahPaymentGatewaySettingsService _myFatoorahPaymentGatewaySettingsService;
        private readonly IVendorTransactionHistoryService _vendorTransactionHistory;
        private readonly ITransactionService _transactionService;
        private readonly IPropertyService _propService;
        private readonly ICarService _carSerivce;

        private readonly IPropertyService _propertyService;
        private readonly ICarService _carService;

        public AccountController(IVendorService vendorService, IVendorUserService vendorUserService, IMail email, ICityService city, ICountryService country,
            IVendorDocumentService vendorDocumentService, INotificationService notificationService, INotificationReceiverService notificationReceiverService,
            IVendorPackagesService vendorPackageService, IMyFatoorahPaymentGatewaySettingsService myFatoorahPaymentGatewaySettingsService,
            IVendorTransactionHistoryService vendorTransactionHistory, IPropertyService propertyService, ICarService carService, ITransactionService transactionService,
            IPropertyService propService, ICarService carSerivce)
        {
            this._vendorService = vendorService;
            this._vendorUserService = vendorUserService;
            this._email = email;
            this._cityService = city;
            this._countryService = country;
            this._vendorDocumentService = vendorDocumentService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;

            _vendorPackageService = vendorPackageService;
            _myFatoorahPaymentGatewaySettingsService = myFatoorahPaymentGatewaySettingsService;
            _vendorTransactionHistory = vendorTransactionHistory;
            _propertyService = propertyService;
            _carService = carService;
            _transactionService = transactionService;
            _propertyService = propService;
            _carSerivce = carSerivce;
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel Login)
        {
            Session.Clear();
            string message = string.Empty;
            int errorCode = 0;
            long VendorID = 0;
            if (ModelState.IsValid)
            {
                var VendorUser = _vendorUserService.Authenticate(Login.EmailAddress, Login.Password, ref ErrorMessage, ref errorCode, ref VendorID);
                if (VendorUser != null)
                {
                    var Vendor = _vendorService.GetVendor((long)VendorUser.VendorID);

                    string Chars = "";
                    try
                    {

                        if (!string.IsNullOrEmpty(VendorUser.Name))
                        {
                            string[] names = VendorUser.Name.Split(' ');
                            for (int i = 0; i < names.Length; i++)
                            {
                                char Char;
                                Char = names[i].ToUpper().First();
                                Chars += Char;
                            }
                        }
                        else
                        {
                            Chars = "V";
                        }

                    }
                    catch (Exception)
                    {
                        Chars = "V";
                    }

                    Session["VendorID"] = VendorUser.VendorID;
                    Session["UserNameChar"] = Chars;
                    Session["Role"] = VendorUser.VendorUserRole.Name;
                    Session["Email"] = VendorUser.EmailAddress;
                    Session["ReceiverType"] = "Vendor";

                    string url = "/Vendor/Dashboard/Index";
                    if (!Vendor.VendorPackageID.HasValue || (Vendor.PackageEndDate.HasValue && Vendor.PackageEndDate.Value < NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime()))
                    {
                        Session["VendorIdle"] = true;
                        url = "/Vendor/Account/PackageSubscription";
                    }
                    else if (Vendor.IsApproved.HasValue && Vendor.IsApproved.Value == true)
                    {
                        Session["VendorUserID"] = VendorUser.ID;
                        Session["VendorUserName"] = VendorUser.Name;
                        Session["VendorPackageID"] = Vendor.VendorPackageID;
                        Session["VendorIdle"] = false;

                        Response.Cookies["Vendor-Session"]["Access-Token"] = VendorUser.AuthorizationCode;

                        GenerateExpiryNotification(Vendor, ref message);
                    }
                    else
                    {
                        Session["VendorIdle"] = true;
                        url = "/Vendor/Account/ProfileManagement";
                    }

                    return Json(new { success = true, url = url, message = ErrorMessage });
                }
                else
                {
                    if (errorCode == 1)
                    {
                        bool isSent;
                        var vendor = _vendorService.GetVendor(VendorID);
                        isSent = _vendorService.SendOTP(vendor.Contact).Result;
                        Session["Contact"] = vendor.Contact;
                        Session["IsSent"] = isSent;
                    }
                    else if (errorCode == 2)
                    {
                        message = string.Empty;
                        var vendor = _vendorService.GetVendor(VendorID);
                        var path = Server.MapPath("~/");
                        if (_email.SendVerificationMail(vendor.Name, vendor.Email, CustomURL.GetFormatedURL("/Vendors/Verify?auth=" + vendor.AuthorizationCode), path))
                        {
                            if (vendor.IsEmailSent == null || !vendor.IsEmailSent.Value)
                            {
                                vendor.IsEmailSent = true;
                                _vendorService.UpdateVendor(ref vendor, ref message);
                            }
                        }
                    }

                    ViewBag.Message = ErrorMessage;
                }
            }
            else
            {
                ErrorMessage = "Please enter email and password first !";
            }
            return Json(new
            {
                success = false,
                errorCode = errorCode,
                message = ErrorMessage
            });
        }

        private void GenerateExpiryNotification(Vendor Model, ref string message)
        {
            DateTime currentDT = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
            Notification notification = _notificationService.GetPackageNotificationsByVendor((int)Model.ID);

            if (currentDT.AddDays(3).Date == Model.PackageEndDate.Value.Date || currentDT.AddDays(2).Date == Model.PackageEndDate.Value.Date ||
                currentDT.AddDays(1).Date == Model.PackageEndDate.Value.Date || currentDT.Date == Model.PackageEndDate.Value.Date)
            {
                if (notification == null || (currentDT.Date > notification.CreatedOn.Value.Date))
                {


                    Notification not = new Notification();
                    not.Title = "Package Expiry";
                    not.TitleAr = "انتهاء صلاحية الحزمة";

                    not.Description = "Your current package is about to expire.";
                    not.Url = "/Vendor/Account/PackageSubscription";

                    not.Description = "Your current package is about to expire.";
                    not.Url = "/Vendor/Account/PackageSubscription";
                    not.OriginatorID = Model.ID;
                    not.OriginatorName = Model.Name;
                    not.Module = "PackageExpiry";
                    not.OriginatorType = "Vendor";

                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        NotificationReceiver notRec = new NotificationReceiver();
                        notRec.ReceiverID = Model.ID;
                        notRec.ReceiverType = "Vendor";
                        notRec.NotificationID = not.ID;
                        if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                        {
                        }
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgetPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            string Message = string.Empty;
            if (ModelState.IsValid)
            {
                var path = Server.MapPath("~/");
                if (_vendorUserService.ForgotPassword(forgotPasswordViewModel.EmailAddress, ref Message, path))
                {
                    return Json(new { success = true, message = Message });
                }
                else
                {
                    ErrorMessage = Message;
                }
            }
            else
            {
                ErrorMessage = "Please fill the form properly ...";
            }


            return Json(new { success = false, message = ErrorMessage });
        }

        public ActionResult ResetPassword()
        {
            var AuthCode = Request.QueryString["auth"];
            if (AuthCode == "" || AuthCode == null)
            {
                ViewBag.ErrorMessage = "Invalid Session!";
            }
            else
            {
                var vendorUser = _vendorUserService.GetByAuthCode(AuthCode);
                if (vendorUser != null)
                {
                    if (vendorUser.AuthorizationExpiry >= Helpers.TimeZone.GetLocalDateTime())
                    {
                        Session["VendorUserResetID"] = vendorUser.ID;
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Session expired!";
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid Session!";
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            string Message = string.Empty;

            if (ModelState.IsValid)
            {
                if (Session["VendorUserResetID"] != null)
                {
                    Int64 VendorUserId = Convert.ToInt64(Session["VendorUserResetID"]);

                    if (_vendorUserService.ResetPassword(resetPasswordViewModel.NewPassword, VendorUserId, ref Message))
                    {
                        Session.Remove("VendorUserResetID");
                        string url = "/Vendor/Dashboard/Index";
                        return Json(new { success = true, url = url, message = Message });
                    }
                    else
                    {
                        ErrorMessage = Message;
                    }
                }
                else
                {
                    ErrorMessage = "Session expired!";
                }
            }
            else
            {
                ErrorMessage = "Please fill the form properly ...";
            }
            return Json(new { success = false, message = ErrorMessage });
        }

        [AuthorizeVendor]
        public ActionResult ChangePassword()
        {
            long vendorUserId = Convert.ToInt64(Session["VendorUserID"]);
            var vendorUser = _vendorUserService.GetVendorUser(vendorUserId);

            return View(vendorUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeVendor]
        public ActionResult ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            string Message = string.Empty;

            if (ModelState.IsValid)
            {

                Int64 VendorUserId = Convert.ToInt64(Session["VendorUserID"]);

                if (_vendorUserService.ChangePassword(changePasswordViewModel.CurrentPassword, changePasswordViewModel.NewPassword, VendorUserId, ref Message))
                {
                    return Json(new
                    {
                        success = true,
                        message = "Password Changed Successfully"
                    });
                }
                else
                {
                    ErrorMessage = Message;
                }
            }
            return Json(new { success = false, message = Message });
        }

        [AuthorizeVendor]
        public ActionResult Logout()
        {
            Session.Remove("VendorUserID");
            Session.Remove("VendorUserName");
            Session.Remove("Role");
            Session.Remove("Email");
            Session.Remove("VendorID");

            Request.Cookies.Remove("Vendor-Session");

            return RedirectToAction("Login");
        }
        [AuthorizeVendor]
        //[AllowAnonymous]
        public ActionResult ProfileManagement()
        {
            if (Session["VendorIdle"] != null && TempData.ContainsKey("IsPaid"))
                ViewBag.MessagePaid = TempData["IsPaid"].ToString();
            else if (Session["VendorIdle"] != null && TempData.ContainsKey("ErrorMessage"))
                ViewBag.MessagePaidErr = TempData["ErrorMessage"].ToString();

            if (Session["VendorID"] == null)
            {
                ErrorMessage = "Vendor Not Authorize!";
                return RedirectPermanent("~/Vendor/Account/Login");
            }


            var id = Convert.ToInt64(Session["VendorID"]);
            //if (id == null)
            //{
            //	return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            Vendor vendor = _vendorService.GetVendor((long)id);
            if (vendor == null)
            {
                return HttpNotFound();
            }

            try
            {
                ViewBag.CountryID = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text", vendor.CountryID);
                ViewBag.CityID = new SelectList(_cityService.GetCitiesForDropDown(), "value", "text", vendor.CityID);
                VendorEditViewModel vendorEditViewModel = new VendorEditViewModel()
                {
                    ID = vendor.ID,
                    VendorCode = vendor.VendorCode,
                    Name = vendor.Name,
                    NameAr = vendor.NameAr,
                    Slug = vendor.Slug,
                    Email = vendor.Email,
                    Contact = string.IsNullOrEmpty(vendor.Contact) ? "" : vendor.Contact.Replace("971", ""),
                    Mobile = string.IsNullOrEmpty(vendor.Mobile) ? "" : vendor.Mobile.Replace("971", ""),
                    Address = vendor.Address,
                    IDNo = vendor.IDNo,
                    TRN = vendor.TRN,
                    Website = vendor.Website,
                    License = vendor.License,
                    Latitude = vendor.Latitude,
                    Longitude = vendor.Longitude,
                    CountryID = vendor.CountryID,
                    CityID = vendor.CityID,
                    Logo = vendor.Logo,
                    CoverImage = vendor.CoverImage,
                    OpeningTime = vendor.OpeningTime.HasValue ? DateTime.Today.Add(vendor.OpeningTime.Value).ToString("hh:mm tt") : null,
                    ClosingTime = vendor.ClosingTime.HasValue ? DateTime.Today.Add(vendor.ClosingTime.Value).ToString("hh:mm tt") : null,
                    TermsAndConditionWebEn = vendor.TermAndConditionWebEn,
                    TermsAndConditionWebAr = vendor.TermAndConditionWebAr,
                    ServingKilometer = vendor.ServingKilometer.HasValue ? (double)vendor.ServingKilometer : 0,
                    IsApproved = vendor.IsApproved,
                    BankName = vendor.BankName,
                    BankAccountNumber = vendor.BankAccountNumber,
                    PassportNo = vendor.PassportNo,
                    Whatsapp = string.IsNullOrEmpty(vendor.Whatsapp) ? "" : vendor.Whatsapp.Replace("971", ""),
                    LinkedIn = vendor.LinkedIn,
                    Snapchat = vendor.Snapchat,
                    Twitter = vendor.Twitter,
                    Youtube = vendor.Youtube,
                    Instagram = vendor.Instagram,
                    Facebook = vendor.Facebook,
                    TikTok = vendor.TikTok,
                    IsSocialManagment = vendor.IsSocialManagment,
                };
                ViewBag.vendorRemarks = !string.IsNullOrEmpty(vendor.Remarks) ? 1 : 0;
                ViewBag.VendorID = vendor.ID;
                ViewBag.SuccessMessage = TempData["SuccessMessage"];

                if (!vendor.IsApproved.HasValue)
                {
                    ViewBag.VendorApprovalState = "null";
                }
                else if (vendor.IsApproved.HasValue && vendor.IsApproved.Value == false)
                {
                    ViewBag.VendorApprovalState = "rejected";
                }
                else if (vendor.IsApproved.HasValue && vendor.IsApproved.Value == false)
                {
                    ViewBag.VendorApprovalState = "rejected";
                }
                else
                {
                    ViewBag.VendorApprovalState = "approved";
                }

                return View(vendorEditViewModel);
            }
            catch (Exception ex)
            {
                VendorEditViewModel vendorEditViewModel = new VendorEditViewModel();
                return View(vendorEditViewModel);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult ProfileManagement(VendorEditViewModel vendorEditViewModel)
        {
            string message = string.Empty;
            vendorEditViewModel.Slug = Slugify.GenerateSlug(vendorEditViewModel.Name);
            if (ModelState.IsValid)
            {
                var vendor = _vendorService.GetVendor(vendorEditViewModel.ID);
                vendor.ID = vendorEditViewModel.ID;
                vendor.VendorCode = vendorEditViewModel.VendorCode;
                vendor.Name = vendorEditViewModel.Name;
                vendor.NameAr = vendorEditViewModel.NameAr;
                vendor.Email = vendorEditViewModel.Email;
                vendor.Contact = "971" + vendorEditViewModel.Contact;
                vendor.Mobile = "971" + vendorEditViewModel.Mobile;
                vendor.BankName = vendorEditViewModel.BankName;
                vendor.BankAccountNumber = vendorEditViewModel.BankAccountNumber;
                vendor.Address = vendorEditViewModel.Address;
                vendor.IDNo = vendorEditViewModel.IDNo;
                vendor.TRN = vendorEditViewModel.TRN;
                vendor.Website = vendorEditViewModel.Website;
                vendor.License = vendorEditViewModel.License;

                int PermitNo;
                if (int.TryParse(vendorEditViewModel.License, out PermitNo)) ;
                {
                    vendor.PermitNo = PermitNo;
                }

                vendor.OpeningTime = DateTime.Parse(vendorEditViewModel.OpeningTime, new CultureInfo("ar-AE")).TimeOfDay;
                vendor.ClosingTime = DateTime.Parse(vendorEditViewModel.ClosingTime, new CultureInfo("ar-AE")).TimeOfDay;
                vendor.Longitude = vendorEditViewModel.Longitude;
                vendor.Latitude = vendorEditViewModel.Latitude;
                vendor.CountryID = vendorEditViewModel.CountryID;
                vendor.CityID = vendorEditViewModel.CityID;
                vendor.TermAndConditionWebEn = vendorEditViewModel.TermsAndConditionWebEn;
                vendor.TermAndConditionWebAr = vendorEditViewModel.TermsAndConditionWebAr;
                vendor.PassportNo = vendorEditViewModel.PassportNo;
                vendor.Whatsapp = vendorEditViewModel.Whatsapp != "" && vendorEditViewModel.Whatsapp != null ? "+971" + vendorEditViewModel.Whatsapp : null;
                vendor.LinkedIn = vendorEditViewModel.LinkedIn;
                vendor.Snapchat = vendorEditViewModel.Snapchat;
                vendor.Twitter = vendorEditViewModel.Twitter;
                vendor.Youtube = vendorEditViewModel.Youtube;
                vendor.Instagram = vendorEditViewModel.Instagram;
                vendor.Facebook = vendorEditViewModel.Facebook;
                vendor.TikTok = vendorEditViewModel.TikTok;
               

                vendor.ServingKilometer = vendorEditViewModel.ServingKilometer;

                if (vendorEditViewModel.IsApproved == false)
                {
                    vendor.IsApproved = null;
                    Notification not = new Notification();
                    not.Title = "Vendor Approval";
                    not.Description = "New Vendor added for approval ";
                    not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                    not.OriginatorName = vendor.Name;
                    not.Url = "/Admin/Vendor/Approvals";
                    not.Module = "Vendor";
                    not.OriginatorType = "Vendor";
                    not.RecordID = vendor.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", not.OriginatorID))
                        {
                        }
                    }
                }

                if (vendorEditViewModel.Logo != null && vendorEditViewModel.Logo!="undedined")
                {
                    string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/Vendors/"), vendor.ID, "/logo");

                    string absolutePath = Server.MapPath("~");
                    string relativePath = string.Format("/Assets/AppFiles/Images/Vendors/{0}/", vendor.VendorCode);

                    vendor.Logo = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "logo", ref message, "Logo");

                }
                if (vendor.CoverImage != vendorEditViewModel.CoverImage && vendorEditViewModel.CoverImage != null)
                {
                    string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/Vendors/"), vendor.ID, "/cover");

                    string absolutePath = Server.MapPath("~");
                    string relativePath = string.Format("/Assets/AppFiles/Images/Vendors/{0}/", vendor.ID);

                    vendor.CoverImage = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "CoverImage", ref message, "CoverImage");
                }

                if (_vendorService.UpdateVendor(ref vendor, ref message))
                {
                    if (vendorEditViewModel.IsApproved == false)
                    {
                        TempData["SuccessMessage"] = "Profile send for approval successfully ...!";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Profile updated successfully ...!";
                    }
                    return Json(new
                    {
                        success = true,
                        message = message
                    });
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }
            ViewBag.CountryID = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text", vendorEditViewModel.CountryID);
            ViewBag.CityID = new SelectList(_cityService.GetCitiesForDropDown(), "value", "text", vendorEditViewModel.CityID);
            ViewBag.ErrorMessage = message;
            return View(vendorEditViewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult SendForApproval(long ID, bool IsApprove)
        {
            string message = string.Empty;
            if (ID < 1)
            {
                return HttpNotFound();
            }
            var vendor = _vendorService.GetVendor(ID);

            if (IsApprove == true)
            {
                vendor.ApprovalStatusID = 2;
                vendor.IsApproved = null;
                if (_vendorService.UpdateVendor(ref vendor, ref message))
                {
                    TempData["SuccessMessage"] = "Profile send for approval successfully ...!";

                    Notification not = new Notification();
                    not.Title = "Vendor Approval";
                    not.Description = "New Vendor " + vendor.Name + " added for approval ";
                    not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                    not.OriginatorName = vendor.Name;
                    not.Url = "/Admin/Vendor/Approvals";
                    not.Module = "Vendor";
                    not.OriginatorType = "Vendor";
                    not.RecordID = vendor.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", not.OriginatorID))
                        {
                        }
                    }
                }
            }
            else
            {
                vendor.ApprovalStatusID = 1;
                vendor.IsApproved = false;
                if (_vendorService.UpdateVendor(ref vendor, ref message))
                {
                    TempData["SuccessMessage"] = "Profile approval canceled successfully ...!";

                    Notification not = new Notification();
                    not.Title = "Vendor Approval";
                    not.Description = "Vendor " + vendor.Name + " canceled the request for approval ";
                    not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                    not.OriginatorName = vendor.Name;
                    //not.Url = "/Admin/Vendor/Approvals";
                    not.Module = "Vendor";
                    not.OriginatorType = "Vendor";
                    not.RecordID = vendor.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", not.OriginatorID))
                        {
                        }
                    }
                }
            }
            return RedirectToAction("ProfileManagement");
        }

        public ActionResult Remarks(long id)
        {
            ViewBag.BuildingID = id;
            var vendor = _vendorService.GetVendor((long)id);

            return View(vendor);
        }

        [AuthorizeVendor]
        public ActionResult CreateDocuments()
        {
            return View();
        }

        [HttpPost]
        [AuthorizeVendor]
        public ActionResult CreateDocuments(string Name, string ExpiryDate)
        {
            if (Name != string.Empty)
            {
                long vendorId = (long)Session["VendorID"];

                string message = string.Empty;
                VendorDocument data = new VendorDocument();
                data.VendorID = vendorId;
                string absolutePath = Server.MapPath("~");
                string relativePath = string.Format("/Assets/AppFiles/Documents/Vendors/{0}/", vendorId.ToString().Replace(" ", "_"));

                data.Name = Name;
                data.Path = Uploader.Uploadpdfandimg(Request.Files, absolutePath, relativePath, "Document", ref message, "FileUpload");
                data.ExpiryDate = Convert.ToDateTime(ExpiryDate);

                if (string.IsNullOrEmpty(data.Path))
                    return Json(new
                    {
                        success = false,
                        message = "Wrong format for Document !",
                        data = data

                    }, JsonRequestBehavior.AllowGet);

                if (Request.Files.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please fill the form correctly",


                    }, JsonRequestBehavior.AllowGet);
                }

                //{
                //	return Json(new
                //	{
                //		success = false,
                //		message = "Please fill the form correctly",


                //	}, JsonRequestBehavior.AllowGet);
                //}

                if (_vendorDocumentService.CreateDocument(ref data, ref message))
                {

                    return Json(new
                    {
                        success = true,
                        message = message,
                        data = data,
                        ExpireDate = GetDate(data.ExpiryDate)

                    }, JsonRequestBehavior.AllowGet);
                }


                return Json(new
                {
                    success = false,
                    message = message,
                    data = data,
                    ExpireDate = GetDate(data.ExpiryDate)

                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = false,
                message = "Please fill the form correctly",


            }, JsonRequestBehavior.AllowGet);
        }
        [AuthorizeVendor]
        public ActionResult DeleteCarDocument(long id)
        {

            string message = string.Empty;
            _vendorDocumentService.DeleteDocument(id, ref message);
            return Json(new { success = true, data = id, message });
        }

        [HttpGet]
        //[AuthorizeVendor]
        public ActionResult GetDocuments(long id)
        {
            var document = _vendorDocumentService.GetDocumentByVendorID(id);


            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                document = document.Select(i => new
                {
                    name = i.Name,
                    path = i.Path,
                    id = i.ID,
                    expiryDate = GetDate(i.ExpiryDate)
                })

            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PackageSubscription()
        {
            if (TempData.ContainsKey("IsPaid"))
                ViewBag.MessagePaid = TempData["IsPaid"].ToString();

            if (TempData.ContainsKey("ErrorMessage"))
                ViewBag.MessagePaidErr = TempData["ErrorMessage"].ToString();

            return View();
        }
        public ActionResult SubscriptionList()
        {
            DateTime currentDateTime = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
            //int noOfDaysLeft = 0, totalNumberOfDays = 0;
            //decimal costPerDay = 0, costForDaysLeft = 0;
            long VendorID = (long)(Session["VendorID"]);
            Vendor vendor = _vendorService.GetVendor(VendorID);
            ViewBag.VendorPackageID = vendor.VendorPackageID == null ? 0 : vendor.VendorPackageID;
            var list = _vendorPackageService.GetAll(true).Where(x => x.IsActive == true).OrderBy(x => x.Price);

            ViewBag.IsExpired = false;
            if (vendor.PackageEndDate.HasValue && currentDateTime > vendor.PackageEndDate)
            {
                ViewBag.IsExpired = true;
            }



            return PartialView(list);
        }
        public ActionResult UpdateVendorPackage(VendorPackageViewModel model)
        {
            DateTime currentDateTime = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
            var list = _vendorPackageService.GetById((int)model.ID);
            string message = string.Empty;
            long VendorID = (long)(Session["VendorID"]);
            Vendor vendor = _vendorService.GetVendor((long)VendorID);

            if (!vendor.VendorPackageID.HasValue)
            {
                vendor.PackageStartDate = currentDateTime;
                vendor.PackageEndDate = currentDateTime.AddMonths(list.MonthCount.Value);
                vendor.VendorPackageID = model.ID;
                if (_vendorService.UpdateVendor(ref vendor, ref message))
                {

                    if (Session["VendorUserID"] == null)
                    {
                        var Vendor = _vendorService.GetVendor(VendorID);

                        if (!Vendor.IsApproved.HasValue || Vendor.IsApproved.Value == false)
                        {
                            return Json(new
                            {
                                success = false,
                                url = "/Vendor/Account/ProfileManagement",
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    return Json(new
                    {
                        success = true,
                        url = "/Vendor/Account/PackageSubscription",
                        id = vendor.VendorPackageID

                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        url = "/Vendor/Account/PackageSubscription",

                    }, JsonRequestBehavior.AllowGet);
                }

            }

            var VendorCurrentPackage = _vendorPackageService.GetById((int)vendor.VendorPackageID);

            if (!vendor.PackageEndDate.HasValue || (vendor.PackageEndDate.Value > currentDateTime && VendorCurrentPackage.Price < list.Price))
            {
                vendor.PackageStartDate = currentDateTime;
                vendor.PackageEndDate = currentDateTime.AddMonths(list.MonthCount.Value);
                vendor.VendorPackageID = model.ID;
                if (_vendorService.UpdateVendor(ref vendor, ref message))
                {

                    if (Session["VendorUserID"] == null)
                    {
                        var Vendor = _vendorService.GetVendor(VendorID);

                        if (!Vendor.IsApproved.HasValue || Vendor.IsApproved.Value == false)
                        {
                            return Json(new
                            {
                                success = true,
                                url = "/Vendor/Account/ProfileManagement"

                            }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    return Json(new
                    {
                        success = true,
                        url = "/Vendor/Account/PackageSubscription",
                        id = vendor.VendorPackageID

                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Vendor package not updated...!",

                    }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                return Json(new
                {
                    success = false,
                    url = "/Vendor/Account/PackageSubscription",

                }, JsonRequestBehavior.AllowGet);
            }


        }
        public ActionResult BuyNow(long id)
        {
            VendorPackageViewModel result = new VendorPackageViewModel();
            long VendorID = (long)(Session["VendorID"]);
            DateTime currentDateTime = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
            int PropertyListingCount = _propertyService.GetVendorLimit(VendorID);
            int CarListingCount = _carService.GetVendorLimit(VendorID);
            int noOfDaysLeft = 0, totalNumberOfDays = 0;
            decimal costPerDay = 0, costForDaysLeft = 0, packageCost = 0;
            Vendor vendor = _vendorService.GetVendor((long)VendorID);
            ViewBag.VendorPackageID = vendor.VendorPackageID == null ? 0 : vendor.VendorPackageID;
            var list = _vendorPackageService.GetAll(true).Where(x => x.IsActive == true && x.ID == id).FirstOrDefault();
            var VendorPackage = _vendorPackageService.GetAll().Where(x =>  x.ID == vendor.VendorPackageID).FirstOrDefault();

            if (vendor.PackageEndDate.HasValue && vendor.PackageEndDate.Value > currentDateTime)
            {
                noOfDaysLeft = (vendor.PackageEndDate.Value.Date - currentDateTime.Date).Days;
                totalNumberOfDays = (vendor.PackageEndDate.Value - vendor.PackageStartDate.Value).Days;
                costPerDay = VendorPackage.Price.Value / totalNumberOfDays;
                costForDaysLeft = noOfDaysLeft * costPerDay;

            }
            packageCost = list.Price.Value - costForDaysLeft;
            packageCost = packageCost < 0 ? list.Price.Value : packageCost;
            //1/7 < 3/7

            if (vendor.PackageEndDate.HasValue && vendor.PackageEndDate.Value.Date > Helpers.TimeZone.GetLocalDateTime().Date)
            {
                if ((noOfDaysLeft > 0 && VendorPackage.Price.Value > list.Price.Value) ||
                (vendor.VendorPackageID.HasValue && list.ID == vendor.VendorPackageID) ||
                vendor.VendorPackageID.HasValue && list.IsFree.HasValue && list.IsFree.Value)
                    result.IsAllowed = false;
            }


            //if (list.PropertyLimit.HasValue && PropertyListingCount > list.PropertyLimit)
            //{
            //    result.PropOverflowMessage = "* Your current property listing count exceeds the listing limit for this package, delete atleast " + (PropertyListingCount - list.PropertyLimit).ToString() +
            //        " properties to aquire this package";

            //    result.IsAllowed = false;
            //}

            //if (list.MotorLimit.HasValue && CarListingCount > list.MotorLimit)
            //{
            //    result.CarOverflowMessage = "* Your current motor listing count exceeds the listing limit for this package, delete atleast " + (CarListingCount - list.MotorLimit).ToString() +
            //        " motors to aquire this package";

            //    result.IsAllowed = false;
            //}

            result = MapVendorPackages(list, result);

            result.CostForDaysLeft = costForDaysLeft.ToString("n2");
            result.CostPerDay = costPerDay.ToString("n2");
            result.TotalNumberOfDays = totalNumberOfDays;
            result.NoOfDaysLeft = noOfDaysLeft;
            result.PriceToPay = packageCost.ToString("n2");
            result.PackagePrice = list.Price.Value;

            return View(result);
        }
        public VendorPackageViewModel MapVendorPackages(VendorPackage Model, VendorPackageViewModel result)
        {
            result.ID = Model.ID;
            result.Name = Model.Name;
            result.NameAr = Model.NameAr;
            result.Price = Model.Price;
            result.Description = Model.Description;
            result.DescriptionAr = Model.DescriptionAr;
            result.BillingPeriod = Model.BillingPeriod;
            result.hasMotorModule = Model.hasMotorModule;
            result.hasPropertyModule = Model.hasPropertyModule;
            result.IsActive = Model.IsActive;
            result.IsDeleted = Model.IsDeleted;
            result.CreatedOn = Model.CreatedOn;
            result.MotorLimit = Model.MotorLimit;
            result.PropertyLimit = Model.PropertyLimit;
            result.IsFree = Model.IsFree;
            result.MonthCount = Model.MonthCount;

            return result;
        }
        [HttpPost]
        public ActionResult Pay(VendorPackageViewModel model)
        {
            VendorPackageViewModel InvoiceDetails = new VendorPackageViewModel();
            VendorTransactionHistory transactionHistory = new VendorTransactionHistory();
            string message = string.Empty;
            try
            {
                var Package = _vendorPackageService.GetById((int)model.ID);
                if (Package != null)
                {
                    try
                    {
                        InvoiceDetails = Calculate(model.ID);
                        long VendorID = (long)(Session["VendorID"]);
                        Vendor vendor = _vendorService.GetVendor((long)VendorID);

                        if ((InvoiceDetails.NoOfDaysLeft > 0 && InvoiceDetails.CurrentPackagePrice < InvoiceDetails.DemandedPackagePrice) || InvoiceDetails.NoOfDaysLeft == 0 ||
                            !vendor.VendorPackageID.HasValue)
                        {

                            if (!vendor.VendorPackageID.HasValue && (Package.IsFree.HasValue && Package.IsFree.Value))
                            {
                                transactionHistory.IsSuccess = true;
                                transactionHistory.Status = "Free";
                                transactionHistory.VendorID = VendorID;
                                transactionHistory.VendorPackageID = Package.ID;
                                transactionHistory.Price = 0;
                                transactionHistory.CompensationAmount = 0;
                                transactionHistory.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();

                                if (_vendorTransactionHistory.AddVendorTransactionHistory(transactionHistory, ref message))
                                {
                                    vendor.VendorPackageID = Package.ID;
                                    vendor.PackageStartDate = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
                                    vendor.PackageEndDate = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime().AddMonths(Package.MonthCount.Value);

                                    _vendorService.UpdateVendor(ref vendor, ref message);
                                }

                                if (!vendor.IsApproved.HasValue || vendor.IsApproved.Value == false)
                                {
                                    return Json(new
                                    {
                                        success = true,
                                        url = "/Vendor/Account/ProfileManagement",
                                    }, JsonRequestBehavior.AllowGet);
                                }

                                return Json(new
                                {
                                    success = true,
                                    url = "/Vendor/Dashboard/Index",
                                }, JsonRequestBehavior.AllowGet);
                            }

                            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                            string TestAPIKey = "rLtt6JWvbUHDDhsZnfpAhpYk4dxYDQkbcPTyGaKp2TYqQgG7FGZ5Th_WD53Oq8Ebz6A53njUoo1w3pjU1D4vs_ZMqFiz_j0urb_BH9Oq9VZoKFoJEDAbRZepGcQanImyYrry7Kt6MnMdgfG5jn4HngWoRdKduNNyP4kzcp3mRv7x00ahkm9LAK7ZRieg7k1PDAnBIOG3EyVSJ5kK4WLMvYr7sCwHbHcu4A5WwelxYK0GMJy37bNAarSJDFQsJ2ZvJjvMDmfWwDVFEVe_5tOomfVNt6bOg9mexbGjMrnHBnKnZR1vQbBtQieDlQepzTZMuQrSuKn-t5XZM7V6fCW7oP-uXGX-sMOajeX65JOf6XVpk29DP6ro8WTAflCDANC193yof8-f5_EYY-3hXhJj7RBXmizDpneEQDSaSz5sFk0sV5qPcARJ9zGG73vuGFyenjPPmtDtXtpx35A-BVcOSBYVIWe9kndG3nclfefjKEuZ3m4jL9Gg1h2JBvmXSMYiZtp9MR5I6pvbvylU_PP5xJFSjVTIz7IQSjcVGO41npnwIxRXNRxFOdIUHn0tjQ-7LwvEcTXyPsHXcMD8WtgBh-wxR8aKX7WPSsT1O8d8reb2aR7K3rkV3K82K_0OgawImEpwSvp9MNKynEAJQS6ZHe_J_l77652xwPNxMRTMASk1ZsJL";

                            var myFatoorahSetting = _myFatoorahPaymentGatewaySettingsService.GetDefaultPaymentGatewaySetting();

                            string baseURL = myFatoorahSetting.IsLive.HasValue && myFatoorahSetting.IsLive.Value ? myFatoorahSetting.LiveEndpoint : myFatoorahSetting.TestEndpoint;
                            string token = myFatoorahSetting.IsLive.HasValue && myFatoorahSetting.IsLive.Value ? myFatoorahSetting.APIKey : TestAPIKey;

                            string url = baseURL + "/v2/InitiatePayment";
                            int? PaymentMethodId = null;

                            using (var client = new HttpClient())
                            {

                                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token);
                                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                                var orderBody = new
                                {
                                    InvoiceAmount = Convert.ToDecimal(InvoiceDetails.PriceToPay),
                                    CurrencyIso = "AED"
                                };

                                var json = JsonConvert.SerializeObject(orderBody);

                                var content = new StringContent(json, Encoding.UTF8, "application/json");
                                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                                var response = client.PostAsync(url, content).Result;
                                if (response.IsSuccessStatusCode)
                                {
                                    var initiatePaymentResponse = JsonConvert.DeserializeObject<InitiatePaymentResponse>(response.Content.ReadAsStringAsync().Result);
                                    if (initiatePaymentResponse != null)
                                    {
                                        var PaymentMethod = initiatePaymentResponse.Data.PaymentMethods.Where(i => i.PaymentMethodCode == "uaecc").FirstOrDefault();
                                        if (initiatePaymentResponse != null)
                                        {
                                            PaymentMethodId = PaymentMethod.PaymentMethodId;
                                        }
                                    }
                                }
                            }

                            if (PaymentMethodId == null)
                            {
                                //return RedirectToAction(nameof(PackageSubscription));
                                return Json(new
                                {
                                    success = false,
                                    message = "Oops! Something went wrong while processing for payment. Please try later."
                                });
                            }

                            /*Create Order on MyFatoorah*/
                            url = baseURL + "/v2/ExecutePayment";

                            using (var client = new HttpClient())
                            {
                                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token);
                                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                                var CallBackUrl = string.Format("Vendor/Account/Paid/{0}?CompensationAmount={1}", Package.ID, Convert.ToDecimal(InvoiceDetails.CostForDaysLeft));
                                var ErrorUrl = string.Format("Vendor/Account/Paid/{0}?CompensationAmount={1}", Package.ID, Convert.ToDecimal(InvoiceDetails.CostForDaysLeft));

                                var orderBody = new NowBuySell.Web.Helpers.Payments.MyFatoorah.ExecutePaymentRequest()
                                {
                                    PaymentMethodId = PaymentMethodId.Value,
                                    CustomerName = vendor.Name,
                                    CustomerMobile = vendor.Contact.Replace("971", ""),
                                    CustomerEmail = vendor.Email,
                                    CustomerAddress = new CustomerAddress()
                                    {
                                        Street = vendor.Address
                                    },
                                    DisplayCurrencyIso = "AED",
                                    MobileCountryCode = "+971",
                                    InvoiceValue = Convert.ToDecimal(InvoiceDetails.PriceToPay),
                                    CallBackUrl = CustomURL.GetFormatedURL(CallBackUrl),
                                    ErrorUrl = CustomURL.GetFormatedURL(ErrorUrl),
                                    Language = "En"
                                };

                                var json = JsonConvert.SerializeObject(orderBody);

                                var content = new StringContent(json, Encoding.UTF8, "application/json");
                                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                                var response = client.PostAsync(url, content).Result;
                                if (response.IsSuccessStatusCode)
                                {
                                    var orderResponse = JsonConvert.DeserializeObject<ExecutePaymentReponse>(response.Content.ReadAsStringAsync().Result);

                                    return Json(new
                                    {
                                        success = true,
                                        url = orderResponse.Data.PaymentURL,
                                        message = "Processing for payment ..."
                                        //OrderStatus = Package.OrderStatus,
                                        //order = new
                                        //{
                                        //    id = Package.ID,
                                        //    OrderNo = Package.OrderNo
                                        //}
                                    }, JsonRequestBehavior.AllowGet);


                                    //string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                }
                                else
                                {
                                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                }
                            }
                        }
                        else
                        {
                            return Json(new
                            {
                                success = false,
                                message = "You cannot downgrage on a package untill your current package is expired!"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Oops! Something went wrong while processing for payment. Please try later."
                        });
                    }
                    return Json(new
                    {
                        success = false,
                        message = "Oops! Something went wrong. Please try later."
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Order Not Found"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }
        private VendorPackageViewModel Calculate(long id)
        {
            VendorPackageViewModel result = new VendorPackageViewModel();
            DateTime currentDateTime = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
            int noOfDaysLeft = 0, totalNumberOfDays = 0;
            decimal costPerDay = 0, costForDaysLeft = 0, packageCost = 0;
            long test = (long)(Session["VendorID"]);
            Vendor vendor = _vendorService.GetVendor((long)test);
            ViewBag.VendorPackageID = vendor.VendorPackageID == null ? 0 : vendor.VendorPackageID;
            var list = _vendorPackageService.GetAll(true).Where(x => x.IsActive == true && x.ID == id).FirstOrDefault();
            var VendorPackage = _vendorPackageService.GetAll().Where(x => x.ID == vendor.VendorPackageID).FirstOrDefault();

            if (vendor.PackageEndDate.HasValue && vendor.PackageEndDate.Value > currentDateTime)
            {
                noOfDaysLeft = (vendor.PackageEndDate.Value - currentDateTime).Days;
                totalNumberOfDays = (vendor.PackageEndDate.Value - vendor.PackageStartDate.Value).Days;
                costPerDay = VendorPackage.Price.Value / totalNumberOfDays;
                costForDaysLeft = noOfDaysLeft * costPerDay;

            }
            packageCost = list.Price.Value - costForDaysLeft;
            packageCost = packageCost < 0 ? list.Price.Value : packageCost;

            result = MapVendorPackages(list, result);

            result.CostForDaysLeft = costForDaysLeft.ToString("n2");
            result.CostPerDay = costPerDay.ToString("n2");
            result.TotalNumberOfDays = totalNumberOfDays;
            result.NoOfDaysLeft = noOfDaysLeft;
            result.PriceToPay = packageCost.ToString("n2");
            result.CurrentPackagePrice = VendorPackage != null ? VendorPackage.Price.Value : 0;
            result.DemandedPackagePrice = list.Price.Value;

            return result;
        }

        public ActionResult Paid(long id, decimal CompensationAmount)
        {
            VendorTransactionHistory transactionHistory = new VendorTransactionHistory();
            try
            {
                int vendorId = Convert.ToInt32(Session["VendorID"]);
                var Package = _vendorPackageService.GetById((int)id);
                Vendor vendorModel = _vendorService.GetVendor(vendorId);
                if (Package != null)
                {
                    string PaymentId = Request.QueryString["PaymentId"].ToString();

                    if (!string.IsNullOrEmpty(PaymentId))
                    {
                        try
                        {
                            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                            string TestAPIKey = "rLtt6JWvbUHDDhsZnfpAhpYk4dxYDQkbcPTyGaKp2TYqQgG7FGZ5Th_WD53Oq8Ebz6A53njUoo1w3pjU1D4vs_ZMqFiz_j0urb_BH9Oq9VZoKFoJEDAbR" +
                                "ZepGcQanImyYrry7Kt6MnMdgfG5jn4HngWoRdKduNNyP4kzcp3mRv7x00ahkm9LAK7ZRieg7k1PDAnBIOG3EyVSJ5kK4WLMvYr7sCwHbHcu4A5WwelxYK0GMJy37bNAarSJDFQsJ2ZvJj" +
                                "vMDmfWwDVFEVe_5tOomfVNt6bOg9mexbGjMrnHBnKnZR1vQbBtQieDlQepzTZMuQrSuKn-t5XZM7V6fCW7oP-uXGX-sMOajeX65JOf6XVpk29DP6ro8WTAflCDANC193yof8-f5_EY" +
                                "Y-3hXhJj7RBXmizDpneEQDSaSz5sFk0sV5qPcARJ9zGG73vuGFyenjPPmtDtXtpx35A-BVcOSBYVIWe9kndG3nclfefjKEuZ3m4jL9Gg1h2JBvmXSMYiZtp9MR5I6pvbvylU_PP5xJF" +
                                "SjVTIz7IQSjcVGO41npnwIxRXNRxFOdIUHn0tjQ-7LwvEcTXyPsHXcMD8WtgBh-wxR8aKX7WPSsT1O8d8reb2aR7K3rkV3K82K_0OgawImEpwSvp9MNKynEAJQS6ZHe_J_l77652xwPNxMRTMASk1ZsJL";

                            var myFatoorahSetting = _myFatoorahPaymentGatewaySettingsService.GetDefaultPaymentGatewaySetting();

                            string Endpoint = myFatoorahSetting.IsLive.HasValue && myFatoorahSetting.IsLive.Value ? myFatoorahSetting.LiveEndpoint : myFatoorahSetting.TestEndpoint;
                            string APIKey = myFatoorahSetting.IsLive.HasValue && myFatoorahSetting.IsLive.Value ? myFatoorahSetting.APIKey : TestAPIKey;

                            using (var client = new HttpClient())
                            {
                                /*Fetch  Access Token From N-Genius*/
                                var body = new
                                {
                                    Key = PaymentId,
                                    KeyType = "PaymentId"
                                };

                                client.BaseAddress = new Uri(Endpoint);
                                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + APIKey);

                                var json = JsonConvert.SerializeObject(body);

                                var content = new StringContent(json, Encoding.UTF8, "application/json");
                                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                                var response = client.PostAsync("/v2/GetPaymentStatus", content).Result;

                                if (response.IsSuccessStatusCode)
                                {
                                    var paymentResponse = JsonConvert.DeserializeObject<NowBuySell.Web.Helpers.Payments.MyFatoorah.Capture.PaymentInquiryResponse>(response.Content.ReadAsStringAsync().Result);
                                    if (paymentResponse.IsSuccess)
                                    {
                                        string message = string.Empty;

                                        var Payment = paymentResponse.Data;

                                        if ((Payment != null && Payment.InvoiceStatus == "Paid"))
                                        {
                                            var InvoiceTransaction = Payment.InvoiceTransactions.Where(i => i.TransactionStatus == "Succss").OrderByDescending(i => i.TransactionDate).FirstOrDefault();

                                            transactionHistory.IsSuccess = true;
                                            transactionHistory.Status = Payment.InvoiceStatus;
                                            transactionHistory.VendorID = vendorId;
                                            transactionHistory.VendorPackageID = id;
                                            transactionHistory.Price = Convert.ToDecimal(Payment.InvoiceValue);
                                            transactionHistory.CompensationAmount = CompensationAmount;
                                            transactionHistory.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();

                                            if (_vendorTransactionHistory.AddVendorTransactionHistory(transactionHistory, ref message))
                                            {
                                                Vendor vendor = new Vendor();

                                                vendor = _vendorService.GetVendor(vendorId);

                                                vendor.VendorPackageID = id;
                                                vendor.PackageStartDate = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
                                                vendor.PackageEndDate = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime().AddMonths(Package.MonthCount.Value);

                                                if (_vendorService.UpdateVendor(ref vendor, ref message))
                                                {
                                                    TempData["IsPaid"] = string.Format("Payment of AED {0} recieved against your selected package", Payment.InvoiceValue);
                                                }

                                                Transaction transaction = new Transaction()
                                                {
                                                    VendorID = vendorId,
                                                    PaymentRef = vendorModel.VendorCode,
                                                    NameOnCard = Payment.CustomerName,
                                                    MaskCardNo = InvoiceTransaction.CardNumber,
                                                    TransactionStatus = InvoiceTransaction.TransactionStatus,
                                                    Amount = Convert.ToDecimal(InvoiceTransaction.TransationValue)
                                                };

                                                _transactionService.CreateTransaction(transaction, ref message);

                                                ActivateVendorListing(vendorId);

                                                Notification not = new Notification();
                                                not.Title = "Package Invoiced";
                                                not.Description = string.Format("Order {0} payment recieved against package from {1}", Package.Name, vendor.Name);
                                                not.Url = "/Admin/Transaction/Index";
                                                not.Module = "Invoiced";
                                                not.OriginatorType = "Vendor";
                                                not.RecordID = Package.ID;

                                                if (_notificationService.CreateNotification(not, ref message))
                                                {
                                                    if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", null))
                                                    {
                                                    }
                                                }

                                                if (!vendor.IsApproved.HasValue || vendor.IsApproved.Value == false)
                                                {
                                                    return RedirectPermanent("/Vendor/Account/ProfileManagement");
                                                }

                                                TempData["PaymentSuccessMessage"] = string.Format("Payment against Order # {0} CAPTURED", Package.Name);
                                                return RedirectPermanent("/Vendor/Account/PackageSubscription");
                                            }
                                            else
                                            {
                                                TempData["ErrorMessage"] = "Oops! Something went wrong. Please try later.";
                                                return RedirectPermanent("/Vendor/Account/PackageSubscription");
                                            }
                                        }
                                        else
                                        {
                                            var InvoiceTransaction = Payment.InvoiceTransactions.Where(i => i.TransactionStatus == "Failed").OrderByDescending(i => i.TransactionDate).FirstOrDefault();

                                            if (InvoiceTransaction != null)
                                            {
                                                transactionHistory.IsSuccess = false;
                                                transactionHistory.Status = "Failed";
                                                transactionHistory.VendorID = vendorId;
                                                transactionHistory.VendorPackageID = id;
                                                transactionHistory.Price = Convert.ToDecimal(Payment.InvoiceValue);
                                                transactionHistory.CompensationAmount = CompensationAmount;
                                                transactionHistory.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();

                                                if (_vendorTransactionHistory.AddVendorTransactionHistory(transactionHistory, ref message))
                                                {
                                                    TempData["ErrorMessage"] = "Oops! Transaction Failed. Please try again.";
                                                    return RedirectPermanent("/Vendor/Account/PackageSubscription");
                                                }
                                                else
                                                {
                                                    TempData["ErrorMessage"] = "Oops! Something went wrong while processing for payment. Please try later.";
                                                    return RedirectPermanent("/Vendor/Account/PackageSubscription");
                                                }
                                            }
                                            else
                                            {
                                                //Package.IsPaymentCaptured = true;

                                                //if (_orderService.UpdateOrder(ref Package, ref message))
                                                //{
                                                TempData["ErrorMessage"] = "Your payment is in process.";
                                                return RedirectPermanent("/Vendor/Account/PackageSubscription");
                                                //}
                                                //else
                                                //{
                                                //    TempData["ErrorMessage"] = "Oops! Something went wrong while processing for payment. Please try later.";
                                                //    return RedirectPermanent("/Customer/Dashboard/Index/" + id + "#orders");
                                                //}
                                            }
                                        }
                                    }
                                    else
                                    {
                                        TempData["ErrorMessage"] = "Oops! Something went wrong while processing for payment. Please try later.";
                                        return RedirectPermanent("/Vendor/Account/PackageSubscription");
                                    }
                                }
                                else
                                {
                                    TempData["ErrorMessage"] = "Oops! Something went wrong while processing for payment. Please try later.";
                                    return RedirectPermanent("/Vendor/Account/PackageSubscription");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = "Oops! Something went wrong while processing for payment. Please try later.";
                            return RedirectPermanent("/Vendor/Account/PackageSubscription");
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Oops! Something went wrong while processing for payment. Please try later.";
                        return RedirectPermanent("/Vendor/Account/PackageSubscription");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Order Not Found";
                    return RedirectPermanent("/Vendor/Account/PackageSubscription");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Oops! Something went wrong. Please try later.";
                return RedirectPermanent("/Vendor/Account/PackageSubscription");
            }
        }

        private string GetDate(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "-";

            string Month = dateTime.Value.ToString("MMM");

            return dateTime.Value.Day + " " + Month + " " + dateTime.Value.Year;
        }

        private void ActivateVendorListing(long VendorID)
        {
            string message = string.Empty;
            IEnumerable<Property> propListing = _propertyService.GetPropertiesByVendor((int)VendorID);
            IEnumerable<Car> carListing = _carService.GetCarsByVendor((int)VendorID);

            foreach (var property in propListing)
            {
                Property prop = property;
                prop.IsVendorAccountDown = false;

                _propertyService.UpdateProperty(ref prop, ref message);
            }

            foreach (var car in carListing)
            {
                Car car2 = car;
                car2.IsVendorAccountDown = false;

                _carSerivce.UpdateCar(ref car2, ref message);
            }
        }
    }
}
