var pg = 1;
var PageSize = 20;
var isPageRendered = false;
var totalPages;
var filter = {
	Status: '',
	PageNumber: 1,
	SortBy: 1,
}

$(document).ready(function () {

	$('#Status-Lead-Request').change(function () {
		$("#lead-request-tbody").empty();
		pg = 1;
		GetLeadFilterValues();
		GetLeadRequests();
	});

	$('#SortBy-Lead-Request').change(function () {
		$("#lead-request-tbody").empty();
		pg = 1;
		GetLeadFilterValues();
		GetLeadRequests();
	});

	$('#load-more-lead').click(function () {

		if (pg < totalPages) {
			pg++;
			$('#load-more-lead').hide();
			$(".filter-loader").show();
			GetLeadFilterValues();
			GetLeadRequests();
		}
	});

});

function LoadLeadRequest() {
	if (!isPageRendered) {
		$("#lead-request-tbody").empty();
		pg = 1;
		GetLeadFilterValues();
		GetLeadRequests();
	}
}

function GetLeadFilterValues() {

	filter.Status = $("#Status-Lead-Request").val();
	filter.PageNumber = pg;
	filter.SortBy = $("#SortBy-Lead-Request").val();
}

function GetLeadRequests() {

	$.ajax({
		type: 'POST',
		url: '/Customer/LeadRequest/List',
		contentType: "application/json",
		data: JSON.stringify(filter),
		success: function (response) {
			BindLeadRequest(response);
		}
	});
}

function BindLeadRequest(response) {
	
	var htmlTemplate = '';

	$.each(response.data, function (k, v) {
		htmlTemplate += '<tr>';
		htmlTemplate += '	<td>' + v.RequestNo + '</td>';
		htmlTemplate += '	<td>' + v.Date + '</td>';
		htmlTemplate += '	<td>' + v.Status + '</td>';
		htmlTemplate += '	<td><a class="btn view" href="/Customer/LeadRequest/Details/' + v.ID + '">View</a></td>';
		htmlTemplate += '	<td><button class="btn view" name="remarksBtn" id="remarksBtn" onclick="RemarksCallLead(this)" data-id="' + v.ID + '">Remarks</button></td>';
		htmlTemplate += '</tr>';
	});

	$("#lead-request-tbody").append(htmlTemplate);

	if (response.data.length > 0) {
		RenderPaginationLead(response.data[0].TotalRecords, response.data[0].filteredRecords);
	}
	if (!$("#lead-request-tbody").html().trim()) {
		$(".filter-loader").hide();
		$("#load-more-lead").hide();
		$("#lead-request-tbody").html('<tr><td colspan="6" class="text-center pt-5"><span class="alert alert-info ">No data found</span></td></tr>');

	} else {

	}

}

function RenderPaginationLead(totalRecord, filteredRecord) {
	
	totalPages = Math.ceil(filteredRecord / PageSize);

	$(".filter-loader").hide();
	if (pg >= totalPages) {
		$("#load-more-lead").fadeOut();
	} else {
		$("#load-more-lead").fadeIn();
	}

}

function RemarksCallLead(element) {
	$(element).prop('disabled', true);
	var GetURL = '/Customer/Dashboard/LeadRequest';
	var id = $(element).attr('data-id');

	$.ajax({
		type: "GET",
		url: GetURL,
		contentType: "application/json; charset=utf-8",
		data: { "Id": id },
		datatype: "json",
		success: function (data) {
			
			$('#RemarksContent').html(data);
			$('#RemarksModal').modal('show');
			$(element).prop('disabled', false);

		},
		error: function () {
			alert("Load failed.");
			$(element).prop('disabled', false);
		}
	});
};