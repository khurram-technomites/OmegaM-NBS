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
				
					return '<button type="button" disabled class="btn btn-secondary  btn-sm mr-1 btnSave"   onclick="EditStock(this, ' + data + ')" title="View">' +
						'<i class="far fa-save"></i> Save' +
						'</button> ';
				},
			}
			//{
			//	targets: 4,
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

$("input").change(function () {
	$("#btnUpdate").prop('disabled', false);
	$($(this).closest('tr').addClass("updated"));
	$($(this).closest('tr').children()[5]).children().prop('disabled', false);
	var saleprice = parseFloat($($($($(this).closest('tr').children()[3])[0]).children()).val());
	var regularprice = parseFloat($($($($(this).closest('tr').children()[2])[0]).children()).val());
	if (saleprice > regularprice) {
		Swal.fire({
			icon: 'error',
			title: 'Oops...',
			text: 'Sale Price is greater than Regular Price!',
		});
		$($($($(this).closest('tr').children()[3])[0]).children()).val(regularprice);
	}

});

$("#btnUpdate").click(function () {

	$(".btnSave:enabled").each(function (i, j) {
		EditStock(j, $(j).closest('tr').attr('id'));
	});

	//$('.btnSave').prop('disabled',false).each(function (i, j) {
	//	EditStock(j, $(this).closest('tr').attr('id'));
	//});
	
})

//$(".SalePrice").change(function () {
//	($($(".SalePrice").closest('tr').children()[5])).children().prop('disabled', false);
//	var saleprice = $(".SalePrice").val();
//	var regularprice = $(".RegularPrice").val();
//	if (saleprice < regularprice) {
//		Swal.fire({
//			icon: 'error',
//			title: 'Oops...',
//			text: 'Sale Price is less than Regular Price!',
		
//		});
//		//$(".SalePrice").val("");
//	}
//});
//$(".RegularPrice").change(function () {
//	($($(".RegularPrice").closest('tr').children()[5])).children().prop('disabled', false);
//	var saleprice = $(".SalePrice").val();
//	var regularprice = $(".RegularPrice").val();
//	if (regularprice > saleprice) {
//		Swal.fire({
//			icon: 'error',
//			title: 'Oops...',
//			text: 'Regular Price is less than Sale Price!',
			
//		});
//		//$(".RegularPrice").val("");
//	}
//});


function EditStock(element, record) {
	
	var data = {
		ID: record,
		CreatedOn: $($("#" + record).children()[0]).text(),
		Name: $($("#" + record).children()[1]).text(),
		RegularPrice: $($($("#" + record).children()[2]).children()).val(),
		SalePrice: $($($("#" + record).children()[3]).children()).val(),
		Stock: $($($("#" + record).children()[4]).children()).val()
	}
	$.ajax({
		type: "POST",
		url: '/Vendor/StockEdit/EditStock',
		async: true,
		data: data,
		success: function (data) {
			console.log(data);
			if (data.success) {
				
				toastr.success(data.message);
				$($("#" + data.data.ID).closest('tr').children()[5]).children().prop('disabled', true);
				//$("#btnUpdate").prop('disabled', true);
				//table1.row($(element).closest('tr')).remove().draw();
				//addRow(data.data);
			} else {
				toastr.error(data.message);
				$(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
				$(element).find('i').show();
			}
		},
		error: function (err) { console.log(err); }
	});

	//swal.fire({
	//	title: 'Save Stock?',
	//	text: "Save Changes!",
	//	type: 'warning',
	//	showCancelButton: true,
	//	confirmButtonText: 'Yes, do it!'
	//}).then(function (result) {
	//	if (result.value) {
	//		$(element).find('i').hide();
	//		$(element).addClass('spinner spinner-left spinner-sm').attr('disabled', true);

	//		$.ajax({
	//			url: '/Vendor/StockEdit/EditStock/',
	//			type: 'POST',
	//			data: data,
	//			success: function (response) {
	//				if (response.success) {
	//					toastr.success(response.message);
	//					table1.row($(element).closest('tr')).remove().draw();
	//					addRow(response.data);
	//				} else {
	//					toastr.error(response.message);
	//					$(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
	//					$(element).find('i').show();
	//				}
	//			},
	//			error: function (xhr, ajaxOptions, thrownError) {
	//				if (xhr.status == 403) {
	//					var response = $.parseJSON(xhr.responseText);
	//					swal.fire(response.Error, response.Message, "warning").then(function () {
	//						$('#myModal').modal('hide');
	//					});

	//					$(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
	//					$(element).find('i').show();

	//				}
	//			}
	//		});
	//	} else {
	//		//swal("Cancelled", "Your imaginary file is safe :)", "error");
	//	}
	//});
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
		row.Name + '|' + row.SKU,
		row.RegularPrice,
		row.SalePrice,
		row.Stock,
		row.ID,
	]).draw(true);

}