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
    public class ServicesCarController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IServicesCarService _servicesCarService;
        private readonly IServicesService _servicesService;

        public ServicesCarController(IServicesCarService servicesCarService, IServicesService servicesService)
        {
            this._servicesCarService = servicesCarService;
            this._servicesService = servicesService;
        }

        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        public ActionResult List()
        {
            var cars = _servicesCarService.GetServiceCars();
            return PartialView(cars);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceCar car = _servicesCarService.GetServiceCar((Int16)id);
            if (car == null)
            {
                return HttpNotFound();
            }
            return View(car);
        }
        public ActionResult Create()
        {
            ViewBag.ServiceID = new SelectList(_servicesService.GetServicesForDropDown(), "value", "text");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string Title, long? ServiceID, string Description, string MobileDescription, string Slug, decimal? Fee)
        {
            try
            {
                string message = string.Empty;
                string error = string.Empty;
                if (ModelState.IsValid)
                {
                    Regex pattern = new Regex("[?;,.+=~`$&*\'\" ]");
                    string imgName = pattern.Replace(Title, "_");
                    string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/ServiceCars/"), imgName, "/Image");

                    string absolutePath = Server.MapPath("~");
                    string relativePath = string.Format("/Assets/AppFiles/Images/ServiceCarss/{0}/", imgName);
                    ServiceCar car = new ServiceCar();
                    car.Title = Title;
                    car.ServiceID = ServiceID;
                    car.Slug = Slug;
                    car.Description = Description;
                    car.MobileDescription = MobileDescription;
                    car.Fee = Fee ?? 0;
                    car.Image = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "thumbnail", ref message, "Image");

                    if (_servicesCarService.CreateServiceCar(car, ref message, ref error))
                    {
                        var service = car.ServiceID.HasValue ? _servicesService.GetService((long)car.ServiceID) : null;
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/ServicesCar/Index",
                            message = message,
                            data = new
                            {
                                ID = car.ID,
                                Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                                Car = car.Image + "|" + car.Title,
                                Service = service != null ? (service.Name) : "",
                                Fee = car.Fee ?? 0,
                                IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString
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
            ServiceCar car = _servicesCarService.GetServiceCar((long)id);
            if (car == null)
            {
                return HttpNotFound();
            }

            ViewBag.ServiceID = new SelectList(_servicesService.GetServicesForDropDown(), "value", "text", car.ServiceID);

            TempData["CarID"] = id;
            return View(car);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, string Title, long? ServiceID, string Description, string MobileDescription, string Slug, decimal? Fee)
        {
            try
            {
                string message = string.Empty;
                string error = string.Empty;
                if (ModelState.IsValid)
                {
                    long Id;
                    if (TempData["CarID"] != null && Int64.TryParse(TempData["CarID"].ToString(), out Id) && id == Id)
                    {
                        ServiceCar car = _servicesCarService.GetServiceCar(id);
                        car.Title = Title;
                        car.ServiceID = ServiceID;
                        car.Slug = Slug;
                        car.Description = Description;
                        car.MobileDescription = MobileDescription;
                        car.Fee = Fee ?? 0;

                        if (Request.Files["Image"] != null)
                        {
                            Regex pattern = new Regex("[;,.+=~`$&*\'\" ]");
                            string imgName = pattern.Replace(Title, "_");
                            string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/ServiceCars/"), imgName, "/Image");

                            string absolutePath = Server.MapPath("~");
                            string relativePath = string.Format("/Assets/AppFiles/Images/ServiceCars/{0}/", imgName);
                            car.Image = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "thumbnail", ref message, "Image");
                        }

                        if (_servicesCarService.UpdateServiceCar(ref car, ref message, ref error, false))
                        {
                            var service = car.ServiceID.HasValue ? _servicesService.GetService((long)car.ServiceID) : null;
                            return Json(new
                            {
                                success = true,
                                url = "/Admin/ServicesCar/Index",
                                message = "Service updated successfully ...",
                                data = new
                                {
                                    ID = car.ID,
                                    Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                                    Car = car.Image + "|" + car.Title,
                                    Service = service != null ? (service.Name) : "",
                                    Fee = car.Fee ?? 0,
                                    IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString
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
            var car = _servicesCarService.GetServiceCar((long)id);
            if (car == null)
            {
                return HttpNotFound();
            }

            if (!(bool)car.IsActive)
                car.IsActive = true;
            else
            {
                car.IsActive = false;
            }
            string message = string.Empty;
            string error = string.Empty;
            if (_servicesCarService.UpdateServiceCar(ref car, ref message, ref error))
            {
                SuccessMessage = "Service Car " + ((bool)car.IsActive ? "activated" : "deactivated") + "  successfully ...";
                var service = car.ServiceID.HasValue ? _servicesService.GetService((long)car.ServiceID) : null;
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        ID = car.ID,
                        Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        Car = car.Image + "|" + car.Title,
                        Service = service != null ? (service.Name) : "",
                        Fee = car.Fee ?? 0,
                        IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString
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
            ServiceCar car = _servicesCarService.GetServiceCar((Int16)id);
            if (car == null)
            {
                return HttpNotFound();
            }
            TempData["CarID"] = id;
            return View(car);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            string error = string.Empty;

            if (_servicesCarService.DeleteServiceCar((Int16)id, ref message, ref error))
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
        public ActionResult ServicesCarReport()
        {
            string ImageServer = CustomURL.GetImageServer();
            var getAllServices = _servicesCarService.GetServiceCars().ToList();
            if (getAllServices.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("ServiceCarReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Title"
                        ,"Description"
                        ,"Mobile Description"
                        ,"Slug"
                        ,"Fee"
                        ,"Attributes"
                        ,"Service"
                        ,"Service Category"
                        ,"Image"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["ServiceCarReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    if (getAllServices.Count != 0)
                        getAllServices = getAllServices.OrderByDescending(x => x.ID).ToList();

                    foreach (var i in getAllServices)
                    {
                        var attributes = "";
                        if (i.ServiceCarAttributes.Count != 0)
                        {
                            attributes = string.Join(", ", i.ServiceCarAttributes.Select(x => $"({x.Name}: {x.Value})"));
                        }
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Title) ? i.Title : "-"
                        ,!string.IsNullOrEmpty(i.Description) ? i.Description : "-"
                        ,!string.IsNullOrEmpty(i.MobileDescription) ? i.MobileDescription : "-"
                        ,!string.IsNullOrEmpty(i.Slug) ? i.Slug : "-"
                        ,i.Fee ?? 0
                        ,attributes
                        ,i.ServiceCompare != null ? (!string.IsNullOrEmpty(i.ServiceCompare.Name) ? i.ServiceCompare.Name : "-") : "-"
                        ,i.ServiceCompare != null ? (i.ServiceCompare.ServiceCategory != null ? (!string.IsNullOrEmpty(i.ServiceCompare.ServiceCategory.Name) ? i.ServiceCompare.ServiceCategory.Name : "-") : "-") : "-"
                        ,!string.IsNullOrEmpty(i.Image) ? (ImageServer + i.Image) : "-"
                        ,i.IsActive == true ? "Active" : "InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Service Car Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }


        #region Service Car Attributes

        public ActionResult Attributes(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IEnumerable<SP_GetServiceCarAtributes_Result> attributes = _servicesCarService.GetCarAttribute((long)id);

            if (attributes == null)
            {
                return HttpNotFound();
            }
            TempData["CarID"] = id;

            return PartialView(attributes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Attributes(long? id, string Name, string Value, long? carId)
        {
            string message = string.Empty;
            string error = string.Empty;
            if (id == null)//Create Attribute
            {
                long Id;
                if (TempData["CarID"] != null && Int64.TryParse(TempData["CarID"].ToString(), out Id))
                {
                    TempData.Keep("CarID");
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            ServiceCarAttribute attributes = new ServiceCarAttribute();
                            attributes.Name = Name;
                            attributes.Value = Value;
                            attributes.ServiceCarsID = Id;

                            if (_servicesCarService.CreateServiceCarAttribute(attributes, ref message, ref error))
                            {
                                return Json(new
                                {
                                    success = true,
                                    url = "/Admin/ServicesCar/Index",
                                    message = message,
                                    data = new
                                    {
                                        ID = attributes.ID,
                                        Date = attributes.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                                        Name = attributes.Name,
                                        Value = attributes.Value,
                                        ServiceCarsID = attributes.ServiceCarsID,
                                        IsActive = attributes.IsActive.HasValue ? attributes.IsActive.Value.ToString() : bool.FalseString
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
                            error = ex.Message,
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Oops! Something went wrong. Please try later.",
                        error = "TempData Car ID not found ...",
                    });
                }

            }
            else if (id != null)//Edit Attribute
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        long Id;
                        if (TempData["CarID"] != null && Int64.TryParse(TempData["CarID"].ToString(), out Id) && carId == Id)
                        {
                            TempData.Keep("CarID");
                            ServiceCarAttribute attributes = _servicesCarService.GetServiceCarAttribute((long)id);
                            attributes.ID = id ?? 0;
                            attributes.Name = Name;
                            attributes.Value = Value;
                            attributes.ServiceCarsID = Id;

                            if (_servicesCarService.UpdateServiceCarAttribute(ref attributes, ref message, ref error, false))
                            {
                                return Json(new
                                {
                                    success = true,
                                    url = "/Admin/ServicesCar/Index",
                                    message = "Attribute updated successfully ...",
                                    data = new
                                    {
                                        ID = attributes.ID,
                                        Date = attributes.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                                        Name = attributes.Name,
                                        Value = attributes.Value,
                                        ServiceCarsID = attributes.ServiceCarsID,
                                        IsActive = attributes.IsActive.HasValue ? attributes.IsActive.Value.ToString() : bool.FalseString
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

            return Json(new
            {
                success = false,
                message = "Oops! Something went wrong. Please try later.",
                error = "Create and edit method not called ...",
            });

        }

        public ActionResult AttributeActivate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var attribute = _servicesCarService.GetServiceCarAttribute((long)id);
            if (attribute == null)
            {
                return HttpNotFound();
            }

            if (!(bool)attribute.IsActive)
                attribute.IsActive = true;
            else
            {
                attribute.IsActive = false;
            }
            string message = string.Empty;
            string error = string.Empty;
            if (_servicesCarService.UpdateServiceCarAttribute(ref attribute, ref message, ref error))
            {
                SuccessMessage = "Attribute " + ((bool)attribute.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        ID = attribute.ID,
                        Date = attribute.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        Name = attribute.Name,
                        Value = attribute.Value,
                        ServiceCarsID = attribute.ServiceCarsID,
                        IsActive = attribute.IsActive.HasValue ? attribute.IsActive.Value.ToString() : bool.FalseString
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage, error = error }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AttributeDelete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var attribute = _servicesCarService.GetServiceCarAttribute((long)id);
            if (attribute == null)
            {
                return HttpNotFound();
            }
            TempData["CarID"] = id;
            return View(attribute);
        }

        [HttpPost, ActionName("AttributeDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult AttributeDeleteConfirmed(long id)
        {
            string message = string.Empty;
            string error = string.Empty;

            if (_servicesCarService.DeleteServiceCarAttribute((Int16)id, ref message, ref error))
            {
                return Json(new
                {
                    success = true,
                    message = message,
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message, error = error }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}