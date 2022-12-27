const API_ENDPOINT = "https://socialbazar.co/";
var pg = 1;
var PageSize = 20;
var isPageRendered = false;
var totalPages;
var filter = {
	Status: '',
	ShipmentStatus: '',
	PageNumber: 1,
	SortBy: 1,
	Lang: 'ar',
}

$(document).ready(function () {

	$('#Status').change(function () {
		$("#orders-tbody").empty();
		pg = 1;
		GetFilterValues();
		GetOrders();
	});

	$('#ShipmentStatus').change(function () {
		$("#orders-tbody").empty();
		pg = 1;
		GetFilterValues();
		GetOrders();
	});


	$('#SortBy').change(function () {
		$("#orders-tbody").empty();
		pg = 1;
		GetFilterValues();
		GetOrders();
	});

	$('#load-more').click(function () {
		if (pg < totalPages) {
			pg++;
			$('#load-more').hide();
			$(".filter-loader").show();
			GetFilterValues();
			GetOrders();
		}
	});


});

function LoadOrders() {
	if (!isPageRendered) {
		$("#orders-tbody").empty();
		pg = 1;
		GetFilterValues();
		GetOrders();

		//$('.total-amount').each(function () {
		//	$(this).html(kFormatter(Number($(this).text()))).removeClass('total-amount');
		//});

	}
}

function GetFilterValues() {

	filter.Status = $("#Status").val();
	filter.ShipmentStatus = $("#ShipmentStatus").val();
	filter.PageNumber = pg;
	filter.SortBy = $("#SortBy").val();
}

function GetOrders() {

	$.ajax({
		type: 'POST',
		url: '/Customer/Order/List',
		contentType: "application/json",
		data: JSON.stringify(filter),
		success: function (response) {
			BindOrders(response);
		}
	});
}

function BindOrders(response) {

	var htmlTemplate = '';

	$.each(response.data, function (k, v) {

		htmlTemplate += '<tr>';
		htmlTemplate += '	<td class="text-center"><a class="btn view" href="/ar/Customer/Order/Details/' + v.ID + '" title="View Details">عرض التفاصيل</a></td>';
		htmlTemplate += '	<td><span class="total-amount money">' + v.TotalAmount + '</span> ' + v.Currency + '</td>';
		htmlTemplate += '	<td class="text-center">' + v.Status + '</td>';
		htmlTemplate += '	<td class="text-center">' + v.Date + '</td>';
		htmlTemplate += '	<td class="text-right" style="max-width:200px">' + v.Cars + '</td>';
		htmlTemplate += '	<td class="text-center">' + v.OrderNo + '</td>';
		htmlTemplate += '</tr>';
	});

	$("#orders-tbody").append(htmlTemplate);

	if (response.data.length > 0) {
		RenderPagination(response.data[0].TotalRecords, response.data[0].filteredRecords);
	}

	if (!$("#orders-tbody").html().trim()) {
		$(".filter-loader").hide();
		$("#load-more").hide();
		$("#orders-tbody").html('<tr><td colspan="6" class="text-center pt-5"><span class="alert alert-info" title="No data found">لاتوجد بيانات</span></td></tr>');

	} else {
		//$('.total-amount').each(function () {
		//	$(this).html(kFormatter(Number($(this).text()))).removeClass('total-amount');
		//});
	}

	FormatPrices();
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