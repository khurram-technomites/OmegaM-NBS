var _URL = window.URL || window.webkitURL;
var input = document.querySelector("#phone");

$(document).ready(function () {

	$('#SellerRegistrationForm').submit(function () {

		var form = $(this);
		var titleSplit = $('.iti__selected-flag').get(0).title.split(' ');
		titleSplit.reverse();
		var code = titleSplit[0].replace("+", "");
		let contact = $('#phone').val().replace("+", "");
		contact = code + contact;
		$('#Contact').val(contact);

		if (code == "94") {
			$('#IsSrilankan').val("true");
		}
		else {
			$('#IsSrilankan').val("false");
		}

		if (code == "94" && contact.length != 11) {
			showErrorMsg(form, 'danger', "Contact number should have 9 digits ...");
			$('#phone').closest('div').addClass('border border-danger');
			setTimeout(function () {
				$('#phone').closest('div').removeClass('border border-danger');
			}, 6000);

			return false;
		}
		else {

			$('#btnSignup').html('<span class="fa fa-circle-notch fa-spin"></span> Register').attr('disabled', true);
			return true;
		}
		
	});

	$('#CountryID').change(function () {
		var count = 0;
		var $dropdown = $("#CityID");
		if ($(this).val() == 0) {
			$dropdown.empty();
			$dropdown.append($("<option />").val('').text("Please Select Country First!"));
		}
		else {
			$.ajax({
				type: 'Get',
				url: '/Cities/GetCitiesByCountry/' + $(this).val(),
				success: function (response) {
					$dropdown.empty();
					$dropdown.append($("<option />").val('').text("Select City"));

					$.each(response.data, function (k, v) {
						$dropdown.append($("<option />").val(v.value).text(v.text));
						count++;
					});
				}
			});
		}
	});

	$("#Name").on('change', function () {
		var name = $(this);
		$("#Slug").val($(name).val().replace(/ /g, "-").toLocaleLowerCase());
	});

	$("#cover").change(function (e) {
		var file, img;
		if ((file = this.files[0])) {
			img = new Image();
			img.onload = function () {
				if (this.width < 1920) {
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image dimension should be greater than 1920x316!',

					}).then(function (result) {
						$("#cover").attr("src", "/Assets/AppFiles/Images/default.png");
						$(".cancelimage").trigger('click');
					});
				}
				else if (this.height < 316) {
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image dimension should be greater than 1920x316!',

					}).then(function (result) {
						$("#cover").attr("src", "/Assets/AppFiles/Images/default.png");
						$(".cancelimage").trigger('click');
					});
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

	//Image Validation Begin
	$("#Logo").change(function (e) {
		var file, img;
		if ((file = this.files[0])) {

			img = new Image();
			img.onload = function () {

				if (this.width < 690 || this.width > 690) {
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image dimension should be 690 x 460 !',

					}).then(function (result) {
						$("#Logo").attr("src", "/Assets/AppFiles/Images/default.png");
						$(".cancelimage").trigger('click');
					});
				}
				else if (this.height < 460 || this.height > 460) {
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image dimension should be 690 x 460 !',

					}).then(function (result) {
						$("#Logo").attr("src", "/Assets/AppFiles/Images/default.png");
						$(".cancelimage").trigger('click');
					});
				}
				else if (this.size > 50) {
					Swal.fire({
						icon: 'error',
						title: 'Oops...',
						text: 'Image size must be less than 50 kb!',

					}).then(function (result) {
						$("#Logo").attr("src", "/Assets/AppFiles/Images/default.png");
						$(".cancelimage").trigger('click');
					});
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
	//Image Validation End
});


window.intlTelInput(input, {
	// allowDropdown: false,
	// autoHideDialCode: false,
	// autoPlaceholder: "off",
	// dropdownContainer: document.body,
	// excludeCountries: ["us"],
	// formatOnDisplay: false,
	// geoIpLookup: function(callback) {
	//   $.get("http://ipinfo.io", function() {}, "jsonp").always(function(resp) {
	//     var countryCode = (resp && resp.country) ? resp.country : "";
	//     callback(countryCode);
	//   });
	// },
	// hiddenInput: "full_number",
	// initialCountry: "auto",
	// localizedCountries: { 'de': 'Deutschland' },
	// nationalMode: false,
	// onlyCountries: ['us', 'gb', 'ch', 'ca', 'do'],
	// placeholderNumberType: "MOBILE",
	// preferredCountries: ['cn', 'jp'],
	// separateDialCode: true,
	utilsScript: "/Assets/js/utils.js",
});