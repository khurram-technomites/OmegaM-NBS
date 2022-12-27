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
            //dom: 'Bfrtip',

            "language": {
                processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
            },
            "initComplete": function (settings, json) {
                $('#kt_datatable1 tbody').fadeIn();
            },
            columnDefs: [{
                targets: 0,
                width: '250px',

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
var KTDatatablesBasicScrollable1 = function () {

    var initTable1 = function () {
        var table = $('#kt_datatable1');

        // begin first table
        table1 = table.DataTable({
            //scrollY: '50vh',
            scrollX: true,
            scrollCollapse: true,
            //dom: 'Bfrtip',

            "language": {
                processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
            },
            "initComplete": function (settings, json) {
                $('#kt_datatable1 tbody').fadeIn();
            },
            //dom: 'Bfrtip',
            
            columnDefs: [{
                targets: 0,
                width: '250px',

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

jQuery(document).ready(function () {

    KTDatatablesBasicScrollable.init();
    $('.kt_datepicker_range').datepicker({
        todayHighlight: true,
    });

    var fromDate = $('#fromDate').val();
    var toDate = $('#toDate').val();

    $('#from').val(fromDate);
    $('#to').val(toDate);


    $('#toDate').change(function () {
        $('#to').val($(this).val());
    });

    $('#fromDate').change(function () {
        //
        //var fromDate = $('#fromDate').val();
        //var toDate = $('#toDate').val();

        //$('#from').val(fromDate);
        //$('#to').val(toDate);

        $('#from').val($(this).val());
    });

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
            url: '/Admin/Transaction/List',
            type: 'POST',
            data: {
                fromDate: $('#fromDate').val(),
                toDate: $('#toDate').val(),
            },
            success: function (data) {
                "use strict";

                if (data != null) {

                    $("#suggestions").html(data);
                    KTDatatablesBasicScrollable1.init();

                    var fromDate = $('#fromDate').val();
                    var toDate = $('#toDate').val();

                    $('#from').val(fromDate);
                    $('#to').val(toDate);
                }
            }
        });

    });
});



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
        '<td data-order=' + row.ID + '> ' + row.CretedOn + '</td>',
        row.OrderNo,
        row.NameOnCard,
        row.MaskCard,
        row.TransactionStatus,
        row.Amount,
    ]).draw(true);

}