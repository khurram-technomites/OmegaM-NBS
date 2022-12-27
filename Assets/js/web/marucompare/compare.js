'use strict';
$(document).ready(function () {

});

function HideService(element, id) {
    $.ajax({
        url: "/delete-service-item?id=" + id,
        type: 'Post',
        success: function (response) {
            if (response.success) {
                console.log(response.message);
                $('.item_' + id).remove();
                if ($('.th-compare').find('td').length < 1) {
                    $('.table').hide();
                    $('.No-Service').show();
                }
            } else {
                $('#message').html('<span class="fa fa-warning"></span> ' + response.message)
                $('#message').slideDown();
                setTimeout(function () {
                    $('#message').slideUp();
                }, 3000);
                console.log(response.message);
            }
        },
    });
}