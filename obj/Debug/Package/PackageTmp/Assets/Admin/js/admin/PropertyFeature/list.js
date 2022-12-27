﻿"use strict";

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
                orderable: false,
                className: "dt-center",

                render: function (data, type, full, meta) {

                    data = data.split(',');
                    var IsApproved = data[2];
                    var isActive = data[0].toUpperCase();
                    var status = {
                        "TRUE": {
                            'title': 'Deactivate',
                            'icon': 'fa-times-circle',
                            'class': ' btn-outline-danger'
                        },
                        "FALSE": {
                            'title': 'Activate',
                            'icon': 'fa-check-circle',
                            'class': ' btn-outline-success'
                        },
                    };

                    var actions = '';



                    if (IsApproved == '' || IsApproved == 'null') {
                        actions += '<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Admin/PropertyFeature/Edit/' + data[1] + '\',true)">' +
							'<i class="fa fa-pen"></i>' +
							'</button> ' +
							'<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Admin/PropertyFeature/Details/' + data[1] + '\')" title="View">' +
							'<i class="fa fa-folder-open"></i>' +
							'</button> ' +
							'<button type="button" class="btn btn-outline-success btn-sm mr-1 btnapprove" onclick="Approval(this, ' + data[1] + ',' + true + ')">' +
							'<i class="fa fa-check-circle"></i> Approve' +
							'</button> ' +
							'<button type="button" class="btn btn-outline-danger btn-sm mr-1 btnapprove" onclick="Approval(this, ' + data[1] + ',' + false + ')">' +
							'<i class="fa fa-times-circle"></i> Reject' +
							'</button> ';
                    }
                    else {
                        actions += '<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Admin/PropertyFeature/Edit/' + data[1] + '\',true)">' +
							'<i class="fa fa-pen"></i>' +
							'</button> ' +
							'<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Admin/PropertyFeature/Details/' + data[1] + '\')" title="View">' +
							'<i class="fa fa-folder-open"></i>' +
							'</button> ' +
							'<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="Delete(this,' + data[1] + ')"><i class="fa fa-trash"></i></button>';

                        if (typeof status[isActive] === 'undefined') {
                            actions += '<button type="button" class="btn btn-outline-success btn-sm mr-1" onclick="Activate(this, ' + data[1] + ')">' +
								'<i class="fa fa-check-circle" aria-hidden="true"></i> Activate' +
								'</button>';
                        } else {
                            actions += '<button type="button" class="btn btn-sm mr-1' + status[isActive].class + '" onclick="Activate(this, ' + data[1] + ')">' +
								'<i class="fa ' + status[isActive].icon + '" aria-hidden="true"></i> ' + status[isActive].title +
								'</button>';
                        }
                    }



                    return actions;
                },
            },
				{
				    targets: 1,
				    //width: '75px',
				    render: function (data, type, full, meta) {
				        if (!data) {
				            return '<span>-</span>';
				        }
				        var category = data.split('|');

				        return '<div class="d-flex align-items-center">' +
							'<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
							'<div class="symbol-label symbol-label-color-size" style="background-image: url(\'' + category[0] + '\')"></div>' +
							'</div>' +
							'<a href="#" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + category[1] + '</a>' +
							//'<span class="text-muted font-weight-bold d-block">' + category[1] + '</span>' +
							'</div>' +
							'</div>';
				    },
				},
			{
			    targets: 2,
			    width: '75px',
			    render: function (data, type, full, meta) {

			        data = data.toUpperCase();
			        var status = {
			            "TRUE": {
			                'title': 'Active',
			                'class': ' label-light-success'
			            },
			            "FALSE": {
			                'title': 'InActive',
			                'class': ' label-light-danger'
			            },
			        };
			        if (typeof status[data] === 'undefined') {

			            return '<span class="label label-lg font-weight-bold label-light-danger label-inline">Inactive</span>';
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
                url: '/Admin/PropertyFeature/Delete/' + record,
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
                            toastr.success('Feature deleted successfully ...');

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

function Activate(element, record) {
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
                url: '/Admin/PropertyFeature/Activate/' + record,
                type: 'Get',
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
        } else {
            //swal("Cancelled", "Your imaginary file is safe :)", "error");
        }
    });
}

function Approval(element, record, value) {
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
                url: '/Admin/PropertyFeature/Approve/',
                data: { 'id': record, 'value': value },
                type: 'Post',

                success: function (response) {

                    if (response.success) {
                        toastr.success(response.message);
                        table1.row($(element).closest('tr')).remove().draw();
                        if (value == true) {
                            addRow(response.data);
                        }

                    } else {
                        toastr.error(response.message);
                        $(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        $(element).find('i').show();
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

    //let tr = `<tr>
    //                	<td > ${row.Date}</td>
    //                	<td>${row.Image}|${row.Name}</td>
    //                	<td>${row.IsActive}</td>
    //                	<td>${row.IsActive},${row.ID},${row.IsApproved}</td>
    //                </tr>`;

    //table1.row.add(tr).draw(true);

    //data-order='${row.ID}'

    table1.row.add([
    	 row.Date,
    	row.Image + '|' + row.Name,
    	row.IsActive,
    	row.IsActive + ',' + row.ID + ',' + row.IsApproved,
    ]).draw(true);

}