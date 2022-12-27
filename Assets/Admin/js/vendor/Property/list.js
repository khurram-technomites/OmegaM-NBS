"use strict";
var table1;
var PropertySelection = [];

$(document).ready(function () {
    KTDatatablesBasicScrollable.init();
    $(document).on('change', '#checkAll', function () {
        if (this.checked) {
            PropertySelection = []
            $('tbody input[name=chkCar]').each(function () {
                this.checked = true
                PropertySelection.push(this.value);
            });
            $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Publish (${PropertySelection.length})`);
        } else {
            $('tbody input[type=checkbox]').each(function () {
                this.checked = false
                var index = PropertySelection.indexOf(this.value)
                PropertySelection.splice(index, 1)
            });
            $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Publish (${PropertySelection.length})`);
        }
    });

    //$("#checkAll").change(function () {
    //    if (this.checked) {
    //        PropertySelection = []
    //        $('tbody input[name=chkCar]').each(function () {
    //            this.checked = true
    //            PropertySelection.push(this.value);
    //        });
    //        $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Publish (${PropertySelection.length})`);
    //    } else {
    //        $('tbody input[type=checkbox]').each(function () {
    //            this.checked = false
    //            var index = PropertySelection.indexOf(this.value)
    //            PropertySelection.splice(index, 1)
    //        });
    //        $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Publish (${PropertySelection.length})`);
    //    }
    //});
})

var KTDatatablesBasicScrollable = function () {
    var initTable1 = function () {
        var table = $('#kt_datatable1');
        var hidden = "hidden"
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
                url: "/Vendor/Property/List",
                type: 'POST',
                data: function (d) {

                    d.filter = $('#filterdropdown').val();
                },
            },
            "createdRow": function (row, data, index) {
                if (data.IsActive == false) {
                    $(row).css("background-color", "lightgray");
                }
            },
            "columns": [
                {
                    "data": "ID",
                    visible: false,
                    className: "dt-center",
                    width: '1px',

                },
                
                {
                    "mData": null,
                    "bSortable": false,
                    width: '45px',
                    "mRender": function (o) {

                        var Published = o.IsPublished ? o.IsPublished.toString().toUpperCase() : "FALSE";
                        if (Published == "FALSE") {
                            if (typeof (PropertySelection.find(x => x == o.ID.toString())) == "string") {
                                return `<label class="checkbox checkbox-lg checkbox-inline">
										<input type="checkbox" checked value="${o.ID}" id="chk${o.ID}" name="chkCar" onchange="PropertySelected(this,${o.ID})">
										<span></span>
									</label>`;
                            } else {
                                return `<label class="checkbox checkbox-lg checkbox-inline">
										<input type="checkbox" value="${o.ID}" id="chk${o.ID}" name="chkCar" onchange="PropertySelected(this,${o.ID})">
										<span></span>
									</label>`;
                            }
                        } else {
                            return '';
                        }
                    }
                },
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
                        var Verified = o.IsVerified == "True" ? "Verified" : "";
                        var Premium = o.IsPremium == "True" ? "Premium" : "";
                        var Verifiedclass = o.IsVerified == "True" ? "" : "hidden";
                        var Premiumclass = o.IsPremium == "True" ? "" : "hidden";
                        if (o.IsFeatured == "True") {
                            return '<div class="d-flex align-items-center">' +
                                '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                                '<div class="symbol-label" style="background-image: url(\'' + o.Thumbnail + '\')"></div>' +
                                '</div>' +
                                '<div class="car-name" title="' + o.Title + '">' +
                                `<div class="preview" onclick="CopyToClipboard(this,'${o.Slug}')"><i class="fa fa-link"></i> | Copy URL to Clipboard</div></br>` +
                                '<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + o.Title + '</a>' +
                                '<span class="text-muted font-weight-bold d-block sku">' + o.AdsReferenceCode + `<small>(Call Clicks: ${o.CallCount ? o.CallCount : 0})</small>` + '</span>' +
                                '<span class="label label-lg font-weight-bold label-light-info label-inline">Featured</span>' +
                                '<span ' + Verifiedclass + ' class="label label-lg font-weight-bold label-light-info label-inline">' + Verified + '</span>' +
                                '<span ' + Premiumclass + ' class="label label-lg font-weight-bold label-light-info label-inline">' + Premium + '</span>' +
                                '</div>' +
                                '</div>'
                        } else {
                            return '<div class="d-flex align-items-center">' +
                                '<div class="symbol symbol-50 flex-shrink-0 mr-4">' +
                                '<div class="symbol-label" style="background-image: url(\'' + o.Thumbnail + '\')"></div>' +
                                '</div>' +
                                '<div class="car-name" title="' + o.Title + '">' +
                                `<div class="preview" onclick="CopyToClipboard(this,'${o.Slug}')"><i class="fa fa-link"></i> | Copy URL to Clipboard</div></br>` +
                                '<a href="javascript:;" class="text-dark-75 font-weight-bolder text-hover-primary mb-1 font-size-lg">' + o.Title + '</a>' +
                                '<span class="text-muted font-weight-bold d-block sku">' + o.AdsReferenceCode + `<small>(Call Clicks: ${o.CallCount ? o.CallCount : 0})</small>` + '</span>' +
                                '<span ' + Verifiedclass + ' class="label label-lg font-weight-bold label-light-info label-inline">' + Verified + '</span>' +
                                '<span ' + Premiumclass + ' class="label label-lg font-weight-bold label-light-info label-inline">' + Premium + '</span>' +
                                '</div>' +
                                '</div>'
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
                        hidden = o.IsActive == true ? "" : "hidden";
                        if (o.IsSold) {
                            return "<label class='switch justify-content-center switch '" + hidden +" >" +
                                "<input type ='checkbox' id='toggle" + o.ID + "' checked onClick='ToggleIsSold(" + o.ID + ", " + true + ")' >" +
                                "<span class='slider round'></span>" +
                                "</label>"

                        } else {
                            return "<label class='switch justify-content-center switch'" + hidden +">" +
                                "<input type ='checkbox' id='toggle" + o.ID + "' onClick='ToggleIsSold(" + o.ID + ")' , " + false + ">" +
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
                        var published = o.IsPublished ? o.IsPublished.toString().toUpperCase() : "FALSE";
                        var status = {
                            "TRUE": {
                                'title': 'Published',
                                'class': ' label-light-success'
                            },
                            "FALSE": {
                                'title': 'Unpublished',
                                'class': ' label-light-danger'
                            },
                        };
                        if (typeof status[published] === 'undefined') {

                            return '<span class="label label-lg font-weight-bold label-light-danger label-inline">Unpublished</span>';
                        }
                        return '<span class="label label-lg font-weight-bold' + status[published].class + ' label-inline">' + status[published].title + '</span>';
                    }
                },
                {
                    "mData": null,
                    //"bSortable": true,
                    orderable: false,
                    width: '180px',
                    className: "dt-center",
                    "mRender": function (o) {
                        var isActive = o.IsPublished ? o.IsPublished.toString().toUpperCase() : "FALSE";
                        var dlthidden = o.IsActive == true ? "hidden" : "";
                        var status = {
                            "TRUE": {
                                'title': 'UnPublish',
                                'icon': 'fa-times-circle',
                                'class': ' btn-outline-danger '
                            },
                            "FALSE": {
                                'title': 'Publish',
                                'icon': 'fa-check-circle',
                                'class': ' btn-outline-success'
                            },
                        };
                        var active = o.IsActive ? o.IsActive.toString().toUpperCase() : "FALSE";
                        var Propstatus = {
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
                        actions += '<a class="btn btn-bg-secondary btn-icon btn-sm mr-1" href="/Vendor/Property/Edit/' + o.ID + '" ' + hidden +' >' +
                            '<i class="fa fa-pen"></i>' +
                            '</a> ' +
                            //'<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Vendor/Car/Details/' + o.ID + '\')" title="View">' +
                            //	'<i class="fa fa-eye"></i>' +
                            //'</button> ' +
                            '<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="Delete(this,' + o.ID + ')" ' + dlthidden +'><i class="fa fa-trash" ></i></button>';
                        if (typeof status[isActive] === 'undefined') {
                            actions += '<button type="button" class="btn btn-outline-success btn-sm mr-1" onclick="Publish(this, ' + o.ID + ')" ' + hidden +'>' +
                                '<i class="fa fa-check-circle" aria-hidden="true"></i> Publish' +
                                '</button>';
                        }
                        else {
                            actions += '<button type="button" class="btn btn-sm mr-1' + status[isActive].class + '" onclick="Publish(this, ' + o.ID + ')" ' + hidden +'>' +
                                '<i class="fa ' + status[isActive].icon + '" aria-hidden="true"></i> ' + status[isActive].title +
                                '</button>';
                        }

                        //if (typeof Propstatus[active] === 'undefined') {
                        //    actions += '<button type="button" class="btn btn-outline-success btn-sm mr-1" onclick="PropertyActivation(this, ' + row.ID + ')">' +
                        //        '<i class="fa fa-check-circle" aria-hidden="true"></i> Activate' +
                        //        '</button>';
                        //}
                        //else {
                        //    actions += '<button type="button" class="btn btn-sm mr-1' + Propstatus[active].class + '" onclick="PropertyActivation(this, ' + row.ID + ')">' +
                        //        '<i class="fa ' + Propstatus[active].icon + '" aria-hidden="true"></i> ' + Propstatus[active].title +
                        //        '</button>';
                        //}

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

function PropertySelected(element, record) {

    if (!PropertySelection) {
        PropertySelection = [];
    }

    var car = PropertySelection.find(function (obj) { return obj == $(element).val() });

    if ($(element).prop('checked')) {
        if (!car) {
            PropertySelection.push($(element).val());
        }
    } else {
        if (car) {
            PropertySelection = PropertySelection.filter(function (obj) { return obj != $(element).val() });
        }
    }

    var chk = PropertySelection.length;
    if (chk > 0) {
        $(".btnapprove").hide();
        $(".btn-bulk-publish").prop("disabled", false);

        $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Publish (${chk})`);
    }
    else {
        $(".btn-bulk-publish").prop("disabled", true);
        $(".btnapprove").show();

        $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Publish (${chk})`);
    }

}

function BulkPublish(element, record) {

    swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, publish it!'
    }).then(function (result) {
        if (result.value) {

            $(element).addClass('spinner spinner-sm spinner-left').attr('disabled', true);
            $(element).find('i').hide();

            //var SelectedCars = [];
            //$("input[name=chkCar]:checked").each(function () {
            //	SelectedCars.push($(this).val());
            //});

            if (PropertySelection.length > 0) {
                $.ajax({
                    url: '/Vendor/Property/BulkPublish',
                    type: 'POST',
                    data: { 'ids': PropertySelection },
                    success: function (response) {
                        if (response.success) {
                            toastr.options = {
                                "positionClass": "toast-bottom-right",
                            };

                            toastr.success(response.message);

                            setTimeout(function () {
                                location.reload();
                            }, 1000);
                        } else {
                            /*toastr.error(response.message);*/

                            $(element).removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
                            $(element).find('i').show();
                            swal.fire({
                                title: 'Please complete the car information before sending it for approval!',
                                html: response.message,
                                icon: 'error',
                                confirmButtonText: "Ok!",
                                //timer: 10000,
                            }).then((result) => {
                                if (result.isConfirmed) {
                                    setTimeout(function () {
                                        location.reload();
                                    }, 1000)
                                }
                            });

                        }
                    }
                });
            } else {
                $(element).removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
                $(element).find('i').show();
                toastr.error("Please select properties first!");
            }
        }
    });
}

function Publish(element, record) {
    swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, do it!'
    }).then(function (result) {
        if (result.value) {
            $(element).find('i').hide();
            $(element).addClass('spinner spinner-left spinner-sm').attr('disabled', true);

            $.ajax({
                url: '/Vendor/Property/Publish/' + record,
                type: 'Get',
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        //var tr = table1.row($(element).closest('tr'));
                        //var data = {
                        //	Date: tr.columns(0).data()[0][0]
                        //	, Name: tr.columns(1).data()[0][0]
                        //	, SKU: tr.columns(2).data()[0][0]
                        //	, Stock: tr.columns(3).data()[0][0]
                        //	, Categories: tr.columns(4).data()[0][0]
                        //	, Tags: tr.columns(5).data()[0][0]
                        //	, IsPublished: response.IsPublished
                        //	, ID: response.ID
                        //}
                        //table1.row($(element).closest('tr')).remove().draw();

                        ////table1.cell({ row: tr.index(), column: 6 }).data(response.IsPublished);

                        //addRow(data);
                        window.setTimeout(function () { location.reload() }, 2000)

                    } else {
                        //toastr.error(response.message);
                        //$(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        //$(element).find('i').show();
                        $(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        $(element).find('i').show();
                        swal.fire({
                            title: 'Please complete the property information before sending it for approval!',
                            html: response.message,
                            icon: 'error',
                            //timer: 10000,
                        });
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
                url: '/Vendor/Property/PropertyActivation/' + record,
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
function ToggleIsSold(ID, IsChecked) {
    $.ajax({
        url: "/Vendor/Property/ToggleIsSold/" + ID,
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

        if (response.messageswl) {
            swal.fire({
                title: 'Your limit has been exceeded.',
                type: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Ok'
            })

        }
        else {
            toastr.error(response.message);
        }
    }
}

function Delete(element, record) {

    swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete it!'
    }).then(function (result) {
        if (result.value) {

            $.ajax({
                url: '/Vendor/Property/Delete/' + record,
                type: 'POST',
                data: {
                    "__RequestVerificationToken":
                        $("input[name=__RequestVerificationToken]").val()
                },
                success: function (result) {
                    
                    if (result.success != undefined) {
                        if (result.success) {
                            toastr.options = {
                                "positionClass": "toast-bottom-right",
                            };
                            toastr.success('Property deleted successfully');

                            table1.row($(element).closest('tr')).remove().draw();
                        }
                        else {
                            toastr.error(result.message);
                        }
                    } else {
                        swal.fire("Your are not authorize to perform this action", "For further details please contact administrator !", "warning").then(function () {
                        });
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
        }
    });
}



function CopyToClipboard(elem, Slug) {

    navigator.clipboard.writeText(`Hey! Another property found for you. Kindly share your reviews https://nowbuysell.com/property/${Slug}`);

    $(elem).html(`URL Copied to Clipboard`);

    setTimeout(function () {
        $(elem).html(`<i class="fa fa-link"></i> | Copy URL to Clipboard`);
    }, 5000)

}

function addRow(row) {
    table1.row.add([
        row.CreatedOn,
        row.Property,
        row.IsSold,
        row.IsPublished,
        row.IsPublished + ',' + row.ID,
    ]).draw(true);

}