'use strict';

//#region Global Variables and Arrays
var AlertPageNo = 1;
var AlertPageSize = 10;
var FilteredRecord = 20;
var AlertTotalPages;

var AlertMoreRecordExist = true;
var AlertIsUnseenRecoredExist = false;
var AlertIsUnReadRecoredExist = false;
var AlertUnSeenMessages = 0;
var AlertUndeliveredMessages = 0;
var receiverId = Number($('#receiverId').val());
var IsAlertsLoaded = false;
//#endregion

//#region document ready function
$(document).ready(function () {

});
//#endregion

//#region Bind Response

function BindAlerts(response) {
	//console.log(response);
	
	if (response.data.length > 0) {
		$("#alerts-ul").empty();

		//#region Checking Notification Data
		if (response.data.length < 5) {
			AlertMoreRecordExist = false;
		}

		AlertUnSeenMessages = response.data.filter(function (obj) {
			return obj.IsSeen == false;
		}).length;

		if (AlertUnSeenMessages > 0) {
			//$('#NotificationCount').html(AlertUnSeenMessages + ' new');
			//$('#NotificationCount').fadeIn();

			//$('.notification-alert').show();
			AlertIsUnseenRecoredExist = true;
		}

		AlertUndeliveredMessages = response.data.filter(function (obj) {
			return obj.IsDelivered == false;
		}).length;

		//if (UndeliveredMessages > 0) {
		//	playSound();
		//}

		AlertIsUnReadRecoredExist = response.data.filter(function (obj) {
			return obj.IsRead == false;
		}).length > 0 ? true : false;

		//#endregion

		$.each(response.data, function (k, v) {
			$("#alerts-ul").append(`
				<li class="wo-dhb-content wo-alert border-radius-0 ${!v.IsSeen ? "new-alert" : ""} ${lang == 'en' ? '' : 'text-right'} ${k % 2 ? 'bg-light' : ''}">
					<div class="wo-alert__holder">
						<div class="wo-alert__holder__titlearea">
							<div class="wo-alert__title">
								<a href="${v.Link && v.Link != null ? "/" + culture + "/" + v.Link : "javascript:void(0);"}">
									<h6>${v.Title}</h6>
									<p>${v.Description}</p>
								</a>
							</div>
							<div class="wo-alert__titlearea__rightarea">
								<em>${v.Date}</em>
							</div>
						</div>
					</div>
				</li>
			`);
		});

		//RenderPaginationAlerts(response.TotalRecord);
		FilteredRecord = response.TotalRecord;
		RenderPaginationAlerts(response.TotalRecord, FilteredRecord);

		if (response.success) {
			setTimeout(function () {
				$('.web-alerts-count').text(0);
			}, 3000);
		}
		IsSeen(receiverId);
	}
	else {
		$("#alerts-ul").empty();
		$("#alerts-ul").html(` <li class="wo-dhb-content wo-alert border-radius-0 text-center">
                                        <span class="alert alert-dark p-2">${ChangeString('No record found', 'لا يوجد سجلات')}</span>
                                    </li>`);
		FilteredRecord = 0;
		RenderPaginationAlerts(0,0);
	}
	$("#alerts-ul-body").show();
	$("#alerts-ul-body-preload").hide();
}

//#endregion

//#region Others Function
function LoadAlerts() {
	if (!IsAlertsLoaded) {
		GetAlerts();
		IsAlertsLoaded = true;
	}
}

function GetAlerts() {
	$.ajax({
		type: 'GET',
		url: '/' + culture + '/Customer/Notification/LoadNotifications?pageNo=' + AlertPageNo,
		contentType: "application/json",
		success: function (response) {
			if (response.success) {
				BindAlerts(response);
			} else {
				$("#alerts-ul").html(`
                                            <li class="wo-dhb-content wo-alert border-radius-0 text-center">
												<span class="alert alert-dark p-2">${ServerError()}</span>
											</li>
                                        `);
			}
		},
		error: function (e) {
			console.log("Get Alerts Error.");
		}
	});
}

function IsSeen(receiverId) {
	if (AlertIsUnseenRecoredExist) {
		$.ajax({
			type: 'POST',
			url: '/' + culture + '/Customer/Notification/MarkNotificationsAsSeen/?receiverId=' + receiverId,
			success: function (response) {
				if (response.success) {
					setTimeout(function () {
						$('.wo-alert').removeClass('new-alert');
					}, 6000);
				}
				else {
				}
			}
		});
	}
}

function RenderPaginationAlerts(totalRecord, filteredRecord) {

	AlertTotalPages = Math.ceil(filteredRecord / AlertPageSize);
	AlertTotalPages = (!AlertTotalPages && AlertTotalPages == 0) ? 1 : AlertTotalPages;

	//$('.ul-alerts-pagination').hide();
	//$(".ul-alerts-pagination").empty();

	//$('.ul-alerts-pagination').append(`<li class="wo-prevpage">
 //                                   <a href="javascript:void(0);" class="pagination-prv${AlertPageNo == 1 ? '-disabled a-disabled-hover' : ''}" 
 //                                       onclick="${AlertPageNo == 1 ? '' : 'PaginationAlertsPre()'}">${ChangeString('PRE', 'سابق')}</a>
 //                               </li>`);

	//for (var i = 1; i <= AlertTotalPages; i++) {
	//	$('.ul-alerts-pagination').append(`<li class="${AlertPageNo == i ? 'wo-active' : ''}">
 //                                       <a href="javascript:void(0);" class="pagination-count" onclick="PaginationAlertsCount(${i})">${i}</a>
 //                                   </li>`);
	//}
	//$('.ul-alerts-pagination').append(`<li class="wo-nextpage"><a href="javascript:void(0);" class="pagination-nex${AlertPageNo == AlertTotalPages ? '-disabled a-disabled-hover' : ''}"
 //                                       onclick="${AlertPageNo == AlertTotalPages ? '' : 'PaginationAlertsNex()'}">${ChangeString('NEX', 'التالي')}</a></li>`);
	//$('.ul-alerts-pagination').show();
	//$('#Pagination-Alerts').show();
	
	if (FilteredRecord > 0) {
		if (FilteredRecord > AlertPageSize) {
			$('.alert-result-count').text(`${ChangeString('Showing', 'عرض')} ${(((AlertPageNo - 1) * AlertPageSize) + 1)}–${AlertPageNo * AlertPageSize} ${ChangeString('of', 'من')} ${FilteredRecord} ${ChangeString('results', 'النتائج')}`);
		} else {
			$('.alert-result-count').text(`${ChangeString('Showing', 'عرض')} ${FilteredRecord} ${ChangeString('results', 'النتائج')}`);
		}
	} else {
		$('.alert-result-count').text(`${ChangeString('Showing', 'عرض')} 0 ${ChangeString('results', 'النتائج')}`);
	}

	$(".ul-alerts-pagination").html('');

	if (AlertTotalPages > 1) {
		$(".ul-alerts-pagination").append(`<li class="wo-prevpage"><a id="btn-alert-pagination-prev" href="javascript:void(0);" class="pagination-prv${AlertPageNo == 1 ? '-disabled a-disabled-hover' : ''}" onclick="${AlertPageNo == 1 ? '' : 'PaginationAlertsPre()'}">${ChangeString('PRE', 'سابق')}</a></li>`);
	}

	for (var i = 1; i <= AlertTotalPages; i++) {
		if (i == AlertPageNo) {
			$(".ul-alerts-pagination").append(`<li class="wo-active"><a href="javascript:void(0);" class="pagination-count" onclick="PaginationAlertsCount(${i})">${i}</a></li>`);
		} else {
			$(".ul-alerts-pagination").append(`<li><a href="javascript:void(0);" class="pagination-count" onclick="PaginationAlertsCount(${i})">${i}</a></li>`);
		}
	}

	if (AlertTotalPages > 1) {
		$(".ul-alerts-pagination").append(`<li id="btn-alert-pagination-next" class="wo-nextpage"><a href="javascript:void(0);" class="pagination-nex${AlertPageNo == AlertTotalPages ? '-disabled a-disabled-hover' : ''}" onclick="${AlertPageNo == AlertTotalPages ? '' : 'PaginationAlertsNex()'}">${ChangeString('NEX', 'التالي')}</a></li>`);
	}

	stylePaginationAlerts();
}

//#region pagination
function PaginationAlertsPre() {
	if (AlertPageNo > 1) {
		$("#alerts-ul-body").hide();
		$("#alerts-ul-body-preload").show();
		AlertPageNo--;
		GetAlerts();

		$(window).scrollTop(180);
		updatePaginationAlerts(AlertPageNo);
	}
}
function PaginationAlertsNex() {
	if (AlertPageNo < AlertTotalPages) {
		$("#alerts-ul-body").hide();
		$("#alerts-ul-body-preload").show();
		AlertPageNo++;
		GetAlerts();

		$(window).scrollTop(180);
		updatePaginationAlerts(AlertPageNo);
	}
}
function PaginationAlertsCount(num) {
	if (AlertPageNo != Number(num)) {
		$("#alerts-ul-body").hide();
		$("#alerts-ul-body-preload").show();
		AlertPageNo = Number(num);
		GetAlerts();

		$(window).scrollTop(180);
		updatePaginationAlerts(pg);
	}
	
}
function updatePaginationAlerts(pageNumber) {
	
	if (FilteredRecord > 0) {
		if (FilteredRecord > PageSize) {
			$('.alert-result-count').text(`${ChangeString('Showing', 'عرض')} ${(((pg - 1) * PageSize) + 1)}–${pg * PageSize} ${ChangeString('of', 'من')} ${FilteredRecord} ${ChangeString('results', 'النتائج')}`);
		} else {
			$('.alert-result-count').text(`${ChangeString('Showing', 'عرض')} ${FilteredRecord} ${ChangeString('results', 'النتائج')}`);
		}
	} else {
		$('.alert-result-count').text(`${ChangeString('Showing', 'عرض')} 0 ${ChangeString('results', 'النتائج')}`);
	}

	stylePaginationAlerts();
}
function stylePaginationAlerts() {

	if ($(".ul-alerts-pagination li:nth-child(2)").hasClass('wo-active')) {
		$('#btn-alert-pagination-prev').hide();
	} else {
		$('#btn-alert-pagination-prev').show();
	}
	if ($(".ul-alerts-pagination li:nth-last-child(2)").hasClass('wo-active')) {
		$('#btn-alert-pagination-next').hide();
	} else {
		$('#btn-alert-pagination-next').show();
	}

	if ($(".ul-alerts-pagination li").length > 6) {
		$(".ul-alerts-pagination li:not(:first):not(:last):not(:nth-last-child(2)):not(:nth-child(2))").hide();

		var $active = $(".ul-alerts-pagination .wo-active");
		var activeIndex = $active.index();
		var $around = $active.siblings().addBack()
			.slice(Math.max(0, activeIndex - 2), activeIndex + 3)
			.not($active);

		if ((activeIndex - 2) > 0) {
			$(".ul-alerts-pagination li:nth-child(2)").addClass(ChangeString('mr-3', 'ml-3'));
		}

		if ((activeIndex + 3) < $(".ul-alerts-pagination li").length) {
			$(".ul-alerts-pagination li:nth-last-child(2)").addClass(ChangeString('ml-3','mr-3'));
		}

		$active.show();
		$around.show();
	}
}
//#endregion

//#endregion 
