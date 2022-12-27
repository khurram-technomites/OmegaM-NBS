using NowBuySell.Service;
using NowBuySell.Web.Helpers.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;
using NowBuySell.Data;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class NewsFeedController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly INewsFeedService _newsFeedService;

        public NewsFeedController(INewsFeedService newsFeedService)
        {
            this._newsFeedService = newsFeedService;
        }
        //int pgno = 1, 

        [HttpGet]
        [Route("{lang}/newsfeed")]
        public HttpResponseMessage GetNewsFeed(int pgno = 1, int pagesize = 20, string lang = "en", string module = null)
        {
            try
            {
                string ImageServer = CustomURL.GetImageServer();
                if (module != null)
                {
                    if (module.ToLower() == "motor")
                    {

                        var newsfeedcar = _newsFeedService.GetNewsFeed().Where(x => x.Module.ToLower() == module).Select(i => new
                        {
                            slug = i.Slug,
                            id = i.ID,
                            title = lang == "en" ? i.Title : i.TitleAr,
                            description = lang == "en" ? i.TitleDescription : i.TitleDescriptionAr,
                            author = lang == "en" ? i.Author : i.AuthorAr,
                            badge = lang == "en" ? i.Badge : i.BadgeAr,
                            bannerImage = ImageServer + i.BannerImage,
                            createdOn = i.CreatedOn,
                            video = string.IsNullOrEmpty(i.Video) ? null : ImageServer + i.Video,
                            module = i.Module,
                            tag = "Luxury Car"
                        }).ToList().Skip(pagesize * (pgno - 1)).Take(pagesize);
                        return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", newsfeeds = newsfeedcar });


                    }
                    if (module.ToLower() == "property")
                    {

                        var newsfeedproperty = _newsFeedService.GetNewsFeed().Where(x => x.Module.ToLower() == module).Select(i => new
                        {
                            slug = i.Slug,
                            id = i.ID,
                            title = lang == "en" ? i.Title : i.TitleAr,
                            description = lang == "en" ? i.TitleDescription : i.TitleDescriptionAr,
                            author = lang == "en" ? i.Author : i.AuthorAr,
                            badge = lang == "en" ? i.Badge : i.BadgeAr,
                            bannerImage = ImageServer + i.BannerImage,
                            createdOn = i.CreatedOn,
                            video = string.IsNullOrEmpty(i.Video) ? null : ImageServer + i.Video,
                            module = i.Module,
                            tag = "Family Home"
                        }).ToList().Skip(pagesize * (pgno - 1)).Take(pagesize);
                        return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", newsfeeds = newsfeedproperty });
                    }
                }
                var newsfeeds = _newsFeedService.GetNewsFeed().Select(i => new
                {
                    slug = i.Slug,
                    id = i.ID,
                    title = lang == "en" ? i.Title : i.TitleAr,
                    description = lang == "en" ? i.TitleDescription : i.TitleDescriptionAr,
                    author = lang == "en" ? i.Author : i.AuthorAr,
                    badge = lang == "en" ? i.Badge : i.BadgeAr,
                    bannerImage = ImageServer + i.BannerImage,
                    createdOn = i.CreatedOn,
                    video = string.IsNullOrEmpty(i.Video) ? null : ImageServer + i.Video,
                    module = i.Module,
                    tag = "Family Home"
                }).ToList().Skip(pagesize * (pgno - 1)).Take(pagesize);
                return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", newsfeeds = newsfeeds });

            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

        [HttpGet]
        [Route("{lang}/newsfeed/{slug}")]
        public HttpResponseMessage GetNewsFeedDetails(string slug, string lang = "en")
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                NewsFeed feed = _newsFeedService.GetNewsFeedBySlug(slug);
                if (feed != null)
                {
                    string ImageServer = CustomURL.GetImageServer();

                    var newsfeedModel = new
                    {
                        feed.ID,
                        title = lang == "en" ? feed.Title : feed.TitleAr,
                        description = lang == "en" ? feed.TitleDescription : feed.TitleDescriptionAr,
                        author = lang == "en" ? feed.Author : feed.AuthorAr,
                        badge = lang == "en" ? feed.Badge : feed.BadgeAr,
                        feed.CreatedOn,
                        BannerImage = ImageServer + feed.BannerImage,
                        Video = ImageServer + feed.Video,
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        newsfeed = newsfeedModel
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { status = "error", message = "Invalid id !" });
                }
            }


            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

        [HttpGet]
        [Route("{lang}/newsfeedByID/{ID}")]
        public HttpResponseMessage GetNewsFeedDetailsByID(long ID, string lang = "en")
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                NewsFeed feed = _newsFeedService.GetNewsFeedByID(ID);
                if (feed != null)
                {
                    string ImageServer = CustomURL.GetImageServer();

                    var newsfeedModel = new
                    {
                        feed.ID,
                        title = lang == "en" ? feed.Title : feed.TitleAr,
                        description = lang == "en" ? feed.TitleDescription : feed.TitleDescriptionAr,
                        author = lang == "en" ? feed.Author : feed.AuthorAr,
                        badge = lang == "en" ? feed.Badge : feed.BadgeAr,
                        feed.CreatedOn,
                        BannerImage = ImageServer + feed.BannerImage,
                        Video = ImageServer + feed.Video,
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        newsfeed = newsfeedModel
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { status = "error", message = "Invalid id !" });
                }
            }


            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

    }
}
