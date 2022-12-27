
"use strict";
var table1;

var KTDatatablesBasicScrollable = function () {

    var initTable1 = function () {
        var table = $('#kt_datatable1');

        // begin first table
        table1 = table.DataTable({
            //scrollY: '50vh',
            //scrollX: true,
            //scrollCollapse: true,
            //dom: 'Bfrtip',
            "order": [[0, "desc"]],
            "language": {
                processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
            },
            "initComplete": function (settings, json) {
                $('#kt_datatable1 tbody').fadeIn();
            },
            columnDefs: [
                {
                    targets: 0,
                    "visible": false,
                    render: function (data, type, full, meta) {
                        return data;
                    },
                },
                {
                    targets: 1,
                    className: "dt-center",
                    width: '130px',
                    render: function (data, type, full, meta) {
                        return data;
                    },
                },
                {
                    targets: 2,
                    className: "dt-center",
                    width: '150px',
                    render: function (data, type, full, meta) {
                        return data;
                    },
                },
                {
                    targets: 3,
                    className: "dt-center",
                    width: '200px',
                    render: function (data, type, full, meta) {
                        return data;
                    },
                },
                {
                    targets: 4,
                    className: "dt-center",
                    width: '120px',
                    render: function (data, type, full, meta) {
                        return data;
                    },
                },
                {
                    targets: 5,
                    width: '300px',
                    className: "dt-center",
                    render: function (data, type, full, meta) {
                        var actions = '';
                        debugger;
                        actions += data;
                        return actions;
                    },
                },
                {
                    targets: 6,
                    "visible": false,
                    className: "dt-center",
                    render: function (data, type, full, meta) {
                        return data;
                    },
                },
                {
                    targets: -1,
                    width: '100px',
                    //title: 'Is Shown',
                    className: "dt-center",
                    orderable: false,
                    render: function (data, type, full, meta) {

                        data = data.split(',');
                        data[0] = data[0].toUpperCase()
                        var status = {
                            "FALSE": {
                                'title': 'Show',
                                'class': ' btn-outline-success',
                                'icon': ' fa-check-circle',
                                'flag': true,
                            },
                            "TRUE": {
                                'title': 'Hide',
                                'class': ' btn-outline-danger',
                                'icon': ' fa-times-circle',
                                'flag': false,
                            },
                        };

                        if (typeof status[data[0]] === 'undefined') {
                            return '<button type="button" class="btn btn-outline-success btn-sm mr-1" onclick="ShowOnWebsite(this,' + data[1] + ', ' + true + ',true)">' +
                                '<i class="fa fa-times-circle"></i> Yes' +
                                '</button> ';
                        }

                        return '<button type="button" class="btn' + status[data[0]].class + ' btn-sm mr-1" onclick="ShowOnWebsite(this,' + data[1] + ', ' + status[data[0]].flag + ',true)">' +
                            '<i class="fa' + status[data[0]].icon + '"></i> ' + status[data[0]].title +
                            '</button> ';
                    },
                },
            ],

        });
    };

    return {
        //main function to initiate the module
        init: function () {
            initTable1();
        },
    };
}();

function ShowOnWebsite(element, data, status) {
    swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, do it!'
    }).then(function (result) {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Admin/Suggestion/ShowOnWebsite',
                type: 'POST',
                data: { 'id': data, 'status': status },
                success: function (response) {
                    if (response.success) {
                        toastr.options = {
                            "positionClass": "toast-bottom-right",
                        };
                        //toastr.success('City Deleted Successfully');
                        toastr.success(response.message);
                        table1.row($(element).closest('tr')).remove().draw();
                        addRow(response.data);
                        //setTimeout(function () {
                        //	location.reload();
                        //}, 1000);
                    } else {
                        //toastr.error(response.message);
                        toastr.error(response.message);
                        $(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        $(element).find('i').show();
                    }
                }
            });
        }
    });
}


jQuery(document).ready(function () {
    KTDatatablesBasicScrollable.init();
    RatingStar();
    $("#fromDate").datepicker({
        todayHighlight: true,
    });

    $("#toDate").datepicker({
        todayHighlight: true,
    });

    $("#fromDate").change(function () {

        if (new Date($("#fromDate").val()) > new Date($("#toDate").val())) {
            $('#toDate').datepicker('setDate', new Date($("#fromDate").val()));
            $("#toDate").datepicker("option", "minDate", new Date($("#fromDate").val()));
        }
    });

    $("#toDate").change(function () {

        if (new Date($("#fromDate").val()) > new Date($("#toDate").val())) {
            $('#fromDate').datepicker('setDate', new Date($("#toDate").val()));
            $("#fromDate").datepicker("option", "maxDate", new Date($("#toDate").val()));
        }
    });

    //$('.kt_datepicker_range').datepicker({
    //    todayHighlight: true,
    //});


    $("#btnSearch").on("click", function () {

        var fromDate = $('#fromDate').val();
        var toDate = $('#toDate').val();

        if (fromDate == "" && toDate == "") {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Please! Select Date',
            })
        }
        else if (fromDate == "") {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Please! Select From Date',
            })
        }
        else if (toDate == "") {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Please! Select To Date',
            })
        }

        $.ajax({
            url: '/Admin/Suggestion/List',
            type: 'POST',
            data: {
                fromDate: $('#fromDate').val(),
                toDate: $('#toDate').val(),
            },
            success: function (data) {
                "use strict";

                if (data != null) {

                    $("#suggestions").html(data);
                    KTDatatablesBasicScrollable.init();
                    $(".seemore").on("click", function () {
                        debugger;
                        if ($(this).closest('tr').hasClass('docked')) {
                            $(this).closest('tr').removeClass('docked');
                        } else {
                            $(this).closest('tr').addClass('docked');
                        }
                    });
                  
                }
            }
        });

    });
    $(".seemore").on("click", function () {
        debugger;
        if ($(this).closest('tr').hasClass('docked')) {
            $(this).closest('tr').removeClass('docked');
        } else {
            $(this).closest('tr').addClass('docked');
        }
    });
});

function RatingStar() {
    $('.btn-rating').each(function (k, v) {
        var rating = parseFloat($(v).attr('data'));
        $(this).find('i:lt(' + (rating) + ')').addClass("la-star").removeClass("la-star-o");
    });
}

function callback(dialog, elem, isedit, response) {

    if (response.success) {
        toastr.success(response.message);

        if (isedit) {
            table1.row($(elem).closest('tr')).remove().draw();
        }

        addRow(response.data);
        jQuery('form', dialog).closest('.modal').find('button[type=submit]').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
        jQuery('#myModal').modal('hide');
    }
    else {
        jQuery('form', dialog).closest('.modal').find('button[type=submit]').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);

        toastr.error(response.message);
    }
}

function addRow(row) {
    table1.row.add([
        row.ID,
        '<td data-order=' + row.ID + '> ' + row.Date + '</td>',
        row.Name,
        row.Email,
        row.Contact,
        row.Rating,
        row.Message,
        row.RatingHidden,
        row.IsShown + ',' + row.ID,
    ]).draw(true);
    RatingStar();
}