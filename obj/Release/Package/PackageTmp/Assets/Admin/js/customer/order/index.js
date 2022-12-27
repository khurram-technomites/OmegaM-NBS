'use strict';

//#region Global Variables and Arrays
//var IsOrderLoaded = false;

var pg = 1;
var PageSize = 20;
var isPageRendered = false;
var FilteredRecord = 20;
var totalPages;
var filter = {
	Status: '',
	ShipmentStatus: '',
	PageNumber: 1,
	SortBy: 1,
	Lang: lang,
}

var Car = {
	//RowId: CarID + '-' + 1,
	RowId: null,
	CarID: null,
	Slug: null,
	Title: null,
	Thumbnail: null,
	Quantity: 1,
	UnitPrice: null,
	Price: null,
	VendorID: null,
	CustomNote: null,
	CarVaraiationID: null,
	Attributes: [],
	IsInStock: true
};
//#endregion

//#region document ready function
$(document).ready(function () {

	$('.table-filter-status').change(function () {
		pg = 1;
		GetFilterValues();
		GetBookings();
	});

	$('.table-filter-sort-by').change(function () {
		pg = 1;
		GetFilterValues();
		GetBookings();
	});

});
//#endregion

//#region Bind Response

function BindBookings(response) {
	console.log(response);
	if (response.data.length > 0) {
		//$("#bookings-tbody").empty();
		//$('.table-filter-result-count').html(`${ChangeString(`About ${response.data[0].filteredRecords} search result founds.`, `تم العثور على حوالي ${response.data[0].filteredRecords} نتيجة بحث.`)}`);
		$("#bookings-tbody").empty();
		
		$.each(response.data, function (k, v) {

			var amount = '-';
			var status_type = {
				"Pending": { 'color': 'wo-bg-warning' },
				"Confirmed": { 'color': 'wo-bg-light-purple' },
				"Processing": { 'color': 'wo-bg-warning' },
				"Completed": { 'color': 'wo-bg-purple' },
				"Canceled": { 'color': 'wo-bg-danger' },
				"-": { 'color': 'wo-bg-grey' }
			}
			if (v.TotalAmount) {
				amount = `${v.Currency} ${v.TotalAmount}`;
			}
			try {
				status_type[v.Status].color;
				if (!v.Status) { v.Status = '-' }
			} catch (e) {
				v.Status = 'Canceled';
			}

			$("#bookings-tbody").append(`
				<tr>
					<td data-label="${ChangeString('Data', 'تاريخ')}">${v.Date}</td>
					<td data-label="${ChangeString('Booking No', 'رقم الحجز')}">${v.OrderNo}</td>
					<td data-label="${ChangeString('Amount', 'مقدار')}">${amount}</td>
					<td data-label="${ChangeString('Status', 'حالة')}">
                        <span class="badge ${status_type[v.Status].color} flex-app justify-content-center wo-titleinput p-1">${v.Status}</span>
                    </td>
					${/*<td data-label="Action"><a class="" href="/${culture}/Customer/Order/Details/${v.ID}">View</a></td>*/''}
					<td data-label="${ChangeString('Action', 'عمل')}">
                        <ul>
							<li><a href="/${culture}/Customer/Order/Details/${v.ID}"><span class="ti-folder"></span> ${ChangeString('View', 'رأي')}</a></li>
                            ${/*
							 * <li><a href="javascript:void(0);"><span class="ti-blackboard"></span></a></li>
							 * <li><a href="javascript:void(0);"><span class="ti-printer"></span></a></li>
							*/''}
                        </ul>
                    </td>
				</tr>
			`);
		});

		//RenderPagination(response.data[0].TotalRecords, response.data[0].filteredRecords);
		FilteredRecord = response.data[0].filteredRecords;
		RenderPagination(response.data[0].TotalRecords, response.data[0].filteredRecords);
	}
	else {
		$("#bookings-tbody").empty();
		$("#bookings-tbody").html(`<tr><td colspan="5" class="text-center p-4">
                                            <span class="alert alert-dark p-2">${ChangeString('No record found', 'لا يوجد سجلات')}</span>
                                </td></tr>`);
		FilteredRecord = 0;
		RenderPagination(0, 0);
	}

	$("#bookings-body").show();
	$("#bookings-body-preload").hide();

	//if (!$("#bookings-tbody").html().trim()) {
	//	//alert('No data found');
	//	//$(".filter-loader").hide();
	//	//$("#load-more").hide();
	//	$("#bookings-tbody").html('<tr><td colspan="5" class="text-center p-4"><span class="alert alert-dark p-2">No data found</span></td></tr>');
	//} else {
	//	//$('.total-amount').each(function () {
	//	//	$(this).html(kFormatter(Number($(this).text()))).removeClass('total-amount');
	//	//});
	//}

	//FormatPrices();
}

//#endregion

//#region Others Function
function LoadBookings() {
	if (!isPageRendered) {
		pg = 1;
		GetFilterValues();
		GetBookings();
		isPageRendered = true;
	}
}

function GetBookings() {

	$.ajax({
		type: 'POST',
		url: '/' + culture + '/Customer/Order/List',
		contentType: "application/json",
		data: JSON.stringify(filter),
		success: function (response) {
			BindBookings(response);
		},
		error: function (e) {
			console.log("Get Bookings Error.");
		}
	});
}

function GetFilterValues() {
	//filter.Status = $("#Status").val();
	//filter.ShipmentStatus = $("#ShipmentStatus").val();

	filter.Status = $(".table-filter-status").val();
	filter.SortBy = $(".table-filter-sort-by").val();
	filter.PageNumber = pg;
}

function RenderPaginations(totalRecord, filteredRecord) {

	totalPages = Math.ceil(filteredRecord / PageSize);

	$('.ul-pagination').hide();
	$(".ul-pagination").empty();

	$('.ul-pagination').append(`<li class="wo-prevpage">
                                    <a href="javascript:void(0);" class="pagination-prv${pg == 1 ? '-disabled a-disabled-hover' : ''}" 
                                        onclick="${pg == 1 ? '' : 'PaginationPre()'}">${ChangeString('PRE', 'سابق')}</a>
                                </li>`);

	for (var i = 1; i <= totalPages; i++) {
		$('.ul-pagination').append(`<li class="${pg == i ? 'wo-active' : ''}">
                                        <a href="javascript:void(0);" class="pagination-count" onclick="PaginationCount(${i})">${i}</a>
                                    </li>`);
	}
	$('.ul-pagination').append(`<li class="wo-nextpage"><a href="javascript:void(0);" class="pagination-nex${pg == totalPages ? '-disabled a-disabled-hover' : ''}"
                                        onclick="${pg == totalPages ? '' : 'PaginationNex()'}">${ChangeString('NEX', 'التالي')}</a></li>`);
	$('.ul-pagination').show();
	$('.ul-pagination').show();

	//$(".filter-loader").hide();
	//if (pg >= totalPages) {
	//	$("#load-more").fadeOut();
	//} else {
	//	$("#load-more").fadeIn();
	//}

}

function RenderPagination(totalRecord, filteredRecord) {

	totalPages = Math.ceil(filteredRecord / PageSize);
	totalPages = (!totalPages && totalPages == 0) ? 1 : totalPages;

	if (FilteredRecord > 0) {
		if (FilteredRecord > PageSize) {
			$('.booking-result-count').text(`${ChangeString('Showing', 'عرض')} ${(((pg - 1) * PageSize) + 1)}–${pg * PageSize} ${ChangeString('of', 'من')} ${FilteredRecord} ${ChangeString('results', 'النتائج')}`);
		} else {
			$('.booking-result-count').text(`${ChangeString('Showing', 'عرض')} ${FilteredRecord} ${ChangeString('results', 'النتائج')}`);
		}
	} else {
		$('.booking-result-count').text(`${ChangeString('Showing', 'عرض')} 0 ${ChangeString('results', 'النتائج')}`);
	}

	$(".ul-pagination").html('');
	if (totalPages > 1) {
		$(".ul-pagination").append(`<li class="wo-prevpage"><a id="btn-pagination-prev" href="javascript:void(0);"class="pagination-prv${pg == 1 ? '-disabled a-disabled-hover' : ''}" onclick="${pg == 1 ? '' : 'PaginationPre()'}">${ChangeString('PRE', 'سابق')}</a></li>`);
	}

	for (var i = 1; i <= totalPages; i++) {
		if (i == pg) {
			$(".ul-pagination").append(`<li class="wo-active"><a href="javascript:void(0);" class="pagination-count" onclick="PaginationCount(${i})">${i}</a></li>`);
		} else {
			$(".ul-pagination").append(`<li><a href="javascript:void(0);" class="pagination-count" onclick="PaginationCount(${i})">${i}</a></li>`);
		}
	}

	if (totalPages > 1) {
		$(".ul-pagination").append(`<li class="wo-nextpage"><a  id="btn-pagination-next" href="javascript:void(0);" class="pagination-nex${pg == totalPages ? '-disabled a-disabled-hover' : ''}"
                                        onclick="${pg == totalPages ? '' : 'PaginationNex()'}">${ChangeString('NEX', 'التالي')}</a></li>`);
	}

	stylePagination();
	//animatePagination();
}
//#region pagination
function updatePagination(pageNumber) {
	if (FilteredRecord > 0) {
		if (FilteredRecord > PageSize) {
			$('.booking-result-count').text(`${ChangeString('Showing', 'عرض')} ${(((pg - 1) * PageSize) + 1)}–${pg * PageSize} ${ChangeString('of', 'من')} ${FilteredRecord} ${ChangeString('results', 'النتائج')}`);
		} else {
			$('.booking-result-count').text(`${ChangeString('Showing', 'عرض')} ${FilteredRecord} ${ChangeString('results', 'النتائج')}`);
		}
	} else {
		$('.booking-result-count').text(`${ChangeString('Showing', 'عرض')} 0 ${ChangeString('results', 'النتائج')}`);
	}

	stylePagination();
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

	if ($(".ul-pagination li:nth-child(2)").hasClass('wo-active')) {
		$('#btn-pagination-prev').hide();
	} else {
		$('#btn-pagination-prev').show();
	}

	if ($(".ul-pagination li:nth-last-child(2)").hasClass('wo-active')) {
		$('#btn-pagination-next').hide();
	} else {
		$('#btn-pagination-next').show();
	}

	if ($(".ul-pagination li").length > 6) {
		$(".ul-pagination li:not(:first):not(:last):not(:nth-last-child(2)):not(:nth-child(2))").hide();

		var $active = $(".ul-pagination .wo-active");
		var activeIndex = $active.index();
		var $around = $active.siblings().addBack()
			.slice(Math.max(0, activeIndex - 2), activeIndex + 3)
			.not($active);

		if ((activeIndex - 2) > 0) {
			$(".ul-pagination li:nth-child(2)").addClass('mr-3');
		}

		if ((activeIndex + 3) < $(".ul-pagination li").length) {
			$(".ul-pagination li:nth-last-child(2)").addClass('ml-3');
		}

		$active.show();
		$around.show();
	}
}
function PaginationPre() {
	if (pg > 1) {
		$("#bookings-body").hide();
		$("#bookings-body-preload").show();
		pg--;
		GetFilterValues();
		GetBookings();

		$(window).scrollTop(256);
		updatePagination(pg);
	}
}
function PaginationNex() {
	if (pg < totalPages) {
		$("#bookings-body").hide();
		$("#bookings-body-preload").show();
		pg++;
		GetFilterValues();
		GetBookings();

		$(window).scrollTop(256);
		updatePagination(pg);
	}
}
function PaginationCount(num) {
	if (pg != num) {
		$("#bookings-body").hide();
		$("#bookings-body-preload").show();
		pg = Number(num);
		GetFilterValues();
		GetBookings();

		$(window).scrollTop(256);
		updatePagination(pg);
	}
}
//#endregion

//#endregion 