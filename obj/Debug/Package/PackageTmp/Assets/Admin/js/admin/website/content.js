jQuery(document).ready(function () {
	var _URL = window.URL || window.webkitURL;
	
    $(".fileuploadheader").change(function (e) {
		var file, img;
		var element = this
		if ((file = this.files[0])) {
			if ($('#ContentType').val() == "Image") {
                if (this.files[0].size >= 500000) {
					element.value = '';
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image size should be equal to 500 KB and dimension should be 1836 x 546!',
						//footer: '<a href>Image size should be less than or equal to  100KB and dimension should be 1713x540</a>'
					})
                   
				}
				img = new Image();
				img.onload = function () {

                    if (this.width < 1836 || this.width > 1836) {
						element.value = '';
						Swal.fire({
							icon: 'error',
							title: 'Oops...',
							text: 'Image size should be equal to 500 KB and dimension should be 1836 x 546!',
							//  footer: '<a href>Image dimension should be 1713x540 and size should less than 100 KB</a>'
						})
                        
					}
                    else if (this.height < 546 || this.height > 546) {
						element.value = '';
						Swal.fire({
							icon: 'error',
							title: 'Oops...',
							text: 'Image size should be equal to 500 KB and dimension should be 1836 x 546!',
							// footer: '<a href>Image dimension should be 1713x540 and size should less than 100 KB</a>'
						})
                      
					}

					else {
						img.onerror = function () {
							alert("not a valid file: " + file.type);
						};
					}
					if (element.value != "") {
						element.closest("form").submit();
					}
				};
				img.src = _URL.createObjectURL(file);
				
				

			}
			else if ($('#ContentType').val() == "Video") {
				var extension = this.files[0].name.replace(/^.*\./, '')
				if (extension == "jpg" || extension == "png" || extension== "gif"){
					element.value = '';
		Swal.fire({
			icon: 'error',
			title: 'Oops...',
			text: "Please select Video",
		}).then(function (result) {

            
			file = "";
		});
	}
				let file;
				let size;
				let videoWidth;
				let videoHeight;
				let ratio;
				let originalWidth;
				let originalHeight;
				file = this.files[0]
				size = this.files[0].size;
				var media = URL.createObjectURL(this.files[0]);
				var video = document.getElementById("video");
				video.src = media;
				video.addEventListener("loadedmetadata", function (e) {

					originalWidth = 1920;
					originalHeight = 1080;
					dimension_message = "Video ratio should be 16:9 !"

					if (size >= 30000000) {
						dimension_message = "video size less then 30 mb !"
						element.value = '';
						file = "";
					}



					videoWidth = this.videoWidth;
					videoHeight = this.videoHeight;


					ratio = ((originalHeight / originalWidth) * videoWidth);
					ratio = Math.floor(ratio);

                    if (ratio != videoHeight || size >= 30000000) {
						element.value = '';
						Swal.fire({
							icon: 'error',
							title: 'Oops...',
							text: dimension_message,
						}).then(function (result) {

                           
							file = "";
						});
					}
					else {
						video.onerror = function () {
                            this.value = '';
							file = "";
						};
					}
				
					if (element.value != "") {
						element.closest("form").submit();
					}
				}, false);
				
			}
            else {
				element.value = '';
				Swal.fire({
					icon: 'error',
					title: 'Oops...',
					text: "Please select Content Type",
				}).then(function (result) {

                   
					file = "";
				});
			}
			
			

		}

	});
    $(".fileupload").change(function (e) {
		var element = this
		var file, img;
		var width = parseInt(this.attributes["data-width"].value); 
		var height = parseInt(this.attributes["data-height"].value);
		var size = parseInt(this.attributes["data-size"].value);
		if ((file = this.files[0])) {
			if (this.files[0].size >= size) {
				this.value = '';
				Swal.fire({
					icon: 'error',
					title: 'Oops...',
					text: 'Image size should be equal to ' +(size==50000?50:500 )+' KB and dimension should be ' + width + ' x ' + height +'!',
					//footer: '<a href>Image size should be less than or equal to  100KB and dimension should be 1713x540</a>'
				})

			}
			img = new Image();
			img.onload = function () {

				if (this.width < width || this.width > width) {
					element.value = '';
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image size should be equal to ' + (size == 50000 ? 50 : 500) + ' KB and dimension should be ' + width + ' x ' + height + '!',
						//  footer: '<a href>Image dimension should be 1713x540 and size should less than 100 KB</a>'
					})

				}
				else if (this.height < height || this.height > height) {
					element.value = '';
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image size should be equal to ' + (size == 50000 ? 50 : 500) + ' KB and dimension should be ' + width + ' x ' + height + '!',
						// footer: '<a href>Image dimension should be 1713x540 and size should less than 100 KB</a>'
					})

				}

				else {
					img.onerror = function () {
						alert("not a valid file: " + file.type);
					};
				}
				if (element.value != "") {
					element.closest("form").submit();
				}
			};
			img.src = _URL.createObjectURL(file);
		}
		
		
           

        
    })
	$('#ContentType').change(function () {
        $("#file").val("");
        if ($('#ContentType').val() == "Image") {
            $('.video').attr("hidden", "hidden")
            $('.image').removeAttr("hidden", "hidden")
        }
        else if ($('#ContentType').val() == "Video") {
            $('.video').removeAttr("hidden", "hidden")
            $('.image').attr("hidden", "hidden")
        }
        else {
            $('.video').attr("hidden", "hidden")
            $('.image').attr("hidden", "hidden")
        }
    })

});

function Delete(element, record,lang) {

	swal.fire({
		title: 'Are you sure?',
		text: "You won't be able to revert this!",
		//type: 'warning',
		showCancelButton: true,
		confirmButtonText: 'Yes, delete it!'
    }).then(function (isConfirm) {
		if (isConfirm.value ) {
		
			$.ajax({
				url: '/Admin/ContentManagment/Delete/' + record,
				type: 'POST',
				success: function (result) {
					if (result.success != undefined) {
						if (result.success) {
							toastr.options = {
								"positionClass": "toast-bottom-right",
							};
                            toastr.success(result.message);
							$(element).closest('.DeleteParent').attr("hidden", "hidden")
							$(element).parent().parent().parent().find('.showparent').removeAttr("hidden", "hidden") 
                            $(element).closest('.banner').remove();
                            if (result.type == "WebsiteHeader") {
                                $('#createform').removeAttr("hidden", "hidden")
                                $('#btnsubmit').removeAttr("hidden", "hidden")
                            }
                            else if (result.type == "MotorSideBanner" && lang == "en") {
                                $('#motorcreateform').removeAttr("hidden", "hidden")
                               
							}
							else if (result.type == "MotorSideBanner" && lang == "ar") {
								$('#motorcreateformar').removeAttr("hidden", "hidden")
								
							}
							else if (result.type == "PropertySideBanner" && lang == "en") {
                                $('#propertycreateform').removeAttr("hidden", "hidden")
                             
							}
							else if (result.type == "PropertySideBanner" && lang == "ar") {
								$('#propertycreateformar').removeAttr("hidden", "hidden")
								
							}
							if (result.mobilearbannear == 0) {
								$('#mobilebannerar').removeAttr("hidden", "hidden")
							}
							 if (result.mobilearbanneen == 0) {
								$('#mobilebanneren').removeAttr("hidden", "hidden")
                            }
						


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
function DeletePromo(element, record, lang) {

	swal.fire({
		title: 'Are you sure?',
		text: "You won't be able to revert this!",
		//type: 'warning',
		showCancelButton: true,
		confirmButtonText: 'Yes, delete it!'
	}).then(function (isConfirm) {
		if (isConfirm.value) {
		
			$.ajax({
				url: '/Admin/ContentManagment/DeletePromoBanner/' + record,
				type: 'POST',
				success: function (result) {
					if (result.success != undefined) {
						if (result.success) {
							toastr.options = {
								"positionClass": "toast-bottom-right",
							};
							toastr.success(result.message);

							$(element).closest('.DeleteParentpromo').attr("hidden", "hidden")
							$(element).parent().parent().parent().find('.showparentpromo').removeAttr("hidden", "hidden")
							$(element).closest('.banner').remove();
							
						


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

