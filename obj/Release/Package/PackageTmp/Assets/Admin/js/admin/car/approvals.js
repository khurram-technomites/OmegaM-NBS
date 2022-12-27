"use strict";
var table1;
var CarSelection = [];
var KTDatatablesBasicScrollable = function () {

    var initTable1 = function () {
        var table = $('#kt_datatablecar');

        // begin first table
        table1 = table.DataTable({
            //scrollY: '50vh',
            scrollX: true,
            scrollCollapse: true,
            "language": {
                processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
            },
            "initComplete": function (settings, json) {
                $('#kt_datatablecar tbody').fadeIn();
            },
            lengthMenu: [
                [10, 25, 100, 500, -1],
                ['10', '25', '100', '500', 'Show all']
            ],
            "proccessing": true,
            "serverSide": true,
            "ajax": {
                url: "/Admin/Car/ApprovalsList",
                type: 'POST',
                data: function (d) {

                    d.filter = $('#filterdropdown').val();
                    d.vendorid = $('#vendordropdown').val();
                },
            },
            "columns": [
                {
                    "data": "ID",
                    visible: false,
                    className: "dt-center",
                    width: '1px',

                },
                {
                    "mData": null,
                    "bSortable": false,
                    width: '30px',
                    "mRender": function (o) {

                        var Published = o.IsPublished ? o.IsPublished.toString().toUpperCase() : "FALSE";
                        if (Published == "FALSE") {
                            return `<label class="checkbox checkbox-lg checkbox-inline">
										<input type="checkbox" disabled value="${o.ID}" id="chk${o.ID}" name="select_specialist">
										<span class="checkmark"></span>
									</label>`;
                        } else {
                            return '';
                        }
                    }
                },
                {
                    "data": "CreatedOn",
                    "bSortable": true,
                    className: "dt-center",
                    width: '70px',

                },
                {
                    "mData": null,
                    "bSortable": true,
                    width: '250px',
                    "mRender": function (o) {
                        if (!o) {
                            return '<span>-</span>';
                        }
                        var Address = o.Address == null ? "" : o.Address;
                        return '<div class="d-flex align-items-center">' +
                            '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                            '<div class="symbol-label" style="background-image: url(\'' + o.Thumbnail + '\')"></div>' +
                            '</div>' +
                            '<div class="car-name" title="' + o.Name + '">' +
                            /*  '<span  class="label label-lg font-weight-bold label-light-success label-inline">Published</span></br>' +*/
                            '<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + o.Name + '</a>' +
                            '<span class="text-muted font-weight-bold d-block">' + Address + '</span>' +
                            '<a  href="javascript:;"  class="text-muted font-weight-bold d-block vendorname">' + o.VendorName + '</a>' +
                            '</div>' +
                            '</div>'
                    }
                },
                {
                    "mData": null,
                    "bSortable": true,
                    className: "dt-center",
                    width: '30px',
                    "mRender": function (o) {
                        var data = o.ApprovalStatusID;
                        var status = {
                            "1": {
                                'title': 'New',
                                'class': ' label-light-light'
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
                            },
                        };

                        if (typeof status[data] === 'undefined') {

                            return '<span class="label label-lg font-weight-bold label-light-danger label-inline">New</span>';
                        }
                        return '<span class="label label-lg font-weight-bold' + status[data].class + ' label-inline">' + status[data].title + '</span>';
                    },
                },
                {
                    "mData": null,
                    width: '175px',
                    className: "dt-center",
                    "mRender": function (o) {

                        var ApprovalStatus = o.ApprovalStatus;
                        var actions = '';

                        actions += '<a type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" href="/Admin/Car/Details/' + o.ID + '\" title="View">' +
                            '<i class="fa fa-folder-open"></i>' +
                            '</a> ';

                        if (ApprovalStatus == 2) {
                            actions += '<button type="button" class="btn btn-outline-success btn-sm mr-1 btnapprove" onclick="Approve(this, ' + o.ID + ',true)">' +
                                '<i class="fa fa-check-circle"></i> Approve' +
                                '</button> ' +
                                '<button type="button" class="btn btn-outline-danger btn-sm mr-1 btnapprove" onclick="OpenModelPopup(this,\'/Admin/Car/Reject/' + o.ID + '/?approvalStatus=false\',true)">' +
                                '<i class="fa fa-times-circle"></i> Reject' +
                                '</button> ';
                        }

                        return actions;
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
    $('#vendordropdown').val("")
    $(document).on('click', '.vendorname', function () {
        $('#vendordropdown option:contains(' + this.text + ')').attr("selected", "selected");
        Filterlist();
    })
    KTDatatablesBasicScrollable.init();

    $(".btn_approveall").prop("disabled", true);
    $(".btn_rejectall").prop("disabled", true);

    $("#checkAll").change(function () {
        if (this.checked) {

            $('tbody input[type=checkbox]').each(function () {

                this.checked = true
                var check = CarSelection.push(this.value);
                $(".btnapprove").hide();
                $(".btn_approveall").prop("disabled", false);
                $(".btn_rejectall").prop("disabled", false);
                console.log("Selection", check);
            });

            $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Send For Approval (${CarSelection.ids})`);
        } else {
            $('tbody input[type=checkbox]').each(function () {

                this.checked = false
                var index = CarSelection.indexOf(this.value)
                CarSelection.splice(index, 1)
                $(".btn_approveall").prop("disabled", true);
                $(".btn_rejectall").prop("disabled", true);
                $(".btnapprove").show();
            });
            $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Send For Approval (${CarSelection.ids})`);
        }
    });
   
   
    $(document).on('change', '.Checkbox', function () {

        var chk = $('.Checkbox:checked').length;
        if (chk > 0) {
            $(".btnapprove").hide();
            $(".btn_approveall").prop("disabled", false);
            $(".btn_rejectall").prop("disabled", false);
        }
        else {
            $(".btn_approveall").prop("disabled", true);
            $(".btn_rejectall").prop("disabled", true);
            $(".btnapprove").show();
        }
    });
    $('[name="cbox[]"]:checked').length
});

function Search(element) {
    table1.search($(element).text().trim()).draw();
}

$(document).on('click', '#approveall', function () {
    $('#approveall').addClass('spinner spinner-sm spinner-left').attr('disabled', true);
    var yourArray = [];
    $("input:checkbox[name=select_specialist]:checked").each(function () {
        yourArray.push($(this).val());
    });
    console.log(yourArray);
    $.ajax({
        url: '/Admin/Car/ApproveAll',
        type: 'POST',
        data: { 'ids': yourArray },
        success: function (response) {
            if (response.success) {
                toastr.options = {
                    "positionClass": "toast-bottom-right",
                };
                toastr.success('Motors Approved ...');
                window.setTimeout(function () { location.reload() }, 2000)
            }
            else {
                $('#approveall').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
            }
        },
        error: function (response) {
            $('#approveall').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
        }
    });
});
$(document).on('click', '#rejectall', function () {
    $('#rejectall').addClass('spinner spinner-sm spinner-left').attr('disabled', true);
    var yourArray = [];
    $("input:checkbox[name=select_specialist]:checked").each(function () {
        yourArray.push($(this).val());
    });
    console.log(yourArray);
    $.ajax({
        url: '/Admin/Car/RejectAll',
        type: 'POST',
        data: { 'ids': yourArray },
        success: function (response) {
            if (response.success) {
                toastr.options = {
                    "positionClass": "toast-bottom-right",
                };
                toastr.success('Motors rejected ... ');
                window.setTimeout(function () { location.reload() }, 2000)
            }
        },
        error: function (response) {
            $('#rejectall').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
        }
    });
});

function callback(dialog, elem, isedit, response) {

    if (response.success) {
        toastr.success(response.message);

        if (isedit) {
            table1.row($(elem).closest('tr')).remove().draw();
        }
        if (response.data.ApprovalStatus == 2) {
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
function Approve(element, record) {
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
                url: '/Admin/Car/Approve/' + record,
                type: 'Get',
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        window.setTimeout(function () { location.reload() }, 2000)

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

function addRow(row) {
    table1.row.add([
        row.CreatedOn,
        row.Motor,
        row.Vendor,
        row.ApprovalStatusID,
    ]).draw(true);

}