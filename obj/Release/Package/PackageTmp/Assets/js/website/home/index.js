'use strict'

//#region Global Variables and Arrays

var latitude;
var longitude;
var IsMapLoaded = false;
var MapAddress;
var IsGetCurrentLocation = false;
//#endregion

//#region document ready function
$(document).ready(function () {

	GetAndBindBannerSlider();

	GetAndBindCategories();

	var currentLocation = localStorage.getItem("currentLocation");


	if (currentLocation) {
		currentLocation = JSON.parse(currentLocation);

		latitude = currentLocation.latitude;
		longitude = currentLocation.longitude;
		MapAddress = currentLocation.address;

		$("#latitude").val(latitude);
		$("#longitude").val(longitude);
		$("#txtAddress").val(MapAddress);
		$("#current-location").val(MapAddress);
	}

	var date = new Date();

	$('.input-group.date').datepicker({
		calendarWeeks: true,
		autoclose: true,
		todayHighlight: true,
		startDate: date
	}).datepicker('update', date).datepicker("option", "minDate", date);;

	$('#StartTime').clockTimePicker({
		duration: true,
		precision: 60
	});

	$('#EndTime').clockTimePicker({
		duration: true,
		precision: 60
	});

	$('.car-packages .package').click(function (event) {
		event.preventDefault()
		$('.car-packages .package').removeClass("selected");
		$(this).addClass("selected");

		PopulateSchedule();
	});

	$('#StartDate,#EndDate').change(function (event) {
		event.preventDefault()
		PopulateSchedule();
	});

	PopulateSchedule();

	//#region txtAddress keyup function

	//$('#txtAddress').keyup(function (e) {

	//	if (e.keyCode == 8 || e.keyCode == 32) {
	//		//$('#txtAddress').val().trim()

	//		var settings = {
	//			"url": "https://maps.googleapis.com/maps/api/place/textsearch/json?key=AIzaSyAEmhLaFjth5xau57Gy1NwE1O6apk443xY&input=sadder&inputtype=textquery",
	//			"method": "GET",
	//			"timeout": 0,
	//		};

	//		$.ajax(settings).done(function (response) {
	//			console.log(response);
	//		});

	//		$.ajax({
	//			type: 'Get',
	//			url: `https://maps.googleapis.com/maps/api/place/textsearch/json?key=AIzaSyAEmhLaFjth5xau57Gy1NwE1O6apk443xY&input=sadder&inputtype=textquery`,
	//			dataType: 'jsonp',
	//			cors: true,
	//			contentType: 'application/json',
	//			secure: true,
	//			headers: {
	//				'Access-Control-Allow-Origin': '*',
	//			},
	//			success: function (response) {

	//				if (response.results.length > 0) {
	//					$.each(response.results, function (k, v) {
	//						$("#MapSearchResult").append(`<span>
	//                                                               <i class="fa fa-map-marked"></i> v.formatted_address
	//                                                           </span>`);
	//					});

	//					//$('#txtAddress').val(response.results[0].formatted_address);

	//					//$('#latitude').val(response.results[0].geometry.location.lat);
	//					//$('#longitude').val(response.results[0].geometry.location.lng);

	//					$("#MapSearchResult").show();

	//				} else {
	//					$("#MapSearchResult").hide();
	//					alert("Geolocation is not supported by this browser.");
	//				}
	//			}
	//		});
	//	}
	//});

	//#endregion

	$('#SearchForm').submit(function () {
		var search = {
			categories: $('.category.selected').map(function (i, opt) { return $(opt).attr('id'); }).toArray().join(','),
			address: $("#txtAddress").val(),
			latitude: $("#latitude").val(),
			longitude: $("#longitude").val(),
			startDate: $("#StartDate").val(),
			startTime: $("#StartTime").val(),
			endDate: $("#EndDate").val(),
			endTime: $("#EndTime").val(),
			packageId: $(".package.selected").attr('data')
		}

		localStorage.setItem("search", JSON.stringify(search));
		window.location.href = "/book";

		return false;
	});

	GetAndBindNewsFeed();
	GetAndBindAppreciation(3);
});
//#endregion

//#region Ajax Call

function GetAndBindBannerSlider() {
	//Carousel Slider Home Page
	$.ajax({
		type: 'Get',
		url: '/' + culture + '/banners/Website',
		success: function (response) {
			if (response.success) {
				BindBannerSlider(response.data);
			} else {
				console.log("GetAndBindBannerSlider() Runtime Error.");
				$('#wo-mainbanner-wrap-header').remove();
			}
		}
	});
}

function GetAndBindCategories() {
	$.ajax({
		type: 'Get',
		url: '/' + culture + '/categories/',
		success: function (response) {
			if (response.success) {
				BindCategories(response.data);
			} else {
			}
		}
	});
}

function GetAndBindNewsFeed() {
	//Newsfeed
	$.ajax({
		type: 'Get',
		url: '/' + culture + '/news/',
		success: function (response) {
			if (response.success) {
				BindNewsfeed(response.data);
			} else {
				console.log("GetAndBindNewsFeed() Runtime Error.");
				$('#web-newsfeed-grid').remove();
			}
		}
	});
}

function GetAndBindAppreciation(max) {
	//Newsfeed
	$.ajax({
		type: 'Get',
		url: '/' + culture + '/appreciation/' + max,
		success: function (response) {
			if (response.success) {
				BindAppreciation(response.data);
			} else {
				console.log("GetAndBindAppreciation() Runtime Error.");
				$('#web-appreciation-slider').remove();
			}
		}
	});
}

function GetAndBindFeaturedCars() {
	$.ajax({
		type: 'POST',
		url: '/' + culture + '/cars',
		contentType: "application/json",
		data: JSON.stringify({
			isFeatured: true,
			pageSize: 24
		}),
		success: function (response) {
			if (response.success) {
				BindCars(response.data);
			} else {
			}
		}
	});
}

//#endregion

//#region Functions for Binding Data

function BindBannerSlider(data) {
	//console.log(data);
	if (data.length > 0) {

		$.each(data, function (k, v) {
			$('#wo-banner__silder').append(`
				<figure>
					<a href="${v.Url}">
						<img src="${v.Name}" alt="img description">
					</a>
				</figure>
			`);
		});

		var _wo_banner__silder = jQuery("#wo-banner__silder")
		_wo_banner__silder.owlCarousel({
			items: 1,
			loop: true,
			nav: true,
			autoplay: false,
			dots: false,
			smartSpeed: 500,
			responsiveClass: true,
			navClass: ['wo-prev', 'wo-next'],
			navContainerClass: 'wo-bannernav',
			navText: ['<span class="ti-angle-left"></span>', '<span class="ti-angle-right"></span>'],
			responsive: {
				0: {
					dots: true,
					nav: false,
				},
				481: {
					dots: false,
					nav: true,
				},
			}
		});

		$('#wo-mainbanner-wrap-header').show();
	}
	else {
		$('#wo-mainbanner-wrap-header').remove();
	}

}

function BindCategories(data) {

	$('#CategoriesContainer').empty();

	$.each(data, function (k, v) {

		var template = `<div class="wo-branditem category ${k == 0 ? "selected" : ""}" id=${v.ID} onclick=$(this).toggleClass("selected");>
								<img src="${v.Cover}" alt="img description">
								<h4>${v.Name}</h4>
								<span>${v.CarCount > 1 ? v.CarCount + ` Cars` : v.CarCount + ` Car`} Available</span>
							</div>`;

		$('#CategoriesContainer').append(template);
	});

	/* Brand Slider */
	var _wo_brands__slider = jQuery(".wo-brands__slider")
	_wo_brands__slider.owlCarousel({
		//autoplay: false,
		//dots: true,
		///*items: 9,*/
		//margin: 10,
		//loop: false,
		//autoWidth: true,
		//responsive: {
		//    0: {
		//        items: 1,
		//    },
		//    767: {
		//        items: 2,
		//    },
		//    1199: {
		//        items: 4,
		//    },
		//    1440: {
		//        items: 4,
		//    },
		//    1750: {
		//        items: 6
		//    }
		//}

		autoplay: false,
		/*center: true,*/
		loop: true,
		infinite: true,
		/*nav: false,*/
		//rtl: true,
		dots: true,
		items: 4,
		autoplayTimeout: 4000,
		autoplayHoverPause: true,

		responsive: {
			0: {
				items: 2,
			},
			450: {
				items: 2,
			},
			767: {
				items: 3,
			},
			1199: {
				items: 4,
			},
			1440: {
				items: 4,
			}
		}
	});
}

function BindNewsfeed(data) {
	/*
	$('#NewsFeedGrid').empty()
	$.each(data, function (k, v) {

		$('#NewsFeedGrid').append(`
			 <div class="col-12 col-sm-12 col-md-6 col-lg-6">
				<div class="wrap-blog">
					<a href="/en/news/${v.Slug}" class="article__grid-image">
						<img src="${v.BannerSliderImage}" alt="${v.Title}" title="${v.Title}" class="blur-up lazyloaded img-lazyload" />
					</a>
					<div class="article__grid-meta article__grid-meta--has-image">
						<div class="wrap-blog-inner">
							<h2 class="h3 article__title">
								<a href="/en/news/${v.Slug}">${v.Title}</a>
							</h2>
							<span class="article__date">May 02, 2017</span>
							<div class="rte article__grid-excerpt text-justify">
								${v.TitleDescription}
							</div>
							<ul class="list--inline article__meta-buttons">
								<li><a href="/en/news/${v.Slug}">Read more</a></li>
							</ul>
						</div>
					</div>
				</div>
			</div>
	`);
	});
	 */

	if (data.length > 0) {
		$('.web-newsfeed-grid').empty();
		$.each(data, function (k, v) {
			//console.log(v);
			var num = v.CreatedOn.match(/\d+/g); //regex to extract numbers
			var date = new Date(parseFloat(num)); //converting to date
			var title = lang == "en" ? v.Title : v.TitleAr;
			var txt = lang == "en" ? v.TitleDescription : v.TitleDescriptionAr;
			if (txt.length > 200) {
				txt = txt.substring(0, 200) + ' ...';
			}
			$('.web-newsfeed-grid').append(`

			<div class="col-12 col-md-6 col-lg-4">
				<div class="wo-articles">
					<div class="wo-articles__head">
						<img src="/Assets/images/articles/info-circle.png" alt="img description">
						<div class="wo-articles-title">
							<h3 class="newsfeed-title" title="${title}">
								<a href="/${culture}/news/${v.Slug}">
									${title}
								</a>
							</h3>
						</div>
					</div>
					<div class="wo-description">
						<p class="text-justify">
							 ${txt}
						</p>
						<a class="text-capitalize ${lang == "en" ? "" : "float-right"}" href="/${culture}/news/${v.Slug}">
							${lang == "en" ? "Read More" : "قراءة المزيد"}
						</a>
					</div>
					<figure class="wo-articles__img">
						<img src="${v.BannerImage}" alt=" Ooops! image not found." title="${title}">
					</figure>
					<ul class="wo-articlesmeta ${lang == "en" ? "" : "justify-content-end"}">
						<li>
							<span>
								<i class="ti-calendar"></i>
								${/*<time datetime="${date.getFullYear()}-${date.getMonth() + 1}-${date.getDate()}">*/''}
								${date.toLocaleString('default', { month: 'long' })} ${date.getDate()} ${date.getFullYear()}
								</time>
							</span>
						</li>
					</ul>
				</div>
			</div>


		`);

		});
		$('.web-newsfeed-grid').append(`
				<div class="col-12">
					<div class="form-group wo-form-btn text-center">
						<a href="/${culture}/newsfeed" class="wo-btn">${lang == "en" ? "View All" : "مشاهدة الكل"}</a>
					</div>
				</div>
			`);
	}
	else {
		$('#web-newsfeed-grid').remove();
	}

}

function BindAppreciation(data) {

	if (data.length > 0) {
		$('.web-appreciation-slider').empty();
		$.each(data, function (k, v) {
			let msg = v.appreciation.replace(/(.{70})/g, "$1<br>");
			//console.log(v);
			$('.web-appreciation-slider').append(`
				<div class="wo-testimonials">
					<figure class="wo-testimonials__img">
						<img src="${v.image}" alt="img description" class="img-avatar">
					</figure>
					<div class="wo-sectionhead__title">
						<span>${ChangeString('Great Review From Customers', 'تعليق رائع من المالكين')}</span>
						<h2>${ChangeString('Appreciation', 'التقدير')}<em> ${ChangeString('That Matters', 'الذي يهم ')}</em></h2>
					</div>
					<blockquote class="wo-blockquote">
						<q class="wrap-anywhere">
							${msg}
						</q>
					</blockquote>
					<div class="wo-testimonials__name">
						<a href="javascript:void(0);">${v.name}</a>
					</div>
				</div>
			`);
		});

		/* Testimonial Slider */
		var _wo_testimonials_slider = jQuery(".wo-testimonials-slider")
		_wo_testimonials_slider.owlCarousel({
			loop: false,
			autoplay: false,
			items: 1,
			margin: 0,
			dots: true,
			autoplayTimeout: 5500,
			autoplaySpeed: 2000,
		});

		OnErrorImage(1);
	}
	else {
		$('#web-appreciation-slider').remove();
	}

}

function BindCars(data) {

	$('#NewFeaturedCars').empty();

	$.each(data, function (k, v) {
		var imagesTemplate = `<img src="${v.Thumbnail}" alt="img description" style="height:175px">`;
		var Images = v.Images.split(',');

		for (var i = 0; i < Images.length; i++) {
			imagesTemplate += `<img src="${Images[i]}" alt="img description" style="height:175px">`
		}

		var template = `<div class="col-lg-3 col-md-6 col-sm-12 my-2" dir="ltr">
							<div class="wo-topvehicles">
								<div class="wo-username">
									<img src="${v.VendorLogo}" alt="img description">
									<a href="javascript:void(0);">${v.VendorName}</a>
								</div>
								<div class="wo-vehiclesslider">
									<figure class="wo-vehicles-img wo-vehiclesimgslider owl-carousel">
										${imagesTemplate}
									</figure>
									<div class="wo-vehiclesimg-tags">
										<div class="wo-tags">
											${/*
											<a href="javascript:void(0);" class="wo-tag-featured">FEATURED</a>
											<a href="javascript:void(0);" class="wo-tag-rent">Rent</a>
											*/''}
											<a href="javascript:void(0);" class="wo-tag-photos">
												<i class="ti-image"></i> ${Images.length}
											</a>
										</div>
											${/*
												<a href="javascript:void(0);" class="wo-likepost">
													<i class="ti-heart"></i>
												</a>
											*/''}
									</div>
								</div>
								<div class="wo-vehicles">
									<div class="wo-vehicles__tags">
										<a href="javascript:void(0);">${v.Category}</a>
									</div>
									<div class="wo-vehicles__title">
										<h5>
											<a href="/${culture}/cars/${v.Slug}">
												${v.Title}
												${v.ModelYear}
											</a><span><sup>AED </sup>${v.Price} <em>${v.PackageName}</em></span>
										</h5>
									</div>
									<ul class="wo-vehicles__list">
										<li>
											<span>
												<i class="wo-themeicon">
                                                    <img src="/Assets/images/theme-icon/img-01.png" alt="img description">
												</i>${v.FuelEconomy}
											</span>
										</li>
										<li>
											<span>
												<i class="wo-themeicon">
                                                    <img src="/Assets/images/theme-icon/img-02.png" alt="img description">
												</i>${v.Transmission}
											</span>
										</li>


									</ul>
								</div>
								<div class="wo-postmeta">
									<address><i class="ti-location-pin"></i> ${v.VendorAddress}</address>
									${/*
									<a href="javascript:void(0);" class="wo-loadicon tippy">
										<i class="ti-reload"></i>
									</a>
									*/''}
								</div>
							</div>
						</div>`;

		$('#NewFeaturedCars').append(template);
	});

	var _wo_vehiclesimgslider = jQuery('.wo-vehiclesimgslider')
	_wo_vehiclesimgslider.owlCarousel({
		items: 1,
		loop: true,
		dots: false,
		nav: true,
		autoHeight: true,
		margin: 0,
		autoplay: false,
		navClass: ['wo-prev', 'wo-next'],
		navContainerClass: 'wo-bannernav wo-vehiclesnav',
		navText: ['<span class="ti-angle-left"></span>', '<span class="ti-angle-right"></span>'],
	});
}

//#endregion

//#region Functions for Binding Map

function SetCurrentLocation(lat, lon, adr) {
	var currentLocation = {
		latitude: lat,
		longitude: lon,
		address: adr,
	}
	localStorage.setItem("currentLocation", JSON.stringify(currentLocation));
}

function getLocation() {
	if (navigator.geolocation) {
		navigator.geolocation.getCurrentPosition(showPosition);
		IsGetCurrentLocation = true;
	} else {
		alert("Geolocation is not supported by this browser.");

		latitude = 25.2048
		longitude = 55.2708
		InsertPosition(latitude, longitude);
		InsertLatLonInInput(latitude, longitude);
	}
}

function showPosition(position) {

	latitude = position.coords.latitude;
	longitude = position.coords.longitude;

	InsertLatLonInInput(latitude, longitude);
	InsertPosition(latitude, longitude);

}

function InsertPosition(latitude, longitude) {
	$.ajax({
		type: 'Get',
		url: `https://maps.googleapis.com/maps/api/geocode/json?latlng=${latitude},${longitude}&key=AIzaSyAEmhLaFjth5xau57Gy1NwE1O6apk443xY`,
		success: function (response) {
			if (response.results.length > 0) {
				
				MapAddress = response.results[0].formatted_address;

				InsertMapAddressInInput(MapAddress);

				if (IsGetCurrentLocation) {
					SetCurrentLocation(latitude, longitude, MapAddress);
					IsGetCurrentLocation = false;
				}

			} else {
				alert("Geolocation is not supported by this browser.");
			}
		}
	});
}

function InsertLatLonInInput(latitude, longitude) {
	$('#latitude').val(latitude);
	$('#longitude').val(longitude);
}

function InsertMapAddressInInput(MapAddress) {
	$('#txtAddress').val(MapAddress);
	$('#current-location').val(MapAddress);
}


function myMap() {


	//#region Old Getting Latitude and Longitude Function

	//var Latlng;
	//const myLatLng = { lat: 25.2048, lng: 55.2708 };
	//const map = new google.maps.Map(document.getElementById("googleMap"), {
	//	zoom: 13,
	//	center: myLatLng,
	//});
	//// Create the initial InfoWindow.
	//let infoWindow = new google.maps.InfoWindow({
	//	content: "Select Location",
	//	position: myLatLng,
	//});
	//new google.maps.Marker({
	//	position: myLatLng,
	//	map,
	//	title: "Hello World!",
	//});
	//infoWindow.open(map);
	//// Configure the click listener.
	//map.addListener("click", (mapsMouseEvent) => {
	//	// Close the current InfoWindow.
	//	infoWindow.close();
	//	// Create a new InfoWindow.
	//	infoWindow = new google.maps.InfoWindow({
	//		position: mapsMouseEvent.latLng,
	//	});
	//	infoWindow.setContent(
	//		JSON.stringify(mapsMouseEvent.latLng.toJSON(), null, 2),
	//		Latlng = mapsMouseEvent.latLng,
	//	);
	//	infoWindow.open(map);
	//	$('#Latitude').val(Latlng.lat);
	//	$('#Longitude').val(Latlng.lng);
	//});

	//#endregion

	if (!latitude) { latitude = 25.2048 }
	if (!longitude) { longitude = 55.2708 }

	var map = new google.maps.Map(document.getElementById('googleMap'), {
		zoom: 15,
		center: new google.maps.LatLng(latitude, longitude),
		mapTypeId: google.maps.MapTypeId.ROADMAP
	});

	//var mImage = new google.maps.MarkerImage(DefaultMapMarker,
	//	new google.maps.Size(34, 35),
	//	new google.maps.Point(0, 10),
	//	new google.maps.Point(10, 34)
	//);

	var myMarker = new google.maps.Marker({
		position: new google.maps.LatLng(latitude, longitude),
		draggable: true,
		icon: DefaultMapMarker,
	});

	google.maps.event.addListener(myMarker, 'dragend', function (evt) {
		document.getElementById('drag-map').innerHTML = '<span>' + ChangeString('Drag marker on the map to select your desired location.', 'اسحب علامة على الخريطة لتحديد الموقع المطلوب.') + '</span>';

		latitude = evt.latLng.lat().toFixed(3);
		longitude = evt.latLng.lng().toFixed(3);

		InsertLatLonInInput(latitude, longitude);
		InsertPosition(latitude, longitude);

	});
	google.maps.event.addListener(myMarker, 'dragstart', function (evt) {
		document.getElementById('drag-map').innerHTML = '<span>' + ChangeString('Currently dragging marker ...', 'جارٍ سحب العلامة حاليًا ...') + '</span>';
	});
	map.setCenter(myMarker.position);
	myMarker.setMap(map);

	//#region getLocationMap()

	//function getLocationMap() {
	//	if (navigator.geolocation) {
	//		navigator.geolocation.getCurrentPosition(showPosition);
	//	} else {
	//		x.innerHTML = "Geolocation is not supported by this browser.";
	//	}
	//}

	//function showPosition(position) {
	//	document.getElementById('drag-map').innerHTML = '<span>Drag marker on the map to select your desired location.</span>';

	//	latitude = position.coords.latitude;
	//	longitude = position.coords.longitude;

	//	InsertLatLonInInput(latitude, longitude);
	//	InsertPosition(latitude, longitude);

	//	var myMarker = new google.maps.Marker({
	//		position: new google.maps.LatLng(latitude, longitude),
	//		draggable: true,
	//		icon:  DefaultMapMarker
	//	});
	//	google.maps.event.addListener(myMarker, 'dragend', function (evt) {
	//		document.getElementById('drag-map').innerHTML = '<span>Drag marker on the map to select your desired location.</span>';
	//		latitude = position.coords.latitude;
	//		longitude = position.coords.longitude;

	//		InsertLatLonInInput(latitude, longitude);
	//		InsertPosition(latitude, longitude);
	//	});
	//	google.maps.event.addListener(myMarker, 'dragstart', function (evt) {
	//		document.getElementById('drag-map').innerHTML = '<span>' + ChangeString('Currently dragging marker ...', 'جارٍ سحب العلامة حاليًا ...') + '</span>';
	//	});
	//	map.setCenter(myMarker.position);
	//	myMarker.setMap(map);
	//}

	//getLocationMap();

	//#endregion
}

function initAutocomplete() {

	var defaultBounds = new google.maps.LatLngBounds();
	var options = {
		types: ['(cities)'],
		bounds: defaultBounds
	};

	var inputs = document.getElementsByClassName('txtAddress');

	var autocompletes = [];

	for (var i = 0; i < inputs.length; i++) {
		var autocomplete = new google.maps.places.Autocomplete(inputs[i], options);
		autocomplete.inputId = inputs[i].id;
		autocomplete.addListener('place_changed', fillIn);
		autocompletes.push(autocomplete);
	}

	function fillIn() {

		var place = this.getPlace();


		latitude = place.geometry.location.lat();
		longitude = place.geometry.location.lng();

		InsertLatLonInInput(latitude, longitude);

		myMap();

		MapAddress = place.formatted_address;

		InsertMapAddressInInput(MapAddress);

	}

	//#region Single Id Bound

	//var defaultBounds = new google.maps.LatLngBounds();
	//var options = {
	//	types: ['(cities)'],
	//	bounds: defaultBounds
	//};

	//// get DOM's input element
	//var input = document.getElementById('txtAddress');

	//// Make Autocomplete instance
	//var autocomplete = new google.maps.places.Autocomplete(input, options);

	//// Listener for whenever input value changes
	//autocomplete.addListener('place_changed', function () {

	//	// Get place info
	//	var place = autocomplete.getPlace();

	//	// Do whatever with the value!
	//	latitude = place.geometry.location.lat();
	//	longitude = place.geometry.location.lng();
	//});

	//#endregion

	myMap();
}

function openMap() {


	if (!IsMapLoaded) {

		if (!latitude) {

			getLocation();
		}
		setTimeout(function () {
			myMap();
			$('.map-div-spin').hide();
			$('.map-div').show();
		}, 1000)
		IsMapLoaded = true;
	}
	else {
		myMap();
		InsertMapAddressInInput(MapAddress)
	}
	$('#map-modal').modal('show');
}

//#endregion