using NowBuySell.Web.Areas.VendorPortal.ViewModels.Gallery;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.Util;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
	[AuthorizationProvider.AuthorizeVendor]
	public class GalleryController : Controller
	{
		public ActionResult Index()
		{
			var vendor = Session["VendorID"];
			if (!Directory.Exists(Server.MapPath("~") + "/Assets/AppFiles/Gallery/" + vendor + "/"))
			{
				Directory.CreateDirectory(Server.MapPath("~") + "/Assets/AppFiles/Gallery/" + vendor + "/");
			}

			var directoryPath = Server.MapPath("~/Assets/AppFiles/Gallery/" + vendor + "/");

			DirectoryInfo d = new DirectoryInfo(directoryPath);//Assuming Test is your Folder
			string ImageServer = CustomURL.GetImageServer();

			var banners = d.GetFiles().Select(i => new GalleryViewModel()
			{
				Name = i.Name,
				Path = ImageServer + "/Assets/AppFiles/Gallery/" + vendor + "/" + i.Name
			}).ToList();

			return View(banners);
		}

		[HttpPost]
		//[ValidateAntiForgeryToken]
		public ActionResult Create(HttpPostedFileBase FileUpload, string connectionId)
		{
			try
			{

				string ErrorMessage = string.Empty;
				string data = "";
				string message = "";
				List<string> ErrorItems = new List<string>();
				int count = 0;
				var total = Request.Files.Count;
				int successCount = 0;

				var vendor = Session["VendorID"];
				for (int index = 0; index < Request.Files.Count; index++)
				{
					string name = string.Empty;
					string FilePath = string.Empty;
					string MainDirectory = string.Empty;
					try
					{
						var file = Request.Files[index];

						if (file != null && Request.Files.AllKeys[index].Equals("fileUpload"))
						{
							string[] SupportedImageFormat = { ".jpeg", ".png", ".jpg",".xlsx",".docx", ".mp4", ".MKV", ".FLV", ".MOV" };
							String fileExtension = System.IO.Path.GetExtension(file.FileName);
							name = System.IO.Path.GetFileName(file.FileName);

							//if (file.ContentType.Contains("image"))
							//{
								if (SupportedImageFormat.Contains(fileExtension.ToLower()))
								{
									FilePath = string.Format("{0}{1}/{2}_{3}{4}", "/Assets/AppFiles/Gallery/", vendor, name, Guid.NewGuid().ToString(), fileExtension);


									if (!Directory.Exists(Server.MapPath("~") + "/Assets/AppFiles/Gallery/" + vendor + "/"))
									{
										Directory.CreateDirectory(Server.MapPath("~") + "/Assets/AppFiles/Gallery/" + vendor + "/");
									}
									MainDirectory = Path.Combine(Server.MapPath("~" + FilePath));
									file.SaveAs(MainDirectory);

									successCount++;
								}
								else
								{
									ErrorItems.Add(string.Format("<b>Image '{0}' Not Inserted:</b><br>Image Format Not Supported", index + 1));
								}
							//}
							//else
							//{
							//	ErrorItems.Add(string.Format("<b>Image '{0}' Not Inserted:</b><br>Wrong format for image", index + 1));
							//}
						}
						else
						{
							ErrorItems.Add(string.Format("<b>Image '{0}' Not Inserted:</b><br>file not found", index + 1));
						}
						count++;
					}
					catch (Exception ex)
					{
						ErrorItems.Add(string.Format("<b>Image '{0}' Not Inserted:</b><br>Internal Server Serror", index + 1));
					}

					//CALLING A FUNCTION THAT CALCULATES PERCENTAGE AND SENDS THE DATA TO THE CLIENT
					Functions.SendGalleryProgress(connectionId, "Upload in progress...", count, total, name, FilePath);
				}

				return Json(new
				{
					success = true,
					successMessage = string.Format("{0} Images uploaded!", (successCount)),
					errorMessage = (ErrorItems.Count() > 0) ? string.Format("{0} Images are not uploaded!", total - successCount) : null,
					detailedErrorMessages = (ErrorItems.Count() > 0) ? string.Join<string>("<br>", ErrorItems) : null,
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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(GalleryViewModel galleryViewModel)
		{
			try
			{
				galleryViewModel.Path = galleryViewModel.Path.Replace(CustomURL.GetImageServer(), "");
				if (System.IO.File.Exists(Server.MapPath("~") + galleryViewModel.Path))
				{
					System.IO.File.Delete(Server.MapPath("~") + galleryViewModel.Path);
				}
				return Json(new
				{
					success = true,
					message = "Image deleted successfully ..."
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
		[ValidateAntiForgeryToken]
		public ActionResult DeleteAll()
		{
			try
			{

				var vendor = Session["VendorID"];
				var directoryPath = Server.MapPath("~/Assets/AppFiles/Gallery/" + vendor + "/");
				System.IO.DirectoryInfo di = new DirectoryInfo(directoryPath);

				foreach (FileInfo file in di.GetFiles())
				{
					file.Delete();
				}
				foreach (DirectoryInfo dir in di.GetDirectories())
				{
					dir.Delete(true);
				}

				return Json(new
				{
					success = true,
					message = "All Images deleted successfully ..."
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
		[ValidateAntiForgeryToken]
		public ActionResult Report()
		{
			var vendor = Session["VendorID"];
			if (!Directory.Exists(Server.MapPath("~") + "/Assets/AppFiles/Gallery/" + vendor + "/"))
			{
				Directory.CreateDirectory(Server.MapPath("~") + "/Assets/AppFiles/Gallery/" + vendor + "/");
			}

			var directoryPath = Server.MapPath("~/Assets/AppFiles/Gallery/" + vendor + "/");

			DirectoryInfo d = new DirectoryInfo(directoryPath);//Assuming Test is your Folder
			string ImageServer = CustomURL.GetImageServer();

			var gallery = d.GetFiles().Select(i => new GalleryViewModel()
			{
				Name = i.Name,
				Path = ImageServer + "/Assets/AppFiles/Gallery/" + vendor + "/" + i.Name
			}).ToList();

			if (gallery.Count() > 0)
			{
				using (ExcelPackage excel = new ExcelPackage())
				{
					excel.Workbook.Worksheets.Add("Gallery");

					var headerRow = new List<string[]>()
					{
					new string[] {
						"Images"
						}
					};

					// Determine the header range (e.g. A1:D1)
					string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

					// Target a worksheet
					var worksheet = excel.Workbook.Worksheets["Gallery"];

					// Popular header row data
					worksheet.Cells[headerRange].LoadFromArrays(headerRow);

					var cellData = new List<object[]>();

					foreach (var i in gallery)
					{
						cellData.Add(new object[]
						{
						i.Path
						});
					}

					worksheet.Cells[2, 1].LoadFromArrays(cellData);

					return File(excel.GetAsByteArray(), "application/msexcel", "Gallery Report.xlsx");
				}
			}
			return RedirectToAction("Index");
		}
	}
}