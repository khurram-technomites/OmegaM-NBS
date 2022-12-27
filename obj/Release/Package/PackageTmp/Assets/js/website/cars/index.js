var pg = 1;
var PageSize = 10;
var FilteredRecord = 10;
var isPageRendered = false;
var totalPages;

var filter = {
	categories: []
	, latitude: null
	, longitude: null
	, startDate: null
	, endDate: null
	, packageID: null
	, makes: []
	, year: null
	, minPrice: null
	, maxPrice: null
	, rating: null
	, features: []
	, isFeatured: null
	, vendorID: null
	, search: null
	, pageSize: 20
	, pageNumber: 1
	, sortBy: 1
	, modelID: null
	, typeID: null
	, isSale: null
}

var CategorywiseFilters = [];
var category;
var search;
$(document).ready(function () {

	search = localStorage.getItem("search");
	if (search) {
		search = JSON.parse(search);

		$("#txtAddress").val(search.address);
		$("#latitude").val(search.latitude);
		$("#longitude").val(search.longitude);
		$("#StartDate").val(search.startDate);
		$("#StartTime").val(search.startTime);
		$("#EndDate").val(search.endDate);
		$("#EndTime").val(search.endTime);

		$('.car-packages .package').removeClass("selected");
		$(`.car-packages .package[data=${search.packageId}]`).addClass('selected');
	}

	var date = new Date();

	$('.input-group.date').datepicker({
		calendarWeeks: true,
		autoclose: true,
		todayHighlight: true,
		startDate: date
	}).datepicker('update', new Date($("#StartDate").val()));

	$('#StartTime').clockTimePicker({
		duration: true,
		precision: 60
	});

	$('#EndTime').clockTimePicker({
		duration: true,
		precision: 60
	});

	$('.car-packages .package').click(function () {
		$('.car-packages .package').removeClass("selected")
		$(this).addClass("selected");

		search.packageId = $(this).attr('data');
		filter.packageID = $(this).attr('data');

		localStorage.setItem("search", JSON.stringify(search));

		PopulateSchedule();

		$('#CarsContainers').empty();
		$('#PaginationBar').removeClass('d-md-flex').hide();
		$("#CarsContainerLoader .loader").show();
		pg = 1;
		$('html, body').animate({ scrollTop: $("#CarsContainers").offset().top - 150 }, 1000);

		GetFilterValues();
		GetCars();
	});

	$('#StartDate,#EndDate').change(function (event) {
		event.preventDefault()
		PopulateSchedule();
	});

	PopulateSchedule();

	//$('#txtAddress').keyup(function (e) {
	//    if (e.keyCode == 8 || e.keyCode == 32) {
	//        //$('#txtAddress').val().trim()

	//        var settings = {
	//            "url": "https://maps.googleapis.com/maps/api/place/textsearch/json?key=AIzaSyAEmhLaFjth5xau57Gy1NwE1O6apk443xY&input=sadder&inputtype=textquery",
	//            "method": "GET",
	//            "timeout": 0,
	//        };

	//        $.ajax(settings).done(function (response) {
	//            console.log(response);
	//        });

	//        $.ajax({
	//            type: 'Get',
	//            url: `https://maps.googleapis.com/maps/api/place/textsearch/json?key=AIzaSyAEmhLaFjth5xau57Gy1NwE1O6apk443xY&input=sadder&inputtype=textquery`,
	//            dataType: 'jsonp',
	//            cors: true,
	//            contentType: 'application/json',
	//            secure: true,
	//            headers: {
	//                'Access-Control-Allow-Origin': '*',
	//            },
	//            success: function (response) {

	//                if (response.results.length > 0) {
	//                    $.each(response.results, function (k, v) {
	//                        $("#MapSearchResult").append(`<span>
	//                                                            <i class="fa fa-map-marked"></i> v.formatted_address
	//                                                        </span>`);
	//                    });

	//                    //$('#txtAddress').val(response.results[0].formatted_address);

	//                    //$('#latitude').val(response.results[0].geometry.location.lat);
	//                    //$('#longitude').val(response.results[0].geometry.location.lng);

	//                    $("#MapSearchResult").show();

	//                } else {
	//                    $("#MapSearchResult").hide();
	//                    alert("Geolocation is not supported by this browser.");
	//                }
	//            }
	//        });
	//    }
	//});

	$('#SearchForm').submit(function () {

		search.categories = filter.categories.join(",");
		search.address = $("#txtAddress").val();
		search.latitude = $("#latitude").val();
		search.longitude = $("#longitude").val();
		search.startDate = $("#StartDate").val();
		search.startTime = $("#StartTime").val();
		search.endDate = $("#EndDate").val();
		search.endTime = $("#EndTime").val();
		search.packageId = $(".package.selected").attr('data');

		localStorage.setItem("search", JSON.stringify(search));

		$('#CarsContainers').empty();
		$('#PaginationBar').removeClass('d-md-flex').hide();
		$("#CarsContainerLoader .loader").show();
		pg = 1;

		$('html, body').animate({ scrollTop: $("#CarsContainers").offset().top - 150 }, 1000);
		GetFilterValues();
		GetCars();

		return false;
	});

	$('#CarsContainers').empty();
	$('#PaginationBar').removeClass('d-md-flex').hide();
	$("#CarsContainerLoader .loader").show();

	GetCategories();
	GetMakes();
	GetFeatures();

	$('.input-group.year').datepicker({
		calendarWeeks: true,
		autoclose: true,
		todayHighlight: true,
		format: "yyyy",
		viewMode: "years",
		minViewMode: "years",
		onSelect: function (dateText) {

			filter.year = dateText;

		}
	});

	$('#Year').change(function () {
		filter.year = $('#Year').val();

		$('#CarsContainers').empty();
		$('#PaginationBar').removeClass('d-md-flex').hide();
		$("#CarsContainerLoader .loader").show();
		pg = 1;
		$('html, body').animate({ scrollTop: $("#CarsContainers").offset().top - 150 }, 1000);

		GetFilterValues();
		GetCars();
	})

	function PriceRangeslider() {
		jQuery("#wo-themepricerangeslider").slider({
			range: true,
			min: 0,
			max: 100000,
			values: [0, 100000],
			slide: function (event, ui) {
				jQuery(".wo-amount").val(ui.values[0] + ' - ' + ui.values[1] + " AED");
				filter.minPrice = ui.values[0];
				filter.maxPrice = ui.values[1];

			}
		});
		jQuery(".wo-amount").val(jQuery("#wo-themepricerangeslider").slider("values", 0) + ' - ' + jQuery("#wo-themepricerangeslider").slider("values", 1) + " AED");
	}
	if (jQuery("#wo-themepricerangeslider").length > 0) {
		PriceRangeslider();
	}

	//#region Rating Work
	$(".rating-star-custom i").hover(
		function () {
			$(this).addClass("filled").prevAll(".fa-star").addClass("filled");
		}, function () {
			$(".rating-star-custom i").removeClass("filled");
		}
	);
	$(".rating-star-custom i").click(function () {
		$(".rating-star-custom i").removeClass("selected");
		$(this).addClass("filled selected").prevAll(".fa-star").addClass("filled selected");

		filter.rating = $('.rating-star-custom i.selected').length
	});
	//#endregion

	$('#CarSearch').change(function () {
		$('#CarsContainers').empty();
		$('#PaginationBar').removeClass('d-md-flex').hide();
		$("#CarsContainerLoader .loader").show();
		pg = 1;

		$('html, body').animate({ scrollTop: $("#CarsContainers").offset().top - 150 }, 1000);

		GetFilterValues();
		GetCars();
	});

	$('#SortBy').change(function () {
		$('#CarsContainers').empty();
		$('#PaginationBar').removeClass('d-md-flex').hide();
		$("#CarsContainerLoader .loader").show();
		pg = 1;

		$('html, body').animate({ scrollTop: $("#CarsContainers").offset().top - 150 }, 1000);
		GetFilterValues();
		GetCars();
	});

	$('#btnfilter').click(function () {
		$('#CarsContainers').empty();
		$('#PaginationBar').removeClass('d-md-flex').hide();
		$("#CarsContainerLoader .loader").show();
		pg = 1;

		$('html, body').animate({ scrollTop: $("#CarsContainers").offset().top - 150 }, 1000);
		GetFilterValues();
		GetCars();

		//$('#btnfilter').prop('disabled', true);
	});

	$("#PageSize").change(function () {
		PageSize = $(this).val();

		$('#CarsContainers').empty();
		$('#PaginationBar').removeClass('d-md-flex').hide();
		$("#CarsContainerLoader .loader").show();
		GetFilterValues();
		GetCars();
	});

	$('#PageNumber').keypress(function (e) {
		if ((e.keyCode >= 48 && e.keyCode <= 57) || (e.keyCode >= 96 && e.keyCode <= 105)) {
			// 0-9 only
			return true;
		}
		return false;
	});

	$('#PageNumber').change(function () {

		if ($('#PageNumber').val()) {
			if ($(this).val() > totalPages) {
				$(this).val(totalPages);
			} else if ($(this).val() <= 0) {
				$(this).val(1);
			}
		} else {
			$(this).val(1);
		}

		$('#FromPageNumber').submit();
	});

	$('#FromPageNumber').submit(function () {
		pagination(this, $('#PageNumber').val());
		return false;
	});

	$('.btn-reset').click(function () {
		if ($('input[name=chkCategory]:checked').length > 0) {
			filter.categories = [];
			$("input[name=chkCategory]:checked").prop('checked', false);
		}

		if ($('input[name=chkMake]:checked').length > 0) {
			filter.makes = [];
			$("input[name=chkMake]:checked").prop('checked', false);
		}

		if ($('input[name=chkFeature]:checked').length > 0) {
			filter.features = [];
			$("input[name=chkFeature]:checked").prop('checked', false);
		}

		filter.minPrice = null;
		filter.maxPrice = null;
		$("#wo-themepricerangeslider").slider('values', 0, 0); // sets first handle (index 0) to 50
		$("#wo-themepricerangeslider").slider('values', 1, 100000);

		filter.year = null
		$('.input-group.year').datepicker('setDate', null);

		filter.rating = null;
		$(".rating-star-custom i").removeClass("filled").removeClass("selected");

		$('#CarsContainers').empty();
		$('#PaginationBar').removeClass('d-md-flex').hide();
		$("#CarsContainerLoader .loader").show();
		pg = 1;
		$('html, body').animate({ scrollTop: $("#CarsContainers").offset().top - 150 }, 1000);

		GetFilterValues();
		GetCars();


	});

	$('#btnCategoryReset').click(function () {
		if (filter.categoryID != null) {
			filter.categoryID = null;
			$(".sidebar_categories a.active").next(".sublinks").slideToggle("slow");
			$(".sidebar_categories a.active").removeClass('active');
			$(".sidebar_categories a.selected").removeClass('selected');

			window.history.pushState('', 'Cars', '/shop');

			$('#CarsContainers').empty();
			pg = 1;


			$('html, body').animate({ scrollTop: $("#CarsContainers").offset().top - 150 }, 1000);

			GetFilterValues();
			GetCars();

			BindFilters();
		}
	});

});

function GetCategories() {
	$.ajax({
		type: 'Get',
		url: '/' + culture + '/categories',
		success: function (response) {
			if (response.success) {
				BindFilterCategories(response.data);
			} else {
			}
		}
	});
}

function BindFilterCategories(data) {

	$('#sidebar_categories').empty()
	$.each(data, function (k, v) {
		$('#sidebar_categories').append(`<span class="wo-checkbox">
                                                <input type="checkbox" id="category${v.ID}" name="chkCategory" data="${v.ID}">
                                                <label for="category${v.ID}">
                                                    <span>${v.Name}</span>
                                                    <em>(${v.CarCount > 1 ? v.CarCount + ` Cars` : v.CarCount + ` Car`}) </em>
                                                </label>
                                            </span>`);

	});

	if (search) {
		var categories = search.categories.split(',');
	}
	filter.categories = categories;
	if (categories) {
		categories.forEach(function (category) {
			if (category) {
				$(`input[name=chkCategory][data=${category}]`).prop('checked', true);
			}
		});
	}

	$('input[name=chkCategory]').change(function () {

		var elem = $(this);
		if (elem.prop('checked')) {
			filter.categories.push(elem.attr('data'));
		} else {
			filter.categories = filter.categories.filter(function (obj) {
				return obj != elem.attr('data')
			});
		}

		search.categories = filter.categories.join(",");
		localStorage.setItem("search", JSON.stringify(search));

		$('#CarsContainers').empty();
		$('#PaginationBar').removeClass('d-md-flex').hide();
		$("#CarsContainerLoader .loader").show();
		pg = 1;
		$('html, body').animate({ scrollTop: $("#CarsContainers").offset().top - 150 }, 1000);

		GetFilterValues();
		GetCars();
	});

	GetFilterValues();
	GetCars();
}

function GetMakes() {
	$.ajax({
		type: 'Get',
		url: '/' + culture + '/makes',
		success: function (response) {
			if (response.success) {
				BindMakes(response.data);
			} else {
			}
		}
	});
}

function BindMakes(data) {

	$('#sidebar_makes').empty()
	$.each(data, function (k, v) {
		$('#sidebar_makes').append(`<span class="wo-checkbox">
                                                <input type="checkbox" id="Make${v.id}" name="chkMake" data="${v.id}">
                                                <label for="Make${v.id}">
                                                    <span>${v.name}</span>
                                                </label>
                                            </span>`);

	});

	$('input[name=chkMake]').change(function () {

		var elem = $(this);
		if (elem.prop('checked')) {
			filter.makes.push(elem.attr('data'));
		} else {
			filter.makes = filter.makes.filter(function (obj) {
				return obj != elem.attr('data')
			});
		}

		$('#CarsContainers').empty();
		$('#PaginationBar').removeClass('d-md-flex').hide();
		$("#CarsContainerLoader .loader").show();
		pg = 1;

		$('html, body').animate({ scrollTop: $("#CarsContainers").offset().top - 150 }, 1000);

		GetFilterValues();
		GetCars();
	});
}

function SearchMakes(elem) {
	var text = $(elem).val().trim();
	if (text && text != "") {
		$('#sidebar_makes .wo-checkbox').hide();
		//$('.car-category .checkbox:contains("' + text + '")').css('background-color', 'red');
		$('#sidebar_makes .wo-checkbox:contains("' + text + '")').fadeIn();
	} else {
		$('#sidebar_makes .wo-checkbox').show();
	}
}

function GetFeatures() {
	$.ajax({
		type: 'Get',
		url: '/' + culture + '/features',
		success: function (response) {
			if (response.success) {
				BindFeatures(response.data);
			} else {
			}
		}
	});
}

function BindFeatures(data) {

	$('#sidebar_features').empty()
	$.each(data, function (k, v) {
		$('#sidebar_features').append(`<span class="wo-checkbox">
                                                <input type="checkbox" id="Feature${v.id}" name="chkFeature" data="${v.id}">
                                                <label for="Feature${v.id}">
                                                    <span>${v.name}</span>
                                                </label>
                                            </span>`);

	});

	$('input[name=chkFeature]').change(function () {
		var elem = $(this);
		if (elem.prop('checked')) {
			filter.features.push(elem.attr('data'));
		} else {
			filter.features = filter.features.filter(function (obj) {
				return obj != elem.attr('data')
			});
		}

		$('#CarsContainers').empty();
		$('#PaginationBar').removeClass('d-md-flex').hide();
		$("#CarsContainerLoader .loader").show();
		pg = 1;

		$('html, body').animate({ scrollTop: $("#CarsContainers").offset().top - 150 }, 1000);

		GetFilterValues();
		GetCars();
	});
}

function SearchFeatures(elem) {
	var text = $(elem).val().trim();
	if (text && text != "") {
		$('#sidebar_features .wo-checkbox').hide();
		//$('.car-category .checkbox:contains("' + text + '")').css('background-color', 'red');
		$('#sidebar_features .wo-checkbox:contains("' + text + '")').fadeIn();
	} else {
		$('#sidebar_features .wo-checkbox').show();
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
			if (v.Name.toUpperCase() == "COLOR" || v.Name.toLowerCase() == "اللون") {
				let count = 0;
				$('#filters-attributes').append(`<div class="border-bottom pb-4 mb-4 filter-widget" id="${v.ID}">
													<h4 class ="font-size-14 mb-3 font-weight-bold">${v.Name}</h4>
												</div>`);

				$.each(v.AttributeValues, function (ind, elem) {
					if (count < 3) {
						$('#filters-attributes .filter-widget[id=' + v.ID + ']').append(`<div class="form-group d-flex align-items-center justify-content-between mb-2 pb-1">
																							<div class="custom-control custom-checkbox">
																								<input type="checkbox" class ="custom-control-input swacth-btn" id="${v.Name}${ind}" attribute-id="${v.ID}" value="${elem}">
																								<label class ="custom-control-label filter-color" for="${v.Name}${ind}" ><span class ="color-badge" style="background-color:${elem}"></span>${elem}</label>
																							</div>
																						</div>`);
						count++;
					} else {
						if ($(`#collapse${v.Name}`).length == 0) {
							$('#filters-attributes .filter-widget[id=' + v.ID + ']').append(`<div class="collapse" id="collapse${v.Name}"></div>`);
							count++;
						}
						$(`#collapse${v.Name}`).append(`<div class="form-group d-flex align-items-center justify-content-between mb-2 pb-1">
																							<div class="custom-control custom-checkbox">
																								<input type="checkbox" class ="custom-control-input swacth-btn" id="${v.Name}${ind}" attribute-id="${v.ID}" value="${elem}">
																								<label class ="custom-control-label filter-color" for="${v.Name}${ind}" ><span class ="color-badge" style="background-color:${elem}"></span>${elem}</label>
																							</div>
																						</div>`);
					}
				});
				if (count > 3) {
					$('#filters-attributes .filter-widget[id=' + v.ID + ']').append(`<a class="link link-collapse small font-size-13 text-gray-27 d-inline-flex mt-2" data-toggle="collapse" href="#collapse${v.Name}" role="button" aria-expanded="false" aria-controls="collapse${v.Name}">
																						<span class="link__icon text-gray-27 bg-white">
																							<span class="link__icon-inner">+</span>
																						</span>
																						<span class="link-collapse__default">Show more</span>
																						<span class="link-collapse__active">Show less</span>
																					</a>`);
				}

			} else {
				let count = 0;
				$('#filters-attributes').append(`<div class="border-bottom pb-4 mb-4 filter-widget" id="${v.ID}">
													<h4 class ="font-size-14 mb-3 font-weight-bold">${v.Name}</h4>
												</div>`);

				$.each(v.AttributeValues, function (ind, elem) {
					if (count < 3) {
						$('#filters-attributes .filter-widget[id=' + v.ID + ']').append(`<div class="form-group d-flex align-items-center justify-content-between mb-2 pb-1">
																							<div class="custom-control custom-checkbox">
																								<input type="checkbox" class ="custom-control-input swacth-btn" id="${v.Name}${ind}" attribute-id="${v.ID}" value="${elem}">
																								<label class ="custom-control-label" for="${v.Name}${ind}">${elem}</label>
																							</div>
																						</div>`);
						count++;
					} else {
						if ($(`#collapse${v.Name}`).length == 0) {
							$('#filters-attributes .filter-widget[id=' + v.ID + ']').append(`<div class="collapse" id="collapse${v.Name}"></div>`);
							count++;
						}
						$(`#collapse${v.Name}`).append(`<div class="form-group d-flex align-items-center justify-content-between mb-2 pb-1">
																							<div class="custom-control custom-checkbox">
																								<input type="checkbox" class ="custom-control-input swacth-btn" id="${v.Name}${ind}" attribute-id="${v.ID}" value="${elem}">
																								<label class ="custom-control-label" for="${v.Name}${ind}">${elem}</label>
																							</div>
																						</div>`);
					}
				});
				if (count > 3) {
					$('#filters-attributes .filter-widget[id=' + v.ID + ']').append(`<a class="link link-collapse small font-size-13 text-gray-27 d-inline-flex mt-2" data-toggle="collapse" href="#collapse${v.Name}" role="button" aria-expanded="false" aria-controls="collapse${v.Name}">
																						<span class="link__icon text-gray-27 bg-white">
																							<span class="link__icon-inner">+</span>
																						</span>
																						<span class="link-collapse__default">Show more</span>
																						<span class="link-collapse__active">Show less</span>
																					</a>`);
				}
			}

			$('#filters-attributes .filter-widget[id=' + v.ID + ']').append(`<div class="row">
																				<div class="col-8">
																				</div>
																				<div class="col-4 text-right margin-25px-top">
																					<button type="button" class="btn btn--secondary btn--small btn-reset" onclick="ResetAttributes(${v.ID})">Reset</button>
																				</div>
																			</div>`);
		}
	});

	$("#filters-attributes .swacth-btn").on("click", function () {

		//if ($(this).hasClass('checked')) {
		//	$(this).removeClass('checked')
		//} else {
		//	$(this).addClass('checked')
		//}

		$('#btnfilter').trigger('click');
	});
}

function GetFilterValues() {

	filter.packageID = $('.car-packages .package.selected').attr('data');
	filter.latitude = $("#latitude").val();
	filter.longitude = $("#longitude").val();
	filter.startDate = $("#StartDate").val() + " " + $("#StartTime").val();
	filter.endDate = $("#EndDate").val() + " " + $("#EndTime").val();

	//filter.Status = $("#Status").val();
	filter.search = $("#CarSearch").val();
	filter.pageNumber = pg;
	filter.pageSize = PageSize;
	filter.sortBy = $("#SortBy").val();
}

function GetCars() {

	//filter.search = getParameterByName('q');

	if (filter.search) {
		$('#PageTitle').text(`"${filter.search}"`);
	}

	$.ajax({
		type: 'POST',
		url: '/' + culture + '/cars',
		contentType: "application/json",
		data: JSON.stringify(filter),
		success: function (response) {
			BindGridCars(response.data);
		}
	});
}

function BindGridCars(data) {
	console.log(data);
	$.each(data, function (k, v) {
		let carPrice = '';
		let amountSaved = '';
		let wishItem = ``;


		var car = `<div class="wo-vehiclesinfos car-item" id=${v.ID}>
                            <figure class="wo-vehicles-img">
                                <img src="${v.Thumbnail}" alt="${v.Title}">
                                <figcaption>
                                    <div class="wo-tags">
                                        <a href="javascript:void(0);" class="wo-tag-photos"><i class="ti-image"></i>${v.Images.split(",").length}</a>
                                    </div>
                                </figcaption>
                            </figure>
                            <div class="wo-vehicles-info">
                                <div class="wo-vehiclesinfo">
                                    <div>
                                        <div class="wo-vehicles__tags">
                                            <a href="javascript:void(0);">${v.Category}</a>
                                        </div>
                                        <div class="wo-vehiclesinfo__title">
                                            <h3><a href="/${culture}/cars/${v.Slug}">${v.Title}</a></h3>
                                            <div class ="rating car-review-new" data="${v.Rating}">
                                                <i class ="fa fa-star unfill"></i>
                                                <i class ="fa fa-star unfill"></i>
                                                <i class ="fa fa-star unfill"></i>
                                                <i class ="fa fa-star unfill"></i>
                                                <i class="fa fa-star unfill"></i>
                                            </div>
                                        </div>
                                    </div>
                                    <div>
                                        <div class="wo-contectinfo__price">
                                            <h4>AED <b>${v.Price}</b><span>/${v.PackageName}</span></h4>
                                        </div>
                                        <div class="wo-contectinfo__location">
                                            <div class="content">
                                                <a href="javascript:void(0);">${v.VendorName}</a>
                                                <address><i class="ti-location-pin"></i>${v.VendorAddress}</address>
                                            </div>
                                            <div class="image">
                                                <img src="${v.VendorLogo}" alt="">
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="wo-vehiclesinfo">
                                    <div>
                                        <ul class="wo-vehiclesinfo__list">
                                            <li><span><em>${ChangeString('Model Year', 'سنة الصنع')}</em>${v.ModelYear}</span></li>
                                            <li><span><em>${ChangeString('Fuel Economy', 'اقتصاد الوقود')}</em>${v.FuelEconomy}</span></li>
                                            <li><span><em>${ChangeString('Transmission', 'الانتقال')}</em>${v.Transmission}</span></li>
                                        </ul>
                                    </div>
                                    <div>
                                        <div class="wo-contectinfo">
                                            <div class="button-block">
                                                <a href="/${culture}/cars/${v.Slug}" class ="wo-btn">${ChangeString('Book Now', 'احجز الآن')}</a>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>`
		$('#CarsContainers').append(car);
	});

	FormatPrices();

	$('#PaginationBar').addClass('d-md-flex').show();
	$("#CarsContainerLoader .loader").hide();

	$('.car-review-new').each(function (k, v) {
		var rating = parseFloat($(v).attr('data'));
		$(this).find('i:lt(' + (rating) + ')').addClass("fill").removeClass("unfill");
	});

	//$('.car-review-new').each(function (k, v) {
	//	var rating = parseFloat($(v).attr('data'));
	//	$(v).removeClass("car-review-new");
	//	//$(v).removeClass("car-review-new").addClass('fa-2x');
	//	addScore(rating * 20, $(v));
	//});

	if (data && data.length > 0) {
		FilteredRecord = data[0].filteredRecords;
		RenderPagination(data[0].TotalRecords, data[0].filteredRecords);
	} else {
		FilteredRecord = 0;
		RenderPagination(0, 0);

	}

	if ($('#CarsContainers').html().length == 0) {
		$('html, body').animate({ scrollTop: $("#CarsContainers").offset().top - 150 }, 1000);
		$('#CarsContainers').html('<div class="col-12 text-center">No Records Found!</div>');
	} else {
		OnErrorImage(0);
	}
}

function RenderPagination(totalRecord, filteredRecord) {

	totalPages = Math.ceil(filteredRecord / PageSize);
	totalPages = (!totalPages && totalPages == 0) ? 1 : totalPages;

	$("#PageNumber").attr("max", totalPages);
	$("#PageNumber").val(pg);
	$("#TotalPages").text(totalPages);
	if (FilteredRecord > 0) {
		if (FilteredRecord > PageSize) {
			$('.cars-filtration-stats').text(
				ChangeString(`Showing ${(((pg - 1) * PageSize) + 1)}–${pg * PageSize} of ${FilteredRecord} results`,
					`عرض ${(((pg - 1) * PageSize) + 1)}–${pg * PageSize} من  ${FilteredRecord} نتيجة`));
		} else {
			$('.cars-filtration-stats').text(ChangeString(`Showing ${FilteredRecord} results`,
				`عرض ${FilteredRecord} نتيجة`));
		}
	} else {
		$('.cars-filtration-stats').text(``);
	}

	$("#pagination").html('');
	if (totalPages > 1) {
		$("#pagination").append(`<li class="wo-prevpage"><a id="btn-pagination-prev" href="javascript:void(0);">${ChangeString('PRE', 'سابق')}</a></li>`);
	}

	for (var i = 1; i <= totalPages; i++) {
		if (i == pg) {
			$("#pagination").append(`<li class="wo-active"><a href="javascript:void(0);" onclick="pagination(this,${i});">${i}</a></li>`);
		} else {
			$("#pagination").append(`<li><a href="javascript:void(0);"  onclick="pagination(this,${i});">${i}</a></li>`);
		}
	}

	if (totalPages > 1) {
		$("#pagination").append(`<li class="wo-nextpage"><a  id="btn-pagination-next" href="javascript:void(0);">${ChangeString('NEX', 'التالي')}</a></li>`);
	}

	$('#btn-pagination-next').click(function () {
		if (pg < totalPages) {
			pg++;
			$('#PaginationBar').removeClass('d-md-flex').hide();
			$("#CarsContainerLoader .loader").show();
			$('#CarsContainers').empty();
			GetFilterValues();
			GetCars();
			updatePagination(pg);
		}
	});

	$('#btn-pagination-prev').click(function () {
		if (pg > 1) {
			pg--;
			$('#PaginationBar').removeClass('d-md-flex').hide();
			$("#CarsContainerLoader .loader").show();
			$('#CarsContainers').empty();
			GetFilterValues();
			GetCars();
			updatePagination(pg);
		}
	});


	stylePagination();
	//animatePagination();
}

function updatePagination(pageNumber) {
	$('.pagination a.current').removeClass('current');
	$(".pagination").find("li a:contains('" + pageNumber + "')").removeClass('current');
	$("#PageNumber").val(pageNumber);
	if (FilteredRecord > 0) {
		if (FilteredRecord > PageSize) {
			$('.cars-filtration-stats').text(ChangeString(`Showing ${(((pg - 1) * PageSize) + 1)}–${pg * PageSize} of ${FilteredRecord} results`,
				`عرض ${(((pg - 1) * PageSize) + 1)}–${pg * PageSize} من  ${FilteredRecord} نتيجة`));
		} else {
			$('.cars-filtration-stats').text(ChangeString(`Showing ${FilteredRecord} results`,
				`عرض ${FilteredRecord} نتيجة`));
		}
	} else {
		$('.cars-filtration-stats').text(``);
	}

	stylePagination();
}

function pagination(e, pageNumber) {
	if (pg != pageNumber) {
		pg = pageNumber;
		$('#PaginationBar').removeClass('d-md-flex').hide();
		$("#CarsContainerLoader .loader").show();
		$('#CarsContainers').empty();

		$('html, body').animate({ scrollTop: $("#CarsContainers").offset().top - 150 }, 1000);

		GetFilterValues();
		GetCars();
		updatePagination(pg);
	}
}

function animatePagination() {
	var fixmeTop = $('#CarsContainers .wo-vehiclesinfos:last').offset().top - 200;       // get initial position of the element

	$(window).scroll(function () {                  // assign scroll event listener

		var currentScroll = $(window).scrollTop(); // get current position

		if (currentScroll >= fixmeTop) {           // apply position: fixed if you
			$('.wo-pagination').removeClass('floating');
		} else {                                   // apply position: static
			$('.wo-pagination').addClass('floating');
		}
	});
}

function stylePagination() {

	if ($("#pagination li:nth-child(2)").hasClass('wo-active')) {
		$('#btn-pagination-prev').hide();
	} else {
		$('#btn-pagination-prev').show();
	}

	if ($("#pagination li:nth-last-child(2)").hasClass('wo-active')) {
		$('#btn-pagination-next').hide();
	} else {
		$('#btn-pagination-next').show();
	}

	if ($("#pagination li").length > 6) {
		$("#pagination li:not(:first):not(:last):not(:nth-last-child(2)):not(:nth-child(2))").hide();

		var $active = $("#pagination .wo-active");
		var activeIndex = $active.index();
		var $around = $active.siblings().addBack()
			.slice(Math.max(0, activeIndex - 2), activeIndex + 3)
			.not($active);

		if ((activeIndex - 2) > 0) {
			$("#pagination li:nth-child(2)").addClass('mr-3');
		}

		if ((activeIndex + 3) < $("#pagination li").length) {
			$("#pagination li:nth-last-child(2)").addClass('ml-3');
		}

		$active.show();
		$around.show();
	}
}

function ResetAttributes(id) {
	if ($('#filters-attributes .filter-widget[id=' + id + '] .swacth-btn:checked').length > 0) {
		$('#filters-attributes .filter-widget[id=' + id + '] .swacth-btn').prop('checked', false);
		$('#btnfilter').trigger('click');
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

jQuery.expr[':'].contains = function (a, i, m) {
	return jQuery(a).text().toUpperCase()
		.indexOf(m[3].toUpperCase()) >= 0;
};


//#region Functions for Binding Map

function getLocation() {
	if (navigator.geolocation) {
		navigator.geolocation.getCurrentPosition(showPosition);
	} else {
		alert("Geolocation is not supported by this browser.");
	}
}

function showPosition(position) {
	$('#latitude').val(position.coords.latitude);
	$('#longitude').val(position.coords.longitude);

	$.ajax({
		type: 'Get',
		url: `https://maps.googleapis.com/maps/api/geocode/json?latlng=${position.coords.latitude},${position.coords.longitude}&key=AIzaSyAEmhLaFjth5xau57Gy1NwE1O6apk443xY`,
		success: function (response) {
			if (response.results.length > 0) {
				$('#txtAddress').val(response.results[0].formatted_address);
			} else {
				alert("Geolocation is not supported by this browser.");
			}
		}
	});
}

function initAutocomplete() {

	//#region Single Id Bound

	var defaultBounds = new google.maps.LatLngBounds();
	var options = {
		types: ['(cities)'],
		bounds: defaultBounds
	};

	// get DOM's input element
	var input = document.getElementById('txtAddress');

	// Make Autocomplete instance
	var autocomplete = new google.maps.places.Autocomplete(input, options);

	// Listener for whenever input value changes
	autocomplete.addListener('place_changed', function () {
		//// Get place info
		var place = autocomplete.getPlace();

		//// Do whatever with the value!
		$('#latitude').val(place.geometry.location.lat());
		$('#longitude').val(place.geometry.location.lng());
	});

	//#endregion
}

//#endregion