var Order = {
	DeliveryCharges: 0,
	TaxAmount: 0,
	TaxPercent: 0,
	DiscountAmount: 0,
	DiscountPercent: 0,
	CouponCode: null,
	CouponDiscount: 0,
	RedeemAmount: 0,
	PaymentMethod: 'Cash',
	Note: null,
	OrderRef: null,
	DeliveryAddress: {
		CountryID: null,
		CityID: null,
		AreaID: null,
		Address: null
	},
	GuestDetails: {
		FirstName: null,
		LastName: null,
		Email: null,
		Telephone: null,
		CreateAccount: null
	},
	OrderDetails: []
};

var IsShippingAvailable = false;
var IsMinimumOrderExtended = false;
var IsCouponApplicable = true;
var IsTermAndConditionsAccepted = false;
var IsOutOfStock = true;
var grandFlag = true;
var grandTotal = +0;

var CartFinalAmount = 0;

$(document).ready(function () {
	$('body').removeClass('template-index').removeClass('home2-default').addClass('page-template').addClass('belle');

	if (ShoppingCart.length <= 0) {

		BindEmptyCart();
	} else {
		if (getParameterByName("orderRef")) {
			RepeatOrder(getParameterByName("orderRef"))
		} else {

			GetDeliveryAddresses();

			$('#Subtotal').html('AED ' + parseFloat(ShoppingCart.sum("Price")));
			if ($("#TaxPercentage").text().trim()) {
				Order.TaxPercent = parseFloat($("#TaxPercentage").text());
			} else {
				Order.TaxPercent = 0;

				$("#TaxPercentage").text('0');
			}

			$('input[name=RedeemAmount]').change(function () {
				let redeemAmount = parseFloat($(this).val());
				if (redeemAmount) {
					if (redeemAmount > parseFloat($(this).attr('max'))) {
						$(this).val($(this).attr('max'));
						redeemAmount = parseFloat($(this).attr('max'));
					} else if (redeemAmount < parseFloat($(this).attr('min'))) {
						$(this).val($(this).attr('min'));
						redeemAmount = parseFloat($(this).attr('min'));
					}

					if (redeemAmount > CartFinalAmount) {
						redeemAmount = CartFinalAmount;

						$(this).val(redeemAmount);
					}

					Order.RedeemAmount = parseFloat($(this).val());
					$('#RedeemAmount').html(`${currency} ${Order.RedeemAmount}`);
					FormatAmount($('#RedeemAmount'), false);

					BindCartTotal();

				} else {
					$(this).val('');

					$('#RedeemAmount').html('-');
					Order.RedeemAmount = 0;

					BindCartTotal();
				}
			});

			$('#ChkTerms').change(function () {
				if (this.checked) {
					IsTermAndConditionsAccepted = true;
				} else {
					IsTermAndConditionsAccepted = false;
				}
				CheckCheckoutValidity();
			});

			$('#btnApplyCoupon').click(function () {
				if ($('#coupon-code').val()) {
					$.ajax({
						url: '/coupons/redeem',
						type: 'POST',
						data: {
							__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
							coupon: {
								CouponCode: $('#coupon-code').val()
							}
						},
						success: function (response) {
							if (response.success) {
								if (response.categories.length > 0) {
									CheckCouponCategories(response);
								} else {
									IsCouponApplicable = true;

									$(this).val('');

									$('#RedeemAmount').html('-');
									Order.RedeemAmount = 0;

									var Discount;
									if (response.Type && response.Type == "Percentage") {
										Discount = (parseFloat(ShoppingCart.sum("Price")) * response.Value) / 100;
										Discount = Discount > response.MaxAmount ? response.MaxAmount : Discount;
									} else {
										Discount = response.Value;
									}

									var Subtotal = parseFloat(ShoppingCart.sum("Price"));
									if (Discount > Subtotal) {
										Discount = Subtotal;
									}

									$('#Discount').html(`${currency} ${Discount}`);

									$('#coupon-code').removeClass('border-danger').addClass('border-success');
									$('#coupon-message').html(`woohoo you saved ${currency} ${Discount}`).removeClass('text-danger').addClass('text-success');

									Order.CouponCode = $('#coupon-code').val();
									Order.CouponDiscount = Discount;
									BindCartTotal();
								}
							}
							else {
								IsCouponApplicable = false;
								$('#coupon-code').removeClass('border-success').addClass('border-danger');
								$('#coupon-message').html(response.message).removeClass('text-success').addClass('text-danger');
							}
							$('#btnCancelCoupon').slideDown();
							CheckCheckoutValidity();
						}
					})
				}
			});

			$('#btnCancelCoupon').click(function () {

				$('#btnCancelCoupon').slideUp();

				$('#Discount').html('-');
				Order.CouponCode = null;
				Order.CouponDiscount = 0;

				$('#coupon-code').removeClass('border-danger').removeClass('border-success');
				$('#coupon-code').val('');
				$('#coupon-message').html('');

				$('.coupon-badge').html('').hide();


				IsCouponApplicable = true;
				BindCartTotal();
				CheckCheckoutValidity();

			});

			$('#CountryID').change(function () {
				var count = 0;
				var $dropdown = $("#CityID");
				if ($(this).val() == 0) {
					$dropdown.empty();
					$dropdown.append($("<option />").val('').text("Select Country First!"));

					$('#CityID').trigger('change');

				}
				else {
					$.ajax({
						type: 'Get',
						url: '/Cities/GetCitiesByCountry/' + $(this).val(),
						success: function (response) {
							$dropdown.empty();
							$dropdown.append($("<option />").val('').text("Select City"));

							$.each(response.data, function (k, v) {
								$dropdown.append($("<option />").val(v.value).text(v.text));
								count++;
							});
						}
					});
				}
			});

			$('#CityID').change(function () {
				var count = 0;
				var $dropdown = $("#AreaID");
				if ($(this).val() == 0) {
					$dropdown.empty();
					$dropdown.append($("<option />").val('').text("Select City First!"));
					$('#AreaID').trigger('change');
				}
				else {
					$.ajax({
						type: 'Get',
						url: '/Areas/GetAreasByCity/' + $(this).val(),
						success: function (response) {
							$dropdown.empty();
							$dropdown.append($("<option />").val('').text("Select Area"));

							$.each(response.data, function (k, v) {
								$dropdown.append($("<option />").val(v.value).text(v.text));
								count++;
							});
						}
					});
				}
			});

			$('#AreaID').change(function () {

				IsShippingAvailable = false;


				Order.DeliveryCharges = null;
				$('#Shipping').html('-');
				$('#subtotal-message').html('');
				$('#btnCalculateShippingCost').html('Calculate Shipping Cost');
				BindCartTotal();
				if ($(this).val()) {
					$('#btnCalculateShippingCost').prop('disabled', false);
					$('#btnCalculateShippingCost').trigger('click');
				}
				else {
					$('#btnCalculateShippingCost').prop('disabled', true);
				}
				CheckCheckoutValidity();
			});

			$('#btnCalculateShippingCost').click(function () {

				let AreaId;
				if ($('input[name=rdoShippingAddress]:checked').attr("id") == "rdoNewAddress") {
					AreaId = $('#AreaID').val();
				} else {
					var address = $('input[name=rdoShippingAddress]:checked').closest('.basicsAccordion').find("p");
					AreaId = address.attr("area-id");
				}

				if (AreaId) {
					$.ajax({
						type: 'Get',
						url: '/DeliveryCharges/GetDeliveryChargesByArea/' + AreaId,
						success: function (response) {
							if (response.success) {
								IsShippingAvailable = true;
								$('#Shipping').html(`${currency} ${parseFloat(response.data.charges)}`);

								FormatAmount($('#Shipping'), false);
								Order.DeliveryCharges = response.data.charges;
								$('#btnCalculateShippingCost').html(`Shipping : ${currency} ${parseFloat(response.data.charges)}`);
								$('#btnCalculateShippingCost').prop('disabled', true);

								if (parseFloat(ShoppingCart.sum("Price")) < response.data.minOrder) {
									$('#subtotal-message').html(`( Minimum order price is ${response.data.minOrder} )`).addClass('text-danger');
									IsMinimumOrderExtended = false;
								} else {
									$('#subtotal-message').html('');
									IsMinimumOrderExtended = true;
								}
								BindCartTotal();
							} else {
								IsShippingAvailable = false;
								//$('#btnOrder').prop('disabled', true);
								$('#message').html('<span class="fa fa-warning fa-2x"></span> ' + response.message);
								$('#message').fadeIn();
								MessageSlideUP();
							}
							CheckCheckoutValidity();
						}
					});
				}
				else {
					IsShippingAvailable = false;
					CheckCheckoutValidity();
				}
			});

			if (!$('#AreaID').val()) {
				$('#CityID').trigger('change');
			} else {
				$('#btnCalculateShippingCost').trigger('click');
			}

			$("#OrderForm").on("submit", function (event) {
				$('#message').html('<span class="fa fa-hour-watch"></span> Placing Order ...')
				$('#message').slideDown();
				MessageSlideUP();
				try {
					if (ShoppingCart.length > 0) {
						$("#btnOrder").html('<i class="fa fa-spinner fa-spin"></i> Place order').prop('disabled', true);
						FillOrder();
						var $this = $(this);
						$.ajax({
							url: '/Orders/Create',
							type: 'post',
							data: { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(), orderViewModel: Order },
							success: function (response) {

								if (response.success) {
									//$('#message').html('<span class="fa fa-check"></span> ' + response.message);

									$("html, body").animate({ scrollTop: 0 }, 1000);
									//setTimeout(function () {
									//	$('#message').slideUp();
									//}, 3000);
									ShoppingCart = [];
									localStorage.setItem("cart", []);
									UpdateCartCounter();
									FillCart();
									UpdateTotal();

									//window.location = '/Customer/Order/Details/' + response.order.id;
									$('#message').slideUp();

									var EmptyCartTemplate = `<div class="row">
															<div class="col-12 col-sm-12 col-md-12 col-lg-12">
        														<div class="empty-page-content text-center">
																	<img src="/Assets/images/order/success.png" />
																	<h5>Order # ${response.order.OrderNo}</h5>
																	<h4>Your Order has been Placed Successfully</h4>
														            <p>
																		<a href="/" class="btn btn--has-icon-after">
																			Continue shopping
																			<i class="fa fa-caret-right" aria-hidden="true"></i>
																		</a>
																	</p>
														          </div>
        													</div>
        												</div>`;

									$("#checkout-container").html(EmptyCartTemplate);

								}
								else {

									$('#message').html('<span class="fa fa-warning fa-2x"></span> ' + response.message);
									$('#message').slideDown();
									MessageSlideUP();
									if (response.isOutOfStock) {
										IsOutOfStock = true;
										$.each(response.data, function (k, v) {
											if (v) {
												$('.checkout_cart_item[car_attribute_id=' + v + ']').addClass('out-of-stock');;

											} else {
												$('.checkout_cart_item[car_id=' + k + ']').addClass('out-of-stock');
											}
										});

										CheckCheckoutValidity();
									}
									$("#btnOrder").html('Place order');
								}
							}
						})
							.done(function (e) {
								console.log(e);
							})
							.fail(function (e) {
								if (e.status == 403) {
									window.location = er.responseJSON.LogOnUrl;
								}
							});
						event.preventDefault();
					}
					else {
						BindEmptyCart();
						return false;
					}
				}
				catch (ex) {
					$("#btnOrder").html('Place order').prop('disabled', false);
					$('#message').html('<span class="fa fa-warning fa-2x"></span> Unable to process your order');
					$('#message').slideDown();
					MessageSlideUP();
					return false;
				}
				return false;
			});

			CheckCartStock();

			BindCartTotal();
		}
	}

	
});

//$(window).on('load', function () {
//	if (!IsLoyaltyEnabled) {
//		$('.redeem-amount-container').remove();
//	}
//});

function BindOrderCart(Car) {

	var tr = '<tr class="checkout_cart_item" car_id="' + Car.CarID + '" car_attribute_id="' + Car.CarVaraiationID + '" id="' + Car.RowId + '">';
	tr += '	<td class="text-left">';
	tr += '		<div class="car-image" style="background-image:url(' + Car.Thumbnail + ')">';
	tr += '		</div>';
	tr += '		<div class="car-details">';
	tr += '			<a class="pName" href="/car/' + Car.Slug + '">' + Car.Title + '</a>';
	if (Car.Attributes.length > 0) {
		tr += '			<span class="variant-cart">' + GetAttributeString(Car.Attributes) + '</span>';
	}
	if (Car.CustomNote) {
		tr += '	<hr class="m-0" />';
		tr += '	<div class="cart__meta-text">';
		tr += '		<strong>Custom Note :</strong> ' + Car.CustomNote + '';
		tr += '	</div>';
	}
	tr += '		</div>';
	tr += '</td>';
	tr += '	<td>AED ' + Car.UnitPrice + '</td>';
	tr += '	<td class="quantity">' + Car.Quantity + '</td>';
	tr += '	<td>AED ' + Car.Price + '</td>';
	tr += '</tr>';

	var li = '<li class="item CartCar' + Car.RowId + '">';
	li += '		<a class="car-image" href="/car/' + Car.Slug + '">';
	li += '			<img class="img-lazyload" src="' + Car.Thumbnail + '" alt="' + Car.Title + '" title="" />';
	li += '		</a>';
	li += '		<div class="car-details">';
	li += '			<a class="pName" href="/car/' + Car.Slug + '">' + Car.Title + '</a>';
	if (Car.Attributes.length > 0) {
		li += '			<div class="variant-cart">' + GetAttributeString(Car.Attributes) + '</div>';
	}
	li += '			<div class="wrapQtyBtn">';
	li += '				<div class="qtyField">';
	li += '					<span class="label">Qty:</span>';
	li += '					<input type="text" id="Quantity" name="quantity" value="' + Car.Quantity + '" class="car-form__input qty" readonly="readonly">';
	li += '				</div>';
	li += '			</div>';
	li += '			<div class="priceRow">';
	li += '				<div class="car-price">';
	li += '					<span class="money">AED ' + Car.Price + '</span>';
	li += '				</div>';
	li += '			</div>';
	li += '		</div>';
	li += '	</li>';

	$('#CheckoutCars').append(tr);
	
	setTimeout(function () { OnErrorImage(); }, 3000);

}

function BindCartTotal() {

	var Subtotal = parseFloat(ShoppingCart.sum("Price"));
	$('#Subtotal').html('AED ' + Subtotal);
	if (grandFlag) {
		grandTotal = +Number(Subtotal).toFixed(2);
		grandFlag = false;
	}
	Subtotal -= Order.DiscountAmount;
	Subtotal -= Order.CouponDiscount;
	Subtotal += Order.DeliveryCharges;

	Order.TaxAmount = (Subtotal * Order.TaxPercent) / 100;
	$('#TaxAmount').html('AED ' + Number(Order.TaxAmount).toFixed(2));

	Subtotal += Order.TaxAmount;
	$('#Amount').html('AED ' + Number(Subtotal).toFixed(2));
	CartFinalAmount = Subtotal;
	Subtotal -= Order.RedeemAmount;

	$('#TotalAmount').html('AED ' + Number(Subtotal).toFixed(2));

}

function FillOrder() {

	Order.GuestDetails.FirstName = $('#FirstName').val();
	Order.GuestDetails.LastName = $('#LastName').val();
	Order.GuestDetails.Email = $('#Email').val();
	Order.GuestDetails.Telephone = $('#Telephone').val();
	Order.GuestDetails.CreateAccount = $('#CreateAccount').prop('checked');


	if ($('input[name=rdoShippingAddress]:checked').attr("id") == "rdoNewAddress") {
		Order.DeliveryAddress.CountryID = $('#CountryID').val();
		Order.DeliveryAddress.CityID = $('#CityID').val();
		Order.DeliveryAddress.AreaID = $('#AreaID').val();
		Order.DeliveryAddress.Address = $('#Address').val();
	} else {
		var address = $('input[name=rdoShippingAddress]:checked').closest('.basicsAccordion').find("p");

		Order.DeliveryAddress.CountryID = address.attr("country-id");
		Order.DeliveryAddress.CityID = address.attr("state-id");
		Order.DeliveryAddress.AreaID = address.attr("area-id");
		Order.DeliveryAddress.Address = $(address).find(".address").text().trim();
	}

	//Order.DeliveryAddress.CountryID = $('#CountryID').val();
	//Order.DeliveryAddress.CityID = $('#CityID').val();
	//Order.DeliveryAddress.AreaID = $('#AreaID').val();
	//Order.DeliveryAddress.Address = $('#Address').val();


	Order.DeliveryAddress.Contact = $('#Contact').val();
	Order.Note = $('#Notes').val();
	Order.DeliveryDate = $('#DeliveryDate').val();
	Order.OrderDetails = ShoppingCart;

}

function CheckCheckoutValidity() {
	if (ShoppingCart.length <= 0) {
		BindEmptyCart();
	} else {
		if (IsShippingAvailable && IsMinimumOrderExtended && IsCouponApplicable && IsTermAndConditionsAccepted && !IsOutOfStock) {
			$('#btnOrder').prop('disabled', false);
		} else {
			$('#btnOrder').prop('disabled', true);
		}
	}
}

function CheckCartStock() {
	var CarIds = ShoppingCart.filter(function (obj) { return obj.CarVaraiationID == null; }).map(a => a.CarID);
	var CarVariationIds = ShoppingCart.filter(function (obj) { return obj.CarVaraiationID != null; }).map(a => a.CarVaraiationID);

	CarIds = CarIds ? CarIds : [];
	CarVariationIds = CarVariationIds ? CarVariationIds : []

	$.ajax({
		url: '/cart/stock',
		type: 'POST',
		data: {
			__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
			cartCar: {
				CarIds: CarIds,
				CarVariationIds: CarVariationIds
			}
		},
		success: function (response) {
			if (response.success) {
				$.each(response.data, function (k, v) {
					if (v.CarVariationID) {
						var tr = $('.checkout_cart_item[car_attribute_id=' + v.CarVaraiationID + ']');
						if (v.Stock != -1 && Number(tr.find('.quantity').text().trim()) > v.Stock) {
							$(tr).addClass('out-of-stock');
						} else {
							$(tr).removeClass('out-of-stock');
						}
					} else {
						var tr = $('.checkout_cart_item[car_id=' + v.CarID + ']');

						if (v.Stock != -1 && Number(tr.find('.quantity').text().trim()) > v.Stock) {
							$(tr).addClass('out-of-stock');
						} else {
							$(tr).removeClass('out-of-stock');
						}
					}
				});

				if ($('.checkout_cart_item .out-of-stock').length > 0) {
					IsOutOfStock = true;
				} else {
					IsOutOfStock = false;
				}
			}
			else {
				IsOutOfStock = false;
			}

			CheckCheckoutValidity();
		}
	})
}

function CheckCouponCategories(response) {
	couponCategories = response.categories;
	let ItemTotalPrice = 0;
	var Discount = 0;
	for (let i = 0; i < ShoppingCart.length; i++) {
		let couponallowed = false;

		let ItemPrice = 0;
		let ItemDiscount = 0;

		$.each(ShoppingCart[i].Categories, function (k, v) {
			if (couponCategories.includes(v.ID)) {
				couponallowed = true;
			}
		});

		if (couponallowed) {
			IsCouponApplicable = true;

			var cartItem = ShoppingCart[i];
			ItemPrice = cartItem.Price * cartItem.Quantity;

			if (cartItem.CarVaraiationID) {
				var tr = $('.checkout_cart_item[car_attribute_id=' + cartItem.CarVaraiationID + ']');
				$(tr).find('.coupon-badge').html(`COUPON APPLIED`).fadeIn();
			} else {
				var tr = $('.checkout_cart_item[car_id=' + cartItem.CarID + ']');
				$(tr).find('.coupon-badge').html(`COUPON APPLIED`).fadeIn();
			}

			if (response.Type && response.Type == "Percentage") {

				ItemDiscount = (parseFloat(ItemPrice) * response.Value) / 100;
				ItemDiscount = ItemDiscount > response.MaxAmount ? response.MaxAmount : ItemDiscount;

			} else {
				ItemDiscount = response.Value * cartItem.Quantity;

				if (ItemDiscount > ItemPrice) {
					ItemDiscount = ItemPrice;
				}
			}
			Discount += ItemDiscount;
		}
	}

	if (IsCouponApplicable) {
		//if (response.Type && response.Type == "Percentage") {
		//	Discount = (parseFloat(ItemTotalPrice) * response.Value) / 100;
		//	Discount = Discount > response.MaxAmount ? response.MaxAmount : Discount;
		//} else {
		//	Discount = response.Value;
		//}
		if (Discount > 0) {
			$(this).val('');

			$('#RedeemAmount').html('-');
			Order.RedeemAmount = 0;

			var Subtotal = parseFloat(ShoppingCart.sum("Price"));

			if (Discount > Subtotal) {
				Discount = Subtotal;
			}

			$('#Discount').html(`${currency} ${Discount}`);

			$('#coupon-code').removeClass('border-danger').addClass('border-success');
			$('#coupon-message').html(`woohoo you saved ${currency} ${Discount}`).removeClass('text-danger').addClass('text-success');

			Order.CouponCode = $('#coupon-code').val();
			Order.CouponDiscount = Discount;
			BindCartTotal();
		} else {
			$('#coupon-message').html(`${$('#coupon-code').val()} is only for `).removeClass('text-danger').addClass('ml-2');
			$.each(couponCategories, function (k, v) {
				$('#coupon-message').append(`<span class="badge badge-dark">${v.Name}</span>`);
			});
		}
	}

	CheckCheckoutValidity();
}

function CheckCouponCategories(response) {
	couponCategories = response.categories;
	let ItemTotalPrice = 0;
	var Discount = 0;
	for (let i = 0; i < ShoppingCart.length; i++) {
		let couponallowed = false;

		let ItemPrice = 0;
		let ItemDiscount = 0;

		$.each(ShoppingCart[i].Categories, function (k, v) {
			if (couponCategories.includes(v.ID)) {
				couponallowed = true;
			}
		});

		if (couponallowed) {
			IsCouponApplicable = true;

			var cartItem = ShoppingCart[i];
			ItemPrice = cartItem.Price * cartItem.Quantity;

			if (cartItem.CarVaraiationID) {
				var tr = $('.checkout_cart_item[car_attribute_id=' + cartItem.CarVaraiationID + ']');
				$(tr).find('.coupon-badge').html(`COUPON APPLIED`).fadeIn();
			} else {
				var tr = $('.checkout_cart_item[car_id=' + cartItem.CarID + ']');
				$(tr).find('.coupon-badge').html(`COUPON APPLIED`).fadeIn();
			}

			if (response.Type && response.Type == "Percentage") {

				ItemDiscount = (parseFloat(ItemPrice) * response.Value) / 100;
				ItemDiscount = ItemDiscount > response.MaxAmount ? response.MaxAmount : ItemDiscount;

			} else {
				ItemDiscount = response.Value * cartItem.Quantity;

				if (ItemDiscount > ItemPrice) {
					ItemDiscount = ItemPrice;
				}
			}
			Discount += ItemDiscount;
		}
	}

	if (IsCouponApplicable) {
		//if (response.Type && response.Type == "Percentage") {
		//	Discount = (parseFloat(ItemTotalPrice) * response.Value) / 100;
		//	Discount = Discount > response.MaxAmount ? response.MaxAmount : Discount;
		//} else {
		//	Discount = response.Value;
		//}
		if (Discount > 0) {
			$(this).val('');

			$('#RedeemAmount').html('-');
			Order.RedeemAmount = 0;

			var Subtotal = parseFloat(ShoppingCart.sum("Price"));

			if (Discount > Subtotal) {
				Discount = Subtotal;
			}

			$('#Discount').html(`${currency} ${Discount}`);

			$('#coupon-code').removeClass('border-danger').addClass('border-success');
			$('#coupon-message').html(`woohoo you saved ${currency} ${Discount}`).removeClass('text-danger').addClass('text-success');

			Order.CouponCode = $('#coupon-code').val();
			Order.CouponDiscount = Discount;
			BindCartTotal();
		} else {
			$('#coupon-message').html(`${$('#coupon-code').val()} is only for `).removeClass('text-danger').addClass('ml-2');
			$.each(couponCategories, function (k, v) {
				$('#coupon-message').append(`<span class="badge badge-dark">${v.Name}</span>`);
			});
		}
	}

	CheckCheckoutValidity();
}

function RemoveCheckoutCartItem(id) {

	$('.checkout_cart_item[id=' + id + ']').fadeOut(300, function () { $(this).remove(); });
	BindCartTotal();
	CheckCheckoutValidity();
}

function BindEmptyCart() {
	var EmptyCartTemplate = `<div class="row">
            	<div class="col-12 col-sm-12 col-md-12 col-lg-12">
        			<div class="empty-page-content text-center">
                        <h1>Your cart is empty</h1>
                        <p><a href="/" class="btn btn--has-icon-after">Continue shopping <i class="fa fa-caret-right" aria-hidden="true"></i></a></p>
                      </div>
        		</div>
        	</div>`;

	$("#checkout-container").html(EmptyCartTemplate);
}

function GetDeliveryAddresses() {

	$.ajax({
		type: 'GET',
		url: `/customer/deliveryaddress/getall?en=${lang}`,
		success: function (response) {
			BindDeliveryAddresses(response);
		}
	});

}

function BindDeliveryAddresses(response) {

	var htmlTemplate = '';
	//$("#ShippingAddress").empty();
	$.each(response.data, function (k, v) {
		if (v.Type && v.Type != "") {
			htmlTemplate += `<div id="basicsAccordionShippingAddress${v.Type.replace(" ", "")}" class="basicsAccordion">
								<div class="border-bottom border-color-1 border-dotted-bottom">
									<div class ="p-3" id="basicsHeading${v.Type.replace(" ", "")}">
										<div class="custom-control custom-radio">
											<input type="radio" class ="custom-control-input" ${v.IsDefault == true ? `checked` : ``} id="rdo${v.Type.replace(" ", "")}" name="rdoShippingAddress">
											<label class ="custom-control-label form-label" for="rdo${v.Type.replace(" ", "")}" data-toggle="collapse" data-target="#basicsCollapse${v.Type.replace(" ", "")}" aria-expanded="false" aria-controls="basicsCollapse${v.Type.replace(" ", "")}">
												${v.Type}
											</label>
										</div>
									</div>
									<div id="basicsCollapse${v.Type.replace(" ", "")}" class ="collapse border-top border-color-1 border-dotted-top bg-dark-lighter ${v.IsDefault == true ? "show" : ""}" aria-labelledby="basicsHeading${v.Type.replace(" ", "")}" data-parent="#basicsAccordionShippingAddress${v.Type.replace(" ", "")}">
										<div class="p-4">
											<p country-id="${v.Country.id}" state-id="${v.State.id}" area-id="${v.Area.id}"><i class ="fa fa-map-marker-alt"></i><span class="address">${v.Address}</span>, <br> ${v.Area.name}, <br> ${v.State.name}, ${v.Country.name}.</p>

										</div>
									</div>
								</div>
							</div>`

			//htmlTemplate += `<div class="delivery-address col-md-4 col-sm-12">
			//					<a class="delete margin-5px-bottom btn-secondary btn-sm" href="javacript:;" onclick="DeleteDeliveryAddresses(this,${v.ID})"><i class="fa fa-trash"></i> Delete</a>
			//					<p><i class ="fa fa-tag"></i>${v.Type} <br><i class="fa fa-map-marker-alt"></i>${v.Address}, <br> ${v.Area.name}, <br> ${v.State.name}, ${v.Country.name}.</p>
			//				</div>`;
		}
	});

	$("#ShippingAddress").append(htmlTemplate);

	if (response.data.length == 0 || $('#CountryID').val()) {
		$("#rdoNewAddress").click();
		$("label[for=rdoNewAddress]").trigger('click');
	} else {
		//$('.total-amount').each(function () {
		//	$(this).html(kFormatter(Number($(this).text()))).removeClass('total-amount');
		//});
	}

	$(".filter-loader").hide();
	$("#load-more").hide();

	$("input[name=rdoShippingAddress]").change(function () {

		if ($(this).prop("checked")) {
			if ($(this).attr("id") == "rdoNewAddress") {
				//$('#CountryID').val('').prop('disabled', false).prop('required', true);
				//$('#CityID').val('').prop('disabled', false).prop('required', true);
				//$('#AreaID').val('').prop('disabled', false).prop('required', true);
				//$('#Address').val('').prop('disabled', false).prop('required', true);

				$('#CountryID').prop('disabled', false).prop('required', true);
				$('#CityID').prop('disabled', false).prop('required', true);
				$('#AreaID').prop('disabled', false).prop('required', true);
				$('#Address').prop('disabled', false).prop('required', true);
			} else {
				//$('#CountryID').val('').prop('disabled', true).prop('required', false);
				//$('#CityID').val('').prop('disabled', true).prop('required', false);
				//$('#AreaID').val('').prop('disabled', true).prop('required', false);
				//$('#Address').val('').prop('disabled', true).prop('required', false);

				$('#CountryID').prop('disabled', true).prop('required', false);
				$('#CityID').prop('disabled', true).prop('required', false);
				$('#AreaID').prop('disabled', true).prop('required', false);
				$('#Address').prop('disabled', true).prop('required', false);
			}
		}

		$('#btnCalculateShippingCost').trigger('click');
	});

	$("input[name=rdoShippingAddress]").trigger('change');

	$("input[name=rdoShippingAddress]")
	if (response.data.length > 0) {
		var address = response.data.find(function (obj) {
			return obj.IsDefault == true;
		});

		if (address) {

		}
	} else {
		if ($("#CountryID").val()) {

		}
	}
}

function RepeatOrder(orderId) {

	$.ajax({
		url: '/Customer/Order/GetOrderShortDetails?orderId=' + orderId,
		type: 'GET',
		success: function (response) {
			if (response.success) {
				console.log(response.order);
				Order.OrderRef = response.order.OrderNo;
				GetDeliveryAddresses();

				$('#Subtotal').html('AED ' + parseFloat(ShoppingCart.sum("Price")));
				if ($("#TaxPercentage").text().trim()) {
					Order.TaxPercent = parseFloat($("#TaxPercentage").text());
				} else {
					Order.TaxPercent = 0;

					$("#TaxPercentage").text('0');
				}

				$('input[name=RedeemAmount]').change(function () {
					let redeemAmount = parseFloat($(this).val());
					if (redeemAmount) {
						if (redeemAmount > parseFloat($(this).attr('max'))) {
							$(this).val($(this).attr('max'));
							redeemAmount = parseFloat($(this).attr('max'));
						} else if (redeemAmount < parseFloat($(this).attr('min'))) {
							$(this).val($(this).attr('min'));
							redeemAmount = parseFloat($(this).attr('min'));
						}

						if (redeemAmount > CartFinalAmount) {
							redeemAmount = CartFinalAmount;

							$(this).val(redeemAmount);
						}

						Order.RedeemAmount = parseFloat($(this).val());
						$('#RedeemAmount').html(`${currency} ${Order.RedeemAmount}`);
						FormatAmount($('#RedeemAmount'), false);

						BindCartTotal();

					} else {
						$(this).val('');

						$('#RedeemAmount').html('-');
						Order.RedeemAmount = 0;

						BindCartTotal();
					}
				});

				$('#ChkTerms').change(function () {
					if (this.checked) {
						IsTermAndConditionsAccepted = true;
					} else {
						IsTermAndConditionsAccepted = false;
					}
					CheckCheckoutValidity();
				});

				$('#btnApplyCoupon').click(function () {
					if ($('#coupon-code').val()) {
						$.ajax({
							url: '/coupons/redeem',
							type: 'POST',
							data: {
								__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
								coupon: {
									CouponCode: $('#coupon-code').val()
								}
							},
							success: function (response) {
								if (response.success) {
									IsCouponApplicable = true;
									var Discount;
									if (response.Type && response.Type == "Percentage") {
										Discount = (parseFloat(ShoppingCart.sum("Price")) * response.Value) / 100;
										Discount = Discount > response.MaxAmount ? response.MaxAmount : Discount;
									} else {
										Discount = response.Value;
									}

									$('#Discount').html('AED ' + Discount);

									$('#coupon-code').removeClass('border-danger').addClass('border-success');
									$('#coupon-message').html('woohoo you saved AED ' + Discount).removeClass('text-danger').addClass('text-success');

									Order.CouponCode = $('#coupon-code').val();
									Order.CouponDiscount = Discount;
									BindCartTotal();

								}
								else {
									IsCouponApplicable = false;
									$('#coupon-code').removeClass('border-success').addClass('border-danger');
									$('#coupon-message').html(response.message).removeClass('text-success').addClass('text-danger');
								}
								$('#btnCancelCoupon').slideDown();
								CheckCheckoutValidity();
							}
						})
					}
				});

				$('#btnCancelCoupon').click(function () {

					$('#btnCancelCoupon').slideUp();

					$('#Discount').html('-');
					Order.CouponCode = null;
					Order.CouponDiscount = 0;

					$('#coupon-code').removeClass('border-danger').removeClass('border-success');
					$('#coupon-code').val('');
					$('#coupon-message').html('');


					IsCouponApplicable = true;
					BindCartTotal();
					CheckCheckoutValidity();

				});

				$('#CountryID').change(function () {
					var count = 0;
					var $dropdown = $("#CityID");
					if ($(this).val() == 0) {
						$dropdown.empty();
						$dropdown.append($("<option />").val('').text("Select Country First!"));

						$('#CityID').trigger('change');

					}
					else {
						$.ajax({
							type: 'Get',
							url: '/Cities/GetCitiesByCountry/' + $(this).val(),
							success: function (response) {
								$dropdown.empty();
								$dropdown.append($("<option />").val('').text("Select City"));

								$.each(response.data, function (k, v) {
									$dropdown.append($("<option />").val(v.value).text(v.text));
									count++;
								});
							}
						});
					}
				});

				$('#CityID').change(function () {
					var count = 0;
					var $dropdown = $("#AreaID");
					if ($(this).val() == 0) {
						$dropdown.empty();
						$dropdown.append($("<option />").val('').text("Select City First!"));
						$('#AreaID').trigger('change');
					}
					else {
						$.ajax({
							type: 'Get',
							url: '/Areas/GetAreasByCity/' + $(this).val(),
							success: function (response) {
								$dropdown.empty();
								$dropdown.append($("<option />").val('').text("Select Area"));

								$.each(response.data, function (k, v) {
									$dropdown.append($("<option />").val(v.value).text(v.text));
									count++;
								});
							}
						});
					}
				});

				$('#AreaID').change(function () {

					IsShippingAvailable = false;


					Order.DeliveryCharges = null;
					$('#Shipping').html('-');
					$('#subtotal-message').html('');
					$('#btnCalculateShippingCost').html('Calculate Shipping Cost');
					BindCartTotal();
					if ($(this).val()) {
						$('#btnCalculateShippingCost').prop('disabled', false);
						$('#btnCalculateShippingCost').trigger('click');
					}
					else {
						$('#btnCalculateShippingCost').prop('disabled', true);
					}
					CheckCheckoutValidity();
				});

				$('#btnCalculateShippingCost').click(function () {

					let AreaId;
					if ($('input[name=rdoShippingAddress]:checked').attr("id") == "rdoNewAddress") {
						AreaId = $('#AreaID').val();
					} else {
						var address = $('input[name=rdoShippingAddress]:checked').closest('.basicsAccordion').find("p");
						AreaId = address.attr("area-id");
					}

					if (AreaId) {
						$.ajax({
							type: 'Get',
							url: '/DeliveryCharges/GetDeliveryChargesByArea/' + AreaId,
							success: function (response) {
								if (response.success) {
									IsShippingAvailable = true;
									$('#Shipping').html(`${currency} ${parseFloat(response.data.charges)}`);

									FormatAmount($('#Shipping'), false);
									Order.DeliveryCharges = response.data.charges;
									$('#btnCalculateShippingCost').html(`Shipping : ${currency} ${parseFloat(response.data.charges)}`);
									$('#btnCalculateShippingCost').prop('disabled', true);

									if (parseFloat(ShoppingCart.sum("Price")) < response.data.minOrder) {
										$('#subtotal-message').html(`( Minimum order price is ${response.data.minOrder} )`).addClass('text-danger');
										IsMinimumOrderExtended = false;
									} else {
										$('#subtotal-message').html('');
										IsMinimumOrderExtended = true;
									}
									BindCartTotal();
								} else {
									IsShippingAvailable = false;
									//$('#btnOrder').prop('disabled', true);
									$('#message').html('<span class="fa fa-warning fa-2x"></span> ' + response.message); /*Unable to process your request*/
									$('#message').fadeIn();
									MessageSlideUP();
								}
								CheckCheckoutValidity();
							}
						});
					}
					else {
						IsShippingAvailable = false;
						CheckCheckoutValidity();
					}
				});

				if (!$('#AreaID').val()) {
					$('#CityID').trigger('change');
				} else {
					$('#btnCalculateShippingCost').trigger('click');
				}

				$("#OrderForm").on("submit", function (event) {
					$('#message').html('<span class="fa fa-hour-watch"></span> Placing Order ...')
					$('#message').slideDown();
					MessageSlideUP();
					try {
						if (ShoppingCart.length > 0) {
							$("#btnOrder").html('<i class="fa fa-spinner fa-spin"></i> Place order').prop('disabled', true);
							FillOrder();
							var $this = $(this);
							$.ajax({
								url: '/Orders/Create',
								type: 'post',
								data: { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(), orderViewModel: Order },
								success: function (response) {

									if (response.success) {
										//$('#message').html('<span class="fa fa-check"></span> ' + response.message);

										$("html, body").animate({ scrollTop: 0 }, 1000);
										//setTimeout(function () {
										//	$('#message').slideUp();
										//}, 3000);
										ShoppingCart = [];
										localStorage.setItem("cart", []);
										UpdateCartCounter();
										FillCart();
										UpdateTotal();

										//window.location = '/Customer/Order/Details/' + response.order.id;
										$('#message').slideUp();

										var EmptyCartTemplate = `<div class="row">
															<div class="col-12 col-sm-12 col-md-12 col-lg-12">
        														<div class ="empty-page-content text-center" style="padding: 40px;">
																	<i class ="fa fa-3x fa-check-circle font-size-130 text-success" style="color:#110044 !important;font-size: 12em;"></i>
																	<h5>Order # ${response.order.OrderNo}</h5>
																	<h4>Your Order has been Placed Successfully</h4>
														            <p>
																		<a href="/" class="btn btn--has-icon-after">
																			Continue shopping
																			<i class="fa fa-caret-right" aria-hidden="true"></i>
																		</a>
																	</p>
														          </div>
        													</div>
        												</div>`;

										$("#checkout-container").html(EmptyCartTemplate);

									}
									else {

										$('#message').html('<span class="fa fa-warning fa-2x"></span> ' + response.message);
										$('#message').slideDown();
										MessageSlideUP();

										if (response.isOutOfStock) {
											IsOutOfStock = true;
											$.each(response.data, function (k, v) {
												if (v) {
													$('.checkout_cart_item[car_attribute_id=' + v + ']').addClass('out-of-stock');;

												} else {
													$('.checkout_cart_item[car_id=' + k + ']').addClass('out-of-stock');
												}
											});

											CheckCheckoutValidity();
										}
										$("#btnOrder").html('Place order');
									}
								}
							})
								.done(function (e) {
									console.log(e);
								})
								.fail(function (e) {
									if (e.status == 403) {
										window.location = er.responseJSON.LogOnUrl;
									}
								});
							event.preventDefault();
						}
						else {
							BindEmptyCart();
							return false;
						}
					}
					catch (ex) {
						$("#btnOrder").html('Place order').prop('disabled', false);
						$('#message').html('<span class="fa fa-warning fa-2x"></span> Unable to process your order');
						$('#message').slideDown();
						MessageSlideUP();
						return false;
					}
					return false;
				});

				CheckCartStock();

				BindCartTotal();

				//if (!IsLoyaltyEnabled) {
				//	$('.redeem-amount-container').remove();
				//}

				/*Bind Address*/
				
				$("#rdoNewAddress").click();
				$("label[for=rdoNewAddress]").trigger('click');

				let deliveryAddress = response.order.DeliveryAddress;

				$('#CountryID').val(deliveryAddress.Country.ID);
				$('#CityID').val(deliveryAddress.City.ID);
				$('#AreaID').val(deliveryAddress.Area.ID);
				$('#Address').val(deliveryAddress.Address);

				/*Bind Other Details*/
				$('#Contact').val(deliveryAddress.Contact);
				$('#Notes').val(response.order.Note);
				$('#DeliveryDate').val(response.order.DeliveryDate);

				//

			} else {
				BindEmptyCart();
			}
		},
		error: function (e) {
			BindEmptyCart();
		},
		failure: function (e) {
			BindEmptyCart();
		}
	});
}

function getParameterByName(name, url) {
	if (!url) url = window.location.href;
	name = name.replace(/[\[\]]/g, "\\$&");
	var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
		results = regex.exec(url);
	if (!results) return null;
	if (!results[2]) return '';
	return decodeURIComponent(results[2].replace(/\+/g, " "));
}

function MessageSlideUP() {
	setTimeout(function () {
		$('#message').slideUp();
	},5000);
}


