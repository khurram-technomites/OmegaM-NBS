"use strict";
var myTable1;
var KTDatatablesBasicScrollableer = function () {

	//var initTable2 = function () {
	//	var myTable = $('#datatablea');

	//	// begin first table
	//	myTable1 = myTable.DataTable({
	//		//scrollY: '50vh',
	//		scrollX: true,
	//		scrollCollapse: true,
	//		"language": {
	//			processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
	//		},
	//		"initComplete": function (settings, json) {
	//			$('#datatablea tbody').fadeIn();
	//		},
	//		columnDefs: [{
	//			targets: -1,
	//			title: 'ACTIONS',
	//			orderable: false,
	//			className: "dt-center",
	//			width: '150px',
	//			render: function (data, type, full, meta) {

	//				data = data.split(',');
	//				var status = data[0].toUpperCase();

	//				var actions = '';

	//				if (status && status === "PENDING") {
	//					actions += '<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="InProcess(this,' + data[1] + ')"><i class="fa fa-check"></i> </button>' +
	//								'<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="Reject(this, ' + data[1] + ')">' +
	//								'<i class="fa fa-times" aria-hidden="true"></i> ' +
	//								'</button>';

	//					actions += '<a class="btn btn-bg-secondary btn-icon btn-sm" href="/Admin/Order/Details?id=' + data[1] + '">' +
	//					'<i class="fa fa-arrow-right"></i> ' +
	//					'</a> ';
	//				}
	//				else {
	//					actions += '<a class="btn btn-bg-secondary btn-sm font-weight-bold" href="/Admin/Order/Details?id=' + data[1] + '">' +
	//					'Details <i class="fa fa-arrow-right"></i> ' +
	//					'</a> ';
	//				}

	//				return actions;
	//			},
	//		},
	//		{
	//			targets: 0,
	//			className: "dt-center",
	//			width: '130px',
	//		},
	//		{
	//			targets: 1,
	//			render: function (data, type, full, meta) {
	//				data = data.split('|');

	//				var action = `<div class="d-flex align-items-center">
	//								<div class="symbol symbol-circle symbol-50 flex-shrink-0 mr-4">
	//									<div class="symbol-label" style="background-image: url(${data[0]})"></div>
	//								</div>
	//								<div class="car-name">
	//									<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">${data[1]}</a>
	//									<br>
	//									<div class ="mb-1 customer-address" style="margin-left: 10px;">
	//											${data[2]}
	//										</div>
	//								</div>
	//							</div>`;
	//				return action;
	//			}
	//		},
	//		{
	//			targets: 2,
	//			className: "dt-center",
	//			width: '100px',
	//			render: function (data, type, full, meta) {
	//				data = data.split('|');

	//				var action = `<div class=" align-items-center">
	//								<div class="car-name">
	//									<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">${data[0]}</a>
	//									<br>
	//									<div class ="text-dark-75 font-weight-bolder">
	//											${data[1]}
	//										</div>
	//								</div>
	//							</div>`;

	//				return action;
	//			}
	//		},
	//		{
	//			targets: 4,
	//			className: "dt-center",
	//			width: '100px',
	//		},
	//		],
	//	});
	//};

	return {
		//main function to initiate the module
		//init: function () {
		//	initTable2();
		//},
	};
}();

jQuery(document).ready(function () {
	KTDatatablesBasicScrollableer.init();
});

function Reject(element, record) {

	swal.fire({
		title: 'Are you sure you want to Reject this booking?',
		text: "You won't be able to revert this!",
		type: 'warning',
		showCancelButton: true,
		confirmButtonText: 'Yes, Reject it!'
	}).then(function (response) {
		if (response.value) {
			$(element).find('i').hide();
			$(element).addClass('spinner spinner-center spinner-sm').attr('disabled', true);

			$.ajax({
				url: '/Admin/Dashboard/StatusChange/',
				type: 'POST',
				data: { "OrderID": record, "Status": "Canceled" },
				success: function (response) {
					if (response.success != undefined) {
						if (response.success) {
							toastr.options = {
								"positionClass": "toast-bottom-right",
							};
							toastr.success('Status updated Successfully');

							myTable1.row($(element).closest('tr')).remove().draw();
							addRow(response.data);
						}
						else {
							toastr.error(response.message);
							$(element).removeClass('spinner spinner-center spinner-sm').attr('disabled', false);
							$(element).find('i').show();
						}
					} else {
						swal.fire("Your are not authorize to perform this action", "For further details please contact administrator !", "warning").then(function () {
						});
						$(element).removeClass('spinner spinner-center spinner-sm').attr('disabled', false);
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

						$(element).removeClass('spinner spinner-center spinner-sm').attr('disabled', false);
						$(element).find('i').show();

					}
				}
			});
		}
	});
}

function InProcess(element, record) {

	swal.fire({
		title: 'Are you sure you want to Accept this booking?',
		text: "You won't be able to revert this!",
		type: 'warning',
		showCancelButton: true,
		confirmButtonText: 'Yes!'
	}).then(function (response) {
		if (response.value) {
			$(element).find('i').hide();
			$(element).addClass('spinner spinner-center spinner-sm').attr('disabled', true);

			$.ajax({
				url: '/Admin/Dashboard/StatusChange/',
				type: 'POST',
				data: { "OrderID": record, "Status": "Confirmed" },
				success: function (response) {
					if (response.success != undefined) {
						if (response.success) {
							toastr.options = {
								"positionClass": "toast-bottom-right",
							};
							toastr.success('Status updated Successfully');

							myTable1.row($(element).closest('tr')).remove().draw();
							addRow(response.data);
						}
						else {
							toastr.error(response.message);

							$(element).removeClass('spinner spinner-center spinner-sm').attr('disabled', false);
							$(element).find('i').show();
						}
					} else {
						swal.fire("Your are not authorize to perform this action", "For further details please contact administrator !", "warning").then(function () {
						});

						$(element).removeClass('spinner spinner-center spinner-sm').attr('disabled', false);
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

						$(element).removeClass('spinner spinner-center spinner-sm').attr('disabled', false);
						$(element).find('i').show();
					}
				}
			});
		}
	});
}

function callback(dialog, elem, isedit, response) {

	if (response.success) {
		toastr.success(response.message);

		if (isedit) {
			myTable1.row($(elem).closest('tr')).remove().draw();
		}
		if (response.data.Status == "Pending") {
			addRow(response.data);
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
	myTable1.row.add([
		row.Date,
		row.Customer.Logo + "|" + row.Customer.Name + "|" + row.Customer.Address,
		row.BookingNo + "|" + row.Status,
		row.Car,
		row.Vendor,
		row.Status + "," + row.ID,
	]).draw(true);

}

Number.prototype.padLeft = function (base, chr) {
	var len = (String(base || 10).length - String(this).length) + 1;
	return len > 0 ? new Array(len).join(chr || '0') + this : this;
}