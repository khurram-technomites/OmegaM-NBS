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
    public class ServicesController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IServicesService _servicesService;
        private readonly IServicesCategoryService _serviceCategoryService;

        public ServicesController(IServicesService servicesService, IServicesCategoryService serviceCategoryService)
        {
            this._servicesService = servicesService;
            this._serviceCategoryService = serviceCategoryService;
        }

        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        public ActionResult List()
        {
            var cities = _servicesService.GetServices();
            return PartialView(cities);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceCompare service = _servicesService.GetService((Int16)id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(_serviceCategoryService.GetServiceCategoriesForDropDown(), "value", "text");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string Name, long? CategoryID, string Description, string Slug)
        {
            try
            {
                string message = string.Empty;
                string error = string.Empty;
                if (ModelState.IsValid)
                {
                    Regex pattern = new Regex("[?;,.+=~`$&*\'\" ]");
                    string imgName = pattern.Replace(Name, "_");
                    string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/Services/"), imgName, "/Image");

                    string absolutePath = Server.MapPath("~");
                    string relativePath = string.Format("/Assets/AppFiles/Images/Services/{0}/", imgName);
                    ServiceCompare service = new ServiceCompare();
                    service.Name = Name;
                    service.CategoryID = CategoryID;
                    service.Slug = Slug;
                    service.Description = Description;
                    service.Image = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "thumbnail", ref message, "Image");

                    if (_servicesService.CreateService(service, ref message, ref error))
                    {
                        var serviceCategory = service.CategoryID.HasValue ? _serviceCategoryService.GetServiceCategory((long)service.CategoryID) : null;
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/Services/Index",
                            message = message,
                            data = new
                            {
                                ID = service.ID,
                                Date = service.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                                Service = service.Image + "|" + service.Name,
                                ServiceCategory = serviceCategory != null ? (serviceCategory.Name) : "",
                                IsActive = service.IsActive.HasValue ? service.IsActive.Value.ToString() : bool.FalseString
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
            ServiceCompare service = _servicesService.GetService((long)id);
            if (service == null)
            {
                return HttpNotFound();
            }

            ViewBag.CategoryID = new SelectList(_serviceCategoryService.GetServiceCategoriesForDropDown(), "value", "text", service.CategoryID);

            TempData["ServiceID"] = id;
            return View(service);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, string Name, long? CategoryID, string Description, string Slug)
        {
            try
            {
                string message = string.Empty;
                string error = string.Empty;
                if (ModelState.IsValid)
                {
                    long Id;
                    if (TempData["ServiceID"] != null && Int64.TryParse(TempData["ServiceID"].ToString(), out Id) && id == Id)
                    {

                        ServiceCompare service = _servicesService.GetService(id);
                        service.Name = Name;
                        service.CategoryID = CategoryID;
                        service.Slug = Slug;
                        service.Description = Description;

                        if (Request.Files["Image"] != null)
                        {
                            Regex pattern = new Regex("[;,.+=~`$&*\'\" ]");
                            string imgName = pattern.Replace(Name, "_");
                            string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/Services/"), imgName, "/Image");

                            string absolutePath = Server.MapPath("~");
                            string relativePath = string.Format("/Assets/AppFiles/Images/Services/{0}/", imgName);
                            service.Image = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "thumbnail", ref message, "Image");
                        }

                        if (_servicesService.UpdateService(ref service, ref message, ref error, false))
                        {
                            var serviceCategory = service.CategoryID.HasValue ? _serviceCategoryService.GetServiceCategory((long)service.CategoryID) : null;
                            return Json(new
                            {
                                success = true,
                                url = "/Admin/Services/Index",
                                message = "Service updated successfully ...",
                                data = new
                                {
                                    ID = service.ID,
                                    Date = service.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                                    Service = service.Image + "|" + service.Name,
                                    ServiceCategory = serviceCategory != null ? (serviceCategory.Name) : "",
                                    IsActive = service.IsActive.HasValue ? service.IsActive.Value.ToString() : bool.FalseString
                                }
                            });
                        }
                    }
                    else
                    {
                        message = "Oops! Something went wrong. Please try later.";
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
            var service = _servicesService.GetService((long)id);
            if (service == null)
            {
                return HttpNotFound();
            }

            if (!(bool)service.IsActive)
                service.IsActive = true;
            else
            {
                service.IsActive = false;
            }
            string message = string.Empty;
            string error = string.Empty;
            if (_servicesService.UpdateService(ref service, ref message, ref error))
            {
                SuccessMessage = "Service Category " + ((bool)service.IsActive ? "activated" : "deactivated") + "  successfully ...";
                var serviceCategory = service.CategoryID.HasValue ? _serviceCategoryService.GetServiceCategory((long)service.CategoryID) : null;
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        ID = service.ID,
                        Date = service.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        Service = service.Image + "|" + service.Name,
                        ServiceCategory = serviceCategory != null ? (serviceCategory.Name) : "",
                        IsActive = service.IsActive.HasValue ? service.IsActive.Value.ToString() : bool.FalseString
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
            ServiceCompare service = _servicesService.GetService((Int16)id);
            if (service == null)
            {
                return HttpNotFound();
            }
            TempData["ServiceID"] = id;
            return View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            string error = string.Empty;

            if (_servicesService.DeleteService((Int16)id, ref message, ref error))
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
        public ActionResult ServicesReport()
        {
            string ImageServer = CustomURL.GetImageServer();
            var getAllServices = _servicesService.GetServices().ToList();
            if (getAllServices.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("ServicesReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Name"
                        ,"Description"
                        ,"Slug"
						,"Service Category"
                        ,"Image"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["ServicesReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    if (getAllServices.Count != 0)
                        getAllServices = getAllServices.OrderByDescending(x => x.ID).ToList();

                    foreach (var i in getAllServices)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
                        ,!string.IsNullOrEmpty(i.Description) ? i.Description : "-"
                        ,!string.IsNullOrEmpty(i.Slug) ? i.Slug : "-"
                        ,i.ServiceCategory != null ? (!string.IsNullOrEmpty(i.ServiceCategory.Name) ? i.ServiceCategory.Name : "-") : "-"
                        ,!string.IsNullOrEmpty(i.Image) ? (ImageServer + i.Image) : "-"
                        ,i.IsActive == true ? "Active" : "InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Services Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }
    }
}