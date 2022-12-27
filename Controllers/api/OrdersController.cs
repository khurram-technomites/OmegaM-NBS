using Newtonsoft.Json;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Payments.MyFatoorah.Capture;
using NowBuySell.Web.Helpers.PushNotification;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.Order;
using NowBuySell.Web.ViewModels.Api.OrderRepeat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Routing;

namespace NowBuySell.Web.Controllers.api
{
	//[Authorize]
	[RoutePrefix("api/v1")]
	public class OrdersController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly INumberRangeService _numberRangeService;
		private readonly ICustomerService _customerService;
		private readonly IOrderService _orderService;
		private readonly IOrderDetailsService _orderDetailsService;
		private readonly IOrderDeliveryAddressService _orderDeliveryAddressService;
		private readonly IOrderDetailAttributeService _orderDetailAttributeService;
		private readonly ICarService _carService;
		private readonly ICarPackageService _carPackageService;
		private readonly ICarAttributeService _carAttributeService;
		private readonly ICarVariationService _carVariationService;
		private readonly ITaxSettingService _taxSettingService;
		private readonly ICouponService _couponService;
		private readonly ICouponRedemptionService _couponRedemptionService;
		private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;
		private readonly IInvoiceService _invoiceService;
		private readonly IMyFatoorahPaymentGatewaySettingsService _myFatoorahPaymentGatewaySettingsService;
		private readonly ITransactionService _transactionService;
		private readonly IVendorService _vendorService;
		private readonly IVendorSessionService _vendorSessionService;
		private readonly ICustomerSessionService _customerSessionService;

		public OrdersController(INumberRangeService numberRangeService, ICustomerService customerService, IOrderService orderService, IOrderDetailsService orderDetailsService, IOrderDeliveryAddressService orderDeliveryAddressService, IOrderDetailAttributeService orderDetailAttributeService, ICarService carService, ICarAttributeService carAttributeService, ICarVariationService carVariationService, ITaxSettingService taxSettingService, ICouponService couponService, ICouponRedemptionService couponRedemptionService, INotificationService notificationService, INotificationReceiverService notificationReceiverService, IInvoiceService invoiceService, ICarPackageService carPackageService, IMyFatoorahPaymentGatewaySettingsService myFatoorahPaymentGatewaySettingsService, ITransactionService transactionService, IVendorService vendorService, IVendorSessionService vendorSessionService, ICustomerSessionService customerSessionService)
		{
			this._numberRangeService = numberRangeService;
			this._customerService = customerService;
			this._orderService = orderService;
			this._orderDetailsService = orderDetailsService;
			this._orderDeliveryAddressService = orderDeliveryAddressService;
			this._orderDetailAttributeService = orderDetailAttributeService;
			this._carService = carService;
			this._carAttributeService = carAttributeService;
			this._carVariationService = carVariationService;
			this._taxSettingService = taxSettingService;
			this._couponService = couponService;
			this._couponRedemptionService = couponRedemptionService;
			this._notificationService = notificationService;
			this._notificationReceiverService = notificationReceiverService;
			this._invoiceService = invoiceService;
			this._carPackageService = carPackageService;
			this._myFatoorahPaymentGatewaySettingsService = myFatoorahPaymentGatewaySettingsService;
			this._transactionService = transactionService;
			this._vendorService = vendorService;
			this._vendorSessionService = vendorSessionService;
			this._customerSessionService = customerSessionService;
		}

		[Authorize]
		[HttpPost]
		[Route("booking")]
		public HttpResponseMessage Create(OrderViewModel orderViewModel)
		{
			try
			{
				string packageName = string.Empty;
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{


					string message = string.Empty;
					//if (ModelState.IsValid)
					//{

					if (customerId > 0)
					{
						var detail = orderViewModel.OrderDetails.FirstOrDefault();
						if (!_orderService.CheckScheduleAvailibity(detail.CarID, detail.StartDateTime.Value, detail.EndDateTime.Value))
						{
							return Request.CreateResponse(HttpStatusCode.BadRequest, new
							{
								status = "error",
								message = "Ooops, the car is not available for requested dates.",
								isOutOfStock = true
							});
						}


						Order OrderModel = new Order();

						OrderModel.OrderNo = _numberRangeService.GetNextValueFromNumberRangeByName("BOOKING");
						OrderModel.TaxPercent = _taxSettingService.GetTotalTax();
						OrderModel.DiscountPercent = orderViewModel.DiscountPercent;
						OrderModel.DiscountAmount = orderViewModel.DiscountAmount;
						OrderModel.DeliveryCharges = orderViewModel.DeliveryCharges;
						OrderModel.RedeemAmount = orderViewModel.RedeemAmount;
						OrderModel.DeliveryDate = orderViewModel.DeliveryDate;
						OrderModel.SelfPickUp = orderViewModel.SelfPickUp;
						OrderModel.DocumentAtPickUp = orderViewModel.DocumentAtPickUp;

						OrderModel.PaymentMethod = orderViewModel.PaymentMethod;
						if (OrderModel.PaymentMethod.Equals("Cash"))
						{
							OrderModel.IsPaid = false;
							OrderModel.Status = "Pending";
							OrderModel.ShipmentStatus = "Pending";
							OrderModel.Tracking = "Pending";
						}
						else
						{
							OrderModel.Status = "Pending";
							OrderModel.ShipmentStatus = "Pending";
							OrderModel.Tracking = "Pending";
						}
						OrderModel.CustomerID = customerId;
						OrderModel.CouponDiscount = 0m;
						OrderModel.Currency = "AED";
						decimal Amount = 0m;
						if (_orderService.CreateOrder(OrderModel, ref message))
						{
							Invoice invoice = new Invoice()
							{
								Description = "Invoice for Order No. " + OrderModel.OrderNo,
								InvoiceNo = OrderModel.OrderNo.Replace("BKN", "INV"),
								OrderID = OrderModel.ID,
								PaymentMethod = OrderModel.PaymentMethod,
								Status = "Unpaid"
							};

							_invoiceService.CreateInvoice(invoice, ref message);

							if (orderViewModel.DeliveryAddress != null)
							{
								var orderDeliveryAddress = new OrderDeliveryAddress();
								orderDeliveryAddress.OrderID = OrderModel.ID;
								orderDeliveryAddress.Address = orderViewModel.DeliveryAddress.Address;
								orderDeliveryAddress.Latitude = orderViewModel.DeliveryAddress.Latitude;
								orderDeliveryAddress.Longitude = orderViewModel.DeliveryAddress.Longitude;
								_orderDeliveryAddressService.CreateOrderDeliveryAddress(orderDeliveryAddress, ref message);
							}



							foreach (var orderDetail in orderViewModel.OrderDetails)
							{
								OrderDetail OrderDetailModel = new OrderDetail();


								OrderDetailModel.OrderID = OrderModel.ID;
								OrderDetailModel.VendorID = orderDetail.VendorID;
								OrderDetailModel.CarID = orderDetail.CarID;
								OrderDetailModel.StartDateTime = orderDetail.StartDateTime;
								OrderDetailModel.EndDateTime = orderDetail.EndDateTime;

								var date = Helpers.TimeZone.GetLocalDateTime();
								var packageDetails = _carPackageService.GetPackageByCarIDandPackageID(orderDetail.CarID, orderDetail.PackageID);

								OrderDetailModel.PackageName = _carPackageService.GetDetailsByCarId((long)packageDetails.PackageID);
								OrderDetailModel.CarPackageID = packageDetails.ID;

								TimeSpan diff = (OrderDetailModel.EndDateTime - OrderDetailModel.StartDateTime).Value;
								switch (OrderDetailModel.PackageName)
								{
									case "Hourly":
										OrderDetailModel.Price = packageDetails.Price * (diff.Hours);
										break;
									case "Daily":
										OrderDetailModel.Price = packageDetails.Price * (diff.Days);
										break;
									case "Weekly":
										OrderDetailModel.Price = packageDetails.Price * (diff.Days / 7);
										break;
									case "Monthly":
										OrderDetailModel.Price = packageDetails.Price * (diff.Days / 30);
										break;
									default:
										break;
								}

								OrderDetailModel.ExtraKilometer = orderDetail.ExtraKilometer;
								OrderDetailModel.ExtraKilometerPrice = orderDetail.ExtraKilometerPrice;
								OrderDetailModel.TotalPrice = (decimal)OrderDetailModel.Price + (decimal)orderDetail.ExtraKilometerPrice + (decimal)orderViewModel.DeliveryCharges;

								if (_orderDetailsService.CreateOrderDetail(OrderDetailModel, ref message))
								{
									Amount += (decimal)OrderDetailModel.TotalPrice;
								}
							}

							OrderModel.Amount = Amount;
							if (!string.IsNullOrEmpty(orderViewModel.CouponCode))
							{
								var coupon = _couponService.GetCoupon(orderViewModel.CouponCode);
								if (coupon != null)
								{
									var couponRedemption = new CouponRedemption()
									{
										CouponID = coupon.ID,
										CustomerID = customerId,
										OrderID = OrderModel.ID
									};
									_couponRedemptionService.CreateCouponRedemption(couponRedemption, ref message);
									OrderModel.CouponCode = orderViewModel.CouponCode;
									OrderModel.CouponDiscount = orderViewModel.CouponDiscount;
								}
							}
							var totalAmount = Amount;
							//Apply Discount
							totalAmount -= orderViewModel.DiscountAmount;
							totalAmount -= orderViewModel.CouponDiscount;
							OrderModel.TotalAmount = totalAmount;
							//totalAmount -= orderViewModel.RedeemAmount;

							//Apply tax
							//OrderModel.TaxAmount = (totalAmount * OrderModel.TaxPercent) / 100;
							//OrderModel.TotalAmount = totalAmount + OrderModel.TaxAmount;

							if (_orderService.UpdateOrder(ref OrderModel, ref message))
							{
								///*Cars Stock Update*/
								//_orderService.UpdateCarStock(OrderModel.ID);
								var customer = _customerService.GetCustomer(customerId);
								/*Loyalty Points Redemption*/
								_customerService.UpdateLoyaltyPoints(customerId, OrderModel.ID, OrderModel.RedeemAmount.HasValue ? (long)OrderModel.RedeemAmount.Value : 0);

								/*Order Notification For Customer*/
								Notification not = new Notification();
								not.Title = "Booking Placed";
								not.TitleAr = "Booking Placed";
								not.Description = string.Format("Your booking # {0} has been placed. You can check the booking status via booking details", OrderModel.OrderNo);
								not.DescriptionAr = string.Format("Your booking # {0} has been placed. You can check the booking status via booking details", OrderModel.OrderNo);

								not.Module = "Booking";
								not.OriginatorType = "System";
								not.RecordID = OrderModel.ID;
								if (_notificationService.CreateNotification(not, ref message))
								{
									NotificationReceiver notRec = new NotificationReceiver();
									notRec.ReceiverID = OrderModel.CustomerID;
									notRec.ReceiverType = "Customer";
									notRec.NotificationID = not.ID;
									if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
									{
										var tokens = _customerSessionService.GetCustomerSessionFirebaseTokens(customerId, true, null);
										var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
										{
											Module = "Booking",
											RecordID = OrderModel.ID,
											OrderNo = OrderModel.OrderNo,
											NotificationID = not.ID
										});
									}
								}

								not = new Notification();

								/*Order Notification For Admin*/
								not.Title = "Booking Placed";
								not.TitleAr = "Booking Placed";
								not.Description = string.Format("New booking # {0} has been placed ", OrderModel.OrderNo);
								not.DescriptionAr = string.Format("New booking # {0} has been placed ", OrderModel.OrderNo);
								not.Url = "/Admin/Order/Index";
								not.Module = "Booking";
								not.OriginatorType = "System";
								not.RecordID = OrderModel.ID;
								if (_notificationService.CreateNotification(not, ref message))
								{
									if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", null))
									{

									}


								}

								/*Order Notification For Vendor*/
								not.Title = "Booking Placed";
								not.TitleAr = "Booking Placed";
								not.Description = string.Format("New booking # {0} has been placed ", OrderModel.OrderNo);
								not.DescriptionAr = string.Format("New booking # {0} has been placed ", OrderModel.OrderNo);
								not.Url = "/Vendor/Order/Index";
								not.Module = "Booking";
								not.OriginatorType = "System";
								not.RecordID = OrderModel.ID;
								if (_notificationService.CreateNotification(not, ref message))
								{


									if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Vendor", orderViewModel.OrderDetails.FirstOrDefault().VendorID))
									{
										var tokens = _vendorSessionService.GetVendorSessionFirebaseTokens(orderViewModel.OrderDetails.FirstOrDefault().VendorID);
										var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
										{
											Module = "Booking",
											RecordID = OrderModel.ID,
											NotificationID = not.ID
										}, false);
									}
								}

								///*Order Email*/


								OrderModel = _orderService.GetOrder((long)OrderModel.ID);
								IEnumerable<SP_GetOrderDetails_Result> OrderDetails = _orderService.GetOrderByOrderID((Int16)OrderModel.ID);
								NowBuySell.Web.ViewModels.Order.OrderDetailViewModel Details = new NowBuySell.Web.ViewModels.Order.OrderDetailViewModel();

								Details.CreatedOn = (DateTime)OrderModel.CreatedOn;
								Details.OrderNo = OrderModel.OrderNo;
								Details.DeliveryAddress = OrderModel.DeliveryAddress;
								Details.Status = OrderModel.Status;
								Details.ShipmentStatus = OrderModel.ShipmentStatus;
								Details.CustomerName = customer.Name;
								Details.Currency = OrderModel.Currency;

								Details.Amount = (decimal)OrderModel.Amount;
								//Details.OrderTaxAmount = (decimal)OrderModel.TaxAmount;
								//Details.OrderTaxPercent = (decimal)OrderModel.TaxPercent;

								Details.Shipping = (decimal)OrderModel.DeliveryCharges;
								Details.CouponDiscount = OrderModel.CouponDiscount.HasValue ? OrderModel.CouponDiscount.Value : 0m;
								Details.CouponCode = OrderModel.CouponCode;
								Details.RedeemAmount = OrderModel.RedeemAmount.HasValue ? OrderModel.RedeemAmount.Value : 0m;

								Details.TotalAmount = (decimal)OrderModel.TotalAmount;

								var deliveryAddress = OrderModel.OrderDeliveryAddresses.FirstOrDefault();
								if (deliveryAddress != null)
								{
									Details.Country = deliveryAddress.Country != null ? deliveryAddress.Country.Name : "-";
									Details.City = deliveryAddress.City != null ? deliveryAddress.City.Name : "-";
									Details.Area = deliveryAddress.Area != null ? deliveryAddress.Area.Name : "-";
									Details.Address = deliveryAddress.Address;
								}

								Details.orderdetails = OrderDetails;


								//var body = ViewToStringRenderer.RenderViewToString(context, "~/Views/Orders/Details.cshtml", Details);
								var body = ViewToStringRenderer.RenderViewToString("Orders", "Details", Details);
								_orderService.SendOrderEmail(customer.Email, "On My Way | Order Placed", body);


								return Request.CreateResponse(HttpStatusCode.OK, new
								{
									status = "success",
									message = "Booking punched!",
									orderId = OrderModel.ID,
									orderNo = OrderModel.OrderNo
								});
							}
						}
						return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.BadRequest, new
						{
							status = "error",
							message = "Bad request !",
							description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
						});
					}
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				//Logs.Write(ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}

		[HttpGet]
		[Route("bookings")]
		public HttpResponseMessage GetAll(int pg = 1, string status = "")
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{

					var orders = _orderService.GetCustomerOrders(customerId, status, string.Empty, pg, 1);

					return Request.CreateResponse(HttpStatusCode.OK, new
					{
						status = "success",
						orders = orders
					});

				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				//Logs.Write(ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}

		[HttpGet]
		[Route("{lang}/bookings/{orderId}")]
		public HttpResponseMessage GetOrderDetails(long orderId, string lang = "en")
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long staffId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out staffId))
				{

					Order order = _orderService.GetOrder(orderId);

					var orderDetails = _orderService.GetOrderByOrderID(orderId, lang);

					var orderDetail = _orderService.GetOrderByOrderID(orderId).FirstOrDefault();
					var vendor = _vendorService.GetVendor((long)orderDetail.VendorID);

					if (order != null)
					{
						string ImageServer = CustomURL.GetImageServer();

						var orderModel = new
						{
							order.ID,
							order.OrderNo,
							order.Status,
							car = new
							{
								id = orderDetail.CarID,
								thumbnail = !string.IsNullOrEmpty(orderDetail.Thumbnail) ? ImageServer + orderDetail.Thumbnail : null,
								name = orderDetail.CarName,
								licensePlate = orderDetail.LicensePlate,
								sku = orderDetail.SKU,
							},
							vendor = new
							{
								id = vendor.ID,
								name = lang == "en" ? vendor.Name : vendor.NameAr,
								image = !string.IsNullOrEmpty(vendor.Logo) ? ImageServer + vendor.Logo : null,
								latitude = vendor.Latitude,
								longitude = vendor.Longitude,
								address = vendor.Address
							},
							deliveryAddress = order.OrderDeliveryAddresses.Select(j => new
							{
								latitude = j.Latitude,
								longitude = j.Longitude,
								address = j.Address
							}).FirstOrDefault(),
							bookingInfo = new
							{
								fromDate = orderDetail.StartDateTime,
								toDate = orderDetail.EndDateTime,
								DeliveryMethod = order.SelfPickUp.HasValue && order.SelfPickUp.Value ? "Self Pickup" : "Delivery",
								ExtraKilometers = orderDetail.ExtraKilometer,
								Package = new
								{
									name = orderDetail.PackageName,
								},
							},
							paymentInfo = new
							{
								Method = order.PaymentMethod,
								Status = order.IsPaid.HasValue && order.IsPaid.Value ? "Paid" : "Unpaid",
								PaymentCaptured = order.PaymentCaptured.HasValue ? order.PaymentCaptured.Value : false,
								charges = new
								{
									RentalFee = orderDetail.Price,
									ExtraKilometerPrice = orderDetail.ExtraKilometerPrice,
									DeliveryCharges = order.DeliveryCharges,
									SubTotal = orderDetail.TotalPrice,
									CouponCode = order.CouponCode,
									CouponDiscount = order.CouponDiscount,
									Total = order.TotalAmount
								}
							},
							orderDetail.Rating,
							orderDetail.Remarks,
						};

						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "success",
							order = orderModel
						});
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.BadRequest, new { status = "error", message = "Invalid order id !" });
					}
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				//Logs.Write(ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}

		//[HttpGet]
		//[Route("orders/{orderId}/Repeat")]
		//public HttpResponseMessage GetRepeatOrderDetails(long orderId)
		//{
		//	try
		//	{
		//		var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
		//		var claims = identity.Claims;
		//		long staffId;
		//		if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out staffId))
		//		{
		//			Order order = _orderService.GetOrder(orderId);

		//			var orderDetails = _orderService.GetOrderByOrderID(orderId);

		//			if (order != null)
		//			{
		//				string ImageServer = CustomURL.GetImageServer();

		//				List<OrderRepeatViewModel> objSP_GetCarDetails_Result = new List<OrderRepeatViewModel>();
		//				var CurrentDateTime = Helpers.TimeZone.GetLocalDateTime();
		//				foreach (var orderDetail in orderDetails.ToList())
		//				{
		//					var car = _carService.GetCarDetails((long)orderDetail.CarID, "en", ImageServer);
		//					var carAttributes = _carAttributeService.GetCarAttributes((long)orderDetail.CarID, "en");
		//					var carvariations = _carVariationService.GetCarVaraitions((long)orderDetail.CarID, "en", ImageServer);

		//					objSP_GetCarDetails_Result.Add(new OrderRepeatViewModel()
		//					{
		//						ID = car.ID,
		//						SKU = car.SKU,
		//						Title = car.Title,
		//						Type = car.Type,
		//						RegularPrice = car.RegularPrice,
		//						SalePrice = car.SalePrice,
		//						SalePriceFrom = car.SalePriceFrom,
		//						SalePriceTo = car.SalePriceTo,
		//						IsSaleAvailable = (car.SalePrice.HasValue
		//									&& (!car.SalePriceFrom.HasValue || car.SalePriceFrom.Value <= CurrentDateTime)
		//									&& (!car.SalePriceTo.HasValue || car.SalePriceTo >= CurrentDateTime)) ? true : false,
		//						IsManageStock = car.IsManageStock,
		//						Stock = car.Stock,
		//						Thumbnail = ImageServer + car.Thumbnail,
		//						StockStatus = car.StockStatus,
		//						IsSoldIndividually = car.IsSoldIndividually,
		//						attributes = carAttributes.Select(i => new OrderRepeatAttributesViewModel()
		//						{
		//							ID = i.AttributeID,
		//							Name = i.AttributeName,
		//							Options = i.AttributeValues.Split('|')
		//						}).ToList(),
		//						variations = carvariations.Select(i => new OrderRepeatVariationsViewModel()
		//						{
		//							ID = i.ID,
		//							SKU = i.SKU,
		//							RegularPrice = i.RegularPrice,
		//							SalePrice = i.SalePrice,
		//							SalePriceFrom = i.SalePriceFrom,
		//							SalePriceTo = i.SalePriceTo,
		//							IsSaleAvailable = (i.SalePrice.HasValue
		//									&& (!i.SalePriceFrom.HasValue || i.SalePriceFrom.Value <= CurrentDateTime)
		//									&& (!i.SalePriceTo.HasValue || i.SalePriceTo >= CurrentDateTime)) ? true : false,
		//							IsManageStock = i.IsManageStock,
		//							Stock = i.Stock,
		//							StockStatus = i.StockStatus,
		//							SoldIndividually = i.SoldIndividually,
		//							attributes = i.Attributes.Split(',')
		//						}).ToList(),
		//						vendor = new OrderRepeatVendorViewModel()
		//						{
		//							ID = car.VendorID,
		//						}
		//					});
		//				}

		//				var orderModel = new
		//				{
		//					order.ID,
		//					order.OrderNo,
		//					order.Amount,
		//					order.DiscountAmount,
		//					order.DiscountPercent,
		//					order.TaxAmount,
		//					order.TaxPercent,
		//					order.DeliveryCharges,
		//					order.TotalAmount,
		//					order.PaymentMethod,
		//					order.Status,
		//					order.ShipmentStatus,
		//					order.CancelationReason,
		//					isReturnable = true,
		//					cars = objSP_GetCarDetails_Result,
		//					details = orderDetails.Select(i => new
		//					{
		//						id = i.DetailID,
		//						vendor = new
		//						{
		//							id = i.VendorID,
		//							code = i.VendorCode,
		//							name = i.VendorName,
		//							logo = ImageServer + i.VendorLogo,
		//						},
		//						car = new
		//						{
		//							id = i.CarID,
		//							name = i.CarName,
		//							thumbnail = ImageServer + i.Thumbnail,
		//							sku = i.SKU,

		//						},
		//						i.CarVariantID,


		//						i.Status,

		//						i.EnableReviews,
		//						i.Remarks,
		//						i.Rating,
		//						returns = i.ReturnID.HasValue ? new
		//						{
		//							id = i.ReturnID,
		//							method = i.ReturnMethod,
		//							status = i.ReturnStatus

		//						} : null
		//					}),
		//					DeliveryAddress = order.OrderDeliveryAddresses.Select(j => new
		//					{
		//						country = j.Area != null ? new
		//						{
		//							j.Country.ID,
		//							j.Country.Name
		//						} : null,
		//						City = j.Area != null ? new
		//						{
		//							j.City.ID,
		//							j.City.Name
		//						} : null,
		//						Area = j.Area != null ? new
		//						{
		//							j.Area.ID,
		//							j.Area.Name
		//						} : null,
		//						j.Contact,
		//						j.Address
		//					}).FirstOrDefault()
		//				};

		//				return Request.CreateResponse(HttpStatusCode.OK, new
		//				{
		//					status = "success",
		//					order = orderModel
		//				});
		//			}
		//			else
		//			{
		//				return Request.CreateResponse(HttpStatusCode.BadRequest, new { status = "error", message = "Invalid order id !" });
		//			}
		//		}
		//		else
		//		{
		//			return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		log.Error("Error", ex);
		//		//Logs.Write(ex);
		//		return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
		//	}
		//}

		[HttpPost]
		[Route("bookings/{orderId}/cancel")]
		public HttpResponseMessage CancelOrder(long orderId, CancellationReasonViewModel cancellationReasonViewModel)
		{
			try
			{
				string message = string.Empty;
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					Order OrderModel = _orderService.GetOrder(orderId);
					if (OrderModel != null)
					{
						if (OrderModel.Status == "Pending")
						{
							OrderModel.Status = "Canceled";
							OrderModel.ShipmentStatus = "NotFulfilled";


							if (_orderService.UpdateOrder(ref OrderModel, ref message))
							{
								Notification not = new Notification();
								not.Title = "Booking Canceled";
								not.TitleAr = "Booking Canceled";
								not.Description = string.Format("Booking # {0} has been canceled by customer", OrderModel.OrderNo);
								not.DescriptionAr = string.Format("Booking # {0} has been canceled by customer", OrderModel.OrderNo);
								not.Module = "Booking";
								not.OriginatorType = "System";
								not.RecordID = OrderModel.ID;
								if (_notificationService.CreateNotification(not, ref message))
								{
									NotificationReceiver notRec = new NotificationReceiver();
									notRec.ReceiverID = OrderModel.CustomerID;
									notRec.ReceiverType = "Customer";
									notRec.NotificationID = not.ID;
									if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
									{
										var tokens = _customerSessionService.GetCustomerSessionFirebaseTokens((long)OrderModel.CustomerID, true, null);
										var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
										{
											Module = "Booking",
											RecordID = OrderModel.ID,
											OrderNo = OrderModel.OrderNo,
											NotificationID = notRec.ID
										});
									}
									_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", null);
								}

								return Request.CreateResponse(HttpStatusCode.OK, new
								{
									status = "success",
									message = "Order canceled!",
									orderId = OrderModel.ID,
									orderNo = OrderModel.OrderNo
								});
							}
							return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
						}
						else
						{
							return Request.CreateResponse(HttpStatusCode.BadRequest, new { status = "error", message = "Only pending order can be canceled!" });
						}
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.BadRequest, new { status = "error", message = "Invalid order number !" });
					}
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				//Logs.Write(ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}


		[HttpPut]
		[Route("bookings/{orderId}/paid/{InvoiceId}")]
		public HttpResponseMessage OrderPaid(long orderId, string InvoiceId)
		{
			try
			{
				string message = string.Empty;
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					Order OrderModel = _orderService.GetOrder(orderId);
					if (OrderModel != null)
					{
						if (OrderModel.IsPaid.HasValue && OrderModel.IsPaid == true)
						{
							return Request.CreateResponse(HttpStatusCode.OK, new
							{
								status = "success",
								message = "Booking payment already processed!",
								orderId = OrderModel.ID,
								orderNo = OrderModel.OrderNo
							});
						}
						try
						{
							System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
							System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

							string TestAPIKey = "rLtt6JWvbUHDDhsZnfpAhpYk4dxYDQkbcPTyGaKp2TYqQgG7FGZ5Th_WD53Oq8Ebz6A53njUoo1w3pjU1D4vs_ZMqFiz_j0urb_BH9Oq9VZoKFoJEDAbRZepGcQanImyYrry7Kt6MnMdgfG5jn4HngWoRdKduNNyP4kzcp3mRv7x00ahkm9LAK7ZRieg7k1PDAnBIOG3EyVSJ5kK4WLMvYr7sCwHbHcu4A5WwelxYK0GMJy37bNAarSJDFQsJ2ZvJjvMDmfWwDVFEVe_5tOomfVNt6bOg9mexbGjMrnHBnKnZR1vQbBtQieDlQepzTZMuQrSuKn-t5XZM7V6fCW7oP-uXGX-sMOajeX65JOf6XVpk29DP6ro8WTAflCDANC193yof8-f5_EYY-3hXhJj7RBXmizDpneEQDSaSz5sFk0sV5qPcARJ9zGG73vuGFyenjPPmtDtXtpx35A-BVcOSBYVIWe9kndG3nclfefjKEuZ3m4jL9Gg1h2JBvmXSMYiZtp9MR5I6pvbvylU_PP5xJFSjVTIz7IQSjcVGO41npnwIxRXNRxFOdIUHn0tjQ-7LwvEcTXyPsHXcMD8WtgBh-wxR8aKX7WPSsT1O8d8reb2aR7K3rkV3K82K_0OgawImEpwSvp9MNKynEAJQS6ZHe_J_l77652xwPNxMRTMASk1ZsJL";

							var myFatoorahSetting = _myFatoorahPaymentGatewaySettingsService.GetDefaultPaymentGatewaySetting();

							string Endpoint = myFatoorahSetting.IsLive.HasValue && myFatoorahSetting.IsLive.Value ? myFatoorahSetting.LiveEndpoint : myFatoorahSetting.TestEndpoint;
							string APIKey = myFatoorahSetting.IsLive.HasValue && myFatoorahSetting.IsLive.Value ? myFatoorahSetting.APIKey : TestAPIKey;

							using (var client = new HttpClient())
							{
								/*Fetch  Access Token From N-Genius*/
								var body = new
								{
									Key = InvoiceId,
									KeyType = "InvoiceId"
								};

								client.BaseAddress = new Uri(Endpoint);
								client.DefaultRequestHeaders.Add("Authorization", "Bearer " + APIKey);

								var json = JsonConvert.SerializeObject(body);

								var content = new StringContent(json, Encoding.UTF8, "application/json");
								content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

								var response = client.PostAsync("/v2/GetPaymentStatus", content).Result;

								if (response.IsSuccessStatusCode)
								{

									var paymentResponse = JsonConvert.DeserializeObject<PaymentInquiryResponse>(response.Content.ReadAsStringAsync().Result);

									if (paymentResponse.IsSuccess)
									{
										message = string.Empty;

										var Payment = paymentResponse.Data;

										if ((Payment != null && Payment.InvoiceStatus == "Paid"))
										{
											OrderModel.IsPaid = true;
											OrderModel.PaymentRef = null;
											OrderModel.PaymentCaptured = null;

											if (_orderService.UpdateOrder(ref OrderModel, ref message))
											{
												var InvoiceTransaction = Payment.InvoiceTransactions.Where(i => i.TransactionStatus == "Succss").OrderByDescending(i => i.TransactionDate).FirstOrDefault();

												if (InvoiceTransaction != null)
												{
													Transaction transaction = new Transaction()
													{
														OrderID = OrderModel.ID,
														NameOnCard = Payment.CustomerName,
														MaskCardNo = InvoiceTransaction.CardNumber,
														TransactionStatus = InvoiceTransaction.TransactionStatus,
														Amount = Convert.ToDecimal(InvoiceTransaction.TransationValue)
													};

													_transactionService.CreateTransaction(transaction, ref message);
												}

												Notification not = new Notification();
												not.Title = "Booking Invoiced";
												not.Description = string.Format("Booking {0}  payment successfully processed", OrderModel.OrderNo);
												not.Url = "/Admin/Order/Index";
												not.Module = "Invoiced";
												not.OriginatorType = "Customer";
												not.RecordID = OrderModel.ID;

												if (_notificationService.CreateNotification(not, ref message))
												{
													if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", null))
													{
													}
												}

												return Request.CreateResponse(HttpStatusCode.OK, new
												{
													status = "success",
													message = "Booking payment processed!",
													orderId = OrderModel.ID,
													orderNo = OrderModel.OrderNo
												});
											}

										}
										else
										{
											var InvoiceTransaction = Payment.InvoiceTransactions.Where(i => i.TransactionStatus == "Failed").OrderByDescending(i => i.TransactionDate).FirstOrDefault();

											if (InvoiceTransaction != null)
											{
												OrderModel.IsPaid = false;
												OrderModel.PaymentRef = null;
												OrderModel.PaymentCaptured = null;

												if (_orderService.UpdateOrder(ref OrderModel, ref message))
												{
													return Request.CreateResponse(HttpStatusCode.OK, new
													{
														status = "success",
														message = "Oops! Transaction Failed. Please try again.",
														orderId = OrderModel.ID,
														orderNo = OrderModel.OrderNo
													});
												}
												else
												{
													return Request.CreateResponse(HttpStatusCode.InternalServerError, new
													{
														status = "failure",
														message = "Oops! Something went wrong while processing for payment. Please try later."
													});
												}
											}
											else
											{
												OrderModel.PaymentCaptured = true;

												if (_orderService.UpdateOrder(ref OrderModel, ref message))
												{
													return Request.CreateResponse(HttpStatusCode.OK, new
													{
														status = "success",
														message = "Your payment is being processed.",
														orderId = OrderModel.ID,
														orderNo = OrderModel.OrderNo
													});
												}
												else
												{
													return Request.CreateResponse(HttpStatusCode.InternalServerError, new
													{
														status = "failure",
														message = "Oops! Something went wrong while processing for payment. Please try later."
													});
												}
											}
										}
									}
									else
									{
										return Request.CreateResponse(HttpStatusCode.InternalServerError, new
										{
											status = "failure",
											message = "Oops! Something went wrong while processing for payment. Please try later."
										});
									}
								}
								else
								{
									return Request.CreateResponse(HttpStatusCode.InternalServerError, new
									{
										status = "failure",
										message = "Oops! Something went wrong while processing for payment. Please try later."
									});
								}
							}
						}
						catch (Exception ex)
						{
							log.Error("Error", ex);
							return Request.CreateResponse(HttpStatusCode.InternalServerError, new
							{
								status = "failure",
								message = "Oops! Something went wrong while processing for payment. Please try later."
							});
						}

						return Request.CreateResponse(HttpStatusCode.InternalServerError, new
						{
							status = "failure",
							message = "Oops! Something went wrong. Please try later."
						});
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.BadRequest, new { status = "error", message = "Invalid order number !" });
					}
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				//Logs.Write(ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}

	}
}
