using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using LinqToExcel;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NowBuySell.Web.Helpers.POCO;
using System.ComponentModel.DataAnnotations;
using OfficeOpenXml;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CarModelController : Controller
    {
        // GET: Admin/CarModel
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;
        private readonly ICarModelService _carModelService;
        private readonly ICarMakeService _carMakeService;

        public CarModelController(ICarModelService carModelService, ICarMakeService carMakeService, INotificationReceiverService notificationReceiverService, INotificationService notificationService)
        {
            this._carModelService = carModelService;
            this._carMakeService = carMakeService;
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
            var carModel = _carModelService.GetCarModel();
            return PartialView(carModel);
        }

        public ActionResult ListReport()
        {
            var carModel = _carModelService.GetCarModel();
            return View(carModel);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CarModel carmodel = _carModelService.GetCarModelByID((Int16)id);
            if (carmodel == null)
            {
                return HttpNotFound();
            }
            return View(carmodel);
        }

        public ActionResult Create()
        {
            ViewBag.CarMake_ID = new SelectList(_carMakeService.GetCarMakeForDropDown(), "value", "text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CarModel carModel)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                //carModel.IsApproved = true;
                if (_carModelService.CreateCarModel(ref carModel, ref message))
                {
                    var carMake = _carMakeService.GetCarMake((long)carModel.CarMake_ID);
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/CarModel/Index",
                        message = message,
                        data = new
                        {
                            Date = carModel.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            CarMakeName = carModel.CarMake.Name,
                            Name = carModel.Name,
                            IsApproved=carModel.IsApproved,
                            IsActive = carModel.IsActive.HasValue ? carModel.IsActive.Value.ToString() : bool.FalseString,
                            ID = carModel.ID
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
            CarModel carModel = _carModelService.GetCarModelByID((long)id);
            if (carModel == null)
            {
                return HttpNotFound();
            }

            ViewBag.CarMake_ID= new SelectList(_carMakeService.GetCarMakeForDropDown(), "value", "text", carModel.CarMake_ID);
            TempData["CarModelID"] = id;
            return View(carModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CarModel carModel)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                long Id;
                if (TempData["CarModelID"] != null && Int64.TryParse(TempData["CarModelID"].ToString(), out Id) && carModel.ID == Id)
                {
                    if (_carModelService.UpdateCarModel(ref carModel, ref message))
                    {
                        var carMake = _carMakeService.GetCarMake((long)carModel.CarMake_ID);
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/CarModel/Index",
                            message = "Model updated successfully ...",
                            data = new
                            {
                                Date = carModel.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                                CarMakeName = carModel.CarMake.Name,
                                Name = carModel.Name,
                                IsActive = carModel.IsActive.HasValue ? carModel.IsActive.Value.ToString() : bool.FalseString,
                                IsApproved=carModel.IsApproved,
                                ID = carModel.ID
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
            var carModel = _carModelService.GetCarModelByID((long)id);
            if (carModel == null)
            {
                return HttpNotFound();
            }

            if (!(bool)carModel.IsActive)
                carModel.IsActive = true;
            else
            {
                carModel.IsActive = false;
            }
            string message = string.Empty;
            if (_carModelService.UpdateCarModel(ref carModel, ref message))
            {
                var carMake = _carMakeService.GetCarMake((long)carModel.CarMake_ID);
                SuccessMessage = "Model " + ((bool)carModel.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        Date = carModel.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                        CarMakeName = carModel.CarMake.Name,
                        Name = carModel.Name,
                        IsActive = carModel.IsActive.HasValue ? carModel.IsActive.Value.ToString() : bool.FalseString,
                        IsApproved = carModel.IsApproved,
                        ID = carModel.ID
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
            CarModel carModel= _carModelService.GetCarModelByID((long)id);
            if (carModel == null)
            {
                return HttpNotFound();
            }
            TempData["CarModelID"] = id;
            return View(carModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_carModelService.DeleteCarModel((Int16)id, ref message))
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
            //string data = "";
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

                        string sheetName = "BulkCarModel";
                        var realEstateID = Convert.ToInt64(Session["RealEstateID"]);

                        int count = 1;
                        try
                        {
                            var excelFile = new ExcelQueryFactory(pathToExcelFile);
                            var tenants = from a in excelFile.Worksheet<CarModelWorkSheet>(sheetName) select a;
                            foreach (var item in tenants)
                            {
                                var results = new List<ValidationResult>();
                                var context = new ValidationContext(item, null, null);
                                if (Validator.TryValidateObject(item, context, results))
                                {

                                    if (_carModelService.PostExcelData(item.CarModel,item.CarModelAr,item.CarMake))
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



                        TempData["SuccessMessage"] = string.Format("{0} Car model inserted!", (count - 1) - ErrorItems.Count());

                        if (ErrorItems.Count() > 0)
                        {
                            TempData["ErrorMessage"] = string.Format("{0} Car model not inserted!", ErrorItems.Count());
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
        public ActionResult CarModelReport()
        {
            var getAllCities = _carModelService.GetCarModel().ToList();
            if (getAllCities.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("CarModelReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Car Make"
                        ,"Car Model"
                        ,"Car Model Ar"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["CarModelReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    if (getAllCities.Count != 0)
                        getAllCities = getAllCities.OrderByDescending(x => x.ID).ToList();

                    foreach (var i in getAllCities)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.CarMake.Name) ? i.CarMake.Name :"-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name :"-"
                        ,!string.IsNullOrEmpty(i.NameAR) ? i.NameAR :"-"
                        ,i.IsActive == true ? "Active" :"InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Car Model Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult Approve(long id, bool value)
        {
            var carModel = _carModelService.GetCarModel(id);
            string message = string.Empty;
            if (value == false)
            {
                if (_carModelService.GetCarModel(id) != null)
                {


                   
                    SuccessMessage = "Model request rejected successfully ...";
                    //var vendor = _vendorService.GetVendor((long)car.VendorID);

                    Notification not = new Notification();
                    not.Title = "Car model Rejection";
                    not.TitleAr = "الموافقة على المنتج";

                    not.Description = "Your car model " + carModel.Name + " have been rejected ";


                    not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                    not.OriginatorName = Session["UserName"].ToString();
                    not.Module = "Car";
                    not.OriginatorType = "Admin";
                    not.RecordID = carModel.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        NotificationReceiver notRec = new NotificationReceiver();
                        notRec.ReceiverID = carModel.VendorID;
                        notRec.ReceiverType = "Vendor";
                        notRec.NotificationID = not.ID;
                        if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                        {

                        }

                        
                    }
                    _carModelService.Delete(id);
                    return Json(new
                    {

                        success = true,
                        message = SuccessMessage,
                        data = new
                        {
                            
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
               
                if (carModel == null)
                {
                    return HttpNotFound();
                }

                if (value == true)
                {
                    carModel.IsApproved = true;


                }
                else
                {
                    carModel.IsApproved = false;


                }





                if (_carModelService.UpdateCarModel( ref carModel, ref message))
                {
                    SuccessMessage = "Car Model " + ((bool)carModel.IsActive ? "Approved" : "Rejected") + "  successfully ...";
                    //var vendor = _vendorService.GetVendor((long)car.VendorID);

                    Notification not = new Notification();
                    not.Title = "Car Model Approval";
                    not.TitleAr = "الموافقة على المنتج";
                    if (carModel.IsApproved == true)
                    {
                        not.Description = "Your car model " + carModel.Name + " have been approved ";
                        not.Url = "/Vendor/CarModel/Index";
                    }
                    else
                    {
                        not.Description = "Your car model " + carModel.Name + " have been rejected ";
                        not.Url = "/Vendor/CarModel/ApprovalIndex";
                    }
                    not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                    not.OriginatorName = Session["UserName"].ToString();
                    not.Module = "Car";
                    not.OriginatorType = "Admin";
                    not.RecordID = carModel.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        NotificationReceiver notRec = new NotificationReceiver();
                        notRec.ReceiverID = carModel.VendorID;
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
                            ID = carModel.ID,
                            Date = carModel.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            //Vendor = vendor.Logo + "|" + vendor.VendorCode + "|" + vendor.Name,
                            Name = carModel.Name,
                            CarMakeName = carModel.CarMake.Name,
                            IsActive = carModel.IsActive.HasValue ? carModel.IsActive.Value.ToString() : bool.FalseString,
                            IsApproved = carModel.IsApproved.HasValue ? carModel.IsApproved.Value.ToString() : ""
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