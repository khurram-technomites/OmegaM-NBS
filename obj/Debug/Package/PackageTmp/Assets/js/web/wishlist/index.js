const API_ENDPOINT = "https://socialbazar.co/";
var pg = 1;
var PageSize = 24;
var wishlist;
var isPageRendered = false;
var totalPages;
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
	GetCars();

	$('#Status').change(function () {
		$("#orders-tbody").empty();
		pg = 1;
		GetFilterValues();
		GetCars();
	});

	$('#btnfilter').click(function () {
		$('#CarsContainers').empty();
		pg = 1;
		GetFilterValues();
		GetCars();
	});

	$('#SortBy').change(function () {
		$('#CarsContainers').empty();
		pg = 1;
		GetFilterValues();
		GetCars();
	});

	$('#load-more').click(function () {
		//if (pg < totalPages) {
		pg++;
		$('#load-more').hide();
		$(".filter-loader").show();
		GetFilterValues();
		GetCars();
		//}
	});

	$("#slider-range").slider({
		range: true,
		min: 100,
		max: 10000,
		values: [100, 10000],
		slide: function (event, ui) {
			$("#amount").val("AED " + ui.values[0] + " - AED" + ui.values[1]);
			filter.minPrice = ui.values[0];
			filter.maxPrice = ui.values[1];
		}
	});
	$("#amount").val("AED " + $("#slider-range").slider("values", 0) +
	" - AED " + $("#slider-range").slider("values", 1));

	

});


function DeleteWish(element, id) {


	$('#message').html('<span class="fa fa-hour-watch"></span> Removing wish ...')
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


						$('#message').html('<span class="fa fa-check"></span>' + response.message)
						$('#message').slideDown();
					});
				} else {
					$('#message').html('<span class="fa fa-warning"></span>' + response.message)
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

			$('#message').html('<span class="fa fa-check"></span> Car removed from your wishlist!')
			$('#message').slideDown();

			setTimeout(function () {
				$('#message').slideUp();
			}, 3000);
		});
	}
}

function GetFilterValues() {
	//filter.Status = $("#Status").val();
	//filter.ShipmentStatus = $("#ShipmentStatus").val();
	filter.pageNumber = pg;
	filter.sortBy = $("#SortBy").val();
}

function GetCars() {

	$.ajax({
		type: 'POST',
		url: '/en/customer/Wishlist',
		contentType: "application/json",
		data: JSON.stringify(filter),
		success: function (response) {
			console.log(response.data);
			BindGridCars(response.data);
		}
	});
}

function BindGridCars(data) {

	$.each(data, function (k, v) {
		var template = '<div class="col-6 col-sm-6 col-md-4 col-lg-2 item" id=' + v.ID + '>';
		template += '		<!-- start car image -->';
		template += '		<div class="car-image">';
		template += '			<!-- start car image -->';
		template += '			<a href="/car/' + v.Slug + '" class="grid-view-item__link">';
		template += '				<!-- image -->';
		template += '				<img class="primary blur-up lazyload img-lazyload" data-src="' + v.Thumbnail + '" src="' + v.Thumbnail + '" alt="image" title="car">';
		template += '				<!-- End image -->';
		template += '				<!-- Hover image -->';
		template += '				<img class="hover blur-up lazyload img-lazyload" data-src="' + v.Thumbnail + '" src="' + v.Thumbnail + '" alt="image" title="car">';
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
		template += '			<div class="variants add">';
		template += '				<button class="btn btn-addto-cart" data-type="1" onclick="CarCompareArrayFunction(this, ' + v.ID + ')" id="car-compare-' + v.ID + '" type="button" tabindex="0">Add To Compare</button>';
		template += '			</div>';
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
		template += '				<a href="/car/' + v.Slug + '">' + v.Title + '</a>';
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
		//template += '			<div class="car-review car-review-new" data="' + (v.Rating ? v.Rating : 0) + '" NoOfRatings="' + v.NoOfRatings + '">';
		//template += '			</div>';
		template += '		</div>';
		template += '		<!-- End car details -->';
		template += '	</div>';

		$('#CarsContainers').append(template);
	});

	CarsCompareCheckArray();

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
		$('#CarsContainers').html('<div class="alert alert-dark">No Records Found!</div>');
	}

	setTimeout(function () { OnErrorImage(); }, 3000);
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

function kFormatter(num) {
	return Math.abs(num) > 999 ? Math.sign(num) * ((Math.abs(num) / 1000).toFixed(1)) + 'k' : Math.sign(num) * Math.abs(num)
	//return numeral(num).format('0.0a');
}

//function BindFilters() {

//	/*Fetch Categories*/
//	$.ajax({
//		type: 'GET',
//		url: '/en/categories',
//		success: function (response) {
//			BindFilterCategories(response.data);
//		}
//	});
//}

function BindBrands(data) {

	$('#sidebar_brands').empty()
	$.each(data, function (k, v) {

		$('#sidebar_brands').append('<li>'
		+ '	<input type="checkbox" name="chkbrand" value="' + v.name + '" id="chk_br_' + v.id + '" data="' + v.id + '">'
		+ '	<label for="chk_br_' + v.id + '"><span><span></span></span>' + v.name + '</label>'
		+ '</li>');
	});

	$('input[name=chkbrand]').change(function () {
		if ($(this).prop('checked')) {
			filter.brandID = $(this).attr('data');
		} else {
			filter.brandID = null;
		}

		$('#CarsContainers').empty();
		pg = 1;
		GetFilterValues();
		GetCars();

		$("input[name=chkbrand]").not($(this)).prop('checked', false);
	});
}

//function BindFilterCategories(data) {

//	var Parents = data.filter(function (obj) {
//		return obj.ParentID == null
//	});

//	var Childs = data.filter(function (obj) {
//		return obj.ParentID != null
//	});

//	$('#sidebar_categories').empty()
//	$.each(Parents, function (k, v) {
//		if (v.hasChilds == true) {

//			$('#sidebar_categories').append('<li class="level1 sub-level" id="' + v.ID + '">'
//											+ '    <a href="javascript:;" class="site-nav">' + v.Name + '</a>'
//											+ '    <ul class="sublinks">'
//											+ '    </ul>'
//											+ '</li>');
//		} else {
//			$('#sidebar_categories').append('<li class="lvl-1"><a href="javascript:;" class="site-nav">' + v.Name + '</a></li>');
//		}
//	});

//	$.each(Childs, function (k, v) {
//		$('.level1.sub-level[id=' + v.ParentID + ']').find('.sublinks').append('<li class="level2"><a href="javascript:;" class="site-nav">' + v.Name + '</a></li>');
//	});

//	$(".sidebar_categories .sub-level a").on("click", function () {
//		$(this).toggleClass('active');
//		$(this).next(".sublinks").slideToggle("slow");
//	});
//}