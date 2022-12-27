var IsCouponLoaded = false;

$(document).ready(function () {

});

function GetCoupons() {
	if (!IsCouponLoaded) {
		$.ajax({
			type: 'GET',
			url: '/' + lang + '/customer/coupons',
			contentType: "application/json",
			success: function (response) {
				if (response.success) {
					IsCouponLoaded = true;
					BindCoupons(response);
				} else {
					$("#coupons-tbody").html('<tr><td colspan="6" class="text-center pt-5"><span class="alert alert-info text-center" title="Oops! Something went wrong. Please try later.">وجه الفتاة! هناك خطأ ما. الرجاء المحاولة لاحقا.</span></td></tr>');
				}
			}
		});
	}
}

function BindCoupons(response) {

	var htmlTemplate = '';
	$("#coupons-tbody").empty();
	$.each(response.data, function (k, v) {
		console.log(v);
		htmlTemplate += '<tr>';
		htmlTemplate += '	<td class="text-center">' + (v.IsExpired == false ? 'Available' : 'Expired') + '</td>';
		htmlTemplate += '	<td class="text-center">' + (v.DicountAmount ? '<span class="money">' + v.DicountAmount + ' AED</span>' : v.DicountPercentage + ' %') + '</td>';
		htmlTemplate += '	<td class="text-center" style="max-width:200px">' + v.name + '</td>';
		htmlTemplate += '	<td class="text-center">' + v.promoCode + '</td>';
		//htmlTemplate += '	<td><span class="total-amount">' + v.TotalAmount + '</span> ' + v.Currency + '</td>';
		//htmlTemplate += '	<td><a class="btn view" href="/Customer/Order/Details/' + v.ID + '">View</a></td>';
		htmlTemplate += '</tr>';
	});

	$("#coupons-tbody").append(htmlTemplate);

	if (!$("#coupons-tbody").html().trim()) {
		$(".filter-loader").hide();
		$("#load-more").hide();
		$("#coupons-tbody").html('<tr><td colspan="6" class="text-center pt-5"><span class="alert alert-info text-center" title="No data found">لاتوجد بيانات</span></td></tr>');

	} else {

		//$('.total-amount').each(function () {
		//	$(this).html(kFormatter(Number($(this).text()))).removeClass('total-amount');
		//});
	}

	$(".filter-loader").hide();
	$("#load-more").hide();
}
