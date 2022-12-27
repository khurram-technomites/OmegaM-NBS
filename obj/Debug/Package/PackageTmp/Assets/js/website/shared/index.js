'use strict';

//#region Global Variables and Arrays
var categories;
var culture = 'en-ae';
var lang = 'en';
var CarsCompareArray = []; //for car compare
if (!localStorage.getItem("carCompareArray")) {
	localStorage.setItem("carCompareArray", "[]");
}
var currency = "AED";
var IsLoyaltyEnabled;
var CheckUrl = true;
var DefaultAvatarUrl = "/Assets/images/default/default-omw-avatar.png";
var DefaultImageUrl = "/Assets/images/default/default-omw-image.png";
var DefaultMapMarker = "/Assets/images/icons/marker-purple.png";

if ($.cookie("_culture") != "undefined") {
	culture = $.cookie("_culture");
	//if (lang == 'ar') {
	//window.location.pathname = window.location.pathname.includes('en/') ? window.location.pathname.replace('en/', 'ar/') : window.location.pathname = '/ar' + window.location.pathname;
	//}
} else {
	culture = 'en-ae'
	$.cookie('_culture', 'en-ae', { expires: 365 });
}
//#region Change Url Path if lang and region is not define
Check_Url();
//#endregion

if (culture.includes('-')) {
	lang = culture.split('-')[0];
}
var ServerError = lang == 'en' ? "Ooops, something went wrong.Try to refresh this page or feel free to contact us if the problem persists." : "عفوًا ، حدث خطأ ما. حاول تحديث هذه الصفحة أو لا تتردد في الاتصال بنا إذا استمرت المشكلة.";

var ServerErrorShort = lang == 'en' ? "Ooops, something went wrong. Please Try Later !" : "عفوًا ، حدث خطأ ما. يرجى المحاولة لاحقًا!";

var InfoEmail = "info@nowbuysell.com";
//#endregion

//#region document ready function
$(document).ready(function () {

	$('#message').click(function () { $(this).html('').hide() });

	//#region Language change submit function
	$('#wo-lang').change(function () {
		$('.ReturnUrl').val(window.location.pathname);
		$("#SetCulture").submit();
	});
	//#endregion

	GetAndBindBusinessSetting();

	/*Signup for NewsLetter*/
	$('#formSubscribe').submit(function () {
		SlideDownToasterMessage('<span class="fa fa-hour-watch ' + margin(1) + ' "></span>', ChangeString("Signing up for newsletter ...", "الاشتراك في النشرة الإخبارية ..."), 6);

		$.ajax({
			type: 'POST',
			url: '/' + culture + '/subscribers/',
			data: $('#formSubscribe').serialize(),
			//data: {
			//	__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
			//	email: $('.input-group__field newsletter__input').val()
			//},
			success: function (response) {
				if (response.success) {
					SlideDownToasterMessage('<span class="fa fa-check-circle ' + margin(1) + ' "></span>', response.message, 6);
				} else {
					if (response.message == 'Subscriber already exist  ...' && lang != 'en')
						response.message = 'المشترك موجود بالفعل ...';
					SlideDownToasterMessage('<span class="fas fa-exclamation-triangle ' + margin(1) + ' "></span>', response.message, 6);
				}
				$('#EmailID').val('');
			},
			error: function (e) {
				console.log("Form Subscribe Error.");
				SlideDownToasterMessage('<span class="fas fa-exclamation-triangle ' + margin(1) + ' "></span>', ServerErrorShort, 6);
			}
		});
		return false;
	});

	var prComapre = localStorage.getItem("carCompareArray");
	if (prComapre != "[0]" && prComapre != "[]" && prComapre != "[null]") {
		CarsCompareArray = JSON.parse(localStorage.getItem("carCompareArray"));
	}

	RefreshCarCompareCount(CarsCompareArray);

	OnErrorImage(1.5);  //after 1.5 seconds, this function checks every image on-page, if the image's broken, then the default image would be shown.
	OnErrorImage(6);    //after 6 seconds, Recheck Images

	//#region Rating Work
	$(".rating-star-custom i").hover(
		function () {
			$(this).addClass("filled").prevAll(".fa-star").addClass("filled");
		}, function () {
			$(".rating-star-custom i").removeClass("filled");
		}
	);
	$(".rating-star-custom i").click(function () {
		$(".rating-star-custom i").removeClass("selected");
		$(this).addClass("filled selected").prevAll(".fa-star").addClass("filled selected");
	});
	//#endregion

	//#region Feedback Work
	$('#Feedback-Form').submit(function () {

		var form = $(this);
		ButtonDisabled('#feedback-submit-btn', true, true);
		ButtonDisabled('#feedback-close-btn', true, false);
		var Rating = $('#Feedback-Rating i.selected').length;
		$('#FeedbackRating').val(parseFloat(Rating));

		if (Rating && Rating > 0) {
			ShowFormAlert(form, 'dark', ChangeString('Please wait ...', 'ارجوك انتظر ...'), 3);
			$.ajax({
				url: $(form).attr('action'),
				type: 'Post',
				data: $(form).serialize(),
				success: function (response) {
					if (response.success) {
						ButtonDisabled('#feedback-submit-btn', false, false);
						ButtonDisabled('#feedback-close-btn', false, false);
						$('#Feedback-Modal').slideUp();
						$('#Feedback-Thankyou').slideDown();
					} else {
						ShowFormAlert(form, 'danger', response.message, 6);
						ButtonDisabled('#feedback-submit-btn', false, false);
						ButtonDisabled('#feedback-close-btn', false, false);
					}
				},
				error: function (e) {
					ShowFormAlert(form, 'danger', ServerErrorShort, 6);
					ButtonDisabled('#feedback-submit-btn', false, false);
					ButtonDisabled('#feedback-close-btn', false, false);
				},
				failure: function (e) {
					ShowFormAlert(form, 'danger', ServerErrorShort, 6);
					ButtonDisabled('#feedback-submit-btn', false, false);
					ButtonDisabled('#feedback-close-btn', false, false);
				}
			});
		}
		else {
			ShowFormAlert(form, 'danger', ChangeString("Please give appropriate rating first!", "يرجى إعطاء التصنيف المناسب أولا!"), 6);
			ButtonDisabled('#feedback-submit-btn', false, false);
			ButtonDisabled('#feedback-close-btn', false, false);
		}
		return false;
	});
	//#endregion

	if (session) {
		GetAndBindNewNotificationsCount();
	}

});
//#endregion

//#region Ajax Call

function GetAndBindBusinessSetting() {
	/*Fecth and Bind Business Setting*/
	$.ajax({
		type: 'Get',
		url: '/contact-setting/',
		success: function (response) {
			if (response.success) {
				BindBusiness(response.data);
			} else {
				console.log("GetAndBindBusinessSetting() Error.");
			}
		}
		,
		error: function (e) {
			console.log("Business Setting Error!");
		}
	});
}

function GetAndBindNewNotificationsCount() {
	$.ajax({
		type: 'Get',
		url: '/' + culture + '/Customer/Notification/GetNewNotificationCount',
		success: function (data) {
			if (data.success) {
				$('.web-alerts-count').text(data.data);
			}
		}
	});
}

//#endregion

//#region Functions for Binding Data

function BindBusiness(data) {
	//#region Social Links

	if ((data.Facebook && data.Facebook != '-')
		|| (data.Instagram && data.Instagram != '-')
		|| (data.Youtube && data.Youtube != '-')
		|| (data.Twitter && data.Twitter != '-')
		|| (data.Snapchat && data.Snapchat != '-')
		|| (data.LinkedIn && data.LinkedIn != '-')
		|| (data.Behance && data.Behance != '-')
		|| (data.Pinterest && data.Pinterest != '-')) {
		$('.web-social-icons').append(`<li><span>${lang == "en" ? "Follow Us:" : "تابعنا:"}</span></li>`);
	}
	if (data.Facebook && data.Facebook != '-') {
		$('.web-social-icons').append(`<li class="wo-facebook">
									    <a href="${data.Facebook}" target="_blank">
										    <i class="fab fa-facebook-f"></i>
									    </a>
								    </li>`);
	}
	if (data.Youtube && data.Youtube != '-') {
		$('.web-social-icons').append(`<li class="wo-youtube">
									    <a href="${data.Youtube}" target="_blank">
										    <i class="fab fa-youtube"></i>
									    </a>
								    </li>`);
	}
	if (data.Twitter && data.Twitter != '-') {
		$('.web-social-icons').append(`<li class="wo-twitter">
									    <a href="${data.Twitter}" target="_blank">
										    <i class="fab fa-twitter"></i>
									    </a>
								    </li>`);
	}
	if (data.LinkedIn && data.LinkedIn != '-') {
		$('.web-social-icons').append(`<li class="wo-linkedin">
									    <a href="${data.LinkedIn}" target="_blank">
										    <i class="fab fa-linkedin-in"></i>
									    </a>
								    </li>`);
	}
	if (data.Instagram && data.Instagram != '-') {
		$('.web-social-icons').append(`<li class="wo-instagram">
									    <a href="${data.Instagram}" target="_blank">
										    <i class="fab fa-instagram"></i>
									    </a>
								    </li>`);
	}
	if (data.Snapchat && data.Snapchat != '-') {
		$('.web-social-icons').append(`<li class="wo-snapchat">
									    <a href="${data.Snapchat}" target="_blank">
										    <i class="fab fa-snapchat-ghost"></i>
									    </a>
								    </li>`);
	}
	if (data.Behance && data.Behance != '-') {
		$('.web-social-icons').append(`<li class="wo-behance">
									    <a href="${data.Behance}" target="_blank">
										    <i class="fab fa-behance"></i>
									    </a>
								    </li>`);
	}
	if (data.Pinterest && data.Pinterest != '-') {
		$('.web-social-icons').append(`<li class="wo-pinteres">
									    <a href="${data.Pinterest}" target="_blank">
										    <i class="fab fa-pinterest-p"></i>
									    </a>
								    </li>`);
	}
	//#endregion Social Links ends

	//#region Contact Us
	if (data.Contact && data.Contact != '-') {
		$('.web-contact-info').append(`	<li class="${lang == "en" ? "" : "text-right"}">
											<a href="tel:+${data.Contact}"><i class="fa fa-phone-alt ${lang == "en" ? "" : "fa-flip-horizontal"} "></i>
												<bdi>${data.Contact}</bdi>
											</a>
										</li>`);
	}
	if (data.StreetAddress && data.StreetAddress != '-') {
		$('.web-contact-info').append(`	<li class="${lang == "en" ? "" : "text-right"}">
											<address><i class="fa fa-map-marker-alt"></i>${lang == "en" ? data.StreetAddress : " " + data.StreetAddressAr}</address>
										</li>`);
	}
	if (data.Email && data.Email != '-') {
		InfoEmail = data.Email;
	}
	//if (data.Fax) {
	//	$('#contact_footer').append(`<li class="fax">
	//                                       <i class="icon anm anm-fax"></i><p id="contact_fax">${data.Fax}</p>
	//                                   </li>`);
	//}

	//if (data.Contact2) {
	//	$('#contact_footer').append(`<li class="phone">
	//                                       <i class="icon anm anm-phone-s"></i><p id="contact_phone2">${data.Contact2}</p>
	//                                   </li>`);
	//}
	//if (data.Email) {
	//	$('#contact_footer').append(`<li class="email">
	//                                       <i class="icon anm anm-envelope-l"></i><p id="contact_email">${data.Email}</p>
	//                                   </li>`);
	//}
	//#endregion Contact Us

	//#region Whatsapp
	if (data.Whatsapp && data.Whatsapp != '-') {
		$('#whatsapp_link').html(`<i class="fab fa-whatsapp"></i> ${lang == "en" ? data.Title : data.TitleAr}`)
		$('#whatsapp_link').attr("href", `https://api.whatsapp.com/send?phone=${data.Whatsapp}&text=${encodeURIComponent(lang == "en" ? data.FirstMessage : data.FirstMessageAr)}`)
	} else {
		$('#whatsapp_link').remove();
	}
	//#endregion

	//#region Others Checks
	//IsLoyaltyEnabled = data.IsLoyaltyEnabled;

	//if (!IsLoyaltyEnabled) {
	//    $('.redeem-amount-container').remove();
	//}
	//if (!data.IsMaruCompare) {
	//    $('.MaruCompareBanner').remove();
	//}
	//else {
	//    $('.MaruCompareBanner').show();
	//
	//#endregion
}

//#endregion

//#region Others Function

function CarsCompareCheckArray() {
	if (CarsCompareArray.length > 0) {
		$.each(CarsCompareArray, function (k, v) {

			$('#car-compare-' + v).attr('data-type', '0').html('Remove From Compare');

		});
	}
}

function CarsCompareCheckArrayItem(id) {

}

function CarCompareForm() {

	if (CarsCompareArray.length > 0) {

		$.ajax({
			url: '/compare/cars-json',
			type: "post",
			dataType: "json",
			contextType: "application/json",
			data: { IDs: CarsCompareArray },
			traditional: true,
			success: function (response) {
				if (response.success) {
					window.location = response.url;
				} else {
					console.log("Compare Error");
				}
			},
			error: function (e) {
				console.log("Compare Error");
			}
		});
	}
	else {
		$('#message').html('<span class="fas fa-exclamation-triangle"></span> Select cars for comparison.');
		$('#message').slideDown();
	}

	setTimeout(function () {
		$('#message').slideUp();
	}, 5000);
}

function BindCoupons(data) {

	$('#couponpromotion').empty()
	$.each(data, function (k, v) {
		//'<marquee behavior="scroll" direction="left" scrollamount="10"><div class=""><i class="fa fa-gift"></i> ' + v.name + '</div></marquee>'
		$('#couponpromotion').append('				<i class="fa fa-gift"></i> ' + v.name);
	});
}

function AddCarToWishlist(element, id) {

	if (typeof session != "undefined" && session == true) {
		$('#message').html('<span class="fa fa-hour-watch"></span> Adding to wishlist ...')
		$('#message').slideDown();
		$('#message').slideDown();

		var wish = localStorage.getItem("wishlist");
		if (wish) {
			wishlist = JSON.parse(wish);
		}

		wish = wishlist.find(function (obj) {
			return obj.CarID == id;
		});
		if (!wish) {
			$.ajax({
				type: 'PUT',
				url: "/" + culture + '/Wishlist/AddCarToWishlist/' + id,
				contentType: "application/json",
				success: function (response) {
					if (response.success) {
						$('#message').html('<span class="fa fa-check-circle"></span> ' + response.message)
						$('#message').slideDown();

						wishlist.push({ ID: response.wishId, CarID: id });
						localStorage.setItem("wishlist", JSON.stringify(wishlist));

						$(element).html('<i class="ti-heart" style="color:#f54337;"></i> Remove From Wishlist');
						$(element).attr('onclick', 'DeleteCarFromWishlist(this,' + response.wishId + ')');

					} else {
						$('#message').html('<span class="fas fa-exclamation-triangle"></span> ' + response.message)
						$('#message').slideDown();
					}

					setTimeout(function () {
						$('#message').slideUp();
					}, 3000);

					GetWislistCount();

				},
				error: function (e) {
					if (e.status == 403) {
						window.location = er.responseJSON.LogOnUrl;
					}
				}
			});
		} else {
			$('#message').html('<span class="fa fa-check-circle"></span> Car added to wishlist successfully')
			$('#message').slideDown();

			$(element).html('<i class="anm anm-heart icon icon-2x icon-light" style="color:#f54337;"></i>');
			$(element).attr('onclick', 'DeleteCarFromWishlist(this,' + wish.ID + ')');


			setTimeout(function () {
				$('#message').slideUp();
			}, 3000);
			GetWislistCount();
		}
	}
	else {
		window.location = "/Customer/Account/Login";
	}

}

function DeleteCarFromWishlist(element, id) {

	if (typeof session != "undefined" && session == true) {
		$('#message').html('<span class="fa fa-hour-watch"></span> Removing wish ...')
		$('#message').slideDown();
		var wish = localStorage.getItem("wishlist");
		if (wish) {
			wishlist = JSON.parse(wish);
		}

		wish = wishlist.find(function (obj) {
			return obj.ID == id;
		});
		if (wish) {
			$.ajax({
				type: 'DELETE',
				url: '/Wishlist/' + id,
				contentType: "application/json",
				success: function (response) {
					if (response.success) {

						wishlist = wishlist.filter(function (obj) {
							return obj.ID !== id;
						});

						localStorage.setItem("wishlist", JSON.stringify(wishlist));

						$(element).html('<i class="ti-heart"></i> Add To Wishlist');
						$(element).attr('onclick', 'AddCarToWishlist(this,' + wish.CarID + ')');

						$('#message').html('<span class="fa fa-check-circle"></span> ' + response.message)
						$('#message').slideDown();

					} else {
						$('#message').html('<span class="fas fa-exclamation-triangle"></span> ' + response.message)
						$('#message').slideDown();
					}

					setTimeout(function () {
						$('#message').slideUp();
					}, 3000);
					GetWislistCount();
				},
				error: function (e) {
					if (e.status == 403) {
						window.location = er.responseJSON.LogOnUrl;
					}
				}
			});
		} else {

			$(element).html('<i class="anm anm-heart-l icon icon-2x"></i>');
			$(element).attr('onclick', 'AddCarToWishlist(this,' + $(element).closest('.item').attr('id') + ')');

			$('#message').html('<span class="fa fa-check-circle"></span> Car removed from your wishlist!')
			$('#message').slideDown();


			setTimeout(function () {
				$('#message').slideUp();
			}, 3000);
			GetWislistCount();
		}
	}
	else {
		window.location = "/Customer/Account/Login";
	}

}

function FormatPrices() {
	$('span.price,span.old-price,.money').each(function (k, v) {
		if (!$(v).hasClass('formatted')) {
			let text = $(v).text();
			if (text.includes('-')) {
				text = text.split('-');
				let min = text[0].replace('AED', '').trim();
				let max = text[1].replace('AED', '').trim();

				$(v).html('AED ' + numeral(min).format('0,0.00') + ' - AED ' + numeral(max).format('0,0.00'));
			} else {
				if (text.includes('AED')) {
					text = text.replace('AED', '').trim();
					$(v).html('AED ' + numeral(text).format('0,0.00'));
				} else {
					$(v).html(numeral(text).format('0,0.00'));
				}
			}
			$(v).addClass('formatted');
		}
	});

	$('span.points').each(function (k, v) {
		if (!$(v).hasClass('formatted')) {
			let text = $(v).text();

			$(v).html(numeral(text).format('0,0'));

			$(v).addClass('formatted');
		}
	});
}

function FormatPrice(v) {
	let text = $(v).text();
	if (text.includes('-')) {
		text = text.split('-');
		let min = text[0].replace('AED', '').trim();
		let max = text[1].replace('AED', '').trim();

		$(v).html('AED ' + numeral(min).format('0,0.00') + ' - AED ' + numeral(max).format('0,0.00'));
	} else {
		if (text.includes('AED')) {
			text = text.replace('AED', '').trim();
			$(v).html('AED ' + numeral(text).format('0,0.00'));
		} else {
			$(v).html(numeral(text).format('0,0.00'));
		}
	}
}

function FormatAmount(v, hasDoublePrice = true) {
	let text = $(v).text();
	if (hasDoublePrice && text.includes('-')) {
		text = text.split('-');
		let min = text[0].replace(currency, '').trim();
		let max = text[1].replace(currency, '').trim();

		$(v).html(`${currency} ${numeral(min).format('0,0.00')} - ${currency} ${numeral(max).format('0,0.00')}`);
	} else {
		if (text.includes(currency)) {
			text = text.replace(currency, '').trim();
			$(v).html(`${currency} ${numeral(text).format('0,0.00')}`);
		} else {
			$(v).html(numeral(text).format('0,0.00'));
		}
	}
}

function FormatPoint(v) {
	let text = $(v).text();
	$(v).html(numeral(text).format('0,0'));
}

function addScore(score, $domElement) {
	$("<span class='stars-container'>")
		.addClass("stars-" + score.toString())
		.text("★★★★★")
		.appendTo($domElement);
}

//Compare Car Functionality
function CarCompareArrayFunction(element, id) {

	id = parseFloat(id);
	if ($(element).prop('id') == "CompareBadge") {
		if ($(element).attr('data-type') == '1') {
			$(element).attr('data-type', '0').attr('title', 'Remove from Compare').html('<i class="anm anm-random-r icon icon-2x" aria-hidden="true" style="color: #f54337;"></i>');
			CarsCompareArray.push(id);
		}
		else {
			$(element).attr('data-type', '1').attr('title', 'Add to Compare').html('<i class="icon anm anm-random-r icon icon-2x" aria-hidden="true"></i>');
			CarsCompareArray = removeCarItem(CarsCompareArray, id);
		}
	}
	else {
		if ($(element).attr('data-type') == '1') {
			$(element).attr('data-type', '0').html('Remove From Compare');
			CarsCompareArray.push(id);
		}
		else {
			$(element).attr('data-type', '1').html('Add To Compare');
			CarsCompareArray = removeCarItem(CarsCompareArray, id);
		}
	}


	RefreshCarCompareCount(CarsCompareArray);
}

function RefreshCarCompareCount(array) {

	if (array && array.length > 0) {
		$('.compare-count').addClass("site-header__compare-count compare-style").html(array.length);
		$('.compare-count-mobile').addClass("site-header__compare-count compare-style-mobile").html(array.length);

		localStorage.setItem("carCompareArray", JSON.stringify(array));
	}
	else {
		$('.compare-count').removeClass("site-header__compare-count compare-style").html('');
		$('.compare-count-mobile').removeClass("site-header__compare-count compare-style-mobile").html('');
		localStorage.setItem("carCompareArray", "[]");
	}
}

function removeCarItem(originalArray, itemRemove) {
	return originalArray.filter(function (array) { return array !== itemRemove });
}

function DeleteCarItem(id) {
	CarsCompareArray = removeCarItem(CarsCompareArray, id);
	RefreshCarCompareCount(CarsCompareArray);
}

function GetWislistCount() {

	var wish = localStorage.getItem("wishlist");
	if (wish) {
		WishlistCount(JSON.parse(wish).length)
	} else {
		WishlistCount(0);
	}
}

function WishlistCount(count) {
	if (count > 0) {
		$('.wishlist-count').addClass("site-header__wishlist-count wishlist-style").html(count);
		$('.wishlist-count-mobile').addClass("site-header__wishlist-count wishlist-style-mobile").html(count);
	}
	else {
		$('.wishlist-count').removeClass("site-header__wishlist-count wishlist-style").html('');
		$('.wishlist-count-mobile').removeClass("site-header__wishlist-count wishlist-style-mobile").html('');
	}
}

//#region Feedback and Suggestions

function ShowFeedbackView() {
	$('#Feedback-Modal').show();
	$('#Feedback-Thankyou').hide();
	$('.feedback-fields').val('');
	$(".rating-star-custom i").removeClass("selected");
	$('#FeedbackRating').val(0);
	//DisableEmojis();

	if (session) {
		$('#FeedbackName').attr('readonly', true).val($('.customer-name').val());
		$('#FeedbackEmail').attr('readonly', true).val($('.customer-email').val());
	}
	else {
		$('#FeedbackName').attr('readonly', false).val('');
		$('#FeedbackEmail').attr('readonly', false).val('');
	}
	$('#feedbackpopup').modal('show');
}

$('.feedback-close-btn').click(function () {
	$('#feedbackpopup').modal('hide');
	$('.feedback-fields').val('');
	$(".rating-star-custom i").removeClass("selected");
	$('#FeedbackRating').val(0);

	$('#Feedback-Modal').slideDown();
	$('#Feedback-Thankyou').slideUp();
	//DisableEmojis();
});

$('#btn-become-a-vendor').click(function () {
	$('#Vendor-Rendor').html(`
            <div class="text-center">
				<span class="fa fa-circle-notch fa-spin fa-2x"></span>
			</div>
            `);
	$('#Vendor-Rendor').load(`/${culture}/Vendors/Signup`);
	$('#vendorpopup').modal('show');
});

//function HoverEmoji(elem) {
//	if ($(elem).attr('src').includes('disable')) {
//		var src = $(elem).attr('src').replace('disable', 'active');
//		$(elem).attr('src', src);
//	}
//}

//function HoverEmojiOut(elem) {
//	if ($(elem).attr('src').includes('active') && $(elem).attr('data-id') == 0) {
//		var src = $(elem).attr('src').replace('active', 'disable');
//		$(elem).attr('src', src);
//	}
//}

//$('.feedback-emoji-icon').click(function () {
//	var emoji = $(this);
//	if (DisableEmojis()) {
//		var src = $(emoji).attr('src').replace('disable', 'active');
//		$(emoji).attr('src', src).attr('data-id', '1');
//		$('#FeedbackExperience').val($(emoji).attr('alt'));
//	}
//});

//var DisableEmojis = function disableEmojis() {
//	$.each($('.feedback-emoji-icon'), function (k, v) {
//		if ($(v).attr('src').includes('active')) {
//			var src = $(v).attr('src').replace('active', 'disable');
//			$(v).attr('src', src).attr('data-id', '0');
//		}
//	});
//	return true;
//}
//#endregion

function scroll_top() {
	$("#site-scroll").on("click", function () {
		//var body = $("html, body");
		//body.stop().animate({ scrollTop: 0 }, 500, 'swing');
		$("html, body").animate({ scrollTop: 0 }, 1000);
		return false;
	});
}
scroll_top();

$(window).scroll(function () {
	if ($(this).scrollTop() > 300) {
		$("#site-scroll").fadeIn();
	} else {
		$("#site-scroll").fadeOut();
	}
});

function SlideDownToasterMessage(span, msg, timeup = 0) {
	$('#ToasterMessage').html(span + " " + msg).slideDown();
	if (timeup && timeup > 0) {
		setTimeout(function () {
			$('#ToasterMessage').slideUp();
		}, (timeup * 1000));
	}
}
function SlideUpToasterMessage(timeup) {
	setTimeout(function () {
		$('#ToasterMessage').slideUp();
	}, (timeup * 1000));
}

var ShowFormAlert = function (form, type, msg, timeup = 0) {
	var alert = $(`
					<div class= "alert alert-${type} ${lang == "en" ? "text-left" : "text-right"}" role = "alert">
						<button type="button" class="close" data-dismiss="alert" aria-label="Close">
							<i class="fa fa-times small"></i>
						</button>
						<span></span>
					</div>
				`);
	form.find('.alert').remove();
	alert.prependTo(form);
	$(alert).slideDown();
	alert.find('span').html(msg);

	if (timeup && timeup > 0) {
		setTimeout(function () {
			$(alert).slideUp();
		}, (timeup * 1000));
	}
}

var HideFormAlert = function (type) {
	var alert = $(`
					<div class= "alert alert-${type} ${lang == "en" ? "text-left" : "text-right"}" role = "alert">
						<button type="button" class="close" data-dismiss="alert" aria-label="Close">
							<i class="fa fa-times small"></i>
						</button>
						<span></span>
					</div>
				`);

	$(alert).slideUp();
}

var ShowFormAlertOffset = function (form, type, msg, offsetElem, timeup = 0) {

	var alert = $(`
					<div class= "alert alert-${type}" role = "alert">
						<button type="button" class="close" data-dismiss="alert" aria-label="Close">
							<i class="fa fa-times small"></i>
						</button>
						<span></span>
					</div>
				`);

	form.find('.alert').remove();
	if (offsetElem) {
		offsetElem.after(alert);
	}
	else {
		alert.prependTo(form);
	}
	$(alert).slideDown();
	alert.find('span').html(msg);
	if (timeup && timeup > 0) {
		setTimeout(function () {
			$(alert).slideUp();
		}, (timeup * 1000));
	}
}

if ($('#SuccessMessage').val()) {
	//SlideDownToasterMessage('<span class="fa fa-check-circle ' + margin(1) + ' "></span>', $('#SuccessMessage').val(), 10);
	ShowSweetAlert('success', ChangeString("Ok!", "حسنا!"), $('#SuccessMessage').val(), '#40194e');
}
if ($('#ErrorMessage').val()) {
	//SlideDownToasterMessage('<span class="fa fa-check-circle ' + margin(1) + ' "></span>', $('#ErrorMessage').val(), 10);
	ShowSweetAlert('error', ChangeString("Oops...", "وجه الفتاة..."), $('#ErrorMessage').val(), '#40194e');
}

function ShowSweetAlert(Icon, Title, Text, cnfrm_btn_color, callbackFunction) {
	Swal.fire({
		icon: Icon,
		title: Title,
		text: Text,
		confirmButtonColor: cnfrm_btn_color,
	});
}

function ShowSweetAlertBtnText(Icon, Title, Text, cnfrm_btn_color, cnfrm_btn_text, callbackFunction) {
	Swal.fire({
		icon: Icon,
		title: Title,
		text: Text,
		confirmButtonColor: cnfrm_btn_color,
		confirmButtonText: cnfrm_btn_text
	});
}

function BorderDangerInput(id, timeup = 0) {

	$(id).closest('div').addClass('border border-danger');

	if (timeup && timeup > 0) {
		setTimeout(function () {
			$(id).closest('div').removeClass('border border-danger');
		}, (timeup * 1000));
	}
}
function BorderDangerInputRemove(id, timeup) {
	setTimeout(function () {
		$(id).closest('div').removeClass('border border-danger');
	}, (timeup * 1000));
}

function ButtonDisabled(id, flag = false, span = false) {
	if (span) {
		$(id).html('<span class="fa fa-circle-notch fa-spin ' + margin(1) + '"></span> ' + $(id).text()).attr('disabled', flag);
	}
	else {
		$(id).html($(id).text()).attr('disabled', flag);
	}
}

function margin(num) {
	return lang == "en" ? "mr-" + num : "ml-" + num;
}
function padding(num) {
	return lang == "en" ? "pr-" + num : "pl-" + num;
}

function ChangeString(en_text, ar_text) {
	return lang == 'en' ? en_text : ar_text;
};

function PageReload(url, timeup = 3) {
	setTimeout(function () {
		window.location.href = url;
	}, timeup * 1000);
}

//$(".web-logout").click(function () {
//    if (localStorage.getItem("wishlist")) {
//        localStorage.setItem("wishlist", "[]");
//    }
//    window.location.href = `/${culture}/Customer/Account/Logout`;
//});

function OnErrorImage(timeup = 3) {
	setTimeout(function () {
		$('img:not(.img-avatar)').each(function () {
			var $this = $(this),
				src = $this.attr('src');

			var img = new Image();
			img.onload = function () {
				$this.attr('src', src);
			}
			img.onerror = function () {
				$this.attr('src', DefaultImageUrl);
			}
			img.src = src;
		});

		$('.img-avatar').each(function () {
			var $this = $(this),
				src = $this.attr('src');

			var img = new Image();
			img.onload = function () {
				$this.attr('src', src);
			}
			img.onerror = function () {
				$this.attr('src', DefaultAvatarUrl);
			}
			img.src = src;
		});

	}, (timeup * 1000));
}

function Check_Url() {

	if (window.location.pathname.toLowerCase().includes('/en-us/') || window.location.pathname.toLowerCase().includes('/ar-us/')) {
		if (window.location.pathname.toLowerCase().includes('/en-us/') && CheckUrl == true) {
			CheckUrl = false;
			window.location.pathname = window.location.pathname.replace('/en-us/', '/en-ae/');
		}
		else if (window.location.pathname.toLowerCase().includes('/ar-us/') && CheckUrl == true) {
			CheckUrl = false;
			window.location.pathname = window.location.pathname.replace('/ar-us/', '/ar-ae/');
		}
	}

	if (window.location.pathname.toLowerCase().includes('/en/') || window.location.pathname.toLowerCase().includes('/ar/')) {
		if (window.location.pathname.toLowerCase().includes('/en/') && CheckUrl == true) {
			CheckUrl = false;
			window.location.pathname = window.location.pathname.replace('/en/', '/en-ae/');
		}
		else if (window.location.pathname.toLowerCase().includes('/ar/') && CheckUrl == true) {
			CheckUrl = false;
			window.location.pathname = window.location.pathname.replace('/ar/', '/ar-ae/');
		}
	}
	if (window.location.pathname.toLowerCase().includes('/en-/') || window.location.pathname.toLowerCase().includes('/ar-/')) {
		if (window.location.pathname.toLowerCase().includes('/en-/') && CheckUrl == true) {
			CheckUrl = false;
			window.location.pathname = window.location.pathname.replace('/en-/', '/en-ae/');
		}
		else if (window.location.pathname.toLowerCase().includes('/ar-/') && CheckUrl == true) {
			CheckUrl = false;
			window.location.pathname = window.location.pathname.replace('/ar-/', '/ar-ae/');
		}
	}
	else if (!window.location.pathname.toLowerCase().includes('/en-ae/') && !window.location.pathname.toLowerCase().includes('/ar-ae/')) {
		if (CheckUrl == true) {
			CheckUrl = false;
			window.location.pathname = "/" + culture + window.location.pathname;
		}
	}

	if (location.hash && !session) {
		location.hash = '';
	}

	//Default_Url();
}

function Default_Url() {
	window.location.pathname = '/' + culture + '/home/index';
}

function getParameterByName(name, url = window.location.href) {
	name = name.replace(/[\[\]]/g, '\\$&');
	var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
		results = regex.exec(url);
	if (!results) return null;
	if (!results[2]) return '';
	return decodeURIComponent(results[2].replace(/\+/g, ' '));
}
//#endregion

function PopulateSchedule() {
	$('#StartDate,#EndDate').off('change');

	$('#StartTime,#EndTime').clockTimePicker('dispose');

	var currentDate = new Date();
	var startDate = new Date($("#StartDate").val());
	var endDate = new Date($("#EndDate").val());
	var packageId = $('.car-packages .package.selected').attr('data');

	if (startDate > currentDate) {

		$('#StartTime').clockTimePicker({
			duration: true,
			precision: 60
		});

		$('#EndTime').clockTimePicker({
			duration: true,
			precision: 60
		});

		$('#StartTime').val(currentDate.getHours() + ':00');
		if (Number(packageId) === 1) {
			currentDate.addHours(1);
		}
		$('#EndTime').val(currentDate.getHours() + ':00');
	} else {
		$('#StartTime').clockTimePicker({
			duration: true,
			precision: 60,
			minimum: currentDate.addHours(3).getHours() + ':00',
		});
		$('#StartTime').val(currentDate.getHours() + ':00');
		if (Number(packageId) === 1) {
			currentDate.addHours(1);
		}
		$('#EndTime').clockTimePicker({
			duration: true,
			precision: 60,
			minimum: currentDate.getHours() + ':00',
		});
		$('#EndTime').val(currentDate.getHours() + ':00');
	}

	switch (Number(packageId)) {
		case 1:
			$("#EndDate").datepicker("setStartDate", startDate);
			$('#EndDate').datepicker('setDate', startDate);

			break;
		case 2:

			var diffInMilliSeconds = Math.abs(endDate - startDate) / 1000;
			// calculate days
			var days = Math.floor(diffInMilliSeconds / 86400);
			days = days <= 0 ? 1 : days;

			startDate = startDate.addDays(days)
			$("#EndDate").datepicker("setStartDate", startDate);
			$('#EndDate').datepicker('setDate', startDate);


			break;
		case 3:

			var diffInMilliSeconds = Math.abs(endDate - startDate) / 1000;
			// calculate days
			var days = Math.floor(diffInMilliSeconds / 86400);
			days = days <= 0 ? 1 : days;

			if (days % 7 !== 0) {
				var daysToAdd = days + (7 - (days % 7))

				startDate = startDate.addDays(daysToAdd)
				$("#EndDate").datepicker("setStartDate", startDate);
				$('#EndDate').datepicker('setDate', startDate);
			}

			break;
		case 4:

			var months;
			months = (endDate.getFullYear() - startDate.getFullYear()) * 12;
			months -= startDate.getMonth();
			months += endDate.getMonth();
			months = months <= 0 ? 1 : months;

			 startDate.setMonth(startDate.getMonth() + months);
			$("#EndDate").datepicker("setStartDate", startDate);
			$('#EndDate').datepicker('setDate', startDate);

			break;
		default:
	}

	$('#StartDate,#EndDate').change(function (event) {
		event.preventDefault()
		PopulateSchedule();
	});

	if (typeof CalulateTotal !== "undefined") {
		CalulateTotal();
	}
}

Date.prototype.addDays = function (days) {
	var date = new Date(this.valueOf());
	date.setDate(date.getDate() + days);
	return date;
}

Date.prototype.addHours = function (h) {
	this.setTime(this.getTime() + (h * 60 * 60 * 1000));
	return this;
}

