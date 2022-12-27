///// <reference path="../../pages/features/miscellaneous/sweetalert2.js" />
'use strict';
//#region Global Variables and Arrays

//#endregion

//#region document ready function
$(document).ready(function () {

	if ($('.rating-star-0').length) {
		var val = parseFloat($('.rating-star-0').text());
		// Make sure that the value is in 0 - 5 range, multiply to get width
		var width = Math.max(0, (Math.min(5, val))) * 20;
		$('.rating-star-0').css('width', width + '%');
		$('.rating-star-0').text('');
	};

	$('.wo-stars-m-TL').click(function () {
		$('#review-booking-modal').modal('show');
	});

	$("#terms-cancel-booking").change(function () {
		var chk = $('#terms-cancel-booking:checked').length;
		if (chk > 0) {
			$("#btn-cancel-booking").prop("disabled", false);
		}
		else {
			$("#btn-cancel-booking").prop("disabled", true);
		}
	});

	//$('#btn-paynow').click(function () {

	//	Swal.fire({
	//		title: ChangeString('Are you sure?', 'هل أنت متأكد؟'),
	//		text: ChangeString("You won't be able to revert this!", "لن تتمكن من التراجع عن هذا!"),
	//		icon: 'warning',
	//		showCancelButton: true,
	//		confirmButtonColor: '#40194e',
	//		cancelButtonColor: '#7a7a7a',
	//		confirmButtonText: ChangeString("Yes, Pay Now!", "نعم ، ادفع الآن!"),
	//		cancelButtonText: ChangeString("Cancel", "يلغي"),
	//	}).then((result) => {
	//		if (result.isConfirmed) {
	//			Swal.fire({
	//				icon: 'success',
	//				title: ChangeString("Ok!", "حسنا!"),
	//				text: ChangeString("You will be redirecting to Payment option!", "ستتم إعادة توجيهك إلى خيار الدفع!"),
	//				confirmButtonColor: '#40194e',
	//				confirmButtonText: ChangeString("Ok", "حسنا")
	//			});
	//		}
	//	});
	//});

	//#region Write Review Submit Form
	$('#Form-Write-Review-Booking').submit(function () {
		var form = $(this);
		ButtonDisabled('#btn-write-review-booking', true, true);
		var Rating = $('#Order-Rating i.selected').length;
		$('#Rating').val(Rating);
		if (Rating && Rating > 0) {
			$.ajax({
				type: 'POST',
				url: '/' + culture + '/Customer/Order/Rating',
				data: $(form).serialize(),
				success: function (response) {
					if (response.success) {
						$('#write-review-modal').modal('hide');

						//ShowFormAlert(form, 'success', response.message, 6);
						ShowSweetAlert('success', ChangeString("Ok!", "حسنا!"), response.message, '#40194e');

						$('#Form-Write-Review-Booking').trigger("reset");

						PageReload(window.location.href, .5);
					} else {
						ShowFormAlert(form, 'danger', response.message, 6);
						//ShowSweetAlert('error', ChangeString("Oops...", "وجه الفتاة..."), response.message, '#40194e');

						ButtonDisabled('#btn-write-review-booking', false, false);
						if (response.description) {
							$(form).prepend$(response.description);
						}
					}
				},
				error: function (e) {
					ShowFormAlert(form, 'danger', ServerError, 6);
					ButtonDisabled('#btn-write-review-booking', false, false);
				},
				failure: function (e) {
					ShowFormAlert(form, 'danger', ServerError, 6);
					ButtonDisabled('#btn-write-review-booking', false, false);
				}
			});
		}
		else {
			ShowFormAlert(form, 'danger', ChangeString("Please give appropriate rating first!", "يرجى إعطاء التصنيف المناسب أولا!"), 6);
			ButtonDisabled('#btn-write-review-booking', false, false);
		}
		return false;
	});
	//#endregion

});
//#endregion

//#region Ajax Call

//#endregion

//#region Bind Response

//#endregion

//#region Others Function

function InitiatePayment(orderId, orderNo) {
	$('#btn-paynow').prepend(`<i class="fa fa-circle-notch fa-spin mr-5"></i>`);
	$('#message').html(`${ChangeString('Processing for payment.', 'معالجة الدفع.')}`).addClass('success').slideDown();

	$.ajax({
		url: '/' + culture + '/Customer/Payment/Initiate/' + orderId,
		type: 'Get',
		/*data: { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(), viewModel: Order },*/
		success: function (result) {
			if (result.success) {
				if (result.redirectUrl) {
					window.location = result.redirectUrl;
				}
			} else {

				$('#btn-paynow i').remove();
				$('#message').html(`<p class="mt-2">
													${ChangeString('Booking payment processing failed, please try again.', 'تم إجراء الحجز ، وتجهيزه للدفع.')}
													</br>
													${ChangeString('If the error persists contact our support.', 'إذا استمر الخطأ ، فاتصل بدعمنا.')}
												</p>`).addClass('error').slideDown();

			}
		}
	})
		.fail(function (e) {

			$('#btn-paynow i').remove();
			$('#message').html(`<p class="mt-2">
													${ChangeString('Booking payment processing failed, please try again.', 'تم إجراء الحجز ، وتجهيزه للدفع.')}
													</br>
													${ChangeString('If the error persists contact our support.', 'إذا استمر الخطأ ، فاتصل بدعمنا.')}
												</p>`).addClass('error').slideDown();
		});
	event.preventDefault();
}
//#endregion