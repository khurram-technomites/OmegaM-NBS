"use strict";

jQuery(document).ready(function () {

	var _URL = window.URL || window.webkitURL;

	$("#file").change(function (e) {
		var file, img;
		if ((file = this.files[0])) {
			if (this.files[0].size > 100000) {
				Swal.fire({
					icon: 'error',
					title: 'Oops...',
					text: 'Image size should be equal to 100 KB and dimension should be 1784 x 446!',
					//footer: '<a href>Image size should be less than or equal to  100KB and dimension should be 1713x540</a>'
				})
				$("#file").val("");
			}
			img = new Image();
			img.onload = function () {
			    if (this.width < 1784 || this.width > 1784) {
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image size should be equal to 100 KB and dimension should be 1784 x 446!',
						//  footer: '<a href>Image dimension should be 1713x540 and size should less than 1 Mb</a>'
					})
					$("#file").val("");
				}
			    else if (this.height < 446 || this.height > 446) {
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image size should be equal to 100 KB and dimension should be 1784 x 446!',
						// footer: '<a href>Image dimension should be 1713x540 and size should less than 1 Mb</a>'
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
			if (this.files[0].size >= 100000) {
				Swal.fire({
					icon: 'error',
					title: 'Oops...',
					text: 'Image size should be less than 100 KB  & dimension should be 1713x608 !',
					//footer: '<a href>Image size should be less than 100 KB  & dimension should be (1713x540) </a>'
				})
				$("#mb-file").val("");
			}
			img = new Image();
			img.onload = function () {


				if (this.width < 1713 || this.width > 1713) {
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image size should be less than 100 KB  & dimension should be 1713x608 !',
						//footer: '<a href>Image size should be less than 100 KB  & dimension should be (1713x540) </a>'
					})
					$("#mb-file").val("");
				}
				else if (this.height < 608 || this.height > 608) {
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image size should be less than 100 KB  & dimension should be 1713x608 !',
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
				url: '/Admin/Website/DeletePromotionBanners/' + record,
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
							toastr.success('Banner Deleted Successfully');

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