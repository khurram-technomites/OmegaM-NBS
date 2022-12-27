var ShoppingCart = [];

$(document).ready(function () {
	var cart = localStorage.getItem("cart");
	if (cart) {
		ShoppingCart = JSON.parse(cart);
	}
	UpdateCartCounter();
	FillCart();
	UpdateTotal();

	

});

function UpdateCartCounter() {
	if (ShoppingCart.length > 0) {

		$('.site-header__cart-count').html(ShoppingCart.length);
		$('.site-header__cart-count').fadeIn();

		$('#CartCount_md').html(ShoppingCart.length);
		$('#CartCount_md').fadeIn();
		$('.btn-check-out').fadeIn();
	} else {
		$('.site-header__cart-count').html('');
		$('.site-header__cart-count').fadeOut();

		$('#CartCount_md').html('');
		$('#CartCount_md').fadeOut();

		$('.btn-check-out').fadeOut();
	}
}

function FillCart() {

	if (ShoppingCart.length > 0) {
		$('.site-cart .mini-cars-list').html('');
		for (var i = 0; i < ShoppingCart.length ; i++) {
			UpdateCart(ShoppingCart[i]);
			if (typeof BindOrderCart != 'undefined') {
				BindOrderCart(ShoppingCart[i]);
			} else if (typeof BindCart != 'undefined') {
				BindCart(ShoppingCart[i]);
			}
		}
	}
	else {
		$('.site-cart .mini-cars-list').html('<li class="item empty-cart text-center">Cart is empty</li>');
	}
}

function UpdateTotal() {
	if (ShoppingCart.length > 0) {
		$('.site-cart .total .money').html('AED ' + parseFloat(ShoppingCart.sum("Price")));
	} else {
		$('.site-cart .total .money').html('AED 0.0');
	}
}

function UpdateCart(Car) {
	var li = '<li class="item CartCar' + Car.RowId + '">';
	li += '		<a class="car-image" href="/car/' + Car.Slug + '">';
	li += '			<img class="img-lazyload" src="' + Car.Thumbnail + '" alt="' + Car.Title + '" title="" />';
	li += '		</a>';
	li += '		<div class="car-details">';
	li += '			<a href="javascript:;" class="remove" onclick="RemoveFromCart(this,\'' + Car.RowId + '\')">';
	li += '				<i class="anm anm-times-l" aria-hidden="true"></i>';
	li += '			</a>';
	//li += '			<a href="/Cars/Details/' + Car.CarID + '" class="edit-i remove">';
	//li += '				<i class="anm anm-edit" aria-hidden="true"></i>';
	//li += '			</a>';
	li += '			<a class="pName" href="/car/' + Car.Slug + '">' + Car.Title + '</a>';
	if (Car.Attributes.length > 0) {
		li += '			<div class="variant-cart">' + GetAttributeString(Car.Attributes) + '</div>';
	}
	li += '			<div class="wrapQtyBtn">';
	li += '				<div class="qtyField">';
	li += '					<span class="label">Qty:</span>';
	//li += '					<a class="qtyBtn minus" href="javascript:void(0);">';
	//li += '						<i class="fa anm anm-minus-r" aria-hidden="true"></i>';
	//li += '					</a>';
	li += '					<input type="text" id="Quantity" name="quantity" value="' + Car.Quantity + '" class="car-form__input qty" readonly="readonly">';
	//li += '					<a class="qtyBtn plus" href="javascript:void(0);">';
	//li += '						<i class="fa anm anm-plus-r" aria-hidden="true"></i>';
	//li += '					</a>';
	li += '				</div>';
	li += '			</div>';
	li += '			<div class="priceRow">';
	li += '				<div class="car-price">';
	li += '					<span class="money">AED ' + Car.Price + '</span>';
	li += '				</div>';
	li += '			</div>';
	li += '		</div>';
	li += '	</li>';

	if ($('.temp-cart' + Car.RowId).length != 0) {

		$('.temp-cart' + Car.RowId).after(li);
		$('.temp-cart' + Car.RowId).remove();
	} else {
		$('.site-cart .mini-cars-list').append(li);
	}

	setTimeout(function () { OnErrorImage(); }, 3000);
}

function AddToCart(Car) {
	let TempItemDetails = Object.assign({}, Car);
	TempItemDetails.Attributes = Array.from(Car.Attributes);

	if (!Car.RowId) {
		var rowCount = ShoppingCart.filter(function (obj) {
			return obj.CarID == Car.CarID;
		}).length + 1;
		TempItemDetails.RowId = TempItemDetails.CarID + '-' + rowCount;
	}
	ShoppingCart.push(TempItemDetails);
	localStorage.setItem("cart", JSON.stringify(ShoppingCart));

	$('.CartCar' + TempItemDetails.RowId).after("<div class=\"temp-cart" + TempItemDetails.RowId + "\"></div>");
	$('.CartCar' + TempItemDetails.RowId).remove();

	UpdateCartCounter();
	UpdateCart(TempItemDetails);
	UpdateTotal();
}

function RemoveFromCart(elem, id) {

	$('.CartCar' + id).fadeOut(300, function () { $(this).remove(); });

	ShoppingCart = ShoppingCart.filter(function (obj) {
		return obj.RowId !== id;
	});

	localStorage.setItem("cart", JSON.stringify(ShoppingCart));
	UpdateCartCounter();
	UpdateTotal();

	if (typeof RemoveCartItem != "undefined") {
		$('.cart__row[id=' + id + ']').fadeOut(300, function () { $(this).remove(); });
		$('#Subtotal').html('AED ' + parseFloat(ShoppingCart.sum("Price")));
	}


	if (typeof RemoveCheckoutCartItem != "undefined") {
		RemoveCheckoutCartItem(id);
	}

	var url = window.location.href;
	var arrurl = url.split("/");
	var lastpath = arrurl.pop();
	var restpath = arrurl.join("/");
	if (restpath.includes("car")) {
		$.ajax({
			type: 'GET',
			url: '/en/cars/' + GetURLParameter(),
			success: function (response) {
				if (response.success) {
					console.log(response.data);
					details = response.data;
					Car.CarID = details.ID;
					Car.Title = details.Title;
					Car.Slug = details.SKU
					Car.Thumbnail = details.Thumbnail;
					Car.VendorID = details.Vendor.ID;

					if (details.Type == '1' || details.Type == 'Simple') {
						BindSimpleCar(details);
					}
					else {
						$('#carAttributes .swatch input').change(function () {
							selectedAttributes = [];

							$('#carAttributes .swatch input:checked').each(function () {
								selectedAttributes.push($(this).attr('car-attribute-id'));
							});

							var variation = details.Variations.find(function (obj) {
								return isEqual(obj.Attributes, selectedAttributes);
							});

							if (variation) {
								BindVariableCar(variation);

								if (variation.Stock) {
									$('#btnAddToCart').prop('disabled', false);
									$('#btnBuyNow').prop('disabled', false);
								}

							} else {
								$('#btnAddToCart').prop('disabled', true);
								$('#btnBuyNow').prop('disabled', true);
							}
						});

						$('.swatch').each(function () { $(this).find('input:first').prop('checked', true) });
						//$('.swatch').each(function () { $(this).find('input:first').trigger('click') });
						$('.swatch input:first').trigger('change');
					}
				} else { }
			}
		});
		var CheckCart = ShoppingCart.find(function (obj) {
			return obj.CarID == Car.CarID
				&& obj.CarVaraiationID == Car.CarVaraiationID
				&& isEqual(obj.Attributes, Car.Attributes);
		});
		if (CheckCart) {
			if (CheckCart.Quantity) {
				variation.Stock = variation.Stock - CheckCart.Quantity;
				$('#cartStock').html(CheckCart.Quantity + ' in Cart').show();
			}
		}
	}

	CheckIsSoldIndividually(ShoppingCart.CarID, ShoppingCart.CarVaraiationID);

}

function GetAttributeString(array) {
	var Attributes = '';
	array.forEach(function (k, v) {
		if (k.Name.toUpperCase() == "COLOR") {
			Attributes += k.Name + ' : <span class="dot" style="background:' + k.Value + '"></span> /';
		} else {
			Attributes += k.Name + ' : <span class="">' + k.Value + '</span> /';
		}
	});

	return Attributes.slice(0, -1);
}

Array.prototype.sum = function (prop) {
	var total = 0
	for (var i = 0, _len = this.length; i < _len; i++) {
		total += this[i][prop]
	}
	return total
}

var isEqual = function (value, other) {

	// Get the value type
	var type = Object.prototype.toString.call(value);

	// If the two objects are not the same type, return false
	if (type !== Object.prototype.toString.call(other)) return false;

	// If items are not an object or array, return false
	if (['[object Array]', '[object Object]'].indexOf(type) < 0) return false;

	// Compare the length of the length of the two items
	var valueLen = type === '[object Array]' ? value.length : Object.keys(value).length;
	var otherLen = type === '[object Array]' ? other.length : Object.keys(other).length;
	if (valueLen !== otherLen) return false;

	// Compare two items
	var compare = function (item1, item2) {

		// Get the object type
		var itemType = Object.prototype.toString.call(item1);

		// If an object or array, compare recursively
		if (['[object Array]', '[object Object]'].indexOf(itemType) >= 0) {
			if (!isEqual(item1, item2)) return false;
		}

			// Otherwise, do a simple comparison
		else {

			// If the two items are not the same type, return false
			if (itemType !== Object.prototype.toString.call(item2)) return false;

			// Else if it's a function, convert to a string and compare
			// Otherwise, just compare
			if (itemType === '[object Function]') {
				if (item1.toString() !== item2.toString()) return false;
			} else {
				if (item1 !== item2) return false;
			}

		}
	};

	// Compare properties
	if (type === '[object Array]') {
		for (var i = 0; i < valueLen; i++) {
			if (compare(value[i], other[i]) === false) return false;
		}
	} else {
		for (var key in value) {
			if (value.hasOwnProperty(key)) {
				if (compare(value[key], other[key]) === false) return false;
			}
		}
	}

	// If nothing failed, return true
	return true;

};


