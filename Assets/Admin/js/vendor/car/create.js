"use strict";

var carAttributes = [];
// Class definition
var KTWizard2 = function () {
	// Base elements
	var _wizardEl;
	var _formEl;
	var _wizardObj;
	var _validations = [];

	// Private functions
	var _initWizard = function () {
		// Initialize form wizard
		_wizardObj = new KTWizard(_wizardEl, {
			startStep: 1, // initial active step number
			clickableSteps: true, // to make steps clickable this set value true and add data-wizard-clickable="true" in HTML for class="wizard" element
			buttons: false

		});

		// Validation before going to next page
		_wizardObj.on('change', function (wizard) {
			if (wizard.getStep() > wizard.getNewStep()) {
				return; // Skip if stepped back
			}

			// Validate form before change wizard step
			var validator = _validations[wizard.getStep() - 1]; // get validator for currnt step

			if (validator) {
				validator.validate().then(function (status) {
					if (status == 'Valid') {
						wizard.goTo(wizard.getNewStep());

						//KTUtil.scrollTop();
					} else {
						Swal.fire({
							text: "Sorry, looks like there are some errors detected, please try again.",
							icon: "error",
							buttonsStyling: false,
							confirmButtonText: "Ok, got it!",
							customClass: {
								confirmButton: "btn font-weight-bold btn-light"
							}
						}).then(function () {
							//KTUtil.scrollTop();
						});
					}
				});
			}

			return false;  // Do not change wizard step, further action will be handled by he validator
		});

		// Change event
		_wizardObj.on('changed', function (wizard) {
			//KTUtil.scrollTop();
		});

		// Submit event
		_wizardObj.on('submit', function (wizard) {
			Swal.fire({
				text: "All is good! Please confirm the form submission.",
				icon: "success",
				showCancelButton: true,
				buttonsStyling: false,
				confirmButtonText: "Yes, submit!",
				cancelButtonText: "No, cancel",
				customClass: {
					confirmButton: "btn font-weight-bold btn-primary",
					cancelButton: "btn font-weight-bold btn-default"
				}
			}).then(function (result) {
				if (result.value) {
					_formEl.submit(); // Submit form
				} else if (result.dismiss === 'cancel') {
					Swal.fire({
						text: "Your form has not been submitted!.",
						icon: "error",
						buttonsStyling: false,
						confirmButtonText: "Ok, got it!",
						customClass: {
							confirmButton: "btn font-weight-bold btn-primary",
						}
					});
				}
			});
		});
	}

	var _initValidation = function () {
		// Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
		// Step 1
		_validations.push(FormValidation.formValidation(
			//_formEl,
			//{
			//	fields: {
			//		fname: {
			//			validators: {
			//				notEmpty: {
			//					message: 'First name is required'
			//				}
			//			}
			//		},
			//		lname: {
			//			validators: {
			//				notEmpty: {
			//					message: 'Last Name is required'
			//				}
			//			}
			//		},
			//		phone: {
			//			validators: {
			//				notEmpty: {
			//					message: 'Phone is required'
			//				}
			//			}
			//		},
			//		email: {
			//			validators: {
			//				notEmpty: {
			//					message: 'Email is required'
			//				},
			//				emailAddress: {
			//					message: 'The value is not a valid email address'
			//				}
			//			}
			//		}
			//	},
			//	plugins: {
			//		trigger: new FormValidation.plugins.Trigger(),
			//		// Bootstrap Framework Integration
			//		bootstrap: new FormValidation.plugins.Bootstrap({
			//			//eleInvalidClass: '',
			//			eleValidClass: '',
			//		})
			//	}
			//}
		));

		// Step 2
		_validations.push(FormValidation.formValidation(
			_formEl,
			{
				fields: {
					address1: {
						validators: {
							notEmpty: {
								message: 'Address is required'
							}
						}
					},
					postcode: {
						validators: {
							notEmpty: {
								message: 'Postcode is required'
							}
						}
					},
					city: {
						validators: {
							notEmpty: {
								message: 'City is required'
							}
						}
					},
					state: {
						validators: {
							notEmpty: {
								message: 'State is required'
							}
						}
					},
					country: {
						validators: {
							notEmpty: {
								message: 'Country is required'
							}
						}
					}
				},
				plugins: {
					trigger: new FormValidation.plugins.Trigger(),
					// Bootstrap Framework Integration
					bootstrap: new FormValidation.plugins.Bootstrap({
						//eleInvalidClass: '',
						eleValidClass: '',
					})
				}
			}
		));

		// Step 3
		_validations.push(FormValidation.formValidation(
			_formEl,
			{
				fields: {
					delivery: {
						validators: {
							notEmpty: {
								message: 'Delivery type is required'
							}
						}
					},
					packaging: {
						validators: {
							notEmpty: {
								message: 'Packaging type is required'
							}
						}
					},
					preferreddelivery: {
						validators: {
							notEmpty: {
								message: 'Preferred delivery window is required'
							}
						}
					}
				},
				plugins: {
					trigger: new FormValidation.plugins.Trigger(),
					// Bootstrap Framework Integration
					bootstrap: new FormValidation.plugins.Bootstrap({
						//eleInvalidClass: '',
						eleValidClass: '',
					})
				}
			}
		));

		// Step 4
		_validations.push(FormValidation.formValidation(
			_formEl,
			{
				fields: {
					locaddress1: {
						validators: {
							notEmpty: {
								message: 'Address is required'
							}
						}
					},
					locpostcode: {
						validators: {
							notEmpty: {
								message: 'Postcode is required'
							}
						}
					},
					loccity: {
						validators: {
							notEmpty: {
								message: 'City is required'
							}
						}
					},
					locstate: {
						validators: {
							notEmpty: {
								message: 'State is required'
							}
						}
					},
					loccountry: {
						validators: {
							notEmpty: {
								message: 'Country is required'
							}
						}
					}
				},
				plugins: {
					trigger: new FormValidation.plugins.Trigger(),
					// Bootstrap Framework Integration
					bootstrap: new FormValidation.plugins.Bootstrap({
						//eleInvalidClass: '',
						eleValidClass: '',
					})
				}
			}
		));

		// Step 5
		_validations.push(FormValidation.formValidation(
			_formEl,
			{
				fields: {
					ccname: {
						validators: {
							notEmpty: {
								message: 'Credit card name is required'
							}
						}
					},
					ccnumber: {
						validators: {
							notEmpty: {
								message: 'Credit card number is required'
							},
							creditCard: {
								message: 'The credit card number is not valid'
							}
						}
					},
					ccmonth: {
						validators: {
							notEmpty: {
								message: 'Credit card month is required'
							}
						}
					},
					ccyear: {
						validators: {
							notEmpty: {
								message: 'Credit card year is required'
							}
						}
					},
					cccvv: {
						validators: {
							notEmpty: {
								message: 'Credit card CVV is required'
							},
							digits: {
								message: 'The CVV value is not valid. Only numbers is allowed'
							}
						}
					}
				},
				plugins: {
					trigger: new FormValidation.plugins.Trigger(),
					// Bootstrap Framework Integration
					bootstrap: new FormValidation.plugins.Bootstrap({
						//eleInvalidClass: '',
						eleValidClass: '',
					})
				}
			}
		));
	}

	return {
		// public functions
		init: function () {
			_wizardEl = KTUtil.getById('kt_wizard');
			_formEl = KTUtil.getById('kt_form');

			_initWizard();
			_initValidation();
		}
	};
}();

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
				url: '/Vendor/CarAttribute/Create/',
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
					url: '/Vendor/CarAttribute/Delete/' + e.detail.data.id,
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
					url: '/Vendor/CarAttribute/DeleteValue/',
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

var KTTagifyCarTags = function (id) {
	var tagify;
	// Private functions
	var demo5 = function (id) {
		$.ajax({
			url: '/Vendor/CarTag/GetCarTags/' + GetURLParameter(),
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
							url: '/Vendor/CarTag/Delete/',

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
				url: '/Vendor/CarTag/Create/',
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

	KTWizard2.init();

	KTTagifyCarTags.init("kt_tagify_car_tags");

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
			template += '			<input id="kt_tagify_' + id + '" class="form-control tagify" name="attribute-values" placeholder="type..." value="" autofocus="" />';
			template += '		</div>';
			template += '	</div>';
			template += '	<div class="col-xl-1">';
			template += '		<button type="button" class="btn btn-icon btn-light-danger btn-circle btn-sm float-right btn-remove-attribute" onclick="DeleteAttribute(this)">';
			template += '			<i class="flaticon2-delete"></i>';
			template += '		</button>';
			template += '	</div>';
			if (prod_attr_stg) {
				template += '	<div class="col-xl-12 attribute-setting" id="' + prod_attr_stg.id + '">';
				template += '		<div class="form-group">';
				template += '			<span class="switch ml-1">';
				template += '				<label>';
				template += '					<input type="checkbox" name="CarPageVisiblity" onchange="UpdateCarAttributeSetting(this)" value="" ' + (prod_attr_stg.CarPageVisiblity ? 'checked' : '') + ' />';
				template += '					<span></span>';
				template += '				</label> Visible on the car page';
				template += '			</span>';
				template += '		</div>'
				template += '		<div class="form-group">';
				template += '			<span class="switch ml-1">';
				template += '				<label>';
				template += '					<input type="checkbox" name="VariationUsage" onchange="UpdateCarAttributeSetting(this)" value="" ' + (prod_attr_stg.VariationUsage ? 'checked' : '') + '/>';
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
				template += '				</label> Visible on the car page';
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

			$(template).appendTo($('.car-attributes')).slideDown("fast");
			KTTagifyDemos.init(id);

			$(this).removeClass('spinner spinner-center spinner-darker-primary').prop('disabled', false);
			$(this).find('i').show();

			$('option:selected', $('#AttributeID')).remove();
			if (!prod_attr_stg) {
				$.ajax({
					url: '/Vendor/CarAttributeSetting/Create/',
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

	BindCarCategories();

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
				url: "/Vendor/CarImage/Create/" + GetURLParameter(),
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

	GetCarVariations();

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
			url: '/Vendor/CarVariation/Create/',
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
					return false;
				}
				$(elem).removeClass('spinner spinner-center spinner-darker-primary').prop('disabled', false);
				$(elem).find('i').show();
			}
		});
	});
});

function UpdateCarAttributeSetting(elem) {
	var record = $(elem).closest('.attribute-setting').attr('id');
	$(elem).addClass('spinner spinner-dark spinner-right').prop('disabled', true);
	$(elem).find('i').hide();
	$.ajax({
		url: '/Vendor/CarAttributeSetting/Update/',
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

function BindCarImages() {
	$.ajax({
		url: '/Vendor/CarImage/GetCarImages/' + GetURLParameter(),
		type: 'GET',
		success: function (response) {
			if (response.success) {
				$('.car-gallery-images').html('');
				$(response.carImages).each(function (k, v) {
					$('.car-gallery-images').append('<div class="symbol symbol-70 flex-shrink-0 mr-5 mb-3">' +
																	'<span class="btn btn-xs btn-icon btn-circle btn-danger btn-hover-text-primary btn-shadow btn-remove-gallery-image" data-action="cancel" data-toggle="tooltip" title="remove" onclick="DeleteGalleryImage(this,' + v.id + ')">' +
																		'<i class="icon-xs ki ki-bold-close ki-bold-trash"></i>' +
																	'</span>' +
													'<div class="symbol-label" style="background-image: url(\'' + v.Image + '\')"></div>' +
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
		url: '/Vendor/CarImage/Delete/' + record,
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
		url: '/Vendor/CarAttribute/DeleteAll/',
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
		url: '/Vendor/CarAttributeSetting/Delete/' + settingId,
		type: 'POST',
		data: {
			"__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val()
		},
		success: function (response) {
			if (response.success) {
				var CarAttributeSetting = CarAttributeSetting.filter(function (obj) {
					return obj.id != settingId
				});
			}
			else {
				return false;
			}
		}
	});
}

var CarAttributeSetting = [];

function BindCarAttributeSetting() {
	$.ajax({
		url: '/Vendor/CarAttributeSetting/GetCarAttributeSetting/' + GetURLParameter(),
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
		url: '/Vendor/CarAttribute/GetCarAttributes/' + GetURLParameter(),
		type: 'GET',
		success: function (response) {
			if (response.success) {
				carAttributes = response.carAttributes;

				var distinctAttributes = [...new Set(CarAttributeSetting.map(x=>x.attributeId))];
				$(distinctAttributes).each(function (k, v) {
					$('#AttributeID').val(v);
					$('.btn-add-attribute').click();
				});
			} else {
			}
		}
	});
}

function BindCarCategories() {
	$.ajax({
		url: '/Vendor/CarCategory/GetCarCategories/' + GetURLParameter(),
		type: 'GET',
		success: function (response) {
			if (response.success) {
				$('.car-categories').html('');
				$(response.categories).each(function (k, v) {
					$('.car-categories').append('<div class="car-category car-category-plain" id="car-category' + v.id + '" parent-id="' + v.ParentId + '">' +
														'<label class="checkbox">' +
															'<input type="checkbox" name="chkCategory' + v.id + '" id="chkCategory' + v.id + '" data="' + v.id + '"/>' +
															'<span></span>' +
															v.name +
														'</label>' +
														(v.ParentId ? '<div class="sub-category pl-10"></div>' : '') +
													'</div>');
				});

				$(response.carCategories).each(function (k, v) {
					$("#chkCategory" + v.categoryId).prop('checked', true).attr('carCategoryId', v.id);
				});

				$('.car-category-plain').each(function (k, v) {
					if ($(this).attr("parent-id") != "null") {
						var html = $(this).get(0);
						$("#car-category" + $(this).attr("parent-id")).find('.sub-category').append(html);
						//$(this).remove();
						$(this).removeClass("car-category-plain").addClass("car-category");
					}
				});

				$('.car-category input[type="checkbox"]').change(function () {
					$(this).closest('.checkbox').addClass('spinner spinner-dark spinner-right');
					$(this).prop('disabled', true);
					if ($(this).prop('checked')) {
						$.ajax({
							url: '/Vendor/CarCategory/Create/',
							type: 'POST',
							data: {
								"__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
								carCategory: { CarID: GetURLParameter(), CarCategoryID: $(this).attr('data') }
							},
							success: function (response) {
								if (response.success) {
									$(this).attr('carCategoryId', response.data);
								} else {
									return false;
								}
							}
						});
					} else {
						$.ajax({
							url: '/Vendor/CarCategory/Delete/' + $(this).attr('carCategoryId'),
							type: 'POST',
							data: {
								"__RequestVerificationToken":
									$("input[name=__RequestVerificationToken]").val()
							},
							success: function (response) {
								if (response.success) {
									$(this).attr('carCategoryId', '');
								}
								else {
									return false;
								}
							}
						});
					}
					$(this).closest('.checkbox').removeClass('spinner spinner-dark spinner-right');
					$(this).prop('disabled', false);
				});
			} else {
			}
		}
	});
}

function GetCarVariations() {
	$.ajax({
		url: '/Vendor/CarVariation/GetCarVariation/' + GetURLParameter(),
		type: 'GET',
		success: function (response) {
			if (response.success) {
				$(response.carVariation).each(function (k, v) {
					BindCarVariation(v.ID, v);
				});

				$('.car-variations .spinner').remove();
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
	template += '		<div id="collapseVariant' + id + '" class="collapse show" data-parent="#accordionVariant' + id + '">';
	template += '			<div class="variat">';
	template += '				<div class="variation-attributes"></div>';
	template += '				<div class="form-group row">';
	template += '					<div class="col-lg-6 text-center">';
	template += '						<div class="image-input image-input-outline" id="kt_image_variat">';
	template += '							<div class="image-input-wrapper" style="background-image: url(' + (data & data.Thumbnail ? data.Thumbnail : '\'../../../../Assets/AppFiles/Images/default.png\'') + ')"></div>';
	template += '							<label class="btn btn-xs btn-icon btn-circle btn-white btn-hover-text-primary btn-shadow" data-action="change" data-toggle="tooltip" title="" data-original-title="Change logo">';
	template += '								<i class="fa fa-pen icon-sm text-muted"></i>';
	template += '								<input type="file" name="V_Image" id="Image" accept=".png, .jpg, .jpeg" />';
	template += '								<input type="hidden" name="V_profile_avatar_remove" />';
	template += '								<input type="hidden" name="V_Thumbnail" value="' + (data && data.Thumbnail ? data.Thumbnail : '') + '"/>';
	template += '							</label>';
	template += '							<span class="btn btn-xs btn-icon btn-circle btn-white btn-hover-text-primary btn-shadow cancelimage" data-action="cancel" data-toggle="tooltip" title="Cancel avatar">';
	template += '								<i class="ki ki-bold-close icon-xs text-muted"></i>';
	template += '							</span>';
	template += '						</div>';
	template += '						<span class="form-text text-muted">Allowed file types: png, jpg, jpeg.</span>';
	template += '					</div>';
	template += '					<div class="col-xl-6">';
	template += '						<!--begin::Input-->';
	template += '						<div class="form-group">';
	template += '							<label>SKU</label>';
	template += '							<input type="text" class="form-control form-control-solid form-control-lg" name="V_variantSKU" placeholder="sku" value="' + (data && data.SKU ? data.SKU : '') + '" />';
	template += '							<span class="form-text text-muted">Please enter variat sku.</span>';
	template += '						</div>';
	template += '						<!--end::Input-->';
	template += '					</div>';
	template += '				</div>';
	template += '				<div class="row">';
	template += '					<div class="col-md-4">';
	template += '						<div class="form-group">';
	template += '							<span class="switch ml-1">';
	template += '								<label>';
	template += '									<input type="checkbox" name="V_Enabled" value="" ' + (data && data.IsActive == true ? 'checked' : '') + '/>';
	template += '									<span></span>';
	template += '								</label> Enabled';
	template += '							</span>';
	template += '						</div>';
	template += '					</div>';
	template += '					<div class="col-md-4">';
	template += '						<div class="form-group">';
	template += '							<span class="switch ml-1">';
	template += '								<label>';
	template += '									<input type="checkbox" name="V_SoldIndividually" value="" ' + (data && data.SoldIndividually == true ? 'checked' : '') + '/>';
	template += '									<span></span>';
	template += '								</label> Sold Individually';
	template += '							</span>';
	template += '						</div>';
	template += '					</div>';
	template += '					<div class="col-md-4">';
	template += '						<div class="form-group">';
	template += '							<span class="switch ml-1">';
	template += '								<label>';
	template += '									<input type="checkbox" onchange="ToggleVariatStockManagement(this)" name="V_IsManageStock" value="" ' + (data && data.IsManageStock == true ? 'checked' : '') + '/>';
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
	template += '							<input type="number" step=".01" min="0" class="form-control form-control-solid form-control-lg" name="V_RegularPrice" placeholder="Regular Price" value="' + (data && data.RegularPrice ? data.RegularPrice : '') + '" />';
	template += '							<span class="form-text text-muted"></span>';
	template += '						</div>';
	template += '						<!--end::Input-->';
	template += '					</div>';
	template += '					<div class="col-xl-6">';
	template += '						<!--begin::Input-->';
	template += '						<div class="form-group">';
	template += '							<label>Sale Price</label>';
	template += '							<input type="number" step=".01" min="0" class="form-control form-control-solid form-control-lg" name="V_SalePrice" placeholder="Sale Price" value="' + (data && data.SalePrice ? data.SalePrice : '') + '" />';
	template += '							<a href="javascript:;" class="float-right form-text text-muted mt-1 toggle-variat-scheduled-sale" onclick="ToggleVariatScheduledSale(this)">' + (data && data.SalePriceFrom ? 'Schedule' : 'Cancel') + '</a>';
	template += '						</div>';
	template += '						<!--end::Input-->';
	template += '					</div>';
	template += '					<div class="col-12">';
	template += '						<div class="form-group variat-sale-price-dates" style="display:none">';
	template += '							<label>Sale Price Dates</label>';
	template += '							<div class="input-daterange input-group kt_datepicker_range">';
	template += '								<input type="text" class="form-control form-control-solid form-control-lg" name="V_startDate" required="required" placeholder="FROM... MM/DD/YYYY" value="' + (data && data.SalePriceFrom ? data.SalePriceFrom : '') + '"/>';
	template += '								<div class="input-group-append">';
	template += '									<span class="input-group-text"><i class="la la-ellipsis-h"></i></span>';
	template += '								</div>';
	template += '								<input type="text" class="form-control form-control-solid form-control-lg" name="V_endDate" required="required" placeholder="TO... MM/DD/YYYY" value="' + (data && data.SalePriceTo ? data.SalePriceTo : '') + '"/>';
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
	template += '							<select name="V_StockStatus" class="form-control form-control-solid form-control-lg">';
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
	template += '					<input type="number" step=".01" min="0" class="form-control form-control-solid form-control-lg" name="V_Weight" placeholder="Weight" value="' + (data && data.Weight ? data.Weight : '') + '" />';
	template += '					<span class="form-text text-muted"></span>';
	template += '				</div>';
	template += '				<!--end::Input-->';
	template += '				<!--begin::Input-->';
	template += '				<div class="form-group row">';
	template += '					<div class="col-12">';
	template += '						<label>Dimensions (cm)</label>';
	template += '					</div>';
	template += '					<div class="col-md-4">';
	template += '						<input type="number" step=".01" min="0" class="form-control form-control-solid form-control-lg" name="V_Length" placeholder="Length" value="' + (data && data.Length ? data.Length : '') + '" />';
	template += '						<span class="form-text text-muted"></span>';
	template += '					</div>';
	template += '					<div class="col-md-4">';
	template += '						<input type="number" step=".01" min="0" class="form-control form-control-solid form-control-lg" name="V_Width" placeholder="Width" value="' + (data && data.Width ? data.Width : '') + '" />';
	template += '						<span class="form-text text-muted"></span>';
	template += '					</div>';
	template += '					<div class="col-md-4">';
	template += '						<input type="number" step=".01" min="0" class="form-control form-control-solid form-control-lg" name="V_Height" placeholder="Height" value="' + (data && data.Height ? data.Height : '') + '" />';
	template += '						<span class="form-text text-muted"></span>';
	template += '					</div>';
	template += '				</div>';
	template += '				<!--end::Input-->';
	template += '				<textarea class="form-control form-control-solid form-control-lg" rows="3" name="V_description">' + (data && data.Description ? data.Description : '') + '</textarea>';
	//template += '				<textarea class="form-control form-control-solid form-control-lg" rows="3" dir="rtl" name="V_descriptionar">' + (data && data.DescriptionAr ? data.DescriptionAr : '') + '</textarea>';
	template += '				<button type="button" class="btn btn-primary font-weight-bolder text-uppercase px-9 py-4" onclick="SaveVariation(this)">Save</button>'
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
		url: '/Vendor/CarVariation/Update/',
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
				//DescriptionAr: $(formElement).find('textarea[name=V_descriptionar]').val(),
				DescriptionAr: "-",
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
		$(e).text('Cancel');

	} else {
		$(e).closest('.variat').find('.variat-sale-price-dates').slideUp();
		$(e).text('Schedule');
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
		$(e).text('Cancel');

	} else {
		$('#sale-price-dates').slideUp();
		$(e).text('Schedule');
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


function SaveCar(e) {
	$(e).addClass('spinner spinner-dark spinner-right').prop('disabled', true);
	$(e).find('i').hide();
	
    var brands= $("#BrandID option:selected").val();
	
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
				//ShortDescriptionAr: EditorShortDescriptionAr.getData(),
			    BrandID: brands,
				LongDescription: EditorLongDescription.getData(),
				//LongDescriptionAr: EditorLongDescriptionAr.getData(),

				ShortDescriptionAr: "-",
				LongDescriptionAr: "-",

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