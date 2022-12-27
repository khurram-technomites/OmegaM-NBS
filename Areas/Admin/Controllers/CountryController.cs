using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LinqToExcel;
using NowBuySell.Web.Helpers.POCO;
using System.ComponentModel.DataAnnotations;
using OfficeOpenXml;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CountryController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            this._countryService = countryService;
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
            var countries = _countryService.GetCountries();
            return PartialView(countries);
        }

        public ActionResult ListReport()
        {
            var countries = _countryService.GetCountries();
            return View(countries);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Country country = _countryService.GetCountry((Int16)id);
            if (country == null)
            {
                return HttpNotFound();
            }
            return View(country);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Country country)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_countryService.CreateCountry(country, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/Country/Index",
                        message = message,
                        data = new
                        {
                            Date = country.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            Name = country.Name,
                            IsActive = country.IsActive.HasValue ? country.IsActive.Value.ToString() : bool.FalseString,
                            ID = country.ID
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
            Country country = _countryService.GetCountry((long)id);
            if (country == null)
            {
                return HttpNotFound();
            }

            TempData["CountryID"] = id;
            return View(country);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Country country)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                long Id;
                if (TempData["CountryID"] != null && Int64.TryParse(TempData["CountryID"].ToString(), out Id) && country.ID == Id)
                {
                    if (_countryService.UpdateCountry(ref country, ref message))
                    {
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/Country/Index",
                            message = "Country updated successfully ...",
                            data = new
                            {
                                Date = country.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                                Name = country.Name,
                                IsActive = country.IsActive.HasValue ? country.IsActive.Value.ToString() : bool.FalseString,
                                ID = country.ID
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
            var country = _countryService.GetCountry((long)id);
            if (country == null)
            {
                return HttpNotFound();
            }

            if (!(bool)country.IsActive)
                country.IsActive = true;
            else
            {
                country.IsActive = false;
            }
            string message = string.Empty;
            if (_countryService.UpdateCountry(ref country, ref message))
            {
                SuccessMessage = "Country " + ((bool)country.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        Date = country.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                        Name = country.Name,
                        IsActive = country.IsActive.HasValue ? country.IsActive.Value.ToString() : bool.FalseString,
                        ID = country.ID
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
            Country country = _countryService.GetCountry((Int16)id);
            if (country == null)
            {
                return HttpNotFound();
            }
            TempData["CountryID"] = id;
            return View(country);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_countryService.DeleteCountry((Int16)id, ref message))
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

                        string sheetName = "BulkCountry";
                        var realEstateID = Convert.ToInt64(Session["RealEstateID"]);

                        int count = 1;
                        try
                        {
                            var excelFile = new ExcelQueryFactory(pathToExcelFile);
                            var tenants = from a in excelFile.Worksheet<CountryWorkSheet>(sheetName) select a;
                            foreach (var item in tenants)
                            {
                                var results = new List<ValidationResult>();
                                var context = new ValidationContext(item, null, null);
                                if (Validator.TryValidateObject(item, context, results))
                                {
                                    if (_countryService.PostExcelData(item.Name,item.NameAr))
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



                        TempData["SuccessMessage"] = string.Format("{0} Countries inserted!", (count - 1) - ErrorItems.Count());

                        if (ErrorItems.Count() > 0)
                        {
                            TempData["ErrorMessage"] = string.Format("{0} Countries not inserted!", ErrorItems.Count());
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
        public ActionResult CountriesReport()
        {
            var getAllCountries = _countryService.GetCountries().ToList();
            if (getAllCountries.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("CountriesReport");

                    var headerRow = new List<string[]>()
                {
                    new string[] {
                        "Creation Date"
                        ,"Name"
                        ,"NameAr"
                        ,"Status"
                    }
                };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["CountriesReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    if (getAllCountries.Count != 0)
                        getAllCountries = getAllCountries.OrderByDescending(x => x.ID).ToList();

                    foreach (var i in getAllCountries)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name :"-"
                        ,!string.IsNullOrEmpty(i.NameAr) ? i.NameAr :"-"
                        ,i.IsActive == true ? "Active" :"InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Countries Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }
    }
}