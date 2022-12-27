$(document).ready(function () {
	$('#CustomerResetPasswordForm').submit(function () {
		var form = $(this);
		$('#btnSubmit').html('<span class="fa fa-spinner fa-spin"></span> إرسال').attr('disabled', true);
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
					$('#btnSubmit').html('إرسال').attr('disabled', false);
				}
			}
		});
		return false;
	})
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
		case "Account reset successfully":
			message = "إعادة تعيين الحساب بنجاح";
			break;
		case "Please fill the form properly ...":
			message = "الرجاء تعبئة النموذج بشكل صحيح ...";
			break;
		case "Oops! Something went wrong. Please try later.":
			message = "وجه الفتاة! حدث خطأ ما ، يرجى المحاولة لاحقًا";
			break;
		case "Session expired!":
			message = "انتهت الجلسة!";
			break;
		default:
			message = message;
	}

	return message;
}