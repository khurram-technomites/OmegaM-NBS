'use strict'

//#region document ready function
$(document).ready(function () {

    $("#loginpopup").on('shown', function () {
        $("#signupContact").focus();
    });

    /*Signup*/
    $('#AuthenticationFormSignup').submit(function () {
        var form = $(this);
        let contact = $('#signupContact').val().replace("+-e", "");
        if (contact.length != 9) {
            ShowFormAlert(form, 'danger', lang == 'en' ? "Mobile number should have 9 digits !" : "يجب أن يتكون رقم الهاتف المحمول من 9 أرقام!", 6);
            BorderDangerInput('#signupContact', 6);
        }
        else {
            ButtonDisabled('#btnSignup', true, true);
            $.ajax({
                url: "/" + culture + '/Customer/Account/Signup',
                type: 'Post',
                data: {
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                    Contact: Number("971" + contact),
                },
                success: function (response) {
                    if (response.success) {
                        if (response.isOTPSent) {
                            ShowFormAlert(form, 'success', response.message, 6);
                            setTimeout(function () {
                                $('#signUpModal').slideUp();
                                $("#signupContact").val('');
                                $('#signUpOTPModal').slideDown();
                                $('#otpContact').html(response.contact);
                                $("#digit-1").focus();
                                $('#btnSignup').html($('#btnSignup').text()).attr('disabled', false);
                                TimerOTP(116); // Countdown Timer
                            }, 500);
                        } else {
                            ShowFormAlert(form, 'danger', ServerError, 6);
                            ButtonDisabled('#btnSignup', false, false);
                        }
                    } else {
                        ShowFormAlert(form, 'danger', response.message, 6);
                        ButtonDisabled('#btnSignup', false, false);
                        if (response.description) {
                            $(form).prepend(response.description);
                        }
                    }
                }
            });
        }

        return false;
    });
    /*End Signup*/

    /*OTP Verification*/
    $('#AuthenticationFormOTPVerify').submit(function () {

        var form = $(this);

        if ($('#otp:visible').length > 0) {

            var OTP = "";
            $('.digit-group').find('input').each(function () {
                OTP = OTP + $(this).val();
            });
            var otp = Number(OTP);
            var contact = $('#otpContact').html();
            contact = contact.replace("+-e", "");

            if (otp == null || otp == NaN || otp == "") {
                ShowFormAlert(form, 'danger', lang == 'en' ? "Invalid OTP!" : "كلمة مرور صالحة لمرة واحدة!", 6);
            }
            else if (contact == null || contact == "") {
                ShowFormAlert(form, 'danger', lang == 'en' ? "Invalid Contact number!" : "رقم الاتصال غير صحيح!", 6);
            }
            else {
                ButtonDisabled('#btnOtpSubmit', true, true)
                $.ajax({
                    url: "/" + culture + '/Customer/Account/verifyOTP',
                    type: 'Post',
                    data: {
                        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                        Contact: contact,
                        otp: otp,
                    },
                    success: function (response) {

                        if (response.success) {
                            ShowFormAlert(form, 'success', response.message, 6);
                            $("#timer").hide();


                            var title = $('title').text().replace(ChangeString('NowBuySell','او ام دبليو'), '').replace('|', '').trim();
                            if (title.startsWith(ChangeString("Car Details","تفاصيل السيارة"))) {
                                // booking details page
                                $('.wo-header-user .customer-name').html(`<em>${response.CustomerName ? response.CustomerName : ``}</em>`);
                                $('.wo-header-user .layout-profile-icon').attr('src',`${response.Photo ? response.Photo : `/Assets/images/default/default-omw-avatar.png`}`);
                                $('.wo-header-user').show();
                                $('#LiMyAccount').show();
                                $('#LiLogout').show();

                                $('.wo-userlogin').remove();
                                $('#LiLogin').remove();

                                $('#btnLoginToProceed').remove();
                                $('#btnBooking').show();
                                $("#loginpopup").modal("hide");

                                GetDocuments();
                                session = true;

                            } else {
                                setTimeout(function () {
                                    window.location = "/" + culture + response.url;
                                }, 500);
                            }
                        } else {
                            ShowFormAlert(form, 'danger', response.message, 6);
                            ButtonDisabled('#btnOtpSubmit', false, false);
                        }
                    },
                    error: function (e) {
                        ShowFormAlert(form, 'danger', ServerError, 6);
                        ButtonDisabled('#btnOtpSubmit', false, false);
                    },
                    failure: function (e) {
                        ShowFormAlert(form, 'danger', ServerError, 6);
                        ButtonDisabled('#btnOtpSubmit', false, false);
                    }
                });
            }
        }
        return false;
    });

    $('#otpResend').click(function () {

        var form = $(this).closest('form');
        $('.digits').val('');
        var contact = $('#otpContact').html();
        $('#otpResend').slideUp();
        ButtonDisabled('#btnOtpSubmit', true, true);
        $.ajax({
            url: "/" + culture + '/Customer/Account/resendOTP',
            type: 'Post',
            data: {
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                Contact: contact,
            },
            success: function (response) {

                if (response.success) {
                    ShowFormAlert(form, 'success', response.message, 6);
                    ButtonDisabled('#btnOtpSubmit', false, false);
                    $('#btnOtpSubmit').slideDown();
                    TimerOTP(120); // Countdown Timer
                } else {
                    ShowFormAlert(form, 'danger', response.message, 6);
                    $('#otpResend').slideDown();
                }
            },
            error: function (e) {
                ShowFormAlert(form, 'danger', ServerError, 6);
                ButtonDisabled('#btnOtpSubmit', false, false);
                $('#btnOtpSubmit').slideDown();
                $('#otpResend').slideDown();
            },
            failure: function (e) {
                ShowFormAlert(form, 'danger', ServerError, 6);
                ButtonDisabled('#btnOtpSubmit', false, false);
                $('#btnOtpSubmit').slideDown();
                $('#otpResend').slideDown();
            }
        });
        return false;
    });

    /*End OTP Verification*/

    $('#otp-closed').click(function () {
        $("#signupContact").val('');

        if ($('#otp:visible').length > 0) {
            $('#signUpOTPModal').slideUp();
            $('#otpResend').slideUp();
            $('#otpContact').html('');
            //$('#btnOtpSubmit').attr('disabled', true).slideUp();
            $('#signUpModal').slideDown();
            $("#signupContact").focus();
            $('#btnSignup').html($('#btnSignup').text()).attr('disabled', false);
            $('.digits').val('');
        }
    });

});
//#endregion

//#region Others Function

$('.digit-group').find('input').each(function () {
    $(this).attr('maxlength', 1);
    $(this).on('keyup', function (e) {
        var parent = $($(this).parent());

        if (e.keyCode === 8 || e.keyCode === 37) {
            var prev = parent.find('input#' + $(this).data('previous'));

            if (prev.length) {
                $(prev).select();
            }
        } else if ((e.keyCode >= 48 && e.keyCode <= 57) || (e.keyCode >= 65 && e.keyCode <= 90) || (e.keyCode >= 96 && e.keyCode <= 105) || e.keyCode === 39) {
            var next = parent.find('input#' + $(this).data('next'));

            if (next.length) {
                $(next).select();
            } else {
                if (parent.data('autosubmit')) {
                    parent.submit();
                }
            }
        }
    });
});

function TimerOTP(time) {

    let timerOn = true;

    function timer(remaining) {
        var m = Math.floor(remaining / 60);
        var s = remaining % 60;

        m = m < 10 ? '0' + m : m;
        s = s < 10 ? '0' + s : s;
        document.getElementById('timer').innerHTML = m + ':' + s;
        remaining -= 1;

        if (remaining >= 0 && timerOn) {
            setTimeout(function () {
                timer(remaining);
            }, 1000);
            return;
        }

        if (!timerOn) {
            // Do validate stuff here
            return;
        }

        // Do timeout stuff here
        //alert('Timeout for otp');
        $('#btnOtpSubmit').attr('disabled', true).slideUp();
        $('#otpResend').slideDown();
    }

    timer(time); //seconds
}

//#endregion
