$(document).ready(function () {
	$('#CustomerLoginForm').submit(function () {
		var form = $(this);
		$('#btnSignIn').html('<span class="fa fa-spinner fa-spin"></span> تسجيل الدخول').attr('disabled', true);
		$.ajax({
			url: '/Customer/Account/Login',
			type: 'Post',
			data: $(form).serialize(),
			success: function (response) {
				if (response.success) {

					showErrorMsg(form, 'success', getTranslatedString(response.message));
					localStorage.setItem("wishlist", JSON.stringify(response.wishlist));
					window.location.href = response.url;
				} else {
					if (!response.isEmailVerified) {

						$('.customer-password').hide();
						$('#btnSignIn').hide();

						$('#btnVerify').slideDown();
						$('#btnCancelVerify').slideDown();
					}
					showErrorMsg(form, 'danger', getTranslatedString(response.message));
					$('#btnSignIn').html('Sign In').attr('disabled', false);
				}
			},
			error: function (e) {
				showErrorMsg(form, 'danger', "عفوًا ، حدث خطأ ما. حاول تحديث هذه الصفحة أو لا تتردد في الاتصال بنا إذا استمرت المشكلة");
				$('#btnSignIn').html('تسجيل الدخول').attr('disabled', false);
			},
			failure: function (e) {
				showErrorMsg(form, 'danger', "عفوًا ، حدث خطأ ما. حاول تحديث هذه الصفحة أو لا تتردد في الاتصال بنا إذا استمرت المشكلة");
				$('#btnSignIn').html('تسجيل الدخول').attr('disabled', false);
			}
		});
		return false;
	});

	$('#btnVerify').click(function () {
		if ($('#EmailAddress').val()) {
			var form = $(this).closest('form');
			$("#btnVerify").html('<i class="fa fa-spinner fa-spin"></i> تحقق').prop('disabled', true);
			$("#btnCancelVerify").prop('disabled', true);
			$.ajax({
				url: '/Customer/Account/VerifyEmail',
				type: 'POST',
				data: {
					__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
					account: {
						EmailAddress: $('#EmailAddress').val()
					}
				},
				success: function (response) {
					if (response.success) {
						showErrorMsg(form, 'success', getTranslatedString(response.message));

						$('#btnCancelVerify').trigger('click');
					} else {
						showErrorMsg(form, 'danger', getTranslatedString(response.message));
						$("#btnVerify").prop('disabled', false);
						$("#btnCancelVerify").prop('disabled', false);
					}
				},
				error: function (e) {
					showErrorMsg(form, 'danger', "عفوًا ، حدث خطأ ما. حاول تحديث هذه الصفحة أو لا تتردد في الاتصال بنا إذا استمرت المشكلة");
					$('#btnSignIn').html('تسجيل الدخول').attr('disabled', false);
				},
				failure: function (e) {
					showErrorMsg(form, 'danger', "عفوًا ، حدث خطأ ما. حاول تحديث هذه الصفحة أو لا تتردد في الاتصال بنا إذا استمرت المشكلة");
					$('#btnSignIn').html('تسجيل الدخول').attr('disabled', false);
				}
			})
		}
	});

	$('#btnCancelVerify').click(function () {

		$('#btnSignIn').html('تسجيل الدخول').attr('disabled', false);

		$('#btnVerify').hide();
		$('#btnCancelVerify').hide();
		$('#btnSignIn').slideDown();
		$('.customer-password').slideDown();
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
		case "Login Successful":
			message = "تم تسجيل الدخول بنجاح";
			break;
		case "Wrong Password!":
			message = "كلمة مرور خاطئة";
			break;
		case "We just need to verify your email address before you can access ":
			message = "نحتاج فقط إلى التحقق من عنوان بريدك الإلكتروني قبل أن تتمكن من الوصول إليه";
			break;
		case "Account suspended!":
			message = "حساب معلق";
			break;
		case "Invalid Email or Password!":
			message = "البريد الإلكتروني أو كلمة السر خاطئة";
			break;
		case "Please enter your email and password first!":
			message = "الرجاء إدخال بريدك الإلكتروني وكلمة المرور أولاً";
			break;
		case "Please fill the form properly ...":
			message = "الرجاء تعبئة النموذج بشكل صحيح ...";
			break;
		case "Cool! Account verification instruction has been sent to your email.":
			message = "رائع! تم إرسال تعليمات التحقق من الحساب إلى بريدك الإلكتروني";
			break;
		case "Oops! Something went wrong. Please try later.":
			message = "وجه الفتاة! حدث خطأ ما ، يرجى المحاولة لاحقًا";
			break;
		case "Invalid Email.":
			message = "بريد إلكتروني خاطئ.";
			break;
		default:
			message = message;
	}

	return message;
}