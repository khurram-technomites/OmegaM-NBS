
var _URL = window.URL || window.webkitURL;
$(document).ready(function () {

	//$("#file").change(function (e) {
	//	var file, img;
	//	if ((file = this.files[0])) {
	//		if (this.files[0].size > 500000) {

	//			Swal.fire({
	//				icon: 'error',
	//				title: 'Oops...',
	//				text: 'Image size should be equal to 100 KB and dimension should be 1784 x 446!',
	//				footer: '<a href>Image size should be less than or equal to  100KB and dimension should be 1713x540</a>'
	//			})
	//			$("#file").val("");
	//		}
	//		img = new Image();
	//		img.onload = function () {
	//			if (this.width > 1784) {
	//				Swal.fire({
	//					icon: 'error',
	//					title: 'Oops...',
	//					text: 'Image size should be equal to 100 KB and dimension should be 1784 x 446!',
	//					//  footer: '<a href>Image dimension should be 1713x540 and size should less than 1 Mb</a>'
	//				})
	//				$("#file").val("");
	//			}
	//			else if (this.height > 446) {
	//				Swal.fire({
	//					icon: 'error',
	//					title: 'Oops...',
	//					text: 'Image size should be equal to 100 KB and dimension should be 1784 x 446!',
	//					// footer: '<a href>Image dimension should be 1713x540 and size should less than 1 Mb</a>'
	//				})
	//				$("#file").val("");
	//			}
	//			else {
	//				img.onerror = function () {
	//					alert("not a valid file: " + file.type);
	//				};
	//			}
	//		};
	//		img.src = _URL.createObjectURL(file);
	//	}
	//});

	//BindSortable();
});

function BindSortable() {
	$('#sortable').sortable({
		start: function (event, ui) {
			var start_pos = ui.item.index();
			ui.item.data('start_pos', start_pos);
		},
		change: function (event, ui) {
			var start_pos = ui.item.data('start_pos');
			var index = ui.placeholder.index();
			if (start_pos < index) {
				$('#sortable .ui-state-default:nth-child(' + index + ')').addClass('highlights');
			} else {
				$('#sortable .ui-state-default:eq(' + (index + 1) + ')').addClass('highlights');
			}
			//$('#btnSavePosition').prop('disabled', false);
		},
		update: function (event, ui) {
			$('#sortable .ui-state-default').removeClass('highlights');

			var ImagePositions = [];
			$('.ui-state-default').each(function (k, v) {
				ImagePositions.push({ ID: Number($(v).attr('data')), Position: (k + 1) });
			});
			if (ImagePositions.length > 0) {
				$.ajax({
					type: 'Post',
					url: "/Website/SaveImagePositions/",
					data: { __RequestVerificationToken: $('input[name=__RequestVerificationToken]').val(), positions: ImagePositions },
					success: function (response) {
						if (response.success) {
							toastr.success(response.message, "Success");
						}
						else {
							toastr.error(response.message, 'Failure');
						}
					}
				});
			}
		}
	});

}

function DeleteAll(element) {

	swal.fire({
		title: 'Are you sure?',
		text: "You won't be able to revert this!",
		type: 'warning',
		showCancelButton: true,
		confirmButtonText: 'Yes, delete all!'
	}).then(function (result) {
		if (result.value) {

			$.ajax({
				url: '/Vendor/Gallery/DeleteAll/',
				type: 'POST',
				data: {
					"__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val()
				},
				success: function (result) {
					if (result.success != undefined) {
						if (result.success) {
							toastr.options = {
								"positionClass": "toast-bottom-right",
							};
							toastr.success('All Images Deleted Successfully');

							$('#GalleryContainer').remove();
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
				url: '/Vendor/Gallery/Delete/',
				type: 'POST',
				data: {
					"__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
					galleryViewModel: {
						Path: record
					}
				},
				success: function (result) {
					if (result.success != undefined) {
						if (result.success) {
							toastr.options = {
								"positionClass": "toast-bottom-right",
							};
							toastr.success('Image Deleted Successfully');

							$(element).closest('.gallery-image').remove();
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

function copyToClipboard(elem, text) {
	
	var $temp = $("<input>");
	$("body").append($temp);
	$temp.val(text).select();
	document.execCommand("copy");
	$temp.remove();


	toastr.success('Copied!', text);
}