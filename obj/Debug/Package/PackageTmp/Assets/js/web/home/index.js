$(document).ready(function () {
    /*Homepage Slideshow */
    $.ajax({
        type: 'Get',
        url: '/en/banners/Website',
        success: function (response) {
            if (response.success) {
                $('#homeSilder').empty()
                $.each(response.data, function (k, v) {
                    $('#homeSilder').append('<div class="slide"  >'
                        + '	<div class="blur-up lazyload" >'
                        + '		<img class="blur-up lazyload" data-src="' + v.Name + '" src="' + v.Name + '" />'
                        + '		<a class="slideshow__text-wrap slideshow__overlay classic middle" href="' + v.Url + '" target="_blank">'
                        + '			<div class="slideshow__text-content middle">'
                        + '				<div class="container">'
                        + '					<div class="wrap-caption center">'
                        + '						<h2 class="h1 mega-title slideshow__title"></h2>'
                        + '						<span class="mega-subtitle slideshow__subtitle" url="' + v.Url + '"></span>'
                        //+ '						<a class="btn" href="' + v.Url + '">Shop now</a>'
                        + '					</div>'
                        + '				</div>'
                        + '			</div>'
                        + '		</a>'
                        + '	</div>'
                        + '</div>');
                });

                $('.home-slideshow').slick({
                    dots: false,
                    infinite: true,
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    fade: true,
                    arrows: true,
                    autoplay: true,
                    autoplaySpeed: 4000,
                    lazyLoad: 'ondemand'
                });

            } else {

            }
        }
    });

    /*Promotion banner */
    $.ajax({
        type: 'Get',
        url: '/en/banners/Promotion',
        success: function (response) {
            if (response.success) {
                $.each(response.data, function (k, v) {
                    $('#PromotionBanner').attr('src', v.Name);
                    $('#PromotionBanner').closest('a').attr('href', v.Url)
                });
            }
        }
    });

    /*featured cars*/
    $.ajax({
        type: 'Get',
        url: '/' + lang + '/featuredcars/',
        success: function (response) {
            if (response.success) {
                BindCars(response.data);
            } else {
            }
        }
    });

    /*offered cars*/
    $.ajax({
        type: 'Get',
        url: '/' + lang + '/offeredcars/',
        success: function (response) {
            if (response.success) {
                BindofferedCars(response.data);
            } else {
            }
        }
    });

    /*recommended cars*/
    $.ajax({
        type: 'Get',
        url: '/' + lang + '/recommendedcars/',
        success: function (response) {
            if (response.success) {
                BindRecommendedCars(response.data);
            } else {
            }
        }
    });

    //Newsfeed
    $.ajax({
        type: 'Get',
        url: '/' + lang + '/news/',
        success: function (response) {
            if (response.success) {
                BindNewsfeed(response.data);
            } else {
            }
        }
    });

    setTimeout(function () { OnErrorImage(); }, 4000);

});

function BindCategory(data) {

    var Parents = data.filter(function (obj) {
        return obj.ParentID == null
    });

    $('#PopularCategory').empty()
    $.each(Parents, function (k, v) {

        $('#PopularCategory').append(`
            
            <div class="col-6 col-sm-3 col-md-3 col-lg-3 item">
                <!-- start car image -->
                <div class="car-image category-car-image" style="border-radius: 20px !important;">
                    <!-- start car image -->
                    <a href="/car-category/${v.Slug}" class="grid-view-item__link">
                        <!-- image -->
                        <img class="image-hover blur-up lazyload img-lazyload category-img" data-src="${v.Cover}" src="${v.Cover}" alt="image" title="car" />
                        <!-- End image -->
                    </a>
                    <!-- end car image -->
                    <!-- Start car button -->
                    <form class="variants add" action="#" style="position: static !important;" onclick="window.location.href='/car-category/${v.Slug}'" method="post">
                        <button class="btn btn-addto-cart" type="button" tabindex="0">${v.Name}</button>
                    </form>
                    <!-- end car button -->
                </div>
                <!-- end car image -->
            </div>

        `);

    });

    
}

function BindCars(data) {

    $('#NewArrivalCars').empty();

    $.each(data, function (k, v) {
        var template = '<div class="col-6 col-sm-2 col-md-3 col-lg-3 item" id=' + v.ID + '>';
        template += '		<!-- start car image -->';
        template += '		<div class="car-image gallery-car-image">';
        template += '			<!-- start car image -->';
        template += '			<a href="/car/' + v.Slug + '" class="grid-view-item__link">';
        template += '				<!-- image -->';
        template += '				<img class="primary blur-up lazyload img-lazyload gallery-img" data-src="' + v.Thumbnail + '" src="' + v.Thumbnail + '" alt="image" title="car">';
        template += '				<!-- End image -->';
        template += '				<!-- Hover image -->';
        template += '				<img class="hover blur-up lazyload img-lazyload gallery-img-hover" data-src="' + v.Image1 + '" src="' + v.Image1 + '" alt="image" title="car">';
        template += '				<!-- End hover image -->';
        //template += '				<img class="grid-view-item__image hover variantImg" src="../assets/images/Category/abaya3.jpg" alt="image" title="car">';
        //template += '				<!-- Variant Image-->';
        template += '				<!-- car label -->';
        template += '				<div class="car-labels rounded">';
        if (v.IsSaleAvailable == true) {
            template += '					<span class="lbl on-sale">Sale</span>';
        }
        //template += '					<span class="lbl pr-label1">new</span>';
        template += '				</div>';
        template += '				<!-- End car label -->';
        template += '			</a>';
        template += '			<!-- end car image -->';
        //template += '			<!-- countdown start -->';
        //template += '			<div class="saleTime desktop" data-countdown="2022/03/01"></div>';
        //template += '			<!-- countdown end -->';
        template += '			<!-- Start car button -->';
        template += '			<div class="variants add">';
        template += '				<button class="btn btn-addto-cart" data-type="1" onclick="CarCompareArrayFunction(this, ' + v.ID + ')" id="car-compare-' + v.ID + '" type="button" tabindex="0">Add To Compare</button>';
        template += '			</div>';
        //template += '			<div class="button-set">';
        //template += '				<a href="javascript:void(0)" title="Quick View" class="quick-view-popup quick-view" data-toggle="modal" data-target="#content_quickview">';
        //template += '					<i class="icon anm anm-search-plus-r"></i>';
        //template += '				</a>';
        //template += '				<div class="wishlist-btn">';
        //template += '					<a class="add-to-wishlist wishlist bg-transparent text-dark" href="#">';
        //template += '						<i class="icon anm anm-heart-l"></i>';
        //template += '					</a>';
        //template += '				</div>';
        //template += '				<div class="compare-btn">';
        //template += '					<a class="compare add-to-compare" href="#" title="Add to Compare">';
        //template += '						<i class="icon anm anm-random-r"></i>';
        //template += '					</a>';
        //template += '				</div>';
        //template += '			</div>';
        template += '			<!-- end car button -->';
        template += '		</div>';
        template += '		<!-- end car image -->';
        template += '		<!--start car details -->';
        template += '		<div class="car-details text-center">';
        template += '			<!-- car name -->';
        template += '			<div class="car-name">';
        template += '				<a href="/car/' + v.Slug + '">' + v.Title + '</a>';
        template += '			</div>';
        template += '			<!-- End car name -->';
        template += '			<!-- car price -->';
        template += '			<div class="car-price">';
        if (v.Type == '1' || v.Type == 'Simple') {
            if (v.IsSaleAvailable == true) {
                template += '				<span class="old-price">AED ' + v.RegularPrice.toString() + '</span>';
                template += '				<span class="price">AED ' + v.SalePrice + '</span>';
            } else {
                template += '				<span class="price">AED ' + v.RegularPrice + '</span>';
            }
        } else {
            if (v.IsSaleAvailable === true) {
                template += '				<span class="old-price">AED ' + v.MinRegularPrice + ' - AED ' + v.MaxRegularPrice + '</span>';
                template += '				<span class="price">AED ' + (v.MinSalePrice ? v.MinSalePrice : v.MinRegularPrice) + ' - AED ' + (v.MaxSalePrice ? v.MaxSalePrice : v.MaxRegularPrice) + '</span>';
            } else {
                template += '				<span class="price">AED ' + v.MinRegularPrice + ' - AED ' + v.MaxRegularPrice + '</span>';
            }
        }

        template += '			</div>';
        template += '			<!-- End car price -->';
        template += '			<!-- Color Variant -->';
        template += '			<!-- End Variant -->';
        template += '		</div>';
        template += '		<!-- End car details -->';
        template += '	</div>';

        $('#NewArrivalCars').append(template);
    });

    

    CarsCompareCheckArray();

    FormatPrices();
}

function BindofferedCars(data) {

    if (data.length > 0) {
        $('.offered-cars-section').show();
    }

    $('#offeredCars').empty();

    $.each(data, function (k, v) {
        var template = '<div class="col-6 col-sm-2 col-md-3 col-lg-3 item" id=' + v.ID + '>';
        template += '		<!-- start car image -->';
        template += '		<div class="car-image gallery-car-image">';
        template += '			<!-- start car image -->';
        template += '			<a href="/car/' + v.Slug + '" class="grid-view-item__link">';
        template += '				<!-- image -->';
        template += '				<img class="primary blur-up lazyload img-lazyload gallery-img" data-src="' + v.Thumbnail + '" src="' + v.Thumbnail + '" alt="image" title="car">';
        template += '				<!-- End image -->';
        template += '				<!-- Hover image -->';
        template += '				<img class="hover blur-up lazyload img-lazyload gallery-img-hover" data-src="' + v.Image1 + '" src="' + v.Image1 + '" alt="image" title="car">';
        template += '				<!-- End hover image -->';
        //template += '				<img class="grid-view-item__image hover variantImg" src="../assets/images/Category/abaya3.jpg" alt="image" title="car">';
        //template += '				<!-- Variant Image-->';
        template += '				<!-- car label -->';
        template += '				<div class="car-labels rounded">';
        if (v.IsSaleAvailable == true) {
            template += '					<span class="lbl on-sale">Sale</span>';
        }
        //template += '					<span class="lbl pr-label1">new</span>';
        template += '				</div>';
        template += '				<!-- End car label -->';
        template += '			</a>';
        template += '			<!-- end car image -->';
        //template += '			<!-- countdown start -->';
        //template += '			<div class="saleTime desktop" data-countdown="2022/03/01"></div>';
        //template += '			<!-- countdown end -->';
        template += '			<!-- Start car button -->';
        template += '			<div class="variants add">';
        template += '				<button class="btn btn-addto-cart" data-type="1" onclick="CarCompareArrayFunction(this, ' + v.ID + ')" id="car-compare-' + v.ID + '" type="button" tabindex="0">Add To Compare</button>';
        template += '			</div>';
        //template += '			<div class="button-set">';
        //template += '				<a href="javascript:void(0)" title="Quick View" class="quick-view-popup quick-view" data-toggle="modal" data-target="#content_quickview">';
        //template += '					<i class="icon anm anm-search-plus-r"></i>';
        //template += '				</a>';
        //template += '				<div class="wishlist-btn">';
        //template += '					<a class="add-to-wishlist wishlist bg-transparent text-dark" href="#">';
        //template += '						<i class="icon anm anm-heart-l"></i>';
        //template += '					</a>';
        //template += '				</div>';
        //template += '				<div class="compare-btn">';
        //template += '					<a class="compare add-to-compare" href="#" title="Add to Compare">';
        //template += '						<i class="icon anm anm-random-r"></i>';
        //template += '					</a>';
        //template += '				</div>';
        //template += '			</div>';
        template += '			<!-- end car button -->';
        template += '		</div>';
        template += '		<!-- end car image -->';
        template += '		<!--start car details -->';
        template += '		<div class="car-details text-center">';
        template += '			<!-- car name -->';
        template += '			<div class="car-name">';
        template += '				<a href="/car/' + v.Slug + '">' + v.Title + '</a>';
        template += '			</div>';
        template += '			<!-- End car name -->';
        template += '			<!-- car price -->';
        template += '			<div class="car-price">';
        if (v.Type == '1' || v.Type == 'Simple') {
            if (v.IsSaleAvailable == true) {
                template += '				<span class="old-price">AED ' + v.RegularPrice.toString() + '</span>';
                template += '				<span class="price">AED ' + v.SalePrice + '</span>';
            } else {
                template += '				<span class="price">AED ' + v.RegularPrice + '</span>';
            }
        } else {
            if (v.IsSaleAvailable === true) {
                template += '				<span class="old-price">AED ' + v.MinRegularPrice + ' - AED ' + v.MaxRegularPrice + '</span>';
                template += '				<span class="price">AED ' + (v.MinSalePrice ? v.MinSalePrice : v.MinRegularPrice) + ' - AED ' + (v.MaxSalePrice ? v.MaxSalePrice : v.MaxRegularPrice) + '</span>';
            } else {
                template += '				<span class="price">AED ' + v.MinRegularPrice + ' - AED ' + v.MaxRegularPrice + '</span>';
            }
        }

        template += '			</div>';
        template += '			<!-- End car price -->';
        template += '			<!-- Color Variant -->';
        template += '			<!-- End Variant -->';
        template += '		</div>';
        template += '		<!-- End car details -->';
        template += '	</div>';

        $('#offeredCars').append(template);
    });

    setTimeout(function () { OnErrorImage(); }, 3000);

    CarsCompareCheckArray();

    FormatPrices();
}

function BindRecommendedCars(cars) {
    if (cars) {

        if (cars.length > 0) {
            $('.recommended-cars').show();
        }

        $('#recommended-cars-main').empty()
        $.each(cars, function (k, v) {

            var price = "";
            if (v.Type == '1' || v.Type == 'Simple') {
                if (v.IsSaleAvailable == true) {
                    price += '				<p class="old-price">AED ' + v.RegularPrice + '</p>';
                    price += '				<p class="price">AED ' + v.SalePrice + '</p>';
                } else {
                    price += '				<p class="price">AED ' + v.RegularPrice + '</p>';
                }
            } else {
                if (v.IsSaleAvailable == true) {
                    price += '				<p class="old-price">AED ' + v.MinRegularPrice + ' - AED ' + v.MaxRegularPrice + '</p>';
                    price += '				<p class="price">AED ' + (v.MinSalePrice ? v.MinSalePrice : v.MinRegularPrice) + ' - AED ' + (v.MaxSalePrice ? v.MaxSalePrice : v.MaxRegularPrice) + '</p>';
                } else {
                    price += '				<p class="price">AED ' + v.MinRegularPrice + ' - AED ' + v.MaxRegularPrice + '</p>';
                }
            }

            var label = "";
            if (v.IsSaleAvailable == true) {
                label += '				    <div class="car-labels rounded" style="left:10px !important;">';
                label += '					    <span class="lbl on-sale">Sale</span>';
                label += '				    </div>';
            }
            $('#recommended-cars-main').append(`
                     <div class="col-12 item">
                        <div class="row">
                            <div class="col-12 item">
                                <div class="car-image">
                                    <a href="/car/${v.Slug}">
                                        <img class="img-fit-slider img-lazyload" src="${v.Thumbnail}" alt="${v.Title}" title="${v.Title}" />
                                    </a>
                                    <div class="variants add slider">
        				                <button class="btn btn-addto-cart" data-type="1" onclick="CarCompareArrayFunction(this, ${v.ID})" id="car-compare-${v.ID}" type="button" tabindex="0">Add To Compare</button>
        		                    </div>
                                </div>
                            </div>
                            <div class="col-12 item" style="background-color: white;">

                                <div class="car-details text-center">
                                    <div class="car-name mt-2">
                                        <a href="/car/${v.Slug}">${v.Title}</a>
                                    </div>

                                    <div class="car-price">
                                            ${price}
                                    </div>

                                    <div class="car-review car-review-recommended" data="${v.Rating ? v.Rating : 0}" NoOfRatings="${v.NoOfRatings}"></div>

                                </div>
                            </div>
                        </div>
                    </div>
            `);

        });

        

        $('.car-review-recommended').each(function (k, v) {
            var rating = parseFloat($(v).attr('data'));
            $(v).removeClass("car-review-recommended").addClass('fa-2x');
            addScore(rating * 20, $(v));
        });

        $('.logo-bar-recommended').slick({
            dots: false,
            infinite: true,
            slidesToShow: 4,
            slidesToScroll: 1,
            autoplay: true,
            autoplaySpeed: 5000,
            arrows: false,
            responsive: [
                {
                    breakpoint: 1024,
                    settings: {
                        slidesToShow: 4,
                        slidesToScroll: 1
                    }
                },
                {
                    breakpoint: 600,
                    settings: {
                        slidesToShow: 3,
                        slidesToScroll: 1
                    }
                },
                {
                    breakpoint: 480,
                    settings: {
                        slidesToShow: 2,
                        slidesToScroll: 1
                    }
                }
            ]
        });
    }

}

function BindVendors(data) {

    $('#Vendors').empty()
    $.each(data, function (k, v) {
        $('#Vendors').append('<div class="logo-bar__item">'
            + '	<img class="img-lazyload" src="' + v.Logo + '" alt="' + v.Name + '" title="' + v.Name + '" />'
            + '</div>');
    });
    
}

function BindLovedBrands(data) {

    $('#Brands').empty()
    $.each(data, function (k, v) {
        $('#Brands').append('<div class="logo-bar__item">'
            + '	<img class="img-lazyload" src="' + v.logo + '" alt="' + v.name + '" title="' + v.name + '" />'
            + '</div>');

    });
    
    $('.logo-bar').slick({
        dots: false,
        infinite: true,
        slidesToShow: 6,
        slidesToScroll: 1,
        arrows: true,
        responsive: [
            {
                breakpoint: 1024,
                settings: {
                    slidesToShow: 4,
                    slidesToScroll: 1
                }
            },
            {
                breakpoint: 600,
                settings: {
                    slidesToShow: 3,
                    slidesToScroll: 1
                }
            },
            {
                breakpoint: 480,
                settings: {
                    slidesToShow: 2,
                    slidesToScroll: 1
                }
            }
        ]
    });
}

function subscribe(data) {
}

function BindNewsfeed(data) {
    $('#NewsFeedGrid').empty()
    $.each(data, function (k, v) {

        $('#NewsFeedGrid').append(`
			 <div class="col-12 col-sm-12 col-md-6 col-lg-6">
                <div class="wrap-blog">
                    <a href="/en/news/${v.Slug}" class="article__grid-image">
                        <img src="${v.BannerImage}" alt="${v.Title}" title="${v.Title}" class="blur-up lazyloaded img-lazyload" />
                    </a>
                    <div class="article__grid-meta article__grid-meta--has-image">
                        <div class="wrap-blog-inner">
                            <h2 class="h3 article__title">
                                <a href="/en/news/${v.Slug}">${v.Title}</a>
                            </h2>
                            <span class="article__date">May 02, 2017</span>
                            <div class="rte article__grid-excerpt text-justify">
                                ${v.TitleDescription}
                            </div>
                            <ul class="list--inline article__meta-buttons">
                                <li><a href="/en/news/${v.Slug}">Read more</a></li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
	`);

    });
    
}