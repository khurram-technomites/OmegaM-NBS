$(document).ready(function () {

	$("#frm-customer-profile").submit(function () {

		$(this).find('.alert').remove();
		$('#btnSubmit').html('<span class="fa fa-spinner fa-spin"></span> حفظ').attr('disabled', true);
		$.ajax({
			url: "/Customer/Account/Profile/",
			type: "POST",
			data: $(this).serialize(),
			success: function (response) {
				if (response.success) {
					showErrorMsg($('#frm-customer-profile'), 'success', getTranslatedString(response.message));
				} else {
					showErrorMsg($('#frm-customer-profile'), 'danger', getTranslatedString(response.message));
				}
				$('#btnSubmit').html('حفظ').attr('disabled', false);
			},
			error: function (er) {
				toastr.error(er);
			}
		});
		return false;
	});

	$('#CityID').change(function () {
		var count = 0;
		var $dropdown = $("#AreaID");
		if ($(this).val() == 0) {
			$dropdown.empty();
			$dropdown.append($("<option />").val('').text("حدد المدينة أولاً!"));
			$('#AreaID').trigger('change');
		}
		else {
			$.ajax({
				type: 'Get',
				url: '/Areas/GetAreasByCity/' + $(this).val(),
				success: function (response) {
					$dropdown.empty();
					$dropdown.append($("<option />").val('').text("حدد المنطقة"));

					$.each(response.data, function (k, v) {
						$dropdown.append($("<option />").val(v.value).text(v.text));
						count++;
					});
				}
			});
		}
	});
	if (!$('#AreaID').val()) {
		$('#CityID').trigger('change');
	}

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
		case "Profile updated!":
			message = "تحديث الملف الشخصي!";
			break;
		case "Account already exist!":
			message = "الحساب موجود مسبقا";
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
