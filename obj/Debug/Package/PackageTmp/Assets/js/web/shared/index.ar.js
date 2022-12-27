var categories;
var lang = 'ar';
var currency = 'درهم';

if (typeof Cookies.get('lang') != "undefined") {
    lang = Cookies.get('lang');

    if (lang == 'en') {
        window.location.pathname = window.location.pathname.replace('ar/', '');
    }
} else {
    lang = 'ar'
    Cookies.set('lang', 'ar', { expires: 1 });
}

$(document).ready(function () {

    /*language Toggle*/
    $("#language li").click(function () {
        if ($(this).attr('value') == "ar") {
            if (lang == 'en') {
                Cookies.set('lang', 'ar', { expires: 1 });
                window.location.pathname = window.location.pathname.includes('en/') ? window.location.pathname.replace('en/', 'ar/') : window.location.pathname = '/ar' + window.location.pathname;
            }

            //$(this).closest('.language-dd').text('AR');
            //lang = 'ar';
            //Cookies.set('lang', 'ar', { expires: 1 });
        }
        else {
            if (lang == 'ar') {
                Cookies.set('lang', 'en', { expires: 1 });
                window.location.pathname = window.location.pathname.replace('ar/', '');
            }

            //$(this).closest('.language-dd').text('EN');
            //lang = 'en';
            //Cookies.set('lang', 'en', { expires: 1 });
        }
    });

    /*Fetch and Bind Header*/
    $.ajax({
        type: 'Get',
        url: '/' + lang + '/categories/',
        success: function (response) {
            if (response.success) {
                categories = response.data;
                BindNavigationBar(response.data);
                BindMobileNavigationBar(response.data);

                if (typeof BindFilterCategories != "undefined") {
                    BindFilterCategories(response.data);

                    var category = categories.find(function (obj) {
                        return obj.Slug == GetURLParameter();
                    })
                    if (category) {
                        filter.categoryID = category.ID;
                    }

                    GetCars();
                    BindFilters();
                }
                if (typeof BindCategory != "undefined") {
                    BindCategory(response.data);
                }

                /*9. Mobile Menu */
                var selectors = {
                    body: 'body',
                    sitenav: '#siteNav',
                    navLinks: '#siteNav .lvl1 > a',
                    menuToggle: '.js-mobile-nav-toggle',
                    mobilenav: '.mobile-nav-wrapper',
                    menuLinks: '#MobileNav .anm',
                    closemenu: '.closemobileMenu'
                };

                $(selectors.navLinks).each(function () {
                    if ($(this).attr('href') == window.location.pathname) $(this).addClass('active');
                })

                $(selectors.menuToggle).on("click", function () {
                    body: 'body',
                        $(selectors.mobilenav).toggleClass("active");
                    $(selectors.body).toggleClass("menuOn");
                    $(selectors.menuToggle).toggleClass('mobile-nav--open mobile-nav--close');
                });

                $(selectors.closemenu).on("click", function () {
                    body: 'body',
                        $(selectors.mobilenav).toggleClass("active");
                    $(selectors.body).toggleClass("menuOn");
                    $(selectors.menuToggle).toggleClass('mobile-nav--open mobile-nav--close');
                });
                $("body").on('click', function (event) {
                    var $target = $(event.target);
                    if (!$target.parents().is(selectors.mobilenav) && !$target.parents().is(selectors.menuToggle) && !$target.is(selectors.menuToggle)) {
                        $(selectors.mobilenav).removeClass("active");
                        $(selectors.body).removeClass("menuOn");
                        $(selectors.menuToggle).removeClass('mobile-nav--close').addClass('mobile-nav--open');
                    }
                });
                $(selectors.menuLinks).on('click', function (e) {
                    e.preventDefault();
                    $(this).toggleClass('anm-plus-l anm-minus-l');
                    $(this).parent().next().slideToggle();
                });

                /*Fetch and Bind Brands*/
                $.ajax({
                    type: 'Get',
                    url: '/' + lang + '/brands/',
                    success: function (response) {
                        if (response.success) {
                            $('#Brands').empty()
                            $.each(response.data, function (k, v) {
                                $.each(v.categories, function (ind, val) {
                                    let category = categories.find(function (obj) {
                                        return obj.ID == val
                                    });
                                    if (category) {
                                        if (category.ParentID == null) {
                                            $('#siteNav .parent[id=' + val + ']').find('.brands').append('<li class="lvl-2"><a href="/' + lang + '/brand/' + v.slug + '" class="site-nav lvl-2">' + v.name + '</a></li>');
                                        } else {
                                            $('#siteNav .parent[id=' + category.ParentIDk + ']').find('.brands').append('<li class="lvl-2"><a href="/' + lang + '/brand/' + v.slug + '" class="site-nav lvl-2">' + v.name + '</a></li>');
                                        }
                                    }
                                });
                            });

                            if (typeof BindLovedBrands != "undefined") {
                                BindLovedBrands(response.data);
                            }

                            if (typeof BindBrands != "undefined") {
                                BindBrands(response.data);
                            }

                        } else {
                        }
                    }
                });

            } else {
            }
        }
    });

    /*Fecth and Bind Header Coupon Marque*/
    $.ajax({
        type: 'Get',
        url: '/en/coupons/',
        success: function (response) {
            if (response.success) {
                BindCoupons(response.data);
            } else {
            }
        }
    });

    /*Fecth and Bind Business Setting*/
    $.ajax({
        type: 'Get',
        url: '/contact-setting/',
        success: function (response) {
            if (response.success) {
                BindBusiness(response.data);
            } else {
            }
        }
    });

    /*Signup for NewsLetter*/
    $('#formSubscribe').submit(function () {
        $('#message').html('<span class="fa fa-hour-watch"></span> signing up for newsletter ...')
        $('#message').slideDown();

        $.ajax({
            type: 'POST',
            url: '/subscribers/',
            data: $('#formSubscribe').serialize(),
            //data: {
            //	__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
            //	email: $('.input-group__field newsletter__input').val()
            //},
            success: function (response) {
                if (response.success) {
                    $('#message').html('<span class="fa fa-check"></span> ' + response.message)
                    $('#message').slideDown();
                    $('#EmailID').val('');

                } else {
                    $('#EmailID').val('');
                    $('#message').html('<span class="fa fa-warning"></span> ' + response.message)
                    $('#message').slideDown();
                }

                setTimeout(function () {
                    $('#message').slideUp();

                }, 3000);
            },
            error: function (er) {

            }
        });
        return false;
    });

    /*Promotion banner */
    $('input[name=q]').keyup(function () {
        if ($(this).val()) {
            $.ajax({
                type: 'Post',
                url: '/' + lang + '/search',
                data: $('#FormSearch').serialize(),
                success: function (response) {
                    if (response.success) {
                        if (response.data && response.data.length > 0) {
                            $('.searched-result-cars .dataset').html('');
                            $('.searched-result-vendors .dataset').html('');
                            $('.searched-result-categories .dataset').html('');
                            $.each(response.data, function (k, v) {
                                if (v.Section == 'Car') {
                                    $('.searched-result-cars .dataset').append('<a class="car" id="' + v.ID + '" href="/' + lang + '/car/' + v.Slug + '">'
                                        + '<img class="car-image" src="' + v.Image + '"/>'
                                        + '<h4>' + v.Title + '</h4>'
                                        + '</a>');
                                } else if (v.Section == 'Vendor') {
                                    $('.searched-result-vendors .dataset').append('<a class="vendor" id="' + v.ID + '" href="/' + lang + '/store/' + v.Slug + '">'
                                        + '<img class="car-image" src="' + v.Image + '"/>'
                                        + '<h4>' + v.Title + '</h4>'
                                        + '</a>');
                                }
                                else if (v.Section == 'Category') {
                                    $('.searched-result-categories .dataset').append('<a class="category" id="' + v.ID + '" href="/' + lang + '/car-category/' + v.Slug + '">'
                                        + '<img class="car-image" src="' + v.Image + '"/>'
                                        + '<h4>' + v.Title + '</h4>'
                                        + '</a>');
                                }

                                if (!$('.searched-result-cars .dataset').html().trim()) {
                                    $('.searched-result-cars').hide();
                                } else {
                                    $('.searched-result-cars').show();
                                }

                                if (!$('.searched-result-vendors .dataset').html().trim()) {
                                    $('.searched-result-vendors').hide();
                                } else {
                                    $('.searched-result-vendors').show();
                                }

                                if (!$('.searched-result-categories .dataset').html().trim()) {
                                    $('.searched-result-categories').hide('');
                                } else {
                                    $('.searched-result-categories').show();
                                }

                            });
                            $('.search-results').slideDown();
                        }
                        else {
                            $('.search-results').slideUp();
                        }
                    } else {
                        $('.search-results').slideUp();
                    }
                }
            });
        } else {
            $('.searched-result-cars .dataset').html('');
            $('.searched-result-vendors .dataset').html('');
            $('.searched-result-categories .dataset').html('');
            $('.search-results').slideUp();
        }
    });

});

function BindNavigationBar(data) {

    data = data.filter(function (obj) {
        return obj.DisplayOnHeader == 1
    });

    var Parents = data.filter(function (obj) {
        return obj.ParentID == null
    });

    var Childs = data.filter(function (obj) {
        return obj.ParentID != null
    });

    $('#siteNav').empty();
    $('#siteNav').prepend('<li class="lvl1 parent megamenu">'
        + '	<a href="/" title="Home"> الصفحة الرئيسية<i class="fa fa-home fa-2x" style="position: relative;top: 3px;"></i></a> '
        + '</li>');

    $.each(Parents, function (k, v) {
        if (v.hasChilds == true) {
            $('#siteNav').prepend('<li class="lvl1 parent megamenu" id="' + v.ID + '">'
                + '	<a href="javascript:;">' + v.Name + ' <i class="fa fa-angle-down"></i></a>'
                + '	<div class="megamenu style4">'
                + '		<ul class="grid grid--uniform mmWrapper">'
                + '			<li class="grid__item lvl-1 col-md-4 col-lg-4">'
                + '				<a href="javascript:;"><img src="' + v.Cover + ' " alt="" title="" /></a>'
                + '			</li>'
                + '			<li class="grid__item lvl-1 col-md-4 col-lg-4 text-center" dir="rtl">'
                + '				<a href="javascript:;" class="site-nav lvl-1"  title="SHOP BY BRAND">تسوق وفق الماركة</a>'
                + '				<ul class="subLinks  brands">'
                + '				</ul>'
                + '			</li>'
                + '			<li class="grid__item lvl-1 col-md-4 col-lg-4 text-center" dir="rtl">'
                + '				<a href="javascript:;" class="site-nav lvl-1" title="SHOP BY CATEGORY">تسوق حسب الاقسام </a>'
                + '				<ul class="subLinks categories">'
                + '				</ul>'
                + '			</li>'
                + '		</ul>'
                + '	</div>'
                + '</li>');
        } else {
            $('#siteNav').prepend('<li class="lvl1 parent megamenu" id="' + v.ID + '">'
                + '	<a href="/ar/Car-category/' + v.Slug + '">' + v.Name
                + '</li>');
        }
    });

    $.each(Childs, function (k, v) {
        $('#siteNav .parent[id=' + v.ParentID + ']').find('.categories').append('<li class="lvl-2"><a href="/ar/Car-category/' + v.Slug + '" class="site-nav lvl-2">' + v.Name + '</a></li>');
    });

    $('#siteNav').prepend('<li class="lvl1 parent megamenu">'
        + '	<a href="/' + lang + '/shop"> جميع الفئات '
        + '</li>');
}

function BindMobileNavigationBar(data) {

    data = data.filter(function (obj) {
        return obj.DisplayOnHeader == 1
    });

    var Parents = data.filter(function (obj) {
        return obj.ParentID == null
    });

    var Childs = data.filter(function (obj) {
        return obj.ParentID != null
    });

    $('#MobileNav').empty();
    $('#MobileNav').append('<li class="lvl1 parent megamenu">'
        + '	<a href="/"> الصفحة الرئيسية'
        + '</li>');

    $.each(Parents, function (k, v) {
        if (v.hasChilds == true) {
            $('#MobileNav').append('<li class="lvl1 parent megamenu" id="' + v.ID + '">'
                + '	<a href="javascript:;">' + v.Name + ' <i class="anm anm-plus-l"></i></a>'
                + '		<ul>'
                + '			<li>'
                + '				<a href="javascript:;" class="site-nav" title="SHOP BY CATEGORY">تسوق حسب الاقسام <i class="anm anm-plus-l"></i></a>'
                + '				<ul class="categories">'
                + '				</ul>'
                + '			</li>'
                + '			<li>'
                + '				<a href="javascript:;" class="site-nav" title="SHOP BY BRAND">تسوق وفق الماركة <i class="anm anm-plus-l"></i></a>'
                + '				<ul class="brands">'
                + '				</ul>'
                + '			</li>'
                + '		</ul>'
                + '</li>');
        } else {
            $('#MobileNav').append('<li class="lvl1 parent megamenu" id="' + v.ID + '">'
                + '	<a href="/' + lang + '/car-category/' + v.Slug + '">' + v.Name + '</a>'
                + '</li>');
        }
    });

    $('#MobileNav').append('<li class="lvl1 parent megamenu">'
        + '	<a href="/' + lang + '/shop" title="ALL CATEGORIES"> جميع الفئات'
        + '</li>');

    $.each(Childs, function (k, v) {
        $('#MobileNav .parent[id=' + v.ParentID + ']').find('.categories').append('<li class=""><a href="/' + lang + '/car-category/' + v.Slug + '" class="site-nav">' + v.Name + '</a></li>');
    });
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
        $('#message').html('جارٍ الإضافة إلى قائمة الرغبات ...  <span class="fa fa-hour-watch"></span>')
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
                url: '/Wishlist/AddCarToWishlist/' + id,
                contentType: "application/json",
                success: function (response) {
                    if (response.success) {
                        $('#message').html('تمت إضافة المنتج إلى قائمة الرغبات بنجاح  <span class="fa fa-check"></span>')
                        $('#message').slideDown();

                        wishlist.push({ ID: response.wishId, CarID: id });
                        localStorage.setItem("wishlist", JSON.stringify(wishlist));

                        $(element).html('<i class="anm anm-heart icon icon-2x icon-light" style="color:#f54337;"></i>');
                        $(element).attr('onclick', 'DeleteCarFromWishlist(this,' + response.wishId + ')');

                    } else {
                        $('#message').html('<span class="fa fa-warning"></span> ' + response.message)
                        $('#message').slideDown();
                    }

                    setTimeout(function () {
                        $('#message').slideUp();
                    }, 3000);
                },
                error: function (er) {
                    if (er.status == 403) {
                        window.location = er.responseJSON.LogOnUrl;
                    }
                }
            });
        } else {
            $('#message').html('تمت إضافة المنتج إلى قائمة الرغبات بنجاح  <span class="fa fa-check"></span>')
            $('#message').slideDown();

            $(element).html('<i class="anm anm-heart icon icon-2x icon-light" style="color:#f54337;"></i>');
            $(element).attr('onclick', 'DeleteCarFromWishlist(this,' + wish.ID + ')');


            setTimeout(function () {
                $('#message').slideUp();
            }, 3000);
        }
    }
    else {
        window.location = "/Customer/Account/Login";
    }
}

function DeleteCarFromWishlist(element, id) {

    if (typeof session != "undefined" && session == true) {
        $('#message').html('إزالة الرغبة ...  <span class="fa fa-hour-watch"></span>')
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

                        $(element).html('<i class="anm anm-heart-l icon icon-2x"></i>');
                        $(element).attr('onclick', 'AddCarToWishlist(this,' + wish.CarID + ')');

                        $('#message').html('تمت إزالة المنتج من قائمة الرغبات الخاصة بك!  <span class="fa fa-check"></span>')
                        $('#message').slideDown();

                    } else {
                        $('#message').html('<span class="fa fa-warning"></span> ' + response.message)
                        $('#message').slideDown();
                    }

                    setTimeout(function () {
                        $('#message').slideUp();
                    }, 3000);
                },
                error: function (er) {
                    if (er.status == 403) {
                        window.location = er.responseJSON.LogOnUrl;
                    }
                }
            });
        } else {

            $(element).html('<i class="anm anm-heart-l icon icon-2x"></i>');
            $(element).attr('onclick', 'AddCarToWishlist(this,' + $(element).closest('.item').attr('id') + ')');

            $('#message').html('تمت إزالة المنتج من قائمة الرغبات الخاصة بك!  <span class="fa fa-check"></span>')
            $('#message').slideDown();


            setTimeout(function () {
                $('#message').slideUp();
            }, 3000);
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

                $(v).html(currency + ' ' + numeral(min).format('0,0.00') + ' - ' + currency + ' ' + numeral(max).format('0,0.00'));
            } else {
                if (text.includes('AED') || text.includes(currency)) {
                    text = text.replace('AED', '').trim();
                    text = text.replace(currency, '').trim();
                    $(v).html(currency + ' ' + numeral(text).format('0,0.00'));
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

function addScore(score, $domElement) {
    $("<span class='stars-container'>")
        .addClass("stars-" + score.toString())
        .text("★★★★★")
        .appendTo($domElement);
}

function BindBusiness(data) {
    //Social Links 
    if (data.Facebook) {
        $('#contact_facebook').attr("href", data.Facebook);
    }
    else {
        $('#contact_facebook').closest("li").remove();
    }
    if (data.Instagram) {
        $('#contact_instagram').attr("href", data.Instagram);
    }
    else {
        $('#contact_instagram').closest("li").remove();
    }
    if (data.Youtube) {
        $('#contact_youtube').attr("href", data.Youtube);
    }
    else {
        $('#contact_youtube').closest("li").remove();
    }
    if (data.Twitter) {
        $('#contact_twitter').attr("href", data.Twitter);
    }
    else {
        $('#contact_twitter').closest("li").remove();
    }
    if (data.Snapchat) {
        $('#contact_snapchat').attr("href", data.Snapchat);
    }
    else {
        $('#contact_snapchat').closest("li").remove();
    }
    if (data.LinkedIn) {
        $('#contact_linkedin').attr("href", data.LinkedIn);
    }
    else {
        $('#contact_linkedin').closest("li").remove();
    }
    if (data.Behance) {
        $('#contact_behance').attr("href", data.Behance);
    }
    else {
        $('#contact_behance').closest("li").remove();
    }
    if (data.Pinterest) {
        $('#contact_pinterest').attr("href", data.Pinterest);
    }
    else {
        $('#contact_pinterest').closest("li").remove();
    }
    //Social Links ends

    // Contact Us
    if (data.StreetAddressAr) {
        $('#contact_address').html(data.StreetAddressAr);
    }
    else {
        $('#contact_address').closest("li").remove();
    }
    if (data.Fax) {
        $('#contact_fax').html(data.Fax);
    }
    else {
        $('#contact_fax').closest("li").remove();
    }
    if (data.Contact) {
        $('#contact_phone1').html(data.Contact);
    }
    else {
        $('#contact_phone1').closest("li").remove();
    }
    if (data.Contact2) {
        $('#contact_phone2').html(data.Contact2);
    }
    else {
        $('#contact_phone2').closest("li").remove();
    }
    if (data.Email) {
        $('#contact_email').html(data.Email);
    }
    else {
        $('#contact_email').closest("li").remove();
    }
    // Contact Us

    if (data.Whatsapp) {
        $('#whatsapp_link').html(`<i class="fa fa-whatsapp"></i> ${data.TitleAr}`)
        $('#whatsapp_link').attr("href", `https://api.whatsapp.com/send?phone=${data.Whatsapp}&text=${encodeURIComponent(data.FirstMessage)}`)

    } else {
        $('#whatsapp_link').remove();
    }

}