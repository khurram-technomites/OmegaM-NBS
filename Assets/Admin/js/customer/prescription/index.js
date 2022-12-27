var pg = 1;
var PageSize = 20;
var isPageRendered = false;
var totalPages;
var filter = {
	Status: '',
	PageNumber: 1,
	SortBy: 1,
	Lang: 'en',
}

$(document).ready(function () {

	$('#Status-Prescription').change(function () {
		$("#prescription-tbody").empty();
		pg = 1;
		GetPrescriptionFilterValues();
		GetPrescriptions();
	});

	$('#SortBy-Prescription').change(function () {
		$("#prescription-tbody").empty();
		pg = 1;
		GetPrescriptionFilterValues();
		GetPrescriptions();
	});

	$('#load-more-prescription').click(function () {
		if (pg < totalPages) {
			pg++;
			$('#load-more-prescription').hide();
			$(".filter-loader").show();
			GetPrescriptionFilterValues();
			GetPrescriptions();
		}
	});

});

function LoadPrescription() {
	if (!isPageRendered) {
		$("#prescription-tbody").empty();
		pg = 1;
		GetPrescriptionFilterValues();
		GetPrescriptions();

	}
}

function GetPrescriptionFilterValues() {

	filter.Status = $("#Status-Prescription").val();
	filter.PageNumber = pg;
	filter.SortBy = $("#SortBy-Prescription").val();
}

function GetPrescriptions() {

	$.ajax({
		type: 'POST',
		url: '/Customer/Prescription/List',
		contentType: "application/json",
		data: JSON.stringify(filter),
		success: function (response) {
			BindPrescriptions(response);
		}
	});
}

function BindPrescriptions(response) {
	
	var htmlTemplate = '';

	$.each(response.data, function (k, v) {
		
		htmlTemplate += '<tr>';
		htmlTemplate += '	<td>' + v.PrescriptionCode + '</td>';
		htmlTemplate += '	<td>' + v.Date + '</td>';
		htmlTemplate += '	<td>' + v.Status + '</td>';
		htmlTemplate += '	<td><a class="btn view" href="/Customer/Prescription/Details/' + v.ID + '">View</a></td>';
		htmlTemplate += '	<td><button class="btn view" name="remarksBtn" id="remarksBtn" onclick="RemarksCall(this)" data-id="' + v.ID + '">Remarks</button></td>';
		htmlTemplate += '</tr>';
	});

	$("#prescription-tbody").append(htmlTemplate);


	if (response.data.length > 0) {
		RenderPaginationPrescription(response.data[0].TotalRecords, response.data[0].filteredRecords);
	}

	if (!$("#prescription-tbody").html().trim()) {
		$(".filter-loader").hide();
		$("#load-more-prescription").hide();
		$("#prescription-tbody").html('<tr><td colspan="6" class="text-center pt-5"><span class="alert alert-info ">No data found</span></td></tr>');

	} else {
	}

}

function RenderPaginationPrescription(totalRecord, filteredRecord) {
	
	totalPages = Math.ceil(filteredRecord / PageSize);

	$(".filter-loader").hide();
	if (pg >= totalPages) {
		$("#load-more-prescription").fadeOut();
	} else {
		$("#load-more-prescription").fadeIn();
	}

}

function RemarksCall(element)
{
	var GetURL = '/Customer/Dashboard/Prescription';
	$(element).attr('disabled', true);
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
					$(element).attr('disabled', false);

				},
				error: function () {
					alert("Load failed.");
					$(element).attr('disabled', false);

				}
			});
};

