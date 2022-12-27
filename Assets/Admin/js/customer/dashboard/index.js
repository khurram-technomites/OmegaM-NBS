'use strict';

//#region Global Variables and Arrays
var customer = {
    Name: null,
    Contact: null,
    Email: null,
    Image: null,
};
let defaultImageName = $('.choose-file-span').text();
//#endregion

//#region document ready function
$(document).ready(function () {

    $('.page-container').css('min-height', 479);

    //#region dashboard sidebar and navigation settings*
    $('#dashboard').find('.wo-dbpostfooter').find('a').prop('href', `mailto:${InfoEmail}`).html(InfoEmail);
    if (location.hash) {
        if ($(location.hash)[0]) {
            ShowActiveContent(location.hash + '-n');
            //$(`a[href='${location.hash}']`).click();
        }
        else {
            ShowActiveContent('#dashboard-n');
        }
    }
    $('.wo-dnavbar__nav a').click(function () {
        ShowActiveContent($(this));
    });
    $('.wo-header-user a').click(function () {
        setTimeout(function () {
            if ($(location.hash)[0]) {
                ShowActiveContent(location.hash + '-n');
                //$(`a[href='${location.hash}']`).click();
            }
            else {
            }
        }, 150);
    });
    //#endregion

    //#region profile settings

    CutomerData();

    $('#btn-profile-edit').click(function () {
        $('#btn-profile-edit').hide();
        $('#btn-profile-close').show();
        ProfileSlideDown();
    });

    $('.btn-profile-cancel').click(function () {
        $('#btn-profile-edit').show();
        $('#btn-profile-close').hide();
        ProfileSlideUp();
    });

    //#region profile image settings
    $('#choose-file').change(function () {
        var i = $(this).prev('span').clone();
        try {
            var file = $('#choose-file')[0].files[0].name;
            $(this).prev('span').text(file);
        } catch (e) {
            $(this).prev('span').text(defaultImageName);
        }
    });
    //#endregion

    $("#form-profile").submit(function () {
        ButtonDisabled('#btn-profile', true, true);

        var form = $(this);
        var formData = new FormData();

        var files = $("#choose-file").get(0).files;
        if (files.length > 0) {
            formData.append("ImagePath", files[0], files[0].name);
        }
        formData.append("__RequestVerificationToken", $('input[name="__RequestVerificationToken"]').val());
        formData.append("Name", $('#Name').val());
        formData.append("Email", $('#Email').val());
        formData.append("Contact", $('#Contact').val());

        $.ajax({
            url: '/' + culture + '/Customer/Account/Profile/',
            type: "POST",
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.success) {
                    ShowFormAlert(form, 'success', response.message, 6);
                    var data = response.data;
                    $("#choose-file").val('').prev('span').text(defaultImageName);

                    $(".wo-dbpoststep__title").find('h3').text(lang == 'en' ? 'Welcome ' : 'اهلا وسهلا' + data.name);
                    $("#Name-Text").text(data.name);
                    $("#Email-Text").text(data.email);
                    $("#Image-Icon").attr('src', data.image);
                    $(".wo-user__img").find('img').attr('src', data.image);

                    CutomerData();
                } else {
                    ShowFormAlert(form, 'danger', response.message, 6);
                }
                ButtonDisabled('#btn-profile', false, false);
            },
            error: function (e) {
                ShowFormAlert(form, 'danger', ServerError, 6);
                ButtonDisabled('#btn-profile', false, false);
            },
            failure: function (e) {
                ShowFormAlert(form, 'danger', ServerError, 6);
                ButtonDisabled('#btn-profile', false, false);
            }
        });
        return false;
    });

    //#endregion

});
//#endregion

//#region Others Function
function ShowActiveContent(elem) {
    let id = $(elem).attr("href");
    location.hash = id;

    if ($('.dashboard-main:visible').length < 1) {
        $('.booking-details').hide();
        $('.dashboard-main').show();
    }

    if (id != '#profile-settings' && $('#profile-form-div:visible').length > 0) {
        $('#btn-profile-edit').show();
        $('#btn-profile-close').hide();
        ProfileSlideUp();
    }

    if (id != '#my-documents') {
        if ($('#license-gcc-form-div:visible').length > 0) {
            LicenseGccHide();
        }
        if ($('#license-non-gcc-form-div:visible').length > 0) {
            LicenseNonGccHide();
        }
        if ($('#passport-form-div:visible').length > 0) {
            PassportHide();
        }
    }

    if (id == '#alerts') {
        LoadAlerts();
    }
    else if (id == '#bookings') {
        LoadBookings();
    }
    else if (id == '#coupons') {
        LoadCoupons();
    }
    else if (id == '#my-documents') {
        LoadMyDocuments();
    }
    else if (id == '#profile-settings') {
        
    }
    else if (id == '#dashboard') {

    }

    $('.wo-dnavbar__nav a').removeClass("active");
    $(elem).addClass("active");

    $('.tab-pane').removeClass('active show');
    $(id).addClass('active show');
}

function CutomerData() {
    customer.Name = $("#Name-Text").text();
    customer.Email = $("#Email-Text").text();
    customer.Contact = $("#Contact-Text").text();
    customer.Image = $("#Image-Icon").attr('src');
}

function ProfileSlideDown() {
    $('#profile-form-div').slideDown();
}
function ProfileSlideUp() {
    $('#profile-form-div').slideUp();

    $("#Name").val(customer.Name);
    $("#Email").val(customer.Email);
    $("#choose-file").val('').prev('span').text(defaultImageName);
}

//#endregion
