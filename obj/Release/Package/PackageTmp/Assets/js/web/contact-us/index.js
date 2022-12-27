$(document).ready(function () {
	$('#contact_form').submit(function () {
		var form = $(this);
		$('#btnSendMessage').html('<span class="fa fa-spinner fa-spin"></span> Send Message').attr('disabled', true);
		$.ajax({
			url: $(form).attr('action'),
			type: 'Post',
			data: $(form).serialize(),
			success: function (response) {
				if (response.success) {
					showErrorMsg(form, 'success', response.message);
					$('#contact_form').trigger("reset");
					$('#btnSendMessage').html('Send Message').attr('disabled', false);
				} else {
					showErrorMsg(form, 'danger', response.message);
					$('#btnSendMessage').html('Send Message').attr('disabled', false);
					if (response.description) {
						$(form).prepend$(response.description);
					}
				}
			},
			error: function (e) {
				showErrorMsg(form, 'danger', "Ooops, something went wrong.Try to refresh this page or feel free to contact us if the problem persists.");
				$('#btnSendMessage').html('Send Message').attr('disabled', false);
			},
			failure: function (e) {
				showErrorMsg(form, 'danger', "Ooops, something went wrong.Try to refresh this page or feel free to contact us if the problem persists.");
				$('#btnSendMessage').html('Send Message').attr('disabled', false);
			}
		});
		return false;
	});
});

var showErrorMsg = function (form, type, msg) {
	var alert = $('<div class="alert kt-alert kt-alert--outline alert alert-' + type + ' " role="alert">\
			<button type="button" class="close" data-dismiss="alert" aria-label="Close"><i class="fa fa-times"></i></button>\
			<span></span>\
		</div>');

	form.find('.alert').remove();
	alert.prependTo(form);
	//KTUtil.animateClass(alert[0], 'fadeIn animated');
	$(alert).slideDown();
	alert.find('span').html(msg);
}