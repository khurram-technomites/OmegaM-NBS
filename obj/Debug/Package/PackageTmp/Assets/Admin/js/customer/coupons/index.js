'use strict';

//#region Global Variables and Arrays
var IsCouponLoaded = false;
//#endregion

//#region document ready function
$(document).ready(function () {

});
//#endregion

//#region Bind Response

function BindCoupons(response) {
    //console.log(response);

    if (response.data.length > 0) {
        $("#coupons-tbody").empty();

        $.each(response.data, function (k, v) {
            $("#coupons-tbody").append(`
				<tr>
					<td data-label="${ChangeString('Coupon Code', 'رمز الكوبون')}">${v.promoCode}</td>
					<td data-label="${ChangeString('Description', 'وصف')}">${v.name}</td>
					<td data-label="${ChangeString('Discount', 'تخفيض')}">
                        ${(v.Type == "FixedAmount" ? `<span class="money">${v.Value} AED</span>` : `<span class="percentage">${v.Value} %</span>`)}
                    </td>
                    <td data-label="${ChangeString('Expiry Date', 'تاريخ انتهاء الصلاحية')}">${v.expiryDate}</td>
					<td data-label="${ChangeString('Status', 'تخفيض')}">
                        <span class="badge ${(v.IsExpired == false ? `wo-bg-purple` : `wo-bg-grey`)} flex-app justify-content-center wo-titleinput p-1">
                            ${(v.IsExpired == false ? `Available` : `Expired`)}
                        </span>
                    </td>
				</tr>
			`);
        });

    }
    else {
        $("#coupons-tbody").empty();
        $("#coupons-tbody").html(`<tr><td colspan="5" class="text-center p-4">
                                            <span class="alert alert-dark p-2">${ChangeString('No record found', 'لا يوجد سجلات')}</span>
                                </td></tr>`);
    }

}

//#endregion

//#region Others Function
function LoadCoupons() {
    if (!IsCouponLoaded) {
        GetCoupons();
        IsCouponLoaded = true;
    }
}

function GetCoupons() {

    $.ajax({
        type: 'GET',
        url: '/' + culture + '/customer/coupons',
        contentType: "application/json",
        success: function (response) {
            if (response.success) {
                BindCoupons(response);
            } else {
                $("#coupons-tbody").html(`<tr><td colspan="5" class="text-center p-4">
                                            <span class="alert alert-dark p-2">${ServerError()}</span>
                                </td></tr>`);
            }
        },
        error: function (e) {
            console.log("Get Coupons Error.");
        }
    });
}

//#endregion 

function BindCouponsa(response) {

    var htmlTemplate = '';
    $("#coupons-tbody").empty();
    $.each(response.data, function (k, v) {
        htmlTemplate += `<tr>
			<td>${v.promoCode}</td>
			<td style="max-width:200px">${v.name}</td>
			<td>${(v.Type == "FixedAmount" ? `<span class="money">${v.Value} AED</span>` : `<span class="percentage">${v.Value} %</span>`)}</td>
			<td>${(v.IsExpired == false ? `Available` : `Expired`)}</td>
		</tr>`;
    });

    $("#coupons-tbody").append(htmlTemplate);

    if (!$("#coupons-tbody").html().trim()) {
        $(".filter-loader").hide();
        $("#load-more").hide();
        $("#coupons-tbody").html('<tr><td colspan="6" class="text-center pt-5"><span class="alert alert-info ">No data found</span></td></tr>');

    } else {

        //$('.total-amount').each(function () {
        //	$(this).html(kFormatter(Number($(this).text()))).removeClass('total-amount');
        //});
    }

    $(".filter-loader").hide();
    $("#load-more").hide();
}
