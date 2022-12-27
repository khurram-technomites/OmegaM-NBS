using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using NowBuySell.Web.Areas.Admin.ViewModels.Banners;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class WebsiteController : Controller
	{
		private readonly IBannerImagesService _bannnerImageService;

		public WebsiteController(IBannerImagesService bannnerImageService)
		{
			this._bannnerImageService = bannnerImageService;
		}

		public ActionResult Manage()
		{
			BannersViewModel objBannersViewModel = new BannersViewModel()
			{
				WebsiteBanners = _bannnerImageService.GetBannersByType("Website").ToList(),
				MobileBanners = _bannnerImageService.GetBannersByType("Mobile").ToList()
			};

			return View(objBannersViewModel);
		}

		[HttpPost]
		public ActionResult Manage(HttpPostedFileBase file, string Url, string Lang, string Type, string MVType, string Description, string DescriptionAr)
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

					if (_bannnerImageService.CreateBanner(objBannerImage, ref message))
					{

					}
					ViewBag.Message = "File uploaded successfully";
				}
				catch (Exception ex)
				{
					ViewBag.Message = "ERROR:" + ex.Message.ToString();
				}
			else
			{
				ViewBag.Message = "You have not specified a file.";
			}
			return RedirectToAction("Manage");
		}

		[HttpPost]
		public ActionResult ManageImageForMobile(HttpPostedFileBase file, string Url,string Lang)
		{
			string message = string.Empty;
			if (file != null && file.ContentLength > 0)
				try
				{
					string path = Path.Combine(Server.MapPath("~/Assets/AppFiles/MobileBanners/"));
					string newfilename = "Mob-banner-" + Guid.NewGuid().ToString() + ".jpg";
					// file.FileName = newfilename;


					Path.GetFileName(file.FileName);
					file.SaveAs(path + newfilename);
					BannerImage obj = new BannerImage();
					obj.ImagePath = "/Assets/AppFiles/MobileBanners/" + newfilename;
					obj.Url = Url;
					obj.Lang = Lang;
					obj.BannerType = "Mobile";
					if (_bannnerImageService.CreateBanner(obj, ref message))
					{

					}
					ViewBag.Message = "File uploaded successfully";
				}
				catch (Exception ex)
				{
					ViewBag.Message = "ERROR:" + ex.Message.ToString();
				}
			else
			{
				ViewBag.Message = "You have not specified a file.";
			}
			return RedirectToAction("Manage");
		}

		public ActionResult Edit(long id)
		{
			BannerImage bannerData = _bannnerImageService.GetBanner(id);
			return View(bannerData);
		}

		[HttpPost]
		public ActionResult Edit(long id, string Url, string Lang, string Type, string MVType, string Description, string DescriptionAr)
		{
			try
			{
				string message = string.Empty;
				BannerImage objBannerImage = _bannnerImageService.GetBanner(id);
				objBannerImage.Url = Url;
				objBannerImage.Lang = Lang;
				objBannerImage.BannerType = MVType;
				objBannerImage.Module = Type;
				objBannerImage.Description = Description;
				objBannerImage.DescriptionAr = DescriptionAr;
				if (_bannnerImageService.UpdateBanner(ref objBannerImage, ref message))
				{
					return Json(new
					{
						success = true,
						message = "Banner updated successfully...",
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

		public ActionResult EditMobileBanners(long id)
		{
			BannerImage bannerData = _bannnerImageService.GetBanner(id);
			return View(bannerData);
		}

		[HttpPost]
		public ActionResult EditMobileBanners(long id, string Url , string Lang)
		{
			string message = string.Empty;
			BannerImage bannerData = _bannnerImageService.GetBanner(id);
			bannerData.Lang = Lang;
			bannerData.Url = Url;
			try
			{
				if (_bannnerImageService.UpdateBanner(ref bannerData, ref message))
				{
					return Json(new
					{ 
						success = true,
						data = new { lang = bannerData.Lang , url = bannerData.Lang},
						message = "Banner updated successfully ..."
					});
				}
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
				});
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		public ActionResult Delete(long id)
		{

			try
			{
				string message = string.Empty;
				string absolutePath = Server.MapPath("~");
				BannerImage bannerData = _bannnerImageService.GetBanner(id);
				if (bannerData != null)
				{
					if (_bannnerImageService.DeleteBanner(id, true))
					{
						if (System.IO.File.Exists(absolutePath + bannerData.ImagePath))
						{
							System.IO.File.Delete(absolutePath + bannerData.ImagePath);
						}
						return Json(new
						{
							success = true,
							message = "Banner deleted successfully ..."
						});
					}
				}
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
				});
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		public ActionResult Promotion()
		{
			BannersViewModel objBannersViewModel = new BannersViewModel()
			{
				PromotionBanners = _bannnerImageService.GetBannersByType("Promotion").ToList(),
				MobileBanners = _bannnerImageService.GetBannersByType("PromotionMobile").ToList()
			};

			return View(objBannersViewModel);
		}

		[HttpPost]
		public ActionResult Promotion(HttpPostedFileBase file, string Url, string Lang)
		{
			string message = string.Empty;
			if (file != null && file.ContentLength > 0)
				try
				{
					string path = Path.Combine(Server.MapPath("~/Assets/AppFiles/Banners/"));
					string newfilename = "web-banner-" + Guid.NewGuid().ToString() + ".jpg";

					BannerImage objBannerImage = new BannerImage();
					objBannerImage.ImagePath = "/Assets/AppFiles/Banners/" + newfilename;
					objBannerImage.Url = Url;
					objBannerImage.Lang = Lang;
					objBannerImage.BannerType = "Promotion";

					if (_bannnerImageService.CreateBanner(objBannerImage, ref message))
					{
						Path.GetFileName(file.FileName);
						file.SaveAs(path + newfilename);

					}
					ViewBag.Message = "File uploaded successfully";
				}
				catch (Exception ex)
				{
					ViewBag.Message = "ERROR:" + ex.Message.ToString();
				}
			else
			{
				ViewBag.Message = "You have not specified a file.";
			}
			return RedirectToAction("Promotion");
		}

		public ActionResult EditPromotionBanners(long id)
		{
			BannerImage bannerData = _bannnerImageService.GetBanner(id);
			return View(bannerData);
		}

		[HttpPost]
		public ActionResult EditPromotionBanners(long id, string Url, string Lang)
		{
			try
			{
				string message = string.Empty;
				BannerImage objBannerImage = _bannnerImageService.GetBanner(id);
				objBannerImage.Url = Url;
				objBannerImage.Lang = Lang;
				if (_bannnerImageService.UpdateBanner(ref objBannerImage, ref message))
				{
					return Json(new
					{
						success = true,
						message = "Banner updated successfully...",
						data = new { lang = objBannerImage.Lang }
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

		public ActionResult DeletePromotionBanners(long id)
		{
			try
			{
				string message = string.Empty;
				string absolutePath = Server.MapPath("~");
				BannerImage bannerData = _bannnerImageService.GetBanner(id);
				if (bannerData != null)
				{
					if (_bannnerImageService.DeleteBanner(id, true))
					{
						if (System.IO.File.Exists(absolutePath + bannerData.ImagePath))
						{
							System.IO.File.Delete(absolutePath + bannerData.ImagePath);
						}
						return Json(new
						{
							success = true,
							message = "Banner deleted successfully ..."
						});
					}
				}
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
				});
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		[HttpPost]
		public ActionResult PromotionImageForMobile(HttpPostedFileBase file, string Url, string Lang)
		{
			string message = string.Empty;
			if (file != null && file.ContentLength > 0)
				try
				{
					string path = Path.Combine(Server.MapPath("~/Assets/AppFiles/MobileBanners/"));
					string newfilename = "Mob-banner-" + Guid.NewGuid().ToString() + ".jpg";
					// file.FileName = newfilename;


					Path.GetFileName(file.FileName);
					file.SaveAs(path + newfilename);
					BannerImage obj = new BannerImage();
					obj.ImagePath = "/Assets/AppFiles/MobileBanners/" + newfilename;
					obj.Url = Url;
					obj.Lang = Lang;
					obj.BannerType = "PromotionMobile";
					if (_bannnerImageService.CreateBanner(obj, ref message))
					{

					}
					ViewBag.Message = "File uploaded successfully";
				}
				catch (Exception ex)
				{
					ViewBag.Message = "ERROR:" + ex.Message.ToString();
				}
			else
			{
				ViewBag.Message = "You have not specified a file.";
			}
			return RedirectToAction("Promotion");
		}

		public ActionResult EditPromotionMobileBanners(long id)
		{
			BannerImage bannerData = _bannnerImageService.GetBanner(id);
			return View(bannerData);
		}

		[HttpPost]
		public ActionResult EditPromotionMobileBanners(long id, string Lang)
		{
			string message = string.Empty;
			BannerImage bannerData = _bannnerImageService.GetBanner(id);
			bannerData.Lang = Lang ;
            try { 
			if (_bannnerImageService.UpdateBanner(ref bannerData, ref message))
			{
					return Json(new
					{
						success = true,
						data = new { lang = bannerData.Lang },
						message = "Banner updated successfully ..."
					}); 
			}
			return Json(new
			{
				success = false,
				message = "Oops! Something went wrong. Please try later."
			});
		}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Oops! Something went wrong. Please try later."
				});
			}

                
		}
	}
}