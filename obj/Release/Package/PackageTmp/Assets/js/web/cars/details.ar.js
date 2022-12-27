var Product = {
	//RowId: ProductID + '-' + 1,
	RowId: null,
	ProductID: null,
	Slug: null,
	Title: null,
	Thumbnail: null,
	Quantity: 1,
	UnitPrice: null,
	Price: null,
	VendorID: null,
	CustomNote: null,
	ProductVaraiationID: null,
	Attributes: [],
	IsInStock: true
};

var details;
var selectedAttributes = [];

$(document).ready(function () {
	$('body').removeClass('template-index').removeClass('home2-default').addClass('template-product').addClass('belle').addClass('template-product-right-thumb');
	$('#btnAddToCart').prop('disabled', true);

	$.ajax({
		type: 'GET',
		url: '/' + lang + '/products/' + GetURLParameter(),
		success: function (response) {
			if (response.success) {
				console.log(response.data);
				details = response.data;

				Product.ProductID = details.ID;
				Product.Title = details.Title;
				Product.Slug = details.SKU;
				Product.Thumbnail = details.Thumbnail;
				Product.VendorID = details.Vendor.ID;

				$('.breadcrumbs span:first').text(details.Title);
				$('.product-single__title').text(details.Title);
				$('.product-single__description').html(details.ShortDescription);
				$('.product-description').html(details.LongDescription);

				/* Vendor Details Binding */
				$('.vendor-details').find('a').attr('href', '/' + lang + '/store/' + details.Vendor.Slug);
				$('.vendor-details').find('img').attr('src', details.Vendor.Logo);
				$('.vendor-details').find('h3').text(details.Vendor.Name);

				BindProductAttributes(details);

				$.each(details.categories, function (k, v) {
					$('.product-cat').append('<a class="mr-2" href="/' + lang + '/product-category/' + v + '" title="' + v + '" target="_blank">' + v + '</a>');
				});

				if (details.Type == '1' || details.Type == 'Simple') {
					BindProductImages(details);
					BindSimpleProduct(details);
					$('.swatch').each(function () { $(this).find('input:first').trigger('click') });
					if (details.Stock) {
						$('#btnAddToCart').prop('disabled', false);
						$('#btnBuyNow').prop('disabled', false);
					}
				} else {
					$('#productAttributes .swatch input').change(function () {
						selectedAttributes = [];

						$('#productAttributes .swatch input:checked').each(function () {
							selectedAttributes.push($(this).attr('product-attribute-id'));
						});

						var variation = details.Variations.find(function (obj) {
							return isEqual(obj.Attributes, selectedAttributes);
						});

						if (variation) {
							BindVariableProduct(variation);
							BindProductImages(details, variation);

							if (variation.Stock) {
								$('#btnAddToCart').prop('disabled', false);
								$('#btnBuyNow').prop('disabled', false);
							}

						} else {
							BindProductImages(details);
							$('.variant-sku').text('-');

							$('#ComparePrice-product-template').hide();
							$('.discount-badge').hide();
							$('#ProductPrice-product-template .money').text('AED 0.0');

							$('#quantity_message').hide();
							$('.instock').hide();
							$('.outstock').show();

							$('#btnAddToCart').prop('disabled', true);
							$('#btnBuyNow').prop('disabled', true);

						}
					});

					$('.swatch').each(function () { $(this).find('input:first').prop('checked', true) });
					//$('.swatch').each(function () { $(this).find('input:first').trigger('click') });
					$('.swatch input:first').trigger('change');
				}

				if (details.IsFeatured) {
					$('.product-labels').html('<span class="lbl pr-label1">Featured</span>');
				}
			} else {

			}
		}
	});

	/*Fetching and Binding of Product Rating */
	$.ajax({
		type: 'GET',
		url: '/products/' + GetURLParameter() + '/ratings',
		success: function (response) {
			BindProductRating(response.data);
		}
	});

	$(function () {
		var $pswp = $('.pswp')[0],
			image = [],
			getItems = function () {
				var items = [];
				$('.lightboximages a').each(function () {
					var $href = $(this).attr('href'),
						$size = $(this).data('size').split('x'),
						item = {
							src: $href,
							w: $size[0],
							h: $size[1]
						}
					items.push(item);
				});
				return items;
			}
		var items = getItems();
		$.each(items, function (index, value) {
			image[index] = new Image();
			image[index].src = value['src'];
		});
		$('.prlightbox').on('click', function (event) {
			event.preventDefault();
			var $index = $(".active-thumb").parent().attr('data-slick-index');
			$index++;
			$index = $index - 1;
			var options = {
				index: $index,
				bgOpacity: 0.9,
				showHideOpacity: true
			}
			var lightBox = new PhotoSwipe($pswp, PhotoSwipeUI_Default, items, options);
			lightBox.init();
		});
	});

	$('.product-form__item--quantity .qtyBtn.minus').on('click', function () {
		if (Product.Quantity > 1) {
			Product.Quantity -= 1;
		}
	});

	$('.product-form__item--quantity .qtyBtn.plus').on('click', function () {
		Product.Quantity += 1;
	});

	$('#btnBuyNow').click(function () {

		$('.empty-cart').remove();
		Product.Attributes = [];
		$('#productAttributes .swatch').each(function (k, v) {
			Product.Attributes.push({
				ID: $(v).attr('id'),
				Name: $(v).find('.header').text().trim().split(':')[0],
				Value: $(v).find('.swatchInput:checked').val(),
				ProductAttributeId: $(v).find('.swatchInput:checked').attr('product-attribute-id'),
			})
		});

		Item = ShoppingCart.find(function (obj) {
			return obj.ProductID == Product.ProductID
				&& obj.ProductVaraiationID == Product.ProductVaraiationID
				&& isEqual(obj.Attributes, Product.Attributes);
		});

		if (Item) {
			ShoppingCart = ShoppingCart.filter(function (obj) {
				return obj.RowId !== Item.RowId;
			});

			Item.Quantity += Product.Quantity;
			Item.UnitPrice = Product.UnitPrice;
			Item.Price = Item.Quantity * Item.UnitPrice;

			AddToCart(Item);
		} else {
			Product.Price = Product.Quantity * Product.UnitPrice
			AddToCart(Product);
		}

		window.location = '/checkout';
	});

	$('#btnAddToCart').click(function () {
		var cart = localStorage.getItem("cart");
		if (cart) {
			ShoppingCart = JSON.parse(cart);
		}

		$('.empty-cart').remove();
		Product.Attributes = [];
		$('#productAttributes .swatch').each(function (k, v) {
			Product.Attributes.push({
				ID: $(v).attr('id'),
				Name: $(v).find('.header').text().trim().split(':')[0],
				Value: $(v).find('.swatchInput:checked').val(),
				ProductAttributeId: $(v).find('.swatchInput:checked').attr('product-attribute-id'),
			})
		});

		Item = ShoppingCart.find(function (obj) {
			return obj.ProductID == Product.ProductID
				&& obj.ProductVaraiationID == Product.ProductVaraiationID
				&& isEqual(obj.Attributes, Product.Attributes);
		});

		if (Item) {
			//var variation = details.Variations.filter(function (obj) {
			//	return obj.ID == Item.ProductVaraiationID;
			//});

			//if (variation.SoldIndividually == false) {
			ShoppingCart = ShoppingCart.filter(function (obj) {
				return obj.RowId !== Item.RowId;
			});

			Item.CustomNote = $('#CustomNote').val();
			Item.Quantity += Product.Quantity;
			Item.UnitPrice = Product.UnitPrice;
			Item.Price = Item.Quantity * Item.UnitPrice;

			AddToCart(Item);
			//} else {
			//	$('#message').html('<span class="fa fa-check"></span> only one Product nature allowed per order!')
			//	$('#message').slideDown();

			//	setTimeout(function () {
			//		$('#message').slideUp();
			//	}, 3000);
			//}
		} else {
			Product.CustomNote = $('#CustomNote').val();
			Product.Price = Product.Quantity * Product.UnitPrice
			AddToCart(Product);
		}
	});
});

function BindProductImages(details, varaint) {

	if ($('.product-dec-slider-2').hasClass('slick-slider')) {

		$('.product-dec-slider-2').slick('unslick');
		$('#gallery').html('');
		$('.lightboximages').html('');
		//$('.product-dec-slider-2').slick('slickRemove');
	}
	if (!varaint) {
		$('#Thumbnail').attr('data-zoom-image', details.Thumbnail);
		$('#Thumbnail').attr('src', details.Thumbnail);
		//$('#gallery').append('<a data-image="' + details.Thumbnail + '" data-zoom-image="' + details.Thumbnail + '" class="slick-slide slick-cloned" data-slick-index="-4" aria-hidden="true" tabindex="-1">' +
		//								'<img class="blur-up lazyload" src="' + details.Thumbnail + '" alt="" />' +
		//							'</a>');

		//$('.lightboximages').append('<a href="' + details.Thumbnail + '" data-size="1462x2048"></a>');

		$.each(details.Images, function (k, v) {
			var template = '<a data-image="' + v + '" data-zoom-image="' + v + '" class="slick-slide slick-cloned" data-slick-index="-4" aria-hidden="true" tabindex="-1">' +
										'<img class="blur-up lazyload" src="' + v + '" alt="" />' +
									'</a>';

			$('#gallery').append(template);

			$('.lightboximages').append('<a href="' + v + '" data-size="1462x2048"></a>');
		});

	}
	else {

		$('#Thumbnail').attr('data-zoom-image', details.Thumbnail);
		$('#Thumbnail').attr('src', details.Thumbnail);

		$.each(varaint.Images, function (k, v) {
			var template = '<a data-image="' + v + '" data-zoom-image="' + v + '" class="slick-slide slick-cloned" data-slick-index="-4" aria-hidden="true" tabindex="-1">' +
										'<img class="blur-up lazyload" src="' + v + '" alt="" />' +
									'</a>';

			$('#gallery').append(template);

			$('.lightboximages').append('<a href="' + v + '" data-size="1462x2048"></a>');
		});

		$.each(details.Images, function (k, v) {
			var template = '<a data-image="' + v + '" data-zoom-image="' + v + '" class="slick-slide slick-cloned" data-slick-index="-4" aria-hidden="true" tabindex="-1">' +
										'<img class="blur-up lazyload" src="' + v + '" alt="" />' +
									'</a>';

			$('#gallery').append(template);

			$('.lightboximages').append('<a href="' + v + '" data-size="1462x2048"></a>');
		});
	}

	$('.product-dec-slider-2').slick({
		//infinite: true,
		slidesToShow: 5,
		vertical: true,
		slidesToScroll: 1,
		centerPadding: '60px'
	});

	$(".zoompro").elevateZoom({
		gallery: "gallery",
		galleryActiveClass: "active",
		zoomWindowWidth: 300,
		zoomWindowHeight: 100,
		scrollZoom: false,
		zoomType: "inner",
		cursor: "crosshair",

		//zoomType: "lens",
		//lensShape: "round",
		//lensSize: 200
	});
}

function BindProductAttributes(details) {

	$('#productAttributes').html('');
	$.each(details.Attributes, function (k, v) {
		if (v.Options && v.Options.length > 0) {
			var template = '<div class="swatch clearfix swatch-' + k + ' option' + k + '" data-option-index="0" id="' + v.ID + '">';
			template += '					<div class="product-form__item">';
			template += '						<label class="header">';
			template += '							' + v.Name + ':';
			template += '							<span class="slVariant"></span>';
			template += '						</label>';
			template += '					</div>';
			template += '				</div>';

			$('#productAttributes').append(template);

			$.each(v.Options, function (i, j) {
				if (j && j.includes(',')) {
					var option = j.split(',');
					if (v.Name.toUpperCase() == "COLOR") {
						var optionTemplate = '<div data-value="' + option[1] + '" class="swatch-element color ' + option[1] + ' available">' +
											'<input class="swatchInput" product-attribute-id="' + option[0] + '" id="swatch-' + k + '-' + option[0] + '" type="radio" name="option-' + k + '" value="' + option[1] + '"><label class="swatchLbl color small rounded" for="swatch-' + k + '-' + option[0] + '" style="background-color:' + option[1] + ';" title="' + option[1] + '"></label>' +
										'</div>';

						$('.swatch-' + k + '.option' + k).append(optionTemplate);
					} else {
						var optionTemplate = '<div data-value="' + option[1] + '" class="swatch-element ' + option[1] + ' available">' +
											'	<input class="swatchInput" product-attribute-id="' + option[0] + '" id="swatch-' + k + '-' + option[0] + '" type="radio" name="option-' + k + '" value="' + option[1] + '">' +
											'	<label class="swatchLbl small flat" for="swatch-' + k + '-' + option[0] + '" title="' + option[1] + '">' + option[1] + '</label>' +
											'</div>';

						$('.swatch-' + k + '.option' + k).append(optionTemplate);
					}
				}
			});
		}
	});
}

function BindVariableProduct(variation) {
	console.log(variation);
	$('.variant-sku').text(variation.SKU);
	Product.ProductVaraiationID = variation.ID;

	if (variation.IsSaleAvailable == true) {
		$('#ComparePrice-product-template .money').text('AED ' + variation.RegularPrice);
		$('#ProductPrice-product-template .money').text('AED ' + variation.SalePrice);
		var savedAmount = variation.RegularPrice - variation.SalePrice;
		$('#SaveAmount-product-template .money').text('AED ' + savedAmount);
		$('.discount-badge .off').html('(<span>' + Math.round((variation.RegularPrice * 100) / savedAmount) + '</span>%)');

		Product.UnitPrice = variation.SalePrice;
		$('.product-labels').html('<span class="lbl on-sale">Sale</span>');

		$('#ComparePrice-product-template').show();
		$('.discount-badge').show();
	} else {
		$('#ComparePrice-product-template').hide();
		$('.discount-badge').hide();
		$('#ProductPrice-product-template .money').text('AED ' + variation.RegularPrice);

		Product.UnitPrice = variation.RegularPrice;
		$('.product-labels').html('');
	}

	if (variation.IsManageStock == true) {

		$('#quantity_message .items').text(variation.Stock);
		$('#quantity_message').show();

		if (variation.Stock > 0) {
			$('.instock').show();
			$('.outstock').hide();

			$('#btnAddToCart').prop('disabled', false);
			$('#btnBuyNow').prop('disabled', false);

		} else {
			$('.instock').hide();
			$('.outstock').show();

			$('#btnAddToCart').prop('disabled', true);
			$('#btnBuyNow').prop('disabled', true);

			$('#quantity_message').hide();
		}
	} else {
		$('#quantity_message').hide();
		if (variation.StockStatus == 1) {
			$('.instock').show();
			$('.outstock').hide();

			$('#btnAddToCart').prop('disabled', false);
			$('#btnBuyNow').prop('disabled', false);

		} else {
			$('.instock').hide();
			$('.outstock').show();

			$('#btnAddToCart').prop('disabled', true);
			$('#btnBuyNow').prop('disabled', true);
		}
	}

	//$('.swatch').each(function () { $(this).find('input:first').trigger('click') })
}

function BindSimpleProduct(details) {
	$('.variant-sku').text(details.SKU);
	Product.ProductVaraiationID = null;
	if (details.IsSaleAvailable == true) {
		$('#ComparePrice-product-template .money').text('AED ' + details.RegularPrice);
		$('#ProductPrice-product-template .money').text('AED ' + details.SalePrice);
		var savedAmount = details.RegularPrice - details.SalePrice;
		$('#SaveAmount-product-template .money').text('AED ' + savedAmount);
		$('.discount-badge .off').html('(<span>' + Math.round((details.RegularPrice * 100) / savedAmount) + '</span>%)');

		Product.UnitPrice = details.SalePrice;
		$('.product-labels').html('<span class="lbl on-sale">Sale</span>');
	} else {
		$('#ComparePrice-product-template').remove();
		$('.discount-badge').remove();
		$('#ProductPrice-product-template .money').text('AED ' + details.RegularPrice);

		Product.UnitPrice = details.RegularPrice;
	}

	if (details.IsManageStock == true) {

		$('#quantity_message .items').text(details.Stock);
		$('#quantity_message').show();

		if (details.Stock > 0) {
			$('.instock').show();
			$('.outstock').hide();

			$('#btnAddToCart').prop('disabled', false);
			$('#btnBuyNow').prop('disabled', false);

		} else {
			$('.instock').hide();
			$('.outstock').show();

			$('#btnAddToCart').prop('disabled', true);
			$('#btnBuyNow').prop('disabled', true);

			$('#quantity_message').hide();
		}
	} else {
		$('#quantity_message').hide();
		if (details.StockStatus == 1) {
			$('.instock').show();
			$('.outstock').hide();

			$('#btnAddToCart').prop('disabled', false);
			$('#btnBuyNow').prop('disabled', false);

		} else {
			$('.instock').hide();
			$('.outstock').show();

			$('#btnAddToCart').prop('disabled', true);
			$('#btnBuyNow').prop('disabled', true);
		}
	}

}

function BindProductRating(data) {
	if (data && data.length > 0) {
		$('.spr-reviews').html('');
		$.each(data, function (k, v) {
			var template = '<div class="spr-review">';
			template += '		<div class="spr-review-header">';
			template += '			<span class="spr-starratings spr-review-header-starratings">';
			template += '				<span class="reviewLink rating" data="' + v.rating + '">';
			//template += '					<i class="fa fa-star"></i>';
			//template += '					<i class="font-13 fa fa-star"></i>';
			//template += '					<i class="font-13 fa fa-star"></i>';
			//template += '					<i class="font-13 fa fa-star"></i>';
			//template += '					<i class="font-13 fa fa-star"></i>';
			template += '				</span>';
			template += '			</span>';
			template += '			<h3 class="spr-review-header-title">' + v.customer + '</h3>';
			template += '			<span class="spr-review-header-byline"><strong></strong> on <strong>' + v.date + '</strong></span>';
			template += '		</div>';
			template += '		<div class="spr-review-content">';
			template += '			<p class="spr-review-content-body">' + v.remarks + '</p>';
			template += '			<section id="photos">'
			$.each(v.images, function (ind, img) {

				template += '<div class="photo quick-view-popup quick-view" data-toggle="modal" data-target="#content_quickview">';
				template += '	<img src="' + img + '" />';
				template += '</div>';

				//template += '				<a href="javascript:void(0)" title="Quick View" class="quick-view-popup quick-view" data-toggle="modal" data-target="#content_quickview">';
				//template += '					<i class="icon anm anm-search-plus-r"></i>';
				//template += '				</a>';

			});
			template += '			</section>'
			template += '		</div>';
			template += '</div>';

			$('.spr-reviews').append(template);
		});

		$('.rating').each(function (k, v) {
			var rating = parseFloat($(v).attr('data'));
			$(v).addClass('.fa-2x');
			addScore(rating * 20, $(v));
		});

		if ($('.spr-reviews').html().length == 0) {
			$('.spr-reviews').html('<div class="alert alert-dark">This product has no ratings.</div>');
		}

		var rating = parseFloat(data.sum("rating") / data.length);
		$('.product-review').addClass('.fa-2x');
		addScore(rating * 20, $('.product-review'));
		$('#overAllRating').html(rating);
		//
		//$('.product-review').find('i:lt(' + + ')').addClass("fa-star").removeClass("fa-star-o");
		$('.spr-summary-actions-togglereviews').html('على أساس ' + data.length + ' الاستعراضات');

		$('.spr-badge-caption').html(data.length + ' استعراض');
	} else {
		addScore(0 * 20, $('.product-review'));

		$('#overAllRating').html('0');
		$('.spr-summary-actions-togglereviews').html('0 reviews');

		$('.spr-badge-caption').html('0 reviews');


		$('.spr-reviews').html('<div class="alert alert-dark text-center">هذا المنتج ليس له تقييمات</div>');
	}
}

function callback(response) {
	if (response.success) {
		$('#message').html('<span class="fa fa-check"></span> ' + response.message)
		$('#message').slideDown();

		wishlist.push({ ID: response.wishId, ProductID: id });
		localStorage.setItem("wishlist", JSON.stringify(wishlist));

		$(element).html('<i class="anm anm-heart icon icon icon-light" style="color:#f54337;"></i>');
		$(element).attr('onclick', 'DeleteProductToWishlist(this,' + response.wishId + ',callback)');

	} else {
		$('#message').html('<span class="fa fa-warning"></span> ' + response.message)
		$('#message').slideDown();
	}

	setTimeout(function () {
		$('#message').slideUp();
	}, 3000);
}

function InputQuantity(element) {
	var newVal = Number($(element).val());

	if (!newVal) {
		newVal = 1;
	}
	if (newVal <= 0) {
		newVal = 1;
	}
	$(element).val(newVal);

	Product.Quantity = Number(newVal);
}

function GetURLParameter() {
	return $("#ProductID").val();
}