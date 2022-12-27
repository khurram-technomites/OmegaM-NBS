using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LinqToExcel;
using NowBuySell.Web.Helpers.POCO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OfficeOpenXml;
using NowBuySell.Web.Helpers.PushNotification;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class CouponController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly ICouponService _couponService;
		private readonly ICustomerService _customerService;
		private readonly ICustomerCouponsService _customerCouponsService;
		private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;
		private readonly ICustomerSessionService _customerSessionService;


		public CouponController(ICouponService couponService, ICustomerService customerService, ICustomerCouponsService customerCouponsService, INotificationService notificationService, INotificationReceiverService notificationReceiverService, ICustomerSessionService customerSessionService)
		{
			this._couponService = couponService;
			this._customerService = customerService;
			this._customerCouponsService = customerCouponsService;
			this._notificationService = notificationService;
			this._notificationReceiverService = notificationReceiverService;
			this._customerSessionService = customerSessionService;
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
			var coupons = _couponService.GetCoupons(null);
			return PartialView(coupons);
		}

		public ActionResult ListReport()
		{
			var coupons = _couponService.GetCoupons(null);
			return View(coupons);
		}

		public ActionResult Details(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Coupon coupon = _couponService.GetCoupon((Int16)id);
			if (coupon == null)
			{
				return HttpNotFound();
			}
			return View(coupon);
		}

		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(Coupon coupon)
		{
			if (coupon.IsOpenToAll == null)
			{
				coupon.IsOpenToAll = false;
			}
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (_couponService.CreateCoupon(ref coupon, ref message))
				{
					if (coupon.IsOpenToAll.HasValue && coupon.IsOpenToAll.Value)
					{
						Notification not = new Notification();
						if (!coupon.DicountAmount.HasValue)
						{
							not.Title = coupon.DicountPercentage + "% Discount";
							not.TitleAr = coupon.DicountPercentage + "% Discount";
							not.Description = string.Format("{0}% off your very first purchase, use promo code: {1}", coupon.DicountPercentage, coupon.CouponCode);
							not.DescriptionAr = string.Format("{0}% off your very first purchase, use promo code: {1}", coupon.DicountPercentage, coupon.CouponCode);
							not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
							not.OriginatorName = Session["UserName"].ToString();
							not.Module = "Coupon";
							not.OriginatorType = "Admin";
							not.RecordID = coupon.ID;
						}
						else
						{
							not.Title = coupon.DicountAmount + " AED Discount";
							not.TitleAr = coupon.DicountAmount + " AED Discount";
							not.Description = string.Format("{0} AED off your very first purchase, use promo code: {1}", coupon.DicountAmount, coupon.CouponCode);
							not.DescriptionAr = string.Format("{0} AED off your very first purchase, use promo code: {1}", coupon.DicountAmount, coupon.CouponCode);
							not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
							not.OriginatorName = Session["UserName"].ToString();
							not.Module = "Coupon";
							not.OriginatorType = "Admin";
							not.RecordID = coupon.ID;
						}

						if (_notificationService.CreateNotification(not, ref message))
						{
							_notificationReceiverService.NotifyCoupons(not.ID, null, string.Empty);

							var notRecs = _notificationReceiverService.GetAllByNotificationId(not.ID);
							foreach (var notRec in notRecs)
							{
								var tokens = _customerSessionService.GetCustomerSessionFirebaseTokens((long)notRec.ReceiverID, null, true);

								var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
								{
									Module = "Coupon",
									RecordID = coupon.ID,
									NotificationID = notRec.ID
								});
							}

						}
					}
					return Json(new
					{
						success = true,
						url = "/Admin/Coupon/Index",
						message = message,
						data = new
						{
							ID = coupon.ID,
							Date = coupon.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
							CouponCode = coupon.CouponCode,
							Name = coupon.Name,
							Discount = coupon.DicountPercentage,
							Type = coupon.Type,
							Value = coupon.Value,
							IsActive = coupon.IsActive.HasValue ? coupon.IsActive.Value.ToString() : bool.FalseString
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
			Coupon coupon = _couponService.GetCoupon((long)id);
			if (coupon == null)
			{
				return HttpNotFound();
			}

			TempData["CouponID"] = id;
			return View(coupon);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(Coupon coupon)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				long Id;
				if (TempData["CouponID"] != null && Int64.TryParse(TempData["CouponID"].ToString(), out Id) && coupon.ID == Id)
				{
					bool isNotifyRequired = false;
					if (_couponService.UpdateCoupon(ref coupon, ref message, ref isNotifyRequired))
					{
						if (isNotifyRequired)
						{
							if (coupon.IsOpenToAll.HasValue && coupon.IsOpenToAll.Value)
							{
								Notification not = new Notification();
								if (!coupon.DicountAmount.HasValue)
								{
									not.Title = coupon.DicountPercentage + "% Discount";
									not.TitleAr = coupon.DicountPercentage + "% Discount";
									not.Description = string.Format("{0}% off your very first purchase, use promo code: {1}", coupon.DicountPercentage, coupon.CouponCode);
									not.DescriptionAr = string.Format("{0}% off your very first purchase, use promo code: {1}", coupon.DicountPercentage, coupon.CouponCode);
									not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
									not.OriginatorName = Session["UserName"].ToString();
									not.Module = "Coupon";
									not.OriginatorType = "Admin";
									not.RecordID = coupon.ID;
								}
								else
								{
									not.Title = coupon.DicountAmount + " AED Discount";
									not.TitleAr = coupon.DicountAmount + " AED Discount";
									not.Description = string.Format("{0} AED off your very first purchase, use promo code: {1}", coupon.DicountAmount, coupon.CouponCode);
									not.DescriptionAr = string.Format("{0} AED off your very first purchase, use promo code: {1}", coupon.DicountAmount, coupon.CouponCode);
									not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
									not.OriginatorName = Session["UserName"].ToString();
									not.Module = "Coupon";
									not.OriginatorType = "Admin";
									not.RecordID = coupon.ID;
								}

								if (_notificationService.CreateNotification(not, ref message))
								{
									_notificationReceiverService.NotifyCoupons(not.ID, coupon.ID, "Coupon");
								}
							}
							else
							{
								_notificationReceiverService.RemoveNotification(coupon.ID, "Coupon", null);
							}
						}

						return Json(new
						{
							success = true,
							url = "/Admin/Coupon/Index",
							message = "Coupon updated successfully ...",
							data = new
							{
								ID = coupon.ID,
								Date = coupon.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
								CouponCode = coupon.CouponCode,
								Name = coupon.Name,
								Discount = coupon.DicountPercentage,
								Type = coupon.Type,
								Value = coupon.Value,
								IsActive = coupon.IsActive.HasValue ? coupon.IsActive.Value.ToString() : bool.FalseString

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
			var coupon = _couponService.GetCoupon((long)id);
			if (coupon == null)
			{
				return HttpNotFound();
			}

			if (!(bool)coupon.IsActive)
				coupon.IsActive = true;
			else
			{
				coupon.IsActive = false;
			}
			string message = string.Empty;
			bool isNitifyRequired = false;
			if (_couponService.UpdateCoupon(ref coupon, ref message, ref isNitifyRequired))
			{
				SuccessMessage = "Coupon " + ((bool)coupon.IsActive ? "activated" : "deactivated") + "  successfully ...";
				return Json(new
				{
					success = true,
					message = SuccessMessage,
					data = new
					{
						ID = coupon.ID,
						Date = coupon.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
						CouponCode = coupon.CouponCode,
						Name = coupon.Name,
						Discount = coupon.DicountPercentage,
						Type = coupon.Type,
						Value = coupon.Value,
						IsActive = coupon.IsActive.HasValue ? coupon.IsActive.Value.ToString() : bool.FalseString

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
			Coupon coupon = _couponService.GetCoupon((Int16)id);
			if (coupon == null)
			{
				return HttpNotFound();
			}
			TempData["CouponID"] = id;
			return View(coupon);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(long id)
		{
			string message = string.Empty;
			if (_couponService.DeleteCoupon((Int16)id, ref message))
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

						string sheetName = "BulkCoupons";
						var realEstateID = Convert.ToInt64(Session["RealEstateID"]);

						int count = 1;
						try
						{
							var excelFile = new ExcelQueryFactory(pathToExcelFile);
							var tenants = from a in excelFile.Worksheet<CouponsWorkSheet>(sheetName) select a;
							foreach (var item in tenants)
							{
								var results = new List<ValidationResult>();
								var context = new ValidationContext(item, null, null);
								if (Validator.TryValidateObject(item, context, results))
								{
									if (_couponService.PostExcelData(item.Name, item.CouponCode, item.Frequency, item.Type, item.Value, item.MaxAmount, item.Expiry, item.IsOpenToAll == "Yes" ? true : false))
									{
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

						TempData["SuccessMessage"] = string.Format("{0} Coupons inserted!", (count - 1) - ErrorItems.Count());

						if (ErrorItems.Count() > 0)
						{
							TempData["ErrorMessage"] = string.Format("{0} Coupons not inserted!", ErrorItems.Count());
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

		public ActionResult SetCoupons(long id)
		{
			ViewBag.CustomerID = new SelectList(_customerService.GetCustomersForDropDown(), "value", "text");
			ViewBag.CouponsID = id;
			return View();
		}

		[HttpGet]
		public ActionResult SetCouponsByCouponID(long CouponsID)
		{
			try
			{
				var coupons = _customerCouponsService.GetCouponsByCouponID(CouponsID);
				return Json(new
				{
					success = true,
					data = coupons.Select(x => new
					{
						id = x.CustomerID,
						email = x.Customer.Email
					})
				}, JsonRequestBehavior.AllowGet);
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

		[HttpPost]
		public ActionResult SaveCustomerCoupons(long CustomerID, long CouponsID)
		{
			string message = string.Empty;
			CustomerCoupon customerCoupon = new CustomerCoupon();
			customerCoupon.CustomerID = CustomerID;
			customerCoupon.CouponsID = CouponsID;

			if (_customerCouponsService.CreateCustomerCoupon(customerCoupon, ref message))
			{
				var coupon = _couponService.GetCoupon((long)customerCoupon.CouponsID);
				Notification not = new Notification();
				if (coupon.Type == "Percentage")
				{
					not.Title = (coupon.DicountPercentage != null ? coupon.DicountPercentage + "" : coupon.Value + "") + "% Discount";
					not.TitleAr = (coupon.DicountPercentage != null ? coupon.DicountPercentage + "" : coupon.Value + "") + "% Discount";
					not.Description = string.Format("{0}% off your very first purchase, use promo code: {1}", (coupon.DicountPercentage != null ? coupon.DicountPercentage + "" : coupon.Value + ""), coupon.CouponCode);
					not.DescriptionAr = string.Format("{0}% off your very first purchase, use promo code: {1}", (coupon.DicountPercentage != null ? coupon.DicountPercentage + "" : coupon.Value + ""), coupon.CouponCode);
					not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
					not.OriginatorName = Session["UserName"].ToString();
					not.Module = "Coupon";
					not.OriginatorType = "Admin";
				}
				else
				{
					not.Title = (coupon.DicountAmount != null ? coupon.DicountAmount + "" : coupon.Value + "") + " AED Discount";
					not.TitleAr = (coupon.DicountAmount != null ? coupon.DicountAmount + "" : coupon.Value + "") + " AED Discount";
					not.Description = string.Format("{0} AED off your very first purchase, use promo code: {1}", (coupon.DicountAmount != null ? coupon.DicountAmount + "" : coupon.Value + ""), coupon.CouponCode);
					not.DescriptionAr = string.Format("{0} AED off your very first purchase, use promo code: {1}", (coupon.DicountAmount != null ? coupon.DicountAmount + "" : coupon.Value + ""), coupon.CouponCode);
					not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
					not.OriginatorName = Session["UserName"].ToString();
					not.Module = "Coupon";
					not.OriginatorType = "Admin";

				}
				not.RecordID = coupon.ID;

				if (_notificationService.CreateNotification(not, ref message))
				{
					NotificationReceiver notRec = new NotificationReceiver();
					notRec.ReceiverID = customerCoupon.CustomerID;
					notRec.ReceiverType = "Customer";
					notRec.NotificationID = not.ID;
					if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
					{
						var tokens = _customerSessionService.GetCustomerSessionFirebaseTokens((long)customerCoupon.CustomerID, null, true);
						var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
						{
							Module = "Coupon",
							RecordID = coupon.ID,
							NotificationID = notRec.ID
						});
					}
				}
				return Json(new
				{
					success = true,
					message = "Coupon assigned to customer successfully ..."
				});
			}
			return Json(new { success = false, message = message });
		}

		[HttpPost]
		public ActionResult DeleteCustomerCoupons(long CustomerID, long CouponsID)
		{

			try
			{
				string message = string.Empty;
				var coupon = _customerCouponsService.GetCoupon(CustomerID, CouponsID);

				if (_customerCouponsService.DeleteCustomerCoupon((Int16)coupon.ID, ref message))
				{
					return Json(new
					{
						success = true,
						message = "Coupon unassigned from customer successfully ..."
					}, JsonRequestBehavior.AllowGet);
				}
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
				}, JsonRequestBehavior.AllowGet);
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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CouponsReport()
		{
			var getAllCoupons = _couponService.GetCoupons(null).ToList();
			if (getAllCoupons.Count() > 0)
			{
				using (ExcelPackage excel = new ExcelPackage())
				{
					excel.Workbook.Worksheets.Add("CouponsReport");

					var headerRow = new List<string[]>()
					{
					new string[] {
						"Creation Date"
						,"Code"
						,"Name"
						,"Type"
						,"Frequency"
						,"Value"
						,"DicountAmount"
						,"DicountPercentage"
						,"Max Amount"
						,"Expiry"
						,"Is Open To All"
						,"Customers"
						,"Status"
						}
					};

					// Determine the header range (e.g. A1:D1)
					string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

					// Target a worksheet
					var worksheet = excel.Workbook.Worksheets["CouponsReport"];

					// Popular header row data
					worksheet.Cells[headerRange].LoadFromArrays(headerRow);

					var cellData = new List<object[]>();

					if (getAllCoupons.Count != 0)
						getAllCoupons = getAllCoupons.OrderByDescending(x => x.ID).ToList();

					foreach (var i in getAllCoupons)
					{
						var customerCoupans = "-";
						if (i.IsOpenToAll == false && i.CustomerCoupons.Count != 0)
							customerCoupans = string.Join(", ", i.CustomerCoupons.Select(x => x.Customer.Email));

						cellData.Add(new object[] {
						i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
						,!string.IsNullOrEmpty(i.CouponCode) ? i.CouponCode : "-"
						,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
						,!string.IsNullOrEmpty(i.Type) ? i.Type : "-"
						,i.Frequency ?? 0
						,i.Value ?? 0
						,i.DicountAmount ?? 0
						,i.DicountPercentage ?? 0
						,i.MaxAmount ?? 0
						,i.Expiry.HasValue ? i.Expiry.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
						,i.IsOpenToAll == true ? "Yes" : "No"
						, customerCoupans
						,i.IsActive == true ? "Active" : "InActive"
						});
					}

					worksheet.Cells[2, 1].LoadFromArrays(cellData);

					return File(excel.GetAsByteArray(), "application/msexcel", "Coupons Report.xlsx");
				}
			}
			return RedirectToAction("Index");
		}
	}
}