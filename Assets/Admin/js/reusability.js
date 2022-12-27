"use strict";

//#region validation form submission on simple page
function validateForm(form) {
    let formValidate = true;

    //insert, remove validate message on input fields
    $.each($(form).find('.required-custom'), function (k, elem) {
        if (
            (!$(elem).attr('multiple') && $(elem).val())
            ||
            ($(elem).attr('multiple') && $(elem).val().length)
        ) {
            if ($(elem).hasClass('password') || $(elem).hasClass('email') || $(elem).hasClass('confirm-password')) {
                if ($(elem).closest('.form-group').find('span.field-validation-valid').text() && $(elem).hasClass('border-danger')) {
                }
                else {
                    $(elem).closest('.form-group').find('span.field-validation-valid').text('');
                }
            }
            else {
                $(elem).closest('.form-group').find('span.field-validation-valid').text('');
            }
        }
        else {
            $(elem).closest('.form-group').find('span.field-validation-valid').text($(elem).closest('.form-group').find('label').text() + " is required.");
        }
    });

    //check if there is non validate input fields
    $.each($(form).find('.required-custom'), function (k, elem) {
        if ($(elem).closest('.form-group').find('span.field-validation-valid').text()) {
            formValidate = false;

            return false; // breaks
        }
        else {
            formValidate = true;
        }
    });

    //return true of false
    if (formValidate) {
        return true;
    }
    else {
        return false;
    }
}

function custom_form_view_submit(form) {


    let btn = $(`button[form="${$(form).attr('id')}"]`);
    if (!btn) {
        btn = $(form).find('button[type="submit"]');
    }
    disableSubmitButton(btn, true);

    $.ajax({
        url: $(form).attr('action'),
        type: 'Post',
        data: $(form).serialize(),
        success: function (response) {
            if (response.success) {
                toastr.success(response.message)
                disableSubmitButton(btn, false);
                $(form).find('.input-fields').val('').removeClass("border border-success border-danger");;
                $(form).find('.input').removeClass("border border-success border-danger");;

                if (typeof custom_form_view_callback === "function")
                    custom_form_view_callback(form, response);
            }
            else {
                toastr.error(response.message)
                if (typeof custom_form_view_callback === "function")
                    custom_form_view_callback(form, response);
                disableSubmitButton(btn, false);
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            ErrorFunction(xhr, ajaxOptions, thrownError, btn);
        },
        failure: function (xhr, ajaxOptions, thrownError) {
            ErrorFunction(xhr, ajaxOptions, thrownError, btn);
        }
    });

}

function form_file_custom(form) {
    /*
    *	this function is use for form which have some files, in edit form  we need to write attribute (data-value = db.file.path) for delete and update proecess
    * */
    if (validateForm(form)) {
        let btn = $(`button[form="${$(form).attr('id')}"]`);
        if (!btn) {
            btn = $(form).find('button[type="submit"]');
        }
        disableSubmitButton(btn, true);

        var data = new FormData();

        $.each($(form).find('input'), function (k, elem) {
            if ($(elem).attr('type') == "file") {

                let files = $(elem)[0].files;
                if (files.length) {
                    data.append($(elem).attr('name'), files[0]);
                }
                else {
                    data.append($(elem).attr('name'), $(elem).attr('value'));
                }
            }
            if ($(elem).attr('type') == "checkbox") {
                data.append($(elem).attr('name'), $(elem).prop("checked"));
            }
            else {
                data.append($(elem).attr('name'), $(elem).val());
            }
        });
        $.each($(form).find('textarea'), function (k, elem) {
            data.append($(elem).attr('name'), $(elem).val());
        });
        $.each($(form).find('select'), function (k, elem) {
            data.append($(elem).attr('name'), $(elem).val());
        });

        $.ajax({
            url: $(form).attr('action'),
            type: 'Post',
            contentType: false, // Not to set any content header
            processData: false, // Not to process data
            data: data,
            success: function (response) {
                if (response.success) {
                    if (typeof callback === "function") {
                        callback($('#myModalContent'), element, true, response);
                    }
                }
                else {

                    toastr.error(response.message)
                    if (typeof form_file_custom_callback === "function")
                        form_file_custom_callback(form, response);
                    disableSubmitButton(btn, false);
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ErrorFunction(xhr, ajaxOptions, thrownError, btn);
            },
            failure: function (xhr, ajaxOptions, thrownError) {
                ErrorFunction(xhr, ajaxOptions, thrownError, btn);
            }
        });
    }
    return false;
}

function datePicker() {
    $(".bstrp-datepicker").datepicker({
        todayHighlight: true,
        format: 'dd MM yyyy',
    });
}

//#endregion