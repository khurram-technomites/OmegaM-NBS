using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class ServicesCategoryController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IServicesCategoryService _servicesCategoryService;

        public ServicesCategoryController(IServicesCategoryService servicesCategoryService)
        {
            this._servicesCategoryService = servicesCategoryService;
        }

        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }
        public ActionResult List()
        {
            var servicesCategories = _servicesCategoryService.GetServicesCategories();
            return PartialView(servicesCategories);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceCategory serviceCategory = _servicesCategoryService.GetServiceCategory((Int16)id);
            if (serviceCategory == null)
            {
                return HttpNotFound();
            }
            return View(serviceCategory);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string Name, string Description, string Slug, bool IsDefault)
        {
            try
            {
                string message = string.Empty;
                string error = string.Empty;
                if (ModelState.IsValid)
                {
                    Regex pattern = new Regex("[?;,.+=~`$&@*\']");
                    string imgName = pattern.Replace(Name, "_");
                    string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/ServicesCategory/"), imgName, "/Image");
                    string absolutePath = Server.MapPath("~");
                    string relativePath = string.Format("/Assets/AppFiles/Images/ServicesCategory/{0}/", imgName);
                    ServiceCategory serviceCategory = new ServiceCategory();
                    serviceCategory.Name = Name;
                    serviceCategory.Slug = Slug;
                    serviceCategory.Description = Description;
                    serviceCategory.IsDefault = IsDefault;
                    serviceCategory.Image = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "thumbnail", ref message, "Image");

                    if (_servicesCategoryService.CreateServiceCategory(serviceCategory, ref message, ref error))
                    {
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/ServicesCategory/Index",
                            message = message,
                            data = new
                            {
                                ID = serviceCategory.ID,
                                Date = serviceCategory.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                                ServiceCategory = serviceCategory.Image + "|" + serviceCategory.Name,
                                IsActive = serviceCategory.IsActive.HasValue ? serviceCategory.IsActive.Value.ToString() : bool.FalseString
                            }
                        });
                    }
                }
                else
                {
                    message = "Please fill the form properly ...";
                    error = "ModelState is not valid.";
                }
                return Json(new { success = false, message = message, error = error });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Oops! Something went wrong. Please try later.",
                    error = ex.Message
                });
            }
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceCategory serviceCategory = _servicesCategoryService.GetServiceCategory((long)id);
            if (serviceCategory == null)
            {
                return HttpNotFound();
            }

            TempData["ServiceCategoryID"] = id;
            return View(serviceCategory);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, string Name, string Description, string Slug, bool IsDefault)
        {
            try
            {
                string message = string.Empty;
                string error = string.Empty;
                if (ModelState.IsValid)
                {
                    long Id;
                    if (TempData["ServiceCategoryID"] != null && Int64.TryParse(TempData["ServiceCategoryID"].ToString(), out Id) && id == Id)
                    {
                        ServiceCategory serviceCategory = _servicesCategoryService.GetServiceCategory(id);
                        serviceCategory.Name = Name;
                        serviceCategory.Slug = Slug;
                        serviceCategory.Description = Description;
                        serviceCategory.IsDefault = IsDefault;

                        if (Request.Files["Image"] != null)
                        {
                            Regex pattern = new Regex("[?;,.+=~`$&@*\' ]");
                            string imgName = pattern.Replace(Name, "_");
                            string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/ServicesCategory/"), imgName, "/Image");

                            string absolutePath = Server.MapPath("~");
                            string relativePath = string.Format("/Assets/AppFiles/Images/ServicesCategory/{0}/", imgName);
                            serviceCategory.Image = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "thumbnail", ref message, "Image");
                        }

                        if (_servicesCategoryService.UpdateServiceCategory(ref serviceCategory, ref message, ref error, false))
                        {
                            return Json(new
                            {
                                success = true,
                                url = "/Admin/ServicesCategory/Index",
                                message = "Category updated successfully ...",
                                data = new
                                {
                                    ID = serviceCategory.ID,
                                    Date = serviceCategory.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                                    ServiceCategory = serviceCategory.Image + "|" + serviceCategory.Name,
                                    IsActive = serviceCategory.IsActive.HasValue ? serviceCategory.IsActive.Value.ToString() : bool.FalseString
                                }
                            });
                        }
                    }
                    else
                    {
                        message = "Oops! Something went wrong. Please try later.";
                        error = "Service Category not found.";
                    }
                }
                else
                {
                    message = "Please fill the form properly ...";
                    error = "ModelState is not valid.";
                }
                return Json(new { success = false, message = message, error = error });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Oops! Something went wrong. Please try later.",
                    error = ex.Message
                });
            }
        }

        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var serviceCategory = _servicesCategoryService.GetServiceCategory((long)id);
            if (serviceCategory == null)
            {
                return HttpNotFound();
            }

            if (!(bool)serviceCategory.IsActive)
                serviceCategory.IsActive = true;
            else
            {
                serviceCategory.IsActive = false;
            }
            string message = string.Empty;
            string error = string.Empty;
            if (_servicesCategoryService.UpdateServiceCategory(ref serviceCategory, ref message, ref error))
            {
                SuccessMessage = "Service Category " + ((bool)serviceCategory.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        ID = serviceCategory.ID,
                        Date = serviceCategory.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        ServiceCategory = serviceCategory.Image + "|" + serviceCategory.Name,
                        IsActive = serviceCategory.IsActive.HasValue ? serviceCategory.IsActive.Value.ToString() : bool.FalseString
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage, error = error }, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceCategory serviceCategory = _servicesCategoryService.GetServiceCategory((Int16)id);
            if (serviceCategory == null)
            {
                return HttpNotFound();
            }
            TempData["ServiceCategoryID"] = id;
            return View(serviceCategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            string error = string.Empty;

            if (_servicesCategoryService.DeleteServiceCategory((Int16)id, ref message, ref error))
            {
                return Json(new
                {
                    success = true,
                    message = message,
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message, error = error }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ServicesCategoriesReport()
        {
            string ImageServer = CustomURL.GetImageServer();
            var getAllServiceCatagories = _servicesCategoryService.GetServicesCategories().ToList();
            if (getAllServiceCatagories.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("ServiceCategoriesReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Name"
                        ,"Description"
                        ,"Slug"
                        ,"Image"
                        ,"Show on Header"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["ServiceCategoriesReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    if (getAllServiceCatagories.Count != 0)
                        getAllServiceCatagories = getAllServiceCatagories.OrderByDescending(x => x.ID).ToList();

                    foreach (var i in getAllServiceCatagories)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
                        ,!string.IsNullOrEmpty(i.Description) ? i.Description : "-"
                        ,!string.IsNullOrEmpty(i.Slug) ? i.Slug : "-"
                        ,!string.IsNullOrEmpty(i.Image) ? (ImageServer + i.Image) : "-"
                        ,i.IsDefault == true ? "Yes" : "No"
                        ,i.IsActive == true ? "Active" : "InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Service Categories Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }
    }
}