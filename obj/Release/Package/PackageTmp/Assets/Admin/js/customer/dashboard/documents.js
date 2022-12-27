'use strict';

//#region Global Variables and Arrays
let defaultGccFileName = $('.choose-file-license-gcc-span').text();
//let defaultNonGccFileName = $('.choose-file-license-non-gcc-span').text();
let defaultPassportFileName = $('.choose-file-passport-span').text();

var IsMyDocumentsLoaded = false;

var LicenseGcc;
//var LicenseNonGcc;
var Passport;
//#endregion

//#region document ready function

//#region File License GCC
$('#btn-license-gcc-edit').click(function () {
	$('#btn-license-gcc-edit').hide();
	$('#btn-license-gcc-close').show();
	LicenseGccSlideDown();
});

$('.btn-license-gcc-cancel').click(function () {
	$('#btn-license-gcc-edit').show();
	$('#btn-license-gcc-close').hide();
	LicenseGccSlideUp();
});

$('#choose-file-license-gcc').change(function () {
	var i = $(this).prev('span').clone();
	try {
		var file = $('#choose-file-license-gcc')[0].files[0].name;
		$(this).prev('span').text(file);
	} catch (e) {
		$(this).prev('span').text(defaultGccFileName);
	}
});

$('#btn-license-gcc-delete').click(function () {
	DeleteDocument(true, false);
});

$("#form-license-gcc").submit(function () {

	var form = $(this);
	var formData = new FormData();

	var files = $("#choose-file-license-gcc").get(0).files;
	if (files.length > 0) {
		ButtonDisabled('#btn-license-gcc', true, true);

		formData.append("Path", files[0], files[0].name);

		formData.append("__RequestVerificationToken", $('input[name="__RequestVerificationToken"]').val());
		formData.append("Type", 'License');
		formData.append("IsGcc", $('input[name=RegionType]:checked').val());
		if (LicenseGcc) {
			formData.append("ID", LicenseGcc.ID);
		}

		$.ajax({
			url: '/' + culture + '/Customer/Documents/CreateOrUpdate/',
			type: "POST",
			data: formData,
			contentType: false,
			processData: false,
			success: function (response) {
				if (response.success) {
					ShowSweetAlertBtnText('success',
						ChangeString("Uploaded!", "تم الرفع!"),
						ChangeString("Your Driving License is uploaded successfully!", "تم تحميل رخصة منطقة دول مجلس التعاون الخليجي تم تحميل رخصة القيادة الخاصة بك بنجاح!"),
						'#40194e',
						ChangeString("Ok", "حسنا"));
					LicenseGccHide();
					LicenseGcc = response.data;
					CheckDouments(true, false);
				} else {
					console.log('Error!')
					ShowSweetAlertBtnText('error',
						ChangeString("Error!", "خطأ!"),
						response.message,
						'#40194e',
						ChangeString("Close", "أغلق"));
				}
				ButtonDisabled('#btn-license-gcc', false, false);
			},
			error: function (e) {
				ShowFormAlert(form, 'danger', ServerErrorShort, 6);
				ButtonDisabled('#btn-license-gcc', false, false);
			},
			failure: function (e) {
				ShowFormAlert(form, 'danger', ServerErrorShort, 6);
				ButtonDisabled('#btn-license-gcc', false, false);
			}
		});
	}
	else {
		ShowSweetAlertBtnText('error',
			ChangeString("Error!", "خطأ!"),
			ChangeString("Please select document first!", "الرجاء تحديد المستند أولاً!"),
			'#40194e',
			ChangeString("Close", "أغلق"));
	}

	return false;
});

//#endregion

//#region File License Non GCC
//$('#btn-license-non-gcc-edit').click(function () {
//    $('#btn-license-non-gcc-edit').hide();
//    $('#btn-license-non-gcc-close').show();
//    LicenseNonGccSlideDown();
//});

//$('.btn-license-non-gcc-cancel').click(function () {
//    $('#btn-license-non-gcc-edit').show();
//    $('#btn-license-non-gcc-close').hide();
//    LicenseNonGccSlideUp();
//});

//$('#choose-file-license-non-gcc').change(function () {
//    var i = $(this).prev('span').clone();
//    try {
//        var file = $('#choose-file-license-non-gcc')[0].files[0].name;
//        $(this).prev('span').text(file);
//    } catch (e) {
//        $(this).prev('span').text(defaultNonGccFileName);
//    }
//});

//$('#btn-license-non-gcc-delete').click(function () {
//    DeleteDocument(false, true, false);
//});

//$("#form-license-non-gcc").submit(function () {

//    var form = $(this);
//    var formData = new FormData();

//    var files = $("#choose-file-license-non-gcc").get(0).files;
//    if (files.length > 0) {
//        ButtonDisabled('#btn-license-non-gcc', true, true);

//        formData.append("Path", files[0], files[0].name);

//        formData.append("__RequestVerificationToken", $('input[name="__RequestVerificationToken"]').val());
//        formData.append("Type", 'License');
//        formData.append("IsGcc", false);
//        if (LicenseNonGcc) {
//            formData.append("ID", LicenseNonGcc.ID);
//        }

//        $.ajax({
//            url: '/' + culture + '/Customer/Documents/CreateOrUpdate/',
//            type: "POST",
//            data: formData,
//            contentType: false,
//            processData: false,
//            success: function (response) {
//                if (response.success) {
//                    ShowSweetAlertBtnText('success',
//                        ChangeString("Uploaded!", "تم الرفع!"),
//                        ChangeString("Your License Non-GCC Region is uploaded successfully!", "تم تحميل رخصتك الخاصة بمنطقة غير دول مجلس التعاون الخليجي بنجاح!"),
//                        '#40194e',
//                        ChangeString("Ok", "حسنا"));
//                    LicenseNonGccHide();
//                    LicenseNonGcc = response.data;
//                    CheckDouments(false, true, false);
//                } else {
//                    console.log('Error!')
//                    ShowSweetAlertBtnText('error',
//                        ChangeString("Error!", "خطأ!"),
//                        response.message,
//                        '#40194e',
//                        ChangeString("Close", "أغلق"));
//                }
//                ButtonDisabled('#btn-license-non-gcc', false, false);
//            },
//            error: function (e) {
//                ShowFormAlert(form, 'danger', ServerErrorShort, 6);
//                ButtonDisabled('#btn-license-non-gcc', false, false);
//            },
//            failure: function (e) {
//                ShowFormAlert(form, 'danger', ServerErrorShort, 6);
//                ButtonDisabled('#btn-license-non-gcc', false, false);
//            }
//        });
//    }
//    else {
//        ShowSweetAlertBtnText('error',
//            ChangeString("Error!", "خطأ!"),
//            ChangeString("Please select document first!", "الرجاء تحديد المستند أولاً!"),
//            '#40194e',
//            ChangeString("Close", "أغلق"));
//    }

//    return false;
//});
//#endregion

//#region File License Passport
$('#btn-passport-edit').click(function () {
	$('#btn-passport-edit').hide();
	$('#btn-passport-close').show();
	PassportSlideDown();
});

$('.btn-passport-cancel').click(function () {
	$('#btn-passport-edit').show();
	$('#btn-passport-close').hide();
	PassportSlideUp();
});

$('#choose-file-passport').change(function () {
	var i = $(this).prev('span').clone();
	try {
		var file = $('#choose-file-passport')[0].files[0].name;
		$(this).prev('span').text(file);
	} catch (e) {
		$(this).prev('span').text(defaultPassportFileName);
	}
});

$('#btn-passport-delete').click(function () {
	DeleteDocument(false, true);
});

$("#form-passport").submit(function () {

	var form = $(this);
	var formData = new FormData();

	var files = $("#choose-file-passport").get(0).files;
	if (files.length > 0) {
		ButtonDisabled('#btn-passport', true, true);

		formData.append("Path", files[0], files[0].name);

		formData.append("__RequestVerificationToken", $('input[name="__RequestVerificationToken"]').val());
		formData.append("Type", 'Passport');
		formData.append("Is", false);
		if (Passport) {
			formData.append("ID", Passport.ID);
		}

		$.ajax({
			url: '/' + culture + '/Customer/Documents/CreateOrUpdate/',
			type: "POST",
			data: formData,
			contentType: false,
			processData: false,
			success: function (response) {
				if (response.success) {
					ShowSweetAlertBtnText('success',
						ChangeString("Uploaded!", "تم الرفع!"),
						ChangeString("Your Passport is uploaded successfully!", "تم تحميل جواز سفرك بنجاح!"),
						'#40194e',
						ChangeString("Ok", "حسنا"));
					PassportHide();
					Passport = response.data;
					CheckDouments(false, true);
				} else {
					console.log('Error!')
					ShowSweetAlertBtnText('error',
						ChangeString("Error!", "خطأ!"),
						response.message,
						'#40194e',
						ChangeString("Close", "أغلق"));
				}
				ButtonDisabled('#btn-passport', false, false);
			},
			error: function (e) {
				ShowFormAlert(form, 'danger', ServerErrorShort, 6);
				ButtonDisabled('#btn-passport', false, false);
			},
			failure: function (e) {
				ShowFormAlert(form, 'danger', ServerErrorShort, 6);
				ButtonDisabled('#btn-passport', false, false);
			}
		});
	}
	else {
		ShowSweetAlertBtnText('error',
			ChangeString("Error!", "خطأ!"),
			ChangeString("Please select document first!", "الرجاء تحديد المستند أولاً!"),
			'#40194e',
			ChangeString("Close", "أغلق"));
	}

	return false;
});
//#endregion

//#endregion

//#region Bind Response

function BindMyDocuments(response) {

	//$('.my-documents-body-preload').hide();
	//$('.my-documents-body').show();

	//console.log(response);
	if (response.data.length > 0) {

		LicenseGcc = response.data.find(function (obj) {
			return obj.Type == 'License';
		});
		//LicenseNonGcc = response.data.find(function (obj) {
		//    return obj.Type == 'License' && obj.IsGCC == false;
		//});
		Passport = response.data.find(function (obj) {
			return obj.Type == 'Passport';
		});

		CheckDouments(true, true);
	}
	EmptyDocuments(true, true);
}

//#endregion

//#region Others Function

function LoadMyDocuments() {
	if (!IsMyDocumentsLoaded) {
		GetMyDocuments();
		IsMyDocumentsLoaded = true;
	}
}

function GetMyDocuments() {
	$.ajax({
		type: 'GET',
		url: '/' + culture + '/Customer/Documents/GetMyDocuments',
		contentType: "application/json",
		success: function (response) {
			if (response.success) {

				BindMyDocuments(response);
			} else {

				console.log(response.message);
				EmptyDocuments(true, true);
			}
			$('.my-documents-body-preload').hide();
			$('.my-documents-body').show();
		},
		error: function (e) {
			console.log("Get My Documents Error.");
		}
	});
}

function CheckDouments(CheckLicenseGcc, CheckPassport) {
	var conditions = ["gif", "jpg", "jpeg", "png", "tiff"];
	if (CheckLicenseGcc && LicenseGcc) {
		if (conditions.some(el => LicenseGcc.Path.toLowerCase().includes(el))) {
			$('#license-gcc-image').attr('src', '/Assets/images/default/default-omw-image-not-found.png');
			$('.view-license-gcc').attr('href', LicenseGcc.Path).attr('target', '_blank').removeClass('text-danger').addClass('text-info').text(ChangeString('View Image', 'عرض الصورة')).show();
		} else {
			$('#license-gcc-image').attr('src', '/Assets/images/default/default-omw-doc.png');
			$('.view-license-gcc').attr('href', LicenseGcc.Path).attr('target', '_blank').removeClass('text-danger').addClass('text-info').text(ChangeString('View Document', 'عرض المستند')).show();
		}
		var regionType = LicenseGcc.IsGCC == true ? ChangeString("GCC Region", "منطقة دول مجلس التعاون الخليجي") : ChangeString("Non-GCC Region", "منطقة خارج دول مجلس التعاون الخليجي");
		$('.upload-license-span').text(regionType);
		$('#btn-license-gcc-edit').hide();
		$('#btn-license-gcc-close').hide();
		$('#btn-license-gcc-delete').show();
	}
	//if (CheckLicenseNonGcc && LicenseNonGcc) {
	//    if (conditions.some(el => LicenseNonGcc.Path.toLowerCase().includes(el))) {
	//        $('#license-non-gcc-image').attr('src', '/Assets/images/default/default-omw-image-not-found.png');
	//        $('.view-license-non-gcc').attr('href', LicenseNonGcc.Path).attr('target', '_blank').removeClass('text-danger').addClass('text-info').text(ChangeString('View Image', 'عرض الصورة')).show();
	//    } else {
	//        $('#license-non-gcc-image').attr('src', '/Assets/images/default/default-omw-doc.png');
	//        $('.view-license-non-gcc').attr('href', LicenseNonGcc.Path).attr('target', '_blank').removeClass('text-danger').addClass('text-info').text(ChangeString('View Document', 'عرض المستند')).show();
	//    }

	//    $('#btn-license-non-gcc-edit').hide();
	//    $('#btn-license-non-gcc-close').hide();
	//    $('#btn-license-non-gcc-delete').show();
	//}
	if (CheckPassport && Passport) {
		if (conditions.some(el => Passport.Path.toLowerCase().includes(el))) {
			$('#passport-image').attr('src', '/Assets/images/default/default-omw-image-not-found.png');
			$('.view-passport').attr('href', Passport.Path).attr('target', '_blank').removeClass('text-danger').addClass('text-info').text(ChangeString('View Image', 'عرض الصورة')).show();
		} else {
			$('#passport-image').attr('src', '/Assets/images/default/default-omw-doc.png');
			$('.view-passport').attr('href', Passport.Path).attr('target', '_blank').removeClass('text-danger').addClass('text-info').text(ChangeString('View Document', 'عرض المستند')).show();
		}
		$('.upload-passport-span').text(ChangeString("Passport", "جواز سفر"));
		$('#btn-passport-edit').hide();
		$('#btn-passport-close').hide();
		$('#btn-passport-delete').show();
	}
}

function EmptyDocuments(EmptyLicenseGcc, EmptyPassport) {

	if (EmptyLicenseGcc && !LicenseGcc) {
		$('#license-gcc-image').attr('src', DefaultImageUrl);
		$('.view-license-gcc').attr('href', 'javascript:void(0)').attr('target', '').removeClass('text-info').addClass('text-danger').text(ChangeString('No Document', 'لا يوجد مستند')).show();

		$('.upload-license-span').text(ChangeString("Upload GCC OR Non-GCC Driving License", "تحميل رخصة القيادة الخليجية أو غير الخليجية"));
		$('#btn-license-gcc-edit').show();
		$('#btn-license-gcc-close').hide();
		$('#btn-license-gcc-delete').hide();
	}
	//if (EmptyLicenseNonGcc && !LicenseNonGcc) {
	//    $('#license-non-gcc-image').attr('src', DefaultImageUrl);
	//    $('.view-license-non-gcc').attr('href', 'javascript:void(0)').attr('target', '').removeClass('text-info').addClass('text-danger').text(ChangeString('No Document', 'لا يوجد مستند')).show();

	//    $('#btn-license-non-gcc-edit').show();
	//    $('#btn-license-non-gcc-close').hide();
	//    $('#btn-license-non-gcc-delete').hide();
	//}
	if (EmptyPassport && !Passport) {
		$('#passport-image').attr('src', DefaultImageUrl);
		$('.view-passport').attr('href', 'javascript:void(0)').attr('target', '').removeClass('text-info').addClass('text-danger').text(ChangeString('No Document', 'لا يوجد مستند')).show();

		$('.upload-passport-span').text(ChangeString("Upload Passport", "تحميل جواز السفر"));
		$('#btn-passport-edit').show();
		$('#btn-passport-close').hide();
		$('#btn-passport-delete').hide();
	}
}

function DeleteDocument(DeleteLicenseGcc, DeletePassport) {
	if (DeleteLicenseGcc && LicenseGcc) {
		Swal.fire({
			title: ChangeString('Are you sure?', 'هل أنت متأكد؟'),
			text: ChangeString("You won't be able to revert this!", "لن تتمكن من التراجع عن هذا!"),
			icon: 'warning',
			showCancelButton: true,
			confirmButtonColor: '#40194e',
			cancelButtonColor: '#7a7a7a',
			confirmButtonText: ChangeString("Yes, Delete Now!", "نعم ، ادفع الآن!"),
			cancelButtonText: ChangeString("Cancel", "يلغي"),
		}).then((result) => {
			if (result.isConfirmed) {
				$('#btn-license-gcc-delete').find('span').removeClass('ti-trash').addClass('fa fa-spin fa-circle-notch');

				$.ajax({
					type: 'Get',
					url: '/' + culture + '/Customer/Documents/Delete?id=' + LicenseGcc.ID,
					contentType: "application/json",
					success: function (response) {
						if (response.success) {
							ShowSweetAlertBtnText('success',
								ChangeString("Deleted!", "تم الحذف!"),
								ChangeString("Your Driving License is deleted successfully!", "تم حذف رخصة قيادتك بنجاح!"),
								'#40194e',
								ChangeString("Ok", "حسنا"));

							LicenseGcc = undefined;
							EmptyDocuments(true, false)
						} else {
							ShowSweetAlertBtnText('error',
								ChangeString("Error!", "خطأ!"),
								response.message,
								'#40194e',
								ChangeString("Close", "أغلق"));
						}

						$('#btn-license-gcc-delete').find('span').removeClass('fa fa-spin fa-circle-notch').addClass('ti-trash');
					},
					error: function (e) {
						console.log('Error!')
						ShowSweetAlertBtnText('error',
							ChangeString("Error!", "خطأ!"),
							response.message,
							'#40194e',
							ChangeString("Close", "أغلق"));

						$('#btn-license-gcc-delete').find('span').removeClass('fa fa-spin fa-circle-notch').addClass('ti-trash');
					}
				});
			}
		});
	}
	//if (DeleteLicenseNonGcc && LicenseNonGcc) {
	//    Swal.fire({
	//        title: ChangeString('Are you sure?', 'هل أنت متأكد؟'),
	//        text: ChangeString("You won't be able to revert this!", "لن تتمكن من التراجع عن هذا!"),
	//        icon: 'warning',
	//        showCancelButton: true,
	//        confirmButtonColor: '#40194e',
	//        cancelButtonColor: '#7a7a7a',
	//        confirmButtonText: ChangeString("Yes, Delete Now!", "نعم ، ادفع الآن!"),
	//        cancelButtonText: ChangeString("Cancel", "يلغي"),
	//    }).then((result) => {
	//        if (result.isConfirmed) {
	//            $.ajax({
	//                type: 'Get',
	//                url: '/' + culture + '/Customer/Documents/Delete?id=' + LicenseNonGcc.ID,
	//                contentType: "application/json",
	//                success: function (response) {
	//                    if (response.success) {
	//                        ShowSweetAlertBtnText('success',
	//                            ChangeString("Deleted!", "تم الحذف!"),
	//                            ChangeString("Your License Non-GCC Region is deleted successfully!", "تم حذف الترخيص الخاص بك في منطقة غير دول مجلس التعاون الخليجي بنجاح!"),
	//                            '#40194e',
	//                            ChangeString("Ok", "حسنا"));

	//                        LicenseNonGcc = undefined;
	//                        EmptyDocuments(false, true, false)
	//                    } else {
	//                        ShowSweetAlertBtnText('error',
	//                            ChangeString("Error!", "خطأ!"),
	//                            response.message,
	//                            '#40194e',
	//                            ChangeString("Close", "أغلق"));
	//                    }
	//                },
	//                error: function (e) {
	//                    console.log('Error!')
	//                    ShowSweetAlertBtnText('error',
	//                        ChangeString("Error!", "خطأ!"),
	//                        response.message,
	//                        '#40194e',
	//                        ChangeString("Close", "أغلق"));
	//                }
	//            });
	//        }
	//    });
	//}

	if (DeletePassport && Passport) {
		Swal.fire({
			title: ChangeString('Are you sure?', 'هل أنت متأكد؟'),
			text: ChangeString("You won't be able to revert this!", "لن تتمكن من التراجع عن هذا!"),
			icon: 'warning',
			showCancelButton: true,
			confirmButtonColor: '#40194e',
			cancelButtonColor: '#7a7a7a',
			confirmButtonText: ChangeString("Yes, Delete Now!", "نعم ، ادفع الآن!"),
			cancelButtonText: ChangeString("Cancel", "يلغي"),
		}).then((result) => {
			if (result.isConfirmed) {
				$('#btn-passport-delete').find('span').removeClass('ti-trash').addClass('fa fa-spin fa-circle-notch');

				$.ajax({
					type: 'Get',
					url: '/' + culture + '/Customer/Documents/Delete?id=' + Passport.ID,
					contentType: "application/json",
					success: function (response) {
						if (response.success) {
							ShowSweetAlertBtnText('success',
								ChangeString("Deleted!", "تم الحذف!"),
								ChangeString("Your Passport is deleted successfully!", "تم حذف جواز سفرك بنجاح!"),
								'#40194e',
								ChangeString("Ok", "حسنا"));

							Passport = undefined;
							EmptyDocuments(false, true)
						} else {
							ShowSweetAlertBtnText('error',
								ChangeString("Error!", "خطأ!"),
								response.message,
								'#40194e',
								ChangeString("Close", "أغلق"));
						}

						$('#btn-passport-delete').find('span').removeClass('fa fa-spin fa-circle-notch').addClass('ti-trash');
					},
					error: function (e) {
						console.log('Error!')
						ShowSweetAlertBtnText('error',
							ChangeString("Error!", "خطأ!"),
							response.message,
							'#40194e',
							ChangeString("Close", "أغلق"));

						$('#btn-passport-delete').find('span').removeClass('fa fa-spin fa-circle-notch').addClass('ti-trash');
					}
				});
			}
		});
	}
}

//#region File License GCC
function LicenseGccSlideDown() {
	$('#license-gcc-form-div').slideDown();
}
function LicenseGccSlideUp() {
	$('#license-gcc-form-div').slideUp();
	$("#choose-file-license-gcc").val('').prev('span').text(defaultGccFileName);
}
function LicenseGccHide() {
	if (LicenseGcc) {
		LicenseGccSlideUp();
	}
	else {
		$('#btn-license-gcc-edit').show();
		$('#btn-license-gcc-close').hide();
		LicenseGccSlideUp();
	}
}
//#endregion

//#region File License Non GCC
//function LicenseNonGccSlideDown() {
//    $('#license-non-gcc-form-div').slideDown();
//}
//function LicenseNonGccSlideUp() {
//    $('#license-non-gcc-form-div').slideUp();
//    $("#choose-file-license-non-gcc").val('').prev('span').text(defaultNonGccFileName);
//}
//function LicenseNonGccHide() {
//    if (LicenseNonGcc) {
//        LicenseNonGccSlideUp();
//    }
//    else {
//        $('#btn-license-non-gcc-edit').show();
//        $('#btn-license-non-gcc-close').hide();
//        LicenseNonGccSlideUp();
//    }
//}
//#endregion

//#region File License Passport
function PassportSlideDown() {
	$('#passport-form-div').slideDown();
}
function PassportSlideUp() {
	$('#passport-form-div').slideUp();
	$("#choose-file-passport").val('').prev('span').text(defaultPassportFileName);
}
function PassportHide() {

	if (Passport) {
		PassportSlideUp();
	}
	else {
		$('#btn-passport-edit').show();
		$('#btn-passport-close').hide();
		PassportSlideUp();
	}
}
//#endregion

//#endregion
