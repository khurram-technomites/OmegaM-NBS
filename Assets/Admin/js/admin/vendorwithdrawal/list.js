"use strict";
var table1;
var KTDatatablesBasicScrollable = function () {

    var initTable1 = function () {
        var table = $('#kt_datatable123');

        // begin first table
        table1 = table.DataTable({
            //scrollY: '50vh',
            scrollX: true,
            scrollCollapse: true,
            "order": [[0, "desc"]],
            "language": {
                processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
            },
            "initComplete": function (settings, json) {
                $('#kt_datatable123 tbody').fadeIn();
            },
            columnDefs: [{
                targets: -1,
                title: 'Actions',
                orderable: false,

                render: function (data, type, full, meta) {
                    var actions = '';
                    
                    actions += '<button type="button" class="btn btn-outline-success btn-sm mr-1 btnapprove" onclick="OpenModelPopup(this,\'/Admin/VendorWalletShareHistory/AcceptRequest/' + data + '/?approvalStatus=true\',true)">' +
									'<i class="fa fa-check-circle"></i> ' +
								'</button> ' +
								'<button type="button" class="btn btn-outline-danger btn-sm mr-1 btnapprove" onclick="OpenModelPopup(this,\'/Admin/VendorWalletShareHistory/AcceptRequest/' + data + '/?approvalStatus=false\',true)">' +
									'<i class="fa fa-times-circle"></i> ' +
								'</button> ';
                    

                    return actions;
                }
            }],
                
            
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
		row.Amount,
		row.Status,
		row.ID,
    ]).draw(true);

}