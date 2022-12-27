$(document).ready(function () {

    $('#CustomerSignupForm').submit(function () {

        var form = $(this);        
        var titleSplit = $('.iti__selected-flag').get(0).title.split(' ');
        titleSplit.reverse();
        var code = titleSplit[0].replace("+", "");
        let contact = $('#phone').val().replace("+", "");
        contact = code + contact;
        $('#Contact').val(contact);

        if (code == "94") {
            $('#IsSrilankan').val("true");
        }
        else {
            $('#IsSrilankan').val("false");
        }

        if (code == "94" && contact.length != 11) {
            showErrorMsg(form, 'danger', "Contact number should have 9 digits ...");
            $('#phone').closest('div').addClass('border border-danger');
            setTimeout(function () {
                $('#phone').closest('div').removeClass('border border-danger');
            }, 6000);
        }
        else {
            $('#btnSignup').html('<span class="fa fa-spinner fa-spin"></span> Signup').attr('disabled', true);

            if ($('#Referral').val()) {
                $("#btnApplyReferral").html('<i class="fa fa-spinner fa-spin"></i> Submit').prop('disabled', true);
                $.ajax({
                    url: '/Customer/Account/Referral',
                    type: 'POST',
                    data: {
                        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                        referral: {
                            Email: $('#Referral').val()
                        }
                    },
                    success: function (response) {
                        if (response.success) {
                            $('#ReferralID').val(response.ReferralID);

                            $.ajax({
                                url: $(form).attr('action'),
                                type: 'Post',
                                data: $(form).serialize(),
                                success: function (response) {
                                    if (response.success) {
                                        showErrorMsg(form, 'success', response.message);
                                        setTimeout(function () {
                                            window.location.href = response.url;
                                        }, 5000);
                                    } else {
                                        showErrorMsg(form, 'danger', response.message);
                                        $('#btnSignup').html('Signup').attr('disabled', false);
                                        if (response.description) {
                                            $(form).prepend$(response.description);
                                        }
                                    }
                                }
                            });
                        }
                        else {
                            showErrorMsg(form, 'danger', response.message);
                            $('#Referral').removeClass('border-success').addClass('border-danger');
                            $('#btnSignup').html('Signup').attr('disabled', true);
                            $('.btn-cancel-referral-container').slideDown();
                        }
                    }
                })
            } else {
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
                            $('#btnSignup').html('Signup').attr('disabled', false);
                            if (response.description) {
                                $(form).prepend$(response.description);
                            }
                        }
                    }
                });
            }
        }

        return false;
    })

    $('#btnCancelReferral').click(function () {

        $('#ReferralID').val('');
        $('#Referral').val("");

        $('#Referral').removeClass('border-danger').removeClass('border-success');
        $('#btnSignup').html('Signup').attr('disabled', false);
        $('.btn-cancel-referral-container').slideUp();
    });

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
    setTimeout(function () {
        $(alert).slideUp();
    }, 6000);
    alert.find('span').html(msg);
}

function isNumber(evt) {
    evt = (evt) ? evt : window.event;
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
        return false;
    }
    return true;
}