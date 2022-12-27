"use strict";
jQuery(document).ready(function () {

	$("#UploadCar").submit(function () {
		Swal.fire({
			title: "Uploading motors in bulk",
			text: "This may take some time, please don't close the window.Popup will disappear automatically once uploading is completed",
			allowOutsideClick: false,
			onOpen: function () {
				Swal.showLoading();

				var data = new FormData();
				var files = $("#fileUpload").get(0).files;
				if (files.length > 0) {
					data.append("FileUpload", files[0]);
				}

				$.ajax({
					url: '/Vendor/Car/BulkUpload?connectionId=' + connectionId,
					type: 'Post',
					processData: false,
					contentType: false,
					data: data,
					success: function (response) {
						if (response.success) {
							toastr.success(response.successMessage);
							if (response.errorMessage) {
								toastr.error(response.errorMessage);
							}

							if (response.detailedErrorMessages) {
								$('#myModalContent').html(`<div class="row"><div class="col-12"> ${response.detailedErrorMessages} </div></div>`);
								$('#myModal').modal({}, 'show');
							} else {
								$('#myModal').modal('hide');
							}
							Swal.close();

							$("#Regions").val("");
							$("#VendorID").val("");

						} else {
							Swal.close()
							toastr.error(response.message);
							$(this).closest('.modal').find('button[type=submit]').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
						}
					}
				});
			}
		}).then(function (result) {
			if (result.dismiss === "timer") {
				console.log("Cars upload completed!");
			}
		})

		return false;
	});
});