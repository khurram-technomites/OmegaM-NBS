using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Service.Helpers;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Vendor;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Globalization;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class VendorController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;
        private readonly IVendorUserRoleService _vendorUserRoleService;
        private readonly IVendorService _vendorService;
        private readonly IVendorUserService _vendorUserService;
        private readonly ICountryService _countryService;
        private readonly ICityService _cityService;
        private readonly IMail _email;
        private readonly INumberRangeService _numberRangeService;
        private readonly IVendorWalletShareService _VendorWalletShareService;
        private readonly IVendorDocumentService _vendorDocumentService;
        private readonly INotificationService _notificationService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly IVendorPackagesService _vendorPackageService;
        private readonly IVendorTransactionHistoryService _vendorTransactionHistoryService;

        public VendorController(IVendorService vendorService, ICountryService countryService, ICityService cityService, IVendorUserRoleService vendorUserRoleService, IVendorUserService vendorUserService, IMail email, INumberRangeService numberRangeService, IVendorWalletShareService VendorWalletShareService, IVendorDocumentService vendorDocumentService, INotificationService notificationService, INotificationReceiverService notificationReceiverService, IVendorPackagesService vendorPackageService, IVendorTransactionHistoryService vendorTransactionHistoryService)
        {
            this._email = email;
            this._numberRangeService = numberRangeService;
            this._vendorUserService = vendorUserService;
            this._vendorUserRoleService = vendorUserRoleService;
            this._vendorService = vendorService;
            this._countryService = countryService;
            this._cityService = cityService;
            this._VendorWalletShareService = VendorWalletShareService;
            this._vendorDocumentService = vendorDocumentService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
            _vendorTransactionHistoryService = vendorTransactionHistoryService;

            _vendorPackageService = vendorPackageService;
        }

        #region Vendor

        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        public ActionResult List()
        {
            var vendors = _vendorService.GetVendors(true);
            return PartialView(vendors);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendor vendor = _vendorService.GetVendor((Int16)id);
            vendor.Contact = !string.IsNullOrEmpty(vendor.Contact) ? vendor.Contact.Replace("971", "") : "-";
            vendor.Contact = !string.IsNullOrEmpty(vendor.Mobile) ? vendor.Mobile.Replace("971", "") : "-";

            if (vendor.OpeningTime.HasValue)
            {
                DateTime dt = DateTime.Today.Add(vendor.OpeningTime.Value);
                ViewBag.OpeningTime = String.Format("{0:d/M/yyyy hh:mm:ss}", dt.ToShortTimeString());
            }
            else
            {
                ViewBag.OpeningTime = "-";
            }

            if (vendor.ClosingTime.HasValue)
            {
                DateTime dt2 = DateTime.Today.Add(vendor.ClosingTime.Value);
                ViewBag.ClosingTime = String.Format("{0:d/M/yyyy hh:mm:ss}", dt2.ToShortTimeString());
            }
            else
            {
                ViewBag.ClosingTime = "-";
            }

            if (vendor == null)
            {
                return HttpNotFound();
            }
            return View(vendor);
        }

        public ActionResult Create()
        {
            VendorFormViewModel model = new VendorFormViewModel();

            ViewBag.CountryID = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text");
            ViewBag.CityID = new SelectList(_cityService.GetCitiesForDropDown(), "value", "text");
            ViewBag.VendorPackageID = new SelectList(_vendorPackageService.GetPackagesForDropDown(), "value", "text");

            ViewBag.VendorCode = _numberRangeService.GetNextValueFromNumberRangeByName("VENDOR");

            return View(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(VendorFormViewModel vendorFormViewModel)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_vendorUserService.GetUserByEmail(vendorFormViewModel.UserEmail) == null)
                {
                    var vendor = new Vendor();

                    vendor.VendorCode = vendorFormViewModel.VendorCode;
                    vendor.Name = vendorFormViewModel.Name;
                    vendor.NameAr = vendorFormViewModel.NameAr;
                    vendor.Slug = vendorFormViewModel.Slug;
                    vendor.Email = vendorFormViewModel.Email;
                    vendor.Logo = vendorFormViewModel.Logo;
                    vendor.Contact = "+971" + vendorFormViewModel.Contact;
                    vendor.Mobile = "+971" + vendorFormViewModel.Mobile;
                    vendor.Address = vendorFormViewModel.Address;
                    vendor.IDNo = vendorFormViewModel.IDNo;
                    vendor.TRN = vendorFormViewModel.TRN;
                    vendor.Website = vendorFormViewModel.Website;
                    vendor.Commission = vendorFormViewModel.Commission;
                    vendor.License = vendorFormViewModel.License;
                    vendor.IsSocialManagment = vendorFormViewModel.IsSocialManagment;
                    int PermitNo;
                    if (int.TryParse(vendorFormViewModel.License, out PermitNo)) ;
                    {
                        vendor.PermitNo = PermitNo;
                    }

                    vendor.FAX = vendorFormViewModel.FAX;
                    vendor.About = vendorFormViewModel.About;
                    vendor.AboutAr = vendorFormViewModel.AboutAr;
                    vendor.CountryID = vendorFormViewModel.CountryID;
                    vendor.CityID = vendorFormViewModel.CityID;
                    vendor.OpeningTime = DateTime.Parse(vendorFormViewModel.OpeningTime, new CultureInfo("en-US")).TimeOfDay;
                    vendor.ClosingTime = DateTime.Parse(vendorFormViewModel.ClosingTime, new CultureInfo("en-US")).TimeOfDay;
                    vendor.Longitude = vendorFormViewModel.Longitude;
                    vendor.Latitude = vendorFormViewModel.Latitude;
                    vendor.TermAndConditionWebAr = vendorFormViewModel.TermsAndConditionWebAr;
                    vendor.TermAndConditionWebEn = vendorFormViewModel.TermsAndConditionWebEn;
                    vendor.ServingKilometer = vendorFormViewModel.ServingKilometer;
                    vendor.VendorPackageID = vendorFormViewModel.VendorPackageID;
                    vendor.PermitNo = vendorFormViewModel.PermitNo;
                    vendor.DEDNo = vendorFormViewModel.DEDNo;
                    vendor.RERANo = vendorFormViewModel.RERANo;
                    vendor.PassportNo = vendorFormViewModel.PassportNo;
                    vendor.Whatsapp = vendorFormViewModel.Whatsapp != "" && vendorFormViewModel.Whatsapp != null ? "+971" + vendorFormViewModel.Whatsapp:null;
                    vendor.LinkedIn = vendorFormViewModel.LinkedIn;
                    vendor.Snapchat = vendorFormViewModel.Snapchat;
                    vendor.Twitter = vendorFormViewModel.Twitter;
                    vendor.Youtube = vendorFormViewModel.Youtube;
                    vendor.Instagram = vendorFormViewModel.Instagram;
                    vendor.Facebook = vendorFormViewModel.Facebook;
                    vendor.TikTok = vendorFormViewModel.TikTok;
                    vendor.IsSocialManagment = vendorFormViewModel.IsSocialManagment;
                    //if (vendorFormViewModel.CoverImage != null)
                    //{
                    //	string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/Vendors/"), vendor.VendorCode, "/cover");

                    //	string absolutePath = Server.MapPath("~");
                    //	string relativePath = string.Format("/Assets/AppFiles/Images/Vendors/{0}/", vendor.VendorCode);
                    //	vendor.CoverImage = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "CoverImage", ref message, "CoverImage");
                    //}
                    if (vendorFormViewModel.Logo != null)
                    {
                        string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/Vendors/"), vendor.ID, "/logo");

                        string absolutePath = Server.MapPath("~");
                        string relativePath = string.Format("/Assets/AppFiles/Images/Vendors/{0}/", vendor.ID);

                        vendor.Logo = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "Logo", ref message, "Logo");
                    }

                    vendor.IsApproved = true;

                    if (string.IsNullOrEmpty(vendor.Logo))
                        vendor.Logo = "/assets/images/vendor/Dafault-vendor-logo.png";

                    if (_vendorService.CreateVendor(ref vendor, ref message))
                    {
                        //long i;
                        long VendorID = vendor.ID;
                        string UserEmail = vendorFormViewModel.UserEmail;
                        string UserPassword = vendorFormViewModel.UserPassword;
                        string Role = "Administrator";
                        var UserRoleID = _vendorUserRoleService.GetVendorUserRoleByName(Role);
                        var vendoruser = new VendorUser();

                        vendoruser.VendorID = VendorID;
                        vendoruser.Name = vendor.Name;
                        vendoruser.MobileNo = vendor.Mobile;
                        vendoruser.EmailAddress = UserEmail;
                        vendoruser.Password = UserPassword;
                        vendoruser.UserRoleID = UserRoleID.ID;

                        if (_vendorUserService.CreateVendorUser(vendoruser, ref message, true))
                        {
                            var path = Server.MapPath("~/");
                            if (_email.SendVendorCreationMail(vendorFormViewModel.Name, vendorFormViewModel.Email, vendoruser.EmailAddress, UserPassword, CustomURL.GetFormatedURL("/Vendor/Account/Login"), path))
                            {
                                vendor.IsEmailSent = true;
                                if (_vendorService.UpdateVendor(ref vendor, ref message))
                                {

                                }
                            }
                            VendorUserRole vendorUserRole = new VendorUserRole();
                            vendorUserRole.VendorID = VendorID;
                            vendorUserRole.Name = "Lead Manager";

                            if (_vendorUserRoleService.CreateVendorUserRole(vendorUserRole, ref message))
                            {
                                string directory = Server.MapPath(string.Format("/AuthorizationProvider/Privileges/Vendor/{0}/", VendorID));
                                Directory.CreateDirectory(directory);
                                TextWriter textWriter = new StreamWriter(directory + vendorUserRole.Name + ".txt");
                                textWriter.Close();
                            }
                            VendorWalletShare VendorHistory = new VendorWalletShare();
                            VendorHistory.VendorID = VendorID;
                            VendorHistory.PendingAmount = 0;
                            VendorHistory.TotalEarning = 0;
                            VendorHistory.TotalEarning = 0;
                            if (_VendorWalletShareService.CreateVendorWalletShare(VendorHistory, ref message))
                            {

                            }
                            return RedirectToAction("Index");
                        }
                        TempData["SuccessMessage"] = message;
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    message = "Vendor user account already exists ...";
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }

            ViewBag.CountryID = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text", vendorFormViewModel.CountryID);
            ViewBag.CityID = new SelectList(_cityService.GetCitiesForDropDown(), "value", "text", vendorFormViewModel.CityID);
            ViewBag.VendorPackageID = new SelectList(_vendorPackageService.GetPackagesForDropDown(), "value", "text");
            ViewBag.ErrorMessage = message;
            ViewBag.VendorCode = vendorFormViewModel.VendorCode;
            return View(vendorFormViewModel);
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendor vendor = _vendorService.GetVendor((long)id);
            if (vendor == null)
            {
                return HttpNotFound();
            }

            ViewBag.CountryID = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text", vendor.CountryID);
            ViewBag.CityID = new SelectList(_cityService.GetCitiesForDropDown(), "value", "text", vendor.CityID);
            ViewBag.VendorPackageID = new SelectList(_vendorPackageService.GetPackagesForDropDown(), "value", "text");
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
                CountryID = vendor.CountryID,
                CityID = vendor.CityID,
                Logo = vendor.Logo,
                CoverImage = vendor.CoverImage,
                Latitude = vendor.Latitude,
                Longitude = vendor.Longitude,
                TermsAndConditionWebAr = vendor.TermAndConditionWebAr,
                TermsAndConditionWebEn = vendor.TermAndConditionWebEn,
                OpeningTime = vendor.OpeningTime.HasValue ? vendor.OpeningTime.Value.ToString() : null,
                ClosingTime = vendor.ClosingTime.HasValue ? vendor.ClosingTime.Value.ToString() : null,
                ServingKilometer = vendor.ServingKilometer.HasValue? (double)vendor.ServingKilometer : 0,
                VendorPackageID = (int)vendor.VendorPackageID.Value,
                PassportNo = vendor.PassportNo,
                Whatsapp = string.IsNullOrEmpty(vendor.Whatsapp) ? "" : vendor.Whatsapp.Replace("971", ""),
                LinkedIn = vendor.LinkedIn,
                Snapchat = vendor.Snapchat,
                Twitter = vendor.Twitter,
                Youtube = vendor.Youtube,
                Instagram = vendor.Instagram,
                Facebook = vendor.Facebook,
                TikTok = vendor.TikTok,
                IsSocialManagment = vendor.IsSocialManagment
        };
            return View(vendorEditViewModel);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit(VendorEditViewModel vendorEditViewModel)
        {
            string message = string.Empty;
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
                vendor.Address = vendorEditViewModel.Address;
                vendor.IDNo = vendorEditViewModel.IDNo;
                vendor.TRN = vendorEditViewModel.TRN;
                vendor.Website = vendorEditViewModel.Website;
                vendor.License = vendorEditViewModel.License;
                vendor.CountryID = vendorEditViewModel.CountryID;
                vendor.CityID = vendorEditViewModel.CityID;
                vendor.OpeningTime = DateTime.Parse(vendorEditViewModel.OpeningTime, new CultureInfo("en-US")).TimeOfDay;
                vendor.ClosingTime = DateTime.Parse(vendorEditViewModel.ClosingTime, new CultureInfo("en-US")).TimeOfDay;
                vendor.Longitude = vendorEditViewModel.Longitude;
                vendor.Latitude = vendorEditViewModel.Latitude;
                vendor.TermAndConditionWebAr = vendorEditViewModel.TermsAndConditionWebAr;
                vendor.TermAndConditionWebEn = vendorEditViewModel.TermsAndConditionWebEn;
                vendor.ServingKilometer = vendorEditViewModel.ServingKilometer;
                vendor.VendorPackageID = vendorEditViewModel.VendorPackageID;
                vendor.PassportNo = vendorEditViewModel.PassportNo;
                vendor.Whatsapp = vendorEditViewModel.Whatsapp != "" && vendorEditViewModel.Whatsapp != null ? "+971" + vendorEditViewModel.Whatsapp : null;
                vendor.LinkedIn = vendorEditViewModel.LinkedIn;
                vendor.Snapchat = vendorEditViewModel.Snapchat;
                vendor.Twitter = vendorEditViewModel.Twitter;
                vendor.Youtube = vendorEditViewModel.Youtube;
                vendor.Instagram = vendorEditViewModel.Instagram;
                vendor.Facebook = vendorEditViewModel.Facebook;
                vendor.TikTok = vendorEditViewModel.TikTok;
                vendor.IsSocialManagment = vendorEditViewModel.IsSocialManagment;

                if (vendor.Logo != vendorEditViewModel.Logo && vendorEditViewModel.Logo != null)
                {
                    string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/Vendors/"), vendor.VendorCode, "/logo");

                    string absolutePath = Server.MapPath("~");
                    string relativePath = string.Format("/Assets/AppFiles/Images/Vendors/{0}/", vendor.ID);

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
                    //if (data.BannerImage != null)
                    //{
                    //    string absolutePath = Server.MapPath("~");
                    //    if (System.IO.File.Exists(imagepath))
                    //    {
                    //        System.IO.File.Delete(imagepath);
                    //    }
                    //}
                    TempData["SuccessMessage"] = message;
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
            ViewBag.VendorPackageID = new SelectList(_vendorPackageService.GetPackagesForDropDown(), "value", "text");
            ViewBag.ErrorMessage = message;
            return View(vendorEditViewModel);
        }

        public ActionResult EmailSent(long? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var vendor = _vendorService.GetVendor((long)id);
                string VendorRole = "Administrator";
                var vendorUser = _vendorUserService.GetUserByRole((long)vendor.ID, "Administrator");
                if (vendor == null)
                {
                    return HttpNotFound();
                }

                if (vendor.IsEmailSent.HasValue)
                {
                    if (!vendor.IsEmailSent.Value)
                        vendor.IsEmailSent = true;
                    else
                    {
                        vendor.IsEmailSent = false;
                    }
                }
                else
                {
                    vendor.IsEmailSent = true;
                }
                string message = string.Empty;
                var path = Server.MapPath("~/");
                if (_email.SendVendorCreationMail(vendor.Name, vendor.Email, vendorUser.EmailAddress, string.Empty, CustomURL.GetFormatedURL("/Vendor/Account/Login"), path))
                {
                    vendor.IsEmailSent = true;
                    if (_vendorService.UpdateVendor(ref vendor, ref message))
                    {

                        return Json(new
                        {
                            success = true,
                            message = "Email Sent Successfully",
                            data = new
                            {
                                ID = vendor.ID,
                                Date = vendor.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                                VendorCode = vendor.Logo + "|" + vendor.VendorCode + "|" + vendor.Name,
                                Email = vendor.Email,
                                IsActive = vendor.IsActive.HasValue ? vendor.IsActive.Value.ToString() : bool.FalseString,

                            }
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    message = "Oops! Something went wrong. Please try later.";
                }

                return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Oops! Something went wrong. Please try later." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var vendor = _vendorService.GetVendor((long)id);
            if (vendor == null)
            {
                return HttpNotFound();
            }

            if (vendor.IsActive.HasValue)
            {
                if (!vendor.IsActive.Value)
                    vendor.IsActive = true;
                else
                {
                    vendor.IsActive = false;
                }
            }
            else
            {
                vendor.IsActive = true;
            }
            string message = string.Empty;
            if (_vendorService.UpdateVendorStatus(vendor, ref message))
            {
                SuccessMessage = "Vendor " + ((bool)vendor.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        ID = vendor.ID,
                        Date = vendor.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        VendorCode = vendor.VendorCode,
                        Name = vendor.Name,
                        Logo = vendor.Logo,
                        Email = vendor.Email,

                        IsActive = vendor.IsActive.HasValue ? vendor.IsActive.Value.ToString() : bool.FalseString,
                        IsEmailSent = vendor.IsEmailSent.HasValue ? vendor.IsEmailSent.Value.ToString() : bool.FalseString,

                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendor vendor = _vendorService.GetVendor((Int16)id);
            if (vendor == null)
            {
                return HttpNotFound();
            }
            TempData["VendorID"] = id;
            return View(vendor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty, altMessage = string.Empty;
            if (_vendorService.DeleteVendor((Int16)id, ref message))
            {
                var users = _vendorUserService.GetVendorUsers(id);

                foreach (var user in users)
                    _vendorUserService.DeleteVendorUser(user.ID, ref altMessage, deleteAdmin: true);

                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateDocuments(long? id)
        {
            ViewBag.VendorID = id;
            Session["VendorIDForDocument"] = ViewBag.VendorID;
            return View();
        }

        [HttpPost]
        public ActionResult CreateDocuments(string Name)
        {
            if (Name != string.Empty)
            {
                long vendorId = (long)Session["VendorIDForDocument"];

                string message = string.Empty;
                VendorDocument data = new VendorDocument();
                data.VendorID = vendorId;
                string absolutePath = Server.MapPath("~");
                string relativePath = string.Format("/Assets/AppFiles/Documents/Vendors/{0}/", vendorId.ToString().Replace(" ", "_"));
                data.Name = Name;
                data.Path = Uploader.UploadDocs(Request.Files, absolutePath, relativePath, "Document", ref message, "FileUpload");

                if (Request.Files.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please fill the form correctly",


                    }, JsonRequestBehavior.AllowGet);
                }
                if (_vendorDocumentService.CreateDocument(ref data, ref message))
                {

                }

                return Json(new
                {
                    success = true,
                    message = "Document added successfully!",
                    name = data.Name,
                    path = data.Path,
                    id = data.ID

                }, JsonRequestBehavior.AllowGet); ;
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Please fill the form correctly!",


                }, JsonRequestBehavior.AllowGet); ;
            }

        }
        [HttpPost, ActionName("DeleteVendorDocument")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCarDocument(long id)
        {

            string message = string.Empty;
            _vendorDocumentService.DeleteDocument(id, ref message);
            return Json(new { success = true, data = id, message });
        }

        [HttpGet]
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
                    id = i.ID
                })

            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VendorTransactionHistory(int VendorID)
        {
            var result = _vendorTransactionHistoryService.GetAllByVendor(VendorID);
            return View(result);
        }

        #endregion


        #region Vendor Approvals
        public ActionResult Approvals()
        {
            //var vendors = _vendorService.GetVendors(false);
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            var vendors = _vendorService.GetVendorsForApproval();
            return View(vendors);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Approve(long? id)
        //{
        //	if (id == null)
        //	{
        //		return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //	}
        //	var vendor = _vendorService.GetVendor((long)id);
        //	if (vendor == null)
        //	{
        //		return HttpNotFound();
        //	}

        //	vendor.IsApproved = true;
        //	vendor.IsActive = true;

        //	string message = string.Empty;
        //	if (_vendorService.UpdateVendor(ref vendor, ref message))
        //	{
        //		SuccessMessage = "Vendor signup request approved successfully ...";
        //		return Json(new
        //		{
        //			success = true,
        //			message = SuccessMessage,
        //		}, JsonRequestBehavior.AllowGet);
        //	}
        //	else
        //	{
        //		ErrorMessage = "Oops! Something went wrong. Please try later.";
        //	}

        //	return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(VendorApprovalFormViewModel vendorApprovalFormViewModel)
        {
            var vendor = _vendorService.GetVendor(vendorApprovalFormViewModel.ID);
            if (vendor == null)
            {
                return HttpNotFound();
            }

            if (vendorApprovalFormViewModel.IsApproved)
            {
                vendor.ApprovalStatusID = 3;
                vendor.IsApproved = true;
                vendor.IsActive = true;
            }
            else
            {
                vendor.ApprovalStatusID = 4;
                vendor.IsApproved = false;
                vendor.IsActive = true;
            }
            vendor.ApprovalStatusID = 4;
            vendor.Remarks = vendor.Remarks + "<hr />" + Helpers.TimeZone.GetLocalDateTime().ToString("dd MMM yyyy, h:mm tt") + "<br />" + vendorApprovalFormViewModel.Remarks;

            string message = string.Empty;

            if (_vendorService.UpdateVendor(ref vendor, ref message, false))
            {

                var vendorUser = _vendorUserService.GetVendorUser(vendor.ID);
                if (vendorUser != null)
                {
                    vendorUser.UserType = null;
                    _vendorUserService.UpdateVendorUser(ref vendorUser, ref message);
                }

                SuccessMessage = "Vendor " + ((bool)vendor.IsApproved ? "Approved" : "Rejected") + "  successfully ...";
                //var vendor = _vendorService.GetVendor((long)vendor.VendorID);

                Notification not = new Notification();
                not.Title = "Vendor Approval";
                not.TitleAr = "موافقة البائع";
                if (vendor.IsApproved == true)
                {
                    not.Description = "Your profile approval have been approved ";
                    not.Url = "/Vendor/Dashboard/Index";
                }
                else
                {
                    not.Description = "Your profile approval have been rejected ";
                    not.Url = "/Vendor/Account/ProfileManagement";
                }
                not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                not.OriginatorName = Session["UserName"].ToString();
                not.Module = "Vendor";
                not.OriginatorType = "Admin";
                not.RecordID = vendor.ID;
                if (_notificationService.CreateNotification(not, ref message))
                {
                    NotificationReceiver notRec = new NotificationReceiver();
                    notRec.ReceiverID = vendor.ID;
                    notRec.ReceiverType = "Vendor";
                    notRec.NotificationID = not.ID;
                    if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                    {
                    }
                }
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        ID = vendor.ID,
                        Date = vendor.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        Vendor = vendor.Logo + "|" + vendor.Name + "|" + vendor.VendorCode,
                        Email = vendor.Email,
                        IsActive = vendor.IsActive.HasValue ? vendor.IsActive.Value.ToString() : bool.FalseString,
                        IsApproved = vendor.IsApproved.HasValue ? vendor.IsApproved.Value.ToString() : bool.FalseString
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Reject(long id)
        {
            ViewBag.BuildingID = id;
            ViewBag.ApprovalStatus = 4;

            var vendor = _vendorService.GetVendor((long)id);

            return View(vendor);
        }

        [HttpGet]
        public ActionResult Approve(VendorApprovalFormViewModel vendorApprovalFormViewModel)
        {

            var vendor = _vendorService.GetVendor(vendorApprovalFormViewModel.ID);
            if (vendor == null)
            {
                return HttpNotFound();
            }
            vendor.IsActive = true;
            vendor.IsApproved = true;
            vendor.ApprovalStatusID = 3;

            vendor.Remarks = vendor.Remarks + "<hr />" + Helpers.TimeZone.GetLocalDateTime().ToString("dd MMM yyyy, h:mm tt") + "<br />" + vendorApprovalFormViewModel.Remarks;

            string message = string.Empty;

            if (_vendorService.UpdateVendor(ref vendor, ref message, false))
            {
                SuccessMessage = "Vendor " + ((bool)vendor.IsActive ? "Approved" : "Rejected") + "  successfully ...";

                var vendorUser = _vendorUserService.GetVendorUser(vendor.ID);
                if (vendorUser != null)
                {
                    vendorUser.UserType = null;
                    _vendorUserService.UpdateVendorUser(ref vendorUser, ref message);
                }

                //var vendor = _vendorService.GetVendor((long)vendor.VendorID);

                Notification not = new Notification();
                if (vendor.ApprovalStatusID == 3)
                {
                    not.Title = "Vendor Approval";
                    not.TitleAr = "موافقة البائع";
                    not.Description = "Your profile approval have been approved ";
                    not.Url = "/Vendor/Dashboard/Index";
                }
                else
                {
                    not.Title = "Vendor Approval";
                    not.TitleAr = "موافقة البائع";
                    not.Description = "Your profile approval have been rejected ";
                    not.Url = "/Vendor/Account/ProfileManagement";
                }
                not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                not.OriginatorName = Session["UserName"].ToString();
                not.Module = "Vendor";
                not.OriginatorType = "Admin";
                not.RecordID = vendor.ID;
                if (_notificationService.CreateNotification(not, ref message))
                {
                    NotificationReceiver notRec = new NotificationReceiver();
                    notRec.ReceiverID = vendor.ID;
                    notRec.ReceiverType = "Vendor";
                    notRec.NotificationID = not.ID;
                    if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                    {
                    }
                }
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        ID = vendor.ID,
                        Date = vendor.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        Vendor = vendor.Logo + "|" + vendor.Name + "|" + vendor.VendorCode,
                        Email = vendor.Email,
                        IsActive = vendor.IsActive.HasValue ? vendor.IsActive.Value.ToString() : bool.FalseString,
                        IsApproved = vendor.IsApproved.HasValue ? vendor.IsApproved.Value.ToString() : bool.FalseString
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Vendor Reports
        public ActionResult VendorListReport()
        {
            var vendors = _vendorService.GetVendors(true);
            return View(vendors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VendorReport()
        {
            string ImageServer = CustomURL.GetImageServer();
            var getAllVendors = _vendorService.GetVendors(true).ToList();
            if (getAllVendors.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("VendorReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Code"
                        ,"Name"
                        ,"Website"
                        ,"Email"
                        ,"Contact"
                        ,"Mobile"
                        ,"ID No"
                        ,"TRN"
                        ,"License"
                        ,"Country Name"
                        ,"City Name"
                        ,"Fax"
                        ,"Address"
                        ,"About"
                        ,"Logo"
                        ,"Cover Image"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["VendorReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllVendors)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.VendorCode) ? i.VendorCode : "-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
                        ,!string.IsNullOrEmpty(i.Website) ? i.Website : "-"
                        ,!string.IsNullOrEmpty(i.Email) ? i.Email : "-"
                        ,!string.IsNullOrEmpty(i.Contact) ? i.Contact : "-"
                        ,!string.IsNullOrEmpty(i.Mobile) ? i.Mobile : "-"
                        ,!string.IsNullOrEmpty(i.IDNo) ? i.IDNo : "-"
                        ,!string.IsNullOrEmpty(i.TRN) ? i.TRN : "-"
                        ,!string.IsNullOrEmpty(i.License) ? i.License : "-"
                        ,!string.IsNullOrEmpty(i.Country.Name) ? i.Country.Name : "-"
                        ,!string.IsNullOrEmpty(i.City.Name) ? i.City.Name : "-"
                        ,!string.IsNullOrEmpty(i.FAX) ? i.FAX : "-"
                        ,!string.IsNullOrEmpty(i.Address) ? i.Address : "-"
                        ,!string.IsNullOrEmpty(i.About) ? i.About : "-"
                        ,!string.IsNullOrEmpty(i.Logo) ? (ImageServer + i.Logo) : "-"
                        ,!string.IsNullOrEmpty(i.CoverImage) ? (ImageServer + i.CoverImage) : "-"
                        ,i.IsActive == true ? "Active" : "InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Vendor Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

        #endregion
    }
}