'use strict';

//#region Global Variables and Arrays
let defaultGccFileName = $('.choose-file-license-gcc-span').text();
let defaultNonGccFileName = $('.choose-file-license-non-gcc-span').text();
let defaultPassportFileName = $('.choose-file-passport-span').text();

var IsMyDocumentsLoaded = false;

var LicenseGcc;
var LicenseNonGcc;
var Passport;

$(document).ready(function () {
	if (session) {
		GetDocuments();
	}

	/*Passport*/
	$('#upload-passport').click(function () {
		$('#choose-file-passport').trigger('click');
	});

	$('#choose-file-passport').change(function () {
		$("#form-passport").submit();
	});

	$("#form-passport").submit(function () {

		var form = $(this);
		var formData = new FormData();

		var files = $("#choose-file-passport").get(0).files;
		if (files.length > 0) {
			ButtonDisabled('#btn-passport', true, true);

			formData.append("Path", files[0], files[0].name);

			formData.append("__RequestVerificationToken", $('input[name="__RequestVerificationToken"]').val());
			formData.append("Type", 'Passport');
			//formData.append("Is", false);
			if (Passport) {
				formData.append("ID", Passport.ID);
			}

			$('#upload-passport').find('i').removeClass('fa-upload').addClass('fa-circle-notch').addClass('fa-spin');

			$.ajax({
				url: '/' + culture + '/Customer/Documents/CreateOrUpdate/',
				type: "POST",
				data: formData,
				contentType: false,
				processData: false,
				success: function (response) {
					if (response.success) {

						$('#view-passport').attr('href', response.data.Path);
						$('#view-passport').show();

						$('#delete-passport').attr('onclick', `DeletePassport(this,${response.data.ID})`);
						$('#delete-passport').show();

						$('#upload-passport').hide();

						$('#message').html(ChangeString("Passport uploaded successfully!", "تم تحميل جواز السفر بنجاح!")).addClass('success').slideDown();
						setTimeout(function () { $('#message').slideUp(); }, 6000);
						$('#upload-passport').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');
					} else {
						$('#message').html(response.message).addClass('success').slideDown();
						setTimeout(function () { $('#message').slideUp(); }, 6000);
						$('#upload-passport').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');
					}

					$('#choose-file-passport').val('');
				},
				error: function (e) {
					$('#message').html(ServerErrorShort).addClass('success').slideDown();
					setTimeout(function () { $('#message').slideUp(); }, 6000);
					$('#upload-passport').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');
					$('#choose-file-passport').val('');
				},
				failure: function (e) {
					$('#message').html(ServerErrorShort).addClass('success').slideDown();
					setTimeout(function () { $('#message').slideUp(); }, 6000);
					$('#upload-passport').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');
					$('#choose-file-passport').val('');
				}
			});
		}
		else {

			$('#message').html(ChangeString("Please select document first!", "الرجاء تحديد المستند أولاً!")).addClass('error').slideDown();
			setTimeout(function () { $('#message').slideUp(); }, 6000);
			$('#upload-passport').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');
		}

		return false;
	});
	/*Passport*/

	/*License GCC*/
	$('#upload-license-gcc').click(function () {
		$('#choose-file-license-gcc').trigger('click');
	});

	$('#choose-file-license-gcc').change(function () {
		$("#form-license-gcc").submit();
	});

	$("#form-license-gcc").submit(function () {

		var form = $(this);
		var formData = new FormData();

		var files = $("#choose-file-license-gcc").get(0).files;
		if (files.length > 0) {
			ButtonDisabled('#btn-license-gcc', true, true);

			formData.append("Path", files[0], files[0].name);

			formData.append("__RequestVerificationToken", $('input[name="__RequestVerificationToken"]').val());
			formData.append("Type", 'License');
			formData.append("IsGcc", true);
			if (LicenseGcc) {
				formData.append("ID", LicenseGcc.ID);
			}

			$('#upload-license-gcc').find('i').removeClass('fa-upload').addClass('fa-circle-notch').addClass('fa-spin');

			$.ajax({
				url: '/' + culture + '/Customer/Documents/CreateOrUpdate/',
				type: "POST",
				data: formData,
				contentType: false,
				processData: false,
				success: function (response) {
					if (response.success) {

						$('#view-license-gcc').attr('href', response.data.Path);
						$('#view-license-gcc').show();

						$('#delete-license-gcc').attr('onclick', `DeleteLicense(this,${response.data.ID})`);
						$('#delete-license-gcc').show();

						$('#upload-license-gcc').closest('.document').addClass('uploaded');
						$('#upload-license-gcc').hide();

						$('#message').html(ChangeString("License ( GCC ) uploaded successfully!", "تم تحميل جواز السفر بنجاح!")).addClass('success').slideDown();
						setTimeout(function () { $('#message').slideUp(); }, 6000);
						$('#upload-license-gcc').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');


						DisabledLicense(false, true, false);

					} else {
						$('#message').html(response.message).addClass('success').slideDown();
						setTimeout(function () { $('#message').slideUp(); }, 6000);
						$('#upload-license-gcc').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');
					}

					$('#choose-file-license-gcc').val('');

				},
				error: function (e) {
					$('#message').html(ServerErrorShort).addClass('success').slideDown();
					setTimeout(function () { $('#message').slideUp(); }, 6000);
					$('#upload-license-gcc').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');
					$('#choose-file-license-gcc').val('');
				},
				failure: function (e) {
					$('#message').html(ServerErrorShort).addClass('success').slideDown();
					setTimeout(function () { $('#message').slideUp(); }, 6000);
					$('#upload-license-gcc').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');
					$('#choose-file-license-gcc').val('');
				}
			});
		}
		else {

			$('#message').html(ChangeString("Please select document first!", "الرجاء تحديد المستند أولاً!")).addClass('error').slideDown();
			setTimeout(function () { $('#message').slideUp(); }, 6000);
			$('#upload-license-gcc').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');
		}

		return false;
	});
	/*License GCC*/

	/*License Non GCC*/
	$('#upload-license-non-gcc').click(function () {
		$('#choose-file-license-non-gcc').trigger('click');
	});

	$('#choose-file-license-non-gcc').change(function () {
		$("#form-license-non-gcc").submit();
	});

	$("#form-license-non-gcc").submit(function () {

		var form = $(this);
		var formData = new FormData();

		var files = $("#choose-file-license-non-gcc").get(0).files;
		if (files.length > 0) {
			ButtonDisabled('#btn-license-non-gcc', true, true);

			formData.append("Path", files[0], files[0].name);

			formData.append("__RequestVerificationToken", $('input[name="__RequestVerificationToken"]').val());
			formData.append("Type", 'License');
			formData.append("IsGcc", false);
			if (LicenseNonGcc) {
				formData.append("ID", LicenseNonGcc.ID);
			}

			$('#upload-license-non-gcc').find('i').removeClass('fa-upload').addClass('fa-circle-notch').addClass('fa-spin');

			$.ajax({
				url: '/' + culture + '/Customer/Documents/CreateOrUpdate/',
				type: "POST",
				data: formData,
				contentType: false,
				processData: false,
				success: function (response) {
					if (response.success) {

						$('#view-license-non-gcc').attr('href', response.data.Path);
						$('#view-license-non-gcc').show();

						$('#delete-license-non-gcc').attr('onclick', `DeleteLicenseNonGCC(this,${response.data.ID})`);
						$('#delete-license-non-gcc').show();

						$('#upload-license-non-gcc').closest('.document').addClass('uploaded');
						$('#upload-license-non-gcc').hide();

						$('#message').html(ChangeString("License ( Non GCC ) uploaded successfully!", "تم تحميل جواز السفر بنجاح!")).addClass('success').slideDown();
						setTimeout(function () { $('#message').slideUp(); }, 6000);
						$('#upload-license-non-gcc').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');

						DisabledLicense(true, false, false);

					} else {
						$('#message').html(response.message).addClass('success').slideDown();
						setTimeout(function () { $('#message').slideUp(); }, 6000);
						$('#upload-license-non-gcc').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');
					}

					$('#choose-file-license-non-gcc').val('');
				},
				error: function (e) {
					$('#message').html(ServerErrorShort).addClass('success').slideDown();
					setTimeout(function () { $('#message').slideUp(); }, 6000);
					$('#upload-license-non-gcc').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');
					$('#choose-file-license-non-gcc').val('');
				},
				failure: function (e) {
					$('#message').html(ServerErrorShort).addClass('success').slideDown();
					setTimeout(function () { $('#message').slideUp(); }, 6000);
					$('#upload-license-non-gcc').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');
					$('#choose-file-license-non-gcc').val('');
				}
			});
		}
		else {

			$('#message').html(ChangeString("Please select document first!", "الرجاء تحديد المستند أولاً!")).addClass('error').slideDown();
			setTimeout(function () { $('#message').slideUp(); }, 6000);
			$('#upload-license-non-gcc').find('i').addClass('fa-upload').removeClass('fa-circle-notch').removeClass('fa-spin');
		}

		return false;
	});
	/*License Non GCC*/
});

function GetDocuments() {
	$.ajax({
		type: 'GET',
		url: '/' + culture + '/Customer/Documents/GetMyDocuments',
		contentType: "application/json",
		success: function (response) {
			if (response.success) {

				var Passport = response.data.find(function (obj) {
					return obj.Type === 'Passport';
				});

				if (Passport) {
					$('#view-passport').attr('href', Passport.Path);
					$('#view-passport').show();

					$('#delete-passport').attr('onclick', `DeletePassport(this,${Passport.ID})`);
					$('#delete-passport').show();

					$('#upload-passport').closest('.document').addClass('uploaded');
					$('#upload-passport').hide();
				}

				var LicenseGcc = response.data.find(function (obj) {
					return obj.Type === 'License' && obj.IsGCC === true;
				});

				if (LicenseGcc) {
					$('#view-license-gcc').attr('href', LicenseGcc.Path);
					$('#view-license-gcc').show();

					$('#delete-license-gcc').attr('onclick', `DeleteLicenseGCC(this,${LicenseGcc.ID})`);
					$('#delete-license-gcc').show();

					$('#upload-license-gcc').closest('.document').addClass('uploaded');
					$('#upload-license-gcc').hide();

					DisabledLicense(false, true, false);
				}

				var LicenseNonGcc = response.data.find(function (obj) {
					return obj.Type === 'License' && obj.IsGCC === false;
				});

				if (LicenseNonGcc) {
					$('#view-license-non-gcc').attr('href', LicenseNonGcc.Path);
					$('#view-license-non-gcc').show();

					$('#delete-license-non-gcc').attr('onclick', `DeleteLicenseNonGCC(this,${LicenseNonGcc.ID})`);
					$('#delete-license-non-gcc').show();

					$('#upload-license-non-gcc').closest('.document').addClass('uploaded');
					$('#upload-license-non-gcc').hide();

					DisabledLicense(true, false, false);
				}
			}
		},
		error: function (e) {
			console.log("Get My Documents Error.");
		}
	});
}

function DeletePassport(elem, id) {
	$(elem).find('i').removeClass('fa-trash').addClass('fa-circle-notch').addClass('fa-spin');
	$.ajax({
		type: 'Get',
		url: '/' + culture + '/Customer/Documents/Delete?id=' + id,
		contentType: "application/json",
		success: function (response) {
			if (response.success) {

				$('#view-passport').attr('href', 'javascript:;');
				$('#view-passport').hide();

				$('#delete-passport').attr('onclick', ``);
				$('#delete-passport').hide();

				//$('#upload-passport').attr('onclick', `$("#form-passport").submit();`);
				$('#upload-passport').closest('.document').removeClass('uploaded');
				$('#upload-passport').show();

				$('#message').html(ChangeString("Passport deleted successfully!", "تم حذف كلمة المرور بنجاح!")).addClass('success').slideDown();
				setTimeout(function () { $('#message').slideUp(); }, 6000);
				$(elem).find('i').addClass('fa-trash').removeClass('fa-circle-notch').removeClass('fa-spin');

			} else {
				$('#message').html(response.message).addClass('error').slideDown();
				setTimeout(function () { $('#message').slideUp(); }, 6000);
				$(elem).find('i').addClass('fa-trash').removeClass('fa-circle-notch').removeClass('fa-spin');
			}
		},
		error: function (e) {
			$('#message').html(response.message).addClass('error').slideDown();
			setTimeout(function () { $('#message').slideUp(); }, 6000);
			$(elem).find('i').addClass('fa-trash').removeClass('fa-circle-notch').removeClass('fa-spin');
		}
	});
}

function DeleteLicenseGCC(elem, id) {
	$(elem).find('i').removeClass('fa-trash').addClass('fa-circle-notch').addClass('fa-spin');
	$.ajax({
		type: 'Get',
		url: '/' + culture + '/Customer/Documents/Delete?id=' + id,
		contentType: "application/json",
		success: function (response) {
			if (response.success) {

				$('#view-license-gcc').attr('href', 'javascript:;');
				$('#view-license-gcc').hide();

				$('#delete-license-gcc').attr('onclick', ``);
				$('#delete-license-gcc').hide();

				//$('#upload-license-gcc').attr('onclick', `$("#form-license-gcc").submit();`);
				$('#upload-license-gcc').closest('.document').removeClass('uploaded');
				$('#upload-license-gcc').show();

				$('#message').html(ChangeString("License (GCC) deleted successfully!", "تم حذف كلمة المرور بنجاح!")).addClass('success').slideDown();
				setTimeout(function () { $('#message').slideUp(); }, 6000);
				$(elem).find('i').addClass('fa-trash').removeClass('fa-circle-notch').removeClass('fa-spin');

				DisabledLicense(false, false, true);

			} else {
				$('#message').html(response.message).addClass('error').slideDown();
				setTimeout(function () { $('#message').slideUp(); }, 6000);
				$(elem).find('i').addClass('fa-trash').removeClass('fa-circle-notch').removeClass('fa-spin');
			}
		},
		error: function (e) {
			$('#message').html(response.message).addClass('error').slideDown();
			setTimeout(function () { $('#message').slideUp(); }, 6000);
			$(elem).find('i').addClass('fa-trash').removeClass('fa-circle-notch').removeClass('fa-spin');
		}
	});
}

function DeleteLicenseNonGCC(elem, id) {
	$(elem).find('i').removeClass('fa-trash').addClass('fa-circle-notch').addClass('fa-spin');
	$.ajax({
		type: 'Get',
		url: '/' + culture + '/Customer/Documents/Delete?id=' + id,
		contentType: "application/json",
		success: function (response) {
			if (response.success) {

				$('#view-license-non-gcc').attr('href', 'javascript:;');
				$('#view-license-non-gcc').hide();

				$('#delete-license-non-gcc').attr('onclick', ``);
				$('#delete-license-non-gcc').hide();

				//$('#upload-license-non-gcc').attr('onclick', `$("#form-license-non-gcc").submit();`);
				$('#upload-license-non-gcc').closest('.document').removeClass('uploaded');
				$('#upload-license-non-gcc').show();

				$('#message').html(ChangeString("License (Non GCC) deleted successfully!", "تم حذف كلمة المرور بنجاح!")).addClass('success').slideDown();
				setTimeout(function () { $('#message').slideUp(); }, 6000);

				$(elem).find('i').addClass('fa-trash').removeClass('fa-circle-notch').removeClass('fa-spin');

				DisabledLicense(false, false, true);

			} else {
				$('#message').html(response.message).addClass('error').slideDown();
				setTimeout(function () { $('#message').slideUp(); }, 6000);

				$(elem).find('i').addClass('fa-trash').removeClass('fa-circle-notch').removeClass('fa-spin');
			}
		},
		error: function (e) {
			$('#message').html(response.message).addClass('error').slideDown();
			setTimeout(function () { $('#message').slideUp(); }, 6000);

			$(elem).find('i').addClass('fa-trash').removeClass('fa-circle-notch').removeClass('fa-spin');
		}
	});
}

function DisabledLicense(DisabledGCC, DisabledNonGCC, EnabledBoth) {
	if (DisabledGCC) {
		$('#upload-license-gcc').closest('.document').addClass('disabled-permanent uploaded');
		$('#upload-license-gcc').hide();
	}
	if (DisabledNonGCC) {
		$('#upload-license-non-gcc').closest('.document').addClass('disabled-permanent uploaded');
		$('#upload-license-non-gcc').hide();
	}
	if (EnabledBoth) {
		$('#upload-license-gcc').closest('.document').removeClass('disabled-permanent uploaded');
		$('#upload-license-gcc').show();
		$('#upload-license-non-gcc').closest('.document').removeClass('disabled-permanent uploaded');
		$('#upload-license-non-gcc').show();
	}
}