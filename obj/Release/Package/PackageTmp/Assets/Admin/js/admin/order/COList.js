"use strict";
var table1;
var KTDatatablesBasicScrollable = function () {

    var initTable1 = function () {
        var table = $('#kt_datatable1');

        // begin first table
        table1 = table.DataTable({
            //scrollY: '50vh',
            scrollX: true,
            scrollCollapse: true,
            "order": [[1, "desc"]],

            "language": {
                processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
            },
            "initComplete": function (settings, json) {
                $('#kt_datatable1 tbody').fadeIn();
                FormatPrices();
            },
            columnDefs: [{
                width: '245px',
                targets: -1,
                title: 'Actions',
                orderable: false,

                render: function (data, type, full, meta) {
                    var actions = '';
                    actions += '<a class="btn btn-secondary btn-sm mr-1" href="/Admin/Order/Details?id=' + full[6] + '">' +
                        '<i class="fa fa-folder-open"></i> Details </a>' +
                        '<a class="btn btn-secondary btn-sm mr-1" href="javscript:void(0);" onclick="OpenModelPopup(this,\'/Admin/Invoice/InvoiceDetail/' + full[6] + '\',true)"><i class="fas fa-file-invoice"></i> Invoice </a>';
                       /* '<a class="btn btn-secondary btn-sm mr-1" href="javscript:void(0);" onclick="OpenModelPopup(this,\'/Admin/Order/PackingSlip/' + full[5] + '\',true)"><i class="fas fa-file-invoice"></i> Packing Slip </a>';*/
                    return actions;
                },
            },
            {
                targets: 2,
                width: '195px',
                render: function (data, type, full, meta) {
                    console.log(data);
                    if (!data) {
                        return '<span>-</span>';
                    }
                    var customer = data.split('|');
                    return '<div class="d-flex align-items-center">' +
                        '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                        '<div class="symbol-label" style="background-image: url(\'/assets/admin/media/users/default.jpg\')"></div>' +
                        '</div>' +
                        '<div>' +
                        '<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + customer[0] + '</a>' +
                        '<span class="text-muted font-weight-bold d-block">' + customer[1] + '</span>' +
                        '</div>' +
                        '</div>';
                },
            },
            {
                targets: 5,
                width: '105px',
                render: function (data, type, full, meta) {

                    var status = {
                        "Pending": {
                            'title': 'Pending',
                            'class': ' label-light-dark'
                        },
                        "Confirmed": {
                            'title': 'Confirmed',
                            'class': ' label-light-success'
                        },
                        "Processing": {
                            'title': 'Processing',
                            'class': ' label-light-primary'
                        },
                        "Completed": {
                            'title': 'Completed',
                            'class': ' label-light-danger'
                        },
                        "Dispatched": {
                            'title': 'Dispatched',
                            'class': ' label-light-danger'
                        },
                        "Delivered": {
                            'title': 'Delivered',
                            'class': ' label-light-success'
                        },
                        "Returned": {
                            'title': 'Returned',
                            'class': ' label-light-success'
                        },
                        "Canceled": {
                            'title': 'Canceled',
                            'class': ' label-light-success'
                        },
                    };
                    if (typeof status[data] === 'undefined') {
                        return '<a  href="javascript:" class="label label-lg label-light-dark label-inline" onclick="OpenModelPopup(this,\'/Admin/Order/StatusChange/' + full[6] + '\',true)">' + data + '</a>';
                    }
                    return '<a href="javascript:" class="label label-lg ' + status[data].class + ' label-inline" onclick="OpenModelPopup(this,\'/Admin/Order/StatusChange/' + full[6] + '\',true)">' + data + ' </a>';

                },
            },
            //{
            //    targets: 4,
            //    width: '75px',
            //    render: function (data, type, full, meta) {

            //        var status = {
            //            "Pending": {
            //                'title': 'Pending',
            //                'class': ' label-light-dark'
            //            },
            //            "Processing": {
            //                'title': 'Processing',
            //                'class': ' label-light-success'
            //            },
            //            "Fulfilled": {
            //                'title': 'Fulfilled',
            //                'class': ' label-light-primary'
            //            },
            //            "Not Fulfilled": {
            //                'title': 'Not Fulfilled',
            //                'class': ' label-light-danger'
            //            },

            //        };
            //        if (typeof status[data] === 'undefined') {

            //            return '<a href="javascript:" class="label label-lg label-light-dark label-inline" onclick="OpenModelPopup(this,\'/Admin/Order/ShipmentChange/' + full[6] + '\',true)">' + data + '</a>';
            //        }
            //        return '<a href="javascript:" class="label label-lg ' + status[data].class + ' label-inline" onclick="OpenModelPopup(this,\'/Admin/Order/ShipmentChange/' + full[6] + '\',true)">' + data + ' </a>';
            //    },
            //},
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

jQuery(document).ready(function () {
    KTDatatablesBasicScrollable.init();
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

    var fromDate = $('#fromDate').val();
    var toDate = $('#toDate').val();

    $('#from').val(fromDate);
    $('#to').val(toDate);

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
            url: '/Admin/Order/COList',
            type: 'POST',
            data: {
                fromDate: $('#fromDate').val(),
                toDate: $('#toDate').val(),
            },
            success: function (data) {
                "use strict";

                if (data != null) {
                    
                    $("#Orders").html(data);
                    KTDatatablesBasicScrollable.init();
                    $('#from').val(fromDate);
                    $('#to').val(toDate);

                    var td = data.includes("</td>");
                    if (td) {
                        $('#btnSubmit').show();
                    }
                }
                else {
                    $('#btnSubmit').hide();
                }
            }
        });

    })//btnSearch;
});

function ChangeStatus(element, record) {
    swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, do it!'
    }).then(function (result) {
        if (result.value) {
            $(element).find('i').hide();
            $(element).addClass('spinner spinner-left spinner-sm').attr('disabled', true);

            $.ajax({
                url: '/Admin/Order/Status/' + record,
                type: 'POST',
                data: JSON.stringify({ status: $(element).text().trim() }),
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        table1.row($(element).closest('tr')).remove().draw();
                        addRow(response.data);
                    } else {
                        toastr.error(response.message);
                        $(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        $(element).find('i').show();
                    }
                }
            });
        } else {
            //swal("Cancelled", "Your imaginary file is safe :)", "error");
        }
    });
}

function callback(dialog, elem, isedit, response) {

    if (response.success) {
        toastr.success(response.message);

        if (isedit) {
            table1.row($(elem).closest('tr')).remove().draw();
        }

        if (response.data.Status == "Completed") {
            addRow(response.data);
        }
        else if (response.data.Status == "Dispatched") {
            addRow(response.data);
        }
        else if (response.data.Status == "Delivered") {
            addRow(response.data);
        }
        else if (response.data.Status == "Canceled") {
            addRow(response.data);
        }
        else if (response.data.Status == "Returned") {
            addRow(response.data);
        }
        else {
            table1.row($(elem).closest('tr')).remove();
        }
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
        row.Date,
        row.OrderNo,
        row.Customer ? row.Customer.Name + '|' + row.Customer.Contact : '',
        row.Vendor,
        row.TotalAmount + ' ' + row.Currency,
        /*row.ShipmentStatus,*/
        row.Status,
        row.ID,
    ]).draw(true);

}