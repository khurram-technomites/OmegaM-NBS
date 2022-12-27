'use strict'

$(document).ready(function () {

	//#region Contact us Form Submission
	$('#contact_form').submit(function () {
		var form = $(this);
		ButtonDisabled('#btnSendMessage', true, true);
		$.ajax({
			url: $(form).attr('action'),
			type: 'Post',
			data: $(form).serialize(),
			success: function (response) {
				if (response.success) {
					//ShowFormAlert(form, 'success', response.message, 6);
					ShowSweetAlert('success', ChangeString("Ok!", "حسنا!"), response.message, '#40194e');

					$('#contact_form').trigger("reset");
					ButtonDisabled('#btnSendMessage', false, false);
				} else {
					//ShowFormAlert(form, 'danger', response.message, 6);
					ShowSweetAlert('error', ChangeString("Oops...", "وجه الفتاة..."), response.message, '#40194e');

					ButtonDisabled('#btnSendMessage', false, false);
					if (response.description) {
						$(form).prepend$(response.description);
					}
				}
			},
			error: function (e) {
				ShowFormAlert(form, 'danger', ServerError, 6);
				ButtonDisabled('#btnSendMessage', false, false);
			},
			failure: function (e) {
				ShowFormAlert(form, 'danger', ServerError, 6);
				ButtonDisabled('#btnSendMessage', false, false);
			}
		});
		return false;
	});
	//#endregion

});
