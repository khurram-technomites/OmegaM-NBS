using LinqToExcel;
using OfficeOpenXml;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
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
    public class BodyTypeController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IBodyTypeService _bodyTypeService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;

        public BodyTypeController(IBodyTypeService bodyTypeService, INotificationReceiverService notificationReceiverService,INotificationService notificationService)
        {
            this._bodyTypeService = bodyTypeService;
            this._notificationReceiverService = notificationReceiverService;
            this._notificationService = notificationService;
        }

        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.ExcelUploadErrorMessage = TempData["ExcelUploadErrorMessage"];
            return View();
        }

        public ActionResult List()
        {
            var type = _bodyTypeService.GetBodyType();
            return PartialView(type);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BodyType type = _bodyTypeService.GetBodyType((Int16)id);
            if (type == null)
            {
                return HttpNotFound();
            }
            return View(type);
        }

        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BodyType type)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_bodyTypeService.CreateBodyType(type, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/BodyType/Index",
                        message = message,
                        data = new
                        {
                            Date = type.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            Name = type.Name,
                            NameAR = type.NameAR,
                            IsActive = type.IsActive.HasValue ? type.IsActive.Value.ToString() : bool.FalseString,
                            IsApproved = type.IsApproved,
                            ID = type.ID
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
            BodyType type = _bodyTypeService.GetBodyType((long)id);
            if (type == null)
            {
                return HttpNotFound();
            }

            TempData["BodyType"] = id;
            return View(type);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BodyType type)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                long Id;
                if (TempData["BodyType"] != null && Int64.TryParse(TempData["BodyType"].ToString(), out Id) && type.ID == Id)
                {
                    if (_bodyTypeService.UpdateBodyType(ref type, ref message))
                    {
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/BodyType/Index",
                            message = "BodyType updated successfully ...",
                            data = new
                            {
                                Date = type.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                                Name = type.Name,
                                NameAR = type.NameAR,
                                IsActive = type.IsActive.HasValue ? type.IsActive.Value.ToString() : bool.FalseString,
                                IsApproved = type.IsApproved,
                                ID = type.ID
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

        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var type = _bodyTypeService.GetBodyType((long)id);
            if (type == null)
            {
                return HttpNotFound();
            }

            if (!(bool)type.IsActive)
                type.IsActive = true;
            else
            {
                type.IsActive = false;
            }
            string message = string.Empty;
            if (_bodyTypeService.UpdateBodyType(ref type, ref message))
            {
                SuccessMessage = "BodyType " + ((bool)type.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        Date = type.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                        Name = type.Name,
                        NameAR = type.NameAR,
                        IsActive = type.IsActive.HasValue ? type.IsActive.Value.ToString() : bool.FalseString,
                        IsApproved = type.IsApproved,
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

        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BodyType type = _bodyTypeService.GetBodyType((Int16)id);
            if (type == null)
            {
                return HttpNotFound();
            }
            TempData["BodyType"] = id;
            return View(type);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_bodyTypeService.DeleteBodyType((Int16)id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BulkUpload()
        {

            return View();

        }

        [HttpPost]
        public ActionResult BulkUpload(HttpPostedFileBase FileUpload)
        {
            string data = string.Empty;
            List<string> ErrorItems = new List<string>();
            List<string> EmailFailed = new List<string>();

            if (FileUpload != null)
            {
                if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string filename = FileUpload.FileName;

                    if (filename.EndsWith(".xlsx"))
                    {
                        string targetpath = Server.MapPath("~/assets/AppFiles/Documents/ExcelFiles");
                        FileUpload.SaveAs(targetpath + filename);
                        string pathToExcelFile = targetpath + filename;

                        string sheetName = "BulkBodyType";
                        var realEstateID = Convert.ToInt64(Session["RealEstateID"]);

                        int count = 1;
                        try
                        {
                            var excelFile = new ExcelQueryFactory(pathToExcelFile);
                            var tenants = from a in excelFile.Worksheet<BodyTypeWorkSheet>(sheetName) select a;
                            foreach (var item in tenants)
                            {
                                var results = new List<ValidationResult>();
                                var context = new ValidationContext(item, null, null);
                                if (Validator.TryValidateObject(item, context, results))
                                {
                                    if (_bodyTypeService.PostExcelData(item.BodyType, item.BodyTypeAr))
                                    {
                                        //Mail ObjMail = new Mail(realEstateID);
                                        //if (!ObjMail.SendTenantAccountCreationMail(item.Name, item.NameAR, item.Country))
                                        //{
                                        //    EmailFailed.Add(item.Email);
                                        //}
                                    }
                                    else
                                    {
                                        ErrorItems.Add(string.Format("Row Number {0} Not Inserted.<br>", count));
                                    }
                                }
                                else
                                {
                                    ErrorItems.Add(string.Format("<b>Row Number {0} Not Inserted:</b><br>{1}", count, string.Join<string>("<br>", results.Select(i => i.ErrorMessage).ToList())));
                                }
                                count++;
                            }
                            System.IO.File.Delete(targetpath + filename);
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = "Error binding some fields, Please check your excel sheet for null or wrong entries";
                            return RedirectToAction("Index");
                        }



                        TempData["SuccessMessage"] = string.Format("{0} Body type inserted!", (count - 1) - ErrorItems.Count());

                        if (ErrorItems.Count() > 0)
                        {
                            TempData["ErrorMessage"] = string.Format("{0} Body type not inserted!", ErrorItems.Count());
                            TempData["ExcelUploadErrorMessage"] = string.Join<string>("<br>", ErrorItems);
                        }
                        return RedirectToAction("Index");
                    }

                    TempData["ErrorMessage"] = "Invalid file format, Only .xlsx format is allowed";
                }

                TempData["ErrorMessage"] = "Invalid file format, Only Excel file is allowed";
            }

            TempData["ErrorMessage"] = "Please upload Excel file first";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BodyTypeReport()
        {
            var getAllMake = _bodyTypeService.GetBodyType().ToList();
            if (getAllMake.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("BodyTypeReport");

                    var headerRow = new List<string[]>()
                {
                    new string[] {
                        "Creation Date"
                        ,"BodyType"
                        ,"BodyTypeAr"
                        ,"Status"
                    }
                };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["BodyTypeReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    if (getAllMake.Count != 0)
                        getAllMake = getAllMake.OrderByDescending(x => x.ID).ToList();

                    foreach (var i in getAllMake)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name :"-"
                        ,!string.IsNullOrEmpty(i.NameAR) ? i.NameAR :"-"
                        ,i.IsActive == true ? "Active" :"InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "BodyType Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Approve(long id, bool value)
        {
            string message = string.Empty;
            if (value == false)
            {
                if (_bodyTypeService.GetBodyType(id) != null)
                {


                    var car = _bodyTypeService.GetBodyType(id);
                    SuccessMessage = "BodyType rejected successfully...";
                    //var vendor = _vendorService.GetVendor((long)car.VendorID);

                    Notification not = new Notification();
                    not.Title = "Body type Rejection";
                    not.TitleAr = "الموافقة على المنتج";

                    not.Description = "Body type " + car.Name + " have been rejected ";


                    not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                    not.OriginatorName = Session["UserName"].ToString();
                    not.Module = "Car";
                    not.OriginatorType = "Admin";
                    not.RecordID = car.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        NotificationReceiver notRec = new NotificationReceiver();
                        notRec.ReceiverID = car.VendorID;
                        notRec.ReceiverType = "Vendor";
                        notRec.NotificationID = not.ID;
                        if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                        {

                        }

                    }

                    _bodyTypeService.Delete(id);
                    return Json(new
                    {

                        success = true,
                        message = SuccessMessage,
                        data = new
                        {
                            Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            Name = car.Name,
                            NameAR = car.NameAR,
                            IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString,
                            IsApproved = car.IsApproved,
                            ID = car.ID
                        }
                    }, JsonRequestBehavior.AllowGet);

                   
                    
                }
                else
                {
                    ErrorMessage = "Oops! Something went wrong. Please try later.";
                }

            }
            else
            {
                var car = _bodyTypeService.GetBodyType(id);
                if (car == null)
                {
                    return HttpNotFound();
                }

                if (value == true)
                {
                    car.IsApproved = true;


                }
                else
                {
                    car.IsApproved = false;


                }





                if (_bodyTypeService.UpdateBodyType(ref car, ref message))
                {
                    SuccessMessage = "BodyType " + ((bool)car.IsApproved ? "Approved" : "Rejected") + " successfully...";
                    //var vendor = _vendorService.GetVendor((long)car.VendorID);

                    Notification not = new Notification();
                    not.Title = "Body type Approval";
                    not.TitleAr = "الموافقة على المنتج";
                    if (car.IsApproved == true)
                    {
                        not.Description = "Your body type " + car.Name + " have been approved ";
                        not.Url = "/Vendor/BodyType/Index";
                    }
                    else
                    {
                        not.Description = "Your request of body type " + car.Name + " have been rejected ";
                        not.Url = "/Vendor/BodyType/ApprovalIndex";
                    }
                    not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                    not.OriginatorName = Session["UserName"].ToString();
                    not.Module = "Car";
                    not.OriginatorType = "Admin";
                    not.RecordID = car.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        NotificationReceiver notRec = new NotificationReceiver();
                        notRec.ReceiverID = car.VendorID;
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

                            Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            Name = car.Name,
                            NameAR = car.NameAR,
                            IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString,
                            IsApproved = car.IsApproved,
                            ID = car.ID
                            //ID = car.ID,
                            //Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            ////Vendor = vendor.Logo + "|" + vendor.VendorCode + "|" + vendor.Name,
                            //Name = car.Name,
                            //IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString,
                            //IsApproved = car.IsApproved.HasValue ? car.IsApproved.Value.ToString() : ""
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ErrorMessage = "Oops! Something went wrong. Please try later.";
                }


            }



            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }
    }
}