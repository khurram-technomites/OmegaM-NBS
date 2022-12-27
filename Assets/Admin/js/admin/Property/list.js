"use strict";
var table1;
var PropertySelection = [];


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
				if (json.recordsTotal <= 0) {
					$('.excel-btn').prop('disabled', true);
				}
				else {
					$('.excel-btn').prop('disabled', false);
				}
			},
			lengthMenu: [
				[10, 25, 100, 500, -1],
				['10', '25', '100', '500', 'Show all']
			],
			"proccessing": true,
			"serverSide": true,
			"ajax": {
				url: "/Admin/Property/List",
				type: 'POST',
				data: function (d) {

					d.filter = $('#filterdropdown').val();
					d.vendorid = $('#vendordropdown').val();
				},
			},
			"columns": [
				{
					"data": "CreatedOn",
					className: "dt-center",
					width: '130px',

				},
				{
					"mData": null,
					"bSortable": true,
					/*className: "dt-center",*/
					width: '250px',
					"mRender": function (o) {
						var Soldclass = o.IsSold == true ? "" : "hidden";
						var ForSale = o.ForSale == true ? "Sale" : "Rent";
						var Address = o.Address == null ? "" : o.Address;
						
						if (o.IsPublished == true) {
							return '<div class="d-flex align-items-center">' +
								'<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
								'<div class="symbol-label" style="background-image: url(\'' + o.Thumbnail + '\')"></div>' +
								/*`<span class="sold">Sold</span>`+*/
								'</div>' +
								'<div class="car-name" title="' + o.Title + '">' +
								'<span  class="label label-lg font-weight-bold label-light-info label-inline">' + ForSale + '</span>' +
								'<span ' + Soldclass + ' class="label label-lg font-weight-bold label-light-info label-inline"style="margin: 2px;">Sold</span>' +
								'<span  class="label label-lg font-weight-bold label-light-success  label-inline"style="margin: 2px;">Published</span></br>' +
								'<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + o.Title + '</a>' +
								'<span class="text-muted font-weight-bold d-block">' + Address + '</span>' +
								'<a  href="javascript:;" class="text-muted font-weight-bold d-block vendorname">' + o.VendorName + '</a>' +
								/* '<span class="text-muted font-weight-bold d-block">' + split[4] + '</span>' +*/
								'</div>' +
								'</div>'
						}
						else {
							return '<div class="d-flex align-items-center">' +
								'<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
								'<div class="symbol-label" style="background-image: url(\'' + o.Thumbnail + '\')"></div>' +
								/*`<span class="sold">Sold</span>`+*/
								'</div>' +
								'<div class="car-name" title="' + o.Title + '">' +
								'<span  class="label label-lg font-weight-bold label-light-info label-inline">' + ForSale + '</span>' +
								'<span  class="label label-lg font-weight-bold label-light-danger label-inline"style="margin: 2px;">UnPublished</span>' +
								'<span ' + Soldclass + ' class="label label-lg font-weight-bold label-light-info label-inline"style="margin: 2px;">Sold</span></br>'+
								'<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + o.Title + '</a>' +
								'<span class="text-muted font-weight-bold d-block">' + Address + '</span>' +
								'<a  href="javascript:;" class="text-muted font-weight-bold d-block vendorname">' + o.VendorName + '</a>' +
								/* '<span class="text-muted font-weight-bold d-block">' + split[4] + '</span>' +*/
								'</div>' +
								'</div>'
						}

					}
				},
				{
					"mData": null,
					//"bSortable": true,
					orderable: false,
					width: '60px',
					className: "dt-center",
					"mRender": function (o) {
						if (o.IsFeatured) {
							return "<label class='switch'>" +
								"<input type ='checkbox' id='toggle" + o.ID + "' checked onClick='ToggleFeature(" + o.ID + ", " + true + ")'>" +
								"<span class='slider round'></span>" +
								"</label>"
						} else {
							return "<label class='switch'>" +
								"<input type ='checkbox' id='toggle" + o.ID + "' onClick='ToggleFeature(" + o.ID + ")' , " + false + ">" +
								"<span class='slider round'></span>" +
								"</label>"
						}
					}
				},



				{
					"mData": null,
					"bSortable": true,
					orderable: false,
					width: '60px',
					className: "dt-center",
					"mRender": function (o) {

						if (o.IsVerified) {
							return "<label class='switch'>" +
								"<input type ='checkbox' id='toggle" + o.ID + "' checked onClick='ToggleVerify(" + o.ID + ", " + true + ")'>" +
								"<span class='slider round'></span>" +
								"</label>"
						} else {
							return "<label class='switch'>" +
								"<input type ='checkbox' id='toggle" + o.ID + "' onClick='ToggleVerify(" + o.ID + ")' , " + false + ">" +
								"<span class='slider round'></span>" +
								"</label>"
						}
					}
				},
				{
					"mData": null,
					"bSortable": true,
					orderable: false,
					width: '60px',
					className: "dt-center",
					"mRender": function (o) {

						if (o.IsPremium) {
							return "<label class='switch'>" +
								"<input type ='checkbox' id='toggle" + o.ID + "' checked onClick='TogglePremium(" + o.ID + ", " + true + ")'>" +
								"<span class='slider round'></span>" +
								"</label>"
						} else {
							return "<label class='switch'>" +
								"<input type ='checkbox' id='toggle" + o.ID + "' onClick='TogglePremium(" + o.ID + ")' , " + false + ">" +
								"<span class='slider round'></span>" +
								"</label>"
						}
					}
				},
				{
					"mData": null,
					"bSortable": true,
					className: "dt-center",
					width: '75px',
					"mRender": function (o) {

						var data = o.IsActive.toString().toUpperCase();
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
					}
				},
				{
					"mData": null,
					//"bSortable": true,
					orderable: false,
					width: '180px',
					className: "dt-center",
					"mRender": function (o) {
						var isActive = o.IsActive.toString().toUpperCase();
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
						actions += '<a class="btn btn-bg-secondary btn-icon btn-sm mr-1" href="/Admin/Property/Edit/' + o.ID + '">' +
							'<i class="fa fa-pen"></i>' +
							'</a> ' +
							'<a  class="btn btn-bg-secondary btn-icon btn-sm mr-1" href="/Admin/Property/Details/' + o.ID + '" title="View">' +
							'<i class="fa fa-eye"></i> ' +
							'</a> ';

						if (typeof status[isActive] === 'undefined') {
							actions += '<button type="button" class="btn btn-outline-success btn-sm mr-1" onclick="PropertyActivation(this, ' + o.ID + ')">' +
								'<i class="fa fa-check-circle" aria-hidden="true"></i> Activate' +
								'</button>';
						} else {
							actions += '<button type="button" class="btn btn-sm mr-1' + status[isActive].class + '" onclick="PropertyActivation(this, ' + o.ID + ')">' +
								'<i class="fa ' + status[isActive].icon + '" aria-hidden="true"></i> ' + status[isActive].title +
								'</button>';
						}

						return actions;
					}
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
$(document).ready(function () {
	$('#vendordropdown').val("")
	KTDatatablesBasicScrollable.init();
	$(document).on('click', '.vendorname', function () {

		$('#vendordropdown option:contains(' + this.text + ')').attr("selected", "selected");
		Filterlist()
	})
})


function Search(element) {
	table1.search($(element).text().trim()).draw();
}

function PropertyActivation(element, record) {
	swal.fire({
		title: 'Are you sure?',
		text: "You want to change the property status!",
		type: 'warning',
		showCancelButton: true,
		confirmButtonText: 'Yes, do it!'
	}).then(function (result) {
		if (result.value) {
			$(element).find('i').hide();
			$(element).addClass('spinner spinner-left spinner-sm').attr('disabled', true);

			$.ajax({
				url: '/Admin/Property/PropertyActivation/' + record,
				type: 'Get',
				success: function (response) {
					if (response.success) {

						toastr.success(response.message);

						window.setTimeout(function () { location.reload() }, 2000)

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



function ToggleFeature(ID, IsChecked) {
	$.ajax({
		url: "ToggleFeatured/" + ID,
		type: 'POST',
		success: function (response) {
			if (response.success) {
				toastr.success(response.message);
			} else {
				toastr.error(response.message);
				if (IsChecked) {
					$("#toggle" + ID).prop('checked', true);
				} else {
					$("#toggle" + ID).prop('checked', false);
				}
			}
		},
		error: function (request, error) {
			toastr.error("Ops! Something went wrong. Please try again later.");
			if (IsChecked) {
				$("#toggle" + ID).prop('checked', true);
			} else {
				$("#toggle" + ID).prop('checked', false);
			}
		}
	});
}

function ToggleVerify(ID, IsChecked) {
	$.ajax({
		url: "ToggleVerify/" + ID,
		type: 'POST',
		success: function (response) {
			if (response.success) {
				toastr.success(response.message);
			} else {
				toastr.error(response.message);
				if (IsChecked) {
					$("#toggle" + ID).prop('checked', true);
				} else {
					$("#toggle" + ID).prop('checked', false);
				}
			}
		},
		error: function (request, error) {
			toastr.error("Ops! Something went wrong. Please try again later.");
			if (IsChecked) {
				$("#toggle" + ID).prop('checked', true);
			} else {
				$("#toggle" + ID).prop('checked', false);
			}
		}
	});
}

function TogglePremium(ID, IsChecked) {
	$.ajax({
		url: "TogglePremium/" + ID,
		type: 'POST',
		success: function (response) {
			if (response.success) {
				toastr.success(response.message);
			} else {
				toastr.error(response.message);
				if (IsChecked) {
					$("#toggle" + ID).prop('checked', true);
				} else {
					$("#toggle" + ID).prop('checked', false);
				}
			}
		},
		error: function (request, error) {
			toastr.error("Ops! Something went wrong. Please try again later.");
			if (IsChecked) {
				$("#toggle" + ID).prop('checked', true);
			} else {
				$("#toggle" + ID).prop('checked', false);
			}
		}
	});
}

function callback(dialog, elem, isedit, response) {

	if (response.success) {
		toastr.success(response.message);

		window.location.href = response.url;

		if (isedit) {
			table1.row($(elem).closest('tr')).remove().draw();
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
		row.CreatedOn,
		row.Property,
		row.Vendor,
		row.IsActive,
		row.IsActive + ',' + row.ID,
	]).draw(true);

}