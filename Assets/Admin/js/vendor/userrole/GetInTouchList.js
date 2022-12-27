"use strict";
var Table;
var table1;
var PropertySelection = [];
var VisibleByRole = false;
var KTDatatablesBasicScrollable = function () {
    var UserRole = $("#UserRoleName").val();
    if (UserRole == "Admin" || UserRole == "Administrator") {
        VisibleByRole = true;
    }
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
            columnDefs: [
                {
                    targets: 0,
                    width: "130px"
                },
                {
                    targets: 1,
                    width: "230px",
                    render: function (data, type, full, meta) {

                        if (!data) {
                            return '<span>-</span>';
                        }
                        var vendor = data.split('|');
                        return '<div class="d-flex align-items-center">' +
                            '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                            '</div>' +
                            '<div>' +
                            '<a href="#" class="text-dark-75 font-weight-bolder mb-1 font-size-lg">' + vendor[0] + '</a><br>' +
                            '<a href="#" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-sm">' + vendor[1] + '</a><br>' +
                            '<a href="#" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-sm">' + vendor[2] + '</a>' +
                            '</div>' +
                            '</div>';
                    }
                },
                {
                    targets: 2,
                    width: '230px',

                    render: function (data, type, full, meta) {
                        var split = data.split('^');
                        console.log("Split", data);
                        var href = "";
                        if (split[4] == "Property") {
                            href = '/Vendor/Property/edit/' + split[0];
                        }
                        else {
                            href = '/Vendor/Car/edit/' + split[0];
                        }

                        return '<div class="d-flex align-items-center">' +
                            '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                            '<div class="" style="background-image: url(\'' + split[2] + '\')"></div>' +
                            '</div>' +
                            '<div>' +
                            '<a href="' + href + '" " target="_blank" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-sm">' + split[1] + '</a><br>' +
                            '</div>' +
                            '</div>';
                    },
                },
                //{
                //    targets: 3,
                //    width: '75px',
                //    className: "dt-center",
                //    render: function (data, type, full, meta) {
                //        data = data.toUpperCase();
                //        var status = {
                //            "TRUE": {
                //                'title': 'Read',
                //                'class': ' label-light-success'
                //            },
                //            "FALSE": {
                //                'title': 'Un-Read',
                //                'class': ' label-light-danger'
                //            },
                //        };
                //        if (typeof status[data] === 'undefined') {

                //            return '<span class="label label-lg font-weight-bold label-light-danger label-inline">Unread</span>';
                //        }
                //        return '<span class="label label-lg font-weight-bold' + status[data].class + ' label-inline">' + status[data].title + '</span>';
                //    },
                //},
                {
                    targets: 3,
                    width: '150px',
                    className: "dt-center",
                    visible: VisibleByRole,
                    render: function (data, type, full, meta) {
                        
                        data = data.split(',');
                        var GetInTouchId = parseInt(data[2]);
                        var actions = '';
                        if (data[0] == '' || data[0] == 'undefined' || data[0] == null) {
                            actions += '<button type="button" class="btn btn-bg-secondary mr-1"  onclick="OpenModelPopup(this,\'/Vendor/GetInTouch/AssignUser/' + GetInTouchId + '\',true)">' +
                                '<i class="fa fa-pencil"></i> Assign' +
                                '</button> '
                        }
                        else {

                            actions += '<div>' +
                                '<a href="#" class="font-weight-bolder mb-2 text-left" style="color:#524545">' + data[0] + '</a>' +
                                '<div>' +
                                '<a href="#" class="label label-inline font-weight-bolder mb-2 text-left opacity-70" onclick="OpenModelPopup(this,\'/Vendor/GetInTouch/AssignUser/' + GetInTouchId + '\',true)">Change</a>' +
                                '</div>' +
                                '</div>';

                        }

                        return actions;
                    },
                },

                {
                    targets: -1,
                    title: 'Actions',
                    orderable: false,
                    width: '300px',
                    className: "dt-center",
                    render: function (o, data, type, full) {
                        var split = o.split("|")
                        var markRead = split[0];
                        console.log(split[1])
                        var actions = '';
                        if (markRead.toUpperCase() == "FALSE") {
                            actions +=
                                '<button type="button" class="btn btn-outline-success btn-sm mr-1" onclick="IsApproved(this, ' + split[1] + ' )",True)">' +
                                '<i class="fa fa-check-circle"></i> Mark As Read' +
                                '</button> ' +
                                '<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Vendor/GetInTouch/Comments/' + split[1] + '\')" title="Details">' +
                                '<i class="fa fa-folder"></i> ' +
                                '</button> ' +
                                '<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Vendor/GetInTouch/Remarks/' + split[1] + '\')" title="Remarks">' +
                                '<i class="fa fa-receipt"></i> ' +
                                '</button> '


                        }

                        else if (markRead.toUpperCase() == "TRUE") {
                            actions +=
                                
                                '<span class="label label-lg font-weight-bold label-light-success label-inline">Read</span>' +
                                '</button> ' +
                                '<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Vendor/GetInTouch/Comments/' + split[1] + '\')" title="Details">' +
                                '<i class="fa fa-folder"></i> ' +
                                '</button> ' +
                                '<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Vendor/GetInTouch/Remarks/' + split[1] + '\')" title="Remarks">' +
                                '<i class="fa fa-receipt"></i> ' +
                                '</button> '

                        }
                        else if (full.ID != null) {
                            actions += '<button type="button" class="btn btn-outline-success btn-sm mr-1" onclick="OpenModelPopup(this,\'/Vendor/GetInTouch/Comments/' + data + '\')" title="Comments">' +
                                '<i class="fa fa-comment-alt"></i> Comments' +
                                '</button> ' +
                                '<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Vendor/GetInTouch/Remarks/' + split[1] + '\')" title="Details">' +
                                '<i class="fa fa-receipt"></i> ' +
                                '</button> '
                        }

                        return actions;
                    }
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

                        //ForCallBack
                        //toastr.success(response.message);
                        //table1.row($(element).closest('tr')).remove().draw();
                        //addRow(response.data);
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


jQuery(document).ready(function () {
    KTDatatablesBasicScrollable.init();
});

function callback(dialog, elem, isedit, response) {

    if (response.success) {
        toastr.success(response.message);
        location.reload();
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


//function addRow(row) {
//    table1.row.add($('<tr>' +
//        '<td data-order=' + row.ID + '>' + row.CreatedOn + '</td>' +
//        '<td>' + row.Name + ',' + row.Email + row.PhoneNo'</td>' +
//        '<td>' + row.Description + '</td>' +
//        '<td>' + row.Priority + '</td>' +
//        '<td>' + row.Status + '</td>' +
//        '<td>' + row.UserName + ',' + row.UserID + ',' + row.ID + '</td>' +
//        '<td nowrap="nowrap">' + row.ID + '</td>' +
//        '</tr>'
//    )).draw();
//}
