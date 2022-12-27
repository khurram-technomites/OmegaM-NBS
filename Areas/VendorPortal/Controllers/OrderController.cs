using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.ViewModels.Order;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NowBuySell.Web.Helpers.PushNotification;
using NowBuySell.Web.Helpers;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
	[AuthorizeVendor]
	public class OrderController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IOrderService _orderService;
		private readonly IOrderDetailsService _orderdetailService;
		private readonly ICarRatingService _carRatingService;
		private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;
		private readonly ICustomerSessionService _customerSessionService;

		private readonly IVendorService _vendorService;
		private readonly IVendorWalletShareService _vendorWalletShareService;
		private readonly IVendorWalletShareHistoryService _vendorwalletsharehistoryService;
		private readonly IVendorWalletShareHistoryOrdersService _vendorWalletShareHistoryOrdersService;

		public OrderController(IOrderService orderService, IOrderDetailsService orderdetailService, ICarRatingService carRatingService, INotificationService notificationService, INotificationReceiverService notificationReceiverService, ICustomerSessionService customerSessionService, IVendorService vendorService, IVendorWalletShareService vendorWalletShareService, IVendorWalletShareHistoryService vendorwalletsharehistoryService, IVendorWalletShareHistoryOrdersService vendorWalletShareHistoryOrdersService)
		{
			this._orderService = orderService;
			this._orderdetailService = orderdetailService;
			this._carRatingService = carRatingService;
			this._notificationReceiverService = notificationReceiverService;
			this._notificationService = notificationService;
			this._customerSessionService = customerSessionService;

			this._vendorService = vendorService;
			this._vendorWalletShareService = vendorWalletShareService;
			this._vendorwalletsharehistoryService = vendorwalletsharehistoryService;
			this._vendorWalletShareHistoryOrdersService = vendorWalletShareHistoryOrdersService;
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
			DateTime EndDate = Helpers.TimeZone.GetLocalDateTime();
			DateTime fromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-30);
			var VendorID = Convert.ToInt64(Session["VendorID"]);
			var orders = _orderService.GetVendorOrdersDateWise(VendorID, fromDate, EndDate);
			return PartialView(orders);
		}

		[HttpPost]
		public ActionResult List(DateTime fromDate, DateTime ToDate)
		{
			DateTime EndDate = ToDate.AddMinutes(1439);
			var VendorID = Convert.ToInt64(Session["VendorID"]);
			var orders = _orderService.GetVendorOrdersDateWise(VendorID, fromDate, EndDate);
			return PartialView(orders);
		}

		public ActionResult CompletedOrders()
		{
			ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
			ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-7).ToString("MM/dd/yyyy");
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			ViewBag.ErrorMessage = TempData["ErrorMessage"];
			return View();
		}

		public ActionResult COList()
		{
			var VendorID = Convert.ToInt64(Session["VendorID"]);
			var orders = _orderService.GetVendorOrderscomp(VendorID);
			return PartialView(orders);
		}

		[HttpPost]
		public ActionResult COList(DateTime fromDate, DateTime ToDate)
		{
			DateTime EndDate = ToDate.AddMinutes(1439);
			var VendorID = Convert.ToInt64(Session["VendorID"]);
			var orders = _orderService.GetVendorOrderscompDateWise(VendorID, fromDate, ToDate);
			return PartialView(orders);
		}

		public ActionResult Details(long? id)
		{
			long test = (long)(Session["VendorID"]);
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Order order = _orderService.GetOrder((long)id);
			IEnumerable<SP_GetOrderDetails_Result> OrderDetails = _orderService.GetOrderByOrderID((Int16)id);
			IEnumerable<SP_GetOrderDetails_Result> OrderDetailsForVendor = OrderDetails.Where(x => x.VendorID == test);
			OrderDetailViewModel Details = new OrderDetailViewModel();


			Details.CreatedOn = (DateTime)order.CreatedOn;
			Details.OrderNo = order.OrderNo;
			Details.Amount = order.Amount.HasValue ? order.Amount.Value : 0;
			Details.DeliveryAddress = order.DeliveryAddress;
			Details.DeliveryDate = order.DeliveryDate;
			Details.CancelationReason = order.CancelationReason;
			Details.Status = order.Status;
			Details.ShipmentStatus = order.ShipmentStatus;
			Details.CustomerName = order.Customer.Name;
			Details.Contact = order.Customer.Contact;
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
			Details.Amount = order.TotalAmount.HasValue ? order.TotalAmount.Value : 0;
			Details.orderID = (long)id;
			Details.CustomerID = (long)order.CustomerID;







			var deliveryAddress = order.OrderDeliveryAddresses.FirstOrDefault();
			if (deliveryAddress != null)
			{
				Details.Country = deliveryAddress.Country != null ? deliveryAddress.Country.Name : "-";
				Details.City = deliveryAddress.City != null ? deliveryAddress.City.Name : "-";
				Details.Area = deliveryAddress.Area != null ? deliveryAddress.Area.Name : "-";
				Details.Address = deliveryAddress.Address;
				Details.Contact = deliveryAddress.Contact;
			}
			Details.orderdetails = OrderDetailsForVendor;

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

		public ActionResult StatusChange(long ID)

		{
			Order order = _orderService.GetOrder((long)ID);
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
					/*Vendor Wallet Update and Admin Commission Adjustment for Cash Bookings*/
					if (order.PaymentMethod == "Cash" && !order.IsEarningCaptured.HasValue)
					{
						var vendorId = Convert.ToInt64(Session["VendorID"]);
						var vendor = _vendorService.GetVendor(vendorId);

						var vendorWalletShareHistory = new VendorWalletShareHistory()
						{
							VendorID = vendor.ID,
							Description = string.Format("Booking no {0} amount deducted from your wallet (On my way Commission).", order.OrderNo),
							Amount = ((order.TotalAmount * vendor.Commission) / 100),
							Type = 1,

						};

						if (_vendorwalletsharehistoryService.CreateVendorWalletShareHistory(ref vendorWalletShareHistory, ref message))
						{
							var vendorWalletShareHistoryOrder = new VendorWalletShareHistoryOrder()
							{
								VendorWalletShareHistoryID = vendorWalletShareHistory.ID,
								OrderID = order.ID
							};

							_vendorWalletShareHistoryOrdersService.CreateVendorWalletShareHistoryOrder(vendorWalletShareHistoryOrder, ref message);

							order.IsEarningCaptured = true;
							if (_orderService.UpdateOrder(ref order, ref message))
							{
								var VendorWalletShare = _vendorWalletShareService.GetWalletShareByVendor(vendorId);

								VendorWalletShare.TotalEarning += (order.TotalAmount - ((order.TotalAmount * vendor.Commission) / 100));
								VendorWalletShare.PendingAmount -= vendorWalletShareHistory.Amount;

								if (_vendorWalletShareService.UpdateVendorWalletShare(ref VendorWalletShare, ref message))
								{
								}
							}
						}
					}
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

				not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
				not.OriginatorName = Session["VendorUserName"].ToString();
				not.Module = "Booking";
				not.OriginatorType = "Vendor";
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
				var VendorID = Convert.ToInt64(Session["VendorID"]);
				return Json(new
				{
					success = true,
					url = "/Vendor/Order/Index",
					message = "Status updated successfully ...",
					data = new
					{
						ID = order.ID,
						Date = order.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
						OrderNo = order.OrderNo,
						//Customer = new { Name = order.Customer.Name, Contact = order.Customer.Contact },
						TotalAmount = order.OrderDetails.Where(i => i.VendorID == VendorID).Sum(i => i.TotalPrice),
						Currency = order.Currency,
						//ShipmentStatus = order.ShipmentStatus,
						Status = order.Status,
					}
				});
			}
			return View("Index");
		}

		#region Reports

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PendingOrdersReport(DateTime fromDate, DateTime ToDate)
		{
			var VendorID = Convert.ToInt64(Session["VendorID"]);
			DateTime EndDate = ToDate.AddMinutes(1439);
			var getAllOrders = _orderService.GetVendorOrdersDateWise(VendorID, fromDate, EndDate);
			if (getAllOrders.Count() > 0)
			{
				using (ExcelPackage excel = new ExcelPackage())
				{
					excel.Workbook.Worksheets.Add("PendingBookings");

					var headerRow = new List<string[]>()
					{
					new string[] {
						"Creation Date"
						,"Customer"
						,"Customer Contact"
						,"Booking No"
						,"Booking Status"
						,"Car Name"
						,"Package Name"
						,"Coupon Code"
						,"Coupon Discount"
						//,"Redeem Amount"
						,"Subtotal"
						//,"Tax"
						//,"TaxPercent"
						,"Grand Total"
						,"Payment Status"
						,"Delivery Address"
						,"Start Date"
						,"End Date"
						,"SelfPickUp"
						,"ExtraKilometer"
						,"ExtraKilometerPrice"
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
						//var deliveryAddress = i.OrderDeliveryAddress.FirstOrDefault();
						if (!string.IsNullOrEmpty(i.OrderDeliveryAddress))
						{
							DeliveryAddress = i.OrderDeliveryAddress;
							DeliveryAddress += i.AreaName != null ? "," + i.AreaName : "";
							DeliveryAddress += i.CityName != null ? "," + i.CityName : "";
							DeliveryAddress += i.CountryName != null ? "," + i.CountryName : "";

						}

						var orderDetails = new Order();

						orderDetails = _orderService.GetOrder(i.ID);
						if (orderDetails == null)
							orderDetails = new Order();

						cellData.Add(new object[] {
						i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
						,!string.IsNullOrEmpty(i.CustomerName) ? i.CustomerName : "-"
						,!string.IsNullOrEmpty(i.CustomerContact) ? i.CustomerContact : "-"
						,!string.IsNullOrEmpty(i.OrderNo) ? i.OrderNo : "-"

						,!string.IsNullOrEmpty(i.Status) ? i.Status : "-"
						,!string.IsNullOrEmpty(i.CarName) ? i.CarName : "-"
						,!string.IsNullOrEmpty(i.PackageName) ? i.PackageName : "-"
						,!string.IsNullOrEmpty(orderDetails.CouponCode) ? orderDetails.CouponCode : "-"
						,!string.IsNullOrEmpty(orderDetails.Currency) ? orderDetails.Currency +" "+ (orderDetails.CouponDiscount ?? 0) : ""+ (orderDetails.CouponDiscount ?? 0)
						//,!string.IsNullOrEmpty(orderDetails.Currency) ? orderDetails.Currency +" "+ (orderDetails.RedeemAmount ?? 0) : ""+ (orderDetails.RedeemAmount ?? 0)
						,!string.IsNullOrEmpty(orderDetails.Currency) ? orderDetails.Currency +" "+ (orderDetails.Amount ?? 0) : ""+ (orderDetails.Amount ?? 0)
						//,!string.IsNullOrEmpty(orderDetails.Currency) ? orderDetails.Currency +" "+ (orderDetails.TaxAmount ?? 0) : ""+ (orderDetails.TaxAmount ?? 0)
						//,i.TaxPercent
						,!string.IsNullOrEmpty(orderDetails.Currency) ? orderDetails.Currency +" "+ (orderDetails.TotalAmount ?? 0) : ""+ (orderDetails.TotalAmount ?? 0)
						,i.PaymentStatus != false ? "Paid" : "Unpaid"
						,!string.IsNullOrEmpty(DeliveryAddress) ? DeliveryAddress : "-"
						,i.StartDate
						,i.EndDate
						,i.SelfPickUp
						,i.ExtraKilometer
						,i.ExtraKilometerPrice
						});
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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CompletedOrdersReport(DateTime fromDate, DateTime ToDate)
		{
			DateTime EndDate = ToDate.AddMinutes(1439);
			var VendorID = Convert.ToInt64(Session["VendorID"]);
			var getAllOrders = _orderService.GetVendorOrderscompDateWise(VendorID, fromDate, EndDate);
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
						,"Booking Status"
						,"Car Name"
						,"Package Name"
						,"Coupon Code"
						,"Coupon Discount"
						//,"Redeem Amount"
						,"Subtotal"
						//,"Tax"
						,"Grand Total"
						,"Delivery Address"
						,"Start Date"
						,"End Date"
						,"SelfPickUp"
						,"ExtraKilometer"
						,"ExtraKilometerPrice"
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
						//var deliveryAddress = i.OrderDeliveryAddress.FirstOrDefault();
						if (!string.IsNullOrEmpty(i.OrderDeliveryAddress))
						{
							DeliveryAddress = i.OrderDeliveryAddress;
							DeliveryAddress += i.AreaName != null ? "," + i.AreaName : "";
							DeliveryAddress += i.CityName != null ? "," + i.CityName : "";
							DeliveryAddress += i.CountryName != null ? "," + i.CountryName : "";

						}

						var orderDetails = new Order();

						orderDetails = _orderService.GetOrder(i.ID);
						if (orderDetails == null)
							orderDetails = new Order();

						cellData.Add(new object[] {
						i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
						,!string.IsNullOrEmpty(i.OrderNo) ? i.OrderNo : "-"

						,!string.IsNullOrEmpty(i.Status) ? i.Status : "-"
						,!string.IsNullOrEmpty(i.CarName) ? i.CarName : "-"
						,!string.IsNullOrEmpty(i.PackageName) ? i.PackageName : "-"
						,!string.IsNullOrEmpty(orderDetails.CouponCode) ? orderDetails.CouponCode : "-"
						,!string.IsNullOrEmpty(orderDetails.Currency) ? orderDetails.Currency +" "+ (orderDetails.CouponDiscount ?? 0) : ""+ (orderDetails.CouponDiscount ?? 0)
						//,!string.IsNullOrEmpty(orderDetails.Currency) ? orderDetails.Currency +" "+ (orderDetails.RedeemAmount ?? 0) : ""+ (orderDetails.RedeemAmount ?? 0)
						,!string.IsNullOrEmpty(orderDetails.Currency) ? orderDetails.Currency +" "+ (orderDetails.Amount ?? 0) : ""+ (orderDetails.Amount ?? 0)
						//,!string.IsNullOrEmpty(orderDetails.Currency) ? orderDetails.Currency +" "+ (orderDetails.TaxAmount ?? 0) : ""+ (orderDetails.TaxAmount ?? 0)

						,!string.IsNullOrEmpty(orderDetails.Currency) ? orderDetails.Currency +" "+ (orderDetails.TotalAmount ?? 0) : ""+ (orderDetails.TotalAmount ?? 0)
						,!string.IsNullOrEmpty(DeliveryAddress) ? DeliveryAddress : "-"
						,i.StartDate
						,i.EndDate
						,i.SelfPickUp
						,i.ExtraKilometer
						,i.ExtraKilometerPrice
						});
					}

					worksheet.Cells[2, 1].LoadFromArrays(cellData);

					return File(excel.GetAsByteArray(), "application/msexcel", "Completed Bookings Report.xlsx");
				}
			}
			return RedirectToAction("Index");
		}

		#endregion
	}
}