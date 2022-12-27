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
                     targets: -1,
                     title: 'Actions',
                     orderable: false,
                     width: '230px',
                     className: "dt-center",
                     render: function (data, type, full, meta) {

                         data = data.split(',');
                         var isActive = data[0].toUpperCase();
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
                     },
                 },
                  {
                      targets: 2,
                      className: "dt-center",
                      width: '130px',
                  },
                   {
                       targets: 3,
                       className: "dt-center",
                       width: '130px',
                   },


            ],
            
            dom: `<'row'<'col-sm-6 text-left'f><'col-sm-6 text-right'B>>
			<'row'<'col-sm-12'tr>>
			<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7 dataTables_pager'lp>>`,

            buttons: [
				'print',
                'csvHtml5',
				
            ],
            // Pagination settings

           
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
