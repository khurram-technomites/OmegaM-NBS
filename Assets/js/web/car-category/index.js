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
	minPrice: 1,
	maxPrice: 10000,
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

			window.history.pushState('', 'shop', '/shop');

			$('#CarsContainers').empty();
			pg = 1;
			GetFilterValues();
			GetCars();

			BindFilters();
		}
	});

	

});

function BindFilters() {

	category = categories.find(function (obj) {
		return obj.Slug == GetURLParameter();
	})

	if (category) {
		$('#car-title').text(category.Name);
		filter.categoryID = category.ID;

		var element = $(".sidebar_categories a[category=" + category.ID + "]");

		if (!category.ParentID || (category.ParentID && !category.hasChilds && !$(element).attr('parent'))) {
			$(".sidebar_categories a[category=" + category.ID + "]").addClass('selected').addClass('active');
			$(".sidebar_categories a[category=" + category.ID + "]").closest(".sub-level").find('a:first').addClass("active");
			$(".sidebar_categories a[category=" + category.ID + "]").closest(".sub-level").find('a:first').next(".sublinks").slideToggle("slow");
		}
		else {
			if ($(element).attr('parent')) {
				$(".sidebar_categories a[category=" + category.ID + "]").addClass('selected').addClass('active');
				$(".sidebar_categories a[category=" + category.ID + "]").closest(".level1").find('a:first').addClass("active");
				$(".sidebar_categories a[category=" + category.ID + "]").closest(".level2").find('a:first').addClass("active");
				$(".sidebar_categories a[category=" + category.ID + "]").closest(".level1").find('a:first').next(".sublinks").slideToggle("slow");
				$(".sidebar_categories a[category=" + category.ID + "]").closest(".level2").find('a:first').next(".sublinks").slideToggle("slow");
			}
			else {
				$(".sidebar_categories a[category=" + category.ID + "]").addClass('selected');
				$(".sidebar_categories a[category=" + category.ID + "]").closest(".level1").find('a:first').addClass("active");
				$(".sidebar_categories a[category=" + category.ID + "]").closest(".level1").find('a:first').next(".sublinks").slideToggle("slow");
            }
		}

		GetCategorywiseFilters();
	} else {
		$('#car-title').text("Shop");
		GetAttributeFilters();
		GetBrands();
	}
}

function GetAttributeFilters() {
	$.ajax({
		type: 'GET',
		url: '/' + lang + '/filters',
		contentType: "application/json",
		success: function (response) {
			BindAttributeFilters(response.data);
		}
	});
}

function BindAttributeFilters(data) {

	$('#filters-attributes').empty()

	$.each(data, function (k, v) {

		if (v.Name) {
			if (v.Name.toUpperCase() == "COLOR") {
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
																	+ '	<div class="col-8">'
																	+ '	</div>'
																	+ '	<div class="col-4 text-right margin-25px-top">'
				+ '		<button type="button" class="btn btn--secondary btn--small btn-reset" onclick="ResetAttributes(' + v.ID + ')" title="Reset">Reset</button>'
																	+ '	</div>'
																	+ '</div>');
		}
	});

	$("#filters-attributes .swacth-btn").on("click", function () {

		if ($(this).hasClass('checked')) {
			$(this).removeClass('checked')
		} else {
			$(this).addClass('checked')
		}

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

function BindFilterCategories(data) {

	var Parents = data.filter(function (obj) {
		return obj.ParentID == null
	});

	var Childs = data.filter(function (obj) {
		return obj.ParentID != null
	});
	

	/*First Level Category Binding*/
	$('#sidebar_categories').empty()
	$.each(Parents, function (k, v) {
		if (v.hasChilds == true) {

			$('#sidebar_categories').append('<li class="level1 sub-level" id="' + v.ID + '"  >'
				+ '    <a href="javascript:;" class="site-nav" slug="' + v.Slug + '" category="' + v.ID + '">' + v.Name + '</a>'
				+ '    <ul class="sublinks sub-links-1">'
				+ '    </ul>'
				+ '</li>');
		} else {
			$('#sidebar_categories').append('<li class="lvl-1"><a href="javascript:;" class="site-nav" slug="' + v.Slug + '" category="' + v.ID + '">' + v.Name + '</a></li>');
		}
	});

	/*Second Level Category Binding*/
	$.each(Childs, function (k, v) {

		if (v.hasChilds == true) {

			$('.level1.sub-level[id=' + v.ParentID + ']').find('.sub-links-1').append('<li class="level2 sub-level" id="' + v.ID + '">'
				+ '    <a href="javascript:;" class="site-nav site-nav-2" slug="' + v.Slug + '" category="' + v.ID + '">' + v.Name + '</a>'
				+ '    <ul class="sublinks sub-links-2">'
				+ '    </ul>'
				+ '</li>');

			/*Third Level Category Binding*/
			var lvl2Childs = data.filter(function (obj) {
				return obj.ParentID == v.ID
			});
			$.each(lvl2Childs, function (k2, v2) {
				$('.level2.sub-level[id=' + v2.ParentID + ']').find('.sub-links-2').append('<li class="lvl-3"><a href="javascript:;" class="site-nav" slug="' + v2.Slug + '"  category="' + v2.ID + '" parent = "' + v2.ParentID + '">' + v2.Name + '</a></li>');
			});

		} else {
			$('.level1.sub-level[id=' + v.ParentID + ']').find('.sub-links-1').append('<li class="lvl-2"><a href="javascript:;" class="site-nav" slug="' + v.Slug + '"  category="' + v.ID + '">' + v.Name + '</a></li>');
		}
	});

	$(".sidebar_categories a").on("click", function () {		



		$(this).toggleClass('active');
		$(this).next(".sublinks").slideToggle("slow");


		if (filter.categoryID != $(this).attr('category')) {
			filter.categoryID = $(this).attr('category');

			$(".sidebar_categories a.selected").removeClass('selected');
			$(this).addClass('selected');

			var selectedcategory = categories.find(function (obj) {
				return obj.ID == filter.categoryID;
			});

			$('#car-title').text(selectedcategory.Name);
			window.history.pushState('', selectedcategory.Name, '/car-category/' + selectedcategory.Slug);

			if (CategorywiseFilters && CategorywiseFilters.length > 0) {
				BindCategorywiseFilters();
			} else {
				GetCategorywiseFilters();
			}
			$('#btnfilter').trigger('click');
		}
	});
}

function GetCategorywiseFilters() {
	if (!IsCategorywiseFiltersLoaded) {
		$.ajax({
			type: 'GET',
			url: '/' + lang + '/Categorywisefilters',
			contentType: "application/json",
			success: function (response) {

				

				IsCategorywiseFiltersLoaded = true;
				CategorywiseFilters = response.data.categories;
				BindCategorywiseFilters();
			}
		});
	}
}

function BindCategorywiseFilters() {

	

	if (CategorywiseFilters && CategorywiseFilters.length > 0) {
		let CategorywiseFilter = CategorywiseFilters.find(function (obj) {
			return obj.id == filter.categoryID;
		})
		if (CategorywiseFilter) {
			BindAttributeFilters(CategorywiseFilter.attributes);
			BindCategoryBrands(CategorywiseFilter.brands);

			$('#sidebar_brands').slideDown(300);
			$("#filters-attributes").slideDown(300);
		} else {
			$('#sidebar_brands').slideUp(300).empty();
			$("#filters-attributes").slideUp(300).empty();
		}
	}
}

function GetBrands() {
	/*Fetch and Bind Loved Brand Section*/
	$.ajax({
		type: 'Get',
		url: '/' + lang + '/brands/',
		success: function (response) {
			BindBrands(response.data);
		}
	});
}

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

function BindCategoryBrands(data) {

	$('#sidebar_brands').empty()
	if (data.length > 0) {
		$.each(data[0].Brands, function (k, v) {
			if (v) {
				v = v.split("|");
				if (v.length > 1) {
					$('#sidebar_brands').append('<li>'
					+ '	<input type="checkbox" name="chkbrand" value="' + v[1] + '" id="chk_br_' + v[0] + '" data="' + v[0] + '">'
					+ '	<label for="chk_br_' + v[0] + '"><span><span></span></span>' + v[1] + '</label>'
					+ '</li>');
				}
			}
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
		template += '		<div class="car-image gallery-car-image">';
		template += '			<!-- start car image -->';
		template += '			<a href="/car/' + v.Slug + '" class="grid-view-item__link">';
		template += '				<!-- image -->';
		template += '				<img class="primary blur-up lazyload img-lazyload gallery-img" data-src="' + v.Thumbnail + '" src="' + v.Thumbnail + '" alt="image" title="car">';
		template += '				<!-- End image -->';
		template += '				<!-- Hover image -->';
		template += '				<img class="hover blur-up lazyload img-lazyload gallery-img-hover" data-src="' + v.Image1 + '" src="' + v.Image1 + '" alt="image" title="car">';
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
		template += '			<!-- Start car button -->';
		template += '			<div class="variants add">';
		template += '				<button class="btn btn-addto-cart" data-type="1" onclick="CarCompareArrayFunction(this, ' + v.ID + ')" id="car-compare-' + v.ID + '" type="button" tabindex="0">Add To Compare</button>';
		template += '			</div>';
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

	setTimeout(function () { OnErrorImage(); }, 3000);

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

function kFormatter(num) {
	return Math.abs(num) > 999 ? Math.sign(num) * ((Math.abs(num) / 1000).toFixed(1)) + 'k' : Math.sign(num) * Math.abs(num)
	//return numeral(num).format('0.0a');
}

function GetURLParameter() {
	
	var sPageURL = window.location.href;
	var indexOfLastSlash = sPageURL.lastIndexOf("/");

	if (indexOfLastSlash > 0 && sPageURL.length - 1 != indexOfLastSlash)
		return sPageURL.substring(indexOfLastSlash + 1).replace('#', '');
	else
		return 0;
}