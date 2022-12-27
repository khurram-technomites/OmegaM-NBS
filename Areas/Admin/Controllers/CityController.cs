using LinqToExcel;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.POCO;
using NowBuySell.Web.Helpers.Routing;
using OfficeOpenXml;
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
    public class CityController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ICityService _cityService;
        private readonly ICountryService _countryService;

        public CityController(ICityService cityService, ICountryService _countryService)
        {
            this._cityService = cityService;
            this._countryService = _countryService;
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
            var cities = _cityService.GetCities();
            return PartialView(cities);
        }

        public ActionResult ListReport()
        {
            var cities = _cityService.GetCities();
            return View(cities);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            City city = _cityService.GetCity((Int16)id);
            if (city == null)
            {
                return HttpNotFound();
            }
            return View(city);
        }

        public ActionResult Create()
        {
            City city = new City();
            ViewBag.CountryID = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text");
            return View(city);
        }


        /*public ActionResult Create(City city)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_cityService.CreateCity(ref city, ref message))
                {
                    var country = _countryService.GetCountry((long)city.CountryID);
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/City/Index",
                        message = message,
                        data = new
                        {
                            Date = city.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            Name = city.Name,
                            Country = country.Name,
                            IsActive = city.IsActive.HasValue ? city.IsActive.Value.ToString() : bool.FalseString,
                            ID = city.ID
                        }
                    });
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }

            return Json(new { success = false, message = message });
        }*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int CountryID , string Name , string NameAr,string PlaceId, string Latitude, string Longitude,string Address)
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/City/"), Name.Replace(" ", "_"), "/Image");

                    string absolutePath = Server.MapPath("~");
                    string relativePath = string.Format("/Assets/AppFiles/Images/City/{0}/", Name.Replace(" ", "_"));
                    City city = new City();
                    city.CountryID = CountryID;
                    city.Name = Name;
                    city.NameAR = NameAr;
                    city.PlaceId = PlaceId;
                    city.Latitude = Latitude;
                    city.Longitude = Longitude;
                    city.Address = Address;
                    city.Image = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "logo", ref message, "Image");
                    /*city.Icon = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "logo", ref message, "Icon");*/

                    if (_cityService.CreateCity(ref city, ref message))
                    {
                        var country = _countryService.GetCountry((long)city.CountryID);
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/City/Index",
                            message = message,
                            data = new
                            {
                                Date = city.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                                Name = city.Name,
                                Image = city.Image + "|" + city.Name,
                                /*Icon = city.Icon,*/
                                Country = country.Name,
                                IsActive = city.IsActive.HasValue ? city.IsActive.Value.ToString() : bool.FalseString,
                                ID = city.IsActive.HasValue ? city.IsActive.Value.ToString() + "," + city.ID : bool.FalseString + "," + city.ID
                            }
                        });
                    }
                }
                else
                {
                    message = "Please fill the form properly ...";
                }
                return Json(new { success = false, message = message });
                /*return PartialView();*/
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

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            City city = _cityService.GetCity((long)id);
            if (city == null)
            {
                return HttpNotFound();
            }

            ViewBag.CountryID = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text", city.CountryID);

            TempData["CityID"] = id;
            return View(city);
        }


        //public ActionResult Edit(City city)
        //{
        //    string message = string.Empty;
        //    if (ModelState.IsValid)
        //    {
        //        long Id;
        //        if (TempData["CityID"] != null && Int64.TryParse(TempData["CityID"].ToString(), out Id) && city.ID == Id)
        //        {
        //            if (_cityService.UpdateCity(ref city, ref message))
        //            {
        //                var country = _countryService.GetCountry((long)city.CountryID);
        //                return Json(new
        //                {
        //                    success = true,
        //                    url = "/Admin/City/Index",
        //                    message = "City updated successfully ...",
        //                    data = new
        //                    {
        //                        Date = city.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
        //                        Name = city.Name,
        //                        Country = country.Name,
        //                        IsActive = city.IsActive.HasValue ? city.IsActive.Value.ToString() : bool.FalseString,
        //                        ID = city.ID
        //                    }
        //                });
        //            }

        //        }
        //        else
        //        {
        //            message = "Oops! Something went wrong. Please try later.";
        //        }
        //    }
        //    else
        //    {
        //        message = "Please fill the form properly ...";
        //    }
        //    return Json(new { success = false, message = message });
        //}
        [HttpPost]
        /*[ValidateAntiForgeryToken]*/
        public ActionResult Edit(long CountryID, long id , string Name, string NameAr, string PlaceId, string Latitude, string Longitude, string Address)
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    long Id;
                    if (TempData["CityID"] != null && Int64.TryParse(TempData["CityID"].ToString(), out Id) && id == Id)
                    {
                        City city = _cityService.GetCity(id);
                        city.CountryID = CountryID;
                        city.ID = id;
                        city.Name = Name;
                        city.NameAR = NameAr;
                        city.PlaceId = PlaceId;
                        city.Latitude = Latitude;
                        city.Longitude = Longitude;
                        city.Address = Address;

                        if (Request.Files["Image"] != null)
                        {
                            string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/City/"), Name.Replace(" ", "_"), "/Image");

                            string absolutePath = Server.MapPath("~");
                            string relativePath = string.Format("/Assets/AppFiles/Images/City/{0}/", Name.Replace(" ", "_"));
                            city.Image = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "Logo", ref message, "Image");
                        }
/*                        if (Request.Files["Icon"] != null)
                        {
                            string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/City/"), Name.Replace(" ", "_"), "/Image");

                            string absolutePath = Server.MapPath("~");
                            string relativePath = string.Format("/Assets/AppFiles/Images/City/{0}/", Name.Replace(" ", "_"));
                            city.Icon = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "Logo", ref message, "Icon");
                        }*/
                        if (_cityService.UpdateCity(ref city, ref message))
                        {
                            var country = _countryService.GetCountry((long)city.CountryID);
                            return Json(new
                            {
                                success = true,
                                url = "/Admin/City/Index",
                                message = message,
                                data = new
                                {
                                    Date = city.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                                    Name = city.Name,
                                    Image = city.Image + "|" + city.Name,
                                    /*Icon = city.Icon,*/
                                    Country = country.Name,
                                    IsActive = city.IsActive.HasValue ? city.IsActive.Value.ToString() : bool.FalseString,
                                    ID = city.IsActive.HasValue ? city.IsActive.Value.ToString() + "," + city.ID : bool.FalseString + "," + city.ID
                                }
                            });
                        }
                    }
                }
                else
                    {
                        message = "Please fill the form properly ...";
                    }
                    return Json(new { success = false, message = message });
                    /*return PartialView();*/
             
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

        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var city = _cityService.GetCity((long)id);
            if (city == null)
            {
                return HttpNotFound();
            }

            if (!(bool)city.IsActive)
                city.IsActive = true;
            else
            {
                city.IsActive = false;
            }
            string message = string.Empty;
            if (_cityService.UpdateCity(ref city, ref message))
            {
                var country = _countryService.GetCountry((long)city.CountryID);
                SuccessMessage = "City " + ((bool)city.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        Date = city.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                        Name = city.Name,
                        Image = city.Image,
                       /* Icon = city.Icon,*/
                        Country = country.Name,
                        IsActive = city.IsActive.HasValue ? city.IsActive.Value.ToString() : bool.FalseString,
                        ID = city.ID,
                       
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
            City city = _cityService.GetCity((Int16)id);
            if (city == null)
            {
                return HttpNotFound();
            }
            TempData["CityID"] = id;
            return View(city);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_cityService.DeleteCity((Int16)id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCitiesByCountry(long id)
        {
            var cities = _cityService.GetCitiesForDropDown(id);

            return Json(new { success = true, message = "Data recieved successfully!", data = cities }, JsonRequestBehavior.AllowGet);
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

                        string sheetName = "BulkCity";
                        var realEstateID = Convert.ToInt64(Session["RealEstateID"]);

                        int count = 1;
                        try
                        {
                            var excelFile = new ExcelQueryFactory(pathToExcelFile);
                            var tenants = from a in excelFile.Worksheet<CityWorkSheet>(sheetName) select a;
                            foreach (var item in tenants)
                            {
                                var results = new List<ValidationResult>();
                                var context = new ValidationContext(item, null, null);
                                if (Validator.TryValidateObject(item, context, results))
                                {
                                    
                                    if (_cityService.PostExcelData(item.Name,item.NameAr, item.Country))
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



                        TempData["SuccessMessage"] = string.Format("{0} Cities inserted!", (count - 1) - ErrorItems.Count());

                        if (ErrorItems.Count() > 0)
                        {
                            TempData["ErrorMessage"] = string.Format("{0} Cities not inserted!", ErrorItems.Count());
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
        public ActionResult CitiesReport()
        {
            var getAllCities = _cityService.GetCities().ToList();
            if (getAllCities.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("CitiesReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Country"
                        ,"City Name"
                        ,"NameAr"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["CitiesReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    if (getAllCities.Count != 0)
                        getAllCities = getAllCities.OrderByDescending(x => x.ID).ToList();

                    foreach (var i in getAllCities)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Country.Name) ? i.Country.Name :"-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name :"-"
                        ,!string.IsNullOrEmpty(i.NameAR) ? i.NameAR :"-"
                        ,i.IsActive == true ? "Active" :"InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Cities Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

    }
}