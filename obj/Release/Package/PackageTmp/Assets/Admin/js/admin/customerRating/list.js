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
			columnDefs: [
				{
					targets: 0,
					className: "dt-center",
					width: '130px',
				}, {
					targets: -1,
					width: '200px',
					title: 'Actions',
					className: "dt-center",
					orderable: false,
					render: function (data, type, full, meta) {

						data = data.split(',');
						var ApprovalStatus = data[0];
						var actions = '';

						actions += '<a type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Admin/CustomerRating/Details/' + data[1] + '\')" title="View">' +
										'<i class="fa fa-folder-open"></i>' +
									'</a> ';


						actions += '<button type="button" class="btn btn-outline-success btn-sm mr-1" onclick="Approval(this,' + data[1] + ', ' + true + ',true)">' +
										'<i class="fa fa-check-circle"></i> Approve' +
									'</button> ' +
									'<button type="button" class="btn btn-outline-danger btn-sm mr-1" onclick="Approval(this,' + data[1] + ', ' + false + ',true)">' +
										'<i class="fa fa-times-circle"></i> Reject' +
									'</button> ';


						return actions;
					},
				},
            //{
            //	targets: 4,
            //	width: '205px',

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

function Approval(element, data, status) {
	swal.fire({
		title: 'Are you sure?',
		text: "You won't be able to revert this!",
		type: 'warning',
		showCancelButton: true,
		confirmButtonText: 'Yes, do it!'
	}).then(function (result) {
		if (result.isConfirmed) {
			$.ajax({
				url: '/Admin/CustomerRating/Approval',
				type: 'POST',
				data: { 'id': data, 'status': status },
				success: function (response) {
					if (response.success) {
						toastr.options = {
							"positionClass": "toast-bottom-right",
						};
						//toastr.success('City Deleted Successfully');

						table1.row($(element).closest('tr')).remove().draw();

					} else {
						//toastr.error(response.message);
						$(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
						$(element).find('i').show();
					}
				}
			});
		}
	});
}

jQuery(document).ready(function () {
	KTDatatablesBasicScrollable.init();

	$('.btn-rating').each(function (k, v) {
		var rating = parseFloat($(v).attr('data'));
		$(this).find('i:lt(' + (rating) + ')').addClass("la-star").removeClass("la-star-o");
	});

	$(".seemore").click(function () {
		Swal.fire($(this).text());
	});
});

function callback(dialog, elem, isedit, response) {


	if (response.success) {
		toastr.success(response.message);

		if (isedit) {
			table1.row($(elem).closest('tr')).remove().draw();
		}
		if (response.data.IsApproved == false) {
			addRow(response.data);
		}
		else {
			table1.row($(elem).closest('tr')).remove();
		}

		//addRow(response.data);
		//jQuery('form', dialog).closest('.modal').find('button[type=submit]').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
		//jQuery('#myModal').modal('hide');
	}
	else {
		jQuery('form', dialog).closest('.modal').find('button[type=submit]').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);

		toastr.error(response.message);
	}
}

function addRow(row) {
	table1.row.add([
		row.Date,
        row.OrderDetailID,
		row.Car,
		row.Rating,
		row.IsApproved + ',' + row.ID,
	]).draw(true);

}