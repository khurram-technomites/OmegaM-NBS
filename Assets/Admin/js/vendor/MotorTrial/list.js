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

            "language": {
                processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
            },
            "initComplete": function (settings, json) {
                $('#kt_datatable1 tbody').fadeIn();
            },
            columnDefs: [{
                targets: -1,
                title: 'Actions',
                width: "130px",
                class: 'dt-center',
                orderable: false,

                render: function (data, type, full, meta) {

                    //data = data.split(',');
                    //var isActive = data[0].toUpperCase();
                    //var status = {
                    //    "TRUE": {
                    //        'title': 'Deactivate',
                    //        'icon': 'fa-times-circle',
                    //        'class': ' btn-outline-danger'
                    //    },
                    //    "FALSE": {
                    //        'title': 'Activate',
                    //        'icon': 'fa-check-circle',
                    //        'class': ' btn-outline-success'
                    //    },
                    //};

                    var actions = '';
                    actions += '<button type = "button" class="btn btn-bg-secondary btn-sm mr-1" title="Details" onclick = "OpenModelPopup(this,\'/Vendor/TestRunCustomer/Details/' + data + '\')" > <i class="fa fa-folder-open"></i>Details</button > ';


                    return actions;
                },
            },
                {
                    targets: 0,
                    width: '130px',
                    className: "dt-center",

                },
                {
                    targets: 1,
                    width: '150px',
                   

                    render: function (data, type, full, meta) {

                        console.log(data);
                        if (!data) {
                            return '<span>-</span>';
                        }
                        var customer = data.split(',');
                        return '<a href="#" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + customer[1] + '</a><br>' +
                            '<a href="#" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + customer[3] + '</a>'

                    },
                },
                {
                    targets: 2,
                    width: '250px',

                    render: function (data, type, full, meta) {
                        
                        var split = data.split(',');
                        var href = '/Vendor/Car/edit/' + split[1];
                        //if (split[4] == "Property") {
                        //    href = '/Vendor/Property/edit/' + split[0];
                        //}
                        //else {
                        //    href = '/Vendor/Car/edit/' + split[0];
                        //}

                        return '<div class="d-flex align-items-center">' +
                            '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                            '</div>' +
                            '<div>' +
                            '<a href="' + href + '" " target="_blank" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + split[0] + '</a><br>' +
                            '</div>' +
                            '</div>';
                    },
                },
                {
                    targets: 3,
                    width: '130px',
                    className: "dt-center",

                },
            {
                targets: 4,
                width: '75px',
                class: 'dt-center',
                render: function (data, type, full, meta) {
                    
                    var status = {
                        "Pending": {
                            'title': 'Pending',
                            'class': ' label-light-dark'
                        },
                        "Confirmed": {
                            'title': 'Confirmed',
                            'class': ' label-light-warning'
                        },

                        "Completed": {
                            'title': 'Completed',
                            'class': ' label-light-success'
                        },
                        "Canceled": {
                            'title': 'Canceled',
                            'class': ' label-light-danger'
                        },
                    };
                    if (typeof status[data] === 'undefined') {
                        return '<a  href="javascript:" class="label label-lg label-light-dark label-inline" onclick="OpenModelPopup(this,\'/Vendor/TestRunCustomer/StatusChange/' + full[5] + '\',true)">' + data + '</a>';
                        /*return '<a href="javascript:" class="label label-lg label-light-dark label-inline" >' + data + '</a>';*/
                    }
                    return '<a href="javascript:" class="label label-lg ' + status[data].class + ' label-inline" onclick="OpenModelPopup(this,\'/Vendor/TestRunCustomer/StatusChange/' + full[5] + '\',true)">' + data + ' </a>';
                    /* return '<a href="javascript:" class="label label-lg ' + status[data].class + ' label-inline" >' + data + ' </a>';*/

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

jQuery(document).ready(function () {
    KTDatatablesBasicScrollable.init();
});


function Delete(element, record) {

    swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete it!'
    }).then(function (result) {
        if (result.value) {

            $.ajax({
                url: '/Vendor/TestRunCustomer/Delete/' + record,
                type: 'POST',
                data: {
                    "__RequestVerificationToken":
                        $("input[name=__RequestVerificationToken]").val()
                },
                success: function (result) {
                    if (result.success != undefined) {
                        if (result.success) {
                            toastr.options = {
                                "positionClass": "toast-bottom-right",
                            };
                            toastr.success('Trial booking deleted successfully ...');

                            table1.row($(element).closest('tr')).remove().draw();
                        }
                        else {
                            toastr.error(result.message);
                        }
                    } else {
                        swal.fire("Your are not authorize to perform this action", "For further details please contact administrator !", "warning").then(function () {
                        });
                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    if (xhr.status == 403) {
                        try {
                            var response = $.parseJSON(xhr.responseText);
                            swal.fire(response.Error, response.Message, "warning").then(function () {
                                $('#myModal').modal('hide');
                            });
                        } catch (ex) {
                            swal.fire("Access Denied", "Your are not authorize to perform this action, For further details please contact administrator !", "warning").then(function () {
                                $('#myModal').modal('hide');
                            });
                        }

                        $(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        $(element).find('i').show();

                    }
                }
            });
        }
    });
}



function callback(dialog, elem, isedit, response) {

    if (response.success) {
        toastr.success(response.message);

        if (isedit) {
            table1.row($(elem).closest('tr')).remove().draw();
        }
        if (response.data.Status == "Pending") {
            addRow(response.data);
        }
        else if (response.data.Status == "Completed") {
            addRow(response.data);
        }
        else if (response.data.Status == "Confirmed") {
            addRow(response.data);
        }
        else if (response.data.Status == "Canceled") {
            addRow(response.data);
        }
        else {
            table1.row($(elem).closest('tr')).remove();
        }

        //addRow(response.data);
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
       /* row.TrialBookingNo,*/
        row.Customer ? row.Customer.CustomerLogo + ',' + row.Customer.CustomerName + ',' + row.Customer.CustomerContact + ',' + row.Customer.CustomerEmail : '',
        row.Property,
        row.BookingDate + ',' + row.BookingTime,
        row.Status,
        row.ID,
    ]).draw(true);
}