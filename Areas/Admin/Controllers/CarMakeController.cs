using System;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LinqToExcel;
using NowBuySell.Web.Helpers.POCO;
using System.ComponentModel.DataAnnotations;
using System.Net;
using NowBuySell.Web.ViewModels.Car;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CarMakeController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ICarMakeService _carMakeService;

        private readonly IVendorService _vendorService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;

        public CarMakeController(ICarMakeService carMakeService, INotificationReceiverService notificationReceiverService, IVendorService vendorService, INotificationService notificationService)
        {
            this._carMakeService = carMakeService;
            this._vendorService = vendorService;
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
            var make = _carMakeService.GetCarMake();
            return PartialView(make);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CarMake make = _carMakeService.GetCarMake((Int16)id);
            if (make == null)
            {
                return HttpNotFound();
            }
            return View(make);
        }

        public ActionResult Create()
        {
            ViewBag.ParentCarMakeID = new SelectList(_carMakeService.GetCarMakeForDropDown(""), "value", "text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CarMake make)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                make.IsApproved = true;
                if (_carMakeService.CreateCarMake(make, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/CarMake/Index",
                        message = message,
                        data = new
                        {
                            Date = make.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            Name = make.Name,
                            NameAR = make.NameAR,
                            IsActive = make.IsActive.HasValue ? make.IsActive.Value.ToString() : bool.FalseString,
                            IsApproved = make.IsApproved.HasValue ? make.IsApproved.Value.ToString() : "",
                            ID = make.ID,
                            Image = make.Image
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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(string Name, string NameAr )
        //{
        //    try
        //    {
        //        string message = string.Empty;
        //        if (ModelState.IsValid)
        //        {
        //            string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/CarMake/"), Name.Replace(" ", "_"), "/Image");

        //            string absolutePath = Server.MapPath("~");
        //            string relativePath = string.Format("/Assets/AppFiles/Images/CarMake/{0}/", Name.Replace(" ", "_"));
        //            CarMake carmake = new CarMake();
        //            carmake.Name = Name;
        //            carmake.NameAR = NameAr;
        //            carmake.Image = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "Logo", ref message, "Image");

        //            if (_carMakeService.CreateCarMake(carmake, ref message))
        //            {
        //                return Json(new
        //                {
        //                    success = true,
        //                    url = "/Admin/CarMake/Index",
        //                    message = message,
        //                    data = new
        //                    {
        //                        Date = carmake.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
        //                        ID = carmake.ID,
        //                        Image = carmake.Image,
        //                        Name = carmake.Name,
        //                        IsActive = carmake.IsActive.HasValue ? carmake.IsActive.Value.ToString() : bool.FalseString,
        //                        IsApproved = carmake.IsApproved.HasValue ? carmake.IsApproved.Value.ToString() : bool.FalseString
        //                    }
        //                });
        //            }
        //        }
        //        else
        //        {
        //            message = "Please fill the form properly ...";
        //        }
        //        return Json(new { success = false, message = message });
        //        /*return PartialView();*/
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = "Oops! Something went wrong. Please try later."
        //        });
        //    }
        //}

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CarMake make = _carMakeService.GetCarMake((long)id);
            if (make == null)
            {
                return HttpNotFound();
            }

            return View(make);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CarMake make)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                long Id;

                if (_carMakeService.UpdateCarMake(ref make, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/CarMake/Index",
                        message = "Make updated successfully ...",
                        data = new
                        {
                            Date = make.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            Name = make.Name,
                            IsActive = make.IsActive.HasValue ? make.IsActive.Value.ToString() : bool.FalseString,

                            IsApproved = make.IsApproved.HasValue ? make.IsApproved.Value.ToString() : "",
                            ID = make.ID,
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

        /*        public ActionResult Edit(long? id)
                {
                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    CarMake make = _carMakeService.GetCarMake((long)id);
                    if (make == null)
                    {
                        return HttpNotFound();
                    }

                    TempData["CarMake"] = id;
                    return View(make);
                }*/

        //[HttpPost]
        //public ActionResult Edit(long id, string Name, string NameAr, string Image)
        //{
        //    try
        //    {
        //        string message = string.Empty;
        //        if (ModelState.IsValid)
        //        {
        //            long Id;
        //            //if (TempData["CategoryID"] != null && Int64.TryParse(TempData["CategoryID"].ToString(), out id) && id == Id)
        //            //{
        //            CarMake carmake = _carMakeService.GetCarMake(id);
        //            carmake.Name = Name;
        //            carmake.NameAR = NameAr;

        //            if (Request.Files["Image"] != null)
        //            {
        //                string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/CarMake/"), Name.Replace(" ", "_"), "/Image");

        //                string absolutePath = Server.MapPath("~");
        //                string relativePath = string.Format("/Assets/AppFiles/Images/CarMake/{0}/", Name.Replace(" ", "_"));
        //                carmake.Image = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "Logo", ref message, "Image");
        //            }
        //            if (_carMakeService.UpdateCarMake(ref carmake, ref message))
        //            {
        //                return Json(new
        //                {
        //                    success = true,
        //                    url = "/Admin/CarMake/Index",
        //                    message = message,
        //                    data = new
        //                    {
        //                        Date = carmake.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
        //                        ID = carmake.ID,
        //                        Image = carmake.Image,
        //                        Name = carmake.Name,
        //                        IsActive = carmake.IsActive.HasValue ? carmake.IsActive.Value.ToString() : bool.FalseString,
        //                        IsApproved = carmake.IsApproved.HasValue ? carmake.IsApproved.Value.ToString() : bool.FalseString
        //                    }
        //                });
        //            }
        //            //}
        //        }
        //        else
        //        {
        //            message = "Please fill the form properly ...";
        //        }
        //        return Json(new { success = false, message = message });
        //        /*return PartialView();*/
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = "Oops! Something went wrong. Please try later."
        //        });
        //    }
        //}

        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var make = _carMakeService.GetCarMake((long)id);
            if (make == null)
            {
                return HttpNotFound();
            }

            if (!(bool)make.IsActive)
                make.IsActive = true;
            else
            {
                make.IsActive = false;
            }
            string message = string.Empty;
            if (_carMakeService.UpdateCarMake(ref make, ref message))
            {
                SuccessMessage = "Make " + ((bool)make.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        Date = make.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                        Name = make.Name,
                        Image = make.Image,
                        IsActive = make.IsActive.HasValue ? make.IsActive.Value.ToString() : bool.FalseString,
                        IsApproved = make.IsApproved.HasValue ? make.IsApproved.Value.ToString() : "",
                        ID = make.ID,
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
            CarMake make = _carMakeService.GetCarMake((Int16)id);
            if (make == null)
            {
                return HttpNotFound();
            }
            TempData["CarMake"] = id;
            return View(make);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_carMakeService.DeleteCarMake((Int16)id, ref message))
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
            string data = "";
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

                        string sheetName = "BulkCarMake";
                        var realEstateID = Convert.ToInt64(Session["RealEstateID"]);

                        int count = 1;
                        try
                        {
                            var excelFile = new ExcelQueryFactory(pathToExcelFile);
                            var tenants = from a in excelFile.Worksheet<CarMakeSheet>(sheetName) select a;
                            foreach (var item in tenants)
                            {
                                var results = new List<ValidationResult>();
                                var context = new ValidationContext(item, null, null);
                                if (Validator.TryValidateObject(item, context, results))
                                {
                                    if (_carMakeService.PostExcelData(item.Make, item.MakeAr, item.Image))
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



                        TempData["SuccessMessage"] = string.Format("{0} Make inserted!", (count - 1) - ErrorItems.Count());

                        if (ErrorItems.Count() > 0)
                        {
                            TempData["ErrorMessage"] = string.Format("{0} Make not inserted!", ErrorItems.Count());
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
        public ActionResult CarMakeReport()
        {
            var getAllMake = _carMakeService.GetCarMake().ToList();
            if (getAllMake.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("CarMakeReport");

                    var headerRow = new List<string[]>()
                {
                    new string[] {
                        "Creation Date"
                        ,"Make"
                        ,"MakeAr"
                        ,"Image"
                        ,"Status"
                    }
                };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["CarMakeReport"];

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

                    return File(excel.GetAsByteArray(), "application/msexcel", "CarMake Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

        #region Approval

        //public ActionResult Approvals()
        //{
        //    ViewBag.SuccessMessage = TempData["SuccessMessage"];
        //    ViewBag.ErrorMessage = TempData["ErrorMessage"];
        //    return View();
        //}

        //public ActionResult ApprovalsList()
        //{
        //    var cars = _carMakeService.GetCarMake();
        //    return PartialView(cars);
        //}

        [HttpGet]
        public ActionResult Approve(long id)
        {
            var CurrentData = _carMakeService.GetCarMake(id);
            CarMake data = new CarMake();

            ViewBag.IsApproved = CurrentData.IsApproved;


            var car = _carMakeService.GetCarMake((long)id);


            return PartialView(car);
        }


        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Approve(long id, bool value)
        {
            string message = string.Empty;
            if (value == false)
            {
                if (_carMakeService.GetCarMake(id) != null)
                {


                    var car = _carMakeService.GetCarMake(id);
                    SuccessMessage = "Car make request is Rejected";
                    //var vendor = _vendorService.GetVendor((long)car.VendorID);

                    Notification not = new Notification();
                    not.Title = "Car Rejection";
                    not.TitleAr = "الموافقة على المنتج";

                    not.Description = "Your car make " + car.Name + " have been rejected ";


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

                    _carMakeService.Delete(id);
                    return Json(new
                    {

                        success = true,
                        message = SuccessMessage,
                        data = new
                        {
                            ID = car.ID,
                            Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            //Vendor = vendor.Logo + "|" + vendor.VendorCode + "|" + vendor.Name,
                            Name = car.Name,
                            IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString,
                            IsApproved = car.IsApproved.HasValue ? car.IsApproved.Value.ToString() : ""
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
                var car = _carMakeService.GetCarMake(id);
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





                if (_carMakeService.UpdateCarMake(ref car, ref message))
                {
                    SuccessMessage = "Car make " + ((bool)car.IsActive ? "Approved" : "Rejected") + "  successfully ...";
                    //var vendor = _vendorService.GetVendor((long)car.VendorID);

                    Notification not = new Notification();
                    not.Title = "Car make Approval";
                    not.TitleAr = "الموافقة على المنتج";
                    if (car.IsApproved == true)
                    {
                        not.Description = "Your car make " + car.Name + " have been approved ";
                        not.Url = "/Vendor/CarMake/Index";
                    }
                    else
                    {
                        not.Description = "Your car make " + car.Name + " have been rejected ";
                        not.Url = "/Vendor/CarMake/ApprovalIndex";
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
                            ID = car.ID,
                            Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            //Vendor = vendor.Logo + "|" + vendor.VendorCode + "|" + vendor.Name,
                            Name = car.Name,
                            IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString,
                            IsApproved = car.IsApproved.HasValue ? car.IsApproved.Value.ToString() : ""
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

        //[HttpPost]
        //public ActionResult ApproveAll(List<long> ids)
        //{
        //    foreach (var items in ids)
        //    {
        //        var car = _carService.GetCar(items);

        //        car.IsApproved = true;
        //        car.IsActive = true;
        //        car.ApprovalStatus = (int)CarApprovalStatus.Approved;


        //        //car.Remarks = car.Remarks + "<hr />" + Helpers.TimeZone.GetLocalDateTime().ToString("dd MMM yyyy, h:mm tt") + "<br />" + carApprovalFormViewModel.Remarks;

        //        string message = string.Empty;

        //        if (_carService.UpdateCar(ref car, ref message, false))
        //        {
        //            SuccessMessage = "Car " + ((bool)car.IsActive ? "Approved" : "Rejected") + "  successfully ...";
        //            var vendor = _vendorService.GetVendor((long)car.VendorID);
        //            Notification not = new Notification();
        //            not.Title = "Car Approval";
        //            not.TitleAr = "الموافقة على المنتج";
        //            if (car.ApprovalStatus == 3)
        //            {
        //                not.Description = "Your car " + car.SKU + " have been approved ";
        //                not.Url = "/Vendor/Car/Index";
        //            }
        //            else
        //            {
        //                not.Description = "Your " + car.SKU + " have been rejected ";
        //                not.Url = "/Vendor/Car/ApprovalIndex";
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
        //            else
        //            {
        //                ErrorMessage = "Oops! Something went wrong. Please try later.";
        //            }
        //        }
        //    }
        //    // return RedirectToAction("Index");
        //    return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        //}
        //public ActionResult Activate(long? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var car = _carService.GetCar((long)id);
        //    if (car == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    if (!(bool)car.IsActive)
        //        car.IsActive = true;
        //    else
        //    {
        //        car.IsActive = false;
        //    }
        //    string message = string.Empty;
        //    if (_carService.UpdateCar(ref car, ref message))
        //    {
        //        SuccessMessage = "Car " + ((bool)car.IsActive ? "activated" : "deactivated") + "  successfully ...";
        //        return Json(new
        //        {
        //            success = true,
        //            message = SuccessMessage,
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        ErrorMessage = "Oops! Something went wrong. Please try later.";
        //    }

        //    return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        //}

        #endregion

    }
}