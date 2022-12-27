"use strict";
var UserRolePrivileges = {
	UserRoleId: UserRoleId,
	Routes: []
};

jQuery(document).ready(function () {

	$('.child').each(function (k, v) {
		//var html = $(this).html();
		//
		$("#" + $(this).attr("parent-id")).find(".child-container").append($(this).get(0));
		//$(this).remove();
	});

	$('.parent').each(function (k, v) {
		var html = $(this).get(0);
		if ($(this).attr("parent-id")) {
			//var mainContent = $("#" + $(this).attr("parent-id")).find(".main-content").append();
			var mainContent = $("#" + $(this).attr("parent-id")).find(".main-content").first();
			//$(this).remove();
			$($(v)).appendTo($(mainContent));
		}
	});

	$('.spinner').remove();
	$('.mytree').fadeIn();

	$(".parent-header").click(function () {

		var $header = $(this);
		//getting the next element
		var $content = $header.next();
		//open up the content needed - toggle the slide- if visible, slide up, if not slidedown.
		$content.slideToggle(500, function () {
			//execute this after slideToggle is done
			//change text of header based on visibility of content div
			$header.text()
			$header.html(function () {
				//change text based on condition
				return $content.is(":visible") ? ($(this).html().replace("chevron-circle-down", "chevron-circle-up")) : ($(this).html().replace("chevron-circle-up", "chevron-circle-down"));
			});
		});

	});

	$(".btn-actions button").click(function () {

		switch ($(this).attr('id')) {
			case 'collapsed':
				$('.main-content').fadeOut();
				$('.fa-chevron-up').removeClass('fa-chevron-up').addClass('fa-chevron-down');
				break;
			case 'expanded':
				$('.main-content').fadeIn();
				$('.fa-chevron-down').removeClass('fa-chevron-down').addClass('fa-chevron-up');
				break;
			case 'checkall':
				$(".main-content input[type='checkbox']").prop('checked', true);
				break;
			case 'uncheckall':
				$(".main-content input[type='checkbox']").prop('checked', false);
				break;
			default:
		}

	});

	$('#search').on('input', function () {

		// Search text
		var text = $(this).val().toLowerCase();
		if (text) {
			// Hide all content class element
			$('.parent').hide();

			// Search 
			$('.parent .parent-header').each(function () {

				if ($(this).text().toLowerCase().indexOf("" + text + "") != -1) {
					$(this).parentsUntil(".mytree").fadeIn();
					$(this).next().fadeIn();
				}
			});
		} else {
			$('.parent').show();
			$('#collapsed').trigger('click');
		}
	});

});

function Update(element) {
	swal.fire({
		title: 'Are you sure?',
		text: "You won't be able to revert this!",
		type: 'warning',
		showCancelButton: true,
		confirmButtonText: 'Yes, do it!'
	}).then(function (result) {
		if (result.value) {
			$(element).find('i').hide();
			$(element).addClass('spinner spinner-left spinner-sm').attr('disabled', true);
			UserRolePrivileges.Routes = [];
			$('input:checkbox:checked').each(function (k, v) {
				UserRolePrivileges.Routes.push({ Id: $(v).attr('id'), Url: $(v).attr('Url') });
			});
			$.ajax({
				url: '/Vendor/UserRole/Update/',
				type: 'POST',
				data: {
					"__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val(),
					userRolePrivileges: UserRolePrivileges
				},
				success: function (response) {
					if (response.success) {
						toastr.success(response.message);
					} else {
						toastr.error(response.message);
					}
					$(element).removeClass('spinner spinner-left spinner-sm').attr('disabled', false);
					$(element).find('i').show();
				}
			});
		} else {
			//swal("Cancelled", "Your imaginary file is safe :)", "error");
		}
	});
}
