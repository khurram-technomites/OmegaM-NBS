var pa = 0;
var BookingSelection = [];

var TableUnreconciledBookings;
var KTDatatablesUnreconciledBookings = function () {

	var initTable2 = function () {
		var myTable = $('#kt_datatable_unreconciled_bookings');

		// begin first table
		TableUnreconciledBookings = myTable.DataTable({
			scrollY: '50vh',
			scrollX: true,
			scrollCollapse: true,
			"language": {
				processing: '<i class="spinner spinner-center spinner-dark spinner-sm"></i>'
			},
			"initComplete": function (settings, json) {
				$('#kt_datatable_unreconciled_bookings tbody').fadeIn();
			},
			columnDefs: [{
				targets: -1,
				title: 'Amount',
				orderable: false,
				className: "dt-center",
				width: '150px',
				render: function (data, type, full, meta) {
					return 100;
				},
			},
			{
				targets: 1,
				className: "dt-center",
				width: '130px',
				render: function (data, type, full, meta) {
					var date = `new ` + data.replace(`/`, '').replace(`\/`, '')
					return moment(eval(date)).format('DD MMM YYYY, hh:mm a');
				}
			},
			{
				targets: 2,
				render: function (data, type, full, meta) {
					data = data.split('|');

					var action = `<div class="d-flex align-items-center">
									<div class="symbol symbol-circle symbol-50 flex-shrink-0 mr-4">
										<div class="symbol-label" style="background-image: url(${data[0]})"></div>
									</div>
									<div class="car-name">
										<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">${data[1]}</a>
										<br>
										<div class ="mb-1 customer-address" style="margin-left: 10px;">
												${data[2]}
											</div>
									</div>
								</div>`;
					return action;
				}
			},
			{
				targets: 3,
				className: "dt-center",
				width: '100px',
				render: function (data, type, full, meta) {
					data = data.split('|');

					var action = `<div class=" align-items-center">
									<div class="car-name">
										<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">${data[0]}</a>
										<br>
										<div class ="text-dark-75 font-weight-bolder">
												${data[1]}
											</div>
									</div>
								</div>`;

					return action;
				}
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

var TableWalletShareHistory;
var KTDatatablesWalletShareHistory = function () {

	var initTable2 = function () {
		var myTable = $('#kt_datatable_wallet_share_history');

		// begin first table
		TableWalletShareHistory = myTable.DataTable({
			scrollY: '50vh',
			scrollX: true,
			scrollCollapse: true,
			dom: 'Bfrtip',
			buttons: [
				{
					extend: 'excel',
					messageTop: function () {
						return 'Vendor Transfer History';
					},
					title: '',
					exportOptions: {
						columns: [0, 1, 2, 3]
					}
				}
			],
			"language": {
				processing: '<i class="spinner spinner-center spinner-dark spinner-sm"></i>'
			},
			"initComplete": function (settings, json) {
				$('#kt_datatable_wallet_share_history tbody').fadeIn();
			},
			columnDefs: [{
				targets: -1,
				className: "dt-center",
				width: '150px',
				render: function (data, type, full, meta) {

					var booking = `<button type="button" class="btn btn-bg-secondary btn-sm mr-1" onclick="OpenModelPopup(this,\'/Admin/VendorWalletShareHistory/Details/' + data + '\')" title="View">
										<i class="fa fa-calender"></i> Bookings
									</button> `;
					return booking;
				},
			},
			{
				targets: 0,
				className: "dt-center",
				width: '130px',
				render: function (data, type, full, meta) {
					var date = `new ` + data.replace(`/`, '').replace(`\/`, '')
					return moment(eval(date)).format('DD MMM YYYY, hh:mm a');
				}
			},
			{
				targets: 2,
				className: "dt-right",
				width: '100px',
				render: function (data, type, full, meta) {
					return `AED ${numeral(data).format('0,0.00')}`;
				}
			},
			{
				targets: 3,
				width: '100px',
				className: "dt-center",
				render: function (data, type, full, meta) {

					if (data == 1) {
						return `<p class="label label-lg  label-light-danger label-inline">Debit</p>`;
					} else {
						return `<p class="label label-lg  label-light-success label-inline">Credit</p>`;
					}
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

	$("#StartDate").datepicker({
		calendarWeeks: true,
		autoclose: true,
		todayHighlight: true,
	}).datepicker('update', new Date().addDays(-30));

	$("#EndDate").datepicker({
		calendarWeeks: true,
		autoclose: true,
		todayHighlight: true,
	}).datepicker('update', new Date());


	$("#VendorID").change(function () {

		if ($('#VendorID').val()) {
			
			$("#earning").addClass('spinner spinner-center');
			$("#pendingamount").addClass('spinner spinner-center');
			$("#transferedAmount").addClass('spinner spinner-center');

			GetVendorWalletShare();
			GetVendorBookings();
			GetVendorWalletShareHistory();
		}
		else {
			TableUnreconciledBookings.clear().draw()
			TableWalletShareHistory.clear().draw()
			$("#earning").html('').removeClass('spinner spinner-center');
			$("#pendingamount").html('').removeClass('spinner spinner-center');
			$("#transferedAmount").html('').removeClass('spinner spinner-center');
		}
	});

	$("#StartDate").change(function () {

		if (new Date($("#StartDate").val()) > new Date($("#EndDate").val())) {
			$('#EndDate').datepicker('setDate', new Date($("#StartDate").val()));
			$("#EndDate").datepicker("option", "minDate", new Date($("#StartDate").val()));
		}
	});

	$("#EndDate").change(function () {

		if (new Date($("#StartDate").val()) > new Date($("#EndDate").val())) {
			$('#StartDate').datepicker('setDate', new Date($("#EndDate").val()));
			$("#StartDate").datepicker("option", "maxDate", new Date($("#EndDate").val()));
		}
	});

	$('#btnFilter').click(function () {

		$('#btnFilter').find('i').hide();
		$('#btnFilter').addClass('spinner spinner-left').prop('disabled', true);

		GetVendorWalletShareHistory();
	});

	var id = Number(GetURLParameter());

	if (id) {
		$("#VendorID").val(id);
		$("#VendorID").trigger('change');
	}

	KTDatatablesUnreconciledBookings.init();
	KTDatatablesWalletShareHistory.init();
});

function GetVendorWalletShare() {

	if ($("#VendorID").val()) {
		$.ajax({
			url: '/Admin/VendorWalletShare/Details/' + $("#VendorID").val(),
			type: "Get",
			success: function (response) {
				if (response.success) {
					var data = response.data;
					$("#earning").text(data.TotalEarning);
					$("#pendingamount").text(data.PendingAmount);
					$("#transferedAmount").text(data.TransferedAmount);

					$("#txtTransferred").attr("max", data.PendingAmount);

					$('#BookingReconciliation div.points').each(function (k, v) {
						if (!$(v).hasClass('formatted')) {
							let text = $(v).text();

							$(v).html(numeral(text).format('0,0.00'));

							$(v).addClass('formatted');
						}
					});



					$('#BookingReconciliation').slideDown();
				} else {
					$('#BookingReconciliation').slideUp();
				}

				$("#earning").removeClass('spinner spinner-center');
				$("#pendingamount").removeClass('spinner spinner-center');
				$("#transferedAmount").removeClass('spinner spinner-center');
			},
			error: function (err) {

				$("#earning").removeClass('spinner spinner-center');
				$("#pendingamount").removeClass('spinner spinner-center');
				$("#transferedAmount").removeClass('spinner spinner-center');

				$('#BookingReconciliation').slideUp();
			}
		});
	} else {
		TableUnreconciledBookings.clear().draw()
		TableWalletShareHistory.clear().draw()
		$("#earning").html('').removeClass('spinner spinner-center');
		$("#pendingamount").html('').removeClass('spinner spinner-center');
		$("#transferedAmount").html('').removeClass('spinner spinner-center');
	}
}

function GetVendorBookings() {
	if ($('#VendorID').val()) {


		$.ajax({
			url: "/Admin/Order/GetVendorOrders",
			type: 'POST',
			data: {
				VendorId: $("#VendorID").val(),
				Status: 'Unreconciled',
			},
			success: function (response) {
				TableUnreconciledBookings.clear().draw();
				if (response.success) {
					$.each(response.data, function (k, v) {
						AddTableUnreconciledBookingRow(v);
					});
				} else {
					TableUnreconciledBookings.clear().draw();
				}
			}
		});
	} else {
		TableUnreconciledBookings.clear().draw()
		TableWalletShareHistory.clear().draw()
		$("#earning").html('').removeClass('spinner spinner-center');
		$("#pendingamount").html('').removeClass('spinner spinner-center');
		$("#transferedAmount").html('').removeClass('spinner spinner-center');
	}
}

function GetVendorWalletShareHistory() {
	if ($('#VendorID').val()) {


		$.ajax({
			url: "/Admin/VendorWalletShareHistory/GetVendorHistory",
			type: 'POST',
			data: {
				vendorId: $('#VendorID').val(),
				startDate: $("#StartDate").val(),
				endDate: $("#EndDate").val()
			},
			success: function (response) {
				TableWalletShareHistory.clear().draw();
				if (response.success) {
					$.each(response.data, function (k, v) {
						AddTableWalletShareHistoryRow(v);
					});
				} else {
					TableWalletShareHistory.clear().draw()
				}

				$('#btnFilter').find('i').show();
				$('#btnFilter').removeClass('spinner spinner-left').prop('disabled', false);
			}
		});
	} else {


		$("#earning").html('').removeClass('spinner spinner-center');
		$("#pendingamount").html('').removeClass('spinner spinner-center');
		$("#transferedAmount").html('').removeClass('spinner spinner-center');

		TableUnreconciledBookings.clear().draw()

		$('#btnFilter').find('i').show();
		$('#btnFilter').removeClass('spinner spinner-left').prop('disabled', false);
		TableWalletShareHistory.clear().draw()
	}
}

function BookingSelected(element, record) {

	if (!BookingSelection) {
		BookingSelection = [];
	}

	var booking = BookingSelection.find(function (obj) { return obj.BookingID === $(element).val() });

	if ($(element).prop('checked')) {
		if (!booking) {
			BookingSelection.push({ BookingID: $(element).val(), Amount: parseFloat($(element).closest('tr').find('td:eq(5)').html()) });
		}
	} else {
		if (booking) {
			BookingSelection = BookingSelection.filter(function (obj) { return obj.BookingID !== $(element).val() });
		}
	}

	$('#TotalReconciliationAmount').html(BookingSelection.sum("Amount") + ' AED');

	var chk = BookingSelection.length;
	if (chk > 0) {
		$("#btnReconcileAll").html(`<i class="fa fa-check-circle"></i> Reconcile (${chk})`).prop("disabled", false);
	}
	else {
		$("#btnReconcileAll").html(`<i class="fa fa-check-circle"></i> Reconcile (${chk})`).prop("disabled", true);
	}
}

function ReconcileAll(element, record) {

	swal.fire({
		title: 'Are you sure?',
		text: "You won't be able to revert this!",
		type: 'warning',
		showCancelButton: true,
		confirmButtonText: 'Yes, send it!'
	}).then(function (result) {
		if (result.value) {

			$(element).addClass('spinner spinner-sm spinner-left').attr('disabled', true);
			$(element).find('i').hide();

			if (BookingSelection.length > 0) {
				$.ajax({
					url: '/Admin/VendorWalletShare/BookingReconciliation?vendorId=' + $("#VendorID").val(),
					type: 'POST',
					data: { BookingReconciliationViewModel: BookingSelection },
					success: function (response) {
						if (response.success) {
							toastr.options = {
								"positionClass": "toast-bottom-right",
							};

							toastr.success(response.message);
							
							$.each(BookingSelection, function (k, v) {
								TableUnreconciledBookings.row($(`chk${v.BookingID}`).closest('tr')).remove().draw();
							});

						} else {
							toastr.error(response.message);
						}

						$(element).removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
						$(element).find('i').show();
					}
				});
			} else {
				toastr.error("Please select booking first!");

				$(element).removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
				$(element).find('i').show();
			}
		}
	});
}

function AddTableUnreconciledBookingRow(row) {
	TableUnreconciledBookings.row.add([
		`<label class="checkbox checkbox-lg checkbox-inline">
			<input type="checkbox" value="${row.ID}" id="chk${row.ID}" name="chkBooking" onchange="BookingSelected(this,${row.ID})">
			<span></span>
		</label>`,
		row.Date,
		row.CustomerImage + "|" + row.CustomerName + "|" + row.CustomerAddress,
		row.OrderNo + "|" + row.Status,
		row.CarName,
		row.TotalAmount,
	]).draw(true);
}

function AddTableWalletShareHistoryRow(row) {
	TableWalletShareHistory.row.add([
		row.CreatedOn,
		row.Description,
		row.Amount,
		row.Type,
		row.ID,
	]).draw(true);
}

function GetURLParameter() {
	var sPageURL = window.location.href;
	var indexOfLastSlash = sPageURL.lastIndexOf("/");

	if (indexOfLastSlash > 0 && sPageURL.length - 1 != indexOfLastSlash)
		return sPageURL.substring(indexOfLastSlash + 1);
	else
		return 0;
}

Array.prototype.sum = function (prop) {
	var total = 0
	for (var i = 0, _len = this.length; i < _len; i++) {
		total += this[i][prop]
	}
	return total
}

Date.prototype.addDays = function (days) {
	var date = new Date(this.valueOf());
	date.setDate(date.getDate() + days);
	return date;
}