
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Areas.Admin.ViewModels.Banners;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class ContentManagmentController : Controller
    {
        private readonly IBannerImagesService _bannnerImageService;
		private readonly IPromoBannerService _promobannnerImageService;

		public ContentManagmentController(IBannerImagesService bannnerImageService, IPromoBannerService promobannnerImageService)
        {
            this._bannnerImageService = bannnerImageService;
            _promobannnerImageService = promobannnerImageService;
        }
        public ActionResult Manage()
        {
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			ViewBag.ErrorMessage = TempData["ErrorMessage"];
			BannersViewModel objBannersViewModel = new BannersViewModel()
            {
                WebsiteBanners = _bannnerImageService.GetBanners().ToList(),
				PromoBannerWeb = _promobannnerImageService.GetBanners().ToList(),
			};

            return View(objBannersViewModel);
        }
        [HttpPost]
		public ActionResult Manage(HttpPostedFileBase file, string Url, string Lang, string ContentType, string MVType, string Description, string DescriptionAr)
		{
			var banner = _bannnerImageService.GetBannersByType("WebsiteHeader").ToList();
            if (banner.Count == 0)
            {
				string message = string.Empty;
				if (file != null && file.ContentLength > 0)

					try
					{
						if (ContentType == "Image")
						{

							string path = Path.Combine(Server.MapPath("~/Assets/AppFiles/Banners/"));
							string newfilename = "web-banner-header" + Guid.NewGuid().ToString() + ".jpg";
							// file.FileName = newfilename;

							Path.GetFileName(file.FileName);
							file.SaveAs(path + newfilename);
							BannerImage objBannerImage = new BannerImage();
							objBannerImage.ImagePath = "/Assets/AppFiles/Banners/" + newfilename;
							objBannerImage.Url = Url;
							objBannerImage.Lang = Lang;
							objBannerImage.BannerType = MVType;
							objBannerImage.Module = "Website";
							objBannerImage.Description = Description;
							objBannerImage.DescriptionAr = DescriptionAr;
							objBannerImage.ContentType = ContentType;

							if (_bannnerImageService.CreateBanner(objBannerImage, ref message))
							{

							}
							TempData["SuccessMessage"] = "File uploaded successfully";
						}
						else
						{

							BannerImage objBannerImage = new BannerImage();
							string newfilename = "web-banner-header" + Guid.NewGuid().ToString() + ".jpg";
							string absolutePath = Server.MapPath("~");
							string relativePath = string.Format("/Assets/AppFiles/Banners/", newfilename);
							objBannerImage.ImagePath = Uploader.UploadVideo(Request.Files, absolutePath, relativePath, "Video", ref message, "file");

							//objBannerImage.ImagePath = "/Assets/AppFiles/Banners/" + newfilename;
							objBannerImage.Url = Url;
							objBannerImage.Lang = Lang;
							objBannerImage.BannerType = MVType;
							objBannerImage.Module = "Website";
							objBannerImage.Description = Description;
							objBannerImage.DescriptionAr = DescriptionAr;
							objBannerImage.ContentType = ContentType;

							if (_bannnerImageService.CreateBanner(objBannerImage, ref message))
							{

							}
							TempData["SuccessMessage"]= "File uploaded successfully";
						}

					}
					catch (Exception ex)
					{
						TempData["ErrorMessage"] = "ERROR:" + ex.Message.ToString();
					}
				else
				{
					TempData["ErrorMessage"] = "You have not specified a file.";
				}

			}
            else
            {
				TempData["ErrorMessage"] = "Cannot add more than One header image or video.";
			}
			
			return RedirectToAction("Manage");
		}


        [HttpPost]
        public ActionResult MotorSideBanner(HttpPostedFileBase file, string Url, string Lang, string ContentType, string MVType, string Description, string DescriptionAr)
        {
            var banner = _bannnerImageService.GetBannersByTypeAndLang("MotorSideBanner",Lang).ToList();
            if (banner.Count == 0)
            {
                string message = string.Empty;
                if (file != null && file.ContentLength > 0)

                    try
                    {
                            string path = Path.Combine(Server.MapPath("~/Assets/AppFiles/Banners/"));
                            string newfilename = "motor-side-banner" + Guid.NewGuid().ToString() + ".jpg";
                            // file.FileName = newfilename;

                            Path.GetFileName(file.FileName);
                            file.SaveAs(path + newfilename);
                            BannerImage objBannerImage = new BannerImage();
                            objBannerImage.ImagePath = "/Assets/AppFiles/Banners/" + newfilename;
                            objBannerImage.Url = Url;
                            objBannerImage.Lang = Lang;
                            objBannerImage.BannerType = MVType;
                            objBannerImage.Module = "Website";
                            objBannerImage.Description = Description;
                            objBannerImage.DescriptionAr = DescriptionAr;
                            objBannerImage.ContentType = ContentType;

                            if (_bannnerImageService.CreateBanner(objBannerImage, ref message))
                            {

                            }
						TempData["SuccessMessage"] = "File uploaded successfully";
                   

                    }
                    catch (Exception ex)
                    {
						TempData["ErrorMessage"] = "ERROR:" + ex.Message.ToString();
                    }
                else
                {
					TempData["ErrorMessage"] = "You have not specified a file.";
                }

            }
            else
            {
				TempData["ErrorMessage"] = "Cannot add more than One Side Banner image .";
            }

            return RedirectToAction("Manage");
        }
        [HttpPost]
        public ActionResult PropertySideBanner(HttpPostedFileBase file, string Url, string Lang, string ContentType, string MVType, string Description, string DescriptionAr)
        {
            var banner = _bannnerImageService.GetBannersByTypeAndLang("PropertySideBanner",Lang).ToList();
            if (banner.Count == 0)
            {
                string message = string.Empty;
                if (file != null && file.ContentLength > 0)

                    try
                    {
                        string path = Path.Combine(Server.MapPath("~/Assets/AppFiles/Banners/"));
                        string newfilename = "property-side-banner" + Guid.NewGuid().ToString() + ".jpg";
                        // file.FileName = newfilename;

                        Path.GetFileName(file.FileName);
                        file.SaveAs(path + newfilename);
                        BannerImage objBannerImage = new BannerImage();
                        objBannerImage.ImagePath = "/Assets/AppFiles/Banners/" + newfilename;
                        objBannerImage.Url = Url;
                        objBannerImage.Lang = Lang;
                        objBannerImage.BannerType = MVType;
                        objBannerImage.Module = "Website";
                        objBannerImage.Description = Description;
                        objBannerImage.DescriptionAr = DescriptionAr;
                        objBannerImage.ContentType = ContentType;

                        if (_bannnerImageService.CreateBanner(objBannerImage, ref message))
                        {

                        }
						TempData["SuccessMessage"] = "File uploaded successfully";


                    }
                    catch (Exception ex)
                    {
						TempData["ErrorMessage"] = "ERROR:" + ex.Message.ToString();
                    }
                else
                {
					TempData["ErrorMessage"] = "You have not specified a file.";
                }

            }
            else
            {
				TempData["ErrorMessage"] = "Cannot add more than One Side Banner image .";
            }

            return RedirectToAction("Manage");
        }
		[HttpPost]
		public ActionResult MainBanner(HttpPostedFileBase file, string Url, string ContentType, string Lang, string Type, string MVType, string Description, string DescriptionAr)
		{
			string message = string.Empty;
			if (file != null && file.ContentLength > 0)
				try
				{
					string path = Path.Combine(Server.MapPath("~/Assets/AppFiles/Banners/"));
					string newfilename = "web-banner-" + Guid.NewGuid().ToString() + ".jpg";
					// file.FileName = newfilename;

					Path.GetFileName(file.FileName);
					file.SaveAs(path + newfilename);
					BannerImage objBannerImage = new BannerImage();
					objBannerImage.ImagePath = "/Assets/AppFiles/Banners/" + newfilename;
					objBannerImage.Url = Url;
					objBannerImage.Lang = Lang;
					objBannerImage.BannerType = MVType;
					objBannerImage.Module = MVType == "Mobile" ? null : Type;
					objBannerImage.Description = Description;
					objBannerImage.DescriptionAr = DescriptionAr;
					objBannerImage.ContentType = ContentType;

					if (_bannnerImageService.CreateBanner(objBannerImage, ref message))
					{
						if(MVType != "Mobile")
                        {
							objBannerImage.Lang = "ar";
							_bannnerImageService.CreateBanner(objBannerImage, ref message);
						}
						
					}
					TempData["SuccessMessage"] = "File uploaded successfully";
				}
				catch (Exception ex)
				{
					TempData["ErrorMessage"] = "ERROR:" + ex.Message.ToString();
				}
			else
			{
				TempData["ErrorMessage"] = "You have not specified a file.";
			}
			return RedirectToAction("Manage");
		}
		[HttpPost]
		public ActionResult PromoBanner(HttpPostedFileBase file, string Url, string Lang, string Type, string MVType, string Description, string DescriptionAr)
		{
			string message = string.Empty;
			if (file != null && file.ContentLength > 0)
				try
				{
					string path = Path.Combine(Server.MapPath("~/Assets/AppFiles/PromoBanners/"));
					string newfilename = "promo-web-banner-" + Guid.NewGuid().ToString() + ".jpg";
					// file.FileName = newfilename;

					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}

					Path.GetFileName(file.FileName);
					file.SaveAs(path + newfilename);
					PromoBanner objBannerImage = new PromoBanner();
					objBannerImage.ImagePath = "/Assets/AppFiles/PromoBanners/" + newfilename;
					objBannerImage.Url = MVType.ToLower() == "mobile" ? string.Empty : Url;
					objBannerImage.Lang = Lang;
					objBannerImage.BannerType = MVType;
					objBannerImage.Module = Type;
					objBannerImage.Description = Description;
					objBannerImage.DescriptionAr = DescriptionAr;
					objBannerImage.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();

					if (_promobannnerImageService.CreateBanner(objBannerImage, ref message))
					{
						objBannerImage.Lang = "ar";
						_promobannnerImageService.CreateBanner(objBannerImage, ref message);
					}
					TempData["SuccessMessage"] = "File uploaded successfully";
				}
				catch (Exception ex)
				{
					TempData["ErrorMessage"] = "ERROR:" + ex.Message.ToString();
				}
			else
			{
				TempData["ErrorMessage"] = "You have not specified a file.";
			}
			return RedirectToAction("Manage");
		}
		public ActionResult Edit(long id)
		{
			BannerImage bannerData = _bannnerImageService.GetBanner(id);
			return View(bannerData);
		}

		[HttpPost]
		public ActionResult Edit(long id, string Url, string Lang, string MVType, string Description, string DescriptionAr)
		{
			try
			{
				string message = string.Empty;
				BannerImage objBannerImage = _bannnerImageService.GetBanner(id);
				objBannerImage.Url = Url;
				objBannerImage.Lang = Lang;
				objBannerImage.BannerType = MVType;
				/*bjBannerImage.Module = Type;*/
				objBannerImage.Description = Description;
				objBannerImage.DescriptionAr = DescriptionAr;
				if (_bannnerImageService.UpdateBanner(ref objBannerImage, ref message))
				{
					return Json(new
					{
						success = true,
						message = "Header updated successfully...",
						data = new { lang = objBannerImage.Lang, module = objBannerImage.Module }
					}, JsonRequestBehavior.AllowGet);
				}

				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
				}, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
				}, JsonRequestBehavior.AllowGet);
			}
		}
		public ActionResult Delete(long id)
		{

			try
			{
				string message = string.Empty;
				string absolutePath = Server.MapPath("~");
                string BannerType = string.Empty;
				BannerImage bannerData = _bannnerImageService.GetBanner(id);
				if (bannerData != null)
				{
					string BannerImage = bannerData.ImagePath;
					BannerType = bannerData.BannerType;
					if (_bannnerImageService.DeleteBanner(id, true))
					{
						var arbanner = _bannnerImageService.GetBannerBYUrl(BannerImage);
						if(arbanner != null)
                        {
							_bannnerImageService.DeleteBanner(arbanner.ID, true);
						}
						int mobilearbannear = _bannnerImageService.GetBanners().Count(x => x.BannerType == "Mobile" && x.Lang == "ar");
						int mobilearbanneen = _bannnerImageService.GetBanners().Count(x => x.BannerType == "Mobile" && x.Lang == "en");
						if (System.IO.File.Exists(absolutePath + bannerData.ImagePath))
						{
							System.IO.File.Delete(absolutePath + bannerData.ImagePath);
						}
						return Json(new
						{
							success = true,
							message = BannerType +" deleted successfully ...",
                            type = BannerType,
							mobilearbannear = mobilearbannear,
							mobilearbanneen = mobilearbanneen,

						});
					}
				}
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later.",
                    type = BannerType
                });
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later.",
                    type = ""
                });
			}
		}
		public ActionResult DeletePromoBanner(long id)
		{

			try
			{
				string message = string.Empty;
				string absolutePath = Server.MapPath("~");
				string BannerType = string.Empty;
				
				PromoBanner bannerData = _promobannnerImageService.GetBanner(id);
				if (bannerData != null)
				{
					BannerType = bannerData.BannerType;
					string BannerImage = bannerData.ImagePath;
					if (_promobannnerImageService.DeleteBanner(id, true))
					{
						var arbanner = _promobannnerImageService.GetBannerBYUrl(BannerImage);
						_promobannnerImageService.DeleteBanner(arbanner.ID, true);
						if (System.IO.File.Exists(absolutePath + bannerData.ImagePath))
						{
							System.IO.File.Delete(absolutePath + bannerData.ImagePath);
						}
						return Json(new
						{
							success = true,
							message = BannerType + " deleted successfully ...",
							type = BannerType
						});
					}
				}
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later.",
					type = BannerType
				});
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later.",
					type = ""
				});
			}
		}
	}
}