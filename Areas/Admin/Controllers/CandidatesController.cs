using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Order;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CandidatesController : Controller
    {
        private readonly ICandidateService _careerService;

        public CandidatesController(ICandidateService careerService)
        {
            this._careerService = careerService;

        }
        // GET: Admin/Candidates
        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
            ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-7).ToString("MM/dd/yyyy");
            return View();
        }
        public ActionResult List()
        {
            var candidates = _careerService.GetCandidates();
            return PartialView(candidates);
        }

        [HttpPost]
        public ActionResult List(DateTime fromDate, DateTime ToDate)
        {
            DateTime EndDate = ToDate.AddMinutes(1439);
            var candidates = _careerService.GetCandidatesDateWise(fromDate, EndDate);
            return PartialView(candidates);
        }
		[HttpPost]
		/*[ValidateAntiForgeryToken]*/
		public ActionResult CVReport()
		{
			string ImageServer = CustomURL.GetImageServer();
			var getAllCVs = _careerService.GetCandidates().ToList();
			if (getAllCVs.Count() > 0)
			{
				using (ExcelPackage excel = new ExcelPackage())
				{
					excel.Workbook.Worksheets.Add("CVReport");

					var headerRow = new List<string[]>()
					{
					new string[] {
						"Date"
						,"Name"
						,"Gender"
						,"Experience"
						,"Position" 
						,"FilePath"
						}
					};

					// Determine the header range (e.g. A1:D1)
					string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

					// Target a worksheet
					var worksheet = excel.Workbook.Worksheets["CVReport"];

					// Popular header row data
					worksheet.Cells[headerRange].LoadFromArrays(headerRow);

					var cellData = new List<object[]>();

					if (getAllCVs.Count != 0)
						getAllCVs = getAllCVs.OrderByDescending(x => x.ID).ToList();

					foreach (var i in getAllCVs)
					{
						//string parentCategory = "-";
						//if (i.ParentCategoryID != null)
						//	parentCategory = getAllCatagories.SingleOrDefault(x => x.ID == i.ParentCategoryID)?.CategoryName;

						cellData.Add(new object[] {
						i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
						,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
						,!string.IsNullOrEmpty(i.Gender) ? i.Gender : "-"
						,!string.IsNullOrEmpty(i.Experience) ? i.Experience : "-"
						,!string.IsNullOrEmpty(i.Position) ? i.Position : "-"
						,!string.IsNullOrEmpty(i.FilePath) ? ImageServer + i.FilePath : "-"
						/*,i.Position. ? i.Position.Value : 0*/
						/*,!string.IsNullOrEmpty(i.Slug) ? i.Slug : "-"*/
						/*,!string.IsNullOrEmpty(i.Image) ? (ImageServer + i.Image) : "-"*/
						//,parentCategory
						/*,i.IsDefault == true ? "Yes" : "No"*/
						/*,i.IsActive == true ? "Active" : "InActive"*/
						});
					}

					worksheet.Cells[2, 1].LoadFromArrays(cellData);

					return File(excel.GetAsByteArray(), "application/msexcel", "CVs Report.xlsx");
				}
			}
			return RedirectToAction("Index");
		}
	}
}