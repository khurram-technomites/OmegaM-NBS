'use strict'

$(document).ready(function () {

    $("#FilePath").change(function (e) {
        var file = this.files;
        if ((file.length > 4)) {
            $('#message').html('<span class="fa fa-warning mr-1"></span> ' + 'Maximum four images can be upload !')
            $('#message').slideDown();
            setTimeout(function () {
                $('#message').slideUp();
            }, 6000);
            $("#FilePath").val("");
        }
    });


    $('#FormPrescription').submit(function () {
        $('#btnSubmit').html('<span class="fa fa-spinner fa-spin"></span> Submit').attr('disabled', true);
        return true;
    });
})