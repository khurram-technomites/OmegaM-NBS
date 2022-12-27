"use strict";
var Table;
var PropertySelection = [];
var KTDatatablesBasicScrollable = function () {

    var initTable1 = function () {
        var table = $('#kt_datatable1');

        // begin first table
        Table = table.DataTable({
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
                orderable: false,
                width: '135px',
                className: "dt-center",
                render: function (data, type, full, meta) {

                    var actions = '';

                    actions += '<a type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" href="/Admin/Property/Details/' + full[0] + '\" title="View">' +
                        '<i class="fa fa-folder-open"></i>' +
                        '</a> ';

                    if (full[4] == "2") {
                        actions += '<button type="button" class="btn btn-outline-success btn-sm mr-1 btnapprove" onclick="ApproveProp(this, ' + full[0] + ')">' +
                            '<i class="fa fa-check-circle"></i> Approve' +
                            '</button> ' +
                            '<button type="button" class="btn btn-outline-danger btn-sm mr-1 btnapprove" onclick="OpenModelPopup(this,\'/Admin/Property/Reject/' + full[0] + '/?approvalStatus=false\',true)">' +
                            '<i class="fa fa-times-circle"></i> Reject' +
                            '</button> ';
                    }

                    return actions;
                },
            },
            {
                targets: 0,
                visible: false,
                width: '1px',
                },
            {
                targets: 1,
                width: '45px',
                visible: false,
                render: function (data, type, row) {

                    var Published = data ? data.toString().toUpperCase() : "FALSE";
                    if (Published == "FALSE") {
                        return `<label class="checkbox checkbox-lg checkbox-inline">
										<input type="checkbox" value="${row[0]}" id="chk${row[0]}" name="chkCar" onchange="PropertySelected(this,${row.ID})">
										<span></span>
									</label>`;
                    } else {
                        return '';
                    }
                }
            },
            {
                targets: 3,
                width: '475px',
                render: function (data, type, full, meta) {

                    if (!data) {
                        return '<span>-</span>';
                    }
                    var vendor = data.split(',');

                    return '<div class="d-flex align-items-center">' +
                        '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                        '<div class="symbol-label" style="background-image: url(\'' + vendor[0] + '\')"></div>' +

                        '</div>' +
                        '<div class="car-name" title="' + vendor[1] + '">' +
                        /*  '<span  class="label label-lg font-weight-bold label-light-success label-inline">Published</span></br>' +*/

                        '<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + vendor[1] + '</a>' +
                        '<span class="text-muted font-weight-bold d-block">' + vendor[3] + '</span>' +
                        '<a  href="javascript:;"  class="text-muted font-weight-bold d-block vendorname">' + vendor[2] + '</a>' +

                        '</div>' +
                        '</div>'
                },
                },
                {
                    targets: 2,
                    width : '130px'
                },
            //{
            //    targets: 4,
            //    width: '75px',
            //    render: function (data, type, full, meta) {

            //        if (!data) {
            //            return '<span>-</span>';
            //        }
            //        var split = data.split(',');

            //        return '<div class="d-flex align-items-center">' +
            //            '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
            //            '<div class="symbol-label" style="background-image: url(\'' + split[0] + '\')"></div>' +
            //            '</div>' +
            //            '<div class="car-name" title="' + split[1] + '">' +
            //            '<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + split[1] + '</a>' +
            //            '<span class="text-muted font-weight-bold d-block">' + split[2] + '</span>' +
            //            '</div>' +
            //            '</div>'
            //    },
            //},
            {
                targets: 4,
                width: '75px',
                render: function (data, type, full, meta) {
                    var status = {
                        "1": {
                            'title': 'New',
                            'class': ' label-light-warning'
                        },
                        "2": {
                            'title': 'Pending',
                            'class': ' label-light-warning'
                        },
                        "3": {
                            'title': 'Approved',
                            'class': ' label-light-success'
                        },
                        "4": {
                            'title': 'Rejected',
                            'class': ' label-light-danger'
                        }
                    };

                    if (typeof status[data] === 'undefined') {

                        return '<span class="label label-lg font-weight-bold label-light-danger label-inline">New</span>';
                    }
                    return '<span class="label label-lg font-weight-bold' + status[data].class + ' label-inline">' + status[data].title + '</span>';
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

function callback(dialog, elem, isedit, response) {

    if (response.success) {
        toastr.success(response.message);
        window.location.reload();
        //toastr.success(response.message);
        //console.log(elem)
        //if (isedit) {
        //    Table.row($(elem).closest('tr')).remove().draw();
        //}
        //if (response.data.ApprovalStatus == 3) {
        //    addRow(response.data);
        //}
        //else {
        //    Table.row($(elem).closest('tr')).remove();
        //}

        ////addRow(response.data);
        ////jQuery('form', dialog).closest('.modal').find('button[type=submit]').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
        //jQuery('#myModal').modal('hide');
    }
    else {
        //jQuery('form', dialog).closest('.modal').find('button[type=submit]').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);

        toastr.error(response.message);
    }
}

function addRow(row) {
    Table.row.add([
        row.ID,
        row.IsPublished,
        row.Date,
        row.Vendor,
        row.Thumbnail,
        row.ApprovalStatus,
        ""
    ]).draw(true);

}

function ApproveProp(element, record) {
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
                url: '/Admin/Property/Approve/' + record,
                type: 'Get',
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        //window.setTimeout(function () { location.reload() })
                        //callback("", element, true, response)
                        Table.row($(element).closest('tr')).remove().draw();

                    } else {
                        $(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        swal.fire({
                            title: 'An error occur while approving the property. please try again later',
                            html: response.message,
                            icon: 'error',
                            //timer: 10000,
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
                            swal.fire("Access Denied", "You are not authorize to perform this action, For further details please contact administrator!", "warning").then(function () {
                                $('#myModal').modal('hide');
                            });
                        }

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

/*//function PropertySelected(element, record) {

//    if (!PropertySelection) {
//        PropertySelection = [];
//    }

//    var car = PropertySelection.find(function (obj) { return obj == $(element).val() });

//    if ($(element).prop('checked')) {
//        if (!car) {
//            PropertySelection.push($(element).val());
//        }
//    } else {
//        if (car) {
//            PropertySelection = PropertySelection.filter(function (obj) { return obj != $(element).val() });
//        }
//    }

//    var chk = PropertySelection.length;
//    if (chk > 0) {
//        //$(".btnapprove").hide();
//        $(".btn-bulk-publish").prop("disabled", false);
//        $(".btn_rejectall").prop("disabled", false);

//        $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Approve All (${chk})`);
//        $("rejectall").html(`<i class="fa fa-check-circle"></i> Reject All (${chk})`);
//    }
//    else {
//        $(".btn-bulk-publish").prop("disabled", true);
//        $("rejectall").prop("disabled", true);
//        //$(".btnapprove").show();

//        $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Send For Approval (${chk})`);
//    }

//}*/
