$(document).ready(function () {
	$('#CustomerResetPasswordForm').submit(function () {
		var form = $(this);
		$('#btnSubmit').html('<span class="fa fa-spinner fa-spin"></span> Submit').attr('disabled', true);
		$.ajax({
			url: $(form).attr('action'),
			type: 'Post',
			data: $(form).serialize(),
			success: function (response) {
				if (response.success) {
					showErrorMsg(form, 'success', response.message);
					window.location.href = response.url;
				} else {
					showErrorMsg(form, 'danger', response.message);
					$('#btnSubmit').html('Submit').attr('disabled', false);
				}
			}
		});
		return false;
	})
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