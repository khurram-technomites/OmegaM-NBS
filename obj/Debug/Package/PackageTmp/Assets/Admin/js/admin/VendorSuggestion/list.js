"use strict";
var Table;


var KTDatatablesBasicScrollable = function () {

    var initTable1 = function () {
        var table = $('#kt_datatable1');

        // begin first table
        table1 = table.DataTable({
            //scrollY: '50vh',
            //scrollX: true,
            //scrollCollapse: true,
            //dom: 'Bfrtip',
            "order": [[0, "desc"]],
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
                    render: function (data, type, full, meta) {
                        return data;
                    },
                },

                {
                    targets: 1,
                    width: '270px',
                    render: function (data, type, full, meta) {

                        if (!data) {
                            return '<span>-</span>';
                        }
                        var vendor = data.split('|');
                        console.log(vendor);
                        return '<div class="d-flex align-items-center">' +
                            '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                            '<div class="symbol-label" style="background-image: url(\'' + vendor[0] + '\')"></div>' +
                            '</div>' +
                            '<div>' +
                            '<a href="#" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + vendor[1] + '</a><br>' +
                            '<a href="#" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + vendor[3] + '</a>' +
                            '<span class="text-muted font-weight-bold d-block">' + vendor[2] + '</span>' +
                            '</div>' +
                            '</div>';

                    },
                },
                {
                    targets: -1,
                    title: 'Actions',
                    orderable: false,
                    width: '100px',
                    className: "dt-center",
                    render: function (data, type, full, meta) {
                        var actions = '';

                        actions += `<button type="button" class="btn btn-bg-secondary btn-sm mr-1"  onclick="OpenModelPopup(this,'/Admin/VendorSuggestion/Details/${data}')" title="View">
                                            <i class="fa fa-folder-open"></i> Details
                                        </button> `;
                        return actions;
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

$(document).ready(function () {

    KTDatatablesBasicScrollable.init();
})









