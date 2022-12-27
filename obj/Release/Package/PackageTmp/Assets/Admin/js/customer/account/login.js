$(document).ready(function () {
    $('#CustomerLoginForm').submit(function () {
        var form = $(this);

        var titleSplit = $('.iti__selected-flag').get(0).title.split(' ');
        titleSplit.reverse();
        var code = titleSplit[0].replace("+", "");
        let contact = $('#phone').val().replace("+", "");
        contact = code + contact;
        $('#Contact').val(contact);

        $('#btnSignIn').html('<span class="fa fa-spinner fa-spin"></span> Sign In').attr('disabled', true);
        $.ajax({
            url: $(form).attr('action'),
            type: 'Post',
            data: $(form).serialize(),
            success: function (response) {
                
                if (response.success) {
                    showErrorMsg(form, 'success', response.message);
                    localStorage.setItem("wishlist", JSON.stringify(response.wishlist));
                    setTimeout(function () {
                        window.location.href = response.url;
                    }, 3000);
                } else {

                    if (response.IsSrilankan) {
                        setTimeout(function () {
                            window.location.href = "/Customer/Account/VerifyOTP/#" + response.Contact;
                        }, 4000);
                    }

                    if (!response.isEmailVerified) {

                        $('.customer-password').hide();
                        $('.customer-number').hide();
                        $('#btnSignIn').hide();

                        $('#EmailAddress').attr('required', true);
                        $('.customer-email').slideDown();
                        $('#btnVerify').slideDown();
                        $('#btnCancelVerify').slideDown();
                    }
                    showErrorMsg(form, 'danger', response.message);
                    $('#btnSignIn').html('Sign In').attr('disabled', false);
                }
            },
            error: function (e) {
                showErrorMsg(form, 'danger', "Ooops, something went wrong.Try to refresh this page or feel free to contact us if the problem persists.");
                $('#btnSignIn').html('Sign In').attr('disabled', false);
            },
            failure: function (e) {
                showErrorMsg(form, 'danger', "Ooops, something went wrong.Try to refresh this page or feel free to contact us if the problem persists.");
                $('#btnSignIn').html('Sign In').attr('disabled', false);
            }
        });
        return false;
    });

    $('#btnVerify').click(function () {
        if ($('#EmailAddress').val()) {
            var form = $(this).closest('form');
            $("#btnVerify").html('<i class="fa fa-spinner fa-spin"></i> Verify').prop('disabled', true);
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
                        showErrorMsg(form, 'success', response.message);
                        $("#btnVerify").html('Verify').prop('disabled', false);
                        $("#btnCancelVerify").prop('disabled', false);
                        $('#btnCancelVerify').trigger('click');
                    } else {
                        showErrorMsg(form, 'danger', response.message);
                        $("#btnVerify").html('Verify').prop('disabled', false);
                        $("#btnCancelVerify").prop('disabled', false);
                    }
                },
                error: function (e) {
                    alert(e);
                },
                failure: function (e) {
                    alert(e);
                }
            })
        }
    });

    $('#btnCancelVerify').click(function () {

        $('#btnSignIn').html('Sign In').attr('disabled', false);

        $('#btnVerify').hide();
        $('#btnCancelVerify').hide();
        $('#btnSignIn').slideDown();
        $('.customer-number').slideDown();
        $('.customer-password').slideDown();
        $('.customer-email').slideUp();
        $('#EmailAddress').attr('required', false);

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