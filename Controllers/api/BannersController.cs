using NowBuySell.Service;
using NowBuySell.Web.Helpers.Routing;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class BannersController : ApiController
    {
        private readonly IBannerImagesService _bannnerImageService;
        private readonly IPromoBannerService _promoBannerService;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public BannersController(IBannerImagesService bannnerImageService, IPromoBannerService promoBannerService)
        {
            this._bannnerImageService = bannnerImageService;
            _promoBannerService = promoBannerService;
        }

        //[HttpGet]
        //[Route("{lang}/{module}/banners")]
        //public HttpResponseMessage GetBanners(string module, string lang = "en")
        //{
        //    try
        //    {
        //        string ImageServer = CustomURL.GetImageServer();
        //        var bannersMobile = _bannnerImageService.GetBannersByTypeLangAndModule("Mobile", module, lang).Select(i => new
        //        {
        //            bannerPath = ImageServer + i.ImagePath,
        //            redirectionUrl = i.Url,
        //            type = i.BannerType
        //        }).ToList();

        //        var bannersWebsite = _bannnerImageService.GetBannersByTypeLangAndModule("Website", module, lang).Select(i => new
        //        {
        //            bannerPath = ImageServer + i.ImagePath,
        //            redirectionUrl = i.Url,
        //            type = i.BannerType
        //        }).ToList();

        //        var PromobannersMobile = _promoBannerService.GetBannersByTypeLangAndModule("Mobile", module, lang).Select(i => new
        //        {
        //            bannerPath = ImageServer + i.ImagePath,
        //            redirectionUrl = i.Url,
        //            type = i.BannerType
        //        }).ToList();

        //        var PromobannersWebsite = _promoBannerService.GetBannersByTypeLangAndModule("Website", module, lang).Select(i => new
        //        {
        //            bannerPath = ImageServer + i.ImagePath,
        //            redirectionUrl = i.Url,
        //            type = i.BannerType
        //        }).ToList();

        //        return Request.CreateResponse(HttpStatusCode.OK, new 
        //        { 
        //            status = "success", 
        //            websiteBanners = bannersWebsite,
        //            mobileBanners = bannersMobile,
        //            promoWebsiteBanners = PromobannersWebsite,
        //            promoMobileBanners = PromobannersMobile
        //        });

        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("Error", ex);
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, new
        //        {
        //            status = "failure",
        //            message = "Oops! Something went wrong. Please try later."
        //        });
        //    }
        //}

        [HttpGet]
        [Route("{lang}/{type}/banners")]
        public HttpResponseMessage GetBanners(string type, string lang = "en")
        {
            try
            {
                string ImageServer = CustomURL.GetImageServer();
                var OnTopBanners = _bannnerImageService.GetBannersByTypeAndLang(type, lang).ToList();
                var Promobanners = _promoBannerService.GetBannersByTypeAndLang(type, lang).ToList();
                if (type != "Mobile")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                       
                        motor = new
                        {
                            topBanners = OnTopBanners.Where(x => x.Module == "Motor").Select(i => new
                            {
                                bannerPath = ImageServer + i.ImagePath,
                                redirectionUrl = i.Url,
                                type = i.BannerType,
                                description = lang == "en" ? i.Description : i.DescriptionAr
                            }).ToList(),

                            promoBanners = Promobanners.Where(x => x.Module == "Motor").Select(i => new
                            {
                                bannerPath = ImageServer + i.ImagePath,
                                redirectionUrl = i.Url,
                                type = i.BannerType,
                                description = lang == "en" ? i.Description : i.DescriptionAr
                            }).ToList()
                        },
                        property = new
                        {
                            topBanners = OnTopBanners.Where(x => x.Module == "Property").Select(i => new
                            {
                                bannerPath = ImageServer + i.ImagePath,
                                redirectionUrl = i.Url,
                                type = i.BannerType,
                                description = lang == "en" ? i.Description : i.DescriptionAr
                            }).ToList(),

                            promoBanners = Promobanners.Where(x => x.Module == "Property").Select(i => new
                            {
                                bannerPath = ImageServer + i.ImagePath,
                                redirectionUrl = i.Url,
                                type = i.BannerType,
                                description = lang == "en" ? i.Description : i.DescriptionAr
                            }).ToList()
                        }
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        banners = OnTopBanners.Select(i => new
                        {
                            bannerPath = ImageServer + i.ImagePath,
                            redirectionUrl = i.Url,
                            type = i.BannerType,
                            description = lang == "en" ? i.Description : i.DescriptionAr
                        }).ToList(),
                        motor = Promobanners.Where(x => x.Module == "Motor").Select(i => new
                        {
                            bannerPath = ImageServer + i.ImagePath,
                            redirectionUrl = i.Url,
                            type = i.BannerType,
                            description = lang == "en" ? i.Description : i.DescriptionAr
                        }).FirstOrDefault(),
                        property = Promobanners.Where(x => x.Module == "Property").Select(i => new
                        {
                            bannerPath = ImageServer + i.ImagePath,
                            redirectionUrl = i.Url,
                            type = i.BannerType,
                            description = lang == "en" ? i.Description : i.DescriptionAr
                        }).FirstOrDefault()
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpGet]
        [Route("{lang}/discountbanner")]
        public HttpResponseMessage GetDiscountBanners(string lang)
        {
            try
            {
                string ImageServer = CustomURL.GetImageServer();
                var banners = _bannnerImageService.GetBannersByTypeAndLang("Promotion", lang).Select(i => new
                {
                    url = ImageServer + i.ImagePath,
                }).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", banners = banners });

            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }
        [HttpGet]
        [Route("mediacontent")]
        public HttpResponseMessage GetWebsiteHeader(string lang="en")
        {
            try
            {
                string ImageServer = CustomURL.GetImageServer();
                var header = _bannnerImageService.GetBannersByType("WebsiteHeader").Select(i => new
                {
                    url = ImageServer + i.ImagePath,
                    ContentType = i.ContentType
                }).ToList();
                var motorsidebanner = _bannnerImageService.GetBannersByTypeAndLang("MotorSideBanner", lang).Select(i => new
                {
                    url = ImageServer + i.ImagePath,
                    ContentType = i.ContentType
                }).ToList();
                var propertysidebanner = _bannnerImageService.GetBannersByTypeAndLang("PropertySideBanner", lang).Select(i => new
                {
                    url = ImageServer + i.ImagePath,
                    ContentType = i.ContentType
                }).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", header = header, motorsidebanner= motorsidebanner, propertysidebanner= propertysidebanner });

            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

        [HttpGet]
        [Route("motorsidebanner")]
        public HttpResponseMessage GetMotorSideBanner()
        {
            try
            {
                string ImageServer = CustomURL.GetImageServer();
                var header = _bannnerImageService.GetBannersByType("MotorSideBanner").Select(i => new
                {
                    url = ImageServer + i.ImagePath,
                    ContentType = i.ContentType
                }).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", header = header });

            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }
        [HttpGet]
        [Route("propertysidebanner")]
        public HttpResponseMessage GetPropertyBanner()
        {
            try
            {
                string ImageServer = CustomURL.GetImageServer();
                var header = _bannnerImageService.GetBannersByType("PropertySideBanner").Select(i => new
                {
                    url = ImageServer + i.ImagePath,
                    ContentType = i.ContentType
                }).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", header = header });

            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }
    }
}
