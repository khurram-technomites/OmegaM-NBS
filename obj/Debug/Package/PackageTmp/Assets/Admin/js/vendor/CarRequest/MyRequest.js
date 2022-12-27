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
					var actions = '';
					actions +=
						'<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Vendor/CarRequests/MyRequestDetails?RequestId=' + data + '\')" title="View">' +
						'<i class="fa fa-folder-open"></i>' +
						'</button> ';

					return actions;
				},
			},
			{
				targets: 2,
				render: function (data, type, full, meta) {
					if (!data) {
						return '<span>-</span>';
					}
					return '<div class="d-flex align-items-center">' +
						'<div>' +
						'<a href="#" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + data + '</a>' +
						'</div>' +
						'</div>';
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
        bindButton();
});





function bindButton() {
    

}
function addRow(row) {
	table1.row.add([
		'<td data-order=' + row.ID + '> ' + row.Date + '</td>',
		row.Title,
		row.Category1.CategoryName,
		row.Transmission,
		row.Horsepower,
		row.Description,
		row.ID,
	]).draw(true);

}

Number.prototype.padLeft = function (base, chr) {
	var len = (String(base || 10).length - String(this).length) + 1;
	return len > 0 ? new Array(len).join(chr || '0') + this : this;
}