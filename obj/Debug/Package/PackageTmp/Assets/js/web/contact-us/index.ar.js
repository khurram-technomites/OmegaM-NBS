$(document).ready(function () {
	$('#contact_form').submit(function () {
		var form = $(this);
		$('#btnSendMessage').html('<span class="fa fa-spinner fa-spin"></span> أرسل رسالة').attr('disabled', true);
		$.ajax({
			url: $(form).attr('action'),
			type: 'Post',
			data: $(form).serialize(),
			success: function (response) {
				if (response.success) {
					showErrorMsg(form, 'success', getTranslatedString(response.message));
					$(form).reset();
				} else {
					showErrorMsg(form, 'danger', getTranslatedString(response.message));
					$('#btnSendMessage').html('أرسل رسالة').attr('disabled', false);
					if (response.description) {
						$(form).prepend$(response.description);
					}
				}
			},
			error: function (e) {
				showErrorMsg(form, 'danger', "عفوًا ، حدث خطأ ما. حاول تحديث هذه الصفحة أو لا تتردد في الاتصال بنا إذا استمرت المشكلة.");
				$('#btnSendMessage').html('أرسل رسالة').attr('disabled', false);
			},
			failure: function (e) {
				showErrorMsg(form, 'danger', "عفوًا ، حدث خطأ ما. حاول تحديث هذه الصفحة أو لا تتردد في الاتصال بنا إذا استمرت المشكلة.");
				$('#btnSendMessage').html('أرسل رسالة').attr('disabled', false);
			}
		});
		return false;
	});
});

var showErrorMsg = function (form, type, msg) {
	var alert = $('<div class="alert kt-alert kt-alert--outline text-center alert alert-' + type + ' " role="alert">\
			<i class="close close-announcement fa fa-times" data-dismiss="alert" aria-label="Close"></button>\
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
		case "Thank you! Your message has been successfully sent.":
			message = "شكرا لك! تم إرسال رسالتك بنجاح";
			break;
		case "Please fill the form properly ...":
			message = "الرجاء تعبئة النموذج بشكل صحيح ...";
			break;
		case "Oops! Something went wrong. Please try later.":
			message = "وجه الفتاة! حدث خطأ ما ، يرجى المحاولة لاحقًا";
			break;
		default:
			message = message;
	}

	return message;
}