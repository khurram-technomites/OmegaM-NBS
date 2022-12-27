using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using NowBuySell.Web.ViewModels.Order;
using System.Linq;
using OfficeOpenXml;
using NowBuySell.Web.Helpers.PushNotification;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Areas.VendorPortal.ViewModels.Order;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class OrderController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IOrderService _orderService;
		private readonly IOrderDetailsService _orderdetailService;
		private readonly ICarRatingService _carRatingService;
		private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;
		private readonly IVendorWalletShareService _vendorWalletShareService;
		private readonly ICustomerSessionService _customerSessionService;

		public OrderController(IOrderService orderService, IOrderDetailsService orderdetailService, ICarRatingService carRatingService, INotificationService notificationService, INotificationReceiverService notificationReceiverService, IVendorWalletShareService vendorWalletShareService, ICustomerSessionService customerSessionService)
		{
			this._orderService = orderService;
			this._orderdetailService = orderdetailService;
			this._carRatingService = carRatingService;
			this._notificationService = notificationService;
			this._notificationReceiverService = notificationReceiverService;
			this._vendorWalletShareService = vendorWalletShareService;
			this._customerSessionService = customerSessionService;
		}

		public ActionResult Index()
		{
			ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
			ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-30).ToString("MM/dd/yyyy");
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			ViewBag.ErrorMessage = TempData["ErrorMessage"];
			return View();
		}

		public ActionResult List()
		{

			DateTime ToDate = Helpers.TimeZone.GetLocalDateTime();
			DateTime FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-30);
			var orders = _orderService.GetOrdersDateWise(FromDate, ToDate);
			return PartialView(orders);
		}

		[HttpPost]
		public ActionResult List(DateTime fromDate, DateTime ToDate)
		{
			DateTime EndDate = ToDate.AddMinutes(1439);
			var orders = _orderService.GetOrdersDateWise(fromDate, EndDate);
			return PartialView(orders);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PendingOrdersReport(DateTime fromDate, DateTime ToDate)
		{
			DateTime EndDate = ToDate.AddMinutes(1439);
			var getAllOrders = _orderService.GetOrdersDateWise(fromDate, EndDate);
			if (getAllOrders.Count() > 0)
			{
				using (ExcelPackage excel = new ExcelPackage())
				{
					excel.Workbook.Worksheets.Add("PendingBookings");

					var headerRow = new List<string[]>()
					{
					new string[] {
						"Creation Date"
						,"Booking No"
						,"Vendor Name"
						,"Customer Name"
						,"Customer Contact"
						,"Status"
						,"Car Name"
						,"Package Name"
						,"Booking Status"
						,"Delivery Address"
						,"Coupon Code"
						,"Coupon Discount"
						//,"Redeem Amount"
						,"Subtotal"
						//,"Tax"
						//,"Tax Percentage"
						,"Payment Status"
						,"Grand Total"
						,"Start Date"
						,"End Date"
						,"Extra Kilometer"
						,"Extra Kilometer Price"
						}
					};

					// Determine the header range (e.g. A1:D1)
					string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

					// Target a worksheet
					var worksheet = excel.Workbook.Worksheets["PendingBookings"];

					// Popular header row data
					worksheet.Cells[headerRange].LoadFromArrays(headerRow);

					var cellData = new List<object[]>();

					foreach (var i in getAllOrders)
					{


						string DeliveryAddress = "-";
						try
						{
							var deliveryAddress = i.OrderDeliveryAddresses.FirstOrDefault();
							if (deliveryAddress != null)
							{
								DeliveryAddress = deliveryAddress.Address;
								DeliveryAddress += deliveryAddress.Area != null ? "," + deliveryAddress.Area.Name : "";
								DeliveryAddress += deliveryAddress.City != null ? "," + deliveryAddress.City.Name : "";
								DeliveryAddress += deliveryAddress.Country != null ? "," + deliveryAddress.Country.Name : "";


							}

						}


						catch (Exception e)
						{
							Console.Write(e);
						}

						foreach (var details in i.OrderDetails)
						{


							cellData.Add(new object[] {
						i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
						,!string.IsNullOrEmpty(i.OrderNo) ? i.OrderNo : "-"
						,details.Vendor.Name
						,i.Customer != null ? !string.IsNullOrEmpty(i.Customer.Name) ? i.Customer.Name : "-" : "-"
						,i.Customer != null ? !string.IsNullOrEmpty(i.Customer.Contact) ? i.Customer.Contact : "-" : "-"
						,!string.IsNullOrEmpty(i.Status) ? i.Status : "-"
						,details.Car.Name
						,details.PackageName
						,!string.IsNullOrEmpty(DeliveryAddress) ? DeliveryAddress : "-"
						,!string.IsNullOrEmpty(i.CouponCode) ? i.CouponCode : "-"
						,!string.IsNullOrEmpty(i.Currency) ? i.Currency +" "+ (i.CouponDiscount ?? 0) : ""+ (i.CouponDiscount ?? 0)
						//,!string.IsNullOrEmpty(i.Currency) ? i.Currency +" "+ (i.RedeemAmount ?? 0) : ""+ (i.RedeemAmount ?? 0)
						,!string.IsNullOrEmpty(i.Currency) ? i.Currency +" "+ (i.Amount ?? 0) : ""+ (i.Amount ?? 0)
						//,!string.IsNullOrEmpty(i.Currency) ? i.Currency +" "+ (i.TaxAmount ?? 0) : ""+ (i.TaxAmount ?? 0)
						//,!string.IsNullOrEmpty(i.Currency) ? i.Currency +" "+ (i.TaxPercent ?? 0) : ""+ (i.TaxPercent ?? 0)
						//,i.TaxPercent + " %"
						,i.IsPaid != false ? "Paid" : "Unpaid"
						,!string.IsNullOrEmpty(i.Currency) ? i.Currency +" "+ (i.TotalAmount ?? 0) : ""+ (i.TotalAmount ?? 0)
						,details.StartDateTime
						,details.EndDateTime
						,details.ExtraKilometer
						,details.ExtraKilometerPrice

						});
						}
					}

					worksheet.Cells[2, 1].LoadFromArrays(cellData);

					return File(excel.GetAsByteArray(), "application/msexcel", "Pending Bookings Report.xlsx");
				}
			}
			else
			{
				TempData["ErrorMessage"] = "No orders in this date range!";

			}
			return RedirectToAction("Index");
		}

		public ActionResult CompletedOrders()
		{
			ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
			ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-30).ToString("MM/dd/yyyy");
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			ViewBag.ErrorMessage = TempData["ErrorMessage"];
			return View();
		}

		public ActionResult COList()
		{
			DateTime ToDate = Helpers.TimeZone.GetLocalDateTime();
			DateTime FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-30);
			var orders = _orderService.GetCompletedOrdersDateWise(FromDate, ToDate);
			return PartialView(orders);
		}

		[HttpPost]
		public ActionResult COList(DateTime fromDate, DateTime ToDate)
		{
			DateTime EndDate = ToDate.AddMinutes(1439);
			var orders = _orderService.GetCompletedOrdersDateWise(fromDate, EndDate);
			return PartialView(orders);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CompletedOrdersReport(DateTime fromDate, DateTime ToDate)
		{
			DateTime EndDate = ToDate.AddMinutes(1439);
			var getAllOrders = _orderService.GetCompletedOrdersDateWise(fromDate, EndDate);
			if (getAllOrders.Count() > 0)
			{
				using (ExcelPackage excel = new ExcelPackage())
				{
					excel.Workbook.Worksheets.Add("CompletedBookings");

					var headerRow = new List<string[]>()
					{
					new string[] {
						"Creation Date"
						,"Booking No"
						,"Vendor Name"
						,"Customer Name"
						,"Customer Contact"
						,"Status"
						,"Car Name"
						,"Package Name"
						,"Booking Status"
						,"Delivery Address"
						,"Coupon Code"
						,"Coupon Discount"
						//,"Redeem Amount"
						,"Subtotal"
						//,"Tax"
						//,"Tax Percentage"
						,"Payment Status"
						,"Grand Total"
						,"Start Date"
						,"End Date"
						,"Extra Kilometer"
						,"Extra Kilometer Price"
						}
					};

					// Determine the header range (e.g. A1:D1)
					string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

					// Target a worksheet
					var worksheet = excel.Workbook.Worksheets["CompletedBookings"];

					// Popular header row data
					worksheet.Cells[headerRange].LoadFromArrays(headerRow);

					var cellData = new List<object[]>();

					foreach (var i in getAllOrders)
					{


						string DeliveryAddress = "-";
						try
						{
							var deliveryAddress = i.OrderDeliveryAddresses.FirstOrDefault();
							if (deliveryAddress != null)
							{
								DeliveryAddress = deliveryAddress.Address;
								DeliveryAddress += deliveryAddress.Area != null ? "," + deliveryAddress.Area.Name : "";
								DeliveryAddress += deliveryAddress.City != null ? "," + deliveryAddress.City.Name : "";
								DeliveryAddress += deliveryAddress.Country != null ? "," + deliveryAddress.Country.Name : "";


							}

						}


						catch (Exception e) { Console.Write(e); }

						foreach (var details in i.OrderDetails)
						{


							cellData.Add(new object[] {
						i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
						,!string.IsNullOrEmpty(i.OrderNo) ? i.OrderNo : "-"
						,details.Vendor.Name
						,i.Customer != null ? !string.IsNullOrEmpty(i.Customer.Name) ? i.Customer.Name : "-" : "-"
						,i.Customer != null ? !string.IsNullOrEmpty(i.Customer.Contact) ? i.Customer.Contact : "-" : "-"
						,!string.IsNullOrEmpty(i.Status) ? i.Status : "-"
						,details.Car.Name
						,details.PackageName
						,!string.IsNullOrEmpty(DeliveryAddress) ? DeliveryAddress : "-"
						,!string.IsNullOrEmpty(i.CouponCode) ? i.CouponCode : "-"
						,!string.IsNullOrEmpty(i.Currency) ? i.Currency +" "+ (i.CouponDiscount ?? 0) : ""+ (i.CouponDiscount ?? 0)
						//,!string.IsNullOrEmpty(i.Currency) ? i.Currency +" "+ (i.RedeemAmount ?? 0) : ""+ (i.RedeemAmount ?? 0)
						,!string.IsNullOrEmpty(i.Currency) ? i.Currency +" "+ (i.Amount ?? 0) : ""+ (i.Amount ?? 0)
						//,!string.IsNullOrEmpty(i.Currency) ? i.Currency +" "+ (i.TaxAmount ?? 0) : ""+ (i.TaxAmount ?? 0)
						//,!string.IsNullOrEmpty(i.Currency) ? i.Currency +" "+ (i.TaxPercent ?? 0) : ""+ (i.TaxPercent ?? 0)
						//,i.TaxPercent + " %"
						,i.IsPaid != false ? "Paid" : "Unpaid"
						,!string.IsNullOrEmpty(i.Currency) ? i.Currency +" "+ (i.TotalAmount ?? 0) : ""+ (i.TotalAmount ?? 0)
						,details.StartDateTime
						,details.EndDateTime
						,details.ExtraKilometer
						,details.ExtraKilometerPrice

						});
						}
					}

					worksheet.Cells[2, 1].LoadFromArrays(cellData);

					return File(excel.GetAsByteArray(), "application/msexcel", "Completed Bookings Report.xlsx");
				}
			}
			return RedirectToAction("Index");
		}

		public ActionResult ListReport()
		{
			var orders = _orderService.GetOrders();
			return View(orders);
		}

		public ActionResult Details(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Order order = _orderService.GetOrder((long)id);
			IEnumerable<SP_GetOrderDetails_Result> OrderDetails = _orderService.GetOrderByOrderID((Int16)id);
			OrderDetailViewModel Details = new OrderDetailViewModel();

			Details.CreatedOn = (DateTime)order.CreatedOn;
			Details.OrderNo = order.OrderNo;
			Details.Amount = order.Amount.HasValue ? order.Amount.Value : 0;
			Details.DeliveryAddress = order.DeliveryAddress;
			Details.DeliveryDate = order.DeliveryDate;
			Details.CancelationReason = order.CancelationReason;
			Details.Status = order.Status;
			Details.Vendor = order.OrderDetails.Count != 0 ? (order.OrderDetails.FirstOrDefault().Vendor != null ? order.OrderDetails.FirstOrDefault().Vendor.Name : "-") : "-";
			Details.ShipmentStatus = order.ShipmentStatus;
			Details.CustomerName = order.Customer.Name;
			Details.Currency = order.Currency;
			Details.OrderTaxAmount = order.TaxAmount.HasValue ? order.TaxAmount.Value : 0;
			Details.OrderTaxPercent = order.TaxPercent.HasValue ? order.TaxPercent.Value : 0;
			Details.DiscountAmount = order.DiscountAmount.HasValue ? order.DiscountAmount.Value : 0;
			Details.DiscountPercent = order.DiscountPercent.HasValue ? order.DiscountPercent.Value : 0;
			Details.CouponCode = string.IsNullOrEmpty(order.CouponCode) ? "-" : order.CouponCode;
			Details.CouponDiscount = order.CouponDiscount.HasValue ? order.CouponDiscount.Value : 0;
			Details.RedeemAmount = order.RedeemAmount.HasValue ? order.RedeemAmount.Value : 0;
			Details.Shipping = order.DeliveryCharges.HasValue ? order.DeliveryCharges.Value : 0;
			Details.TotalAmount = order.TotalAmount.HasValue ? order.TotalAmount.Value : 0;
			Details.Contact = order.Customer.Contact;
			Details.orderID = (long)id;

			var deliveryAddress = order.OrderDeliveryAddresses.FirstOrDefault();
			if (deliveryAddress != null)
			{
				Details.Country = deliveryAddress.Country != null ? deliveryAddress.Country.Name : "-";
				Details.City = deliveryAddress.City != null ? deliveryAddress.City.Name : "-";
				Details.Area = deliveryAddress.Area != null ? deliveryAddress.Area.Name : "-";
				Details.Address = deliveryAddress.Address;
				Details.Contact = deliveryAddress.Contact;
			}
			Details.orderdetails = OrderDetails;

			if (Details == null)
			{
				return HttpNotFound();
			}

			ViewBag.Referrer = Request.UrlReferrer;
			return View(Details);
		}

		public ActionResult Comments(long id)
		{
			var OrderDetails = _carRatingService.GetOrderDetailRating(id);

			return View(OrderDetails);
		}

		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(Order order)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (_orderService.CreateOrder(order, ref message))
				{
					return Json(new
					{
						success = true,
						url = "/Admin/Order/Index",
						message = message,
						data = new
						{
							ID = order.ID,
							Date = order.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
							OrderNo = order.OrderNo,
							PaymentMethod = order.PaymentMethod,
							CouponCode = order.CouponCode,
							DeliveryAddress = order.DeliveryAddress,
							CancelationReason = order.CancelationReason,
							Status = order.Status,
							Currency = order.Currency,


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
			Order order = _orderService.GetOrder((long)id);
			if (order == null)
			{
				return HttpNotFound();
			}

			TempData["OrderID"] = id;
			return View(order);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(Order order)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				long Id;
				if (TempData["OrderID"] != null && Int64.TryParse(TempData["OrderID"].ToString(), out Id) && order.ID == Id)
				{
					if (_orderService.UpdateOrder(ref order, ref message))
					{
						return Json(new
						{
							success = true,
							url = "/Admin/Order/Index",
							message = "Bookings updated successfully ...",
							data = new
							{
								ID = order.ID,
								Date = order.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
								OrderNo = order.OrderNo,
								PaymentMethod = order.PaymentMethod,
								CouponCode = order.CouponCode,
								DeliveryAddress = order.DeliveryAddress,
								CancelationReason = order.CancelationReason,
								Status = order.Status,
								Currency = order.Currency,

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

		public ActionResult StatusChange(long id)
		{
			Order order = _orderService.GetOrder((long)id);
			return View(order);
		}

		[HttpPost]
		public ActionResult StatusChange(Order orderID, string status)
		{
			string message = string.Empty;
			Order order = _orderService.GetOrder((long)orderID.ID);
			order.Status = status;
			if (_orderService.UpdateOrder(ref order, ref message))
			{
				if (order.Status == "Completed")
				{
					//_vendorWalletShareService.UpdateVendorEarning(orderID.ID);
				}

				if (order.Status == "Confirmed")
				{
					/*Order Email*/
					var OrderModel = _orderService.GetOrder((long)orderID.ID);
					IEnumerable<SP_GetOrderDetails_Result> OrderDetails = _orderService.GetOrderByOrderID((Int16)OrderModel.ID);
					OrderDetailViewModel Details = new OrderDetailViewModel();

					Details.CreatedOn = (DateTime)OrderModel.CreatedOn;
					Details.OrderNo = OrderModel.OrderNo;
					Details.DeliveryAddress = OrderModel.DeliveryAddress;
					Details.Status = OrderModel.Status;
					Details.ShipmentStatus = OrderModel.ShipmentStatus;
					Details.CustomerName = OrderModel.Customer.Name;
					Details.Currency = OrderModel.Currency;

					Details.Amount = OrderModel.Amount.HasValue ? OrderModel.Amount.Value : 0;

					Details.OrderTaxPercent = OrderModel.TaxPercent.HasValue ? OrderModel.TaxPercent.Value : 0;
					Details.OrderTaxAmount = OrderModel.TaxAmount.HasValue ? OrderModel.TaxAmount.Value : 0;

					Details.Shipping = OrderModel.DeliveryCharges.HasValue ? OrderModel.DeliveryCharges.Value : 0;
					Details.CouponDiscount = OrderModel.CouponDiscount.HasValue ? OrderModel.CouponDiscount.Value : 0m;
					Details.CouponCode = OrderModel.CouponCode != null ? OrderModel.CouponCode : "-";
					Details.RedeemAmount = OrderModel.RedeemAmount.HasValue ? OrderModel.RedeemAmount.Value : 0m;

					Details.TotalAmount = OrderModel.TotalAmount.HasValue ? OrderModel.TotalAmount.Value : 0;

					var deliveryAddress = OrderModel.OrderDeliveryAddresses.FirstOrDefault();
					if (deliveryAddress != null)
					{
						Details.Country = deliveryAddress.Country != null ? deliveryAddress.Country.Name : "-";
						Details.City = deliveryAddress.City != null ? deliveryAddress.City.Name : "-";
						Details.Area = deliveryAddress.Area != null ? deliveryAddress.Area.Name : "-";
						Details.Address = deliveryAddress.Address;
					}

					Details.orderdetails = OrderDetails;

					var body = ViewToStringRenderer.RenderViewToString(this.ControllerContext, "~/Views/Orders/Details.cshtml", Details);

					_orderService.SendOrderEmail(order.Customer.Email, "NowBuySell | Booking Placed", body);
				}

				Notification not = new Notification();

				if (order.Status == "Pending")
				{
					not.Title = "Bookings Placed";
					not.TitleAr = "Bookings Placed";
					not.Description = string.Format("Your booking # {0} has been placed. You can check the booking status via booking details", order.OrderNo);
					not.DescriptionAr = string.Format("Your booking # {0} has been placed. You can check the booking status via booking details", order.OrderNo);

				}
				else if (order.Status == "Confirmed")
				{

					not.Title = "Bookings Confirmed";
					not.TitleAr = "Bookings Confirmed";
					not.Description = string.Format("Your booking # {0} has been confirmed. You can check the booking status via booking details", order.OrderNo);
					not.DescriptionAr = string.Format("Your booking # {0} has been confirmed. You can check the booking status via booking details", order.OrderNo);

				}
				else if (order.Status == "Processing")
				{
					not.Title = "Bookings Processed";
					not.TitleAr = "Bookings Processed";
					not.Description = string.Format("Your booking # {0} has been processed. You can check the booking status via booking details", order.OrderNo);
					not.DescriptionAr = string.Format("Your booking # {0} has been processed. You can check the booking status via booking details", order.OrderNo);
				}
				else if (order.Status == "Completed")
				{
					not.Title = "Bookings Completed";
					not.TitleAr = "Bookings Completed";
					not.Description = string.Format("Your booking # {0} is completed. You can check the booking status via booking details", order.OrderNo);
					not.DescriptionAr = string.Format("Your booking # {0} is completed. You can check the booking status via booking details", order.OrderNo);

				}
				else if (order.Status == "Canceled")
				{
					not.Title = "Bookings Canceled";
					not.TitleAr = "Bookings Canceled";
					not.Description = string.Format("Your booking # {0} has been canceled. You can check the booking status via booking details", order.OrderNo);
					not.DescriptionAr = string.Format("Your booking # {0} has been canceled. You can check the booking status via booking details", order.OrderNo);

				}

				not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
				not.OriginatorName = Session["UserName"].ToString();
				not.Module = "Booking";
				not.OriginatorType = "Admin";
				not.RecordID = order.ID;
				if (_notificationService.CreateNotification(not, ref message))
				{
					NotificationReceiver notRec = new NotificationReceiver();
					notRec.ReceiverID = order.CustomerID;
					notRec.ReceiverType = "Customer";
					notRec.NotificationID = not.ID;
					if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
					{
						var tokens = _customerSessionService.GetCustomerSessionFirebaseTokens((long)order.CustomerID, true, null);
						var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
						{
							Module = "Booking",
							RecordID = order.ID,
							OrderNo = order.OrderNo,
							NotificationID = notRec.ID
						});
					}
				}
				return Json(new
				{
					success = true,
					url = "/Admin/Order/Index",
					message = "Status updated successfully ...",
					data = new
					{
						ID = order.ID,
						Date = order.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
						OrderNo = order.OrderNo,
						Customer = new { Name = order.Customer != null ? order.Customer.Name : null, Contact = order.Customer != null ? order.Customer.Contact : null },
						Vendor = order.OrderDetails.FirstOrDefault().Vendor.Name,
						TotalAmount = order.TotalAmount,
						Currency = order.Currency,
						ShipmentStatus = order.ShipmentStatus,
						Status = order.Status,


					}
				});
			}
			return View("Index");
		}

		[HttpPost]
		public ActionResult GetVendorOrders(OrderFilterViewModel orderFilter)
		{
			try
			{

				var orders = _orderService.GetFilteredOrders(orderFilter.StartDate, orderFilter.EndDate, orderFilter.VendorId, orderFilter.Status, orderFilter.PageNumber.HasValue ? orderFilter.PageNumber.Value : 1, orderFilter.SortBy.HasValue ? orderFilter.SortBy.Value : 1);

				return Json(new
				{
					success = true,
					data = orders
				});
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
	}
}