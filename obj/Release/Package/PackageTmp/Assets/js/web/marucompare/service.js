'use strict';
var scroll = true;
var Cars = [];

$(document).ready(function () {

    if ($('#IDArray').val() != 'false') {
        Cars = $('#IDArray').val().split(',').map(Number);
        if (Cars != null) {
            $.each(Cars, function (i, val) {
                console.log($('#btn_compare_' + val).attr('data-type', '0').html('<i class="fa fa-check mr-1"></i>Compare').css('opacity', '0.5'));
            });
        }
    }

    CheckCarsinArray();

    $('.btn-sticky').click(function () {
        $('.btn-sticky').html('<span class="fa fa-spinner fa-spin"></span> Compare Now').attr('disabled', true);
        $.ajax({
            url: "/compare-cars-request",
            type: 'Post',
            data: {
                "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
                IDs: Cars,
            },
            success: function (response) {
                if (response.success) {
                    window.location.href = response.url;
                } else {
                    $('#message').html('<span class="fa fa-warning"></span> ' + response.message)
                    $('#message').slideDown();
                    setTimeout(function () {
                        $('#message').slideUp();
                    }, 3000);
                }
            },
        });
        return false;
    });

    //window.onscroll = function () { myFunction() };

    //var compareDiv = $("#compare-section").get(0);
    //var sticky = compareDiv.offsetTop;

    //function myFunction() {
    //    if (window.pageYOffset < sticky) {
    //        compareDiv.classList.add("sticky-div");
    //        console.log(window.pageYOffset + " | " + sticky);
    //    } else {
    //        compareDiv.classList.remove("sticky-div");
    //        console.log(window.pageYOffset + " | " + sticky);
    //    }
    //}
});

function CarArray(element, id) {

    if (scroll) {
        scroll = false;
        setTimeout(function () { $('.scroll-a-link').attr('href', 'javascript:void(0)'); }, 1000);
    }

    if ($(element).attr('data-type') == '1') {
        $(element).attr('data-type', '0').html('<i class="fa fa-check mr-1"></i>Compare').css('opacity', '0.5');
        Cars.push(id);
        CheckCarsinArray();
    }
    else {
        $(element).attr('data-type', '1').html('<i class="fa fa-balance-scale mr-1"></i>Compare').css('opacity', '1');
        Cars = removeItem(Cars, id);
        CheckCarsinArray();
    }
}

function CheckCarsinArray() {
    if (Cars.length > 0) {
        $('#compare-section').show();
        if (Cars.length == 1) {
            $('#compare-section').find('.service-select').html(Cars.length + ' Service Selected');
            $('.btn-sticky').hide();
        }
        else {
            $('#compare-section').find('.service-select').html(Cars.length + ' Services Selected');
            $('.btn-sticky').show();
        }
    }
    else {
        $('#compare-section').hide();
    }
}

function removeItem(originalArray, itemRemove) {
    return originalArray.filter(function (array) { return array !== itemRemove });
}

var showErrorMsg = function (form, type, msg) {
    var alert = $('<div class="alert kt-alert kt-alert--outline alert alert-' + type + ' " role="alert">\
			<button type="button" class="close" data-dismiss="alert" aria-label="Close"><i class="fa fa-times"></i></button>\
			<span></span>\
		</div>');

    form.find('.alert').remove();
    alert.prependTo(form);
    alert.find('span').html(msg);
    $(alert).slideDown();
    setTimeout(function () { $(alert).slideUp(); }, 6000);
}
