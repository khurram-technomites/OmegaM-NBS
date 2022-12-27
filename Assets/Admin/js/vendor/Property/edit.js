
$(document).ready(function () {

    $('#BuildYear').datepicker({
        format: "yyyy",
        viewMode: "years",
        minViewMode: "years",
        autoclose: true //to close picker once year is selected
    });

    bindButton();

    BindPropertyImages();

    BindPropertyFloorImages();

    bindCityDropdown();
    BindPropertyInspections();
    BindPropertyNearByPlaces();

    $('.tab-options span').click(function () {
        if ($(this).hasClass('active')) {

            $(this).removeClass('active');
            $(this).closest('.form-group').find('input').val('');
        } else {

            $(this).closest('.tab-options').find('span.active').removeClass('active');

            $(this).addClass('active');

            if ($(this).text().includes("+")) {
                $(this).closest('.form-group').find('input').val($(this).text().replace("+", ""));
            } else {
                $(this).closest('.form-group').find('input').val($(this).text());
            }

        }
    });

    $('.checkbox-list').on('change', 'input[type=checkbox]', function (event) {
        var propID = $('#ID').val()
        var id = $(event.target).val();
        const checked = $(this).is(':checked');

        if (checked) {
            $(this).closest('label').addClass('active');

            $.ajax({
                type: "POST",
                url: "/Vendor/Property/AssignFeature?PropId=" + propID + "&FeatureId=" + id,
                data: { "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val() },
                success: function (data) {
                    toastr.success(data.message);
                }
            });
        } else {
            $(this).closest('label').removeClass('active');

            $.ajax({
                type: "POST",
                url: "/Vendor/Property/RemoveFeature?PropId=" + propID + "&FeatureId=" + id,
                data: { "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val() },
                success: function (data) {
                    toastr.success(data.message);
                }
            });
        }

    });
    $(document).on('click', '#uploadfloorplan', function () {

        $('#uploadfloorplan').addClass('spinner spinner-light spinner-left').prop('disabled', true);
        $('#uploadfloorplan').find('span').hide();
        var source = $('#imagepreview').attr('src')
        var imageid = $('#imagepreview').attr("data-imageid")
        if ($('.Property-floor-images .symbol').length  >= 4) {
            toastr.error("you can only upload four images");
            $('#uploadfloorplan').removeClass('spinner spinner-light spinner-left').prop('disabled', false);
            $('#uploadfloorplan').find('span').show();
            return;
        }
        $.ajax({
            type: "POST",
            url: "/Vendor/Property/MoveFloorImage?sourcePath=" + source + "&id=" + $("#ID").val(),
            success: function (data) {
                if (data.success) {
                    toastr.success("Image Move Successfully");
                    $('#uploadfloorplan').removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                    $('#uploadfloorplan').find('span').show();
                    BindPropertyFloorImages()
                    DeleteGalleryImage(this, imageid)
                    BindPropertyImages();
                    
                    $("#imagemodal").modal('hide');
                }
                else {
                    toastr.error("An error Occured");
                      $('#uploadfloorplan').removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                    $('#uploadfloorplan').find('span').show();
                }
                
                
            }
        });

    })
    $(document).on('click', '.property-image', function () {
        $('#imagepreview').attr('src', this.style.backgroundImage.slice(5, -2)); // here asign the image to the modal when the user click the enlarge link
        $('#imagepreview').attr('data-id', $(this).data("id"))
        $('#imagepreview').attr('data-imageid', $(this).data("propertyimageid"))
        $('.w3-display-left').removeAttr("hidden", "hidden")
        $('.w3-display-right').removeAttr("hidden", "hidden")
        $('#imagemodal').modal('show'); // imagemodal is the id attribute assigned to the bootstrap modal, then i use the show function
    });
    $(document).on('click', '.thumbnail', function () {
        $('#imagepreview').attr('src', this.style.backgroundImage.slice(5, -2)); // here asign the image to the modal when the user click the enlarge link
        /* $('#imagepreview').attr('data-id', $(this).data("id"))*/
        $('.w3-display-left').attr("hidden", "hidden")
        $('.w3-display-right').attr("hidden", "hidden")
        $('#imagemodal').modal('show'); // imagemodal is the id attribute assigned to the bootstrap modal, then i use the show function
    });
    //thumbnail uploader

    $('#kt_image_Property_image #Image').change(function () {
        var data = new FormData();
        var files = $("#kt_image_Property_image #Image").get(0).files;
        var elem = $(this)
        if (files.length > 0) {
            $(elem).closest('.image-input').find('label[data-action="change"] i').hide();
            $(elem).closest('.image-input').find('label[data-action="change"]').addClass('spinner spinner-dark spinner-center spinner-sm').prop('disabled', true);
            data.append("Image", files[0]);
            $.ajax({
                url: "/Vendor/Property/Thumbnail/" + $("#ID").val(),
                type: "POST",
                processData: false,
                contentType: false,
                data: data,
                success: function (response) {
                    if (response.success) {
                        $('#kt_image_Property_image .image-input-wrapper').css('background-image', 'url(' + response.data + ')');
                        toastr.success(response.message);
                        $("#Thumbnail").val("")
                        $("#Thumbnail").val(response.data)
                    } else {
                        toastr.error(response.message);
                    }
                    $(elem).closest('.image-input').find('label[data-action="change"] i').show();
                    $(elem).closest('.image-input').find('label[data-action="change"]').removeClass('spinner spinner-dark spinner-center spinner-sm').prop('disabled', false);
                },
                error: function (e) {
                    $(elem).closest('.image-input').find('label[data-action="change"] i').show();
                    $(elem).closest('.image-input').find('label[data-action="change"]').removeClass('spinner spinner-dark spinner-center spinner-sm').prop('disabled', false);
                    toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                },
                failure: function (e) {
                    $(elem).closest('.image-input').find('label[data-action="change"] i').show();
                    $(elem).closest('.image-input').find('label[data-action="change"]').removeClass('spinner spinner-dark spinner-center spinner-sm').prop('disabled', false);
                    toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                }
            });
        }
    });

    //Video upload
    /*   $('#kt_Video_car_Video #Video').change(function () {
           var data = new FormData();
           var files = $("#kt_Video_car_Video #Video").get(0).files;
           var elem = $(this)
           if (files.length > 0) {
               $(elem).closest('.image-input').find('label[data-action="change"] i').hide();
               $(elem).closest('.image-input').find('label[data-action="change"]').addClass('spinner spinner-dark spinner-center spinner-sm').prop('disabled', true);
               data.append("Video", files[0]);
               $.ajax({
                   url: "/Vendor/Property/Video/" + GetURLParameter(),
                   type: "POST",
                   processData: false,
                   contentType: false,
                   data: data,
                   success: function (response) {
                       if (response.success) {
                           var Video = $("#VideoTag")
                           var Source = $("#videoSource")
   
                           Video.get(0).pause();
                           Source.attr('src', response.data + '#t=0.5');
                           Video.get(0).load();
   
                           //$('#kt_Video_car_Video #videoSource').attr('src', 'url(' + response.data + ')#t=0.5');
                           toastr.success(response.message);
                       } else {
                           toastr.error(response.message);
                       }
                       $(elem).closest('.image-input').find('label[data-action="change"] i').show();
                       $(elem).closest('.image-input').find('label[data-action="change"]').removeClass('spinner spinner-dark spinner-center spinner-sm').prop('disabled', false);
                   },
                   error: function (e) {
                       console.log(e)
                       $(elem).closest('.image-input').find('label[data-action="change"] i').show();
                       $(elem).closest('.image-input').find('label[data-action="change"]').removeClass('spinner spinner-dark spinner-center spinner-sm').prop('disabled', false);
                       toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                   },
                   failure: function (e) {
                       console.log(e)
                       $(elem).closest('.image-input').find('label[data-action="change"] i').show();
                       $(elem).closest('.image-input').find('label[data-action="change"]').removeClass('spinner spinner-dark spinner-center spinner-sm').prop('disabled', false);
                       toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                   }
               });
           }
       });
   */

    //gallery upload

    $('#btn-gallery-image-upload').click(function () {
        $('input[name=GalleryImages]').trigger('click');
    });

    $('input[name=GalleryImages]').change(function () {

        var data = new FormData();
        var files = $("input[name=GalleryImages]").get(0).files;
        if (files.length > 0) {

            //if ($('.Property-gallery-images .symbol').length + files.length > 4) {
            //    toastr.error("You can only upload four images");
            //    return;
            //}

            $('#btn-gallery-image-upload').addClass('spinner spinner-light spinner-left').prop('disabled', true);
            $('#btn-gallery-image-upload').find('span').hide();
            $.each(files, function (j, file) {
                data.append('Image[' + j + ']', file);
            })
            //data.append("Image", files);
            data.append("count", $('.Property-gallery-images .symbol').length);

            $.ajax({
                url: "/Vendor/Property/CreateImage/" + $("#ID").val(),
                type: "POST",
                processData: false,
                contentType: false,
                data: data,
                success: function (response) {
                    if (response.success) {
                        $(response.data).each(function (k, v) {
                            $('.Property-gallery-images').append('<div class="symbol symbol-70 flex-shrink-0 mr-5 mb-3">' +
                                '<span class="btn btn-xs btn-icon btn-circle btn-danger btn-hover-text-primary btn-shadow btn-remove-gallery-image" data-action="cancel" data-toggle="tooltip" title="remove" onclick="DeleteGalleryImage(this,' + v.Key + ')">' +
                                '<i class="icon-xs ki ki-bold-close ki-bold-trash"></i>' +
                                '</span>' +
                                '<div class="symbol-label  property-image"data-propertyimageid=' + v.Key +' data-id=' + TotalImage + 1 + '  style="background-image: url(\'' + v.Value + '\')"></div>' +
                                '</div>');
                        });
                        TotalImage = TotalImage + 1
                        $('#btn-gallery-image-upload').removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                        $('#btn-gallery-image-upload').find('span').show();
                        toastr.success("Gallery images uploaded ...");

                        //if ($('.Property-gallery-images .symbol').length === 4) {
                        //    $('#btn-gallery-image-upload').hide();
                        //} else {
                        //    $('#btn-gallery-image-upload').show();
                        //}
                    } else {
                        $('#btn-gallery-image-upload').removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                        $('#btn-gallery-image-upload').find('span').show();
                        toastr.error(response.message);
                    }
                },
                error: function (e) {
                    $('#btn-gallery-image-upload').removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                    $('#btn-gallery-image-upload').find('span').show();
                    toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                },
                failure: function (e) {
                    $('#btn-gallery-image-upload').removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                    $('#btn-gallery-image-upload').find('span').show();
                    toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                }
            });
        }
    });

    //floor image

    $('#btn-floor-image-upload').click(function () {
        $('input[name=FloorImages]').trigger('click');
    });

    $('input[name=FloorImages]').change(function () {

        var data = new FormData();
        var files = $("input[name=FloorImages]").get(0).files;
        if (files.length > 0) {

            if ($('.Property-floor-images .symbol').length + files.length > 4) {
                toastr.error("you can only upload four images");
                return;
            }

            $('#btn-floor-image-upload').addClass('spinner spinner-light spinner-left').prop('disabled', true);
            $('#btn-floor-image-upload').find('span').hide();
            $.each(files, function (j, file) {
                data.append('Image[' + j + ']', file);
            })
            //data.append("Image", files);
            data.append("count", $('.Property-floor-images .symbol').length);

            $.ajax({
                url: "/Vendor/Property/CreateFloorImage/" + $("#ID").val(),
                type: "POST",
                processData: false,
                contentType: false,
                data: data,
                success: function (response) {
                    if (response.success) {
                        $(response.data).each(function (k, v) {
                            $('.Property-floor-images').append('<div class="symbol symbol-70 flex-shrink-0 mr-5 mb-3">' +
                                '<span class="btn btn-xs btn-icon btn-circle btn-danger btn-hover-text-primary btn-shadow btn-remove-gallery-image" data-action="cancel" data-toggle="tooltip" title="remove" onclick="DeleteFloorImage(this,' + v.Key + ')">' +
                                '<i class="icon-xs ki ki-bold-close ki-bold-trash"></i>' +
                                '</span>' +
                                '<div class="symbol-label" style="background-image: url(\'' + v.Value + '\')"></div>' +
                                '</div>');
                        });

                        $('#btn-floor-image-upload').removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                        $('#btn-floor-image-upload').find('span').show();
                        toastr.success("Floor images uploaded ...");

                        if ($('.Property-floor-images .symbol').length === 4) {
                            $('#btn-floor-image-upload').hide();
                        } else {
                            $('#btn-floor-image-upload').show();
                        }
                    } else {
                        $('#btn-floor-image-upload').removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                        $('#btn-floor-image-upload').find('span').show();
                        toastr.error(response.message);
                    }
                },
                error: function (e) {
                    $('#btn-floor-image-upload').find('span').show();
                    $('#btn-floor-image-upload').removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                    toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                },
                failure: function (e) {
                    $('#btn-floor-image-upload').removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                    $('#btn-floor-image-upload').find('span').show();
                    toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                }
            });
        }
    });

    $("#Video").change(function (e) {

        var file;

        if ((file = this.files[0])) {

            if (!file.type.match('video.*')) {
                Swal.fire({
                    icon: 'error',
                    title: 'Oops...',
                    text: 'Please upload valid video file !',

                }).then(function (result) {
                    $("#Video").val("");
                });
            }
            else if (file.size >= 10000000) {
                Swal.fire({
                    icon: 'error',
                    title: 'Oops...',
                    text: 'Video size must be less than 10 mb !',

                }).then(function (result) {
                    $("#Video").val("");
                });
            }
            else if (file.size <= 10000000) {
                VideoUpload();
            }
        }
    });

    $("#ForSale").change(function (e) {

        if ($(this).val() == 'False') {
            $('#Price').closest('.form-group').find('label').text('Rent (Yearly)');

        } else {

            $('#Price').closest('.form-group').find('label').text('Price');
        }

    });

    $("#ForSale").trigger('change');

})
function BindPropertyInspections() {
    $.ajax({
        url: '/Vendor/PropertyInspection/GetInspections/' + GetURLParameter(),
        type: 'GET',
        success: function (response) {
            if (response.success) {
                var html = "";
                $('.car-inspections').html('');

                $(response.document).each(function (k, v) {
                    $("#inspectionbtn").attr('hidden', 'hidden')
                    html += `<div class="row mt-1 ${v.id}">
                                 <div class="col-6">
                                     <label class="mt-2"><i class="fa fa-file-contract mr-2"></i><b>${v.name}</b></label>
                                 </div>
                                 <div class="col-1">
                                     <a href="${v.path}" class="btn btn-bg-secondary  btn-sm" target="blank"> <span class="fas fa-eye"> View</span></a>
                                 </div>
                                 <div class="col-1">
                                    <button id="btnDeleteInspection" class="btn btn-bg-secondary  btn-sm" onclick="DeleteInspection(this,${v.id} )">
										<span class="fas fa-trash "> Delete</span>
									</button>
                                 </div>
                             </div> `;
                });

                $('.car-inspections').append(html);

            } else {
                $('.car-inspectionse').html('No Packages!');
            }
        }
    });
}


function BindPropertyNearByPlaces() {
    

    $.ajax({
        url: '/Vendor/PropertyNearByPlaces/GetNearByPlaces/' + GetURLParameter(),
        type: 'GET',
        success: function (response) {
            if (response.success) {
                var html = "";
                $('.property-nearbyplaces').html('');
                

                $(response.data).each(function (k, v) {
                    html += `<div class="bg-primary-o-65 m-5 rounded ${v.ID}" style="width: 300px;">
                               <span class="btn btn-xs btn-icon btn-circle btn-danger btn-hover-text-primary btn-shadow position-absolute" style="margin-top: -12px; margin-left: 285px;" title="remove" onclick="DeletePropertyNearByPlaces(this,${v.ID})"><i class="icon-xs ki ki-bold-close ki-bold-trash"></i></span>
                                   <div class="row p-3">
                                       <div class="col-3 m-auto">
                                           <img class"rounded" height="55" src="${v.Image}" />
                                       </div>
                                       <div class="col-9">
                                           <p class="font-size-lg m-0" style="font-weight: 700; color: steelblue;">${v.Category}</p>
                                           <p class="m-0 font-size-sm line-height-md">${v.Name}</p>
                                           <p class="m-0 line-height-md text-dark-65 font-size-sm">${v.Distance}</p>
                                       </div>
                                   </div>
                               </div>`;
                });

                $('.property-nearbyplaces').append(html);

            } else {
                $('.property-nearbyplaces').html('No Packages!');
            }
        }
    });
}

function SavePropertyNearByPlaces() {
    
    $("#btnSubmitNearByPlaces").addClass('spinner spinner-dark spinner-right');
    var data = new FormData();
    

    data.append("PropertyID", $("#PropertyID").val());
    data.append("NearByPlacesCategoryID", $("#NearByPlacesCategoryID").val());
    data.append("Name", $('#Name').val());
    data.append("NameAr", $("#NameAr").val());
    data.append("Distance", $("#Distance").val());
    data.append("Latitude", $("#Lat").val());
    data.append("Longitude", $("#Long").val());

    $.ajax({
        url: '/Vendor/PropertyNearByPlaces/NearByPlacesCreate/',
        type: 'POST',
        processData: false,
        contentType: false,
        data: data,
        success: function (response) {
            if (response.success) {

                $('#myModal').modal('hide');
                var html = "";

                html += `<div class="bg-primary-o-65 m-5 rounded ${response.data.ID}" style="width: 300px;">
                               <span class="btn btn-xs btn-icon btn-circle btn-danger btn-hover-text-primary btn-shadow position-absolute" style="margin-top: -12px; margin-left: 285px;" title="remove" onclick="DeletePropertyNearByPlaces(this,${response.data.ID})"><i class="icon-xs ki ki-bold-close ki-bold-trash"></i></span>
                                   <div class="row p-3">
                                       <div class="col-3 m-auto">
                                           <img class"rounded" height="55" src="${response.data.Image}" />
                                       </div>
                                       <div class="col-9">
                                           <p class="font-size-lg m-0" style="font-weight: 700; color: steelblue;">${response.data.Category}</p>
                                           <p class="m-0 font-size-sm line-height-md">${response.data.Name}</p>
                                           <p class="m-0 line-height-md text-dark-65 font-size-sm">${response.data.Distance}</p>
                                       </div>
                                   </div>
                               </div>`;

                $('.property-nearbyplaces').append(html);
                toastr.options = {
                    "positionClass": "toast-bottom-right",
                };
                /*$("." + result.data).remove();*/
                toastr.success(response.message);
                $("#btnSubmitNearByPlaces").removeClass('spinner spinner-dark spinner-right')

            }
            else {
                toastr.error(response.message);
                $("#btnSubmitNearByPlaces").removeClass('spinner spinner-dark spinner-right')
            }
        },
    });
}

function DeletePropertyNearByPlaces(element, record) {
    swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete it!'
    }).then(function (result) {
        if (result.value) {

            $(this).addClass('spinner spinner-dark spinner-right');
            $.ajax({
                url: '/Vendor/PropertyNearByPlaces/DeletePropertyNearByPlaces/' + record,
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
                            $("." + result.data).remove();
                            toastr.success('Near by place deleted successfully');
                            $("#btnDeleteNearByPlaces").removeClass('spinner spinner-dark spinner-right')

                        }
                        else {
                            toastr.error(result.message);
                            $("#btnDeleteNearByPlaces").removeClass('spinner spinner-dark spinner-right')
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

function SaveNewCarInspection() {

    $("#btnSubmitInspection").addClass('spinner spinner-dark spinner-right');
    var data = new FormData();
    var files = $("#fileUpload").get(0).files;
    if (files.length > 0) {
        data.append("FileUpload", files[0]);
    }

    data.append("Name", $('#NameDocs').val());
    data.append("id", $("#PropertyID").val());

    $.ajax({
        url: '/Vendor/PropertyInspection/CreateInspection/',
        type: 'POST',
        processData: false,
        contentType: false,
        data: data,
        success: function (response) {
            if (response.success) {

                $('#myModal').modal('hide');
                var html = "";
                $("#inspectionbtn").attr('hidden', 'hidden')

                html += `<div class="row mt-1 ${response.data.ID}">
                                 <div class="col-6">
                                     <label class="mt-2"><i class="fa fa-file-contract mr-2"></i><b>${response.data.Name}</b></label>
                                 </div>
                                 <div class="col-1">
                                     <a href="${response.data.Path}" class="btn btn-bg-secondary  btn-sm" target="blank"> <span class="fas fa-eye" style="font-family: Font Awesome 5 Free;">  View</span>  </a>
                                 </div>
                                 <div class="col-1">
                                    <button id="btnDeleteInspection" class="btn btn-bg-secondary  btn-sm" onclick="DeleteInspection(this,${response.data.ID} )">
										<span class="fas fa-trash " style="font-family: Font Awesome 5 Free;">  Delete</span> 
									</button>
                                 </div>
                             </div> `;

                $('.car-inspections').append(html);
                toastr.options = {
                    "positionClass": "toast-bottom-right",
                };
                /*$("." + result.data).remove();*/
                toastr.success(response.message);
                $("#btnSubmitInspection").removeClass('spinner spinner-dark spinner-right')

            }
            else {
                toastr.error(response.message);
                $("#btnSubmitInspection").removeClass('spinner spinner-dark spinner-right')
            }
        },
    });
}

function DeleteInspection(element, record) {
    swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete it!'
    }).then(function (result) {
        if (result.value) {

            $(this).addClass('spinner spinner-dark spinner-right');
            $.ajax({
                url: '/Vendor/PropertyInspection/DeletePropertyInspection/' + record,
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
                            $("." + result.data).remove();
                            toastr.success('Inspection deleted successfully');
                            $("#inspectionbtn").removeAttr('hidden', 'hidden')
                            $("#btnDeleteInspection").removeClass('spinner spinner-dark spinner-right')

                        }
                        else {
                            toastr.error(result.message);
                            $("#btnDeleteInspection").removeClass('spinner spinner-dark spinner-right')
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




function bindCityDropdown() {
    $("#CountryId").change(function () {
        var ID = $("#CountryId").val()

        if (ID != "") {
            $.ajax({
                type: "GET",
                url: "/Vendor/Property/GetCities?CountryId=" + ID,
                data: "{}",
                success: function (data) {
                    var s = '<option value="-1">Select City</option>';
                    for (var i = 0; i < data.length; i++) {
                        s += '<option value="' + data[i].value + '">' + data[i].text + '</option>';
                    }
                    $("#City").html(s);
                }
            });
            $("#City").prop("disabled", false)
        } else {
            $("#City").prop("disabled", true)
            $("#City option").remove();
            $("#City").html('<option value="-1">Select City</option>');
        }
    })
}

function bindButton() {
    $('#btnFormSubmit').submit(function () {
        var e = $("#btnSaveCar");
        $(e).addClass('spinner spinner-light spinner-left').prop('disabled', true);
        $(e).find('i').hide();
        var brands = $("#BrandID option:selected").val();

        $.ajax({
            url: '/Vendor/Property/Update/',
            type: 'POST',
            data:
            {
                "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
                Entity: {

                    ID: $('#ID').val(),
                    Slug: $('#Slug').val(),
                    ForSale: $("#ForSale").val(),
                    CategoryId: $("#CategoryId").val(),
                    Title: $('#Title').val(),
                    TitleAr: $('#TitleAr').val(),

                    NoOfRooms: $('#NoOfRooms').val(),
                    NoOfBaths: $("#NoOfBaths").val(),
                    NoOfDinings: $("#NoOfDinings").val(),
                    NoOfLaundry: $("#NoOfLaundry").val(),
                    NoOfGarage: $('#NoOfGarage').val(),
                    IsFurnished: $('input[name=IsFurnished]').prop('checked'),

                    BuildYear: $("#BuildYear").val(),
                    Price: $('#Price').val(),
                    Size: $("#Size").val(),

                    Description: Description.getData(),
                    DescriptionAr: DescriptionAr.getData(),

                }
            },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                }
                else {
                    toastr.error(response.message);
                }
                $(e).find('i').show();
                $(e).removeClass('spinner spinner-light spinner-left').prop('disabled', false);
            },
            error: function (e) {
                $(e).find('i').show();
                $(e).removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
            },
            failure: function (e) {
                $(e).find('i').show();
                $(e).removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
            }
        });

        return false;
    });

    $('#PropertyLocationForm').submit(function () {
        var e = $("#btnSavePropertyLocation");
        $(e).addClass('spinner spinner-light spinner-left').prop('disabled', true);
        $(e).find('i').hide();

        $.ajax({
            url: '/Vendor/Property/UpdateLocation/',
            type: 'POST',
            data:
            {
                "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
                Entity: {

                    ID: $('#ID').val(),
                    CountryID: $("#CountryId").find('option:selected').attr('value'),
                    CityID: $("#CityID").val(),
                    Area: $("#Area").val(),
                    Address: $("#Address").val(),
                    Latitude: $("#Latitude").val(),
                    Longitude: $("#Longitude").val()
                }
            },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                }
                else {
                    toastr.error(response.message);
                }
                $(e).find('i').show();
                $(e).removeClass('spinner spinner-light spinner-left').prop('disabled', false);
            },
            error: function (e) {
                $(e).find('i').show();
                $(e).removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
            },
            failure: function (e) {
                $(e).find('i').show();
                $(e).removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
            }
        });

        return false;
    });

}

function SearchFeatures(elem) {
    var text = $(elem).val().trim();
    if (text && text !== "") {
        $('.prop-feature').hide();
        //$('.car-category .checkbox:contains("' + text + '")').css('background-color', 'red');
        $('.prop-feature:contains("' + text + '")').fadeIn();
    } else {
        $('.prop-feature').show();
    }
}

function DeleteGalleryImage(elem, record) {
    $(elem).addClass('spinner spinner-light spinner-right').prop('disabled', true);
    $(elem).find('i').hide();
    $.ajax({
        url: '/Vendor/Property/DeleteImage/' + record,
        type: 'POST',
        data: {
            "__RequestVerificationToken":
                $("input[name=__RequestVerificationToken]").val()
        },
        success: function (response) {
            if (response.success) {
                $(elem).closest('.symbol').remove();
                toastr.success(response.message);

                //if ($('.Property-gallery-images .symbol').length === 4) {
                //    $('#btn-gallery-image-upload').hide();
                //} else {
                //    $('#btn-gallery-image-upload').show();
                //}
            }
            else {
                $(elem).removeClass('spinner spinner-light spinner-right').prop('disabled', false);
                $(elem).find('i').show();
                toastr.error(response.message);
                return false;

            }
        },
        error: function (e) {
            $(elem).removeClass('spinner spinner-light spinner-right').prop('disabled', false);
            $(elem).find('i').show();
            toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
        },
        failure: function (e) {
            $(elem).removeClass('spinner spinner-light spinner-right').prop('disabled', false);
            $(elem).find('i').show();
            toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
        }
    });
}
var TotalImage = 0;
function BindPropertyImages() {
    $.ajax({
        url: '/Vendor/Property/GetPropertyImages/' + $("#ID").val(),
        type: 'GET',
        success: function (response) {
            if (response.success) {
                $('.Property-gallery-images').html('');
                $(response.carImages).each(function (k, v) {
                    $('.Property-gallery-images').append('<div class="symbol symbol-70 flex-shrink-0 mr-5 mb-3">' +
                        '<span class="btn btn-xs btn-icon btn-circle btn-danger btn-hover-text-primary btn-shadow btn-remove-gallery-image" data-action="cancel" data-toggle="tooltip" title="remove" onclick="DeleteGalleryImage(this,' + v.id + ')">' +
                        '<i class="icon-xs ki ki-bold-close ki-bold-trash"></i>' +
                        '</span>' +
                        '<div class="symbol-label property-image" data-propertyimageid=' + v.id +' data-id=' + k + ' style="background-image: url(\'' + v.Image + '\')"></div>' +
                        '</div>');
                    TotalImage = k
                });

                //if ($('.Property-gallery-images .symbol').length === 4) {
                //    $('#btn-gallery-image-upload').hide();
                //} else {
                //    $('#btn-gallery-image-upload').show();
                //}
            } else {
            }
        }
    });
}

function DeleteFloorImage(elem, record) {
    $(elem).addClass('spinner spinner-light spinner-right').prop('disabled', true);
    $(elem).find('i').hide();
    $.ajax({
        url: '/Vendor/Property/DeleteFloorImage/' + record,
        type: 'POST',
        data: {
            "__RequestVerificationToken":
                $("input[name=__RequestVerificationToken]").val()
        },
        success: function (response) {
            if (response.success) {
                $(elem).closest('.symbol').remove();
                toastr.success(response.message);

                if ($('.Property-floor-images .symbol').length === 4) {
                    $('#btn-floor-image-upload').hide();
                } else {
                    $('#btn-floor-image-upload').show();
                }
            }
            else {
                $(elem).removeClass('spinner spinner-light spinner-right').prop('disabled', false);
                $(elem).find('i').show();
                toastr.error(response.message);
                return false;

            }
        },
        error: function (e) {
            $(elem).removeClass('spinner spinner-light spinner-right').prop('disabled', false);
            $(elem).find('i').show();
            toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
        },
        failure: function (e) {
            $(elem).removeClass('spinner spinner-light spinner-right').prop('disabled', false);
            $(elem).find('i').show();
            toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
        }
    });
}

function BindPropertyFloorImages() {
    $.ajax({
        url: '/Vendor/Property/GetPropertyFloor/' + $("#ID").val(),
        type: 'GET',
        success: function (response) {
            if (response.success) {
                $('.Property-floor-images').html('');
                $(response.carImages).each(function (k, v) {
                    $('.Property-floor-images').append('<div class="symbol symbol-70 flex-shrink-0 mr-5 mb-3">' +
                        '<span class="btn btn-xs btn-icon btn-circle btn-danger btn-hover-text-primary btn-shadow btn-remove-gallery-image" data-action="cancel" data-toggle="tooltip" title="remove" onclick="DeleteFloorImage(this,' + v.id + ')">' +
                        '<i class="icon-xs ki ki-bold-close ki-bold-trash"></i>' +
                        '</span>' +
                        '<div class="symbol-label" style="background-image: url(\'' + v.Image + '\')"></div>' +
                        '</div>');
                });

                if ($('.Property-floor-images .symbol').length === 4) {
                    $('#btn-floor-image-upload').hide();
                } else {
                    $('#btn-floor-image-upload').show();
                }
            } else {
            }
        }
    });
}

function VideoUpload() {
    var data = new FormData();
    var files = $("#kt_Video_car_Video #Video").get(0).files;
    var elem = $(this)
    if (files.length > 0) {
        $(elem).closest('.image-input').find('label[data-action="change"] i').hide();
        $(elem).closest('.image-input').find('label[data-action="change"]').addClass('spinner spinner-dark spinner-center spinner-sm').prop('disabled', true);
        data.append("Video", files[0]);
        $.ajax({
            url: "/Vendor/Property/Video/" + GetURLParameter(),
            type: "POST",
            processData: false,
            contentType: false,
            data: data,
            success: function (response) {
                if (response.success) {
                    var Video = $("#VideoTag")
                    var Source = $("#videoSource")

                    Video.get(0).pause();
                    Source.attr('src', response.data + '#t=0.5');
                    Video.get(0).load();

                    //$('#kt_Video_car_Video #videoSource').attr('src', 'url(' + response.data + ')#t=0.5');
                    toastr.success("Property video uploaded successfully...!");
                } else {
                    toastr.error(response.message);
                }
                $(elem).closest('.image-input').find('label[data-action="change"] i').show();
                $(elem).closest('.image-input').find('label[data-action="change"]').removeClass('spinner spinner-dark spinner-center spinner-sm').prop('disabled', false);
            },
            error: function (e) {
                console.log(e)
                $(elem).closest('.image-input').find('label[data-action="change"] i').show();
                $(elem).closest('.image-input').find('label[data-action="change"]').removeClass('spinner spinner-dark spinner-center spinner-sm').prop('disabled', false);
                toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
            },
            failure: function (e) {
                console.log(e)
                $(elem).closest('.image-input').find('label[data-action="change"] i').show();
                $(elem).closest('.image-input').find('label[data-action="change"]').removeClass('spinner spinner-dark spinner-center spinner-sm').prop('disabled', false);
                toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
            }
        });
    }
};

function SaveNewFeature() {

    $("#btnSubmitFeature").addClass('spinner spinner-dark spinner-right');
    var data = new FormData();
    data.append("Name", $('#FeatureName').val());
    data.append("NameAR", $("#FeatureNamear").val());
    data.append("CarID", GetURLParameter());

    $.ajax({
        url: '/Vendor/Property/AddFeature/',
        type: 'POST',
        processData: false,
        contentType: false,
        data: data,
        success: function (response) {
            if (response.success) {
                $('#myModal').modal('hide');

                $('.car-features').append('<div class="car-feature car-feature-plain mb-1" id="car-feature' + response.id + '">' +
                    '<label class="checkbox">' +
                    '<input type="checkbox" onchange="MyNewFeature(this)" checked name="chkFeature' + response.id + '" id="chkFeature' + response.id + '" data="' + response.id + '" FeatureID=' + response.FeatureID + '/>' +
                    '<span></span>' +
                    response.name +
                    '</label>' +
                    '</div>');
                toastr.success(response.message);
            }
        },
    });
}

function GetURLParameter() {
    var sPageURL = window.location.href;
    var indexOfLastSlash = sPageURL.lastIndexOf("/");

    if (indexOfLastSlash > 0 && sPageURL.length - 1 != indexOfLastSlash)
        return sPageURL.substring(indexOfLastSlash + 1);
    else
        return 0;
}