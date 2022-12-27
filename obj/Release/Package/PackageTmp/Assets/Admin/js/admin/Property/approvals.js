"use strict";
var table1;
var PropertySelection = [];
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
                if (json.recordsTotal <= 0) {
                    $('.excel-btn').prop('disabled', true);
                }
                else {
                    $('.excel-btn').prop('disabled', false);
                }
            },
            lengthMenu: [
                [10, 25, 100, 500, -1],
                ['10', '25', '100', '500', 'Show all']
            ],
            "proccessing": true,
            "serverSide": true,
            "ajax": {
                url: "/Admin/Property/ApprovalsList",
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
                    width: '45px',
                    "mRender": function (o) {

                        var Published = o.IsPublished ? o.IsPublished.toString().toUpperCase() : "FALSE";
                        if (Published == "FALSE") {
                            return `<label class="checkbox checkbox-lg checkbox-inline">
										<input type="checkbox" value="${o.ID}" id="chk${o.ID}" name="chkCar" onchange="PropertySelected(this,${o.ID})">
										<span></span>
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
                    width: '130px',

                },
                {
                    "mData": null,
                    "bSortable": true,
                    /*className: "dt-center",*/
                    width: '475px',
                    "mRender": function (o) {
                        var Soldclass = o.IsSold == true ? "" : "hidden";
                        var ForSale = o.ForSale == true ? "Sale" : "Rent";
                        var Address = o.Address == null ? "" : o.Address;

                        if (o.IsPublished == true) {
                            return '<div class="d-flex align-items-center">' +
                                '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                                '<div class="symbol-label" style="background-image: url(\'' + o.Thumbnail + '\')"></div>' +
                                /*`<span class="sold">Sold</span>`+*/
                                '</div>' +
                                '<div class="car-name" title="' + o.Title + '">' +
                                '<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + o.Title + '</a>' +
                                '<span class="text-muted font-weight-bold d-block">' + Address + '</span>' +
                                '<a  href="javascript:;" class="text-muted font-weight-bold d-block vendorname">' + o.VendorName + '</a>' +
                                /* '<span class="text-muted font-weight-bold d-block">' + split[4] + '</span>' +*/
                                '</div>' +
                                '</div>'
                        }
                        else {
                            return '<div class="d-flex align-items-center">' +
                                '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                                '<div class="symbol-label" style="background-image: url(\'' + o.Thumbnail + '\')"></div>' +
                                /*`<span class="sold">Sold</span>`+*/
                                '</div>' +
                                '<div class="car-name" title="' + o.Title + '">' +
                                '<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + o.Title + '</a>' +
                                '<span class="text-muted font-weight-bold d-block">' + Address + '</span>' +
                                '<a  href="javascript:;" class="text-muted font-weight-bold d-block vendorname">' + o.VendorName + '</a>' +
                                /* '<span class="text-muted font-weight-bold d-block">' + split[4] + '</span>' +*/
                                '</div>' +
                                '</div>'
                        }

                    }
                },
                {
                    "mData": null,
                    "bSortable": true,
                    className: "dt-center",
                    width: '75px',
                    "mRender": function (o) {

                        var data = o.ApprovalStatusID.toString().toUpperCase();
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
                    }
                },
                {
                    "mData": null,
                    width: '135px',
                    className: "dt-center",
                    "mRender": function (o) {

                        var actions = '';

                        actions += '<a type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" href="/Admin/Property/Details/' + o.ID + '\" title="View">' +
                            '<i class="fa fa-folder-open"></i>' +
                            '</a> ';

                        if (o.ApprovalStatusID == "2") {
                            actions += '<button type="button" class="btn btn-outline-success btn-sm mr-1 btnapprove" onclick="Approve(this, ' + o.ID + ')">' +
                                '<i class="fa fa-check-circle"></i> Approve' +
                                '</button> ' +
                                '<button type="button" class="btn btn-outline-danger btn-sm mr-1 btnapprove" onclick="OpenModelPopup(this,\'/Admin/Property/Reject/' + o.ID + '/?approvalStatus=false\',true)">' +
                                '<i class="fa fa-times-circle"></i> Reject' +
                                '</button> ';
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

$(document).ready(function () {
    $('#vendordropdown').val("")
    $(document).on('click', '.vendorname', function () {
        $('#vendordropdown option:contains(' + this.text + ')').attr("selected", "selected");
        Filterlist();
    })
    KTDatatablesBasicScrollable.init();


})


//$(document).on('change', '#vendordropdown', function () {
//    var vendor = $('#vendordropdown').val() == "" ? "0" : $('#vendordropdown').val();
//    $.ajax({
//        url: '/Admin/Property/ApprovalsList/',
//        type: 'POST',
//        data: { 'VendorId': vendor },
//        success: function (response) {

//            if (response != null) {
//                debugger
//                //$('#propertyapproval').html(response)
//                $('#checks').html($(response).find('.card-body').html())
//                $('#vendordropdown').val(vendor == "0" ? "" : vendor)
//                
//                //table.DataTable().destroy()
//                KTDatatablesBasicScrollable.init();
//                $("#checkAll").change(function () {
//                    PropertySelection = []
//                    if (this.checked) {
//                        $('tbody input[type=checkbox]').each(function () {
//                            this.checked = true
//                            var check = PropertySelection.push(this.value);
//                            $(".btnapprove").hide();
//                            $(".btn_approveall").prop("disabled", false);
//                            $(".btn_rejectall").prop("disabled", false);
//                            console.log("Selection", check);
//                        });
//                        $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Send For Approval (${PropertySelection.ids})`);
//                    } else {
//                        $('tbody input[type=checkbox]').each(function () {
//                            this.checked = false
//                            var index = PropertySelection.indexOf(this.value)
//                            PropertySelection.splice(index, 1)
//                            $(".btn_approveall").prop("disabled", true);
//                            $(".btn_rejectall").prop("disabled", true);
//                            $(".btnapprove").show();
//                        });
//                        $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Send For Approval (${PropertySelection.ids})`);
//                    }
//                });

//            }


//        },
//    })
//})

function Search(element) {
    table1.search($(element).text().trim()).draw();
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
                url: '/Admin/Property/Approve/' + record,
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
        row.CreatedOn,
        row.Property,
        row.Vendor,
        row.ApprovalStatusID,
    ]).draw(true);

}

