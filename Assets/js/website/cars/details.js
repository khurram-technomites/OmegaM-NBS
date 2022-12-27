'use strict';

//#region Global Variables and Arrays

var CarRating = { RatingCount: null, TotalRating: null };
var IsVaraiation = false;

var Order = {
	VendorID: null,
	CarID: null,
	PackageID: null,
	StartDate: null,
	StartTime: null,
	EndDate: null,
	EndTime: null,
	ExtraKilometers: null,
	ExtraKilometersPrice: null,
	DocumentAtPickUp: false,
	SelfPickUp: true,
	DeliveryCharges: 0,
	CouponCode: null,
	CouponDiscount: 0,
	PaymentMethod: 'Cash',
	DeliveryAddress: {
		Latitude: null,
		Longitude: null,
		Address: null
	}
};

var search;
var objCar;
var TotalPrice = 0;

var IsShippingAvailable = false;
var IsMinimumOrderExtended = false;
var IsCouponApplicable = true;
var IsTermAndConditionsAccepted = false;
var IsOutOfStock = true;
var grandFlag = true;
var grandTotal = +0;

var pg = 1;
var totalPages;
var filters = {
	CarID: 0,
	PageNumber: 1,
	PageSize: 3,
	SortBy: 1,
	Lang: lang,
}

var latitude;
var longitude;
var IsMapLoaded = false;
var MapAddress;

//#endregion

//#region document ready function
$(document).ready(function () {

	GetAndBindCarDetails();

	BindBookingPopupEvents();

	$('#load-more').click(function () {
		if (pg < totalPages) {
			pg++;
			$('#load-more').hide();
			$("#filter-loader").show();
			filters.PageNumber = pg;
			GetAndBindProductRating();
		}
	});

	BindOrderPunch();
});
//#endregion

//#region Ajax Call

function GetAndBindCarDetails() {
	$.ajax({
		type: 'GET',
		url: `/${culture}/cars/${GetURLParameter()}/details`,
		success: function (response) {
			if (response.success) {
				BindCarDetails(response.data);
			} else {
			}
		},
		error: function (e) {
		},
		failure: function (e) {
		}
	});
}

function GetAndBindProductRating() {
	filters.CarID = objCar.id;
	$.ajax({
		type: 'POST',
		url: '/' + culture + '/cars/ratings/filters',
		contentType: "application/json",
		data: JSON.stringify(filters),
		success: function (response) {
			if (response.success) {
				BindProductRatings(response.data);
			} else {
			}
		},
		error: function (e) {
		},
		failure: function (e) {
		}
	});
}

function GetAndBindVendorCars() {
	$.ajax({
		type: 'POST',
		url: `/${culture}/vendorcars/cars`,
		contentType: "application/json",
		data: JSON.stringify({
			vendorID: objCar.vendor.id,
			pageSize: 4
		}),
		success: function (response) {
			if (response.success) {
				BindVendorCars(response.data);
			} else {
				console.log("GET Vendor Cars Response Error.");
			}
		},
		error: function (e) {
			console.log("GET Error!");
		},
		failure: function (e) {
			console.log("GET Error!");
		}
	});
}

//#endregion

//#region Functions for Binding Data

function BindCarDetails(data) {
	objCar = data;

	GetAndBindVendorCars();
	GetAndBindProductRating();

	$('.vendor-address').append(`<i class="ti-location-pin"></i> ${data.vendor.address}</a>`);

	$('#Category').html(data.category.name);
	$('#Title').html(data.title);

	if (data.isFeatured) {
		$('.cd-feature').show();
	}

	$('#CarRating').find('i:lt(' + (data.rating) + ')').addClass("fill").removeClass("unfill");
	$('#NoOfRating').html(`(${data.noOfRatings})`)

	$("#LicensePlate").append(data.licenseplate);
	$("#Doors").append(data.door);
	$("#Cylinders").append(data.cylinders);
	$("#Capacity").append(data.capacity);

	BindCarImages(data.thumbnail, data.images);

	$("#ModelYear").html(data.year);
	$("#FuelEconomy").html(data.fuelEconomy);
	$("#Transmission").html(data.transmission);
	$("#HorsePower").html(data.hoursepower);

	if (data.specification && data.specification.length > 0) {
		$("#Specification").html(data.specification);
	} else {
		$("#Specification").closest('.wo-vsingledetails').remove();
	}

	if (data.cancelationpolicy && data.cancelationpolicy.length > 0) {
		$("#CancelationPolicy").html(data.cancelationpolicy);
	} else {
		$("#CancelationPolicy").closest('.wo-vsingledetails').remove();
	}

	if (data.insurance && data.insurance.description && data.insurance.description > 0) {
		$("#InsuranceDetails").html(data.insurance.description);
	} else {
		$("#InsuranceDetails").closest('.wo-vsingledetails').remove();
	}

	BindCarFeatures(data.features);

	BindCarTags(data.tags);

	BindVendor(data.vendor);

	BindCarPackages(data.packages);

	BindCouponRedeem();

	BindSearchResults();

	BindExtraKilometers()
	//CalulateTotal();

	$("*.skeleton").removeClass('skeleton').removeClass('skeleton-text').removeClass('skeleton-component').removeClass('skeleton-block').removeClass('skeleton-block-25');

}

function BindCarImages(thumbnail, images) {

	$('#wo-vsingleslider').empty();

	$('#wo-vsingleslider').append(`<figure><img style="max-height:410px;object-fit: cover;" src="${thumbnail}" alt="thumbnail"></figure>`);
	$.each(images, function (k, image) {
		$('#wo-vsingleslider').append(`<figure><img style="max-height:410px;object-fit: cover;" src="${image}" alt="thumbnail"></figure>`);
	});

	var _wo_vsingleslider = jQuery("#wo-vsingleslider")
	_wo_vsingleslider.owlCarousel({
		items: 1,
		loop: true,
		nav: true,
		autoplay: false,
		dots: false,
		smartSpeed: 500,
		responsiveClass: true,
		navClass: ['wo-prev', 'wo-next'],
		navContainerClass: 'wo-bannernav wo-vehiclesnav',
		navText: ['<span class="ti-angle-left"></span>', '<span class="ti-angle-right"></span>'],
	});

	setTimeout(function () { OnErrorImage(); }, 3000);
}

function BindCarFeatures(features) {

	$('#features').empty();
	$.each(features, function (k, feature) {
		var featureArr = feature.split('|');
		if (featureArr.length == 2) {
			$('#features').append(`<li>
                                   <div class="wo-featureslistcontent">
                                       <strong style="text-align: center;line-height: 40px;">
                                           <img src="${featureArr[1]}" class="object-fit-cover" width="20" height="20" alt="img feature" style="border-radius: 50%;width: 40px;height: 40px;">
                                            ${featureArr[0]}
                                       </strong>
                                       <span></span>
                                   </div>
                               </li>`);
		}
	});
	setTimeout(function () { OnErrorImage(); }, 3000);
}

function BindCarTags(tags) {
	if (tags.length > 0) {

		$.each(tags, function (k, tag) {
			$('#CarTags').append(`<a href="javascript:void(0);">${tag}</a>`);

		});

		setTimeout(function () { OnErrorImage(); }, 3000);
	} else {
		$('#CarTags').hide();
	}
}

function BindVendor(vendor) {
	$('.vendor-image').attr('src', vendor.image);
	$('.vendor-name').html(vendor.name);
	$('.vendor-address').html(`<i class="ti-location-pin"></i> ${vendor.address} <a href="https://www.google.com/maps/search/?api=1&query=${vendor.latitude},${vendor.longitude}">(${ChangeString('Directions', 'الاتجاهات')})</a>`);
	$('.vendor-number').html(`<em class="wo-hidenum">***** - ***</em><em class="wo-shownum">${vendor.contact}</em>`);
}

function BindCarPackages(packages) {

	if (packages.length > 0) {

		$.each(packages, function (k, pkg) {
			$('#CarPackages').append(`<li class="justify-content-between">
                                     <span>Per ${pkg.PackageName.replace('ly', '').replace('Dai', 'Day')} :</span>
                                     <span><sub class="small-sup-BD">AED</sub> <b>${pkg.Price}</b> <sub class="small-sup-BD">(${pkg.availableKilometer} km)</sup></span>
                                 </li>`);
			if (k + 1 < packages.length) {
				$('#CarPackages').append(` <li class="separator">
                                        <hr />
                                    </li>`);
			}
		});

		setTimeout(function () { OnErrorImage(); }, 3000);
	} else {
		$('#CarPackages').empty();
	}
}

function BindProductRatings(data) {
	console.log(data);
	if (data.length > 0) {

		//detail
		var rating = parseFloat(data[0].TotalRating);

		var avg = rating / data[0].NoOfRatings;
		var stars = Math.max(0, (Math.min(5, avg))) * 20;

		$('.total-rating').html(`${avg.toFixed(1)} / <em>5</em>`);
		$('.rating-car-stars').css('width', stars + '%');
		$('.total-stars').css('width', stars + '%');
		$('.total-user-rating').html(`Based on ${data[0].NoOfRatings} Review${data[0].NoOfRatings > 1 ? "s" : ""}`);
		//$('#reviews-list').find('h3').html(`${data[0].NoOfRatings} Reviews`);

		$.each(data, function (k, v) {
			var obj = $('#reviews-list').find('ul').find('li');

			var stars = Math.max(0, (Math.min(5, v.rating))) * 20;
			$('#reviews-list').find('ul').append(`
				<li>
					<div class="wo-comment">
						<figure class="wo-comment__img">
							<img src="${v.customer.image}" alt="image-description">
						</figure>
						<div class="wo-comment__content">
							<div class="wo-comment-title">
								<span class="wo-stars"><span style="width: ${stars}%;"></span></span>
								<h4>${v.customer.name}</h4>
								<span>${v.date}</span>
							</div>
							<div class="wo-comment-description">
								<p>${v.remarks}</p>
							</div>
						</div>
					</div>
				</li>
			`);
		});
		RenderPagination(data[0].TotalRecords, data[0].filteredRecords);
	}
	else {
		$('.total-rating').html(ChangeString(`0 / <em>5</em>`, `0 / <em>5</em>`));
		$('.rating-car-stars').css('width', 0 + '%').closest('.wo-stars').hide();
		$('.total-stars').css('width', 0 + '%');
		$('.total-user-rating').html(ChangeString('No Reviews Yet', 'لا توجد تعليقات حتى الآن'));

		RenderPagination(0, 0);
	}

}

function BindVendorCars(data) {
	$('#VendorCars').empty();

	$.each(data, function (k, v) {

		if (v.ID != objCar.id) {
			if (k == 3) { return false; }

			var imagesTemplate = `<img src="${v.Thumbnail}" alt="img description" style="height:247px">`;
			var Images = v.Images.split(',');

			for (var i = 0; i < Images.length; i++) {
				imagesTemplate += `<img src="${Images[i]}" alt="img description" style="height:247px">`;
			}

			var template = `<div class="col-md-6 col-xl-4" dir="ltr">
							<div class="wo-topvehicles">
								<div class="wo-username vendor-info">
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
												<a href="javascript:void(0);" class="wo-tag-featured">${ChangeString('Featured', 'متميز')}</a>
												<a href="javascript:void(0);" class="wo-tag-rent">${ChangeString('Rent', 'تأجير')}</a>
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
		}


		$('#VendorCars').append(template);
	});
	$('.vendor-cars').show();

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

	if (!$("#VendorCars").html().trim()) {
		$('.vendor-cars').remove();
	}
}

//#endregion

//#region Others Function


function BindExtraKilometers() {
	$('#ExtraKilometers').change(function () {

		if ($('#ExtraKilometers').val() && Number($('#ExtraKilometers').val()) > 0) {

			Order.ExtraKilometers = Number($('#ExtraKilometers').val());
			Order.ExtraKilometersPrice = Number($('#ExtraKilometers').val()) * objCar.pricePerKilometer;

		} else {
			Order.ExtraKilometers = 0
			Order.ExtraKilometersPrice = 0
		}
		BindCartTotal();
	});
}

function BindCouponRedeem() {
	$('#btnApplyCoupon').click(function () {
		if ($('#CouponCode').val()) {
			$('#btnApplyCoupon').addClass('applied');
			$.ajax({
				url: '/' + culture + '/coupons/redeem',
				type: 'POST',
				data: {
					__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
					coupon: {
						CouponCode: $('#CouponCode').val()
					}
				},
				success: function (response) {
					if (response.success) {
						if (response.categories.length > 0) {
							CheckCouponCategories(response);
						} else {
							IsCouponApplicable = true;

							$(this).val('');

							var Discount;
							if (response.Type && response.Type == "Percentage") {
								Discount = (parseFloat(TotalPrice) * response.Value) / 100;
								Discount = Discount > response.MaxAmount ? response.MaxAmount : Discount;
							} else {
								Discount = response.Value;
							}

							var Subtotal = parseFloat(TotalPrice);
							if (Discount > Subtotal) {
								Discount = Subtotal;
							}

							$('#Discount').html(`${Discount}`);

							$('#CouponCode').removeClass('border-danger').addClass('border-success');
							$('#coupon-message').html(`Congratulations, you saved ${currency} ${Discount} 🎉`).removeClass('text-danger').addClass('text-success');

							Order.CouponCode = $('#CouponCode').val();
							Order.CouponDiscount = Discount;
							BindCartTotal();
						}
					}
					else {
						IsCouponApplicable = false;
						$('#CouponCode').removeClass('border-success').addClass('border-danger');
						$('#coupon-message').html(response.message).removeClass('text-success').addClass('text-danger');
					}
					$('#btnCancelCoupon').show();
					$('#btnApplyCoupon').hide();
					CheckCheckoutValidity();
				}
			})
		} else {
			$('#CouponCode').focus()
			$('#btnApplyCoupon').removeClass('applied');
		}
	});

	$('#btnCancelCoupon').click(function () {

		$('#btnCancelCoupon').hide();
		$('#btnApplyCoupon').show();

		$('#Discount').html('0');
		Order.CouponCode = null;
		Order.CouponDiscount = 0;

		$('#CouponCode').removeClass('border-danger').removeClass('border-success');
		$('#CouponCode').val('');
		$('#coupon-message').html('');

		$('.coupon-badge').html('').hide();
		$('#btnApplyCoupon').removeClass('applied');

		IsCouponApplicable = true;
		BindCartTotal();
		CheckCheckoutValidity();

	});
}

function BindCartTotal() {

	var RentalFee = TotalPrice;
	$('#RentalFee').html(RentalFee);
	if (grandFlag) {
		grandTotal = +Number(RentalFee).toFixed(2);
		grandFlag = false;
	}

	$('#DeliveryCharges').html(Number(Order.DeliveryCharges).toFixed(2));
	RentalFee += Order.DeliveryCharges;

	$('#ExtraKilometersPrice').html(Number(Order.ExtraKilometersPrice).toFixed(2));
	RentalFee += Order.ExtraKilometersPrice;

	$('#Subtotal').html(Number(RentalFee).toFixed(2));
	RentalFee -= Order.CouponDiscount;

	$('#Total').html(Number(RentalFee).toFixed(2));
}

function CheckCheckoutValidity() {
	//if (IsShippingAvailable && IsMinimumOrderExtended && IsCouponApplicable && IsTermAndConditionsAccepted && !IsOutOfStock) {
	//    $('#btnOrder').prop('disabled', false);
	//} else {
	//    $('#btnOrder').prop('disabled', true);
	//}
}

function BindSearchResults() {

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
		Order.PackageID = $(this).attr('data');

		localStorage.setItem("search", JSON.stringify(search));

		PopulateSchedule();
	});

	$('#StartDate,#EndDate').change(function (event) {
		event.preventDefault()
		PopulateSchedule();
	});

	$('#StartTime,#EndTime').change(function (event) {
		event.preventDefault()
		CalulateTotal();
	});

	PopulateSchedule();
}

function CalulateTotal() {

	search.startDate = $("#StartDate").val();
	search.endDate = $("#EndDate").val();
	search.startTime = $("#StartTime").val();
	search.endTime = $("#EndTime").val();
	localStorage.setItem("search", JSON.stringify(search));

	var startDateTime = new Date(search.startDate + " " + search.startTime);
	var endDateTime = new Date(search.endDate + " " + search.endTime);

	var diffInMilliSeconds = Math.abs(endDateTime - startDateTime) / 1000;

	// calculate days
	var days = Math.floor(diffInMilliSeconds / 86400);
	diffInMilliSeconds -= days * 86400;

	// calculate hours
	var hours = Math.floor(diffInMilliSeconds / 3600) % 24;
	diffInMilliSeconds -= hours * 3600;

	// calculate minutes
	const minutes = Math.floor(diffInMilliSeconds / 60) % 60;
	diffInMilliSeconds -= minutes * 60;

	var difference = '';
	if (days > 0) {
		difference += (days === 1) ? `${days}d ` : `${days}d `;
	}

	if (hours > 0) {
		difference += (hours === 0 || hours === 1) ? `${hours}hr ` : `${hours}hr `;
	}
	if (minutes > 0) {
		difference += (minutes === 0 || hours === 1) ? `${minutes}min` : `${minutes}min`;
	}

	$('#BookingDuration').html(difference);


	var PackageName = $('.car-packages .package.selected').text().trim();

	var Package = objCar.packages.filter(function (obj) {
		return obj.PackageName === PackageName
	})

	var Price = 0;
	if (Package.length > 0) {
		switch (PackageName) {
			case "Hourly":
				Price = Package[0].Price * hours;
				break;
			case "Daily":
				Price = Package[0].Price * days;
				break;
			case "Weekly":
				Price = Package[0].Price * (days / 7);
				break;
			case "Monthly":
				Price = Package[0].Price * (days / 30);
				break;
			default:
				break;
		}
	}

	TotalPrice = Price
	$('#BookingTotal').html(`<sup class="small-sup-BD">AED </sup> ${Price.toFixed(2)}`);

	BindCartTotal();
}

function RenderPagination(totalRecord, filteredRecord) {

	totalPages = Math.ceil(filteredRecord / filters.PageSize);

	$("#filter-loader").hide();

	if (!totalRecord) {
		$("#load-more").fadeOut();
	}
	else if (pg >= totalPages) {
		$("#load-more").fadeOut();
	}
	else {
		$("#load-more").fadeIn();
	}

}

function BindBookingPopupEvents() {
	$('#chkDocumentsAtPickup').change(function () {
		if (this.checked) {
			Order.DocumentAtPickUp = true;
			$(".document").addClass('disabled');
			//$(".documents-container").slideUp();
		} else {
			Order.DocumentAtPickUp = false;
			$(".document").removeClass('disabled');
			// $(".documents-container").slideDown();
		}
	});

	$('#chkDocumentsAtPickup').trigger("change");

	$('#chkSelfPickup').change(function () {
		if (this.checked) {

			$('#PaymentMethodCash ~span').html(ChangeString(`Pay on Pickup`, `دفع على بيك اب`));
			$('.documents-at-pickup-heading').html(ChangeString(`Submit at Pickup`, `إرسال عند الاستلام`));

			$("#ProvidorLocation").slideDown();
			$("#CustomerLocation").slideUp();

			Order.SelfPickUp = true;

			Order.DeliveryCharges = 0;
			BindCartTotal();
		} else {

			$('#PaymentMethodCash ~span').html(ChangeString(`Pay on Delivery`, `الدفع عند الاستلام`));
			$('.documents-at-pickup-heading').html(ChangeString(`Submit at Delivery`, `إرسال عند التسليم`));

			$("#ProvidorLocation").slideUp();
			$("#CustomerLocation").slideDown();

			Order.SelfPickUp = false;

			if (objCar.deliveryChargesType == "PerKilometer") {
				var distance = 1;

				distance = CalculateDistance(objCar.vendor.latitude, objCar.vendor.longitude, Order.DeliveryAddress.Latitude, Order.DeliveryAddress.Longitude);

				Order.DeliveryCharges = objCar.deliveryChargesAmount * distance;
			} else {
				Order.DeliveryCharges = objCar.deliveryChargesAmount
			}

			BindCartTotal();
		}
	});

	$('#chkSelfPickup').trigger("change");

	$('.payment-method').click(function () {
		$('.payment-method').removeClass('selected');
		$(this).addClass('selected');
	});

	$('#btnBooking').click(function () {
		openMap();
		$('#Bookingpopup').modal({}, 'show');
	});

}

function FillOrder() {

	Order.VendorID = objCar.vendor.id;
	Order.CarID = objCar.id;
	Order.PackageID = $('.car-packages .package.selected').attr('data');

	// Order.StartDate = new Date($('#StartDate').val());
	Order.StartDate = $('#StartDate').val();

	Order.StartTime = $('#StartTime').val();

	//Order.EndDate = new Date($('#EndDate').val());
	Order.EndDate = $('#EndDate').val();

	Order.EndTime = $('#EndTime').val();
	Order.PaymentMethod = $('input[name=PaymentMethod]:checked').val();

	if (Order.SelfPickUp) {
		Order.DeliveryAddress.Latitude = objCar.vendor.latitude;
		Order.DeliveryAddress.Longitude = objCar.vendor.longitude;
		Order.DeliveryAddress.Address = objCar.vendor.address
	} else {

		Order.DeliveryAddress.Latitude = $("#latitude").val();
		Order.DeliveryAddress.Longitude = $("#longitude").val();
		Order.DeliveryAddress.Address = $("#txtAddress").val();
	}
}

function BindOrderPunch() {

	$("#btnOrder").on("click", function (event) {

		try {
			$("#btnOrder").html('<i class="fa fa-circle-notch fa-spin"></i> Proceed To Payment').prop('disabled', true);
			FillOrder();
			var $this = $(this);

			if (Order.DocumentAtPickUp === false && $('.document.uploaded').length < 3) {
				$('.document:not(.uploaded)').addClass('error');
				$('#message').html('Please upload docments').addClass('error').slideDown();
				$("#btnOrder").html('Proceed To Payment').prop('disabled', false);
				setTimeout(function () {
					$('.document:not(.uploaded)').removeClass('error');
				}, 1000);

				return;
			}

			if (!IsCouponApplicable) {
				$('#CouponCode').addClass('error');
				$('#message').html($('#coupon-message').text()).addClass('error').slideDown();
				$("#btnOrder").html('Proceed To Payment').prop('disabled', false);
				setTimeout(function () {
					$('#CouponCode').removeClass('error');
				}, 1000);

				return;
			}

			$.ajax({
				url: '/' + culture + '/Orders/Create',
				type: 'post',
				data: { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(), orderViewModel: Order },
				success: function (response) {
					if (response.success) {

						$("html, body").animate({ scrollTop: 0 }, 1000);
						if (Order.PaymentMethod === "Card") {
							InitiatePayment(response);
						}
						else {

							var EmptyCartTemplate = `<div class="row">
															<div class="col-12 col-sm-12 col-md-12 col-lg-12" style="margin: 6% auto">
        														<div class ="empty-page-content text-center" style="padding: 40px;">
																	<img src="/Assets/images/order/success.png" />
																	<h5 class="mt-2">${ChangeString('Order #', 'الطلب رقم #')} ${response.order.orderNo}</h5>
																	<h4 class="mt-2">${ChangeString('Your Booking has been Placed Successfully', 'تم وضع الحجز الخاص بك بنجاح')}</h4>
														            <p class="mt-2">
																		<a href="/${culture}/Home" class="btn wo-btn" style="text-decoration-line: unset;">
																			${ChangeString('Back To Home', 'العودة إلى المنزل')}
																			<i class="fa fa-caret-right" aria-hidden="true"></i>
																		</a>
																	</p>
														          </div>
        													</div>
        												</div>`;

							$("#checkout-container").html(EmptyCartTemplate);
						}
					}
					else {
						$("#btnOrder").html('Proceed To Payment').prop('disabled', false);
						$('#message').html(response.message).addClass('error').slideDown();
						return false;
					}
				}
			})
				.done(function (e) {
					console.log(e);
				})
				.fail(function (e) {
					if (e.status === 403) {
						window.location = er.responseJSON.LogOnUrl;
					}
				});
			event.preventDefault();
		}
		catch (ex) {
			$("#btnOrder").html('Proceed To Payment').prop('disabled', false);
			$('#message').html('Unable to process your order').addClass('error').slideDown();
			return false;
		}
	});
}

function InitiatePayment(response) {
	var EmptyCartTemplate = `<div class="row">
								<div class="col-12 col-sm-12 col-md-12 col-lg-12" style="margin: 6% auto">
        							<div class="empty-page-content text-center" style="padding: 40px;">
										<img src="/Assets/images/order/success.png" />
										<h5 class="mt-2">${ChangeString('Order #', 'الطلب رقم #')} ${response.order.orderNo}</h5>
                                        <h4 class="mt-2">${ChangeString('Booking has been Placed, processing for payment.', 'تم إجراء الحجز ، وتجهيزه للدفع.')}</h4>
							            <span class="fa fa-circle-notch fa-spin"></span>
							          </div>
        						</div>
        					</div>`;

	$("#checkout-container").html(EmptyCartTemplate);

	$.ajax({
		url: '/' + culture + '/Customer/Payment/Initiate/' + response.order.id,
		type: 'Put',
		data: { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(), viewModel: Order },
		success: function (result) {
			if (result.success) {
				if (result.redirectUrl) {
					window.location = result.redirectUrl;
				}
			} else {
				EmptyCartTemplate = `<div class="row">
										<div class="col-12 col-sm-12 col-md-12 col-lg-12" style="margin: 6% auto">
        									<div class="empty-page-content text-center"  style="padding: 40px;">
												<h5 class="mt-2">${ChangeString('Order #', 'الطلب رقم #')}  ${response.order.orderNo}</h5>
									            <p class="mt-2">
													${ChangeString('Booking payment processing failed, please try again.', 'تم إجراء الحجز ، وتجهيزه للدفع.')}
													</br>
													${ChangeString('If the error persists contact our support.', 'إذا استمر الخطأ ، فاتصل بدعمنا.')}
												</p>
									          </div>
        								</div>
        							</div>`;

				$("#checkout-container").html(EmptyCartTemplate);
			}
		}
	})
		.fail(function (e) {
			EmptyCartTemplate = `<div class="row">
										<div class="col-12 col-sm-12 col-md-12 col-lg-12" style="margin: 6% auto">
        									<div class="empty-page-content text-center p-2">
												<h5 class="mt-2">${ChangeString('Order #', 'الطلب رقم #')} ${response.order.orderNo}</h5>
									            <p class="mt-2">
													${ChangeString('Booking payment processing failed, please try again.', 'تم إجراء الحجز ، وتجهيزه للدفع.')}
													</br>
													${ChangeString('If the error persists contact our support.', 'إذا استمر الخطأ ، فاتصل بدعمنا.')}
												</p>
									          </div>
        								</div>
        							</div>`;

			$("#checkout-container").html(EmptyCartTemplate);
		});
	event.preventDefault();
}

function GetURLParameter() {
	return $("#ProductID").val();
}

function timeDiffCalc(dateFuture, dateNow) {
	let diffInMilliSeconds = Math.abs(dateFuture - dateNow) / 1000;

	// calculate days
	const days = Math.floor(diffInMilliSeconds / 86400);
	diffInMilliSeconds -= days * 86400;

	// calculate hours
	const hours = Math.floor(diffInMilliSeconds / 3600) % 24;
	diffInMilliSeconds -= hours * 3600;

	// calculate minutes
	const minutes = Math.floor(diffInMilliSeconds / 60) % 60;
	diffInMilliSeconds -= minutes * 60;

	let difference = '';
	if (days > 0) {
		difference += (days === 1) ? `${days} d ` : `${days} d `;
	}

	if (hours > 0) {
		difference += (hours === 0 || hours === 1) ? `${hours} hr ` : `${hours} hr `;
	}
	if (minutes > 0) {
		difference += (minutes === 0 || hours === 1) ? `${minutes} min` : `${minutes} min`;
	}
	return difference;
}

function CalculateDistance(lat1, long1, lat2, long2) {

	return 1;
}

//#endregion


//#region Functions for Binding Map

function getLocation() {
	if (navigator.geolocation) {
		navigator.geolocation.getCurrentPosition(showPosition);
	} else {
		alert("Geolocation is not supported by this browser.");
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
				$('#txtAddress').val(response.results[0].formatted_address);
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
	$('.txtAddress').val(MapAddress);
}

function myMap() {

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

	//var defaultBounds = new google.maps.LatLngBounds();
	//var options = {
	//    types: ['(cities)'],
	//    bounds: defaultBounds
	//};

	//var inputs = document.getElementsByClassName('txtAddress');

	//var autocompletes = [];

	//for (var i = 0; i < inputs.length; i++) {
	//    var autocomplete = new google.maps.places.Autocomplete(inputs[i], options);
	//    autocomplete.inputId = inputs[i].id;
	//    autocomplete.addListener('place_changed', fillIn);
	//    autocompletes.push(autocomplete);
	//}

	//function fillIn() {

	//    var place = this.getPlace();

	//    latitude = place.geometry.location.lat();
	//    longitude = place.geometry.location.lng();

	//    InsertLatLonInInput(latitude, longitude);

	//    myMap();

	//    MapAddress = place.formatted_address;

	//    InsertMapAddressInInput(MapAddress);

	//}

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

		// Get place info
		var place = autocomplete.getPlace();

		// Do whatever with the value!
		latitude = place.geometry.location.lat();
		longitude = place.geometry.location.lng();

		InsertLatLonInInput(latitude, longitude);

		myMap();

		MapAddress = place.formatted_address;

		InsertMapAddressInInput(MapAddress);

	});

	//#endregion

	//myMap();
}

function openMap() {

	if (!IsMapLoaded) {
		if (!latitude) {
			getLocation();
		}
		setTimeout(function () {
			myMap();
		}, 1000)
		IsMapLoaded = true;
	}
	else {
		myMap();
		InsertMapAddressInInput(MapAddress)
	}
}

//#endregion