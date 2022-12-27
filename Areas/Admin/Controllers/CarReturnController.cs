using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers.Routing;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizationProvider.AuthorizeAdmin]
	public class CarReturnController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly ICarReturnService _carReturnService;
		private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;
		private readonly IVendorWalletShareService _vendorWalletShareService;

		public CarReturnController(ICarReturnService carReturnService, INotificationService notificationService, INotificationReceiverService notificationReceiverService, IVendorWalletShareService vendorWalletShareService)
		{
			this._carReturnService = carReturnService;
			this._notificationService = notificationService;
			this._notificationReceiverService = notificationReceiverService;
			this._vendorWalletShareService = vendorWalletShareService;
		}

		public ActionResult Index()
		{
			ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
			ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-7).ToString("MM/dd/yyyy");
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			ViewBag.ErrorMessage = TempData["ErrorMessage"];
			return View();
		}

		public ActionResult List()
		{
			var Return = _carReturnService.GetCarReturns();
			return PartialView(Return);
		}

		[HttpPost]
		public ActionResult List(DateTime fromDate, DateTime ToDate)
		{
			DateTime EndDate = ToDate.AddMinutes(1439);
			var Return = _carReturnService.GetCarReturnsDateWise(fromDate, EndDate);
			return PartialView(Return);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CarReturnsReport(DateTime fromDate, DateTime ToDate)
		{
			DateTime EndDate = ToDate.AddMinutes(1439);
			var getAllReturns = _carReturnService.GetCarReturnsDateWise(fromDate, EndDate);
			if (getAllReturns.Count() > 0)
			{
				string ImageServer = CustomURL.GetImageServer();
				using (ExcelPackage excel = new ExcelPackage())
				{
					excel.Workbook.Worksheets.Add("CarReturns");

					var headerRow = new List<string[]>()
					{
					new string[] {
						"Creation Date"
						,"Order No"
						,"Car SKU"
						,"Car Name"
						,"Car Image"
						,"Customer Name"
						,"Customer Contact"
						,"Customer Email"
						,"Return Type"
						,"Reason"
						,"Received Car Images"
						,"Status"
						}
					};

					// Determine the header range (e.g. A1:D1)
					string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

					// Target a worksheet
					var worksheet = excel.Workbook.Worksheets["CarReturns"];

					// Popular header row data
					worksheet.Cells[headerRange].LoadFromArrays(headerRow);

					var cellData = new List<object[]>();

					foreach (var i in getAllReturns)
					{
						string Images = i.CarReturnImages.Count != 0 ? string.Join(", ", i.CarReturnImages.Select(x => ImageServer + x.Image)) : "-";

						cellData.Add(new object[] {
						i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
						,i.OrderDetail != null ? "" +i.OrderDetail.Order != null ? i.OrderDetail.Order.OrderNo : "-" + "" : "-"
						,i.Car != null ? !string.IsNullOrEmpty(i.Car.SKU) ? i.Car.SKU : "-" : "-"
						,i.Car != null ? !string.IsNullOrEmpty(i.Car.Name) ? i.Car.Name : "-" : "-"
						,i.Car != null ? !string.IsNullOrEmpty(i.Car.Thumbnail) ? (ImageServer + i.Car.Thumbnail) : "-" : "-"
						,i.Customer != null ? !string.IsNullOrEmpty(i.Customer.Name) ? i.Customer.Name : "-" : "-"
						,i.Customer != null ? !string.IsNullOrEmpty(i.Customer.Contact) ? i.Customer.Contact : "-" : "-"
						,i.Customer != null ? !string.IsNullOrEmpty(i.Customer.Email) ? i.Customer.Email : "-" : "-"
						,!string.IsNullOrEmpty(i.ReturnMethod) ? i.ReturnMethod : "-"
						,!string.IsNullOrEmpty(i.Reason) ? i.Reason : "-"
						,Images
						,!string.IsNullOrEmpty(i.Status) ? i.Status : "-"
						});
					}

					worksheet.Cells[2, 1].LoadFromArrays(cellData);

					return File(excel.GetAsByteArray(), "application/msexcel", "Car Returns Report.xlsx");
				}
			}
			return RedirectToAction("Index");
		}

		public ActionResult StatusChange(long id)
		{
			var carReturn = _carReturnService.GetCarReturn((long)id);
			return View(carReturn);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult StatusChange(CarReturn carReturn)
		{
			try
			{
				string message = string.Empty;
				if (_carReturnService.UpdateCarReturn(ref carReturn, ref message))
				{
					/*need to add amount substraction from vendor wallet logic */
					//if (carReturn.Status == "Completed")
					//{
					//	_vendorWalletShareService.UpdateVendorEarning(orderID.ID);
					//}

					Notification not = new Notification();

					if (carReturn.Status == "Pending")
					{
						not.Title = "Return Request Placed";
						not.TitleAr = "Return Request  Placed";
						not.Description = string.Format("Your return request # {0} has been placed. You can check the return status via my returns", carReturn.ReturnCode);
						not.DescriptionAr = string.Format("Your return request # {0} has been placed. You can check the return status via my returns", carReturn.ReturnCode);

					}
					else if (carReturn.Status == "Completed")
					{
						not.Title = "Return Request Completed";
						not.TitleAr = "Return Request  Completed";
						not.Description = string.Format("Your return request # {0} has been completed. You can check the return status via my returns", carReturn.ReturnCode);
						not.DescriptionAr = string.Format("Your return request # {0} has been completed. You can check the return status via my returns", carReturn.ReturnCode);
					}
					else if (carReturn.Status == "Canceled")
					{
						not.Title = "Return Request Canceled";
						not.TitleAr = "Return Request  Canceled";
						not.Description = string.Format("Your return request # {0} has been canceled. You can check the return status via my returns", carReturn.ReturnCode);
						not.DescriptionAr = string.Format("Your return request # {0} has been canceled. You can check the return status via my returns", carReturn.ReturnCode);
					}
					else if (carReturn.Status == "Closed")
					{
						not.Title = "Return Request Closed";
						not.TitleAr = "Return Request  Closed";
						not.Description = string.Format("Your return request # {0} has been closed. You can check the return status via my returns", carReturn.ReturnCode);
						not.DescriptionAr = string.Format("Your return request # {0} has been closed. You can check the return status via my returns", carReturn.ReturnCode);
					}

					not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
					not.OriginatorName = Session["UserName"].ToString();
					not.Module = "Booking";
					not.OriginatorType = "Admin";
					not.RecordID = carReturn.ID;
					if (_notificationService.CreateNotification(not, ref message))
					{
						NotificationReceiver notRec = new NotificationReceiver();
						notRec.ReceiverID = carReturn.CustomerID;
						notRec.ReceiverType = "Customer";
						notRec.NotificationID = not.ID;
						if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
						{
						}
					}
					return Json(new
					{
						success = true,
						url = "/Admin/CarReturn/Index",
						message = "Car return request status updated successfully ...",
						data = new
						{
							Date = carReturn.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
							OrderNo = carReturn.OrderDetail.Order.OrderNo,
							Customer = carReturn.Customer.Name,
							Car = carReturn.Car.Name,
							Status = carReturn.Status,
							ID = carReturn.ID
						}
					});
				}
				else
				{

					return Json(new
					{
						success = false,
						message = "Oops! Something went wrong. Please try later."
					}, JsonRequestBehavior.AllowGet);
				}
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
				}, JsonRequestBehavior.AllowGet);
			}
		}

		public ActionResult Details(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			CarReturn rating = _carReturnService.GetCarReturn((Int16)id);
			if (rating == null)
			{
				return HttpNotFound();
			}
			return View(rating);
		}
	}
}