$(document).ready(function () {

	$("#frm-customer-profile").submit(function () {

		$(this).find('.alert').remove();
		$('#btnSubmit').html('<span class="fa fa-spinner fa-spin"></span> Save').attr('disabled', true);
		$.ajax({
			url: "/Customer/Account/Profile/",
			type: "POST",
			data: $(this).serialize(),
			success: function (response) {
				if (response.success) {
					showErrorMsg($('#frm-customer-profile'), 'success', response.message);
				} else {
					showErrorMsg($('#frm-customer-profile'), 'danger', response.message);
				}
				$('#btnSubmit').html('Save').attr('disabled', false);
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
			$dropdown.append($("<option />").val('').text("Select City First!"));
			$('#AreaID').trigger('change');
		}
		else {
			$.ajax({
				type: 'Get',
				url: '/Areas/GetAreasByCity/' + $(this).val(),
				success: function (response) {
					$dropdown.empty();
					$dropdown.append($("<option />").val('').text("Select Area"));

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
	//$('#AreaID').change(function () {

	//	if ($(this).val()) {
	//		$('#btnCalculateShippingCost').prop('disabled', false);
	//	}
	//	else {
	//		$('#btnCalculateShippingCost').prop('disabled', true);
	//	}
	//	Order.DeliveryCharges = null;
	//	$('#Shipping').html('-');
	//	$('#subtotal-message').html('');
	//	$('#btnCalculateShippingCost').html('Calculate Shipping Cost');
	//	BindCartTotal();
	//});

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

