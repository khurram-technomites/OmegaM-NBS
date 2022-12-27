var IsDeliveryAddressesLoaded = false;

$(document).ready(function () {
});

function GetDeliveryAddresses() {
	if (!IsDeliveryAddressesLoaded) {
		$.ajax({
			type: 'GET',
			url: `/customer/deliveryaddress/getall?lang=${lang}`,
			success: function (response) {
				IsDeliveryAddressesLoaded = true;
				BindDeliveryAddresses(response);
			}
		});
	}
}

function BindDeliveryAddresses(response) {

	var htmlTemplate = '';
	$("#ShippingAddress").empty();
	$.each(response.data, function (k, v) {
		htmlTemplate += `<div class="delivery-address col-md-5 col-sm-12 m-2">
							<a class="delete margin-5px-bottom btn-soft-danger font-weight-normal btn-sm" href="javascript:;" onclick="DeleteDeliveryAddresses(this,${v.ID})">
								<i class="fa fa-trash"></i> Delete
							</a>
							<p>
								<i class ="fa fa-tag"></i>${v.Type}
								<br>
								<i class="fa fa-map-marker-alt"></i>${v.Address},
								<br> ${v.Area.name},
								<br> ${v.State.name}, ${v.Country.name}.
							</p>
							<div class="custom-control custom-radio mb-2 mt-n2">
								<input class="custom-control-input myradio" name="radioopt" id="isDefault_${v.ID}" type="radio" ${v.IsDefault == true ? "checked disabled" : ""} onclick="IsDefaultDeliveryAddresses(this,${v.ID})">
								<label class="custom-control-label form-label" for="isDefault_${v.ID}" style="color:#747474;" onselectstart="return false">IsDefault</label>
							</div>
						</div>`;

	});

	$("#ShippingAddress").append(htmlTemplate);

	if (!$("#ShippingAddress").html().trim()) {

		$(".filter-loader").hide();
		$("#load-more").hide();
		$("#ShippingAddress").removeClass('row').html('<span class="text-mute text-center no-address">No shipping address</span>');

	} else {
		//$('.total-amount').each(function () {
		//	$(this).html(kFormatter(Number($(this).text()))).removeClass('total-amount');
		//});
	}

	$(".filter-loader").hide();
	$("#load-more").hide();
}

function IsDefaultDeliveryAddresses(element, Id) {

	$('.myradio').removeAttr("disabled");
	$(element).prop("disabled", true);

	$.ajax({
		url: '/customer/DeliveryAddress/isDefault/' + Id,
		type: 'Get',
		success: function (response) {
			if (response.success) {
				$('#message').html('<span class="fa fa-check"></span> <span class="message">' + response.message + '</span>');
				alertMsg();

			} else {
				$('#message').html('<span class="fa fa-warning mr-1"></span> ' + response.message);
				alertMsg();

			}
		},
		error: function (e) {
			$('#message').html('<span class="fa fa-warning mr-1"></span> <span class="message">Ooops, something went wrong.Try to refresh this page or feel free to contact us if the problem persists.</span>')
			alertMsg();

		},
		failure: function (e) {
			$('#message').html('<span class="fa fa-warning mr-1"></span> <span class="message">Ooops, something went wrong.Try to refresh this page or feel free to contact us if the problem persists.</span>')
			alertMsg();

		}
	});

}


function DeleteDeliveryAddresses(element, Id) {

	$(element).html('<span class="fa fa-spinner fa-spin"></span> Delete').attr('disabled', true);

	$.ajax({
		url: '/customer/DeliveryAddress/Delete/' + Id,
		type: 'Delete',
		data: {
			__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
		},
		success: function (response) {
			if (response.success) {
				$('#message').html('<span class="fa fa-check"></span> <span class="message">' + response.message + '</span>');
				alertMsg();

				$(element).closest('.delivery-address').remove();
			} else {
				$('#message').html('<span class="fa fa-warning mr-1"></span> ' + response.message);
				alertMsg();

				$(element).html('Delete').attr('disabled', false);

			}
		},
		error: function (e) {
			$('#message').html('<span class="fa fa-warning mr-1"></span> Ooops, something went wrong.Try to refresh this page or feel free to contact us if the problem persists.')
			alertMsg();

			$(element).html('Delete').attr('disabled', false);
		},
		failure: function (e) {
			$('#message').html('<span class="fa fa-warning mr-1"></span> Ooops, something went wrong.Try to refresh this page or feel free to contact us if the problem persists.')
			alertMsg();

			$(element).html('Delete').attr('disabled', false);
		}
	});
}

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

function callback(dialog, elem, isedit, response) {

	if (response.success) {
		$('#message').html('<span class="fa fa-check"></span> <span class="message">' + response.message + '</span>')
		alertMsg();
		v = response.data;

		$("#ShippingAddress").append(`<div class="delivery-address col-md-5 col-sm-12 m-2">
							<a class="delete margin-5px-bottom btn-soft-danger font-weight-normal btn-sm" href="javascript:;" onclick="DeleteDeliveryAddresses(this,${v.ID})">
								<i class="fa fa-trash"></i> Delete
							</a>
							<p>
								<i class ="fa fa-tag"></i>${v.Type}
								<br>
								<i class="fa fa-map-marker-alt"></i>${v.Address},
								<br> ${v.Area.name},
								<br> ${v.State.name}, ${v.Country.name}.
							</p>
							<div class="custom-control custom-radio mb-2 mt-n2">
								<input class="custom-control-input myradio" name="radioopt" id="isDefault_${v.ID}" type="radio" ${v.IsDefault == true ? "checked disabled" : ""} onclick="IsDefaultDeliveryAddresses(this,${v.ID})">
								<label class="custom-control-label form-label" for="isDefault_${v.ID}" style="color:#747474;" onselectstart="return false">IsDefault</label>
							</div>
						</div>`);

		

		if ($('.no-address')) {
			$('.no-address').remove();
        }


		jQuery('form', dialog).closest('.modal').find('button[type=submit]').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
		jQuery('#myModal').modal('hide');

	}
	else {
		jQuery('form', dialog).closest('.modal').find('button[type=submit]').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
		$('#message').html('<span class="fa fa-warning mr-1"></span> ' + response.message);
		alertMsg();
	}
}

function alertMsg() {
	$('#message').slideDown();
	setTimeout(function () { $('#message').slideUp(); }, 4000);
}