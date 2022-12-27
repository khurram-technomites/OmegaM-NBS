const API_ENDPOINT = "https://socialbazar.co/";
var pg = 1;
var PageSize = 24;
var isPageRendered = false;
var totalPages;
var wishlist;

var filter = {
	search: null,
	categoryID: null,
	brandID: brand,
	minPrice: null,
	maxPrice: null,
	attributes: [],
	pageNumber: 1,
	sortBy: 1
}
var CategorywiseFilters = [];
var category;
var IsCategorywiseFiltersLoaded = false;

$(document).ready(function () {

	var wish = localStorage.getItem("wishlist");
	if (wish) {
		wishlist = JSON.parse(wish);
	}

	$('#CarsContainers').empty();
	GetFilterValues();
	GetCars();

	$('#CarSearch').change(function () {
		$('#CarsContainers').empty();
		pg = 1;
		GetFilterValues();
		GetCars();
	});

	$('#btnfilter').click(function () {
		$('#CarsContainers').empty();
		pg = 1;
		GetFilterValues();
		GetCars();

		$('#btnfilter').prop('disabled', true);
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
		min: 1,
		max: 10000,
		values: [1, 10000],
		slide: function (event, ui) {
			$("#amount").val("AED " + ui.values[0] + " - AED " + ui.values[1]);
			filter.minPrice = ui.values[0];
			filter.maxPrice = ui.values[1];
			$('#btnfilter').prop('disabled', false);
		}
	});
	$("#amount").val("AED " + $("#slider-range").slider("values", 0) +
	" - AED " + $("#slider-range").slider("values", 1));

	$('#btnBrandReset').click(function () {
		if ($('input[name=chkbrand]:checked').length > 0) {
			filter.brandID = null;
			$("input[name=chkbrand]").prop('checked', false);

			$('#CarsContainers').empty();
			pg = 1;
			GetFilterValues();
			GetCars();

		}
	});

	$('#btnCategoryReset').click(function () {
		if (filter.categoryID != null) {
			filter.categoryID = null;
			$(".sidebar_categories a.active").next(".sublinks").slideToggle("slow");
			$(".sidebar_categories a.active").removeClass('active');
			$(".sidebar_categories a.selected").removeClass('selected');

			$('#CarsContainers').empty();
			pg = 1;
			GetFilterValues();
			GetCars();
		}
	});

	/*Fetch and Bind Categories Filter*/
	$.ajax({
		type: 'Get',
		url: '/' + lang + '/brand/' + filter.brandID + '/categories',
		success: function (response) {
			if (response.success) {
				BindStoreCategoriesFilter(response.data);
			} else {
				$('#sidebar_categories').html('<div class="alert alert-dark">No Categories Found!</div>');
			}
		}
	});

	/*Fetch and Bind Attributes Filter*/
	$.ajax({
		type: 'Get',
		url: '/' + lang + '/brand/' + filter.brandID + '/filters',
		success: function (response) {
			if (response.success) {
				BindStoreAttributeFilter(response.data);
			} else {
				$('#filters-attributes').html('<div class="alert alert-dark" title="No Filters Found!">لم يتم العثور على مرشحات!</div>');
			}
		}
	});

});

function BindStoreCategoriesFilter(data) {

	var Parents = data.filter(function (obj) {
		return obj.ParentID == null
	});

	var Childs = data.filter(function (obj) {
		return obj.ParentID != null
	});

	$('#sidebar_categories').empty()
	$.each(Parents, function (k, v) {
		if (v.hasChilds == true) {

			$('#sidebar_categories').append('<li class="level1 sub-level" id="' + v.ID + '"  >'
											+ '    <a href="javascript:;" class="site-nav" slug="' + v.Slug + '" category="' + v.ID + '">' + v.Name + '</a>'
											+ '    <ul class="sublinks">'
											+ '    </ul>'
											+ '</li>');
		} else {
			$('#sidebar_categories').append('<li class="lvl-1"><a href="javascript:;" class="site-nav" slug="' + v.Slug + '" category="' + v.ID + '">' + v.Name + '</a></li>');
		}
	});

	$.each(Childs, function (k, v) {
		$('.level1.sub-level[id=' + v.ParentID + ']').find('.sublinks').append('<li class="level2"><a href="javascript:;" class="site-nav" slug="' + v.Slug + '"  category="' + v.ID + '">' + v.Name + '</a></li>');
	});

	$(".sidebar_categories a").on("click", function () {
		$(this).toggleClass('active');
		$(this).next(".sublinks").slideToggle("slow");


		if (filter.categoryID != $(this).attr('category')) {
			filter.categoryID = $(this).attr('category');
			$('#btnfilter').trigger('click');

			$(".sidebar_categories a.selected").removeClass('selected');
			$(this).addClass('selected');
		}
	});
}

function BindStoreAttributeFilter(data) {

	$('#filters-attributes').empty()
	$.each(data, function (k, v) {

		if (v.Name) {
			if (v.Name.toUpperCase() == "COLOR" || v.Name.toLowerCase() == "اللون") {
				$('#filters-attributes').append('<div class="sidebar_widget filterBox filter-widget filter-attribute" id="' + v.ID + '">'
													+ '	<div class="widget-title"><h2>' + v.Name + '</h2></div>'
														+ '<div class="filter-color swacth-list clearfix">'
														+ '</div>'
												+ '</div>');

				$.each(v.AttributeValues, function (ind, elem) {
					$('#filters-attributes .filter-widget[id=' + v.ID + ']').find('.swacth-list').append('<span class="swacth-btn ' + elem + '" style="background-color:' + elem + '" attribute-id="' + v.ID + '" value="' + elem + '"></span>');
				});

			} else {
				$('#filters-attributes').append('<div class="sidebar_widget filterBox filter-widget size-swacthes" id="' + v.ID + '">'
													+ '	<div class="widget-title"><h2>' + v.Name + '</h2></div>'
														+ '	<div class="filter-color swacth-list">'
															+ '<ul>'
															+ '</ul>'
														+ '</div>'
												+ '</div>');

				$.each(v.AttributeValues, function (ind, elem) {
					$('#filters-attributes .filter-widget[id=' + v.ID + ']').find('ul').append('<li><span class="swacth-btn" attribute-id="' + v.ID + '"  value="' + elem + '"> ' + elem + '</span></li>');
				});
			}

			$('#filters-attributes .filter-widget[id=' + v.ID + ']').append('<div class="row">'
														+ '	<div class="col-4 text-right margin-25px-top">'
														+ '		<button type="button" class="btn btn--secondary btn--small btn-reset" onclick="ResetAttributes(' + v.ID + ')" title="Reset">إعادة تعيين</button>'
														+ '	</div>'
														+ '	<div class="col-8">'
														+ '	</div>'
														+ '</div>');
		}
	});

	$("#filters-attributes .swacth-btn").on("click", function () {

		//$(this).parent().find(n).removeClass("checked");
		if ($(this).hasClass('checked')) {
			$(this).removeClass('checked')
		} else {
			$(this).addClass('checked')
		}
		//$(this).toggleClass('active');
		//$(this).next(".sublinks").slideToggle("slow");

		//filter.categoryID = $(this).attr('category');

		$('#btnfilter').trigger('click');
	});

	$(".filter-widget .widget-title").on("click", function () {
		$(this).next().slideToggle();
		$(this).toggleClass("active");
	});

}

function ResetAttributes(id) {
	if ($('#filters-attributes .filter-widget[id=' + id + '] .swacth-btn.checked').length > 0) {
		$('#filters-attributes .filter-widget[id=' + id + '] .swacth-btn').removeClass('checked');
		$('#btnfilter').trigger('click');
	}
}

function GetFilterValues() {
	//filter.Status = $("#Status").val();
	filter.search = $("#CarSearch").val();
	filter.pageNumber = pg;
	filter.sortBy = $("#SortBy").val();
	filter.attributes = [];

	$.each($("#filters-attributes .swacth-btn.checked"), function (k, v) {

		let attributeId = $(v).attr('attribute-id');
		let attribute = filter.attributes.find(function (obj) {
			return obj.attributeID == attributeId;
		});

		if (attribute) {
			if (!attribute.values.find(function (obj) {
				return obj == $(v).attr("value").trim()
			})) {

				attribute.values.push($(v).attr("value").trim());
			}
		} else {
			filter.attributes.push({
				attributeID: attributeId,
				values: [$(v).attr("value").trim()]
			});
		}
	});

}

function GetCars() {
	$.ajax({
		type: 'POST',
		url: '/' + lang + '/cars',
		contentType: "application/json",
		data: JSON.stringify(filter),
		success: function (response) {
			BindGridCars(response.data);
		}
	});
}

function BindGridCars(data) {
	$.each(data, function (k, v) {
		var template = '<div class="col-6 col-sm-6 col-md-4 col-lg-3 item" id=' + v.ID + '>';
		template += '		<!-- start car image -->';
		template += '		<div class="car-image">';
		template += '			<!-- start car image -->';
		template += '			<a href="/car/' + v.Slug + '" class="grid-view-item__link">';
		template += '				<!-- image -->';
		template += '				<img class="primary blur-up lazyload" data-src="' + v.Thumbnail + '" src="' + v.Thumbnail + '" alt="image" title="car">';
		template += '				<!-- End image -->';
		template += '				<!-- Hover image -->';
		template += '				<img class="hover blur-up lazyload" data-src="' + v.Image1 + '" src="' + v.Image1 + '" alt="image" title="car">';
		template += '				<!-- End hover image -->';
		//template += '				<!-- Variant Image-->';
		//template += '				<img class="grid-view-item__image hover variantImg" src="../assets/images/Category/abaya3.jpg" alt="image" title="car">';
		//template += '				<!-- Variant Image-->';
		template += '				<!-- car label -->';
		template += '				<div class="car-labels rounded">';
		if (v.IsSaleAvailable == true) {
			template += '					<span class="lbl on-sale">Sale</span>';
		}
		//template += '					<span class="lbl pr-label1">new</span>';
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
		if (typeof session != "undefined" && session == true && wishlist.filter(function (obj) { return obj.CarID == v.ID }).length > 0) {
			template += '				<div class="wishlist-btn customer-fields">';
			template += '					<a class="add-to-wishlist wishlist bg-transparent text-dark" href="javascript:;" onclick="DeleteCarFromWishlist(this,' + wishlist.find(function (obj) { return obj.CarID == v.ID }).ID + ')">';
			template += '						<i class="anm anm-heart icon icon-2x icon-light" style="color:#f54337;"></i>';
			template += '					</a>';
			template += '				</div>';
		} else {
			template += '				<div class="wishlist-btn customer-fields">';
			template += '					<a class="add-to-wishlist wishlist bg-transparent text-dark" href="javascript:;" onclick="AddCarToWishlist(this,' + v.ID + ')">';
			template += '						<i class="anm anm-heart-l icon icon-2x"></i>';
			template += '					</a>';
			template += '				</div>';
		}
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

function GetURLParameter() {
	var sPageURL = window.location.href;
	var indexOfLastSlash = sPageURL.lastIndexOf("/");

	if (indexOfLastSlash > 0 && sPageURL.length - 1 != indexOfLastSlash)
		return sPageURL.substring(indexOfLastSlash + 1).replace('#', '');
	else
		return 0;
}