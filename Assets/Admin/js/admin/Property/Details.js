
$(document).ready(function () {


    bindButton();
    BindPropertyNearByPlaces()
    BindPropertyImages();

    BindPropertyFloorImages();

    bindCityDropdown();
    BindPropertyInspections();

    $(document).on('click', '.property-image', function () {
        $('#imagepreview').attr('src', this.style.backgroundImage.slice(5, -2)); // here asign the image to the modal when the user click the enlarge link
        $('#imagepreview').attr('data-id', $(this).data("id"))
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

    $('.checkbox-list').on('change', 'input[type=checkbox]', function (event) {
        var propID = $('#ID').val()
        var id = $(event.target).val();
        const checked = $(this).is(':checked');

        if (checked) {
            $.ajax({
                type: "POST",
                url: "/Vendor/Property/AssignFeature?PropId=" + propID + "&FeatureId=" + id,
                data: { "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val() },
                success: function (data) {
                    toastr.success(data.message);
                }
            });
        } else {
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

    //gallery upload

    $('#btn-gallery-image-upload').click(function () {
        $('input[name=GalleryImages]').trigger('click');
    });

    $('input[name=GalleryImages]').change(function () {

        var data = new FormData();
        var files = $("input[name=GalleryImages]").get(0).files;
        if (files.length > 0) {

            if ($('.Property-gallery-images .symbol').length + files.length > 4) {
                toastr.error("You can only upload four images");
                return;
            }

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
                                '<div class="symbol-label" style="background-image: url(\'' + v.Value + '\')"></div>' +
                                '</div>');
                        });

                        $('#btn-gallery-image-upload').removeClass('spinner spinner-light spinner-left').prop('disabled', false);
                        $('#btn-gallery-image-upload').find('span').show();
                        toastr.success("Gallery images uploaded ...");

                        if ($('.Property-gallery-images .symbol').length === 4) {
                            $('#btn-gallery-image-upload').hide();
                        } else {
                            $('#btn-gallery-image-upload').show();
                        }
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

            //if ($('.Property-floor-images .symbol').length + files.length > 4) {
            //    toastr.error("You can only upload four images");
            //    return;
            //}

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



})
function GetURLParameter() {
    var sPageURL = window.location.href;
    var indexOfLastSlash = sPageURL.lastIndexOf("/");

    if (indexOfLastSlash > 0 && sPageURL.length - 1 != indexOfLastSlash)
        return sPageURL.substring(indexOfLastSlash + 1);
    else
        return 0;
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
                    Title: $('#Title').val(),
                    Thumbnail: $("#Thumbnail").val(),
                    TitleAr: $('#TitleAr').val(),
                    Slug: $('#Slug').val(),
                    Price: $('#Price').val(),
                    SellerTransactionFee: $("#SellerTransactionFee").val(),
                    BuyerTransactionFee: $("#BuyerTransactionFee").val(),
                    VendorId: $("#VendorId").val(),
                    Description: $("#Description").val(),
                    DescriptionAr: $("#DescriptionAr").val(),
                    ApprovalStatusID: $("#ApprovalStatusID").val(),
                    IsPublished: $("#IsPublished").val(),
                    IsDeleted: $("#IsDeleted").val(),
                    Size: $("#Size").val(),
                    NoOfGarage: $('#NoOfGarage').val(),
                    NoOfRooms: $('#NoOfRooms').val(),
                    NoOfBaths: $("#NoOfBaths").val(),
                    NoOfDinings: $("#NoOfDinings").val(),
                    NoOfLaundry: $("#NoOfLaundry").val(),
                    BuildYear: $("#BuildYear").val(),
                    CategoryId: $("#CategoryId").find('option:selected').attr('value'),
                    ForSale: $("#ForSale").find('option:selected').attr('value'),
                    CreatedOn: $("#CreatedOn").val(),
                    CityId: $("#City").find('option:selected').attr('value'),
                    CountryId: $("#CountryId").find('option:selected').attr('value'),
                    Area: $("#Area").val(),
                    State: $("#State").val(),
                    ZipCode: $("#ZipCode").val(),
                    ZonedFor: $("#ZonedFor").val(),
                    Address: $("#Address").val(),
                    Latitude: $("#Latitude").val(),
                    Longitude: $("#Longitude").val(),
                    BuildYear: $("#BuildYear").val(),
                    IsFurnished: $('select[name=IsFurnished]').val() == "1" ? true : false,
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
        $('.prop-features .checkbox').hide();
        //$('.car-category .checkbox:contains("' + text + '")').css('background-color', 'red');
        $('.prop-features .checkbox:contains("' + text + '")').fadeIn();
    } else {
        $('.prop-features .checkbox').show();
    }
}
function BindPropertyInspections() {
    
    $.ajax({
        url: '/Admin/PropertyInspection/GetInspections/' + GetURLParameter(),
        type: 'GET',
        success: function (response) {
            if (response.success) {
                var html = "";
                $('.car-inspections').html('');

                $(response.document).each(function (k, v) {

                    html += `<div class="row mt-1 ${v.id}">
                                 <div class="col-4">
                                     <label class="mt-2"><i class="fa fa-file-contract mr-2"></i><b>${v.name}</b></label>
                                 </div>
                                 <div class="col-1">
                            <a href="${v.path}" class="btn btn-bg-secondary  btn-sm" target="blank"> <span class="fas fa-eye">View</span>  </a>
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
function BindPropertyImages() {
    $.ajax({
        url: '/Admin/Property/GetPropertyImages/' + $("#ID").val(),
        type: 'GET',
        success: function (response) {
            if (response.success) {
                $('.Property-gallery-images').html('');
                $(response.carImages).each(function (k, v) {
                    $('.Property-gallery-images').append('<div class="symbol symbol-70 flex-shrink-0 mr-5 mb-3">' +                        
                        '<div class="symbol-label property-image" data-id=' + k +' style="background-image: url(\'' + v.Image + '\')"></div>' +
                        '</div>');
                });

                if ($('.Property-gallery-images .symbol').length === 4) {
                    $('#btn-gallery-image-upload').hide();
                } else {
                    $('#btn-gallery-image-upload').show();
                }
            } else {
            }
        }
    });
}

function BindPropertyFloorImages() {
    $.ajax({
        url: '/Admin/Property/GetPropertyFloor/' + $("#ID").val(),
        type: 'GET',
        success: function (response) {
            if (response.success) {
                $('.Property-floor-images').html('');
                $(response.carImages).each(function (k, v) {
                    $('.Property-floor-images').append('<div class="symbol symbol-70 flex-shrink-0 mr-5 mb-3">' +                        
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



