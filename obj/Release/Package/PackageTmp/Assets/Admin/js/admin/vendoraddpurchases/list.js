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
			columnDefs: [
				{
					targets: 0,
					className: "dt-center",
					width: '130px',
				},
				{
					targets: 1,
					className: "dt-center",
				},
				{
					targets: 2,
					title: 'ExpiryDate',
					orderable: false,
					width: '230px',
					className: "dt-center",
					render: function (data, type, full, meta) {
						var actions = '';
						const date = new Date();
						const expirydate = new Date(data);
						if (date > expirydate) {
							actions += '<span  class="label label-lg font-weight-bold label-danger label-inline">Expired</span></br>' +
								'' + data + ''
						}
						else {
							actions += '<span  class="label label-lg font-weight-bold label-success label-inline">Active</span></br>' +
								'' + data + ''
						}

						return actions;
					}
				},
					{
					targets: -1,
					title: 'Actions',
					orderable: false,
					width: '150px',
					className: "dt-center",
					render: function (data, type, full, meta) {


						var actions = '';


						actions += '<button type="button" class="btn btn-bg-secondary  btn-sm mr-1" onclick="OpenModelPopup(this,\'/Admin/VendorAddPurchases/Details/' + data + '\')" title="View">' +
							'<i class="fa fa-folder-open"></i>Details' +

							'</button> '

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
	KTDatatablesBasicScrollable.init();
});

$(document).on('change', '#vendordropdown', function () {
	var vendorid = $('#vendordropdown').val() == '' ? 0 : $('#vendordropdown').val()
	$.ajax({
		url: "/Admin/VendorAddPurchases/List?VendorId=" + vendorid,
		type: 'POST',
		/*data: { VendorId = $('#vendordropdown').val() },*/
		success: function (response) {
			$('#list').html(response)
			 //var table = $("#kt_datatable1");
			 //table.DataTable().destroy()
			$('#vendordropdown').val(vendorid)
			 KTDatatablesBasicScrollable.init()
        }
	})
	
})
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
		row.Vendor,
		row.ExpiryDate,
		row.NoOfProperty,
		row.NoOfMotor,
		row.Price,
		row.ID,
	]).draw(true);
}