"use strict";
var table1;
var KTDatatablesBasicScrollable = function () {

	var initTable1 = function () {
		var table = $('#kt_datatable1');
		// begin first table
		table1 = table.DataTable({
			scrollY: '50vh',
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
					return '<button type="button" class="btn btn-secondary  btn-sm mr-1" onclick="OpenModelPopup(this,\'/Vendor/Customer/Details/' + data + '\')" title="View">' +
						'<i class="fa fa-folder-open"></i> Details' +
						'</button> ' ;
						
				},
			},
				{
					targets: 3,
					width: '175px',
				},
			{
				targets: 5,
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
				url: '/Vendor/Customer/Activate/' + record,
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

//function Booking(f) {
//	
//	var myId = $(f).attr('attr-id');

//	$.ajax({

//		url: "/Vendor/Customer/CustomerBooking",
//		type: "POST",
//		data: { id: myId }
		
//	})

 


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
		'<td data-order=' + row.ID + '> ' + row.Date + '</td>',
		row.Name,
		row.Contact,
		row.Email,
		row.IsActive,
		row.ID,
	]).draw(true);

}