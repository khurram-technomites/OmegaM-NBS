var IsOrderLoaded = false;

var IsChangePasswordLoaded = false;

var IsProfileLoaded = false;

$(document).ready(function () {

	$('a[href="#change-password"]').click(function () {
		if (!IsChangePasswordLoaded) {
			jQuery('#change-password').load('/ar/Customer/Account/ChangePassword', function () {
				IsChangePasswordLoaded = true;

			});
		} else {
			$('.alert').remove();
		}
	});

	$('a[href="#account-details"]').click(function () {
		if (!IsProfileLoaded) {
			jQuery('#account-details').load('/ar/Customer/Account/Profile', function () {
				IsProfileLoaded = true;
			});
		} else { $('.alert').remove(); }
	});

	FormatPrices();
});

