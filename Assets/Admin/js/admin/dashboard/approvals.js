"use strict";
var table1;
var KTDatatablesBasicScrollable1 = function () {

	var initTable2 = function () {
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
			columnDefs: [{
				targets: -1,
				title: 'Actions',
				orderable: false,
				width: '135px',
				className: "dt-center",
				render: function (data, type, full, meta) {

					data = data.split(',');
					var ApprovalStatus = data[0];
					console.log("Data ", data)
					var actions = '';

					actions += '<a type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" href="/Admin/Car/Details/' + data[1] + '\" title="View">' +
						'<i class="fa fa-folder-open"></i>' +
						'</a> ';

					if (ApprovalStatus == 2) {
						actions += '<button type="button" class="btn btn-outline-success btn-sm mr-1 btnapprove" onclick="Approve(this, ' + data[1] + ',true)">' +
							'<i class="fa fa-check-circle"></i> Approve' +
							'</button> ' +
							'<button type="button" class="btn btn-outline-danger btn-sm mr-1 btnapprove" onclick="OpenModelPopup(this,\'/Admin/Car/Reject/' + data[1] + '/?approvalStatus=false\',true)">' +
							'<i class="fa fa-times-circle"></i> Reject' +
							'</button> ';
					}

					return actions;
				},
			},
				{
					targets: 1,
					width: '70px',
                },
			{
				targets: 2,
				width: '250px',
				render: function (data, type, full, meta) {
					console.log(data);
					if (!data) {
						return '<span>-</span>';
					}
					var vendor = data.split(',');
					console.log(vendor);
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
					targets: 0,
					visible: false,
					width: '1px',
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
			//{
			//	targets: 3,
			//	//width: '75px',
			//	render: function (data, type, full, meta) {
			//		if (!data) {
			//			return '<span>-</span>';
			//		}
			//		var car = data.split('|');
			//		return '<div class="d-flex align-items-center">' +
			//					'<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
			//			'<div class="symbol-label" style="background-image: url(\'' + car[2] + '\')"></div>' +
			//					'</div>' +
			//					'<div>' +
			//						'<a href="#" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + car[0] + '</a>' +
			//						'<span class="text-muted font-weight-bold d-block">' + car[1] + '</span>' +
			//					'</div>' +
			//				'</div>';
			//	},
			//},
                {
                	targets: 3,
					width: '30px',
                	render: function (data, type, full, meta) {
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
			],
		});
	};

	return {
		//main function to initiate the module
		init: function () {
			initTable2();
		},
	};
}();

jQuery(document).ready(function () {
	KTDatatablesBasicScrollable1.init();
});

function callback(dialog, elem, isedit, response) {

	if (response.success) {
		toastr.success(response.message);
		window.location.reload();
		//toastr.success(response.message);


		//if (isedit) {
		//	table1.row($(elem).closest('tr')).remove().draw();
		//	Table.row($(element).closest('tr')).remove().draw();
		//}
		//if (response.data.ApprovalStatus == 2) {
		//    addRow(response.data);
		//}
		//else {
		//	table1.row($(elem).closest('tr')).remove();
		//	Table.row($(element).closest('tr')).remove().draw();
		//}

		////addRow(response.data);
		//jQuery('form', dialog).closest('.modal').find('button[type=submit]').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
		//jQuery('#myModal').modal('hide');
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
		row.Date,
		row.Vendor,
		row.Car,
		row.ApprovalStatus,
		row.ApprovalStatus + ',' + row.ID,
	]).draw(true);

}