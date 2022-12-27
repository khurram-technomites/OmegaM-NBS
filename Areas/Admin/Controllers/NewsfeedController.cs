using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class NewsfeedController : Controller
    {
        private readonly INewsFeedService _newsFeedService;
        public NewsfeedController(INewsFeedService newsFeedService)
        {
            this._newsFeedService = newsFeedService;
        }

        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            var NewsFeed = _newsFeedService.GetNewsFeed();
            return PartialView(NewsFeed);
        }

        public ActionResult Create()
        {
            NewsFeed newsFeed = new NewsFeed();
            return View(newsFeed);
        }

        [HttpPost]
        public ActionResult Create(NewsFeed data, string Title)
        {
            string message = string.Empty;

            if (ModelState.IsValid)
            {

                NewsFeed record = new NewsFeed();
                record.Title = data.Title;
                record.TitleAr = data.TitleAr;
                record.Slug = Slugify.GenerateSlug(data.Title);
                record.TitleDescription = data.TitleDescription;
                record.TitleDescriptionAr = data.TitleDescriptionAr;
                record.Module = data.Module;
                record.Author = data.Author;
                record.AuthorAr = data.AuthorAr;
                record.BadgeAr = data.BadgeAr;

                string replacelement = Title.Replace("?", "");

                if (data.BannerImage != null)
                {
                    string absolutePath = Server.MapPath("~");
                    string relativePath = string.Format("/Assets/AppFiles/NewsFeed/BannerImages/{0}/", replacelement.Replace(" ", "_"));
                    record.BannerImage = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "BannerImage", ref message, "BannerImage");
                }
                if (data.Video != null)
                {
                    string absolutePath = Server.MapPath("~");
                    string relativePath = string.Format("/Assets/AppFiles/NewsFeed/Video/{0}/", replacelement.Replace(" ", "_"));
                    record.Video = Uploader.UploadVideo(Request.Files, absolutePath, relativePath, "Video", ref message, "Video");
                }
                if (_newsFeedService.CreateNewsFeed(record, ref message))
                {
                    TempData["SuccessMessage"] = message;
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/NewsFeed/Index",
                        message = message
                    });
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }

            TempData["ErrorMessage"] = message;
            return Json(new
            {
                success = false,
                url = "/Admin/NewsFeed/Index",
                message = message
            });
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NewsFeed feed = _newsFeedService.GetNewsFeedByID((long)id);
            if (feed == null)
            {
                return HttpNotFound();
            }


            return View(feed);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Edit(NewsFeed data, string Title)
        {
            string message = string.Empty;

            NewsFeed updatefeed = _newsFeedService.GetNewsFeedByID(data.ID);
            string imagepath = Server.MapPath("~") + updatefeed.BannerImage;
            string videopath = Server.MapPath("~") + updatefeed.Video;
            string replacelement = Title.Replace("?", "");

            if (ModelState.IsValid)
            {
                updatefeed.Title = data.Title;
                updatefeed.TitleAr = data.TitleAr;
                updatefeed.TitleDescription = data.TitleDescription;
                updatefeed.TitleDescriptionAr = data.TitleDescriptionAr;
                updatefeed.Slug = Slugify.GenerateSlug(data.Title);
                updatefeed.Module = data.Module;
                updatefeed.Author = data.Author;
                updatefeed.AuthorAr = data.AuthorAr;
                updatefeed.Badge = data.Badge;
                updatefeed.BadgeAr = data.BadgeAr;

               if(data.BannerImage != null )
                {
                    string absolutePath = Server.MapPath("~");
                    string relativePath = string.Format("/Assets/AppFiles/NewsFeed/BannerImages/{0}/", replacelement.Replace(" ", "_"));
                    updatefeed.BannerImage = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "BannerImage", ref message, "BannerImage");
                }
                    
                
                if (data.Video != null)
                {
                    string absolutePath = Server.MapPath("~");
                    string relativePath = string.Format("/Assets/AppFiles/NewsFeed/Video/{0}/", replacelement.Replace(" ", "_"));
                    updatefeed.Video = Uploader.UploadVideo(Request.Files, absolutePath, relativePath, "Video", ref message, "Video");
                    
                }
                if (_newsFeedService.UpdateNewsFeed(ref updatefeed, ref message))
                {
                    if (data.BannerImage != null)
                    {
                        string absolutePath = Server.MapPath("~");
                        if (System.IO.File.Exists(imagepath))
                        {
                            System.IO.File.Delete(imagepath);
                        }
                    }
                    if (data.Video != null)
                    {
                        string absolutePath = Server.MapPath("~");
                        if (System.IO.File.Exists(videopath))
                        {
                            System.IO.File.Delete(videopath);
                        }
                    }
                    // System.IO.File.Delete(your file path)
                    TempData["SuccessMessage"] = message;
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/NewsFeed/Index",
                        message = message
                    });
                  
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }


            TempData["ErrorMessage"] = message;
            return Json(new
            {
                success = false,
                url = "/Admin/NewsFeed/Index",
                message = message

            });
            
        }

        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var Feed = _newsFeedService.GetNewsFeedByID((long)id);
            if (Feed == null)
            {
                return HttpNotFound();
            }

            if (!(bool)Feed.IsActive)
                Feed.IsActive = true;
            else
            {
                Feed.IsActive = false;
            }
            string message = string.Empty;
            if (_newsFeedService.UpdateNewsFeed(ref Feed, ref message))
            {
                SuccessMessage = "NewsFeed " + ((bool)Feed.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        Date = Feed.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                        Title = Feed.Title,
                        BannerImage = Feed.BannerImage,
                        Module = Feed.Module,
                        IsActive = Feed.IsActive.HasValue ? Feed.IsActive.Value.ToString() : bool.FalseString,
                        ID = Feed.ID
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NewsFeed Feed = _newsFeedService.GetNewsFeedByID((Int16)id);

            if (Feed == null)
            {
                return HttpNotFound();
            }
            return View(Feed);
        }

        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NewsFeed Newsfeed = _newsFeedService.GetNewsFeedByID((Int16)id);
            if (Newsfeed == null)
            {
                return HttpNotFound();
            }
            TempData["ID"] = id;
            return View(Newsfeed);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            NewsFeed Newsfeed = _newsFeedService.GetNewsFeedByID((Int16)id);
            string message = string.Empty;
            if (_newsFeedService.DeleteNewsFeed((Int16)id, ref message))
            {
                if (Newsfeed.BannerImage != null)
                {
                    string absolutePath = Server.MapPath("~");
                    if (System.IO.File.Exists(absolutePath + Newsfeed.BannerImage))
                    {
                        System.IO.File.Delete(absolutePath + Newsfeed.BannerImage);
                    }
                }
                if (Newsfeed.Video != null)
                {
                    string absolutePath = Server.MapPath("~");
                    if (System.IO.File.Exists(absolutePath + Newsfeed.Video))
                    {
                        System.IO.File.Delete(absolutePath + Newsfeed.Video);
                    }
                }
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}