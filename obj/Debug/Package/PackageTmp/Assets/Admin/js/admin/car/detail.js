"use strict";

var carAttributes = [];

var CarAttributeSetting = [];

var carVariationAttributes = [];

// Class definition


var KTTagifyDemos = function (id) {
    var tagify;
    // Private functions
    var demo1 = function (id) {
        var prod_attr = carAttributes.filter(function (obj) {
            return obj.attributeId == id
        });
        var input = document.getElementById("kt_tagify_" + id);
        // init Tagify script on the above inputs
        tagify = new Tagify(input)


        tagify.addTags(prod_attr);
        // "remove all tags" button event listener


        // Chainable event listeners
        tagify.on('add', function (e) {
            $.ajax({
                url: '/Admin/CarAttribute/Create/',
                type: 'POST',
                data: {
                    "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
                    carAttribute: {
                        CarID: GetURLParameter(),
                        AttributeID: $(e.detail.tag).closest('.attribute').attr('id'),
                        Value: e.detail.data.value
                    }
                },
                success: function (response) {
                    if (response.success) {
                        //$(this).attr('carCategoryId', '');
                    }
                    else {
                        return false;
                    }
                }
            });
        })
        tagify.on('remove', function (e) {
            if (e.detail.data.id) {
                $.ajax({
                    url: '/Admin/CarAttribute/Delete/' + e.detail.data.id,
                    type: 'POST',
                    data: {
                        "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val()
                    },
                    success: function (response) {
                        if (response.success) {
                            //$(this).attr('carCategoryId', '');
                        }
                        else {
                            return false;
                        }
                    }
                });
            } else {
                $.ajax({
                    url: '/Admin/CarAttribute/DeleteValue/',
                    type: 'POST',
                    data: {
                        "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
                        carAttribute: {
                            CarID: GetURLParameter(),
                            AttributeID: $(e.detail.tagify.DOM.input).closest('.attribute').attr('id'),
                            Value: e.detail.data.value
                        }
                    },
                    success: function (response) {
                        if (response.success) {
                            //$(this).attr('carCategoryId', '');
                        }
                        else {
                            return false;
                        }
                    }
                });
            }
        });
    }

    return {
        // public functions
        init: function (id) {
            demo1(id);
        }
    };
}();

$(document).on('click', '.car-image', function () {
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


var KTTagifyCarTags = function (id) {
    var tagify;
    // Private functions
    var demo5 = function (id) {
        $.ajax({
            url: '/Admin/CarTag/GetCarTags/' + GetURLParameter(),
            type: 'GET',
            success: function (response) {
                if (response.success) {

                    // Init autocompletes
                    var toEl = document.getElementById(id);
                    tagify = new Tagify(toEl, {
                        delimiters: ", ", // add new tags when a comma or a space character is entered
                        maxTags: 10,
                        blacklist: ["fuck", "shit", "pussy"],
                        keepInvalidTags: false, // do not remove invalid tags (but keep them marked as invalid)
                        whitelist: response.tags,
                        templates: {
                            dropdownItem: function (tagData) {
                                try {
                                    var html = '';

                                    html += '<div class="tagify__dropdown__item">';
                                    html += '   <div class="d-flex align-items-center">';
                                    //html += '       <span class="symbol sumbol-' + (tagData.initialsState ? tagData.initialsState : '') + ' mr-2">';
                                    //html += '           <span class="symbol-label" style="background-image: url(\'' + (tagData.pic ? tagData.pic : '') + '\')">' + (tagData.initials ? tagData.initials : '') + '</span>';
                                    //html += '       </span>';
                                    html += '       <div class="d-flex flex-column">';
                                    html += '           <a href="#" id="' + (tagData.id ? tagData.id : '') + '" class="text-dark-75 text-hover-primary font-weight-bold">' + (tagData.value ? tagData.value : '') + '</a>';
                                    //html += '           <span class="text-muted font-weight-bold">' + (tagData.value ? tagData.value : '') + '</span>';
                                    html += '       </div>';
                                    html += '   </div>';
                                    html += '</div>';

                                    return html;
                                } catch (err) { }
                            }
                        },
                        transformTag: function (tagData) {
                            tagData.class = 'tagify__tag tagify__tag--primary';
                        },
                        dropdown: {
                            classname: "color-blue",
                            enabled: 1,
                            maxItems: 5
                        }
                    })

                    tagify.addTags(response.carTags);
                    tagify.on('dropdown:select', onSelectSuggestion)
                    tagify.on('remove', function (e) {

                        $.ajax({
                            url: '/Admin/CarTag/Delete/',

                            type: 'POST',
                            data: {
                                "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
                                carTag: { CarID: GetURLParameter(), TagID: e.detail.data.id }
                            },
                            success: function (response) {
                                if (response.success) {
                                    //$(this).attr('carCategoryId', '');
                                }
                                else {
                                    return false;
                                }
                            }
                        });
                    })
                } else {
                }
            }
        });

        var addAllSuggestionsElm;

        function onSelectSuggestion(e) {
            $.ajax({
                url: '/Admin/CarTag/Create/',
                type: 'POST',
                data: {
                    "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
                    carTag: { CarID: GetURLParameter(), TagID: $(e.detail.tagify.DOM.dropdown).find('.tagify__dropdown__item--active a').attr('id') }
                },
                success: function (response) {
                    if (response.success) {
                    } else {
                        return false;
                    }
                }
            });
        }

        // create a "add all" custom suggestion element every time the dropdown changes
        function getAddAllSuggestionsElm() {
            // suggestions items should be based on "dropdownItem" template
            return tagify.parseTemplate('dropdownItem', [{
                class: "addAll",
                name: "Add all",
                email: tagify.settings.whitelist.reduce(function (remainingSuggestions, item) {
                    return tagify.isTagDuplicate(item.value) ? remainingSuggestions : remainingSuggestions + 1
                }, 0) + " Members"
            }])
        }
    }
    return {
        // public functions
        init: function (id) {
            demo5(id);
        }
    };
}();

jQuery(document).ready(function () {

    //KTWizard2.init();

    KTTagifyCarTags.init("kt_tagify_car_tags");
    BindPackages();
    BindDocument();
    BindCarFeatures();

    $('#IsManageStock').change(function () {
        if ($(this).prop('checked')) {
            $('.managed-stock').slideDown();
            $('.unmanaged-stock').slideUp();
        } else {

            $('.managed-stock').slideUp();
            $('.unmanaged-stock').slideDown();
        }

        //$('.managed-stock').animate({ opacity: 'toggle' }, 'fast');
        //$('.unmanaged-stock').animate({ opacity: 'toggle' }, 'fast');
    });

    $('.btn-add-attribute').click(function () {
        var id = $('#AttributeID').val();
        var name = $('#AttributeID :selected').text().trim();

        if ($('#AttributeID').val()) {
            $(this).find('i').hide();
            $(this).addClass('spinner spinner-center spinner-darker-primary').prop('disabled', true);
            //AJAX CALL To SERVER

            var prod_attr = carAttributes.filter(function (obj) {
                return obj.attributeId == id
            });

            var prod_attr_stg = CarAttributeSetting.find(function (obj) {
                return obj.attributeId == id
            });

            var template = '<div class="row attribute attribute' + id + '" id="' + id + '">';
            template += '	<div class="col-xl-4">';
            template += '		<!--begin::Select-->';
            template += '		<div class="form-group">';
            template += '			<label class="form-control form-control-solid form-control-lg name">' + name + '</label>';
            template += '		</div>';
            template += '		<!--end::Select-->';
            template += '	</div>';
            template += '	<div class="col-xl-7">';
            template += '		<div class="form-group">';
            template += '			<input readonly id="kt_tagify_' + id + '" class="form-control tagify" name="attribute-values" placeholder="type..." value="" autofocus="" />';
            template += '		</div>';
            template += '	</div>';
            template += '	<div class="col-xl-1">';
            template += '		<button hidden type="button" class="btn btn-icon btn-light-danger btn-circle btn-sm float-right btn-remove-attribute" onclick="DeleteAttribute(this)">';
            template += '			<i class="flaticon2-delete"></i>';
            template += '		</button>';
            template += '	</div>';
            if (prod_attr_stg) {
                template += '	<div class="col-xl-12 attribute-setting" id="' + prod_attr_stg.id + '">';
                template += '		<div class="form-group">';
                template += '			<span class="switch ml-1">';
                template += '				<label>';
                template += '					<input disabled="true" type="checkbox" name="CarPageVisiblity" onchange="UpdateCarAttributeSetting(this)" value="" ' + (prod_attr_stg.CarPageVisiblity ? 'checked' : '') + ' />';
                template += '					<span></span>';
                template += '				</label> Visible on the motor page';
                template += '			</span>';
                template += '		</div>'
                template += '		<div class="form-group">';
                template += '			<span class="switch ml-1">';
                template += '				<label>';
                template += '					<input disabled="true" type="checkbox" name="VariationUsage" onchange="UpdateCarAttributeSetting(this)" value="" ' + (prod_attr_stg.VariationUsage ? 'checked' : '') + '/>';
                template += '					<span></span>';
                template += '				</label> Used for variations';
                template += '			</span>';
                template += '		</div>';
                template += '	</div>';
            } else {
                template += '	<div class="col-xl-12 attribute-setting">';
                template += '		<div class="form-group">';
                template += '			<span class="switch ml-1">';
                template += '				<label>';
                template += '					<input type="checkbox" name="CarPageVisiblity" onchange="UpdateCarAttributeSetting(this)" value=""  />';
                template += '					<span></span>';
                template += '				</label> Visible on the motor page';
                template += '			</span>';
                template += '		</div>'
                template += '		<div class="form-group">';
                template += '			<span class="switch ml-1">';
                template += '				<label>';
                template += '					<input type="checkbox" name="VariationUsage" onchange="UpdateCarAttributeSetting(this)" value=""/>';
                template += '					<span></span>';
                template += '				</label> Used for variations';
                template += '			</span>';
                template += '		</div>';
                template += '	</div>';
            }
            template += '	<div class="separator separator-dashed my-1"></div>';
            template += '</div>';

            $(template).prependTo($('.car-attributes')).slideDown("fast");
            KTTagifyDemos.init(id);

            $(this).removeClass('spinner spinner-center spinner-darker-primary').prop('disabled', false);
            $(this).find('i').show();

            $('option:selected', $('#AttributeID')).remove();
            if (!prod_attr_stg) {
                $.ajax({
                    url: '/Admin/CarAttributeSetting/Create/',
                    type: 'POST',
                    data: {
                        "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
                        carAttributeSetting: {
                            CarID: GetURLParameter(),
                            AttributeID: id,
                            CarPageVisiblity: $('.attribute' + id).find('input[name=CarPageVisiblity]').prop('checked'),
                            VariationUsage: $('.attribute' + id).find('input[name=VariationUsage]').prop('checked'),
                        }
                    },
                    success: function (response) {
                        if (response.success) {
                            $('.attribute' + id).find('.attribute-setting').attr('id', response.data);
                            CarAttributeSetting.push({
                                id: response.data,
                                attributeId: id,
                                CarPageVisiblity: false,
                                VariationUsage: false
                            });
                        }
                        else {
                            return false;
                            if ($('.attribute' + id).find('input[name=CarPageVisiblity]').prop('checked')) {
                                $('.attribute' + id).find('input[name=CarPageVisiblity]').prop('checked', false);
                            } else {
                                $('.attribute' + id).find('input[name=CarPageVisiblity]').prop('checked', true);
                            }

                            if ($('.attribute' + id).find('input[name=VariationUsage]').prop('checked')) {
                                $('.attribute' + id).find('input[name=VariationUsage]').prop('checked', false);
                            } else {
                                $('.attribute' + id).find('input[name=VariationUsage]').prop('checked', true);
                            }
                        }
                    }
                });
            }

        } else {
            jQuery.ajaxSetup({ cache: false });
            jQuery('#AttributeModal').modal({}, 'show');
        }
    });

    $('#CarType').change(function () {
       
        if ($(this).val() == "1") {
            
            $('.variable-car').slideUp();
            $('.simple-car').slideDown();
            $('#wizard-step-general').click();
        } else if ($(this).val() == "2") {
            
            $('.variable-car').slideDown();
            $('.simple-car').slideUp();
            $('#wizard-step-inventory').click();
        }
    });

    $('.btn-schedule').click(function () {
        if ($(this).text() == "Schedule") {
            $(this).closest('.variat').find('.variat-sale-price-dates').slideDown();
            $(this).text('Cancel');

        } else {
            $(this).closest('.variat').find('.variat-sale-price-dates').slideUp();
            $(this).text('Schedule');
        }
    });

    $('.variat-manage-stock').change(function () {
        if ($(this).prop('checked')) {
            $(this).closest('.variat').find('.managed-stock').slideDown();
            $(this).closest('.variat').find('.unmanaged-stock').slideUp();
        } else {

            $(this).closest('.variat').find('.managed-stock').slideUp();
            $(this).closest('.variat').find('.unmanaged-stock').slideDown();
        }

        //$('.managed-stock').animate({ opacity: 'toggle' }, 'fast');
        //$('.unmanaged-stock').animate({ opacity: 'toggle' }, 'fast');
    });

   // BindCarCategories();
    BindCarInspections();

    function BindCarFeatures() {

        $.ajax({
            url: '/Admin/CarFeature/GetCarFeatures/' + GetURLParameter(),
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    $('.car-features').html('');

                    $(response.categories).each(function (k, v) {
                        //console.log("Features", v.name);
                        $('.car-features').append('<div class="car-feature car-feature-plain mb-1 ' + v.name + '" id="car-feature' + v.id + '">' +
                            '<label class="checkbox">' +
                            '<input type="checkbox" disabled name="chkFeature' + v.id + '" id="chkFeature' + v.id + '" data="' + v.id + '"/>' +
                            '<span></span>' +
                            v.name +
                            '</label>' +

                            '</div>');
                    });
                    $(response.carCategories).each(function (k, v) {
                        $("#chkFeature" + v.FeatureID).prop('checked', true).attr('FeatureID', v.id);

                        $("#chkFeature" + v.FeatureID).closest('.checkbox').addClass('active');

                    });

                    //$('.car-feature-plain').each(function (k, v) {
                    //    if ($(this).attr("parent-id") != "null") {
                    //        /*$(this).appendTo("#car-feature" + $(this).attr("parent-id")).find('.sub-feature');*/

                    //        var html = $(this).get(0);
                    //        $("#car-feature" + $(this).attr("parent-id")).find('.sub-feature:first').append(html);
                    //        /*$(this).remove();*/
                    //        $(this).removeClass("car-feature-plain").addClass("car-feature");
                    //    }
                    //});

                    //$('.car-feature input[type="checkbox"]').change(function () {

                    //    var chkfeature = $(this);
                    //    $(chkfeature).closest('.checkbox').addClass('spinner spinner-dark spinner-right');
                    //    $(chkfeature).prop('disabled', true);
                    //    if ($(chkfeature).prop('checked')) {
                    //        $(this).closest('label').addClass('active');
                    //        $.ajax({
                    //            url: '/Admin/CarFeature/Create/',
                    //            type: 'POST',
                    //            data: {
                    //                "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
                    //                carFeature: { CarID: GetURLParameter(), FeatureID: $(this).attr('data') }
                    //            },
                    //            success: function (response) {
                    //                if (response.success) {
                    //                    $(chkfeature).attr('FeatureID', response.data);
                    //                    toastr.success(response.message);
                    //                } else {
                    //                    $(chkfeature).prop('checked', false);
                    //                    toastr.error(response.message);
                    //                    return false;
                    //                }
                    //                $(chkfeature).closest('.checkbox').removeClass('spinner spinner-dark spinner-right');
                    //                $(chkfeature).prop('disabled', false);
                    //            },
                    //            error: function (e) {

                    //                $(chkfeature).closest('.checkbox').removeClass('spinner spinner-dark spinner-right');
                    //                $(chkfeature).prop('disabled', false);
                    //                toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                    //            },
                    //            failure: function (e) {

                    //                $(chkfeature).closest('.checkbox').removeClass('spinner spinner-dark spinner-right');
                    //                $(chkfeature).prop('disabled', false);
                    //                toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                    //            }

                    //        });
                    //    } else {
                    //        $(this).closest('label').removeClass('active');
                    //        $.ajax({
                    //            url: '/Admin/CarFeature/Delete/' + $(chkfeature).attr('FeatureID'),
                    //            type: 'POST',
                    //            data: {
                    //                "__RequestVerificationToken":
                    //                    $("input[name=__RequestVerificationToken]").val()
                    //            },
                    //            success: function (response) {
                    //                if (response.success) {
                    //                    $(chkfeature).attr('FeatureID', '');
                    //                    toastr.success(response.message);
                    //                } else {
                    //                    $(chkfeature).prop('checked', true);
                    //                    toastr.error(response.message);
                    //                    return false;
                    //                }
                    //                $(chkfeature).closest('.checkbox').removeClass('spinner spinner-dark spinner-right');
                    //                $(chkfeature).prop('disabled', false);
                    //            },
                    //            error: function (e) {
                    //                $(chkfeature).closest('.checkbox').removeClass('spinner spinner-dark spinner-right');
                    //                $(chkfeature).prop('disabled', false);
                    //                toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                    //            },
                    //            failure: function (e) {
                    //                $(chkfeature).closest('.checkbox').removeClass('spinner spinner-dark spinner-right');
                    //                $(chkfeature).prop('disabled', false);
                    //                toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                    //            }
                    //        });
                    //    }
                    //});

                } else {
                    $('.car-features').html('No Features!');
                }
            }
        });
    }

    function BindPackages() {

        $.ajax({
            url: '/Admin/Car/GetCarPackages/' + GetURLParameter(),
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    var html = "";
                    $('.car-packages').html('');
                    $(response.packages).each(function (k, v) {

                        $('#tBodyCarPackages').append(`<tr class="car-packages  car-packages-plain " id="car-packages${v.id}">
												<td>
                                                    <span class="switch">
                                                         <label>
                                                             <input type="checkbox" disabled class="car-Package" name="chkcarpackage" id="car-package${v.id}" value="${v.id}">
                                                             <span class=""></span>
                                                         </label> 
                                                     </span>
                                                </td>
												<td> <span class=""><b>${v.name}</b></span> </td>
												<td> <input type="number" disabled class="form-control price" name="Price" onchange="getPrice(this)" id="packagePrice${v.id}" data="${v.id}"/></td>
												<td> <input type="number" disabled class="form-control kilometer" name="Kilometer"  onchange="getPrice(this)" id="packageKilomerters${v.id}" data="${v.id}"/></td>
											</tr>`);

                        //html += `<div class="car-packages row car-packages-plain mt-10" id="car-packages${v.id}">
                        //            <span class="switch ml-1">
                        //                 <label>
                        //                     <input type="checkbox" class="car-Package" name="chkcarpackage" id="car-package${v.id}" value="${v.id}">
                        //                     <span class="ml-10"></span>
                        //                 </label> 
                        //             </span>
                        //             <div class="col-md-3 pl-18">
                        //                <span class="pl-19"><b>${v.name}</b></span>                                    
                        //             </div>
                        //            <input type="number" disabled class="col-2 ml-17 form-control price" name="Price" onchange="getPrice(this)" id="packagePrice${v.id}" data="${v.id}"/>
                        //            <input type="number" disabled class="col-2 ml-17 form-control kilometer" name="Kilometer"  onchange="getPrice(this)" id="packageKilomerters${v.id}" data="${v.id}"/>
                        //         </div>`;
                    });

                    //$('.car-packages').append(html);

                    $(response.CarPackages).each(function (k, v) {
                        $("#car-package" + v.packageId).prop('checked', true).attr('carPackageId', v.id);
                        $("#packagePrice" + v.packageId).val(v.price);
                        $("#packageKilomerters" + v.packageId).val(v.kilometer);
                        $("#packagePrice" + v.packageId).prop('disabled', true);
                        $("#packageKilomerters" + v.packageId).prop('disabled', true);

                    });


                    //$(".price").change(function () {
                    //    
                    //    var packageID = $(this).closest('.price').find('input[name="chkcarpackage"]').val();

                    //});


                    $('.car-Package').change(function () {

                        var chkcarPackage = $(this);
                        var price = $(this).closest('.car-packages').find('input[name="Price"]').val();
                        var km = $(this).closest('.car-packages').find('input[name="Kilometer"]').val();

                        $(chkcarPackage).closest('.checkbox').addClass('spinner spinner-dark spinner-right');
                        $(chkcarPackage).prop('disabled', true);
                        if ($(chkcarPackage).prop('checked')) {
                            $.ajax({
                                url: '/Vendor/Car/AddCarPackage/',
                                type: 'POST',
                                data: {
                                    "data": { CarID: GetURLParameter(), PackageID: $(this).val(), Kilometer: km, Price: price, IsActive: true }
                                },
                                success: function (response) {
                                    if (response.success) {

                                        $(chkcarPackage).closest('.car-packages').find('input[name="Kilometer"]').attr('disabled', false);
                                        $(chkcarPackage).closest('.car-packages').find('input[name="Price"]').attr('disabled', false);
                                        toastr.success(response.message);
                                    } else {
                                        $(chkcarPackage).prop('checked', false);
                                        toastr.error(response.message);
                                        return false;
                                    }
                                    $(chkcarPackage).closest('.checkbox').removeClass('spinner spinner-dark spinner-right');
                                    $(chkcarPackage).prop('disabled', false);
                                },
                                error: function (e) {

                                    $(chkcarPackage).closest('.checkbox').removeClass('spinner spinner-dark spinner-right');
                                    $(chkcarPackage).prop('disabled', false);
                                    toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                                },
                                failure: function (e) {

                                    $(chkcarPackage).closest('.checkbox').removeClass('spinner spinner-dark spinner-right');
                                    $(chkcarPackage).prop('disabled', false);
                                    toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                                }

                            });
                        } else {
                            var price = $(this).closest('.car-packages').find('input[name="Price"]').val();
                            var km = $(this).closest('.car-packages').find('input[name="Kilometer"]').val();
                            $.ajax({
                                url: '/Vendor/Car/DeleteCarPackage/',
                                type: 'POST',
                                data: {
                                    "data": { CarID: GetURLParameter(), PackageID: $(this).val(), Kilometer: km, Price: price, IsActive: false }
                                },
                                success: function (response) {
                                    if (response.success) {
                                        $(chkcarPackage).attr('carPackageId', '');
                                        toastr.success(response.message);
                                    } else {
                                        $(chkcarPackage).prop('checked', true);
                                        toastr.error(response.message);
                                        return false;
                                    }
                                    $(chkcarPackage).closest('.checkbox').removeClass('spinner spinner-dark spinner-right');
                                    $(chkcarPackage).prop('disabled', false);
                                },
                                error: function (e) {
                                    $(chkcarPackage).closest('.checkbox').removeClass('spinner spinner-dark spinner-right');
                                    $(chkcarPackage).prop('disabled', false);
                                    toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                                },
                                failure: function (e) {
                                    $(chkcarPackage).closest('.checkbox').removeClass('spinner spinner-dark spinner-right');
                                    $(chkcarPackage).prop('disabled', false);
                                    toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
                                }
                            });
                        }
                    });

                } else {
                    $('.car-Package').html('No Packages!');
                }
            }
        });
    }

    function BindDocument() {

        $.ajax({
            url: '/Admin/Car/GetDocuments/' + GetURLParameter(),
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    var html = "";
                    $('.car-documents').html('');
                    $(response.document).each(function (k, v) {


                        html += `<div class="row mt-1 ${v.id}">
                                 <div class="col-10">
                                     <label class="mt-2"><i class="fa fa-file-contract mr-2"></i><b>${v.name}</b></label>
                                 </div>
                                 <div class="col-2">
                                     <a href="${v.path}" class="btn btn-bg-secondary  btn-sm" target="blank"> <span class="fas fa-eye">View</span></a>
                                 </div>
                                
                             </div> `;
                    });

                    $('.car-documents').append(html);




                    //$(".price").change(function () {
                    //    
                    //    var packageID = $(this).closest('.price').find('input[name="chkcarpackage"]').val();

                    //});




                } else {
                    $('.car-Package').html('No Packages!');
                }
            }
        });
    }


    BindCarImages();

    $('#kt_image_car_image #Image').change(function () {
        var data = new FormData();
        var files = $("#kt_image_car_image #Image").get(0).files;
        if (files.length > 0) {
            data.append("Image", files[0]);
            $.ajax({
                url: "/Vendor/Car/Thumbnail/" + GetURLParameter(),
                type: "POST",
                processData: false,
                contentType: false,
                data: data,
                success: function (response) {
                    if (response.success) {
                        $('#kt_image_car_image .image-input-wrapper').css('background-image', 'url(' + response.data + ')');
                    } else {
                        toastr.error(response.message);
                    }
                },
                error: function (er) {
                    toastr.error(er);
                }
            });
        }
    });

    $('#btn-gallery-image-upload').click(function () {
        $('input[name=GalleryImages]').trigger('click');
    });

    $('input[name=GalleryImages]').change(function () {
        var data = new FormData();
        var files = $("input[name=GalleryImages]").get(0).files;
        if (files.length > 0) {
            $.each(files, function (j, file) {
                data.append('Image[' + j + ']', file);
            })
            //data.append("Image", files);
            data.append("count", $('.car-gallery-images .symbol').length);

            $.ajax({
                url: "/Admin/CarImage/Create/" + GetURLParameter(),
                type: "POST",
                processData: false,
                contentType: false,
                data: data,
                success: function (response) {
                    if (response.success) {
                        $(response.data).each(function (k, v) {
                            $('.car-gallery-images').append('<div class="symbol symbol-70 flex-shrink-0 mr-5 mb-3">' +
																	'<span class="btn btn-xs btn-icon btn-circle btn-danger btn-hover-text-primary btn-shadow btn-remove-gallery-image" data-action="cancel" data-toggle="tooltip" title="remove" onclick="DeleteGalleryImage(this,' + v.Key + ')">' +
																		'<i class="icon-xs ki ki-bold-close ki-bold-trash"></i>' +
																	'</span>' +
																	'<div class="symbol-label" style="background-image: url(\'' + v.Value + '\')"></div>' +
																'</div>');
                        });
                        console.log(response);
                        $('#kt_image_car_image .image-input-wrapper').css('background-image', 'url(' + response.data + ')');
                    }
                },
                error: function (er) {
                    toastr.error(er);
                }
            });
        }

    });

    BindCarAttributeSetting();

    $('.btn-add-variation').click(function () {
        var id = $('#AttributeID').val();
        var name = $('#AttributeID :selected').text().trim();
        var elem = $(this);
        $(this).find('i').hide();
        $(this).addClass('spinner spinner-center spinner-darker-primary').prop('disabled', true);
        //AJAX CALL To SERVER

        var prod_attr = carAttributes.filter(function (obj) {
            return obj.attributeId == id
        });

        var prod_attr_stg = CarAttributeSetting.find(function (obj) {
            return obj.attributeId == id
        });

        $.ajax({
            url: '/Admin/CarVariation/Create/',
            type: 'POST',
            data: {
                "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
                carVariation: {
                    CarID: GetURLParameter()
                }
            },
            success: function (response) {
                if (response.success) {
                    BindCarVariation(response.data);
                }
                else {
                    toastr.error(response.message);
                }
                $(elem).removeClass('spinner spinner-center spinner-darker-primary').prop('disabled', false);
                $(elem).find('i').show();
                //$(this).removeClass('spinner spinner-center spinner-darker-primary').prop('disabled', false);
                //$(this).find('i').show();
            }
        });
    });

    $('#AttributeForm').submit(function () {
        $("#btnAddNewAttribute").addClass('spinner spinner-dark').prop('disabled', true);

        $.ajax({
            url: '/Admin/Attribute/Create/',
            type: 'POST',
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    $('#AttributeID').append($("<option />").val(response.data.id).text(response.data.name));
                    toastr.success(response.message);
                    $("#AttributeForm").trigger("reset");

                    jQuery('#AttributeModal').modal('hide');
                    $('#AttributeID').val(response.data.id);
                    $('.btn-add-attribute').trigger('click');
                }
                else {
                    toastr.error(response.message);
                }
                $("#btnAddNewAttribute").removeClass('spinner spinner-dark spinner-right').prop('disabled', false);
                $("#btnAddNewAttribute").find('i').show();
            }
        });
        return false;
    });

    $('#FormVariationAttribute').submit(function () {
        $("#btnAddNewVariat").addClass('spinner spinner-dark').prop('disabled', true);

        var temp = [];
        $('.form-control-variation-attribute').each(function (k, v) {
            temp.push($(v).val());
        });

        $.ajax({
            url: '/Admin/Attribute/Create/',
            type: 'POST',
            data: {
                "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
                carAttributeSetting: {
                    CarID: GetURLParameter(),
                    CarVariantID: $(this).find('input[name=CarVariantID]').val(),
                    Attributes: temp,
                }
            },
            success: function (response) {
                if (response.success) {
                    $('#AttributeID').append($("<option />").val(response.data.id).text(response.data.name));
                    toastr.success(response.message);
                    $("#AttributeForm").trigger("reset");

                    jQuery('#AttributeModal').modal('hide');
                    $('#AttributeID').val(response.data.id);
                    $('.btn-add-attribute').trigger('click');
                }
                else {
                    toastr.error(response.message);
                }
                $("#btnAddNewAttribute").removeClass('spinner spinner-dark spinner-right').prop('disabled', false);
                $("#btnAddNewAttribute").find('i').show();
            }
        });
        return false;
    });



});

/*Car Categories*/


function UpdateCarAttributeSetting(elem) {
    var record = $(elem).closest('.attribute-setting').attr('id');
    $(elem).addClass('spinner spinner-dark spinner-right').prop('disabled', true);
    $(elem).find('i').hide();
    $.ajax({
        url: '/Admin/CarAttributeSetting/Update/',
        type: 'POST',
        data: {
            "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
            carAttributeSetting: {
                ID: record,
                CarID: GetURLParameter(),
                AttributeID: $(elem).closest('.attribute').attr('id'),
                CarPageVisiblity: $('#' + record + '.attribute-setting').find('input[name=CarPageVisiblity]').prop('checked'),
                VariationUsage: $('#' + record + '.attribute-setting').find('input[name=VariationUsage]').prop('checked'),
            }
        },
        success: function (response) {
            if (response.success) {
            }
            else {
                return false;
            }
            $(elem).removeClass('spinner spinner-dark spinner-right').prop('disabled', false);
            $(elem).find('i').show();
        }
    });
}
function BindCarInspections() {

    $.ajax({
        url: '/Vendor/CarInspection/GetInspections/' + GetURLParameter(),
        type: 'GET',
        success: function (response) {
            if (response.success) {
                var html = "";
                $('.car-inspections').html('');

                $(response.document).each(function (k, v) {

                    html += `<div class="row mt-1 ${v.id}">
                        <div class="col-10">
                            <label class="mt-2"><i class="fa fa-file-contract mr-2"></i><b>${v.name}</b></label>
                        </div>
                        <div class="col-2">
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
function BindCarImages() {
    $.ajax({
        url: '/Admin/CarImage/GetCarImages/' + GetURLParameter(),
        type: 'GET',
        success: function (response) {
            if (response.success) {
                $('.car-gallery-images').html('');
                $(response.carImages).each(function (k, v) {
                    $('.car-gallery-images').append('<div class="symbol symbol-70 flex-shrink-0 mr-5 mb-3">' +
																	'<span  data-action="cancel" data-toggle="tooltip" title="remove" onclick="DeleteGalleryImage(this,' + v.id + ')">' +
																		'' +
																	'</span>' +
                        '<div class="symbol-label car-image" data-id=' + k +' style="background-image: url(\'' + v.Image + '\')"></div>' +
												'</div>');
                });
            } else {
            }
        }
    });
}

function DeleteGalleryImage(elem, record) {
    $(elem).addClass('spinner spinner-dark spinner-right').prop('disabled', true);
    $(elem).find('i').hide();
    $.ajax({
        url: '/Admin/CarImage/Delete/' + record,
        type: 'POST',
        data: {
            "__RequestVerificationToken":
				$("input[name=__RequestVerificationToken]").val()
        },
        success: function (response) {
            if (response.success) {
                $(elem).closest('.symbol').remove();
            }
            else {
                return false;
                $(elem).removeClass('spinner spinner-dark spinner-right').prop('disabled', false);
                $(elem).find('i').show();
            }
        }
    });
}

function DeleteAttribute(elem) {
    $(elem).find('i').hide();
    $(elem).addClass('spinner spinner-center spinner-darker-danger').prop('disabled', true);
    //AJAX CALL To SERVER

    var Attribute = $(elem).closest('.attribute');
    var id = $(Attribute).attr('id');
    var name = $(Attribute).find('.name').text().trim();
    var settingId = $(Attribute).find('.attribute-setting').attr('id')
    $.ajax({
        url: '/Admin/CarAttribute/DeleteAll/',
        type: 'POST',
        data: {
            "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
            carAttribute: {
                CarID: GetURLParameter(),
                AttributeID: id,
            }
        },
        success: function (response) {
            if (response.success) {
                $(Attribute).slideUp().remove();
            }
            else {
                return false;
            }
        }
    });

    $.ajax({
        url: '/Admin/CarAttributeSetting/Delete/' + settingId,
        type: 'POST',
        data: {
            "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val()
        },
        success: function (response) {
            if (response.success) {
                if (CarAttributeSetting) {
                    CarAttributeSetting = CarAttributeSetting.filter(function (obj) {
                        return obj.id != settingId
                    });
                }
            }
            else {
                return false;
            }
        }
    });
}

function BindCarAttributeSetting() {
    $.ajax({
        url: '/Admin/CarAttributeSetting/GetCarAttributeSetting/' + GetURLParameter(),
        type: 'GET',
        success: function (response) {
            if (response.success) {
                CarAttributeSetting = response.carAttributeSetting;
                BindCarAttributes();
            } else {
            }
        }
    });
}

function BindCarAttributes() {
    $.ajax({
        url: '/Admin/CarAttribute/GetCarAttributes/' + GetURLParameter(),
        type: 'GET',
        success: function (response) {
            if (response.success) {
                carAttributes = response.carAttributes;

                var distinctAttributes = [...new Set(CarAttributeSetting.map(x=>x.attributeId))];
                $(distinctAttributes).each(function (k, v) {
                    $('#AttributeID').val(v);
                    $('.btn-add-attribute').click();
                });

                GetCarVariations();
            } else {
            }
        }
    });
}

function BindVariationAttributes() {
    var variationAttributes = CarAttributeSetting.filter(function (obj) {
        return obj.VariationUsage == true;
    });

    $(variationAttributes).each(function (k, v) {

        var template = '<div class="col-xl-4 variation-attribute variation-attribute' + v.attributeId + '" id="' + v.attributeId + '">';
        template += '				<label class="form-control form-control-solid form-control-lg name">' + v.attributeName + '</label>';

        template += '			<select class="form-control" id="variation-attribute-value' + v.attributeId + '">';
        template += '			</select>';
        template += '	</div>';


        $('.variation-attributes-form').append(template);

        var prod_attr = carAttributes.filter(function (obj) {
            return obj.attributeId == v.attributeId
        });

        console.log(prod_attr);
        $("#variation-attribute-value" + v.attributeId).append($("<option />").val('').text("Select " + v.attributeName));
        $(prod_attr).each(function (k, v) {
            $("#variation-attribute-value" + v.attributeId).append($("<option />").val(v.id).text(v.value));
        });

        //KTTagifyCarVariationAttribute.init(v.attributeId);

    });
}

function BindCarVariationAttributes() {
    $.ajax({
        url: '/Admin/CarVariationAttribute/GetCarVariationAttributes/' + GetURLParameter(),
        type: 'GET',
        success: function (response) {
            if (response.success) {
                carVariationAttributes = response.carVariationAttributes;
                var variationAttributes = CarAttributeSetting.filter(function (obj) {
                    return obj.VariationUsage == true;
                });

                var distinctAttributes = [...new Set(variationAttributes.map(x=>x.attributeId))];
                $(distinctAttributes).each(function (k, v) {
                    //$('#AttributeID').val(v);
                    //$('.btn-add-attribute').click();


                    var prod_attr = carAttributes.filter(function (obj) {
                        return obj.attributeId == v
                    });
                    if (prod_attr) {
                        var template = '<div class="row variation-attribute variation-attribute' + prod_attr.id + '" id="' + prod_attr.id + '">';
                        template += '		<div class="col-xl-4">';
                        template += '			<!--begin::Select-->';
                        template += '			<div class="form-group">';
                        template += '				<label class="form-control form-control-solid form-control-lg name">' + prod_attr.name + '</label>';
                        template += '			</div>';
                        template += '			<!--end::Select-->';
                        template += '		</div>';
                        template += '		<div class="col-xl-8">';
                        template += '			<article >';
                        template += '				<input id="kt_tagify_Variation_Attribute' + prod_attr.id + '" class="customLook form-control tagify" name="variation-attribute" placeholder="type..." value="" autofocus="" />';
                        template += '				<button type="button">+</button>';
                        template += '			</article>';
                        template += '		</div>';
                        template += '	</div>';

                        $('.variation-attributes-form').append(template);
                        KTTagifyCarVariationAttribute.init(prod_attr.id);
                    }
                });
            } else {
            }
        }
    });
}

//function BindCarCategories() {
//    $.ajax({
//        url: '/Admin/CarCategory/GetCarCategories/' + GetURLParameter(),
//        type: 'GET',
//        success: function (response) {
//            if (response.success) {
//                $('.car-categories').html('');
//                $(response.categories).each(function (k, v) {
//                    $('.car-categories').append('<div class="car-category car-category-plain"  id="car-category' + v.id + '" parent-id="' + v.ParentId + '">' +
//														'<label class="checkbox">' +
//															'<input type="checkbox" disabled name="chkCategory' + v.id + '" id="chkCategory' + v.id + '" data="' + v.id + '"/>' +
//															'<span></span>' +
//															v.name +
//														'</label>' +
//														(v.ParentId ? '<div class="sub-category pl-10"></div>' : '') +
//													'</div>');
//                });


//                $(response.carCategories).each(function (k, v) {
//                    $("#chkCategory" + v.categoryId).prop('checked', true).attr('carCategoryId', v.id);
//                });

//                $('.car-category-plain').each(function (k, v) {
//                    if ($(this).attr("parent-id") != "null") {
//                        var html = $(this).get(0);
//                        $("#car-category" + $(this).attr("parent-id")).find('.sub-category').append(html);
//                        //$(this).remove();
//                        $(this).removeClass("car-category-plain").addClass("car-category");
//                    }
//                });




//            } else {
//            }
//        }
//    });
//}

function GetCarVariations() {
    $.ajax({
        url: '/Admin/CarVariation/GetCarVariation/' + GetURLParameter(),
        type: 'GET',
        success: function (response) {
            if (response.success) {
                $(response.carVariation).each(function (k, v) {
                    BindCarVariation(v.ID, v);
                });

                $('.car-variations .spinner').remove();

                BindVariationAttributes();
                //BindCarVariationAttributes();
            } else {

            }
        }
    });
}

function BindCarVariation(id, data) {
    var template = '<div class="accordion accordion-light accordion-toggle-plus accordion-light-borderless accordion-svg-toggle" id="accordionVariant' + id + '">';
    template += '	<div class="card">';
    template += '		<div class="card-header" id="headingVariant' + id + '">';
    template += '			<div class="card-title" data-toggle="collapse" data-target="#collapseVariant' + id + '">';
    template += '				<div class="card-label pl-4">';
    template += (data && data.SKU ? data.SKU : 'Set New Variation');
    template += '				</div>';
    template += '			</div>';
    template += '		</div>';
    template += '		<div id="collapseVariant' + id + '" class="collapse" data-parent="#accordionVariant' + id + '">';
    template += '			<div class="variat">';
    template += '				<div class="variation-attributes"></div>';
    template += '				<div class="form-group row">';
    template += '					<div class="col-lg-6 text-center">';
    template += '						<div class="image-input image-input-outline" id="kt_image_variat">';
    template += '							<div class="image-input-wrapper" style="background-image: url(' + (data && data.Thumbnail ? data.Thumbnail : '\'../../../../Assets/AppFiles/Images/default.png\'') + ')"></div>';
    template += '							<label hidden class="btn btn-xs btn-icon btn-circle btn-white btn-hover-text-primary btn-shadow" data-action="change" data-toggle="tooltip" title="" data-original-title="Change logo">';
    template += '								<i class="fa fa-pen icon-sm text-muted"></i>';
    template += '								<input type="file" name="V_Image" id="Image" accept=".png, .jpg, .jpeg" />';
    template += '								<input type="hidden" name="V_profile_avatar_remove" />';
    template += '								<input type="hidden" name="V_Thumbnail" value="' + (data && data.Thumbnail ? data.Thumbnail : '') + '"/>';
    template += '							</label>';
    template += '							<span class="btn btn-xs btn-icon btn-circle btn-white btn-hover-text-primary btn-shadow cancelimage" data-action="cancel" data-toggle="tooltip" title="Cancel avatar">';
    template += '								<i class="ki ki-bold-close icon-xs text-muted"></i>';
    template += '							</span>';
    template += '						</div>';
    template += '						<span hidden class="form-text text-muted">Allowed file types: png, jpg, jpeg.</span>';
    template += '					</div>';
    template += '					<div class="col-xl-6">';
    template += '						<!--begin::Input-->';
    template += '						<div class="form-group">';
    template += '							<label>SKU</label>';
    template += '							<input readonly type="text" class="form-control form-control-solid form-control-lg" name="V_variantSKU" placeholder="sku" value="' + (data && data.SKU ? data.SKU : '') + '" />';
    template += '							<span hidden class="form-text text-muted">Please enter variat sku.</span>';
    template += '						</div>';
    template += '						<!--end::Input-->';
    template += '					</div>';
    template += '				</div>';
    template += '				<div class="row">';
    template += '					<div class="col-md-4">';
    template += '						<div class="form-group">';
    template += '							<span class="switch ml-1">';
    template += '								<label>';
    template += '									<input disabled="true" type="checkbox" name="V_Enabled" value="" ' + (data && data.IsActive == true ? 'checked' : '') + '/>';
    template += '									<span></span>';
    template += '								</label> Enabled';
    template += '							</span>';
    template += '						</div>';
    template += '					</div>';
    template += '					<div class="col-md-4">';
    template += '						<div class="form-group">';
    template += '							<span class="switch ml-1">';
    template += '								<label>';
    template += '									<input disabled="true" type="checkbox" name="V_SoldIndividually" value="" ' + (data && data.SoldIndividually == true ? 'checked' : '') + '/>';
    template += '									<span></span>';
    template += '								</label> Sold Individually';
    template += '							</span>';
    template += '						</div>';
    template += '					</div>';
    template += '					<div class="col-md-4">';
    template += '						<div class="form-group">';
    template += '							<span class="switch ml-1">';
    template += '								<label>';
    template += '									<input disabled="true" type="checkbox" onchange="ToggleVariatStockManagement(this)" name="V_IsManageStock" value="" ' + (data && data.IsManageStock == true ? 'checked' : '') + '/>';
    template += '									<span></span>';
    template += '								</label> Manage Stock';
    template += '							</span>';
    template += '						</div>';
    template += '					</div>';
    template += '				</div>';
    template += '				<div class="row">';
    template += '					<div class="col-xl-6">';
    template += '						<!--begin::Input-->';
    template += '						<div class="form-group">';
    template += '							<label>Regular Price</label>';
    template += '							<input readonly type="number" step=".01" min="0" class="form-control form-control-solid form-control-lg" name="V_RegularPrice" placeholder="Regular Price" value="' + (data && data.RegularPrice ? data.RegularPrice : '') + '" />';
    template += '							<span class="form-text text-muted"></span>';
    template += '						</div>';
    template += '						<!--end::Input-->';
    template += '					</div>';
    template += '					<div class="col-xl-6">';
    template += '						<!--begin::Input-->';
    template += '						<div class="form-group">';
    template += '							<label>Sale Price</label>';
    template += '							<input readonly type="number" step=".01" min="0" class="form-control form-control-solid form-control-lg" name="V_SalePrice" placeholder="Sale Price" value="' + (data && data.SalePrice ? data.SalePrice : '') + '" />';
    template += '							<a href="javascript:;" class="float-right form-text text-muted mt-1 toggle-variat-scheduled-sale" onclick="ToggleVariatScheduledSale(this)">' + (data && data.SalePriceFrom ? 'Schedule' : 'Cancel') + '</a>';
    template += '						</div>';
    template += '						<!--end::Input-->';
    template += '					</div>';
    template += '					<div class="col-12">';
    template += '						<div class="form-group variat-sale-price-dates" style="display:none">';
    template += '							<label>Sale Price Dates</label>';
    template += '							<div class="input-daterange input-group kt_datepicker_range">';
    template += '								<input disabled="true" type="text" class="form-control form-control-solid form-control-lg" name="V_startDate" required="required" placeholder="FROM... MM/DD/YYYY"  value="' + (data && data.SalePriceFrom ? GetDate(data.SalePriceFrom) : '') + '"/>';
    template += '								<div class="input-group-append">';
    template += '									<span class="input-group-text"><i class="la la-ellipsis-h"></i></span>';
    template += '								</div>';
    template += '								<input disabled="true" type="text" class="form-control form-control-solid form-control-lg" name="V_endDate" required="required" placeholder="TO... MM/DD/YYYY" value="' + (data && data.SalePriceTo ? GetDate(data.SalePriceTo) : '') + '"/>';
    template += '								<div class="input-group-append">';
    template += '									<span class="input-group-text"><i class="fa fa-calendar"></i></span>';
    template += '								</div>';
    template += '							</div>';
    template += '						</div>';
    template += '					</div>';
    template += '				</div>';
    template += '				<div class="row">';
    template += '					<div class="col-12">';
    template += '						<!--begin::Select-->';
    template += '						<div class="form-group unmanaged-stock">';
    template += '							<label>Stock Status</label>';
    template += '							<select disabled="true" name="V_StockStatus" class="form-control form-control-solid form-control-lg">';
    template += '								<option value="1" selected="selected">In Stock</option>';
    template += '								<option value="2">Out Of Stock</option>';
    template += '							</select>';
    template += '						</div>';
    template += '						<!--end::Select-->';
    template += '						<div class="row managed-stock" style="display:none">';
    template += '							<div class="col-xl-6">';
    template += '								<!--begin::Input-->';
    template += '								<div class="form-group">';
    template += '									<label>Stock Quantity</label>';
    template += '									<input type="number" min="0" class="form-control form-control-solid form-control-lg" name="V_Stock" placeholder="Stock" value="' + (data && data.Stock ? data.Stock : '') + '" />';
    template += '									<span class="form-text text-muted"></span>';
    template += '								</div>';
    template += '								<!--end::Input-->';
    template += '							</div>';
    template += '							<div class="col-xl-6">';
    template += '								<!--begin::Input-->';
    template += '								<div class="form-group">';
    template += '									<label>Low stock threshold</label>';
    template += '									<input type="number" min="0" class="form-control form-control-solid form-control-lg" name="V_StockThreshold" placeholder="Stock Threshold" value="' + (data && data.Threshold ? data.Threshold : '') + '" />';
    template += '									<span class="form-text text-muted"></span>';
    template += '								</div>';
    template += '								<!--end::Input-->';
    template += '							</div>';
    template += '						</div>';
    template += '					</div>';
    template += '				</div>';
    template += '				<!--begin::Input-->';
    template += '				<div class="form-group">';
    template += '					<label>Weight (kg)</label>';
    template += '					<input readonly type="number" step=".01" min="0" class="form-control form-control-solid form-control-lg" name="V_Weight" placeholder="Weight" value="' + (data && data.Weight ? data.Weight : '') + '" />';
    template += '					<span class="form-text text-muted"></span>';
    template += '				</div>';
    template += '				<!--end::Input-->';
    template += '				<!--begin::Input-->';
    template += '				<div class="form-group row">';
    template += '					<div class="col-12">';
    template += '						<label>Dimensions (cm)</label>';
    template += '					</div>';
    template += '					<div class="col-md-4">';
    template += '						<input readonly type="number" step=".01" min="0" class="form-control form-control-solid form-control-lg" name="V_Length" placeholder="Length" value="' + (data && data.Length ? data.Length : '') + '" />';
    template += '						<span class="form-text text-muted"></span>';
    template += '					</div>';
    template += '					<div class="col-md-4">';
    template += '						<input readonly type="number" step=".01" min="0" class="form-control form-control-solid form-control-lg" name="V_Width" placeholder="Width" value="' + (data && data.Width ? data.Width : '') + '" />';
    template += '						<span class="form-text text-muted"></span>';
    template += '					</div>';
    template += '					<div class="col-md-4">';
    template += '						<input readonly type="number" step=".01" min="0" class="form-control form-control-solid form-control-lg" name="V_Height" placeholder="Height" value="' + (data && data.Height ? data.Height : '') + '" />';
    template += '						<span class="form-text text-muted"></span>';
    template += '					</div>';
    template += '				</div>';
    template += '				<!--end::Input-->';
    template += '				<textarea  readonly class="form-control form-control-solid form-control-lg" rows="3" name="V_description">' + (data && data.Description ? data.Description : '') + '</textarea>';
    template += '				<textarea readonly class="form-control form-control-solid form-control-lg" rows="3" dir="rtl" name="V_descriptionar">' + (data && data.DescriptionAr ? data.DescriptionAr : '') + '</textarea>';
    template += '				<button hidden type="button" class="btn btn-primary font-weight-bolder text-uppercase px-9 py-4" onclick="SaveVariation(this)">Save</button>'
    template += '			</div>';
    template += '		</div>';
    template += '	</div>';
    template += '</div>';

    $('.car-variations').append(template);

    // Date Range
    $('#accordionVariant' + id).find('.kt_datepicker_range').datepicker({
        rtl: KTUtil.isRTL(),
        todayHighlight: true,
        templates: arrows,
        //format: 'dd/mm/yyyy',
    });

    if (data) {
        $('#accordionVariant' + id).find('select[name=StockStatus]').val(data.StockStatus);

        $("#accordionVariant" + id).find('input[name=IsManageStock]').trigger('change');
        $("#accordionVariant" + id).find('.toggle-variat-scheduled-sale').trigger('click');
    }
}

function SaveVariation(form) {
    var record = $(form).closest('.accordion').attr('id').replace('accordionVariant', '');
    $(form).addClass('spinner spinner-dark spinner-right').prop('disabled', true);

    var formElement = $(form).closest('.accordion');
    $.ajax({
        url: '/Admin/CarVariation/Update/',
        type: 'POST',
        data: {
            "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
            carVariation: {
                ID: record,
                Thumbnail: $(formElement).find('input[name=V_Thumbnail]').val(),
                SKU: $(formElement).find('input[name=V_variantSKU]').val(),
                CarID: GetURLParameter(),
                RegularPrice: $(formElement).find('input[name=V_RegularPrice]').val(),
                SalePrice: $(formElement).find('input[name=V_SalePrice]').val(),
                SalePriceFrom: $(formElement).find('.variat-sale-price-dates').is(":visible") ? $(formElement).find('input[name=V_startDate]').val() : null,
                SalePriceTo: $(formElement).find('.variat-sale-price-dates').is(":visible") ? $(formElement).find('input[name=V_endDate]').val() : null,
                Stock: $(formElement).find('input[name=V_IsManageStock]').prop('checked') ? $(formElement).find('input[name=V_Stock]').val() : null,
                Threshold: $(formElement).find('input[name=V_IsManageStock]').prop('checked') ? $(formElement).find('input[name=V_StockThreshold]').val() : null,
                StockStatus: !$(formElement).find('input[name=V_IsManageStock]').prop('checked') ? $(formElement).find('select[name=V_StockStatus]').val() : null,
                Weight: $(formElement).find('input[name=V_Weight]').val(),
                Length: $(formElement).find('input[name=V_Length]').val(),
                Width: $(formElement).find('input[name=V_Width]').val(),
                Height: $(formElement).find('input[name=V_Height]').val(),
                Description: $(formElement).find('textarea[name=V_description]').val(),
                DescriptionAr: $(formElement).find('textarea[name=V_descriptionar]').val(),
                IsManageStock: $(formElement).find('input[name=V_IsManageStock]').prop('checked'),
                SoldIndividually: $(formElement).find('input[name=V_SoldIndividually]').prop('checked'),
                IsActive: $(formElement).find('input[name=V_Enabled]').prop('checked'),
            }
        },
        success: function (response) {
            if (response.success) {
            }
            else {

            }
            $(form).removeClass('spinner spinner-dark spinner-right').prop('disabled', false);
            //$(form).find('i').show();
        }
    });
}

function ToggleVariatScheduledSale(e) {

    if ($(e).text() == "Schedule") {
        $(e).closest('.variat').find('.variat-sale-price-dates').slideDown();
        $(e).text('Cancel').hide();

    } else {
        $(e).closest('.variat').find('.variat-sale-price-dates').slideUp();
        $(e).text('Schedule').hide();
    }
}

function ToggleVariatStockManagement(e) {

    if ($(e).prop('checked')) {
        $(e).closest('.variat').find('.managed-stock').slideDown();
        $(e).closest('.variat').find('.unmanaged-stock').slideUp();
    } else {

        $(e).closest('.variat').find('.managed-stock').slideUp();
        $(e).closest('.variat').find('.unmanaged-stock').slideDown();
    }
}

function ToggleScheduledSale(e) {

    if ($(e).text().trim() == "Schedule") {
        $('#sale-price-dates').slideDown();
        $(e).text('Cancel').hide();

    } else {
        $('#sale-price-dates').slideUp();
        $(e).text('Schedule').hide();
    }

}

function ToggleStockManagement(e) {

    if ($(e).prop('checked')) {
        $(e).closest('.variat').find('.managed-stock').slideDown();
        $(e).closest('.variat').find('.unmanaged-stock').slideUp();
    } else {

        $(e).closest('.variat').find('.managed-stock').slideUp();
        $(e).closest('.variat').find('.unmanaged-stock').slideDown();
    }
}

function SearchCategory(elem) {
    var text = $(elem).val().trim();
    if (text && text != "") {
        $('.car-category .checkbox').hide();
        //$('.car-category .checkbox:contains("' + text + '")').css('background-color', 'red');
        $('.car-category .checkbox:contains("' + text + '")').fadeIn();
    } else {
        $('.car-category .checkbox').show();
    }
}

function SaveCar(e) {
    $(e).addClass('spinner spinner-dark spinner-right').prop('disabled', true);
    $(e).find('i').hide();

    $.ajax({
        url: '/Vendor/Car/Update/',
        type: 'POST',
        data: {
            "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
            car: {
                ID: GetURLParameter(),
                SKU: $('input[name=SKU]').val(),
                Name: $('input[name=Name]').val(),
                NameAr: $('input[name=NameAr]').val(),

                ShortDescription: EditorShortDescription.getData(),
                ShortDescriptionAr: EditorShortDescriptionAr.getData(),

                LongDescription: EditorLongDescription.getData(),
                LongDescriptionAr: EditorLongDescriptionAr.getData(),

                RegularPrice: $('input[name=RegularPrice]').val(),
                SalePrice: $('input[name=SalePrice]').val(),
                SalePriceFrom: $('#sale-price-dates').is(":visible") ? $('input[name=startDate]').val() : null,
                SalePriceTo: $('#sale-price-dates').is(":visible") ? $('input[name=endDate]').val() : null,
                Stock: $('input[name=IsManageStock]').prop('checked') ? $('input[name=Stock]').val() : null,
                Threshold: $('input[name=IsManageStock]').prop('checked') ? $('input[name=StockThreshold]').val() : null,
                StockStatus: !$('input[name=IsManageStock]').prop('checked') ? $('select[name=StockStatus]').val() : null,

                Weight: $('input[name=Weight]').val(),
                Length: $('input[name=Length]').val(),
                Width: $('input[name=Width]').val(),
                Height: $('input[name=Height]').val(),

                PurchaseNote: $('textarea[name=PurchaseNote]').val(),
                EnableReviews: $('input[name=EnableReviews]').prop('checked'),

                Type: $('select[name=CarType]').val(),
                IsManageStock: $('input[name=IsManageStock]').prop('checked'),
                IsPublished: $('input[name=IsPublished]').prop('checked'),
                IsSoldIndividually: $('input[name=IsSoldIndividually]').prop('checked'),
                Status: $('select[name=Status]').val(),
            }
        },
        success: function (response) {
            if (response.success) {
                toastr.success(response.message);
            }
            else {
                toastr.error(response.message);
            }
            $(e).removeClass('spinner spinner-dark spinner-right').prop('disabled', false);
            $(e).find('i').show();
        }
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

var KTTagifyCarVariationAttribute = function (id) {
    var tagify;
    // Private functions
    var demo1 = function (id) {
        var prod_attr = carAttributes.filter(function (obj) {
            return obj.attributeId == id
        });

        var input = document.getElementById("kt_tagify_Variation_Attribute" + id),
			tagify = new Tagify(input, {
			    whitelist: prod_attr,
			    callbacks: {
			        "invalid": onInvalidTag
			    },
			    dropdown: {
			        position: 'text',
			        enabled: 1 // show suggestions dropdown after 1 typed character
			    }
			}),
			button = input.nextElementSibling;  // "add new tag" action-button

        button.addEventListener("click", onAddButtonClick)

        function onAddButtonClick() {
            tagify.addEmptyTag()
        }

        function onInvalidTag(e) {
            console.log("invalid", e.detail)
        }

        // init Tagify script on the above inputs
        //tagify = new Tagify(input)


        //tagify.addTags(prod_attr);
        // "remove all tags" button event listener

        // Chainable event listeners
        //tagify.on('add', function (e) {
        //	$.ajax({
        //		url: '/Vendor/CarVariationAttribute/Create/',
        //		type: 'POST',
        //		data: {
        //			"__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
        //			carAttribute: {
        //				CarID: GetURLParameter(),
        //				AttributeID: $(e.detail.tag).closest('.variation-attribute').attr('id'),
        //				Value: e.detail.data.value
        //			}
        //		},
        //		success: function (response) {
        //			if (response.success) {
        //				//$(this).attr('carCategoryId', '');
        //			}
        //			else {
        //				return false;
        //			}
        //		}
        //	});
        //})
        //tagify.on('remove', function (e) {
        //	if (e.detail.data.id) {
        //		$.ajax({
        //			url: '/Vendor/CarVariationAttribute/Delete/' + e.detail.data.id,
        //			type: 'POST',
        //			data: {
        //				"__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val()
        //			},
        //			success: function (response) {
        //				if (response.success) {
        //					//$(this).attr('carCategoryId', '');
        //				}
        //				else {
        //					return false;
        //				}
        //			}
        //		});
        //	} else {
        //		$.ajax({
        //			url: '/Vendor/CarVariationAttribute/DeleteValue/',
        //			type: 'POST',
        //			data: {
        //				"__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
        //				carAttribute: {
        //					CarID: GetURLParameter(),
        //					AttributeID: $(e.detail.tagify.DOM.input).closest('.attribute').attr('id'),
        //					Value: e.detail.data.value
        //				}
        //			},
        //			success: function (response) {
        //				if (response.success) {
        //					//$(this).attr('carCategoryId', '');
        //				}
        //				else {
        //					return false;
        //				}
        //			}
        //		});
        //	}
        //});
    }

    return {
        // public functions
        init: function (id) {
            demo1(id);
        }
    };
}();

function GetDate(jsonDate) {
    var value = new Date(parseInt(jsonDate.substr(6)));
    return value.getMonth() + 1 + "/" + value.getDate() + "/" + value.getFullYear();
}
