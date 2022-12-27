"use strict";
var table1;
var KTDatatablesBasicScrollable = function () {

	var initTable1 = function () {
		var table = $('#kt_datatable1');

		// begin first table
		table1 = table.DataTable({
			//scrollY: '50vh',
			scrollX: true,
			"order": [[0, "desc"]],
			scrollCollapse: true,

			"language": {
				processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
			},
			"initComplete": function (settings, json) {
				$('#kt_datatable1 tbody').fadeIn();
			},

			columnDefs: [{

				targets: 2,
				//width: '75px',
				render: function (data, type, full, meta) {
					console.log(data);
					data = data;
					if (full[3] == 1) {
						return '<text ' + data + ' ">' + data + '</text>';
					}
					else if (full[3] == 2) {
						return '<text ' + data + ' >' + data + '</text>';
					}
				},
			},
                {
                	targets: 3,
                	width: '130px',
                	render: function (data, type, full, meta) {

                		data = data;
                		var status = {
                			1: {
                				'title': 'Debit',
                                'class': ' label-light-success'
                			},
                			2: {
                				'title': 'Credit',
                                'class': ' label-light-danger'
                			},
                		};
                		if (typeof status[data] === 'undefined') {

                			return '<span class="label label-lg font-weight-bold label-light-danger label-inline">Inactive</span>';
                		}
                		else if (status[data].title === 'Debit') {
                			return '<span class="label label-lg font-weight-bold' + status[data].class + ' label-inline">' + status[data].title + ' </span>';
                		}
                		else if (status[data].title === 'Credit') {
                			return '<span class="label label-lg font-weight-bold' + status[data].class + ' label-inline">' + status[data].title + ' </span>';
                		}
                	},


                },

			]
		});
	};

	return {
		//main function to initiate the module
		init: function () {
			initTable1();
		},
	};
}();
var KTDatatablesBasicScrollable1 = function () {

    var initTable1 = function () {
        var table = $('#kt_datatable1');

        // begin first table
        table1 = table.DataTable({
            //scrollY: '50vh',
            scrollX: true,
            "order": [[0, "desc"]],
            scrollCollapse: true,

            "language": {
                processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
            },
            "initComplete": function (settings, json) {
                $('#kt_datatable1 tbody').fadeIn();
            },
            dom: 'Bfrtip',
            buttons: [
                {
                    extend: 'excel',
                    messageTop: function () {
                        return '';
                    },
                    title: 'Wallet History',
                    exportOptions: {
                        columns: [0, 1, 2, 3]
                    }
                }
            ],
            columnDefs: [{

                targets: 2,
                //width: '75px',
                render: function (data, type, full, meta) {
                    console.log(data);
                    data = data;
                    if (full[3] == 1) {
                        return '<text ' + data + ' ">' + data + '</text>';
                    }
                    else if (full[3] == 2) {
                        return '<text ' + data + ' >' + data + '</text>';
                    }
                },
            },
            {
                targets: 3,
                width: '130px',
                render: function (data, type, full, meta) {

                    data = data;
                    var status = {
                        1: {
                            'title': 'Debit',
                            'class': ' label-light-success'
                        },
                        2: {
                            'title': 'Credit',
                            'class': ' label-light-danger'
                        },
                    };
                    if (typeof status[data] === 'undefined') {

                        return '<span class="label label-lg font-weight-bold label-light-danger label-inline">Inactive</span>';
                    }
                    else if (status[data].title === 'Debit') {
                        return '<span class="label label-lg font-weight-bold' + status[data].class + ' label-inline">' + status[data].title + ' </span>';
                    }
                    else if (status[data].title === 'Credit') {
                        return '<span class="label label-lg font-weight-bold' + status[data].class + ' label-inline">' + status[data].title + ' </span>';
                    }
                },


            },

            ]
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

    //$('.kt_datepicker_range').datepicker({
    //    todayHighlight: true,
    //});

    $("#fromDate").datepicker({
        todayHighlight: true,
    });

    $("#toDate").datepicker({
        todayHighlight: true,
    });

    $("#fromDate").change(function () {

        if (new Date($("#fromDate").val()) > new Date($("#toDate").val())) {
            $('#toDate').datepicker('setDate', new Date($("#fromDate").val()));
            $("#toDate").datepicker("option", "minDate", new Date($("#fromDate").val()));
        }
    });

    $("#toDate").change(function () {

        if (new Date($("#fromDate").val()) > new Date($("#toDate").val())) {
            $('#fromDate').datepicker('setDate', new Date($("#toDate").val()));
            $("#fromDate").datepicker("option", "maxDate", new Date($("#toDate").val()));
        }
    });



    $("#btnSearch").on("click", function () {
        
        var fromDate = $('#fromDate').val();
        var toDate = $('#toDate').val();

        if (fromDate == "" && toDate == "") {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Please! Select Date',
            })
        }
        else if (fromDate == "") {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Please! Select From Date',
            })
        }
        else if (toDate == "") {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Please! Select To Date',
            })
        }

        $.ajax({
            url: '/Vendor/VendorWalletHistory/VendorHistor',
            type: 'POST',
            data: {
                fromDate: $('#fromDate').val(),
                toDate: $('#toDate').val(),
            },
            success: function (data) {
                "use strict";

                if (data != null) {
                    
                    $("#VendorHistory").html(data);
                    KTDatatablesBasicScrollable1.init();
                }
            }
        });

    })//btnSearch;
});

//function ChangeStatus(element, record) {
//    swal.fire({
//        title: 'Are you sure?',
//        text: "You won't be able to revert this!",
//        type: 'warning',
//        showCancelButton: true,
//        confirmButtonText: 'Yes, do it!'
//    }).then(function (result) {
//        if (result.value) {
//            $(element).find('i').hide();
//            $(element).addClass('spinner spinner-left spinner-sm').attr('disabled', true);

//            $.ajax({
//                url: '/Admin/Order/Status/' + record,
//                type: 'POST',
//                data: JSON.stringify({ status: $(element).text().trim() }),
//                success: function (response) {
//                    if (response.success) {
//                        toastr.success(response.message);
//                        table1.row($(element).closest('tr')).remove().draw();
//                        addRow(response.data);
//                    } else {
//                        toastr.error(response.message);
//                        $(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
//                        $(element).find('i').show();
//                    }
//                }
//            });
//        } else {
//            //swal("Cancelled", "Your imaginary file is safe :)", "error");
//        }
//    });
//}

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
		row.CreatedOn,
		row.Description,
		row.Amount,
		row.Type,

	]).draw(true);

}