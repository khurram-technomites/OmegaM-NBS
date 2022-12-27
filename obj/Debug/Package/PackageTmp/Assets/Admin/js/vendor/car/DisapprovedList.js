 "use strict";
var table1;
var CarSelection = [];


$(document).ready(function () {
    KTDatatablesBasicScrollable.init();

    $("#checkAll").change(function () {
        if (this.checked) {
            CarSelection = []
            $('tbody input[type=checkbox]').each(function () {
                this.checked = true
                CarSelection.push(this.value);
            });
            $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Send For Approval (${CarSelection.length})`);
        } else {
            $('tbody input[type=checkbox]').each(function () {
                this.checked = false
                var index = CarSelection.indexOf(this.value)
                CarSelection.splice(index, 1)
            });
            $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Send For Approval (${CarSelection.length})`);
        }
    });
})
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
                url: "/Vendor/Car/ApprovalList",
                type: 'POST',
                data: function (d) {

                    d.filter = $('#filterdropdown').val();
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
                        if (Published == "FALSE" && o.ApprovalStatus != '2') {
                            if (typeof (CarSelection.find(x => x == o.ID.toString())) == "string") {
                                return `<label class="checkbox checkbox-lg checkbox-inline">
										<input type="checkbox" checked value="${o.ID}" id="chk${o.ID}" name="chkCar" onchange="CarSelected(this,${o.ID})">
										<span></span>
									</label>`;
                            } else {
                                return `<label class="checkbox checkbox-lg checkbox-inline">
										<input type="checkbox" value="${o.ID}" id="chk${o.ID}" name="chkCar" onchange="CarSelected(this,${o.ID})">
										<span></span>
									</label>`;
                            }
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
                        return '<div class="d-flex align-items-center">' +
                            '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                            '<div class="symbol-label" style="background-image: url(\'' + o.Thumbnail + '\')"></div>' +
                            '</div>' +
                            '<div class="car-name" title="' + o.Name + '">' +
                            '<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + o.Name + '</a>' +
                            '<span class="text-muted font-weight-bold d-block">' + o.AdsReferenceCode + '</span>' +
                            '</div>' +
                            '</div>'
                    }
                },
                {
                    "mData": null,
                    "bSortable": true,
                    className: "dt-center",
                    width: '75px',
                    "mRender": function (o) {

                        var data = o.ApprovalStatus
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
                        debugger;
                        var approvalID = o.ApprovalStatus
                        var status = {
                            "1": {
                                'title': 'Send For Approval',
                                'icon': 'fa-check-circle',
                                'class': ' btn-outline-success'
                            },
                            "2": {
                                'title': 'Cancel Approval Request',
                                'icon': 'fa-times-circle',
                                'class': ' btn-outline-danger'
                            },
                            "4": {
                                'title': 'Send For Approval',
                                'icon': 'fa-check-circle',
                                'class': ' btn-outline-success'
                            },
                        };

                        var action;

                        action = '<a class="btn btn-bg-secondary btn-icon btn-sm mr-1" href="/Vendor/Car/Edit/' + o.ID + '">' +
                            '<i class="fa fa-pen"></i>' +
                            '</a> ' +
                            '<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="Delete(this,' + o.ID + ')"><i class="fa fa-trash"></i></button>';
                        if (approvalID != 3 && approvalID != 2) {
                            action += '<button type="button" class="btn btn-sm mr-1' + status[approvalID].class + '" onclick="IsApproved(this, ' + o.ID + ')">' +
                                '<i class="fa ' + status[approvalID].icon + '" aria-hidden="true"></i> ' + status[approvalID].title +
                                '</button>';
                        }

                        if (approvalID == 2) {
                            action += '<button type="button" class="btn btn-sm mr-1' + status[approvalID].class + '" onclick="IsCanceled(this, ' + o.ID + ')">' +
                                '<i class="fa ' + status[approvalID].icon + '" aria-hidden="true"></i> ' + status[approvalID].title +
                                '</button>';

                        }
                        if (o.Remarks) {
                            debugger;
                            action += '<button type="button" id="approveall" class="btn btn-outline-danger btn-sm mr-1" onclick="OpenModelPopup(this,\'/Vendor/Car/Remarks/' + o.ID + '/?approvalStatus=true\',true)">' +
                                'Remarks' +
                                '</button >';
                        }

                        return action;
                    }
                },

            ],
            order: [[3, 'desc']]
        });
    };

    return {
        //main function to initiate the module
        init: function () {
            initTable1();
        },
    };
}();

function BindDataTable() {

    Table = $('#kt_datatable1').DataTable({
        "filter": true,
        "initComplete": function (settings, json) {
            $('#kt_datatable1 tbody').fadeIn();
        },
        "columns": [
            
            {
                "data": "ID",
                "width":"1px",
                "visible": false,
                className: "dt-center",
            },
            {
                "data": "isPublished",
                "width": "20px",
                "orderable": false,
                className : "dt-center",
                "render": function (data, type, row) {

                    var Published = data

                    if (Published != 2) {
                        return `<label class="checkbox checkbox-lg checkbox-inline">
										<input type="checkbox" value="${row.ID}" id="chk${row.ID}" name="chkCar" onchange="CarSelected(this,${row.ID})">
										<span></span>
									</label>`;
                    }
                    else {
                        return '';
                    }
                }
            },
            {
                "data": "CreatedOn",
                "width" : "80px",
                className: "dt-center",
            },
            {
                "data": "Thumbnail,Title,Address",
                /*className: "dt-center",*/
                "width" : "290px",
                "render": function (data, type, row) {
                    var split = data.split(',');
                    return '<div class="d-flex align-items-center">' +
                        '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                        '<div class="symbol-label" style="background-image: url(\'' + split[0] + '\')"></div>' +
                        '</div>' +
                        '<div class="car-name" title="' + split[1] + '">' +
                        '<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + split[1] + '</a>' +
                        '<span class="text-muted font-weight-bold d-block">' + split[2] + '</span>' +
                        '</div>' +
                        '</div>'
                }
            },
            {
                "data": "ApprovalStatus",
                className: "dt-center",
                width : "80px",
                "render": function (data) {


                    var status = {
                        "1": {
                            'title': 'New',
                            'class': ' label-light-success'
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

                        return '<span class="label label-lg font-weight-bold label-light-danger label-inline">Unpublished</span>';
                    }
                    return '<span class="label label-lg font-weight-bold' + status[data].class + ' label-inline">' + status[data].title + '</span>';
                }
            },
            {
                "data": "Remarks",
                "orderable": false,
                width : "130px",
                "render": function (data, type, row) {

                    var approvalID = row.ApprovalStatus
                    var status = {
                        "1": {
                            'title': 'Send For Approval',
                            'icon': 'fa-check-circle',
                            'class': ' btn-outline-success'
                        },
                        "2": {
                            'title': 'Cancel Approval Request',
                            'icon': 'fa-times-circle',
                            'class': ' btn-outline-danger'
                        },
                        "4": {
                            'title': 'Send For Approval',
                            'icon': 'fa-check-circle',
                            'class': ' btn-outline-success'
                        },
                    };
                    console.log("Data", row);
                    var action;

                    action = '<a class="btn btn-bg-secondary btn-icon btn-sm mr-1" href="/Vendor/Car/Edit/' + row.ID + '">' +
                        '<i class="fa fa-pen"></i>' +
                        '</a> ' +
                        '<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="Delete(this,' + row.ID + ')"><i class="fa fa-trash"></i></button>';
                    if (approvalID != 3 && approvalID != 2) {
                        action += '<button type="button" class="btn btn-sm mr-1' + status[approvalID].class + '" onclick="IsApproved(this, ' + row.ID + ')">' +
                            '<i class="fa ' + status[approvalID].icon + '" aria-hidden="true"></i> ' + status[approvalID].title +
                            '</button>';
                    }

                    if (approvalID == 2) {
                        action += '<button type="button" class="btn btn-sm mr-1' + status[approvalID].class + '" onclick="IsCanceled(this, ' + row.ID + ')">' +
                            '<i class="fa ' + status[approvalID].icon + '" aria-hidden="true"></i> ' + status[approvalID].title +
                            '</button>';

                    }
                    if (data != "") {

                        action += '<button type="button" id="remarks" class="btn btn-outline-danger btn-sm mr-1" onclick="OpenModelPopup(this,\'/Vendor/Car/Remarks/' + row.ID + '/?approvalStatus=true\',true)">' +
                            'Remarks' +
                            '</button >';
                    }

                    return action;

                }
            }

        ],
        order: [[3, 'desc']]
    });
}

function CarSelected(element, record) {

    if (!CarSelection) {
        CarSelection = [];
    }

    var car = CarSelection.find(function (obj) { return obj == $(element).val() });

    if ($(element).prop('checked')) {
        if (!car) {
            CarSelection.push($(element).val());
        }
    } else {
        if (car) {
            CarSelection = CarSelection.filter(function (obj) { return obj != $(element).val() });
        }
    }

    var chk = CarSelection.length;
    if (chk > 0) {
        $(".btnapprove").hide();
        $(".btn-bulk-publish").prop("disabled", false);

        $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Send For Approval (${chk})`);
    }
    else {
        $(".btn-bulk-publish").prop("disabled", true);
        $(".btnapprove").show();

        $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Send For Approval (${chk})`);
    }

}

function IsApproved(element, record) {
    swal.fire({
        title: 'Motor Approval',
        text: "Send Your motor for Approval !",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, do it!'
    }).then(function (result) {
        if (result.value) {
            $(element).find('i').hide();
            $(element).addClass('spinner spinner-left spinner-sm').attr('disabled', true);

            $.ajax({
                url: '/Vendor/Car/IsApproved/' + record,
                type: 'Get',
                success: function (response) {
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
                }
            });
        } else {
            //swal("Cancelled", "Your imaginary file is safe :)", "error");
        }
    });
}

function IsCanceled(element, record) {
    swal.fire({
        title: 'Motor Approval Cancel',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, do it!'
    }).then(function (result) {
        if (result.value) {
            $(element).find('i').hide();
            $(element).addClass('spinner spinner-left spinner-sm').attr('disabled', true);

            $.ajax({
                url: '/Vendor/Car/CancelApproval/' + record,
                type: 'Get',
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        setTimeout(function () {
                            location.reload();
                        }, 1000);
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
                url: '/Vendor/Car/Delete/' + record,
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
                            toastr.success('Motor deleted successfully');

                            Table.row($(element).closest('tr')).remove().draw();
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

function BulkApproval(element, record) {

    swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, do it!'
    }).then(function (result) {
        if (result.value) {

            $(element).addClass('spinner spinner-sm spinner-left').attr('disabled', true);
            $(element).find('i').hide();

            //var SelectedCars = [];
            //$("input[name=chkCar]:checked").each(function () {
            //	SelectedCars.push($(this).val());
            //});

            if (CarSelection.length > 0) {
                $.ajax({
                    url: '/Vendor/Car/BulkSendForApproval',
                    type: 'POST',
                    data: { 'ids': CarSelection },
                    success: function (response) {
                        if (response.success) {
                            toastr.options = {
                                "positionClass": "toast-bottom-right",
                            };

                            toastr.success(response.message);

                            setTimeout(function () {
                                location.reload();
                            }, 1000);
                        } else {
                            /*toastr.error(response.message);*/

                            $(element).removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
                            $(element).find('i').show();
                            swal.fire({
                                title: 'Please complete the motor information before sending it for approval!',
                                html: response.message,
                                icon: 'error',
                                confirmButtonText: "Ok!",
                                //timer: 10000,
                            }).then((result) => {
                                if (result.isConfirmed) {
                                    setTimeout(function () {
                                        location.reload();
                                    }, 1000)
                                }
                            });

                        }
                    }
                });
            } else {
                $(element).removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
                $(element).find('i').show();
                toastr.error("Please select properties first!");
            }
        }
    });
}


