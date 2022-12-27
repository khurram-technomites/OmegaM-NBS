"use strict";
jQuery(document).on('ready', function() {
    /* PRELOADER*/
        jQuery(window).on('load', function() {
        jQuery(".preloader-outer").delay(200).fadeOut();
        jQuery(".loader").delay(100).fadeOut("slow");
    });

    /* MOBILE MENU*/
    function collapseMenu(){
        if (jQuery(window).width() < 1200) {
            jQuery('.menu-item-has-children > a, .page_item_has_children > a').on('click', function() {
            jQuery(this).closest('li').toggleClass('wo-open-menu');
            jQuery(this).next().slideToggle(300);
        });
      }
    }
    collapseMenu();
    /* MAIN HEADER LOCATION DISTANCE START*/
    var woArrowIcon = document.querySelector('.wo-distanceicon')
    if (woArrowIcon !== null) {
        $('.wo-distanceicon').on('click', function() {
            $(this).siblings('.wo-distance').slideToggle("fast");
        })
    }
    /* RANGE FEE SLIDER */
    function ageRangeslider1() {
        jQuery("#wo-themerangeslider").slider({
            range: true,
            min: 0,
            max: 100000,
            values: [40000, 80000],
            slide: function(event, ui) {
                jQuery(".wo-amount").val(ui.values[0] + ' - ' + ui.values[1] + "km");
            }
        });
        jQuery(".wo-amount").val(jQuery("#wo-themerangeslider").slider("values", 0) + ' - ' + jQuery("#wo-themerangeslider").slider("values", 1) + "km");
    }
    if (jQuery("#wo-themerangeslider").length > 0) {
        ageRangeslider1();
    }
    function ageRangeslider2() {
        jQuery("#wo-themerangeslider2").slider({
            range: true,
            min: 0,
            max: 1000,
            values: [120, 500],
            slide: function(event, ui) {
                jQuery(".wo-amount2").val("$" + ui.values[0] + " - $" + ui.values[1]);
            }
        });
        jQuery(".wo-amount2").val("$" + jQuery("#wo-themerangeslider2").slider("values", 0) + " - $" + jQuery("#wo-themerangeslider2").slider("values", 1));
    }
    if (jQuery("#wo-themerangeslider2").length > 0) {
        ageRangeslider2();
    }
    function ageRangeslider3() {
        jQuery("#wo-themerangeslider3").slider({
            range: true,
            min: 0,
            max: 1000,
            values: [120, 500],
            slide: function(event, ui) {
                jQuery(".wo-amount3").val("$" + ui.values[0] + " - $" + ui.values[1]);
            }
        });
        jQuery(".wo-amount3").val("$" + jQuery("#wo-themerangeslider3").slider("values", 0) + " - $" + jQuery("#wo-themerangeslider3").slider("values", 1));
    }
    if (jQuery("#wo-themerangeslider3").length > 0) {
        ageRangeslider3();
    }
    /* SLIDER RANGE MIN START */
    var sliderRangeMin = document.getElementById('slider-range-min')
    if (sliderRangeMin !== null) {
        $(function() {
            $("#slider-range-min").slider({
                range: "min",
                value: 70,
                min: 1,
                max: 700,
                slide: function(event, ui) {
                    $("#amountOne").val(ui.value + " km");
                }
            });
            $("#amountOne").val($("#slider-range-min").slider("value") + " km");
        });
    }	
    // SELECT 2 START
	var config = {
		'#wo-lang' : {allowClear: true, minimumResultsForSearch: Infinity},
		'#wo-usd'  : {allowClear: true, minimumResultsForSearch: Infinity},
		'#vehicle-condition1'  : {allowClear: true, minimumResultsForSearch: Infinity},
		'#vehicle-condition2'  : {allowClear: true, minimumResultsForSearch: Infinity},
        '#vehicle-condition3'  : {allowClear: true, minimumResultsForSearch: Infinity},
        '#vehicle-condition4'  : {allowClear: true, minimumResultsForSearch: Infinity},
        '#vehicle-condition5'  : {allowClear: true, minimumResultsForSearch: Infinity},
        '#vehicle-condition6'  : {allowClear: true, minimumResultsForSearch: Infinity},
		'#vehicle-condition7'  : {allowClear: true, minimumResultsForSearch: Infinity},
        '#vehicle-condition8'  : {allowClear: true, minimumResultsForSearch: Infinity},
        '#vehicle-condition9'  : {allowClear: true, minimumResultsForSearch: Infinity},
        '#vehicle-condition10'  : {allowClear: true, minimumResultsForSearch: Infinity},
        '#vehicle-condition11'  : {allowClear: true, minimumResultsForSearch: Infinity},
        '#vehicle-condition12'  : {allowClear: true, minimumResultsForSearch: Infinity},
        '#vehicle-condition13'  : {allowClear: true, minimumResultsForSearch: Infinity},
        '#vehicle-condition14'  : {allowClear: true, minimumResultsForSearch: Infinity},
	}
	for (var selector in config) {
		jQuery(selector).select2(config[selector]);
	}
    // Particles Start
    var particle1 = document.getElementById('particles-js')
    if (particle1 !== null) {
        particlesJS("particles-js", {
            "particles": {
                "number": {
                    "value": 10,
                    "density": {
                        "enable": true,
                        "value_area": 800
                    }
                },
                "color": {
                    "value": ["#fff", ]
                },
                "line_linked": {
                    "enable": false,
                },
                "size": {
                    "value": 101,
                    "random": true,
                },
                "opacity": {
                    "value": 0.3,
                },
            }
        })
    }
    var particle2 = document.getElementById('particlesfooter-js')
    if (particle2 !== null) {
        particlesJS("particlesfooter-js", {
            "particles": {
                "number": {
                    "value": 10,
                    "density": {
                        "enable": true,
                        "value_area": 800
                    }
                },
                "color": {
                    "value": ["#fff", ]
                },
                "line_linked": {
                    "enable": false,
                },
                "size": {
                    "value": 110,
                    "random": true,
                },
                "opacity": {
                    "value": 0.02,
                },
            }
        })
    }
    /* COUNTER */
    try {
        var _wo_aboutbanner__counter = jQuery('#wo-aboutbanner__counter');
        _wo_aboutbanner__counter.appear(function() {
            var _wo_aboutbanner__counter = jQuery('.wo-counternum h4');
            _wo_aboutbanner__counter.countTo({
                formatter: function(value, options) {
                    return value.toFixed(options.decimals).replace(/\B(?=(?:\d{3})+(?!\d))/g, ',');
                }
            });
        });
    } catch (err) {}
    /*OPEN CLOSE */
    jQuery('.wo-socialicon').on('click', function(event) {
        jQuery(this).find('.wo-socialmedialist').slideToggle();
    });
    // Select Socail Dropdown
    jQuery(".wo-socialmedialist a").on("click", function (e) {
        var socailcolor = jQuery(this).closest("li").attr("class");
        var socailicon = jQuery(this).find("i").attr("class");
        jQuery(this).closest(".wo-socialicon").removeClass().addClass(socailcolor+' wo-socialicon')
        jQuery(this).closest(".wo-socialicon").children('i').removeClass().addClass(socailicon);
  });
    // SHOW PASSWORD ICON 
    jQuery(document).on('click', '.wt-eyeicon a', function(e) {
        jQuery(this).toggleClass('wo-showpassword')
    });
    // SHOW NUMBER
    jQuery(document).on('click', '.wo-contectnum', function(e) {
        jQuery(this).addClass('wo-shownumwrap')
    });
    // SHOW FILTER
    jQuery(document).on('click', '.wo-searchfkm .wo-filtertitle, .wo-searchftype .wo-filtertitle, .wo-searchftype .wo-filter-section__content, .wo-searchfkm .wo-filter-section__content', function(e) {
        jQuery(this).siblings('.wo-searchf').slideToggle(300);
    });
    jQuery(document).mouseup(function(e){
		var container = jQuery(".wo-searchf");
		if(!container.is(e.target) && container.has(e.target).length === 0){
			container.hide();
		}
	});
    // SHOW FILTER
    jQuery('.wo-togglefilter').on('click', function() {
        jQuery(this).parent().siblings('.wo-filter-form').slideToggle();
    });
    let venobox = document.querySelector('.venobox')
    if (venobox !== null) {
        $('.venobox').venobox();
    }
    let wo_tippy = document.querySelector('.tippy')
    if (wo_tippy !== null) {
        tippy('.tippy', {
            content: "Add to compare",
            animation: 'scale',
        });
    }
    jQuery(document).on('click', '#nav-icon4', function(e) {
        $(this).toggleClass('open');
        $('.wo-dnavbar').toggleClass('opendnavbar');
    });
    // Range
	$( '.input-range').each(function(){
		var value = $(this).attr('data-slider-value');
		var separator = value.indexOf(',');
		if( separator !== -1 ){
			value = value.split(',');
			value.forEach(function(item, i, arr) {
				arr[ i ] = parseFloat( item );
			});
		} else {
			value = parseFloat( value );
		}
		$( this ).slider({
			formatter: function(value) {
				return '$' + value;
			},
			min: parseFloat( $( this ).attr('data-slider-min') ),
			max: parseFloat( $( this ).attr('data-slider-max') ), 
			range: $( this ).attr('data-slider-range'),
			value: value,
			tooltip_split: $( this ).attr('data-slider-tooltip_split'),
			tooltip: $( this ).attr('data-slider-tooltip')
		});
    });
    jQuery('.wo-message__userlist li').on('click', function(){
        jQuery(this).closest('.wo-message__holder').addClass('wo-open-message')
    })
    jQuery('.wo-messageback').on('click', function(){
        jQuery(this).closest('.wo-message__holder').removeClass('wo-open-message')
    })
    jQuery('.wo-delete').on('click', function(){
        jQuery(this).closest('li').remove()
    })
    // hover zoom on spare parts
    var zoom = document.querySelector('.slider-main img');
    if (zoom !== null) {
        $('.slider-main img').okzoom({
            width: 200,
            height: 200,
            border: "1px solid #eee",
            shadow: "0 0 5px #ddd"
        });
    }
    // Clear Fillter
    let wo_clearfilter = document.getElementById('wo-clearfilter')
    if(wo_clearfilter !== null){
        jQuery(document).on("click","#wo-clearfilter",function(event){
            var uncheckfilter = jQuery('.wo-resultoptions wo-checkbox input[type=checkbox]') 
            if(jQuery(uncheckfilter).attr('checked')) {
                jQuery('input:checkbox').attr('checked',true);
            }
            else {
                jQuery('input:checkbox').attr('checked',false);
            }
        });
    }
     /*Responsive Table */
     var table = document.querySelector('.table')
    if (table !== null) {
		try {
            $('.table').responsiveTables();
		} catch (e) {

		}
     }
         /* Brand Slider */
    //var _wo_brands__slider = jQuery(".wo-brands__slider")
    //_wo_brands__slider.owlCarousel({
    //    autoplay: false,
    //    dots: false,
    //    items: 9,
    //    margin: 10,
    //    loop: true,
    //    autoWidth: true,
    //    responsive: {
    //        0: {
    //            items: 1,
    //        },
    //        767: {
    //            items: 2,
    //        },
    //        1199: {
    //            items: 3,
    //        },
    //        1440: {
    //            items: 8,
    //        },
    //        1750: {
    //            items: 9
    //        }
    //    }
    //});

    /* Banner Slider */
    //var _wo_vsingleslider = jQuery("#wo-vsingleslider")
    //_wo_vsingleslider.owlCarousel({
    //     items: 1,
    //     loop: true,
    //     nav: true,
    //     autoplay: false,
    //     dots: false,
    //     smartSpeed: 500,
    //     responsiveClass: true,
    //     navClass: ['wo-prev', 'wo-next'],
    //     navContainerClass: 'wo-bannernav wo-vehiclesnav',
    //     navText: ['<span class="ti-angle-left"></span>', '<span class="ti-angle-right"></span>'],
    //});
   /* Vehicles Images Slider */
   var _wo_vehiclesimgslider = jQuery('.wo-vehiclesimgslider')
   _wo_vehiclesimgslider.owlCarousel({
       items: 1,
       loop: true,
       dots: false,
       nav: true,
       autoHeight: true,
       margin: 0,
       autoplay: false,
       navClass: ['wo-prev', 'wo-next'],
       navContainerClass: 'wo-bannernav wo-vehiclesnav',
       navText: ['<span class="ti-angle-left"></span>', '<span class="ti-angle-right"></span>'],
   });
	
	setTimeout(function(){ jQuery('.wo-vehiclesimgslider').trigger('refresh.owl.carousel'); }, 2000);
	setTimeout(function(){ jQuery('.wo-vehiclesimgslider').trigger('refresh.owl.carousel'); }, 3000);
	setTimeout(function(){ jQuery('.wo-vehiclesimgslider').trigger('refresh.owl.carousel'); }, 3500);
	setTimeout(function(){ jQuery('.wo-vehiclesimgslider').trigger('refresh.owl.carousel'); }, 4500);
   /* vehicles Slider */
   //var _wo_testimonials_slider = jQuery(".wo-testimonials-slider")
   //_wo_testimonials_slider.owlCarousel({
   //    loop: false,
   //    autoplay: false,
   //    items: 1,
   //    margin: 0,
   //    dots: true,
   //    autoplayTimeout: 5500,
   //    autoplaySpeed: 2000,
   //});
    /* Banner Slider */

    //var _wo_banner__silder = jQuery("#wo-banner__silder")
    //_wo_banner__silder.owlCarousel({
    //     items: 1,
    //     loop: true,
    //     nav: true,
    //     autoplay: false,
    //     dots: false,
    //     smartSpeed: 500,
    //     responsiveClass: true,
    //     navClass: ['wo-prev', 'wo-next'],
    //     navContainerClass: 'wo-bannernav',
    //     navText: ['<span class="ti-angle-left"></span>', '<span class="ti-angle-right"></span>'],
    //     responsive: {
    //         0: {
    //             dots: true,
    //             nav: false,
    //         },
    //         481: {
    //             dots: false,
    //             nav: true,
    //         },
    //     }
    // });

    // Select mCustomscrollbar
    $('select').on('select2:open', function (e) {
        $('.select2-results__options').mCustomScrollbar('destroy');
        setTimeout(function () {
            $('.select2-results__options').mCustomScrollbar();
      }, 0);
  });
});
//var locationmap = document.querySelector("#wo-locationmap")
//    if (locationmap !== null) {
//        var center = [37.772323, -122.214897];
//        $('#wo-locationmap')
//            .gmap3({
//                center: center,
//                zoom: 13,
//                mapTypeId: google.maps.MapTypeId.ROADMAP
//            })
//            .marker({
//                position: center,
//                icon: '/Assets/images/icons/marker-red.png'
//                //icon: 'https://maps.google.com/mapfiles/marker_green.png'
//            });
//    }

(function($) {

    $.fn.bekeyProgressbar = function(options) {

        options = $.extend({
            animate: true,
            animateText: true
        }, options);

        var $this = $(this);

        var $progressBar = $this;
        var $progressCount = $progressBar.find('.ProgressBar-percentage--count');
        var $circle = $progressBar.find('.ProgressBar-circle');
        var percentageProgress = $progressBar.attr('data-progress');
        var percentageRemaining = (100 - percentageProgress);
        var percentageText = $progressCount.parent().attr('data-progress');
        var radius = $circle.attr('r');
        var diameter = radius * 2;
        var circumference = Math.round(Math.PI * diameter);
        var percentage = circumference * percentageRemaining / 100;

        $circle.css({
            'stroke-dasharray': circumference,
            'stroke-dashoffset': percentage
        })

        if (options.animate === true) {
            $circle.css({
                'stroke-dashoffset': circumference
            }).animate({
                'stroke-dashoffset': percentage
            }, 3000)
        }

        if (options.animateText == true) {

            $({
                Counter: 0
            }).animate({
                Counter: percentageText
            }, {
                duration: 3000,
                step: function() {
                    $progressCount.text(Math.ceil(this.Counter) + '%');
                }
            });

        } else {
            $progressCount.text(percentageText + '%');
        }

    };

})(jQuery);
    // Readmore Start
    var additionalservices = document.querySelector(".wo-commentsspare");
    if (additionalservices !== null) {
        $(".wo-commentsspare").readmore({
            collapsedHeight: 312,
            moreLink: '<a href="#">Load more comments</a>',
            lessLink: '<a href="#">Show Less</a>',
        });
    }
    var wo_childrencomment = document.querySelector(".wo-childrencomment");
    if (wo_childrencomment !== null) {
        $(".wo-childrencomment").readmore({
            collapsedHeight: 166,
            moreLink: '<a href="#">Load more comments</a>',
            lessLink: '<a href="#">Show Less</a>',
        });
    }
    var  wo_widgetcheckbox = document.querySelector(".wo-widgetcheckbox");
    if ( wo_widgetcheckbox !== null) {
       wo_widgetcheckbox = {
        collapsedHeight: 142,
        blockCSS: 'display: flex',
        moreLink: '<a href="#">Show All</a>',
        lessLink: '<a href="#">Show Less</a>',
      };
      $('.wo-widgetcheckbox').readmore(wo_widgetcheckbox);
    }
    var  wo_collapsetitle = document.querySelector(".wo-collapse-title");
    if ( wo_collapsetitle !== null) {
        jQuery('.wo-collapse-title').on('click',function(){
            var $this = $(this);
            setTimeout(function(){
              jQuery($this).next().find('.wo-widgetcheckbox').readmore(wo_widgetcheckbox);
            }, 100);
        })
    }
    /* Part Stock */
    var woInputIncrement = document.querySelector('.wo-input-increment')
    if (woInputIncrement !== null) {
        jQuery(document).on('click', '.wo-input-increment', function(e) {
            var $input = $(this).closest('.wo-vlaue-btn').find('.wo-input-number');
            var val = parseInt($input.val(), 10);
            $input.val(val + 1);
        })
        jQuery(document).on('click', '.wo-input-decrement', function(e) {
            var $input = $(this).closest('.wo-vlaue-btn').find('.wo-input-number');
            var val = parseInt($input.val(), 10);
            if (val >= 1) {
                $input.val(val - 1);
            }
        })
    }

    /* Input text show/hide */
    function passwordFunction(id) {
        var x = document.querySelector('#' + id);
        if (x.type === "password") {
            x.type = "text";
        } else {
            x.type = "password";
        }
    }
         
    $('.ProgressBar--animateAll').bekeyProgressbar();
    let __slider_main = document.querySelector(".slider-main");
    if (__slider_main !== null) {
        //spare-part-single slider
        $(".slider-main").slick({
        slidesToShow: 1,
        arrows: false,
        asNavFor: ".slider-nav",
        vertical: true,
        autoplay: false,
        verticalSwiping: true,
        centerMode: false,
        speed: 300,
        responsive: [
            {
                breakpoint: 481,
                settings: {
                    verticalSwiping: false,
                    vertical: true, 
                    touchMove:true,
                    verticalSwiping: false,

                }
            },
        ]
        });
    }
    let __slider_nav = document.querySelector(".slider-nav");
    if (__slider_nav !== null) {
        $(".slider-nav").slick({
        initialSlide: 1,
        infinite: true,
        slidesToShow: 6,
        asNavFor: ".slider-main",
        focusOnSelect: false,
        autoplay: false,
        centerMode: false,
        verticalSwiping: true,
        vertical: true,
        focusOnSelect: true,
        autoplay: false,
        centerMode: false,
        responsive: [
            {
                breakpoint: 481,
                settings: {
                    infinite: false,
                    slidesToShow: 4,
                    slidesToScroll: 2,
                    vertical: false,
                    variableWidth: true,
                    verticalSwiping: false,
                }
            },
        ]
        });
    }
/* tO remove active classs in profile setting pages*/
jQuery('.wo-dbpost .wo-dbpoststep').on('click', function (e) { 
    if (jQuery('.wo-dbpoststep__title a , .wo-dbpost.active a ').hasClass('active')){
        jQuery('.wo-dbpost__content  [data-toggle="tab"]').removeClass('active');
        jQuery('.wo-prevstep , .wo-savenext a').removeClass('active');
    }
});
