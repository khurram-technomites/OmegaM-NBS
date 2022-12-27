
$(document).ready(function () {

	$('#Userform input').prop('disabled', true);
	$('#edit-cancel').hide();
	$('#save-changes').hide();
	$('#edit-profile').fadeIn();

	$('#edit-profile').click(function () {

		$('#Userform input').prop('disabled', false);


		$('#edit-profile').hide();
		$('#edit-cancel').fadeIn();
		$('#save-changes').fadeIn();
	});

	$('#edit-cancel').click(function () {
		$('#Userform input').prop('disabled', true);
		$('#edit-cancel').hide();
		$('#save-changes').hide();
		$('#edit-profile').fadeIn();
	});

});


function EmailTest() {
	$('#btnEmailSend').addClass('kt-spinner kt-spinner--left kt-spinner--sm kt-spinner--light');
	$.ajax({
		url: '/Emails/TestEMail/',
		type: 'Post',
		data: {

			Email: $('#EmailSend').val()
		},
		success: function (response) {
			if (response.success) {
				toastr.success(response.message)
				$('#btnEmailSend').removeClass('kt-spinner kt-spinner--left kt-spinner--sm kt-spinner--light');
			}
			else {
				toastr.error(response.message)
				$('#btnEmailSend').removeClass('kt-spinner kt-spinner--left kt-spinner--sm kt-spinner--light');
			}
			$('#EmailSend').val('');
		}
	});


}