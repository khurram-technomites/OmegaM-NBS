$(document).ready(function () {

	/*$('body').removeClass('template-index').removeClass('home2-default').addClass('template-car').addClass('belle').addClass('template-car-right-thumb');*/

	$('#showVideo').click(function () {
		$('.newsfeed-video').show();
		$('.newsfeed-banner').hide();
	});

	$('#showBanner').click(function () {
		$('.newsfeed-video').hide();
		$('.newsfeed-banner').show();
	});
});
