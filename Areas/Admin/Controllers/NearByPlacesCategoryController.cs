using LinqToExcel;
using OfficeOpenXml;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.POCO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class NearByPlacesCategoryController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly INearByPlacesCategoryService _nearByPlacesCategoryService ;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;


        public NearByPlacesCategoryController(INearByPlacesCategoryService nearByPlacesCategoryService, INotificationReceiverService notificationReceiverService, INotificationService notificationService)
        {
            _nearByPlacesCategoryService = nearByPlacesCategoryService;
            _notificationReceiverService = notificationReceiverService;
            _notificationService = notificationService;
        }
        // GET: Admin/PropertyFeature
        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.ExcelUploadErrorMessage = TempData["ExcelUploadErrorMessage"];
            return View();
        }

        public ActionResult List()
        {
            var nearByPlacesCategory = _nearByPlacesCategoryService.GetAllNearByPlacesCategory();
            return PartialView(nearByPlacesCategory);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult NearByPlacesCategoryReport()
        //{
        //    var getAllMake = _featureService.GetAllPropertyFeature().ToList();
        //    if (getAllMake.Count() > 0)
        //    {
        //        using (ExcelPackage excel = new ExcelPackage())
        //        {
        //            excel.Workbook.Worksheets.Add("FeatureReport");

        //            var headerRow = new List<string[]>()
        //        {
        //            new string[] {
        //                "Creation Date"
        //                ,"Feature"
        //                ,"FeatureAr"
        //                ,"Status"
        //            }
        //        };

        //            // Determine the header range (e.g. A1:D1)
        //            string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

        //            // Target a worksheet
        //            var worksheet = excel.Workbook.Worksheets["FeatureReport"];

        //            // Popular header row data
        //            worksheet.Cells[headerRange].LoadFromArrays(headerRow);

        //            var cellData = new List<object[]>();

        //            if (getAllMake.Count != 0)
        //                getAllMake = getAllMake.OrderByDescending(x => x.ID).ToList();

        //            foreach (var i in getAllMake)
        //            {
        //                cellData.Add(new object[] {
        //                i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
        //                ,!string.IsNullOrEmpty(i.Name) ? i.Name :"-"
        //                ,!string.IsNullOrEmpty(i.NameAR) ? i.NameAR :"-"
        //                ,i.IsActive == true ? "Active" :"InActive"
        //                });
        //            }

        //            worksheet.Cells[2, 1].LoadFromArrays(cellData);

        //            return File(excel.GetAsByteArray(), "application/msexcel", "FeatureReport.xlsx");
        //        }
        //    }
        //    return RedirectToAction("Index");
        //}

        //public ActionResult BulkUpload()
        //{

        //    return View();

        //}

        //[HttpPost]
        //public ActionResult BulkUpload(HttpPostedFileBase FileUpload)
        //{
        //    string data = "";
        //    List<string> ErrorItems = new List<string>();
        //    List<string> EmailFailed = new List<string>();

        //    if (FileUpload != null)
        //    {
        //        if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        //        {
        //            string filename = FileUpload.FileName;

        //            if (filename.EndsWith(".xlsx"))
        //            {
        //                string targetpath = Server.MapPath("~/assets/AppFiles/Documents/ExcelFiles");
        //                FileUpload.SaveAs(targetpath + filename);
        //                string pathToExcelFile = targetpath + filename;

        //                string sheetName = "BulkFeature";
        //                var realEstateID = Convert.ToInt64(Session["RealEstateID"]);

        //                int count = 1;
        //                try
        //                {
        //                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
        //                    var tenants = from a in excelFile.Worksheet<FeatureWorkSheet>(sheetName) select a;
        //                    foreach (var item in tenants)
        //                    {
        //                        var results = new List<ValidationResult>();
        //                        var context = new ValidationContext(item, null, null);
        //                        if (Validator.TryValidateObject(item, context, results))
        //                        {
        //                            if (_featureService.PostExcelData(item.Feature, item.FeatureAr, "Property"))
        //                            {
        //                                //Mail ObjMail = new Mail(realEstateID);
        //                                //if (!ObjMail.SendTenantAccountCreationMail(item.Name, item.NameAR, item.Country))
        //                                //{
        //                                //    EmailFailed.Add(item.Email);
        //                                //}
        //                            }
        //                            else
        //                            {
        //                                ErrorItems.Add(string.Format("Row Number {0} Not Inserted.<br>", count));
        //                            }
        //                        }
        //                        else
        //                        {
        //                            ErrorItems.Add(string.Format("<b>Row Number {0} Not Inserted:</b><br>{1}", count, string.Join<string>("<br>", results.Select(i => i.ErrorMessage).ToList())));
        //                        }
        //                        count++;
        //                    }
        //                    System.IO.File.Delete(targetpath + filename);
        //                }
        //                catch (Exception ex)
        //                {
        //                    TempData["ErrorMessage"] = "Error binding some fields, Please check your excel sheet for null or wrong entries";
        //                    return RedirectToAction("Index");
        //                }



        //                TempData["SuccessMessage"] = string.Format("{0} Features inserted!", (count - 1) - ErrorItems.Count());

        //                if (ErrorItems.Count() > 0)
        //                {
        //                    TempData["ErrorMessage"] = string.Format("{0} Body Type not inserted!", ErrorItems.Count());
        //                    TempData["ExcelUploadErrorMessage"] = string.Join<string>("<br>", ErrorItems);
        //                }
        //                return RedirectToAction("Index");
        //            }

        //            TempData["ErrorMessage"] = "Invalid file format, Only .xlsx format is allowed";
        //        }

        //        TempData["ErrorMessage"] = "Invalid file format, Only Excel file is allowed";
        //    }

        //    TempData["ErrorMessage"] = "Please upload Excel file first";
        //    return RedirectToAction("Index");
        //}

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(string Name, string NameAR, string Category)
        {
            string message = string.Empty;
            string FImage = string.Empty;
            if (ModelState.IsValid)
            {

                string FilePath = string.Format("{0}", Server.MapPath("~/Assets/AppFiles/Images/NearByPlacesCategory/"), Name);

                string absolutePath = Server.MapPath("~");
                string relativePath = string.Format("/Assets/AppFiles/Images/NearByPlacesCategory/{0}/", Name);
                FImage = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "logo", ref message, "Image");

                NearByPlacesCategory nearByPlacesCategory = new NearByPlacesCategory();
                nearByPlacesCategory.Name = Name;
                nearByPlacesCategory.NameAr = NameAR;
                nearByPlacesCategory.Image = FImage;
                if (_nearByPlacesCategoryService.CreateNearByPlacesCategory(nearByPlacesCategory, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/NearByPlacesCategory/Index",
                        message = message,
                        data = new
                        {
                            Date = nearByPlacesCategory.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            Name = nearByPlacesCategory.Name,
                            Image = nearByPlacesCategory.Image,
                            IsActive = nearByPlacesCategory.IsActive.HasValue ? nearByPlacesCategory.IsActive.Value.ToString() : bool.FalseString,
                            //IsApproved = feature.IsApproved,
                            ID = nearByPlacesCategory.ID
                        }
                    });
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }

            return Json(new { success = false, message = message });
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NearByPlacesCategory nearByPlacesCategory = _nearByPlacesCategoryService.GetNearByPlacesCategory((long)id);
            if (nearByPlacesCategory == null)
            {
                return HttpNotFound();
            }

            TempData["nearByPlacesCategory"] = id;
            return View(nearByPlacesCategory);
        }

        [HttpPost]
        public ActionResult Edit(string Name, string NameAr, long ID)
        {
            string FImage = string.Empty;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                long Id;
                var NByPlacesCategory = _nearByPlacesCategoryService.GetNearByPlacesCategory(ID);
                if (TempData["nearByPlacesCategory"] != null && Int64.TryParse(TempData["nearByPlacesCategory"].ToString(), out Id) && ID == Id)
                {
                    if (Request.Files.Count > 0)
                    {
                        string FilePath = string.Format("{0}", Server.MapPath("~/Assets/AppFiles/Images/NearByPlacesCategory/"), Name);

                        string absolutePath = Server.MapPath("~");
                        string relativePath = string.Format("/Assets/AppFiles/Images/NearByPlacesCategory/{0}/", Name);

                        FImage = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "logo", ref message, "Image");
                    }

                    NearByPlacesCategory nearByPlacesCategory = new NearByPlacesCategory();
                    nearByPlacesCategory.ID = Int64.Parse(TempData["nearByPlacesCategory"].ToString());
                    nearByPlacesCategory.Name = Name;
                    nearByPlacesCategory.NameAr = NameAr;
                    nearByPlacesCategory.Image = string.IsNullOrEmpty(FImage) ? NByPlacesCategory.Image : FImage;
                    /*feature.Image = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "logo", ref message, "Image");*/

                    if (_nearByPlacesCategoryService.UpdateNearByPlacesCategory(ref nearByPlacesCategory, ref message))
                    {
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/NearByPlacesCategory/Index",
                            message = "Category updated successfully ...",
                            data = new
                            {
                                Date = nearByPlacesCategory.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                                Name = nearByPlacesCategory.Name,
                                Image = nearByPlacesCategory.Image,
                                IsActive = nearByPlacesCategory.IsActive.HasValue ? nearByPlacesCategory.IsActive.Value.ToString() : bool.FalseString,
                                //IsApproved = nearByPlacesCategory.IsApproved,
                                ID = nearByPlacesCategory.ID
                            }
                        });
                    }

                }
                else
                {
                    message = "Oops! Something went wrong. Please try later.";
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }
            return Json(new { success = false, message = message });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_nearByPlacesCategoryService.DeleteNearByPlacesCategory((Int16)id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var type = _nearByPlacesCategoryService.GetNearByPlacesCategory((long)id);
            if (type == null)
            {
                return HttpNotFound();
            }

            if (!(bool)type.IsActive)
            {
                type.IsActive = true;
            }
            else
            {
                type.IsActive = false;
            }
            string message = string.Empty;
            if (_nearByPlacesCategoryService.UpdateNearByPlacesCategory(ref type, ref message))
            {
                SuccessMessage = "Category " + ((bool)type.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        Date = type.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                        Name = type.Name,
                        Image = type.Image,
                        IsActive = type.IsActive.HasValue ? type.IsActive.Value.ToString() : bool.FalseString,
                        //IsApproved = type.IsApproved,
                        ID = type.ID
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult Approve(long id)
        //{
        //    var CurrentData = _featureService.GetFeature(id);
        //    Feature data = new Feature();
        //    ViewBag.IsApproved = CurrentData.IsApproved;
        //    var feature = _featureService.GetFeature((long)id);
        //    return PartialView(feature);
        //}

        //[HttpPost]
        //public ActionResult Approve(long id, bool value)
        //{
        //    string message = string.Empty;
        //    if (value == false)
        //    {
        //        if (_featureService.GetFeature(id) != null)
        //        {


        //            var car = _featureService.GetFeature(id);
        //            SuccessMessage = "Feature request is Rejected";
        //            //var vendor = _vendorService.GetVendor((long)car.VendorID);

        //            Notification not = new Notification();
        //            not.Title = "Feature Rejection";
        //            not.TitleAr = "الموافقة على المنتج";

        //            not.Description = "Feature " + car.Name + " have been rejected ";


        //            not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
        //            not.OriginatorName = Session["UserName"].ToString();
        //            not.Module = "Car";
        //            not.OriginatorType = "Admin";
        //            not.RecordID = car.ID;
        //            if (_notificationService.CreateNotification(not, ref message))
        //            {
        //                NotificationReceiver notRec = new NotificationReceiver();
        //                notRec.ReceiverID = car.VendorID;
        //                notRec.ReceiverType = "Vendor";
        //                notRec.NotificationID = not.ID;
        //                if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
        //                {

        //                }
        //            }

        //            _featureService.DeleteFeatureFromCarFeature(id);
        //            return Json(new
        //            {

        //                success = true,
        //                message = SuccessMessage,
        //                data = new
        //                {

        //                    ID = car.ID,
        //                    Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
        //                    //Vendor = vendor.Logo + "|" + vendor.VendorCode + "|" + vendor.Name,
        //                    Name = car.Name,
        //                    IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString,
        //                    IsApproved = car.IsApproved.HasValue ? car.IsApproved.Value.ToString() : ""
        //                }
        //            }, JsonRequestBehavior.AllowGet);



        //        }
        //        else
        //        {
        //            ErrorMessage = "Oops! Something went wrong. Please try later.";
        //        }

        //    }
        //    else
        //    {
        //        var car = _featureService.GetFeature(id);
        //        if (car == null)
        //        {
        //            return HttpNotFound();
        //        }

        //        if (value == true)
        //        {
        //            car.IsApproved = true;


        //        }
        //        else
        //        {
        //            car.IsApproved = false;


        //        }


        //        if (_featureService.UpdateFeature(ref car, ref message))
        //        {
        //            SuccessMessage = "Feature " + ((bool)car.IsActive ? "Approved" : "Rejected") + "  successfully ...";
        //            //var vendor = _vendorService.GetVendor((long)car.VendorID);s

        //            Notification not = new Notification();
        //            not.Title = "Feature Rejection";
        //            not.TitleAr = "الموافقة على المنتج";
        //            if (car.IsApproved == true)
        //            {
        //                not.Description = "Feature " + car.Name + " have been approved ";
        //                not.Url = "/Vendor/Feature/Index";
        //            }
        //            else
        //            {
        //                not.Description = "Feature " + car.Name + " have been rejected ";
        //                not.Url = "/Vendor/Feature/ApprovalIndex";
        //            }
        //            not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
        //            not.OriginatorName = Session["UserName"].ToString();
        //            not.Module = "Car";
        //            not.OriginatorType = "Admin";
        //            not.RecordID = car.ID;
        //            if (_notificationService.CreateNotification(not, ref message))
        //            {
        //                NotificationReceiver notRec = new NotificationReceiver();
        //                notRec.ReceiverID = car.VendorID;
        //                notRec.ReceiverType = "Vendor";
        //                notRec.NotificationID = not.ID;
        //                if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
        //                {

        //                }

        //            }

        //            return Json(new
        //            {

        //                success = true,
        //                message = SuccessMessage,
        //                data = new
        //                {
        //                    ID = car.ID,
        //                    Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
        //                    //Vendor = vendor.Logo + "|" + vendor.VendorCode + "|" + vendor.Name,
        //                    Name = car.Name,
        //                    IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString,
        //                    IsApproved = car.IsApproved.HasValue ? car.IsApproved.Value.ToString() : ""
        //                }
        //            }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            ErrorMessage = "Oops! Something went wrong. Please try later.";
        //        }


        //    }



        //    return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NearByPlacesCategory nearByPlacesCategory = _nearByPlacesCategoryService.GetNearByPlacesCategory((Int16)id);
            if (nearByPlacesCategory == null)
            {
                return HttpNotFound();
            }
            return View(nearByPlacesCategory);
        }

    }
}