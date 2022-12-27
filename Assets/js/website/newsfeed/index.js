var pg = 1;
var PageSize = 24;
var isPageRendered = false;
var totalPages;
/*var lang = "en";*/

var filter = {
    search: null,
    pageNumber: 1,
    sortBy: 1
}

$(document).ready(function () {

    $('#ArticleContainers').empty();

    GetFilterValues();
    GetArticles();

    $('#ArticleSearch').change(function () {
        if ($('#ArticleSearch').val()) {
            $('#ArticleContainers').empty();
            pg = 1;
            GetFilterValues();
            GetArticles();
        }
    });

    $('#btnSearch').click(function () {
        $('#ArticleContainers').empty();
        pg = 1;
        GetFilterValues();
        GetArticles();
    });

    $('#SortBy').change(function () {
        $('#ArticleContainers').empty();
        pg = 1;
        GetFilterValues();
        GetArticles();
    });

    $('#load-more').click(function () {
        //if (pg < totalPages) {
        pg++;
        $('#load-more').hide();
        $(".filter-loader").show();
        GetFilterValues();
        GetArticles();
        //}
    });
    
});

function GetFilterValues() {
    filter.search = $("#ArticleSearch").val();
    filter.pageNumber = pg;
    filter.sortBy = $("#SortBy").val();
}

function GetArticles() {
    $.ajax({
        type: 'POST',
        url: '/' + culture + '/news',
        contentType: "application/json",
        data: JSON.stringify(filter),
        success: function (response) {
            BindGridArticles(response.data);
        }
    });
}

function BindGridArticles(data) {
    $.each(data, function (k, v) {
        //console.log(v);
        //var num = v.CreatedOn.match(/\d+/g); //regex to extract numbers
        //var date = new Date(parseFloat(num)); //converting to date
        //var title = lang == "en" ? v.Title : v.TitleAr;
        //var txt = lang == "en" ? v.TitleDescription : v.TitleDescriptionAr;
        var date = v.Date;
        var title = v.Title;
        var txt = v.Description;
        if (txt.length > 200) {
            txt = txt.substring(0, 200) + ' ...';
        }
        $('#ArticleContainers').append(`
			
			<div class="col-12 col-md-6 col-lg-4 my-3 ${lang == "en" ? "" : "text-right"}">
				<div class="wo-articles">
					<div class="wo-articles__head">
						<img src="/Assets/images/articles/info-circle.png" alt="img description">
						<div class="wo-articles-title">
							<h3 class="newsfeed-title" title="${title}">
								<a href="/${culture}/news/${v.Slug}">
									${title}
								</a>
							</h3>
						</div>
					</div>
					<div class="wo-description">
						<p class="text-justify">
							 ${txt}
						</p>
						<a class="text-capitalize ${lang == "en" ? "" : "float-right"}" href="/${culture}/news/${v.Slug}">
							${lang == "en" ? "Read More" : "قراءة المزيد"}
						</a>
					</div>
					<figure class="wo-articles__img">
						<img src="${v.Cover}" alt=" Ooops! image not found." title="${title}">
					</figure>
					<ul class="wo-articlesmeta ${lang == "en" ? "" : "justify-content-end"}">
						<li><span><i class="ti-calendar"></i>${date}</span></li>
					</ul>
				</div>
			</div>

		`);

    });

    //$.each(data, function (k, v) {
    //    var template = '<div class="col-12 col-sm-12 col-md-4 col-lg-4 article">';
    //    template += '			<!-- Article Image -->';
    //    template += '			<a class="article_featured-image" href="/' + lang + '/news/' + v.Slug + '">';
    //    template += '				<img class="blur-up lazyload img-lazyload" data-src="' + v.Cover + '" src="' + v.Cover + '" alt="' + v.Title + '">';
    //    template += '			</a>';
    //    template += '			<h2 class="h3">';
    //    template += '				<a href="/' + lang + '/news/' + v.Slug + '">' + v.Title + '</a>';
    //    template += '			</h2>';
    //    template += '			<ul class="publish-detail">';

    //    template += '				<li>';
    //    template += '					<i class="icon anm anm-clock-r"></i> <time datetime="2017-05-02">' + v.Date + '</time>';
    //    template += '				</li>';
    //    template += '			</ul>';
    //    template += '			<div class="rte">';
    //    template += '				<p>' + v.Description + '...</p>';
    //    template += '			</div>';
    //    template += '			<p>';
    //    template += '				<a href="/' + lang + '/news/' + v.Slug + '" class="btn btn-secondary btn--small" title="Read more">';
    //    template += '					Read more';
    //    template += '					<i class="fa fa-caret-right" aria-hidden="true"></i>';
    //    template += '				</a>';
    //    template += '			</p>';
    //    template += '		</div>';

    //    $('#ArticleContainers').append(template);
    //});

    //setTimeout(function () { OnErrorImage(); }, 3000);
    if (data && data.length >= PageSize) {
        $("#load-more").fadeIn();
    } else {
        $("#load-more").fadeOut();
    }

    if ($('#ArticleContainers').html().length == 0) {
        $("html, body").animate({ scrollTop: 0 }, 1000);
        $('#ArticleContainers').html('<div class="alert alert-dark">No Records Found!</div>');
    }
}

function UpdateQueryString(key, value, url) {
    if (!url) url = window.location.href;
    var re = new RegExp("([?&])" + key + "=.*?(&|#|$)(.*)", "gi"),
        hash;

    if (re.test(url)) {
        if (typeof value !== 'undefined' && value !== null) {
            return url.replace(re, '$1' + key + "=" + value + '$2$3');
        }
        else {
            hash = url.split('#');
            url = hash[0].replace(re, '$1$3').replace(/(&|\?)$/, '');
            if (typeof hash[1] !== 'undefined' && hash[1] !== null) {
                url += '#' + hash[1];
            }
            return url;
        }
    }
    else {
        if (typeof value !== 'undefined' && value !== null) {
            var separator = url.indexOf('?') !== -1 ? '&' : '?';
            hash = url.split('#');
            url = hash[0] + separator + key + '=' + value;
            if (typeof hash[1] !== 'undefined' && hash[1] !== null) {
                url += '#' + hash[1];
            }
            return url;
        }
        else {
            return url;
        }
    }
}

function GetURLParameter() {
    var sPageURL = window.location.href;
    var indexOfLastSlash = sPageURL.lastIndexOf("/");

    if (indexOfLastSlash > 0 && sPageURL.length - 1 != indexOfLastSlash)
        return sPageURL.substring(indexOfLastSlash + 1).replace('#', '');
    else
        return 0;
}

//$("#language-selector").change(function () {
//    $("select option:selected").each(function () {
//        lang = $(this).val();
//    });
//    $('#ArticleContainers').empty();
//    GetArticles();
//});