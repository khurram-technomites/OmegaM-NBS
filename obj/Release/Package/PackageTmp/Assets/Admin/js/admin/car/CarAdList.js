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

					data = data.split(',');
					var ApprovalStatus = data[0];
					var actions = '';

					actions += '<a type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" href="/Admin/Car/Details/' + data[1] + '\" title="View">' +
									'<i class="fa fa-folder-open"></i>' +
								'</a> '; 

					if (ApprovalStatus == 2) {
					    actions += '<button type="button" class="btn btn-outline-success btn-sm mr-1 btnapprove" onclick="OpenModelPopup(this,\'/Admin/Car/Approve/' + data[1] + '/?approvalStatus=true\',true)">' +
									'<i class="fa fa-check-circle"></i> Approve' +
								'</button> ' +
								'<button type="button" class="btn btn-outline-danger btn-sm mr-1 btnapprove" onclick="OpenModelPopup(this,\'/Admin/Car/Approve/' + data[1] + '/?approvalStatus=false\',true)">' +
									'<i class="fa fa-times-circle"></i> Reject' +
								'</button> ';
					}

					return actions;
				},
			},
			{
				targets: 2,
				//width: '75px',
				render: function (data, type, full, meta) {
					console.log(data);
					if (!data) {
						return '<span>-</span>';
					}
					var vendor = data.split('|');
					console.log(vendor);
					return '<div class="d-flex align-items-center">' +
								//'<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
								//	'<div class="symbol-label" style="background-image: url(\'' + vendor[0] + '\')"></div>' +
								//'</div>' +
								'<div>' +
									'<a href="#" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + vendor[0] + '</a>' +
									'<span class="text-muted font-weight-bold d-block">' + vendor[1] + '</span>' +
								'</div>' +
							'</div>';
				},
			},
			{
				targets: 3,
				//width: '75px',
				render: function (data, type, full, meta) {
					if (!data) {
						return '<span>-</span>';
					}
					var car = data.split('|');
					return '<div class="d-flex align-items-center">' +
								'<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
									//'<div class="symbol-label" style="background-image: url(\'' + vendor[0] + '\')"></div>' +
								'</div>' +
								'<div>' +
									'<a href="#" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + car[0] + '</a>' +
									'<span class="text-muted font-weight-bold d-block">' + car[1] + '</span>' +
								'</div>' +
							'</div>';
				},
			},
                {
                	targets: 4,
                	width: '75px',
                	render: function (data, type, full, meta) {
                		var status = {
                			"1": {
                				'title': 'New',
                				'class': ' label-light-light'
                			},
                			"2": {
                				'title': 'Pending',
                				'class': ' label-light-primary'
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

function addRow(row) {
	table1.row.add([
		row.Date,
		row.Vendor,
		row.Car,
		row.ApprovalStatus,
		row.ApprovalStatus + ',' + row.ID,
	]).draw(true);

}