using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Car;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using static NowBuySell.Web.Helpers.Enumerations.Enumeration;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class CustomerRatingController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly ICustomerRatingService _customerratingService;

		public CustomerRatingController(ICustomerRatingService customerratingService)
		{
			this._customerratingService = customerratingService;
		}
		// GET: Admin/CustomerRating
		public ActionResult Index()
		{
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			ViewBag.ErrorMessage = TempData["ErrorMessage"];
			return View();
		}

		public ActionResult List()
		{
			var Rating = _customerratingService.GetRatings();
			return PartialView(Rating);
		}
		[HttpPost]
		public ActionResult Approval(long id, bool status)
		{
			string message = string.Empty;
			var getdata = _customerratingService.GetRating(id);
			if (status == true)
			{
				getdata.IsApproved = true;
				if (_customerratingService.UpdateCarRating(ref getdata, ref message))
				{
					SuccessMessage = "Rating " + ((bool)getdata.IsApproved ? "Approved" : "Rejected") + "  successfully ...";

					return Json(new
					{
						success = true,
						message = SuccessMessage,
						data = new
						{
							Date = getdata.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
							OrderDetailID = getdata.OrderDetailID,
							customer = getdata.Customer.Name,
							Car = getdata.Car.Name,
							Rating = getdata.Rating,
							Remarks = getdata.Remarks,
							IsApproved = getdata.IsApproved.HasValue ? getdata.IsApproved.Value.ToString() : bool.FalseString,
							ID = getdata.ID
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
				{
					getdata.IsApproved = false;
					if (_customerratingService.UpdateCarRating(ref getdata, ref message))
					{
						SuccessMessage = "Rating " + ((bool)getdata.IsApproved ? "Approved" : "Rejected") + "  successfully ...";

						return Json(new
						{
							success = true,
							message = SuccessMessage,
							data = new
							{
								Date = getdata.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
								OrderDetailID = getdata.OrderDetailID,
								customer = getdata.Customer.Name,
								Car = getdata.Car.Name,
								Rating = getdata.Rating,
								Remarks = getdata.Remarks,
								IsApproved = getdata.IsApproved.HasValue ? getdata.IsApproved.Value.ToString() : bool.FalseString,
								ID = getdata.ID
							}
						}, JsonRequestBehavior.AllowGet);
					}
					else
					{
						ErrorMessage = "Oops! Something went wrong. Please try later.";
					}
				}
			}
			return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);

		}
		public ActionResult Details(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			CarRating rating = _customerratingService.GetRating((Int16)id);
			if (rating == null)
			{
				return HttpNotFound();
			}
			return View(rating);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CustomerRatingReport()
		{
			string ImageServer = CustomURL.GetImageServer();

			var getAllCatagories = _customerratingService.GetRatings().ToList();
			if (getAllCatagories.Count() > 0)
			{
				using (ExcelPackage excel = new ExcelPackage())
				{
					excel.Workbook.Worksheets.Add("CustomerRating");

					var headerRow = new List<string[]>()
					{
					new string[] {
						"Creation Date"
						,"Order No"
						,"Customer"
						,"Car"
						,"Remarks"
						,"Images"
						,"Status"
						}
					};

					// Determine the header range (e.g. A1:D1)
					string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

					// Target a worksheet
					var worksheet = excel.Workbook.Worksheets["CustomerRating"];

					// Popular header row data
					worksheet.Cells[headerRange].LoadFromArrays(headerRow);

					var cellData = new List<object[]>();

					if (getAllCatagories.Count != 0)
						getAllCatagories = getAllCatagories.OrderByDescending(x => x.ID).ToList();

					foreach (var i in getAllCatagories)
					{
						string Images = i.CarRatingImages.Count != 0 ? string.Join(", ", i.CarRatingImages.Select(x => ImageServer + x.Image)) : "-";

						cellData.Add(new object[] {
						i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
						,i.OrderDetail.Order.OrderNo
						,i.Customer != null ? (!string.IsNullOrEmpty(i.Customer.Name) ? i.Customer.Name : "-") : "-"
						,i.Car != null ? (!string.IsNullOrEmpty(i.Car.Name) ? i.Car.Name : "-") : "-"
						,!string.IsNullOrEmpty(i.Remarks) ? i.Remarks : "-"
						,Images
						,"Pending"
						});
					}

					worksheet.Cells[2, 1].LoadFromArrays(cellData);

					return File(excel.GetAsByteArray(), "application/msexcel", "Customer Rating Report.xlsx");
				}
			}
			return RedirectToAction("Index");
		}
	}
}





