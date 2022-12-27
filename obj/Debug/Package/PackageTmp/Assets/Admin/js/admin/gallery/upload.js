"use strict";
jQuery(document).ready(function () {

	$("#UploadGallery").submit(function () {
		Swal.fire({
			title: "Uploading gallery images",
			text: "This may take some time, please don't close the window.Popup will disappear automatically once uploading is completed",
			allowOutsideClick: false,
			onOpen: function () {
				Swal.showLoading();
				
				var formdata = new FormData($('#UploadGallery').get(0));
				
				$.ajax({
					url: '/Admin/Gallery/Create?connectionId=' + connectionId,
					type: 'Post',
					processData: false,
					contentType: false,
					data: formdata,
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

						} else {
							Swal.close()
							toastr.error(response.message);
							$(this).closest('.modal').find('button[type=submit]').removeClass('spinner spinner-sm spinner-left').attr('disabled', false);
						}
					},
					error: function (e) {

						Swal.close();
						$('#myModal').modal('hide');
						toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
					},
					failure: function (e) {

						Swal.close();
						$('#myModal').modal('hide');
						toastr.error("Ooops, something went wrong.Try to refresh this page or contact Administrator if the problem persists.");
					}
				});
			}
		}).then(function (result) {
			if (result.dismiss === "timer") {
				console.log("Gallery upload completed!");
			}
		})

		return false;
	});
});