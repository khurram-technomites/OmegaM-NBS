using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using NowBuySell.Web.Helpers.POCO;
using System.ComponentModel.DataAnnotations;
using OfficeOpenXml;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class DeliveryChargeController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IDeliveryChargesService _deliverychargeService;

        private readonly ICountryService _countryService;
        private readonly ICityService _cityService;
        private readonly IAreaService _areaService;

        public DeliveryChargeController(IDeliveryChargesService deliverychargeService, ICountryService countryService, ICityService cityService, IAreaService areaService)
        {
            this._deliverychargeService = deliverychargeService;
            this._countryService = countryService;
            this._cityService = cityService;
            this._areaService = areaService;
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
            var deliverycharges = _deliverychargeService.GetDeliveryCharges();
            return PartialView(deliverycharges);
        }

        public ActionResult ListReport()
        {
            var deliverycharges = _deliverychargeService.GetDeliveryCharges();
            return View(deliverycharges);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeliveryCharge deliverycharge = _deliverychargeService.GetDeliveryCharges((Int16)id);
            if (deliverycharge == null)
            {
                return HttpNotFound();
            }
            return View(deliverycharge);
        }

        public ActionResult Create()
        {
            ViewBag.CountryID = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DeliveryCharge deliverycharge)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_deliverychargeService.CreateDeliveryCharges(deliverycharge, ref message))
                {
                    var Area = _areaService.GetArea((long)deliverycharge.AreaID);
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/DeliveryCharge/Index",
                        message = message,
                        data = new
                        {
                            ID = deliverycharge.ID,
                            Date = deliverycharge.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            Area = Area != null ? Area.Name : "",
                            Charges = deliverycharge.Charges,
                            MinimumOrder = deliverycharge.MinOrder,
                            IsActive = deliverycharge.IsActive.HasValue ? deliverycharge.IsActive.Value.ToString() : bool.FalseString
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
            DeliveryCharge deliverycharge = _deliverychargeService.GetDeliveryCharges((long)id);
            if (deliverycharge == null)
            {
                return HttpNotFound();
            }


            ViewBag.CountryID = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text", deliverycharge.Area.CountryID);
            ViewBag.CityID = new SelectList(_cityService.GetCitiesForDropDown((long)deliverycharge.Area.CountryID), "value", "text", deliverycharge.Area.CityID);
            ViewBag.AreaID = new SelectList(_areaService.GetAreasForDropDown((long)deliverycharge.Area.CityID), "value", "text", deliverycharge.AreaID);

            TempData["DeliveryChargeID"] = id;
            return View(deliverycharge);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DeliveryCharge deliverycharge)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                long Id;
                if (TempData["DeliveryChargeID"] != null && Int64.TryParse(TempData["DeliveryChargeID"].ToString(), out Id) && deliverycharge.ID == Id)
                {
                    if (_deliverychargeService.UpdateDeliveryCharges(ref deliverycharge, ref message))
                    {
                        var Area = _areaService.GetArea((long)deliverycharge.AreaID);
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/DeliveryCharge/Index",
                            message = "DeliveryCharge updated successfully ...",
                            data = new
                            {
                                ID = deliverycharge.ID,
                                Date = deliverycharge.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                                Area = Area != null ? Area.Name : "",
                                Charges = deliverycharge.Charges,
                                MinimumOrder = deliverycharge.MinOrder,
                                IsActive = deliverycharge.IsActive.HasValue ? deliverycharge.IsActive.Value.ToString() : bool.FalseString
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
            var deliverycharge = _deliverychargeService.GetDeliveryCharges((long)id);
            if (deliverycharge == null)
            {
                return HttpNotFound();
            }

            if (!(bool)deliverycharge.IsActive)
                deliverycharge.IsActive = true;
            else
            {
                deliverycharge.IsActive = false;
            }
            string message = string.Empty;
            if (_deliverychargeService.UpdateDeliveryCharges(ref deliverycharge, ref message))
            {
                var Area = _areaService.GetArea((long)deliverycharge.AreaID);
                SuccessMessage = "DeliveryCharge " + ((bool)deliverycharge.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        ID = deliverycharge.ID,
                        Date = deliverycharge.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        Area = Area != null ? Area.Name : "",
                        Charges = deliverycharge.Charges,
                        MinimumOrder = deliverycharge.MinOrder,
                        IsActive = deliverycharge.IsActive.HasValue ? deliverycharge.IsActive.Value.ToString() : bool.FalseString
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
            DeliveryCharge deliverycharge = _deliverychargeService.GetDeliveryCharges((Int16)id);
            if (deliverycharge == null)
            {
                return HttpNotFound();
            }
            TempData["DeliveryChargeID"] = id;
            return View(deliverycharge);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_deliverychargeService.DeleteDeliveryCharges((Int16)id, ref message))
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

                        string sheetName = "BulkDeliveryCharges";
                        var realEstateID = Convert.ToInt64(Session["RealEstateID"]);

                        int count = 1;
                        try
                        {
                            var excelFile = new ExcelQueryFactory(pathToExcelFile);
                            var tenants = from a in excelFile.Worksheet<DeliveryChargesWorkSheet>(sheetName) select a;
                            foreach (var item in tenants)
                            {
                                var results = new List<ValidationResult>();
                                var context = new ValidationContext(item, null, null);
                                if (Validator.TryValidateObject(item, context, results))
                                {
                                    if (_deliverychargeService.PostExcelData(item.AreaName, item.MinOrder, item.Charges))
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
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = "Error binding some fields, Please check your excel sheet for null or wrong entries";
                            return RedirectToAction("Index");
                        }



                        TempData["SuccessMessage"] = string.Format("{0} Charges inserted!", (count - 1) - ErrorItems.Count());

                        if (ErrorItems.Count() > 0)
                        {
                            TempData["ErrorMessage"] = string.Format("{0} Charges not inserted!", ErrorItems.Count());
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
        public ActionResult DeliveryChargesReport()
        {
            var getAllDeliveryCharges = _deliverychargeService.GetDeliveryCharges().ToList();
            if (getAllDeliveryCharges.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("DeliveryChargesReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Country"
                        ,"City"
                        ,"Area"
                        ,"Delivery Charges"
                        ,"Minimum Order"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["DeliveryChargesReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllDeliveryCharges)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Area.Country.Name) ? i.Area.Country.Name : "-"
                        ,!string.IsNullOrEmpty(i.Area.City.Name) ? i.Area.City.Name : "-"
                        ,!string.IsNullOrEmpty(i.Area.Name) ? i.Area.Name :"-"
                        ,i.Charges ?? 0
                        ,i.MinOrder ?? 0
                        ,i.IsActive == true ? "Active" : "InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Delivery Charges Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

    }
}