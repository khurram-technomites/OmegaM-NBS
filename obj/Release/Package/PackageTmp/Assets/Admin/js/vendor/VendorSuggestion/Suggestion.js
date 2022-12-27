"use strict";
var Table;
var PropertySelection = [];


$(document).ready(function () {
    BindDataTable();

   /* $("#checkAll").change(function () {
        if (this.checked) {
            PropertySelection = []
            $('tbody input[type=checkbox]').each(function () {
                this.checked = true
                PropertySelection.push(this.value);
            });
            $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Send For Approval (${PropertySelection.length})`);
        } else {
            $('tbody input[type=checkbox]').each(function () {
                this.checked = false
                var index = PropertySelection.indexOf(this.value)
                PropertySelection.splice(index, 1)
            });
            $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Send For Approval (${PropertySelection.length})`);
        }
    });*/
})


function BindDataTable() {

    Table = $('#kt_datatable1').DataTable({
        "filter": true, 
        "initComplete": function (settings, json) {
            $('#kt_datatable1 tbody').fadeIn();
        },
        "columns": [
            {
                "data": "ID",
                "visible": false
            },
            {
                "data": "CreatedOn",
                width: "130px"
            },
            {
                width: "2000px"
            },
            {
                "data": "Action",
                "orderable": false,
                classname : "dt-center",
                "render": function (data, type, row) {

                    var approvalID = row.ID
                   /* var status = {
                        "1": {
                            'title': 'Send For Approval',
                            'icon': 'fa-check-circle',
                            'class': ' btn-outline-success'
                        },
                        "2": {
                            'title': 'Cancel Approval Request',
                            'icon': 'fa-times-circle',
                            'class': ' btn-outline-danger'
                        },
                        "4": {
                            'title': 'Send For Approval',
                            'icon': 'fa-check-circle',
                            'class': ' btn-outline-success'
                        },
                    };*/
                    
                    var action;
                    action = '<a class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="OpenModelPopup(this,\'/Vendor/Suggestion/Edit/' + row.ID + '\',true)">' +
                        '<i class="fa fa-pen"></i>' +
                        '</a> ' +
                        '<button type="button" class="btn btn-bg-secondary btn-icon btn-sm mr-1" onclick="Delete(this,' + row.ID + ')"><i class="fa fa-trash"></i></button>';

                    return action;

                }
            }
            
        ]
    });
}

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

        $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Send For Approval (${chk})`);
    }
    else {
        $(".btn-bulk-publish").prop("disabled", true);
        $(".btnapprove").show();

        $(".btn-bulk-publish").html(`<i class="fa fa-check-circle"></i> Send For Approval (${chk})`);
    }

}
function IsApproved(element, record) {
    swal.fire({
        title: 'Sent Suggestion',
        text: "Send Your Suggestion to Admin !",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, do it!'
    }).then(function (result) {
        if (result.value) {
            $(element).find('i').hide();
            $(element).addClass('spinner spinner-left spinner-sm').attr('disabled', true);

            $.ajax({
                url: '/Vendor/Car/IsApproved/' + record,
                type: 'Get',
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        setTimeout(function () {
                            location.reload();
                        }, 1000);

                    } else {
                        //toastr.error(response.message);
                        //$(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        //$(element).find('i').show();
                        $(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        swal.fire({
                            title: 'Please complete the car information before sending it for approval!',
                            html: response.message,
                            icon: 'error',
                            //timer: 10000,
                        });
                    }
                }
            });
        } else {
            //swal("Cancelled", "Your imaginary file is safe :)", "error");
        }
    });
}

function IsApproved(element, record) {
    swal.fire({
        title: 'Property Approval',
        text: "Send Your property for Approval !",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, do it!'
    }).then(function (result) {
        if (result.value) {
            $(element).find('i').hide();
            $(element).addClass('spinner spinner-left spinner-sm').attr('disabled', true);

            $.ajax({
                url: '/Vendor/property/IsApproved/' + record,
                type: 'Get',
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        setTimeout(function () {
                            location.reload();
                        }, 1000);

                    } else {
                        //toastr.error(response.message);
                        //$(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        //$(element).find('i').show();
                        $(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        swal.fire({
                            title: 'Please complete the property information before sending it for approval!',
                            html: response.message,
                            icon: 'error',
                            //timer: 10000,
                        });
                    }
                }
            });
        } else {
            //swal("Cancelled", "Your imaginary file is safe :)", "error");
        }
    });
}

function IsCanceled(element, record) {
    swal.fire({
        title: 'Property Approval Cancel',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, do it!'
    }).then(function (result) {
        if (result.value) {
            $(element).find('i').hide();
            $(element).addClass('spinner spinner-left spinner-sm').attr('disabled', true);

            $.ajax({
                url: '/Vendor/Property/CancelApproval/' + record,
                type: 'Get',
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        setTimeout(function () {
                            location.reload();
                        }, 1000);
                    } else {
                        toastr.error(response.message);
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
                url: '/Vendor/Suggestion/Delete/' + record,
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
                            toastr.success('Suggestion deleted successfully');

                            Table.row($(element).closest('tr')).remove().draw();
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

function BulkApproval(element, record) {

    swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, do it!'
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
                    url: '/Vendor/Property/BulkSendForApproval',
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
                                title: 'Please complete the property information before sending it for approval!',
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

function callback(dialog, elem, isedit, response) {

    if (response.success) {
        toastr.success(response.message);

        if (isedit) {
            Table.row($(elem).closest('tr')).remove().draw();
        }

        location.reload();
        jQuery('form', dialog).closest('.modal').find('button[type=submit]').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
        jQuery('#myModal').modal('hide');
    }
    else {
        jQuery('form', dialog).closest('.modal').find('button[type=submit]').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);

        toastr.error(response.message);
    }
}

function addRow(row) {
    console.log("Data of row", row);
    Table.row.add([
        row.ID,
        row.suggestion,
        '<td data-order=' + row.ID + '> ' + row.createdOn + '</td>',
        ''
    ]).draw(true);

}


