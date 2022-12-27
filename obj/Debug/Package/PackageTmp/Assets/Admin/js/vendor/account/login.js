﻿"use strict";

// Class Definition
var KTLogin = function () {
	var _login;

	var showErrorMsg = function (form, type, msg) {
		var alert = $('<div class="mb-10 kt-alert kt-alert--outline  max-width-alert alert alert-' + type + ' " role="alert">\
			<button type="button" class="close" data-dismiss="alert" aria-label="Close"><i class="fa fa-times"></i></button>\
			<span></span>\
		</div>');

		form.find('.alert').remove();
		alert.prependTo(form);
		//alert.animateClass('fadeIn animated');
		KTUtil.animateClass(alert[0], 'fadeIn animated');
		alert.find('span').html(msg);
	}

	var _showForm = function (form) {
		var cls = 'login-' + form + '-on';
		var form = 'kt_login_' + form + '_form';

		_login.removeClass('login-forgot-on');
		_login.removeClass('login-signin-on');
		_login.removeClass('login-signup-on');

		_login.addClass(cls);

		KTUtil.animateClass(KTUtil.getById(form), 'animate__animated animate__backInUp');
	}

	var _handleSignInForm = function () {
		var validation;

		// Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
		validation = FormValidation.formValidation(
			KTUtil.getById('kt_login_signin_form'),
			{
				fields: {
					EmailAddress: {
						validators: {
							notEmpty: {
								message: 'Email is required'
							},
							emailAddress: {
								message: 'Invalid email address'
							}
						}
					},
					Password: {
						validators: {
							notEmpty: {
								message: 'Password is required'
							}
						}
					}
				},
				plugins: {
					//trigger: new FormValidation.plugins.Trigger(),
					//submitButton: new FormValidation.plugins.SubmitButton(),
					////defaultSubmit: new FormValidation.plugins.DefaultSubmit(), // Uncomment this line to enable normal button submit after form validation
					bootstrap: new FormValidation.plugins.Bootstrap()
				}
			}
		);

		$('#kt_login_signin_submit').on('click', function (e) {
			e.preventDefault();

			var form = $(this).closest('form');
			validation.validate().then(function (status) {
				if (status == 'Valid') {
					$('#kt_login_signin_submit').addClass('spinner spinner-sm spinner-left').attr('disabled', true);
					$.ajax({
						url: '/Vendor/Account/Login',
						type: 'Post',
						data: $(form).serialize(),
						success: function (response) {
							// similate 2s delay
							if (response.success == true) {
								showErrorMsg(form, 'success', response.message);
								window.location.href = response.url;
							} else {

								showErrorMsg(form, 'danger', response.message);
								$('#kt_login_signin_submit').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);

								if (response.errorCode == 2) {
									//window.location.href = "";
								}
							}
						}
					});
				} else {
					swal.fire({
						text: "Sorry, looks like there are some errors detected, please try again.",
						icon: "error",
						buttonsStyling: false,
						confirmButtonText: "Ok, got it!",
						customClass: {
							confirmButton: "btn font-weight-bold btn-light-primary"
						}
					}).then(function () {
						KTUtil.scrollTop();
					});
				}
			});
		});

		// Handle forgot button
		$('#kt_login_forgot').on('click', function (e) {
			e.preventDefault();
			_showForm('forgot');
		});

		// Handle signup
		$('#kt_login_signup').on('click', function (e) {
			e.preventDefault();
			_showForm('signup');
		});
	}

	var _handleForgotForm = function (e) {
		var validation;

		// Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
		validation = FormValidation.formValidation(
			KTUtil.getById('kt_login_forgot_form'),
			{
				fields: {
					EmailAddress: {
						validators: {
							notEmpty: {
								message: 'Email address is required'
							},
							emailAddress: {
								message: 'The value is not a valid email address'
							}
						}
					}
				},
				plugins: {
					//trigger: new FormValidation.plugins.Trigger(),
					bootstrap: new FormValidation.plugins.Bootstrap()
				}
			}
		);

		// Handle submit button
		$('#kt_login_forgot_submit').on('click', function (e) {
			e.preventDefault();
			var form = $(this).closest('form');
			validation.validate().then(function (status) {
				if (status == 'Valid') {
					$('#kt_login_forgot_submit').addClass('spinner spinner-sm spinner-left').attr('disabled', true);
					// Submit form
					$.ajax({
						url: '/Vendor/Account/ForgetPassword',
						type: 'Post',
						data: $(form).serialize(),
						success: function (response) {
							// similate 2s delay
							if (response.success == true) {
								showErrorMsg(form, 'success', response.message);
								//window.location.href = response.url;
								$('input[name=EmailAddress]').val('');
							} else {
								showErrorMsg(form, 'danger', response.message);
							}
							$('#kt_login_forgot_submit').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
						}
					});
					KTUtil.scrollTop();
				} else {
					swal.fire({
						text: "Sorry, looks like there are some errors detected, please try again.",
						icon: "error",
						buttonsStyling: false,
						confirmButtonText: "Ok, got it!",
						customClass: {
							confirmButton: "btn font-weight-bold btn-light-primary"
						}
					}).then(function () {
						KTUtil.scrollTop();
					});
				}
			});
		});

		// Handle cancel button
		$('#kt_login_forgot_cancel').on('click', function (e) {
			e.preventDefault();

			_showForm('signin');
		});
	}

	// Public Functions
	return {
		// public functions
		init: function () {
			_login = $('#kt_login');

			_handleSignInForm();
			_handleForgotForm();
		}
	};
}();

// Class Initialization
jQuery(document).ready(function () {
	KTLogin.init();
});
