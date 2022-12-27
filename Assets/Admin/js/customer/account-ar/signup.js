$(document).ready(function () {

	$('#CustomerSignupForm').submit(function () {
		var form = $(this);
		$('#btnSignup').html('<span class="fa fa-spinner fa-spin"></span> سجل').attr('disabled', true);

		if ($('#Referral').val()) {
			$("#btnApplyReferral").html('<i class="fa fa-spinner fa-spin"></i> إرسال').prop('disabled', true);
			$.ajax({
				url: '/Customer/Account/Referral',
				type: 'POST',
				data: {
					__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
					referral: {
						Email: $('#Referral').val()
					}
				},
				success: function (response) {
					if (response.success) {
						$('#ReferralID').val(response.ReferralID);

						$.ajax({
							url: $(form).attr('action'),
							type: 'Post',
							data: $(form).serialize(),
							success: function (response) {
								if (response.success) {
									showErrorMsg(form, 'success', getTranslatedString(response.message));
									window.location.href = response.url;
								} else {
									showErrorMsg(form, 'danger', getTranslatedString(response.message));
									$('#btnSignup').html('سجل').attr('disabled', false);
									if (response.description) {
										$(form).prepend$(response.description);
									}
								}
							}
						});
					}
					else {
						$('#Referral').removeClass('border-success').addClass('border-danger');
						$('#btnSignup').html('سجل').attr('disabled', true);
						$('.btn-cancel-referral-container').slideDown();
					}
				}
			})
		} else {
			$.ajax({
				url: $(form).attr('action'),
				type: 'Post',
				data: $(form).serialize(),
				success: function (response) {
					if (response.success) {
						showErrorMsg(form, 'success', getTranslatedString(response.message));
						window.location.href = response.url;
					} else {
						showErrorMsg(form, 'danger', getTranslatedString(response.message));
						$('#btnSignup').html('سجل').attr('disabled', false);
						if (response.description) {
							$(form).prepend$(response.description);
						}
					}
				}
			});
		}
		return false;
	})

	$('#btnCancelReferral').click(function () {

		$('#ReferralID').val('');
		$('#Referral').val("");

		$('#Referral').removeClass('border-danger').removeClass('border-success');
		$('#btnSignup').html('سجل').attr('disabled', false);
		$('.btn-cancel-referral-container').slideUp();
	});
});

var showErrorMsg = function (form, type, msg) {
	var alert = $('<div class="alert kt-alert kt-alert--outline text-center alert alert-' + type + ' " role="alert">\
			<i class="close close-announcement fa fa-times" data-dismiss="alert" aria-label="Close"></i>\
			<span></span>\
		</div>');

	form.find('.alert').remove();
	alert.prependTo(form);
	//KTUtil.animateClass(alert[0], 'fadeIn animated');
	$(alert).slideDown();
	alert.find('span').html(msg);
}

function getTranslatedString(message) {

	switch (message) {
		case "Account created!":
			message = "تم إنشاء الحساب!";
			break;
		case "We just need to verify your email address before you can access ":
			message = "نحتاج فقط إلى التحقق من عنوان بريدك الإلكتروني قبل أن تتمكن من الوصول إليه";
			break;
		case "Account already exist!":
			message = "الحساب موجود مسبقا";
			break;
		case "Please fill the form properly ...":
			message = "الرجاء تعبئة النموذج بشكل صحيح ...";
			break;
		case "Oops! Something went wrong. Please try later.":
			message = "وجه الفتاة! حدث خطأ ما ، يرجى المحاولة لاحقًا";
			break;
		case "Invalid Referral!":
			message = "إحالة غير صالحة!";
			break;
		case "The email address is required!":
			message = "عنوان البريد الإلكتروني مطلوب!";
			break;

		default:
			message = message;
	}

	return message;
}