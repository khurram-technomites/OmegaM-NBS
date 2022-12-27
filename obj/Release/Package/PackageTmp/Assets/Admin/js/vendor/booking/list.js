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
				orderable: false,

				render: function (data, type, full, meta) {
					

					/*data = data.split(',');*/
					//var isActive = data[0].toUpperCase();
					//var status = {
					//	"TRUE": {
					//		'title': 'Deactivate',
					//		'icon': 'fa-times-circle',
					//		'class': ' btn-outline-danger'
					//	},
					//	"FALSE": {
					//		'title': 'Activate',
					//		'icon': 'fa-check-circle',
					//		'class': ' btn-outline-success'
					//	},
					//};

					var actions = '';

					actions += '<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Admin/City/Details/' + data[1] + '\')" title="View">' +
						'<i class="fa fa-folder-open"></i>' +
						'</button> ';



					return actions;
				},
			},
			{
				targets: 3,
				width: '75px',
				render: function (data, type, full, meta) {
					

					var status = {
						"Pending": {
							'title': 'Pending',
							'class': ' label-light-dark'
						},
						"Confirmed": {
							'title': 'Confirmed',
							'class': ' label-light-success'
						},
						"Processing": {
							'title': 'Processing',
							'class': ' label-light-primary'
						},
						"Completed": {
							'title': 'Completed',
							'class': ' label-light-danger'
						},
						"Dispatched": {
							'title': 'Dispatched',
							'class': ' label-light-danger'
						},
						"Delivered": {
							'title': 'Delivered',
							'class': ' label-light-success'
						},
						"Returned": {
							'title': 'Returned',
							'class': ' label-light-success'
						},
						"Canceled": {
							'title': 'Canceled',
							'class': ' label-light-success'
						},
					};
					if (typeof status[data] === 'undefined') {
						return '<a  href="javascript:" class="label label-lg label-light-dark label-inline" onclick="OpenModelPopup(this,\'/Vendor/Booking/StatusChange/' + full[4] + '\',true)">' + data + '</a>';
					}
					return '<a href="javascript:" class="label label-lg ' + status[data].class + ' label-inline" onclick="OpenModelPopup(this,\'/Vendor/Booking/StatusChange/' + full[4] + '\',true)">' + data + ' </a>';

				},
			},
				//{
				//	targets: 3,
				//	width: '75px',
				//	render: function (data, type, full, meta) {

				//		data = data.toUpperCase();
				//		var status = {
				//			"TRUE": {
				//				'title': 'Active',
				//				'class': ' label-light-success'
				//			},
				//			"FALSE": {
				//				'title': 'InActive',
				//				'class': ' label-light-danger'
				//			},
				//		};
				//		if (typeof status[data] === 'undefined') {

				//			return '<span class="label label-lg font-weight-bold label-light-danger label-inline">Inactive</span>';
				//		}
				//		return '<span class="label label-lg font-weight-bold' + status[data].class + ' label-inline">' + status[data].title + '</span>';
				//	},
				//},
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
				url: '/Admin/City/Delete/' + record,
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
							toastr.success('City Deleted Successfully');

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
				url: '/Admin/City/Activate/' + record,
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
		row.Date,
		row.Car,
		row.Customer.Name,
		row.Status,
		row.ID,
	]).draw(true);

}

Number.prototype.padLeft = function (base, chr) {
	var len = (String(base || 10).length - String(this).length) + 1;
	return len > 0 ? new Array(len).join(chr || '0') + this : this;
}