"use strict";

jQuery(document).ready(function () {
	var _URL = window.URL || window.webkitURL;

	$("#file").change(function (e) {
		var file, img;
		
		if ((file = this.files[0])) {
			if (this.files[0].size >= 500000) {
				Swal.fire({
					icon: 'error',
					title: 'Oops...',
					text: 'Image size should be equal to 500 KB and dimension should be 1836 x 546!',
					//footer: '<a href>Image size should be less than or equal to  100KB and dimension should be 1713x540</a>'
				})
				$("#file").val("");
			}
			img = new Image();
			img.onload = function () {

				if (this.width < 1836 || this.width > 1836) {
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image size should be equal to 500 KB and dimension should be 1836 x 546!',
						//  footer: '<a href>Image dimension should be 1713x540 and size should less than 100 KB</a>'
					})
					$("#file").val("");
				}
				else if (this.height < 546 || this.height > 546) {
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image size should be equal to 500 KB and dimension should be 1836 x 546!',
						// footer: '<a href>Image dimension should be 1713x540 and size should less than 100 KB</a>'
					})
					$("#file").val("");
				}

				else {
					img.onerror = function () {
						alert("not a valid file: " + file.type);
					};
				}
			};

			img.src = _URL.createObjectURL(file);

		}

	});
	$("#mb-file").change(function (e) {
		var file, img;
		

		if ((file = this.files[0])) {
			if (this.files[0].size >= 500000) {
				Swal.fire({
					icon: 'error',
					title: 'Oops...',
					text: 'Image size should be less than 500 KB  & dimension should be 1098 x 300 !',
					//footer: '<a href>Image size should be less than 100 KB  & dimension should be (1713x540) </a>'
				})
				$("#mb-file").val("");
			}
			img = new Image();
			img.onload = function () {


				if (this.width < 378 || this.width > 378) {
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image size should be less than 500 KB  & dimension should be 1098 x 300 !',
						//footer: '<a href>Image size should be less than 100 KB  & dimension should be (1713x540) </a>'
					})
					$("#mb-file").val("");
				}
				else if (this.height < 168 || this.height > 168) {
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image size should be less than 500 KB  & dimension should be 1098 x 300 !',
						//footer: '<a href>Image size should be less than 100 KB  & dimension should be (1713x540) </a>'
					})
					$("#mb-file").val("");
				}

				else {
					img.onerror = function () {
						alert("not a valid file: " + file.type);
					};
				}
			};

			img.src = _URL.createObjectURL(file);
		}

	});
});

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
				url: '/Admin/Website/Delete/' + record,
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
							toastr.success('Banner deleted successfully ...');

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