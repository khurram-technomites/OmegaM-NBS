﻿"use strict";
var table2;
var KTDatatablesBasicScrollable = function () {

    var initTable2 = function () {
        var table = $('#kt_advance_table_widget_order');

        // begin first table
        table2 = table.DataTable({
            //scrollY: '50vh',
            scrollX: true,
            scrollCollapse: true,
            "order": [[1, "desc"]],

            "language": {
                processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
            },
            "initComplete": function (settings, json) {
                FormatPrices();

                $('#kt_advance_table_widget_order tbody').fadeIn();

            },
            columnDefs: [
                {
                    targets: 0,
                    className: "dt-center",
                    width: '130px',
                },
                {
                    targets: 3,
                    className: "dt-right pr-10",
                }, {
                    targets: -1,
                    title: 'Actions',
                    orderable: false,
                    render: function (data, type, full, meta) {
                        var actions = '';
                        actions += '<a href="/Vendor/Order/Details/' + data + '" class="btn btn-bg-secondary btn-sm">'

                            + '					<span class="svg-icon svg-icon-md svg-icon-warning">'
                            + '						<!--begin::Svg Icon | path:assets/media/svg/icons/General/Settings-1.svg-->'
                            + '						<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="24px" height="24px" viewBox="0 0 24 24" version="1.1">'
                            + '							<g stroke="none" stroke-width="1" fill="none" fill-rule="evenodd">'
                            + '								<rect x="0" y="0" width="24" height="24" />'
                            + '								<path d="M7,3 L17,3 C19.209139,3 21,4.790861 21,7 C21,9.209139 19.209139,11 17,11 L7,11 C4.790861,11 3,9.209139 3,7 C3,4.790861 4.790861,3 7,3 Z M7,9 C8.1045695,9 9,8.1045695 9,7 C9,5.8954305 8.1045695,5 7,5 C5.8954305,5 5,5.8954305 5,7 C5,8.1045695 5.8954305,9 7,9 Z" fill="#000000" />'
                            + '								<path d="M7,13 L17,13 C19.209139,13 21,14.790861 21,17 C21,19.209139 19.209139,21 17,21 L7,21 C4.790861,21 3,19.209139 3,17 C3,14.790861 4.790861,13 7,13 Z M17,19 C18.1045695,19 19,18.1045695 19,17 C19,15.8954305 18.1045695,15 17,15 C15.8954305,15 15,15.8954305 15,17 C15,18.1045695 15.8954305,19 17,19 Z" fill="#000000" opacity="0.3" />'
                            + '							</g>'
                            + '						</svg>'
                            + '						<!--end::Svg Icon-->'
                            + '					</span>'
                            + '					Details'
                            + '				</a> ';
                        return actions;
                    },
                },
                {
                    targets: 2,
                    //width: '75px',
                    render: function (data, type, full, meta) {
                        if (!data) {
                            return '<span>-</span>';
                        }
                        var customer = data.split('|');
                        return '<div class="d-flex align-items-center">' +
                            '<div>' +
                            '<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + customer[0] + '</a>' +
                            '<span class="text-muted font-weight-bold d-block">' + customer[1] + '</span>' +
                            '</div>' +
                            '</div>';
                    },
                },


                {
                    targets: 4,
                    width: '75px',
                    render: function (data, type, full, meta) {

                        var status = {
                            "Pending": {
                                'title': 'Pending',
                                'class': ' label-light-dark'
                            },
                            "Confirmed": {
                                'title': 'Confirmed',
                                'class': ' label-light-success'
                            },
                            "Processing": {
                                'title': 'Processing',
                                'class': ' label-light-primary'
                            },
                            "Completed": {
                                'title': 'Completed',
                                'class': ' label-light-danger'
                            },
                            "Dispatched": {
                                'title': 'Dispatched',
                                'class': ' label-light-danger'
                            },
                            "Delivered": {
                                'title': 'Delivered',
                                'class': ' label-light-success'
                            },
                            "Returned": {
                                'title': 'Returned',
                                'class': ' label-light-success'
                            },
                            "Canceled": {
                                'title': 'Canceled',
                                'class': ' label-light-success'
                            },
                        };
                        if (typeof status[data] === 'undefined') {
                            return '<a  href="javascript:" class="label label-lg label-light-dark label-inline">' + data + '</a>';
                        }
                        return '<a href="javascript:" class="label label-lg ' + status[data].class + ' label-inline" >' + data + ' </a>';

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
var KTDatatablesBasicScrollableCustomer = function () {
    var initTable2 = function () {
        var table = $('#kt_advance_table_widget_customer');
        // begin first table
        table2 = table.DataTable({
            //scrollY: '50vh',
            scrollX: true,
            scrollCollapse: true,
            "order": [[1, "desc"]],

            "language": {
                processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
            },
            "initComplete": function (settings, json) {
                FormatPrices();

                $('#kt_advance_table_widget_customer tbody').fadeIn();

            },
            columnDefs: [
                {
                    targets: 1,
                    className: "dt-center",
                },
                {
                    targets: 2,
                    className: "dt-right pr-10",
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
var KTDatatablesBasicScrollableCategory = function () {
    var initTable2 = function () {
        var table = $('#kt_advance_table_widget_categories');
        // begin first table
        table2 = table.DataTable({
            //scrollY: '50vh',
            scrollX: true,
            scrollCollapse: true,
            "order": [[1, "desc"]],

            "language": {
                processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
            },
            "initComplete": function (settings, json) {
                FormatPrices();

                $('#kt_advance_table_widget_categories tbody').fadeIn();

            },
            columnDefs: [
                {
                    targets: 1,
                    className: "dt-center",
                },
                {
                    targets: 2,
                    className: "dt-right pr-10",
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
var KTDatatablesBasicScrollableCar = function () {
    var initTable2 = function () {
        var table = $('#kt_advance_table_widget_cars');
        // begin first table
        table2 = table.DataTable({
            //scrollY: '50vh',
            scrollX: true,
            scrollCollapse: true,
            "order": [[1, "desc"]],

            "language": {
                processing: '<i class="spinner spinner-left spinner-dark spinner-sm"></i>'
            },
            "initComplete": function (settings, json) {
                FormatPrices();

                $('#kt_advance_table_widget_cars tbody').fadeIn();

            },
            columnDefs: [
                {
                    targets: 1,
                    className: "dt-center",
                },
                {
                    targets: 2,
                    className: "dt-right pr-10",
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
    KTDatatablesBasicScrollable.init();
    KTDatatablesBasicScrollableCustomer.init();
    KTDatatablesBasicScrollableCategory.init();
    KTDatatablesBasicScrollableCar.init();

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

	//$('.kt_datepicker_range').datepicker({
	//    todayHighlight: true,
	//});

    var fromDate = $('#fromDate').val();
    var toDate = $('#toDate').val();

    $('#from').val(fromDate);
    $('#to').val(toDate);
    $('.FromDate').val($('#fromDate').val());
    $('.ToDate').val($('#toDate').val());

    filterFunction();

    $("#btnSearch").on("click", function () {
        $("#btnSearch").addClass('spinner spinner-left spinner-sm').attr('disabled', true);

        var fromDate = $('#fromDate').val();
        var toDate = $('#toDate').val();

        if (fromDate == "" && toDate == "") {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Please! Select Date',
            });
        }
        else if (fromDate == "") {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Please! Select From Date',
            });
        }
        else if (toDate == "") {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Please! Select To Date',
            });
        }
        $('.FromDate').val($('#fromDate').val());
        $('.ToDate').val($('#toDate').val());
        filterFunction();
    });
});

function filterFunction() {
    $.ajax({
        url: '/Vendor/Dashboard/Filter',
        type: 'POST',
        data: {
            sd: $('#fromDate').val(),
            ed: $('#toDate').val(),
        },
        success: function (response) {
            "use strict";
            Sales = [];
            category = [];
            Dates = [];
            if (response.data != null) {
                
                //$(".topVendor tbody").empty();
                $(".topCategory tbody").empty();
                $(".topCustomer tbody").empty();
                //$("#kt_charts_widget_2_chart12").empty();
                //$("#myDoughnutChart").empty();

                $(".ts").text("AED " + response.data.StatusByRange.TotalSale.toFixed(2).toString());
                $('.ns').text("AED " + response.data.StatusByRange.NetSale.toFixed(2).toString());
                $('.comord').text(response.data.StatusByRange.CompletedOrders.toString());
                $('.co').text(response.data.StatusByRange.CanceledOrders.toString());
                $('.af').text(response.data.StatusByRange.ActiveFeatures.toString());
                $('.po').text(response.data.StatusByRange.PendingOrders.toString());
                $('.ta').text("AED " + response.data.StatusByRange.TransferedAmountWallet.toFixed(2).toString());
                $('.pam').text("AED " + response.data.StatusByRange.PendingAmountWallet.toFixed(2).toString());
                //$('.nou').text(response.data.StatusByRange.NoOfUsers.toString());
                //$('.acustomer').text(response.data.StatusByRange.ActiveCustomers.toString());
                //$('.av').text(response.data.StatusByRange.ActiveVendors.toString());
                $('.ac').text(response.data.StatusByRange.ActiveCars.toString());
                $('.car-ap').text(response.data.StatusByRange.CarApprovals.toString());
                //nsChart(response.data.GetAdminDashboardCharts);
                //oChart(response.data.GetAdminDashboardCharts);
                //aovChart(response.data.GetAdminDashboardCharts);
                //isChart(response.data.GetAdminDashboardChartsForItemsSold);
            	//retChart(response.data.GetAdminDashboardChartForReturn);

                //$.each(response.data.GetNetSalesChartValues, function (k, v) {
                //    Sales.push(v.NetSale);
                //    Dates.push(v.Date);
                //});

                $.each(response.data.TopCategories, function (k, v) {
                    category.push(v.Category);
                    itemsold.push(v.ItemsSold);
                });


                if (response.data.TopCategories.length == 0) {
                    $(".topCategory tbody").append(`<tr>
															<td style="width: 100%;text-align: center;color: #3f5454a8;">
																No data available

																
															</td>
														</tr>`);
                }

                else {
                    $.each(response.data.TopCategories, function (k, v) {
                        $(".topCategory tbody").append(`<tr>
															<td class="pl-0 pr-0">
																<div class ="symbol symbol-circle symbol-50 symbol-light mr-2">
																<div class ="symbol-label" style="background-image: url('${v.Logo}')"></div>
																</div>
															</td>
															<td>
																<div class="d-flex flex-column w-100 mr-2">
																	<div class="d-flex align-items-center justify-content-between">
																		<span class ="text-muted mr-2 font-size-sm font-weight-bold">${v.Category}</span>

																	</div>
																	<div class ="progress progress-xs progress-md w-100">
																		<div class ="progress-bar bg-danger" role="progressbar" style="width: ${v.percentage}%;" aria-valuenow="${v.percentage}" aria-valuemin="0" aria-valuemax="100"></div>
																		<span class ="spn-items-sold" >${v.ItemsSold}</span>
																	</div>
																</div>
															</td>
														</tr>`);
                    });
                }

                if (response.data.TopCustomers.length == 0) {
                    $("")
                    $(".topCustomer tbody").append(`<tr>
														
															<td colspan=4 style="    text-align: center;">
																No data available

																
															</td>
														</tr>`);
                }
                else {
                    $.each(response.data.TopCustomers, function (k, v) {
                        $(".topCustomer tbody").append(`<tr>
															<td class="pr-0">
																<span class ="text-muted font-size-sm font-weight-bold">#${k + 1}</span>
															</td>
															<td class="pl-0 py-5 w-50px">
																<div class ="symbol symbol-circle symbol-50 symbol-light mr-2" style="padding-top: 4px;">
																	<div class ="symbol-label" style="background-image: url('${v.Logo}')"></div>
																</div>
															</td>
															<td class="pl-0">
																<a href="javascript:;" class ="text-dark font-weight-bolder mb-1 font-size-lg" style="color:#2D2D2D">${v.Name}</a>
															</td>
															<td class="text-center pr-0">
																<span class ="text-muted font-size-sm font-weight-bold">${v.Orders}</span>
															</td>
														</tr>`);
                    });
				}

                

                //MyChart(Sales, Dates);

                $("#btnSearch").removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
            }
        }
    });

    $.ajax({
    	url: '/Vendor/Dashboard/OnGoingOrders',
        type: 'POST',
        data: {
            fromDate: $('#fromDate').val(),
            toDate: $('#toDate').val(),
        },
        success: function (data) {
            "use strict";

            if (data != null) {
                $("#TopList").html(data);
                //KTDatatablesBasicScrollableCustomer.init();
                //KTDatatablesBasicScrollableCategory.init();
                //KTDatatablesBasicScrollableCar.init();

                $('.FromDate').val($('#fromDate').val());
                $('.ToDate').val($('#toDate').val());

                KTDatatablesBasicScrollableer.init();

                $("#btnSearch").removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                $("#btnSearch").find('i').show();

                var hdnOrderCounter = Number($('#hdnOrderCounter').val());
                $('#OrderCounter').text(hdnOrderCounter > 1 ? hdnOrderCounter + " Bookings" : hdnOrderCounter + " Booking");
            }
            else {
            }
        }
    });
}
