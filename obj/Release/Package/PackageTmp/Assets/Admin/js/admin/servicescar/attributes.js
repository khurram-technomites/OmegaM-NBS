"use strict";

jQuery(document).ready(function () {
    //KTFormRepeater.init();
});

// Class definition
var KTFormRepeater = function () {

    // Private functions
    var addList = function () {
        $('#kt_repeater_1').repeater({
            initEmpty: false,
            defaultValues: {
                'text-input': 'foo'
            },
            show: function () {
                
                $(this).find(".ID").attr("value", "");
                $(this).find(".Name").attr("value", "");
                $(this).find(".Value").attr("value", "");
                $(this).find(".IsActive").attr("disabled", true).removeClass("btn-outline-success").addClass("btn-outline-danger").html('<i class="fa fa-times-circle"></i>Deactivate');
                $(this).find(".Delete").attr("disabled", true);
                $(this).slideDown();
            },
        });
    }

    return {
        // public functions
        init: function () {
            addList();
        }
    };
}();

function AddAttribue(element) {
    var row = $(element).closest(".list-repeat");

    if (row.find(".Name").val() == "") {
        Swal.fire(
            'Oops!',
            'Name required ...',
            'error'
        )
    }
    else if (row.find(".Value").val() == "") {
        Swal.fire(
            'Oops!',
            'Value required ...',
            'error'
        )
    }
    else {
        let ID = row.find(".ID").val();
        let ProductID = row.find(".ProductID").val();
        let Name = row.find(".Name").val();
        let Value = row.find(".Value").val();

        var data = new FormData();
        data.append("id", ID);
        data.append("productId", ProductID);
        data.append("Name", Name);
        data.append("Value", Value);
        data.append("__RequestVerificationToken", $('input[name=__RequestVerificationToken]').val());
        $.ajax({
            url: "/Admin/ServicesProduct/Attributes/",
            type: "POST",
            processData: false,
            contentType: false,
            data: data,
            success: function (response) {
                if (response.success) {
                    
                    row.find(".ID").val(response.data.ID);
                    row.find(".ProductID").val(response.data.ServiceProductsID);
                    row.find(".IsActive").attr("onclick", "Activate(this," + response.data.ID+")");
                    row.find(".IsActive").attr("disabled", false);
                    row.find(".Delete").attr("onclick", "Delete(this," + response.data.ID + ")");
                    row.find(".Delete").attr("disabled", false);
                    toastr.success(response.message);
                }
                else {
                    toastr.error(response.message);
                    console.log(response.error);
                }
            },
            error: function (er) {
                toastr.error(er);
            }
        });
        return false;
    }
}

function Activate(element, record) {
    swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, do it!'
    }).then(function (result) {
        if (result.value) {
            $(element).find('i').remove();
            $(element).addClass('spinner spinner-left spinner-sm').attr('disabled', true);

            $.ajax({
                url: '/Admin/ServicesProduct/AttributeActivate/' + record,
                type: 'Get',
                success: function (response) {
                    if (response.success) {
                        
                        if (response.data.IsActive == "True") {
                            $(element).removeClass("btn-outline-success").addClass("btn-outline-danger");
                            $(element).html('<i class="fa fa-times-circle"></i>Deactivate');
                        }
                        else {
                            $(element).removeClass("btn-outline-danger").addClass("btn-outline-success");
                            $(element).html('<i class="fa fa-check-circle"></i>Activate');
                        }
                        $(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
                        toastr.success(response.message);
                    } else {
                        toastr.error(response.message);
                        console.log(response.error);
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

function Delete(element, record) {
    var row = $(element).closest(".list-repeat");
    swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete it!'
    }).then(function (result) {
        if (result.value) {
            $.ajax({
                url: '/Admin/ServicesProduct/AttributeDelete/' + record,
                type: 'POST',
                data: {
                    "__RequestVerificationToken":
                        $("input[name=__RequestVerificationToken]").val()
                },
                success: function (response) {
                    if (response.success != undefined) {
                        if (response.success) {
                            row.slideUp();
                            setTimeout(function () { row.remove(); }, 250);
                            toastr.success('Attribute Deleted Successfully');
                        }
                        else {
                            toastr.error(response.message);
                            console.log(response.error);
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