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
	public class AreaController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IAreaService _areaService;
		private readonly ICountryService _countryService;
		private readonly ICityService _cityService;

		public AreaController(IAreaService areaService, ICountryService countryService, ICityService cityService)
		{
			this._areaService = areaService;
			this._countryService = countryService;
			this._cityService = cityService;
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
			var areas = _areaService.GetAreas();
			return PartialView(areas);
		}

		public ActionResult ListReport()
		{
			var areas = _areaService.GetAreas();
			return View(areas);
		}

		public ActionResult Details(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Area area = _areaService.GetArea((Int16)id);
			if (area == null)
			{
				return HttpNotFound();
			}
			return View(area);
		}

		public ActionResult Create()
		{
			ViewBag.CountryID = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text");

			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(Area area)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (_areaService.CreateArea(area, ref message))
				{
					var city = _cityService.GetCity((long)area.CityID);
					var country = _countryService.GetCountry((long)area.CountryID);
					return Json(new
					{
						success = true,
						url = "/Admin/Area/Index",
						message = message,
						data = new
						{
							ID = area.ID,
							Date = area.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
							Country = country != null ? country.Name : "",
							City = city != null ? city.Name : "",
							Name = area.Name,
							IsActive = area.IsActive.HasValue ? area.IsActive.Value.ToString() : bool.FalseString
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
			Area area = _areaService.GetArea((long)id);
			if (area == null)
			{
				return HttpNotFound();
			}


			ViewBag.CountryID = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text", area.CountryID);
			ViewBag.CityID = new SelectList(_cityService.GetCitiesForDropDown(), "value", "text", area.CityID);

			TempData["AreaID"] = id;
			return View(area);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(Area area)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				long Id;
				if (TempData["AreaID"] != null && Int64.TryParse(TempData["AreaID"].ToString(), out Id) && area.ID == Id)
				{
					if (_areaService.UpdateArea(ref area, ref message))
					{
						var city = _cityService.GetCity((long)area.CityID);
						var country = _countryService.GetCountry((long)area.CountryID);
						return Json(new
						{
							success = true,
							url = "/Admin/Area/Index",
							message = "Area updated successfully ...",
							data = new
							{
								ID = area.ID,
								Date = area.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
								Country = country != null ? country.Name : "",
								City = city != null ? city.Name : "",
								Name = area.Name,
								IsActive = area.IsActive.HasValue ? area.IsActive.Value.ToString() : bool.FalseString
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
			var area = _areaService.GetArea((long)id);
			if (area == null)
			{
				return HttpNotFound();
			}

			if (!(bool)area.IsActive)
				area.IsActive = true;
			else
			{
				area.IsActive = false;
			}
			string message = string.Empty;
			if (_areaService.UpdateArea(ref area, ref message))
			{
				City city=new City();
				Country country=new Country();
				if (area.CityID != null)
				{
					city = _cityService.GetCity((long)area.CityID);
				}
				if (area.CountryID!=null){ 
				 country = _countryService.GetCountry((long)area.CountryID);
				} 
				SuccessMessage = "Area " + ((bool)area.IsActive ? "activated" : "deactivated") + "  successfully ...";
				return Json(new
				{
					success = true,
					message = SuccessMessage,
					data = new
					{
						ID = area.ID,
						Date = area.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
						Country = country != null ? country.Name : "",
						City = city != null ? city.Name : "",
						Name = area.Name,
						IsActive = area.IsActive.HasValue ? area.IsActive.Value.ToString() : bool.FalseString
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
            Area area = _areaService.GetArea((Int16)id);
            if (area == null)
            {
                return HttpNotFound();
            }
            TempData["AreaID"] = id;
            return View(area);
        }

        [HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(long id)
		{
			string message = string.Empty;
			if (_areaService.DeleteArea((Int16)id, ref message))
			{
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult GetAreasByCity(long id)
		{
			var areas = _areaService.GetAreasForDropDown(id);

			return Json(new { success = true, message = "Data recieved successfully!", data = areas }, JsonRequestBehavior.AllowGet);
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

                        string sheetName = "BulkArea";
                        var realEstateID = Convert.ToInt64(Session["RealEstateID"]);

                        int count = 1;
                        try
                        {
                            var excelFile = new ExcelQueryFactory(pathToExcelFile);
                            var tenants = from a in excelFile.Worksheet<AreaWorkSheet>(sheetName) select a;
                            foreach (var item in tenants)
                            {
                                var results = new List<ValidationResult>();
                                var context = new ValidationContext(item, null, null);
                                if (Validator.TryValidateObject(item, context, results))
                                {
                                    if (_areaService.PostExcelData(item.Name,item.NameAR, item.CountryName, item.CityName))
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



                        TempData["SuccessMessage"] = string.Format("{0} Areas inserted!", (count - 1) - ErrorItems.Count());

                        if (ErrorItems.Count() > 0)
                        {
                            TempData["ErrorMessage"] = string.Format("{0} Areas not inserted!", ErrorItems.Count());
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
		public ActionResult AreasReport()
		{
			var getAllAreas = _areaService.GetAreas().ToList();
			if (getAllAreas.Count() > 0)
			{
				using (ExcelPackage excel = new ExcelPackage())
				{
					excel.Workbook.Worksheets.Add("AreasReport");

					var headerRow = new List<string[]>()
					{
					new string[] {
						"Creation Date"
						,"Country"
						,"City"
						,"Area Name"
						, "NameAr"
						,"Status"
						}
					};

					// Determine the header range (e.g. A1:D1)
					string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

					// Target a worksheet
					var worksheet = excel.Workbook.Worksheets["AreasReport"];

					// Popular header row data
					worksheet.Cells[headerRange].LoadFromArrays(headerRow);

					var cellData = new List<object[]>();

					foreach (var i in getAllAreas)
					{
						cellData.Add(new object[] {
						i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
						,!string.IsNullOrEmpty(i.Country.Name) ? i.Country.Name :"-"
						,!string.IsNullOrEmpty(i.City.Name) ? i.City.Name :"-"
						,!string.IsNullOrEmpty(i.Name) ? i.Name :"-"
						,!string.IsNullOrEmpty(i.NameAR) ? i.NameAR :"-"
						,i.IsActive == true ? "Active" :"InActive"
						});
					}

					worksheet.Cells[2, 1].LoadFromArrays(cellData);

					return File(excel.GetAsByteArray(), "application/msexcel", "Areas Report.xlsx");
				}
			}
			return RedirectToAction("Index");
		}

	}
}