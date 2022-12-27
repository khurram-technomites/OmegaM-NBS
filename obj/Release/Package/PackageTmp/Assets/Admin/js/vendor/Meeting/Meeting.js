"use strict";
var Table;
var table1;
var PropertySelection = [];
var KTDatatablesBasicScrollable = function () {

    var initTable1 = function () {
        var table = $('#kt_datatable2');

        // begin first table
        table1 = table.DataTable({
            //scrollY: '50vh',
            scrollX: true,
            scrollCollapse: true,

            "language": {
                processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
            },
            "initComplete": function (settings, json) {
                $('#kt_datatable2 tbody').fadeIn();
            },
            columnDefs: [{
                /*targets: -1,
                title: 'Actions',
                orderable: false,

                render: function (data, type, full, meta) {

                    var actions = '';

                    actions += '<a type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" href="/Admin/Property/Details/' + full[0] + '\" title="View">' +
                        '<i class="fa fa-folder-open"></i>' +
                        '</a> ';

                    if (full[5] == "2") {
                        actions += '<button type="button" class="btn btn-outline-success btn-sm mr-1 btnapprove" onclick="Approve(this, ' + full[0] + ')">' +
                            '<i class="fa fa-check-circle"></i> Approve' +
                            '</button> ' +
                            '<button type="button" class="btn btn-outline-danger btn-sm mr-1 btnapprove" onclick="OpenModelPopup(this,\'/Admin/Property/Reject/' + full[0] + '/?approvalStatus=false\',true)">' +
                            '<i class="fa fa-times-circle"></i> Reject' +
                            '</button> ';
                    }

                    return actions;
                },*/
            },
            {
                targets: 0,
                visible: false
            },

            //{
            //	targets: 2,
            //	className: "dt-center",

            //},
            {
                targets: 1,
                width: '130px',
                className: "dt-center",

            },
            {
                targets: 2,
                width: '150px',

                render: function (data, type, full, meta) {

                    if (!data) {
                        return '<span>-</span>';
                    }
                    var vendor = data.split('|');
                    return '<a href="#" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + vendor[1] + '</a><br>' +
                        '<a href="#" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + vendor[3] + '</a>'
                }
            },
            {
                targets: -1,
                title: 'Actions',
                orderable: false,
                width: '100px',
                className: "dt-center",
                render: function (data, type, full, meta) {

                    return '<button type="button" class="btn btn-secondary btn-sm mr-1" onclick="OpenModelPopup(this,\'/Vendor/Meeting/Message/' + data + '\')" title="Details">' +
                        '<i class="fa fa-folder-open"></i> Details' +
                        '</button> ';
                }
            },
            {
                targets: 3,
                width: '250px',

                render: function (data, type, full, meta) {
                    var split = data.split('^');
                    var href = "";
                    if (split[4] == "Property") {
                        href = '/Vendor/Property/Details/' + split[0];
                    }
                    else {
                        href = '/Vendor/Car/Details/' + split[0];
                    }


                    return '<div class="d-flex align-items-center">' +
                        '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                        '<div class="" style="background-image: url(\'' + split[2] + '\')"></div>' +
                        '</div>' +
                        '<div>' +
                        '<a href="' + href + '" " target="_blank" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + split[1] + '</a><br>' +
                        '</div>' +
                        '</div>';
                },

                },
                {
                    targets: 4,
                    width: '130px',
                    className: "dt-center",

                },
            {
                targets: 5,
                className: "dt-center",
                width: '75px',

                render: function (data, type, full, meta) {
                    
                    console.log("data", data);
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

                        return '<a  href="javascript:" class="label label-lg label-light-dark label-inline" onclick="OpenModelPopup(this,\'/Vendor/Meeting/StatusChange/' + full[6] + '\',true)">' + data + '</a>';
                    }
                    return '<a href="javascript:"  onclick="OpenModelPopup(this,\'/Vendor/Meeting/StatusChange/' + full[6] + '\',true)" class="label label-lg ' + status[data].class + ' label-inline ">' + status[data].title + '</a>';
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

function IsApproved(element, record) {
    swal.fire({
        title: 'Update Status',
        text: "Change status of this feedback?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, do it!'
    }).then(function (result) {
        if (result.value) {
            $(element).find('i').hide();
            $(element).addClass('spinner spinner-left spinner-sm').attr('disabled', true);
            $.ajax({
                url: '/Vendor/GetInTouch/Update/' + record,
                type: 'Get',
                success: function (response) {
                    location.reload();
                    if (response.success) {
                        toastr.success(response.message);
                        setTimeout(function () {
                            location.reload();
                        }, 1000);

                    } else {
                        //toastr.error(response.message);
                        //$(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        //$(element).find('i').show();
                        $(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        swal.fire({
                            title: 'Please complete the car information before sending it for approval!',
                            html: response.message,
                            icon: 'error',
                            //timer: 10000,
                        });
                    }
                },
                Error: function (response) {
                    location.reload();
                    if (response.success) {
                        toastr.success(response.message);
                        setTimeout(function () {
                            location.reload();
                        }, 1000);

                    }
                }
            });
        } else {
            location.reload();
            swal("cancelled", "your imaginary file is safe :)", "error");
        }
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
    console.log(row)
    //table1.row.add([
    //    row.ID,
    //    '<td data-order=' + row.ID + '> ' + row.Date + '</td>',
    //    row.Customer,
    //    row.For,
    //    row.Status,
    //    row.ID,
    //]).draw(true);

    //var For = '';
    //if (row.CarID != null) {
    //    For = row.CarID + "^" + row.Car.Name + "^" + row.Car.Thumbnail + "^" + row.Car.Address + "^" + "Car";
    //}
    //else {
    //    For = row.PropertyID + "^" + row.Property.Title + "^" + row.Property.Thumbnail + "^" + row.Property.Address + "^" + "Property";
    //}

    table1.row.add($('<tr>' +
        '<td hidden>' + row.ID + '</td>' +
        ' <td data-order=' + row.ID + '>' + row.Date + '</td>' +
        ' <td>' + row.Customer + '</td>' +
        ' <td>' + row.For + '</td>' +
        ' <td>' + row.MeetingDate + '</td>' +
        ' <td>' + row.Status + '</td>' +
        ' <td>' + row.ID + '</td>' +
        '</tr>'
    )).draw();

}

jQuery(document).ready(function () {
    KTDatatablesBasicScrollable.init();

});


