var categories;
var lang = 'en';
var CarsCompareArray = []; //for car compare
if (!localStorage.getItem("carCompareArray")) {
    localStorage.setItem("carCompareArray", "[]");
}

//if (typeof Cookies.get('lang') != "undefined") {
//	lang = Cookies.get('lang');
//	if (lang == 'ar') {
//		window.location.pathname = window.location.pathname.includes('en/') ? window.location.pathname.replace('en/', 'ar/') : window.location.pathname = '/ar' + window.location.pathname;
//	}
//} else {
//	lang = 'en'
//	Cookies.set('lang', 'en', { expires: 1 });
//}

var currency = "AED";
var IsLoyaltyEnabled;

$(document).ready(function () {

    GetWislistCount();

    /*language Toggle*/
    //$("#language li").click(function () {
    //	if ($(this).attr('value') == "ar") {
    //		if (lang == 'en') {
    //			Cookies.set('lang', 'ar', { expires: 1 });
    //			window.location.pathname.includes('en/') ? window.location.pathname.replace('en/', 'ar/') : window.location.pathname = '/ar' + window.location.pathname;
    //		}
    //	}
    //	else {
    //		if (lang == 'ar') {
    //			Cookies.set('lang', 'en', { expires: 1 });
    //			window.location.pathname = window.location.pathname.replace('ar/', '');
    //		}
    //	}
    //});

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
                                            $('#siteNav .parent[id=' + val + ']').find('.brands').append('<li class="lvl-2"><a href="/brand/' + v.slug + '" class="site-nav lvl-2">' + v.name + '</a></li>');
                                        } else {
                                            $('#siteNav .parent[id=' + category.ParentID + ']').find('.brands').append('<li class="lvl-2"><a href="/brand/' + v.slug + '" class="site-nav lvl-2">' + v.name + '</a></li>');
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
            $(this).closest('.form-group').find('.input-group-text i').addClass('anm-spinner4').addClass('fa-spin').removeClass('anm-search-l');

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
                                    $('.searched-result-cars .dataset').append('<a class="car" id="' + v.ID + '" href="/car/' + v.Slug + '">'
                                        + '<img class="car-image" src="' + v.Image + '"/>'
                                        + '<h4>' + v.Title + '</h4>'
                                        + '</a>');
                                } else if (v.Section == 'Vendor') {
                                    $('.searched-result-vendors .dataset').append('<a class="vendor" id="' + v.ID + '" href="/store/' + v.Slug + '">'
                                        + '<img class="car-image" src="' + v.Image + '"/>'
                                        + '<h4>' + v.Title + '</h4>'
                                        + '</a>');
                                }
                                else if (v.Section == 'Category') {
                                    $('.searched-result-categories .dataset').append('<a class="category" id="' + v.ID + '" href="/car-category/' + v.Slug + '">'
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

                            $('input[name=q]').closest('.form-group').find('.input-group-text i').removeClass('anm-spinner4').removeClass('fa-spin').addClass('anm-search-l');
                        }
                        else {
                            $('.search-results').slideUp();
                            $('input[name=q]').closest('.form-group').find('.input-group-text i').removeClass('anm-spinner4').removeClass('fa-spin').addClass('anm-search-l');
                        }
                    } else {
                        $('.search-results').slideUp();
                        $('input[name=q]').closest('.form-group').find('.input-group-text i').removeClass('anm-spinner4').removeClass('fa-spin').addClass('anm-search-l');
                    }
                }
            });
        } else {
            $('.searched-result-cars .dataset').html('');
            $('.searched-result-vendors .dataset').html('');
            $('.searched-result-categories .dataset').html('');
            $('.search-results').slideUp();
            $('input[name=q]').closest('.form-group').find('.input-group-text i').removeClass('anm-spinner4').removeClass('fa-spin').addClass('anm-search-l');
        }
    });


    var prComapre = localStorage.getItem("carCompareArray");
    if (prComapre != "[0]" && prComapre != "[]" && prComapre != "[null]") {
        CarsCompareArray = JSON.parse(localStorage.getItem("carCompareArray"));
    }

    RefreshCarCompareCount(CarsCompareArray);

});
//Ready function End

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
                    console.log("Error");
                }
            }
        });
    }
    else {
        $('#message').html('<span class="fa fa-warning"></span> Select cars for comparison.');
        $('#message').slideDown();
    }

    setTimeout(function () {
        $('#message').slideUp();
    }, 5000);
}

function BindNavigationBar(data) {

    data = data.filter(function (obj) {
        return obj.DisplayOnHeader == 1
    });

    var Parents = data.filter(function (obj) {
        return obj.ParentID == null
        //&& obj.treeNodeLevel == 1
    });

    $('#siteNav').empty();
    $('#siteNav').append(`<li class="lvl1 parent megamenu">
	                        <a href="/"> <i class="fa fa-home fa-2x" style="position: relative;top: 3px;"></i> Home</a>
						</li>`);

    /*First Level Category Binding*/
    $.each(Parents, function (k, v) {
        if (v.hasChilds == true) {
            $('#siteNav').append(`<li class="lvl1 parent megamenu" id="car-category${v.ID}" parent-id="${v.ParentID}">
										<a  href="javascript:;" >${v.Name}</a>
										<div class ="megamenu style1">
											<ul class ="grid mmWrapper">
												<li class ="grid__item large-up--one-whole">
													<ul class ="grid categories">
													</ul>
												</div>
											</ul>
										</div>
									</li>`);

            /*Second Level Category Binding*/
            var Childs = data.filter(function (obj) {
                return obj.ParentID == v.ID
            });

            $.each(Childs, function (k2, v2) {
                if (v.hasChilds == true) {
                    let lvl2container = $(`#siteNav .parent[id=car-category${v2.ParentID}]`).find('.grid.categories');
                    $(lvl2container).append(`<li class="grid__item lvl-1 col-md-3 col-lg-3" id="car-category${v2.ID}" parent-id="${v2.ParentID}">
											    <a href="/car-category/${v2.Slug}" class ="site-nav lvl-1">${v2.Name}</a>
											    <ul class ="subLinks categories">
											    </ul>
											</li>`);

                    /*Third Level Category Binding*/
                    lvl2Childs = data.filter(function (obj) {
                        return obj.ParentID == v2.ID
                    });
                    $.each(lvl2Childs, function (k3, v3) {
                        let lvl3container = $(`#siteNav .grid__item[id=car-category${v3.ParentID}]`).find('.subLinks.categories');
                        $(lvl3container).append(`<li class="lvl-2"><a href="/car-category/${v3.Slug}" class="site-nav lvl-2">${v3.Name}</a></li>`);

                    });

                } else {
                    let lvl2container = $(`#siteNav .parent[id=car-category${v2.ParentID}]`).find('.grid.categories');
                    $(lvl2container).append(`<li class="grid__item lvl-1 col-md-3 col-lg-3" id="car-category${v2.ID}" parent-id="${v2.ParentID}">
											    <a href="/car-category/${v2.Slug}" class ="site-nav lvl-1">${v2.Name}</a>
											</li>`);
                }
            });
        } else {
            $('#siteNav').append(`<li class="lvl1 parent megamenu" id="car-category${v.id}" parent-id="${v.ParentID}">
										<a  href="/car-category/${v.Slug}">${v.Name}</a>
									</li>`);
        }
    });

    $('#siteNav').append(`<li class="nav-item hs-has-mega-menu u-header__nav-item"
										data-event="hover"
										data-animation-in="slideInUp"
										data-animation-out="fadeOut"
										data-position="left">
										<a id="homeMegaMenu" class ="nav-link u-header__nav-link" href="/shop" aria-haspopup="true" aria-expanded="false"> All Categories</a>
									</li>`);

    // initialization of header
    //$.HSCore.components.HSHeader.init($('#header'));
}

//function BindNavigationBar(data) {

//    data = data.filter(function (obj) {
//        return obj.DisplayOnHeader == 1
//    });

//    var Parents = data.filter(function (obj) {
//        return obj.ParentID == null
//    });

//    var Childs = data.filter(function (obj) {
//        return obj.ParentID != null
//    });

//    $('#siteNav').empty();
//    $('#siteNav').append('<li class="lvl1 parent megamenu">'
//        + '	<a href="/"> <i class="fa fa-home fa-2x" style="position: relative;top: 3px;"></i> Home</a> '
//        + '</li>');

//    $.each(Parents, function (k, v) {
//        if (v.hasChilds == true) {
//            $('#siteNav').append('<li class="lvl1 parent megamenu" id="' + v.ID + '">'
//                + '	<a href="javascript:;">' + v.Name + ' <i class="fa fa-angle-down"></i></a>'
//                + '	<div class="megamenu style4">'
//                + '		<ul class="grid grid--uniform mmWrapper">'
//                + '			<li class="grid__item lvl-1 col-md-4 col-lg-4">'
//                + '				<a href="javascript:;" class="site-nav lvl-1">SHOP BY CATEGORY </a>'
//                + '				<ul class="subLinks categories">'
//                + '				</ul>'
//                + '			</li>'
//                + '			<li class="grid__item lvl-1 col-md-4 col-lg-4">'
//                + '				<a href="javascript:;" class="site-nav lvl-1">SHOP BY BRAND</a>'
//                + '				<ul class="subLinks  brands">'
//                + '				</ul>'
//                + '			</li>'
//                + '			<li class="grid__item lvl-1 col-md-4 col-lg-4">'
//                + '				<a href="javascript:;"><img src="' + v.Cover + ' " alt="" title="" /></a>'
//                + '			</li>'
//                + '		</ul>'
//                + '	</div>'
//                + '</li>');
//        } else {
//            $('#siteNav').append('<li class="lvl1 parent megamenu" id="' + v.ID + '">'
//                + '	<a href="/car-category/' + v.Slug + '">' + v.Name
//                + '</li>');
//        }
//    });

//    $.each(Childs, function (k, v) {
//        $('#siteNav .parent[id=' + v.ParentID + ']').find('.categories').append('<li class="lvl-2"><a href="/car-category/' + v.Slug + '" class="site-nav lvl-2">' + v.Name + '</a></li>');
//    });

//    $('#siteNav').append('<li class="lvl1 parent megamenu">'
//        + '	<a href="/shop"> ALL CATEGORIES'
//        + '</li>');
//}

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
        + '	<a href="/"> HOME'
        + '</li>');
    $('#MobileNav').append('<li class="lvl1 parent megamenu">'
        + '	<a href="/shop"> ALL CATEGORIES'
        + '</li>');
    $.each(Parents, function (k, v) {
        if (v.hasChilds == true) {
            $('#MobileNav').append('<li class="lvl1 parent megamenu" id="' + v.ID + '">'
                + '	<a href="javascript:;">' + v.Name + ' <i class="anm anm-plus-l"></i></a>'
                + '		<ul class="categories">'
                //+ '			<li>'
                //+ '				<a href="javascript:;" class="site-nav">SHOP BY CATEGORY <i class="anm anm-plus-l"></i></a>'
                //+ '				<ul class="categories">'
                //+ '				</ul>'
                //+ '			</li>'
                //+ '			<li>'
                //+ '				<a href="javascript:;" class="site-nav">SHOP BY BRAND <i class="anm anm-plus-l"></i></a>'
                //+ '				<ul class="brands">'
                //+ '				</ul>'
                //+ '			</li>'
                + '		</ul>'
                + '</li>');
        } else {
            $('#MobileNav').append('<li class="lvl1 parent megamenu" id="' + v.ID + '">'
                + '	<a href="/car-category/' + v.Slug + '">' + v.Name + '</a>'
                + '</li>');
        }
    });

    $.each(Childs, function (k, v) {
        $('#MobileNav .parent[id=' + v.ParentID + ']').find('.categories').append('<li class=""><a href="/car-category/' + v.Slug + '" class="site-nav">' + v.Name + '</a></li>');
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
                url: '/Wishlist/AddCarToWishlist/' + id,
                contentType: "application/json",
                success: function (response) {
                    if (response.success) {
                        $('#message').html('<span class="fa fa-check"></span> ' + response.message)
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

                    GetWislistCount();

                },
                error: function (er) {
                    if (er.status == 403) {
                        window.location = er.responseJSON.LogOnUrl;
                    }
                }
            });
        } else {
            $('#message').html('<span class="fa fa-check"></span> Car added to wishlist successfully')
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

                        $(element).html('<i class="anm anm-heart-l icon icon-2x"></i>');
                        $(element).attr('onclick', 'AddCarToWishlist(this,' + wish.CarID + ')');

                        $('#message').html('<span class="fa fa-check"></span> ' + response.message)
                        $('#message').slideDown();

                    } else {
                        $('#message').html('<span class="fa fa-warning"></span> ' + response.message)
                        $('#message').slideDown();
                    }

                    setTimeout(function () {
                        $('#message').slideUp();
                    }, 3000);
                    GetWislistCount();
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

            $('#message').html('<span class="fa fa-check"></span> Car removed from your wishlist!')
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

function BindBusiness(data) {

    //Social Links
    if (data.Facebook) {
        $('#social-icons').append(`
                    <li>
                        <a href="${data.Facebook}" target="_blank" class="social-icons__link" title="Maru Deals Social Links">
                            <i class="icon icon-facebook"></i>
                        </a>
                    </li>`);
    }
    if (data.Instagram) {
        $('#social-icons').append(`
					 <li>
                        <a href="${data.Instagram}" target="_blank" class="social-icons__link" title="Maru Deals Social Links">
                            <i class="icon icon-instagram"></i>
                            <span class="icon__fallback-text"></span>
                        </a>
                    </li>`);
    }
    if (data.Youtube) {
        $('#social-icons').append(`
                    <li>
                        <a href="${data.Youtube}" target="_blank" class="social-icons__link" title="Maru Deals Social Links">
                            <i class="icon icon-youtube"></i>
                            <span class="icon__fallback-text"></span>
                        </a>
                    </li>`);
    }
    if (data.Twitter) {
        $('#social-icons').append(`
                     <li>
                        <a href="${data.Twitter}" target="_blank" class="social-icons__link" title="Maru Deals Social Links">
                            <i class="icon icon-twitter "></i>
                            <span class="icon__fallback-text"></span>
                        </a>
                    </li>`);
    }
    if (data.Snapchat) {
        $('#social-icons').append(`
                    <li>
                        <a href="${data.Snapchat}" target="_blank" class="social-icons__link" title="Maru Deals Social Links">
                            <i class="fa fa-snapchat fa-lg"></i>
                        </a>
                    </li>`);
    }
    if (data.LinkedIn) {
        $('#social-icons').append(`
                    <li>
                        <a href="${data.LinkedIn}" target="_blank" class="social-icons__link" title="Maru Deals Social Links">
                            <i class="icon icon-linkedin"></i>
                            <span class="icon__fallback-text"></span>
                        </a>
                    </li>`);
    }
    if (data.Behance) {
        $('#social-icons').append(`
                    <li>
                        <a href="${data.Behance}" target="_blank" class="social-icons__link" title="Maru Deals Social Links">
                            <i class="fa fa-behance fa-lg"></i>
                            <span class="icon__fallback-text"></span>
                        </a>
                    </li>`);
    }
    if (data.Pinterest) {
        $('#social-icons').append(`
                    <li>
                        <a href="${data.Pinterest}" target="_blank" class="social-icons__link" title="Maru Deals Social Links">
                            <i class="icon icon-pinterest"></i>
                            <span class="icon__fallback-text"></span>
                        </a>
                    </li>`);
    }
    //Social Links ends

    // Contact Us
    if (data.StreetAddress) {
        $('#contact_footer').append(`<li>
                                        <i class="icon anm anm-map-marker-al"></i><p id="contact_address">${data.StreetAddress}</p>
                                    </li>`);
    }
    if (data.Fax) {
        $('#contact_footer').append(`<li class="fax">
                                        <i class="icon anm anm-fax"></i><p id="contact_fax">${data.Fax}</p>
                                    </li>`);
    }
    if (data.Contact) {
        $('#contact_footer').append(`<li class="phone">
                                        <i class="icon anm anm-phone-s"></i><p id="contact_phone1">${data.Contact}</p>
                                    </li>`);

        $('.phone-no').html(`<i class="anm anm-phone-s"></i> ${data.Contact}`);
    }
    if (data.Contact2) {
        $('#contact_footer').append(`<li class="phone">
                                        <i class="icon anm anm-phone-s"></i><p id="contact_phone2">${data.Contact2}</p>
                                    </li>`);
    }
    if (data.Email) {
        $('#contact_footer').append(`<li class="email">
                                        <i class="icon anm anm-envelope-l"></i><p id="contact_email">${data.Email}</p>
                                    </li>`);
    }
    // Contact Us

    if (data.Whatsapp) {
        $('#whatsapp_link').html(`<i class="fa fa-whatsapp"></i> ${data.Title}`)
        $('#whatsapp_link').attr("href", `https://api.whatsapp.com/send?phone=${data.Whatsapp}&text=${encodeURIComponent(data.FirstMessage)}`)

    } else {
        $('#whatsapp_link').remove();
    }

    IsLoyaltyEnabled = data.IsLoyaltyEnabled;

    if (!IsLoyaltyEnabled) {
        $('.redeem-amount-container').remove();
    }
    if (!data.IsMaruCompare) {
        $('.MaruCompareBanner').remove();
    }
    else {
        $('.MaruCompareBanner').show();
    }
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

//Compare Car Functionality End

function OnErrorImage() {
    var errorURL = "/Assets/images/default.jpg";
    $('.img-lazyload').each(function () {
        var $this = $(this),
            src = $this.attr('src');

        var img = new Image();
        img.onload = function () {
            $this.attr('src', src);
        }
        img.onerror = function () {
            $this.attr('src', errorURL);
        }
        img.src = src;
    });
}

$(".logout-anchor").click(function () {
    localStorage.setItem("wishlist", "[]");
    window.location.href = `/${culture}/home/index`;
});

