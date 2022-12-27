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
	ProductVaraiationSoldIndividual: null,
	Attributes: [],
	IsInStock: true,
	IsSoldIndividually: false
};

var details;
var selectedAttributes = [];
var catFlag = true;
var wishlist;

$(document).ready(function () {
	var wish = localStorage.getItem("wishlist");
	if (wish) {
		wishlist = JSON.parse(wish);
	}

	var comp = localStorage.getItem("productCompareArray");
	if (comp) {
		var compare = JSON.parse(comp);
		if (compare.length > 0) {

			if (compare.includes(parseFloat(GetURLParameter()))) {
				$('#CompareBadge').attr('data-type', '0').attr('title', 'Remove from Compare').html('<i class="anm anm-random-r icon icon-2x" aria-hidden="true" style="color: #f54337;"></i>');
			}
		}
	}

	$('body').removeClass('template-index').removeClass('home2-default').addClass('template-product').addClass('belle').addClass('template-product-right-thumb');
	$('#btnAddToCart').prop('disabled', true);

	ReloadEvents();

	function ReloadEvents() {
		$.ajax({
			type: 'GET',
			url: '/en/products/' + GetURLParameter(),
			success: function (response) {
				if (response.success) {
					console.log(response.data);
					details = response.data;
					Product.ProductID = details.ID;
					Product.Title = details.Title;
					Product.Slug = details.ProductSlug;
					Product.Thumbnail = details.Thumbnail;
					Product.VendorID = details.Vendor.ID;
					Product.IsSoldIndividually = details.IsSoldIndividually;

					$('.breadcrumbs span:last').text(details.Title);
					$('.product-single__title').text(details.Title);
					$('.product-single__description').html(details.ShortDescription);
					$('.product-description').html(details.LongDescription);

					/* Vendor Details Binding */
					$('.vendor-details').find('a').attr('href', '/store/' + details.Vendor.Slug);
					$('.vendor-details').find('img').attr('src', details.Vendor.Logo);
					$('.vendor-details').find('h3').text(details.Vendor.Name);

					/* QR Code Details Binding */

					$('.qr-details').find('a').attr('href', '/product/' + details.ProductSlug);
					$('.qr-details').find('.sub-heading').html('<img class="" src="https://api.qrserver.com/v1/create-qr-code/?data=' + window.location.href + ')&amp;size=120x120&amp;color=110044&amp;ecc=M" alt=' + details.ProductSlug + ' title=' + details.ProductSlug + '/> ');


					BindProductAttributes(details);
					if (catFlag == true) {

						$.each(details.categories, function (k, v) {
							$('.product-cat').append('<a class="mr-2" href="/product-category/' + v + '" title="' + v + '" target="_blank">' + v + '</a>');
						});
						catFlag = false;
					}

					if (details.Type == '1' || details.Type == 'Simple') {

						BindProductImages(details);
						BindSimpleProduct(details);
						$('.swatch').each(function () { $(this).find('input:first').trigger('click') });
						//if (details.Stock) {
						//    $('#btnAddToCart').prop('disabled', false);
						//    $('#btnBuyNow').prop('disabled', false);
						//}

						if (Product.IsSoldIndividually) {
							if (!Product.ProductVaraiationID) {
								$('.numinput').prop('disabled', true).val(1);
								$('.one-item-text').show();
							}
							CheckIsSoldIndividually(Product.ProductID, 0);
						}

					} else {

						$('#productAttributes .swatch input').change(function () {



							selectedAttributes = [];

							$('#productAttributes .swatch[variation-usage=true] input:checked').each(function () {
								selectedAttributes.push($(this).attr('product-attribute-id'));
							});

							var variation = details.Variations.find(function (obj) {
								return isEqual(obj.Attributes, selectedAttributes);
							});
							if (variation) {
								$('.product-single__description').html(`<h3>${variation.Description}</h3>`);
								BindVariableProduct(variation);
								BindProductImages(details, variation);

								//if (variation.Stock) {
								//    $('#btnAddToCart').prop('disabled', false);
								//    $('#btnBuyNow').prop('disabled', false);
								//} else {
								//    if (variation.IsManageStock == true) {
								//        $('#btnAddToCart').prop('disabled', true);
								//        $('#btnBuyNow').prop('disabled', true);
								//    }
								//}

								if (variation.SoldIndividually) {
									$('.numinput').prop('disabled', true).val(1);
									$('.one-item-text').show();
									CheckIsSoldIndividually(Product.ProductID, variation.ID);
								}
								else {
									$('.numinput').prop('disabled', false);
									$('.one-item-text').hide();
								}


							} else {
								BindProductImages(details);
								$('.variant-sku').text('-');

								$('#ComparePrice-product-template').hide();
								$('.discount-badge').hide();
								$('#ProductPrice-product-template .money').text('AED 0.00');

								$('#quantity_message').hide();
								$('.instock').hide();
								$('.outstock').show();
								$('#btnAddToCart').prop('disabled', true);
								$('#btnBuyNow').prop('disabled', true);

								$('[data-countdown]').each(function () {
									var $this = $(this);
									$this.countdown('remove');
								});

								$('#timer').html(``);
							}
						});

						$('.swatch').each(function () { $(this).find('input:first').prop('checked', true) });
						//$('.swatch').each(function () { $(this).find('input:first').trigger('click') });
						$('.swatch input:first').trigger('change');
					}

					//if (details.IsFeatured) {
					//    $('.product-labels').html('<span class="lbl pr-label1">Featured</span>');
					//}
					if (typeof session != "undefined" && session == true && wishlist.filter(function (obj) { return obj.ProductID == details.ID }).length > 0) {

						$('#WishlistBadge').html('<i class="anm anm-heart icon icon icon-light icon-2x" style="color:#f54337;"></i>');
						$('#WishlistBadge').attr('onclick', 'DeleteProductToWishlist(this,' + wishlist.find(function (obj) { return obj.ProductID == details.ID }).ID + ',callback)');
					}

				}
				else {
				}
			}
		});
	}

	/*Fetching and Binding of Product Rating */
	$.ajax({
		type: 'GET',
		url: '/products/' + GetURLParameter() + '/ratings',
		success: function (response) {
			BindProductRating(response.data);
		}
	});

	/*Fetching and Binding of Related Products */
	$.ajax({
		type: 'GET',
		url: '/products/' + GetURLParameter() + '/relatedproducts',
		success: function (response) {
			BindRelatedProducts(response.products);
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


		if (!Product.ProductVaraiationID && Product.IsSoldIndividually) {
			$('.numinput').val(1);
		}
		else if (Product.ProductVaraiationID) {
			var variation;
			var Variations = details.Variations;
			if (Variations.length > 0) {
				variation = Variations.filter(function (obj) { return obj.ID == Product.ProductVaraiationID });
				if (variation.length > 0 && variation[0].SoldIndividually == true) {
					$('.numinput').val(1);
				}
				else {
					if (Product.Quantity > 0) {
						Product.Quantity -= 1;
					}
					var ele = $('.numinput');
					inputQty(ele);
				}
			}
		}
		else {
			if (Product.Quantity > 0) {
				Product.Quantity -= 1;
			}
			var ele = $('.numinput');
			inputQty(ele);
		}
	});

	$('.product-form__item--quantity .qtyBtn.plus').on('click', function () {
		if (!Product.ProductVaraiationID && Product.IsSoldIndividually) {
			$('.numinput').val(1);
		}
		else if (Product.ProductVaraiationID) {
			var variation;
			var Variations = details.Variations;
			if (Variations.length > 0) {
				variation = Variations.filter(function (obj) { return obj.ID == Product.ProductVaraiationID });
				if (variation.length > 0 && variation[0].SoldIndividually == true) {
					$('.numinput').val(1);
				}
				else {
					var ele = $('.numinput');
					inputQty(ele);
				}
			}
		}
		else {
			var ele = $('.numinput');
			inputQty(ele);
			//Product.Quantity += 1;
		}
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

		Product.CustomNote = $('#CustomNote').val();
		Product.Price = Product.Quantity * Product.UnitPrice

		var cart = localStorage.getItem("cart");
		if (Product.Price) {

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
				$('.numinput').val("1");
				ReloadEvents();
				$('#message').html('<span class="fa fa-check"></span> <span class="message">Product added into cart successfully!</span>');
				alertMsg();
				//} else {
				//	$('#message').html('<span class="fa fa-check"></span> only one Product nature allowed per order!')
				//	$('#message').fadeIn();

				//	setTimeout(function () {
				//		$('#message').fadeOut();
				//	}, 3000);
				//}
			} else {
				AddToCart(Product);
				$('.numinput').val("1");
				ReloadEvents();
				$('#message').html('<span class="fa fa-check"></span> <span class="message">Product added into cart successfully!</span>')
				alertMsg();
			}
		}
		else {
			$('#message').html('<span class="fa fa-exclamation-triangle"></span> <span class="message">Product have 0 quantity!</span>')
			alertMsg();
		}
		Product.Quantity = 1;
		CheckIsSoldIndividually(Product.ProductID, (Product.ProductVaraiationID != null ? Product.ProductVaraiationID : 0));
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

		$('#gallery').append('<a data-image="' + details.Thumbnail + '" data-zoom-image="' + details.Thumbnail + '" class="slick-slide slick-cloned" data-slick-index="-4" aria-hidden="true" tabindex="-1">' +
			'<img class="blur-up lazyload img-lazyload" src="' + details.Thumbnail + '" alt="" />' +
			'</a>');

		$('.lightboximages').append('<a href="' + details.Thumbnail + '" data-size="1462x2048"></a>');

		$.each(details.Images, function (k, v) {
			var template = '<a data-image="' + v + '" data-zoom-image="' + v + '" class="slick-slide slick-cloned" aria-hidden="true" tabindex="-1">' +
				'<img class="blur-up lazyload img-lazyload" src="' + v + '" alt="" />' +
				'</a>';

			$('#gallery').append(template);

			$('.lightboximages').append('<a href="' + v + '" data-size="1462x2048"></a>');
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
			//lensSize: 300
		});

		setTimeout(function () {
			$('.product-dec-slider-2').slick({
				//infinite: true,
				slidesToShow: 4,
				vertical: true,
				slidesToScroll: 1,
				centerPadding: '60px',
				adaptiveHeight: true,
			});

			$('#gallery a:first').click();

		}, 100);
	}
	else {
		$('#Thumbnail').attr('data-zoom-image', varaint.Thumbnail);
		$('#Thumbnail').attr('src', varaint.Thumbnail);

		$('#gallery').append('<a data-image="' + varaint.Thumbnail + '" data-zoom-image="' + varaint.Thumbnail + '" class="slick-slide slick-cloned" aria-hidden="true" tabindex="-1">' +
			'<img class="blur-up lazyload img-lazyload" src="' + varaint.Thumbnail + '" alt="" />' +
			'</a>');

		$('.lightboximages').append('<a href="' + varaint.Thumbnail + '" data-size="1462x2048"></a>');


		var flag = true;
		$.each(varaint.Images, function (k, v) {

			if (flag) {
				$('#Thumbnail').attr('data-zoom-image', v);
				$('#Thumbnail').attr('src', v);
				flag = false;
			}

			var template = '<a data-image="' + v + '" data-zoom-image="' + v + '" class="slick-slide slick-cloned" aria-hidden="true" tabindex="-1">' +
				'<img class="blur-up lazyload img-lazyload" src="' + v + '" alt="" />' +
				'</a>';

			$('#gallery').append(template);

			$('.lightboximages').append('<a href="' + v + '" data-size="1462x2048"></a>');
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
			//lensSize: 300
		});

		setTimeout(function () {
			$('.product-dec-slider-2').slick({
				//infinite: true,
				slidesToShow: 4,
				vertical: true,
				slidesToScroll: 1,
				centerPadding: '60px',
				adaptiveHeight: true,
			});

			$('#gallery a:first').click();

		}, 100);
	}

	setTimeout(function () { OnErrorImage(); }, 3000);

}

function BindProductAttributes(details) {

	$('#productAttributes').html('');
	$.each(details.Attributes, function (k, v) {
		if (v.Options && v.Options.length > 0) {
			var template = '<div class="swatch clearfix swatch-' + k + ' option' + k + '" data-option-index="0" id="' + v.ID + '" variation-usage="' + v.VariationUsage + '">';
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
	Product.ProductVaraiationSoldIndividual = variation.SoldIndividually;

	if (variation.IsSaleAvailable == true) {
		$('#ComparePrice-product-template .money').text('AED ' + variation.RegularPrice);
		FormatPrice($('#ComparePrice-product-template .money'));

		$('#ProductPrice-product-template .money').text('AED ' + variation.SalePrice);
		FormatPrice($('#ProductPrice-product-template .money'));

		var savedAmount = variation.RegularPrice - variation.SalePrice;
		$('#SaveAmount-product-template .money').text('AED ' + savedAmount);
		FormatPrice($('#SaveAmount-product-template .money'));

		//$('.discount-badge .off').html('(<span>' + Math.round((variation.RegularPrice * 100) / savedAmount) + '</span>%)');
		$('.discount-badge .off').html('(<span>' + numeral(Math.round((savedAmount / variation.RegularPrice) * 100)).format('0,0.00') + '</span>%)');

		Product.UnitPrice = variation.SalePrice;
		$('.product-labels').html('<span class="lbl on-sale">Sale</span>');

		$('#ComparePrice-product-template').show();
		$('.discount-badge').show();

		if (variation.SalePriceTo) {
			$('[data-countdown]').each(function () {
				var $this = $(this);
				$this.countdown('remove');
			});

			$('#timer').empty();
			$('#timer').html(`<div class="saleTime desktop"  style="position:initial !important"></div>`);
			$('.saleTime').attr('data-countdown', variation.SalePriceTo);

			$('[data-countdown]').each(function () {

				var $this = $(this),
					finalDate = $(this).data('countdown');
				$this.countdown(finalDate, function (event) {
					$this.html(event.strftime('<span class="ht-count days"><span class="count-inner"><span class="time-count">%-D</span> <span>Days</span></span></span> <span class="ht-count hour"><span class="count-inner"><span class="time-count">%-H</span> <span>HR</span></span></span> <span class="ht-count minutes"><span class="count-inner"><span class="time-count">%M</span> <span>Min</span></span></span> <span class="ht-count second"><span class="count-inner"><span class="time-count">%S</span> <span>Sc</span></span></span>'));
				});
			});
		} else {
			$('[data-countdown]').each(function () {
				var $this = $(this);
				$this.countdown('remove');
			});

			$('#timer').html(``);
		}
	} else {
		$('#ComparePrice-product-template').hide();
		$('.discount-badge').hide();
		$('#ProductPrice-product-template .money').text('AED ' + variation.RegularPrice);
		FormatPrice($('#ProductPrice-product-template .money'));

		Product.UnitPrice = variation.RegularPrice;
		if (details.IsFeatured) {
			$('.product-labels').html('<span class="lbl pr-label1">Featured</span>');
		}
		else {
			$('.product-labels').html('');
		}


	}

	if (variation.IsManageStock == true) {

		$('#quantity_message .items').text(variation.Stock);
		$('#quantity_message').show();

		if (variation.Stock > 0) {
			$('.instock').show();
			$('.outstock').hide();

			var CheckCart = ShoppingCart.filter(function (obj) {
				return obj.ProductID == Product.ProductID
					&& obj.ProductVaraiationID == Product.ProductVaraiationID;
			});

			if (CheckCart && CheckCart.length > 0) {
				if (CheckCart.sum("Quantity")) {
					$('#cartStock').html(CheckCart.sum("Quantity") + ' items in Cart').show();
				}
			}
			else {
				$('#cartStock').html('0 items in Cart').show();
			}

			if (variation.Stock) {
				$('.numinput').val("1");
				$('.numinput').attr("max", variation.Stock);
				if (CheckCart && CheckCart.length > 0) {
					if (CheckCart.sum("Quantity") >= variation.Stock) {
						btnDisable(true);
					}
					else {
						btnDisable(false);
					}
				}
				else {
					btnDisable(false);
				}
			}
			else {
				$('.numinput').val("1");
				$('.numinput').attr("max", 0);
				btnDisable(true);
			}

		} else {
			$('.instock').hide();
			$('.outstock').show();
			btnDisable(true);
			$('#quantity_message').hide();

			$('.numinput').attr("max", 0);
			$('.numinput').val("1");
		}
	} else {
		$('#quantity_message').hide();
		if (variation.StockStatus == 1) {
			$('.instock').show();
			$('.outstock').hide();
			btnDisable(false);
			$('.numinput').val("1");

		} else {
			$('.instock').hide();
			$('.outstock').show();
			btnDisable(true);
		}
	}

	//$('.swatch').each(function () { $(this).find('input:first').trigger('click') })
}

function BindSimpleProduct(details) {
	if (details.IsFeatured) {
		$('.product-labels').html('<span class="lbl pr-label1">Featured</span>');
	}
	$('.variant-sku').text(details.SKU);
	Product.ProductVaraiationID = null;
	if (details.IsSaleAvailable == true) {
		$('#ComparePrice-product-template .money').text('AED ' + details.RegularPrice);
		FormatPrice($('#ComparePrice-product-template .money'));

		$('#ProductPrice-product-template .money').text('AED ' + details.SalePrice);
		FormatPrice($('#ProductPrice-product-template .money'));

		var savedAmount = details.RegularPrice - details.SalePrice;
		$('#SaveAmount-product-template .money').text('AED ' + savedAmount);
		FormatPrice($('#SaveAmount-product-template .money'));

		//$('.discount-badge .off').html('(<span>' + Math.round((details.RegularPrice * 100) / savedAmount) + '</span>%)');
		$('.discount-badge .off').html('(<span>' + numeral(Math.round(((savedAmount / details.RegularPrice) * 100))).format('0,0.00') + '</span>%)');

		Product.UnitPrice = details.SalePrice;
		$('.product-labels').html('<span class="lbl on-sale">Sale</span>');

		if (details.SalePriceTo) {
			$('.saleTime').html(`<div class="saleTime desktop"  style="position:initial !important"></div>`);
			$('.saleTime').attr('data-countdown', details.SalePriceTo);

			$('[data-countdown]').each(function () {
				var $this = $(this),
					finalDate = $(this).data('countdown');
				$this.countdown(finalDate, function (event) {
					$this.html(event.strftime('<span class="ht-count days"><span class="count-inner"><span class="time-count">%-D</span> <span>Days</span></span></span> <span class="ht-count hour"><span class="count-inner"><span class="time-count">%-H</span> <span>HR</span></span></span> <span class="ht-count minutes"><span class="count-inner"><span class="time-count">%M</span> <span>Min</span></span></span> <span class="ht-count second"><span class="count-inner"><span class="time-count">%S</span> <span>Sc</span></span></span>'));
				});
			});
		} else {

		}
	} else {
		$('#ComparePrice-product-template').remove();
		$('.discount-badge').remove();
		$('#ProductPrice-product-template .money').text('AED ' + details.RegularPrice);
		FormatPrice($('#ProductPrice-product-template .money'));

		Product.UnitPrice = details.RegularPrice;
	}

	if (details.IsManageStock == true) {

		$('#quantity_message .items').text(details.Stock);
		$('#quantity_message').show();

		if (details.Stock > 0) {
			$('.instock').show();
			$('.outstock').hide();

			var CheckCart = ShoppingCart.filter(function (obj) {
				return obj.ProductID == Product.ProductID;
			});

			if (CheckCart && CheckCart.length > 0) {
				if (CheckCart.sum("Quantity")) {
					$('#cartStock').html(CheckCart.sum("Quantity") + ' items in Cart').show();
				}
			}
			else {
				$('#cartStock').html('0 items in Cart').show();
			}
			if (details.Stock) {
				$('.numinput').val("1");
				$('.numinput').attr("max", details.Stock);
				if (CheckCart && CheckCart.length > 0) {
					if (CheckCart.sum("Quantity") >= details.Stock) {
						btnDisable(true);
					}
					else {
						btnDisable(false);
					}
				}
				else {
					btnDisable(false);
				}
			}
			else {
				$('.numinput').val("1");
				$('.numinput').attr("max", 0);
				btnDisable(true);
			}

		} else {
			$('.instock').hide();
			$('.outstock').show();

			btnDisable(true);

			$('#quantity_message').hide();
			$('.numinput').attr("max", 0);
			$('.numinput').val("1");
		}
	} else {
		$('#quantity_message').hide();
		if (details.StockStatus == 1) {

			var CheckCart = ShoppingCart.find(function (obj) {
				return obj.ProductID == Product.ProductID;
			});
			if (CheckCart) {
				if (CheckCart.Quantity) {
					$('#cartStock').html(CheckCart.Quantity + ' items in Cart').show();
				}
			}
			else {
				$('#cartStock').html('0 items in Cart').show();
			}

			$('.instock').show();
			$('.outstock').hide();
			btnDisable(false);
			$('.numinput').val("1");

		} else {
			$('.instock').hide();
			$('.outstock').show();
			btnDisable(true);
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

		setTimeout(function () { OnErrorImage(); }, 3000);

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
		$('.spr-summary-actions-togglereviews').html('Based on ' + data.length + ' reviews');

		$('.spr-badge-caption').html(data.length + ' reviews');
	} else {
		addScore(0 * 20, $('.product-review'));

		$('#overAllRating').html('0');
		$('.spr-summary-actions-togglereviews').html('0 reviews');

		$('.spr-badge-caption').html('0 reviews');


		$('.spr-reviews').html('<div class="alert alert-dark">This product has no ratings.</div>');
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
	}, 3000
}

function InputQuantity(element) {

	var key = event.keyCode || event.charCode;
	if (key == 8) {
		$(element).val('');
		Product.Quantity = 1;
	}
	else {
		inputQty(element);
	}
}

function inputQty(element) {


	if ($(element).val()) {
		var newVal = Number(parseInt($(element).val()));
		var maxQty = Number($(element).attr("max"));
		if (!newVal) {
			newVal = 1;
		}
		if (newVal <= 0) {
			newVal = 1;
		}
		var CheckCart = ShoppingCart.find(function (obj) {
			return obj.ProductID == Product.ProductID
				&& obj.ProductVaraiationID == Product.ProductVaraiationID;
		});

		if (CheckCart) {
			if (CheckCart.Quantity) {
				//variation.Stock = variation.Stock - CheckCart.Quantity;
				if ((newVal + CheckCart.Quantity) < maxQty) {
					$(element).val(newVal);
					Product.Quantity = Number(newVal);
				}
				else if ((newVal + CheckCart.Quantity) >= maxQty) {
					newVal = Number(maxQty - CheckCart.Quantity);
					if (newVal) {
						$(element).val(Number(newVal));
						Product.Quantity = Number(newVal);
					}
					else {
						$(element).val(Number(1));
						Product.Quantity = Number(1);
					}
				}
				else {

					$(element).attr('disabled', true);
					Product.Quantity = Number(1);
					btnDisable(true);
				}
			}
		}
		else {
			if (Number($(element).attr("max")) > newVal) {
				$(element).val(newVal);
				Product.Quantity = Number(newVal);
			}
			else {
				$(element).val(Number($(element).attr("max")));
				Product.Quantity = Number($(element).attr("max"));
			}
		}
	}
	else {
		$(element).val('1');
		Product.Quantity = 1;
	}
}

function GetURLParameter() {
	return $("#ProductID").val();
}

function alertMsg() {
	$('#message').slideDown();
	setTimeout(function () { $('#message').slideUp(); }, 4000);
}

function btnDisable(btnFlag) {
	if (btnFlag == true) {
		$('#btnAddToCart').prop('disabled', true);
		$('#btnBuyNow').prop('disabled', true);
		$('.numinput').prop('disabled', true);
		//if (stockExceed) {

		//}
	}
	else {
		$('#btnAddToCart').prop('disabled', false);
		$('#btnBuyNow').prop('disabled', false);
		$('.numinput').prop('disabled', false);
	}
}

function CheckIsSoldIndividually(productId, variantId) {

	if (!Product.ProductVaraiationID) {
		if (ShoppingCart.filter(function (obj) { return obj.ProductID == productId }).length > 0) {
			$('.numinput').prop('disabled', true).val(1);
			$('#btnAddToCart').prop('disabled', true);
		}
		else if (Product.IsSoldIndividually) {
			$('.numinput').prop('readonly', true).val(1);
			$('#btnAddToCart').prop('disabled', false);
		}
		else {
			$('.numinput').prop('disabled', false);
			$('#btnAddToCart').prop('disabled', false);
		}
	}
	else {
		var variation;
		var Variations = details.Variations;
		if (Variations.length > 0) {
			variation = Variations.filter(function (obj) { return obj.ID == variantId });
		}
		if (ShoppingCart.filter(function (obj) { return obj.ProductID == productId && obj.ProductVaraiationID == variantId }).length > 0) {
			$('.numinput').prop('disabled', true).val(1);
			$('#btnAddToCart').prop('disabled', true);
		}
		else if (variation.length > 0 && variation[0].SoldIndividually == true) {
			$('.numinput').prop('readonly', true).val(1);
			$('#btnAddToCart').prop('disabled', false);
		}
		else {
			$('.numinput').prop('disabled', false);
			$('#btnAddToCart').prop('disabled', false);
		}
	}
}

function BindRelatedProducts(products) {

	if (products) {
		if (products.length > 0) {
			$('.related-products').show();
		}
		$('#related-products-main').empty()
		$.each(products, function (k, v) {

			var price = "";
			if (v.Type == '1' || v.Type == 'Simple') {
				if (v.IsSaleAvailable == true) {
					price += '				<p class="old-price">AED ' + v.RegularPrice + '</p>';
					price += '				<p class="price">AED ' + v.SalePrice + '</p>';
				} else {
					price += '				<p class="price">AED ' + v.RegularPrice + '</p>';
				}
			} else {
				if (v.IsSaleAvailable == true) {
					price += '				<p class="old-price">AED ' + v.MinRegularPrice + ' - AED ' + v.MaxRegularPrice + '</p>';
					price += '				<p class="price">AED ' + (v.MinSalePrice ? v.MinSalePrice : v.MinRegularPrice) + ' - AED ' + (v.MaxSalePrice ? v.MaxSalePrice : v.MaxRegularPrice) + '</p>';
				} else {
					price += '				<p class="price">AED ' + v.MinRegularPrice + ' - AED ' + v.MaxRegularPrice + '</p>';
				}
			}

			var label = "";
			if (v.IsSaleAvailable == true) {
				label += '				    <div class="product-labels rounded" style="left:10px !important;">';
				label += '					    <span class="lbl on-sale">Sale</span>';
				label += '				    </div>';
			}

			$('#related-products-main').append(`
					<div class="col-12 item">
                        <div class="row">
                            <div class="col-12 item">
                                <div class="product-image">
                                    <a href="/product/${v.Slug}">
                                        <img class="img-fit-slider img-lazyload" src="${v.Thumbnail}" alt="${v.Title}" title="${v.Title}" />
                                    </a>
                                    <div class="variants add slider">
        				                <button class="btn btn-addto-cart" data-type="1" onclick="ProductCompareArrayFunction(this, ${v.ID})" id="product-compare-${v.ID}" type="button" tabindex="0">Add To Compare</button>
        		                    </div>
                                </div>
                            </div>
                            <div class="col-12 item" style="background-color: white;">

                                <div class="product-details text-center">
                                    <div class="product-name mt-2">
                                        <a href="/product/${v.Slug}">${v.Title}</a>
                                    </div>

                                    <div class="product-price">
                                            ${price}
                                    </div>

                                    <div class="product-review product-review-related" data="${v.Rating ? v.Rating : 0}" NoOfRatings="${v.NoOfRatings}"></div>

                                </div>
                            </div>
                        </div>
                    </div>
            `);


		});

		setTimeout(function () { OnErrorImage(); }, 3000);

		$('.product-review-related').each(function (k, v) {
			var rating = parseFloat($(v).attr('data'));
			$(v).removeClass("product-review-related").addClass('fa-2x mt-2');
			addScore(rating * 20, $(v));
		});

		$('.logo-bar').slick({
			dots: false,
			infinite: true,
			slidesToShow: 6,
			slidesToScroll: 1,
			autoplay: true,
			autoplaySpeed: 5000,
			arrows: false,
			responsive: [
				{
					breakpoint: 1024,
					settings: {
						slidesToShow: 4,
						slidesToScroll: 1
					}
				},
				{
					breakpoint: 600,
					settings: {
						slidesToShow: 3,
						slidesToScroll: 1
					}
				},
				{
					breakpoint: 480,
					settings: {
						slidesToShow: 2,
						slidesToScroll: 1
					}
				}
			]
		});
	}

}

