const API_ENDPOINT = "https://socialbazar.co/";
var pg = 1;
var PageSize = 24;
var isPageRendered = false;
var totalPages;
var wishlist;
var filter = {
	search: null,
	categoryID: null,
	vendorID: null,
	brandID: null,
	minPrice: null,
	maxPrice: null,
	attributes: [],
	pageNumber: 1,
	sortBy: 1
}

$(document).ready(function () {
	var wish = localStorage.getItem("wishlist");
	if (wish) {
		wishlist = JSON.parse(wish);
	}

	$('#CarsContainers').empty();
	GetWishlist();

	$('#load-more').click(function () {
		//if (pg < totalPages) {
		pg++;
		$('#load-more').hide();
		$(".filter-loader").show();
		GetFilterValues();
		GetCars();
		//}
	});

});

function DeleteWish(element, id) {


	$('#message').html('إزالة الرغبة ...  <span class="fa fa-hour-watch"></span>')
	$('#message').slideDown();
	var wish = localStorage.getItem("wishlist");
	if (wish) {
		wishlist = JSON.parse(wish);
	}
	wish = wishlist.find(function (obj) {
		return obj.ID == id;
	});
	if (wish) {
		$.ajax({
			type: 'DELETE',
			url: '/Wishlist/' + id,
			contentType: "application/json",
			success: function (response) {
				if (response.success) {

					wishlist = wishlist.filter(function (obj) {
						return obj.ID !== id;
					});

					localStorage.setItem("wishlist", JSON.stringify(wishlist));

					$(element).closest('.item').slideUp(function () {
						$(this).remove();

						$('#message').html(getTranslatedString(response.message) + '<span class="fa fa-check"></span>');
						$('#message').slideDown();
					});
				} else {
					$('#message').html(getTranslatedString(response.message) + '<span class="fa fa-warning"></span>');
					$('#message').slideDown();
				}

				setTimeout(function () {
					$('#message').slideUp();
				}, 3000);
			}
		});
	} else {
		$(element).closest('.item').slideUp(function () {
			$(this).remove();

			$('#message').html('تمت إزالة المنتج من قائمة الرغبات الخاصة بك!  <span class="fa fa-check"></span>')
			$('#message').slideDown();

			setTimeout(function () {
				$('#message').slideUp();
			}, 3000);
		});
	}
}

function GetWishlist() {

	$.ajax({
		type: 'POST',
		url: '/' + lang + '/customer/wishlist',
		contentType: "application/json",
		data: JSON.stringify(filter),
		success: function (response) {
			if (response.success) {
				BindGridWislist(response.data);
			}
		}
	});
}

function BindGridWislist(data) {

	$.each(data, function (k, v) {
		var template = '<div class="col-6 col-sm-6 col-md-4 col-lg-2 item" id=' + v.ID + '>';
		template += '		<!-- start car image -->';
		template += '		<div class="car-image">';
		template += '			<!-- start car image -->';
		template += '			<a href="/' + lang + '/car/' + v.Slug + '" class="grid-view-item__link">';
		template += '				<!-- image -->';
		template += '				<img class="primary blur-up lazyload" data-src="' + v.Thumbnail + '" src="' + v.Thumbnail + '" alt="image" title="car">';
		template += '				<!-- End image -->';
		template += '				<!-- Hover image -->';
		template += '				<img class="hover blur-up lazyload" data-src="' + v.Thumbnail + '" src="' + v.Thumbnail + '" alt="image" title="car">';
		template += '				<!-- End hover image -->';
		//template += '				<!-- Variant Image-->';
		//template += '				<img class="grid-view-item__image hover variantImg" src="../assets/images/Category/abaya3.jpg" alt="image" title="car">';
		//template += '				<!-- Variant Image-->';
		template += '				<!-- car label -->';
		template += '				<div class="car-labels rounded">';
		if (v.IsSaleAvailable == true) {
			template += '					<span class="lbl on-sale">Sale</span>';
		}
		template += '				</div>';
		template += '				<!-- End car label -->';
		template += '			</a>';
		template += '			<!-- end car image -->';
		//template += '			<!-- countdown start -->';
		//template += '			<div class="saleTime desktop" data-countdown="2022/03/01"></div>';
		//template += '			<!-- countdown end -->';
		//template += '			<!-- Start car button -->';
		//template += '			<form class="variants add" action="javascript:;" method="post">';
		//template += '				<button class="btn btn-addto-cart" type="button" tabindex="0">Add To Cart</button>';
		//template += '			</form>';
		template += '			<div class="button-set">';
		//template += '				<a href="javascript:void(0)" title="Quick View" class="quick-view-popup quick-view" data-toggle="modal" data-target="#content_quickview">';
		//template += '					<i class="icon anm anm-search-plus-r"></i>';
		//template += '				</a>';

		template += '				<div class="wishlist-btn">';
		template += '					<a class="add-to-wishlist wishlist bg-transparent text-dark" href="javascript:;" onclick="DeleteWish(this,' + v.WishID + ')">';
		template += '						<i class="anm anm-heart icon icon-2x icon-light" style="color:#f54337;"></i>';
		template += '					</a>';
		template += '				</div>';
		//template += '				<div class="compare-btn">';
		//template += '					<a class="compare add-to-compare" href="#" title="Add to Compare">';
		//template += '						<i class="icon anm anm-random-r"></i>';
		//template += '					</a>';
		//template += '				</div>';
		template += '			</div>';
		//template += '			<!-- end car button -->';
		template += '		</div>';
		template += '		<!-- end car image -->';
		template += '		<!--start car details -->';
		template += '		<div class="car-details text-center">';
		template += '			<!-- car name -->';
		template += '			<div class="car-name">';
		template += '				<a href="/' + lang + '/car/' + v.Slug + '">' + v.Title + '</a>';
		template += '			</div>';
		template += '			<!-- End car name -->';
		template += '			<!-- car price -->';
		template += '			<div class="car-price">';
		if (v.Type == '1' || v.Type == 'Simple') {
			if (v.IsSaleAvailable == true) {
				template += '				<span class="old-price">AED ' + v.RegularPrice + '</span>';
				template += '				<span class="price">AED ' + v.SalePrice + '</span>';
			} else {
				template += '				<span class="price">AED ' + v.RegularPrice + '</span>';
			}
		} else {
			if (v.IsSaleAvailable == true) {
				template += '				<span class="old-price">AED ' + v.MinRegularPrice + ' - AED ' + v.MaxRegularPrice + '</span>';
				template += '				<span class="price">AED ' + (v.MinSalePrice ? v.MinSalePrice : v.MinRegularPrice) + ' - AED ' + (v.MaxSalePrice ? v.MaxSalePrice : v.MaxRegularPrice) + '</span>';
			} else {
				template += '				<span class="price">AED ' + v.MinRegularPrice + ' - AED ' + v.MaxRegularPrice + '</span>';
			}
		}
		template += '			</div>';
		template += '			<!-- End car price -->';
		template += '			<!-- Color Variant -->';
		template += '			<!-- End Variant -->';
		template += '			<div class="car-review car-review-new" data="' + (v.Rating ? v.Rating : 0) + '" NoOfRatings="' + v.NoOfRatings + '">';
		template += '			</div>';
		template += '		</div>';
		template += '		<!-- End car details -->';
		template += '	</div>';

		$('#CarsContainers').append(template);
	});

	FormatPrices();

	$('.car-review-new').each(function (k, v) {
		var rating = parseFloat($(v).attr('data'));
		$(v).removeClass("car-review-new").addClass('fa-2x');
		addScore(rating * 20, $(v));
	});

	if (data && data.length >= PageSize) {
		$("#load-more").fadeIn();
	} else {
		$("#load-more").fadeOut();
	}

	if ($('#CarsContainers').html().length == 0) {
		$("html, body").animate({ scrollTop: 0 }, 1000);
		$('#CarsContainers').html('<div class="alert alert-dark" title="No Records Found!">لا توجد سجلات!</div>');
	}
}

function RenderPagination(totalRecord, filteredRecord) {

	totalPages = Math.ceil(filteredRecord / PageSize);

	$(".filter-loader").hide();
	if (pg >= totalPages) {
		$("#load-more").fadeOut();
	} else {
		$("#load-more").fadeIn();
	}

}

function getUrlParameter(name) {
	name = name.replace(/[\[]/, '\\[').replace(/[\]]/, '\\]');
	var regex = new RegExp('[\\?&]' + name + '=([^&#]*)');
	var results = regex.exec(location.search);
	return results === null ? '' : decodeURIComponent(results[1].replace(/\+/g, ' '));
};

function UpdateQueryString(key, value, url) {
	if (!url) url = window.location.href;
	var re = new RegExp("([?&])" + key + "=.*?(&|#|$)(.*)", "gi"),
        hash;

	if (re.test(url)) {
		if (typeof value !== 'undefined' && value !== null) {
			return url.replace(re, '$1' + key + "=" + value + '$2$3');
		}
		else {
			hash = url.split('#');
			url = hash[0].replace(re, '$1$3').replace(/(&|\?)$/, '');
			if (typeof hash[1] !== 'undefined' && hash[1] !== null) {
				url += '#' + hash[1];
			}
			return url;
		}
	}
	else {
		if (typeof value !== 'undefined' && value !== null) {
			var separator = url.indexOf('?') !== -1 ? '&' : '?';
			hash = url.split('#');
			url = hash[0] + separator + key + '=' + value;
			if (typeof hash[1] !== 'undefined' && hash[1] !== null) {
				url += '#' + hash[1];
			}
			return url;
		}
		else {
			return url;
		}
	}
}


function getTranslatedString(message) {

	switch (message) {
		case "Car removed from your wishlist!":
			message = "تمت إزالة المنتج من قائمة الرغبات الخاصة بك!";
			break;
		case "Oops! Something went wrong. Please try later.":
			message = "وجه الفتاة! حدث خطأ ما ، يرجى المحاولة لاحقًا";
			break;
		default:
			message = message;
	}

	return message;
}