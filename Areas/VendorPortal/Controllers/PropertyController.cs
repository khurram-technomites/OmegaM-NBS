using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.ViewModels.DataTables;
using System;
using LinqToExcel;
using System.Net;
using System.Web.Mvc;
using System.Linq;
using NowBuySell.Web.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Collections.Generic;
using NowBuySell.Web.Helpers.POCO;
using System.Drawing.Imaging;
using OfficeOpenXml;
using static NowBuySell.Web.Helpers.Enumerations.Enumeration;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.Util;
using System.IO;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    [AuthorizeVendor]
    public class PropertyController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPropertyCategoryService _PropCatService;
        private readonly IPropertyService _propService;
        private readonly INearByPlacesCategoryService _nearByPlacesCategoryService;
        private readonly INearByPlaceService _nearByPlaceService;
        private readonly IVendorService _vendorService;
        private readonly ICountryService _countryService;
        private readonly ICityService _cityService;
        private readonly IFeatureService _featureService;
        private readonly IPropertyFeaturesService _propfeatureService;
        private readonly IPropertyImagesService _propImageService;
        private readonly IPropertyFloorPlanService _floorPlanService;
        private readonly IVendorPackagesService _vendorPackagesService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;
        private readonly INumberRangeService _numberRangeService;
        private readonly IVendorAddPurchasesService _VendorAddPurchasesService;

        public PropertyController(IPropertyCategoryService PropCatService, IPropertyService propService, ICountryService countryService,
            ICityService cityService, IFeatureService featureService, IPropertyFeaturesService propfeatureService, IPropertyImagesService propImageService,
            IVendorPackagesService vendorPackagesService, IPropertyFloorPlanService floorPlanService, INotificationReceiverService notificationReceiverService, INotificationService notificationService, INearByPlacesCategoryService nearByPlacesCategoryService, INearByPlaceService nearByPlacesService,
            IVendorService vendorService, INumberRangeService numberRangeService, IVendorAddPurchasesService vendorAddPurchasesService)
        {
            _PropCatService = PropCatService;
            _propService = propService;
            _countryService = countryService;
            _vendorService = vendorService;
            _numberRangeService = numberRangeService;

            _cityService = cityService;
            _nearByPlacesCategoryService = nearByPlacesCategoryService;
            _nearByPlaceService = nearByPlacesService;
            _featureService = featureService;
            _propfeatureService = propfeatureService;
            _propImageService = propImageService;
            _floorPlanService = floorPlanService;
            _vendorPackagesService = vendorPackagesService;
            _notificationReceiverService = notificationReceiverService;
            _notificationService = notificationService;
            _VendorAddPurchasesService = vendorAddPurchasesService;
        }
        // GET: VendorPortal/Property
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult List()
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);

            var feature = _propService.GetApprovedListByVendor(vendorId).OrderByDescending(x => x.ID);
            return PartialView(feature);
        }
        //[HttpPost]
        //public ActionResult List(string Type)
        //{
        //    int vendorId = Convert.ToInt32(Session["VendorID"]);

        //    var feature = _propService.GetPropertiesByFilter(Type, vendorId).OrderByDescending(x => x.ID);
        //    return PartialView(feature);
        //}
        [HttpPost]
        public JsonResult List(DataTableAjaxPostModel model, string filter, int? vendorid = null)
        {
            var searchBy = (model.search != null) ? model.search.value : "";
            int sortBy = 0;
            string sortDir = "";

            if (model.order != null)
            {
                sortBy = model.order[0].column;
                sortDir = model.order[0].dir.ToLower();
            }
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            var Properties = _propService.GetVendorPropertiesVendor(model.length, model.start, sortBy, sortDir, searchBy, vendorId, true, filter);

            int filteredResultsCount = Properties != null && Properties != null && Properties.Count() > 0 ? (int)Properties.FirstOrDefault().FilteredResultsCount : 0;
            int totalResultsCount = Properties != null && Properties.Count() > 0 ? (int)Properties.FirstOrDefault().TotalResultsCount : 0;

            return Json(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = Properties
            });
        }

        public ActionResult Create()
        {
            BindDropdown();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Property Entity)
        {
            string message = string.Empty;
            Vendor vendor = _vendorService.GetVendor(Convert.ToInt32(Session["VendorID"]));
            string numberRange = _numberRangeService.GetNextValueFromNumberRangeByName("PROPERTY");
            /*int vendorId = Convert.ToInt32(Session["VendorID"]);*/
            long packageId = 0;

            if (vendor != null)
            {
                packageId = (long)vendor.VendorPackageID;
            }
            int Vendorlimit = _propService.GetVendorLimit(vendor.ID);
            var packagelimit = _vendorPackagesService.GetPackageLimit(packageId);
            DateTime currentDateTime = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
            int ExtraLimit = _VendorAddPurchasesService.GetVendorLimitProperty(currentDateTime, (int)vendor.ID);
            if (currentDateTime.Date < vendor.PackageEndDate.Value.Date)
            {
                if (packagelimit.PropertyLimit + ExtraLimit > Vendorlimit)
                {
                    Entity.VendorId = vendor.ID;
                    Entity.ApprovalStatusID = 1;
                    Entity.IsActive = true;
                    Entity.PropertyID = _numberRangeService.GetNextValueFromNumberRangeByName("PROPERTY");
                    Entity.AdsReferenceCode = _numberRangeService.GetNextValueFromNumberRangeByName("PROPERTYREFERENCE");
                    Entity.Slug = Slugify.GenerateSlug(Entity.Title + "-" + numberRange);
                    if (_propService.AddProperty(Entity, ref message))
                        /* return RedirectToAction(nameof(Edit), new { id = Entity.ID });*/
                        return Json(new
                        {
                            success = true,
                            message = message,
                            url = "/Vendor/Property/Edit/" + Entity.ID,

                        });

                    BindDropdown();

                }
                else
                {
                    BindDropdown();
                    /*                TempData["message"] = "Your Package has been exceeded";
                    */
                    message = "Your Ad limit has been exceeded.";
                }
                /* return RedirectToAction(nameof(Edit), new { id = Entity.ID });*/
            }
            else
            {
                BindDropdown();
                message = "Your package has been expired.";
            }
            return Json(new
            {
                success = false,
                messageswl = message,

            });
        }

        public ActionResult Edit(int Id)
        {


            Property model = _propService.GetById(Id);

            var List = _PropCatService.GetAll();

            var dropdownList = from item in List
                               select new { value = item.ID, text = item.CategoryName };

            ViewBag.CategoryId = new SelectList(dropdownList, "value", "text", model.CategoryId);

            ViewBag.CountryId = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text", model.CountryID);

            ViewBag.CityID = new SelectList(_cityService.GetCitiesForDropDown(), "value", "text", model.CityID);

            ViewBag.Features = _featureService.GetAllPropertyFeature();


            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Update(Property Entity)
        {

            string message = string.Empty;
            if (ModelState.IsValid)
            {
                var currentProperty = _propService.GetProperty(Entity.ID);


                currentProperty.ForSale = Entity.ForSale;
                currentProperty.CategoryId = Entity.CategoryId;

                currentProperty.Title = Entity.Title;
                currentProperty.TitleAr = Entity.TitleAr;

                currentProperty.NoOfRooms = Entity.NoOfRooms;
                currentProperty.NoOfBaths = Entity.NoOfBaths;
                currentProperty.NoOfDinings = Entity.NoOfDinings;
                currentProperty.NoOfLaundry = Entity.NoOfLaundry;
                currentProperty.NoOfGarage = Entity.NoOfGarage;
                currentProperty.IsFurnished = Entity.IsFurnished;

                currentProperty.BuildYear = Entity.BuildYear;
                currentProperty.Price = Entity.Price;
                currentProperty.Size = Entity.Size;

                currentProperty.Description = Entity.Description;
                currentProperty.DescriptionAr = Entity.DescriptionAr;

                currentProperty.IsPublished = Entity.IsPublished;

                if ((currentProperty.ApprovalStatusID == 3) || (currentProperty.ApprovalStatusID == 4))
                {
                    currentProperty.ApprovalStatusID = 2;
                }
                else
                {
                    currentProperty.ApprovalStatusID = 1;
                }

                currentProperty.Slug = currentProperty.Slug.Replace(Slugify.GenerateSlug(currentProperty.Title), Slugify.GenerateSlug(Entity.Title));
                currentProperty.ModifiedDate = Helpers.TimeZone.GetLocalDateTime();

                Entity = null;

                if (_propService.UpdateProperty(ref currentProperty, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        message = "Property updated successfully ...",
                    });
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }
            return Json(new { success = false, message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateLocation(Property Entity)
        {

            string message = string.Empty;
            if (ModelState.IsValid)
            {
                var currentProperty = _propService.GetProperty(Entity.ID);

                currentProperty.CountryID = Entity.CountryID;
                currentProperty.CityID = Entity.CityID;
                currentProperty.Area = Entity.Area;
                currentProperty.Address = Entity.Address;
                currentProperty.Latitude = Entity.Latitude;
                currentProperty.Longitude = Entity.Longitude;

                currentProperty.IsPublished = false;

                if ((currentProperty.ApprovalStatusID == 3) || (currentProperty.ApprovalStatusID == 4))
                {
                    currentProperty.ApprovalStatusID = 2;
                }
                else
                {
                    currentProperty.ApprovalStatusID = 1;
                }

                currentProperty.ModifiedDate = Helpers.TimeZone.GetLocalDateTime();

                Entity = null;

                if (_propService.UpdateProperty(ref currentProperty, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        message = "Property location updated successfully ...",
                    });
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }
            return Json(new { success = false, message = message });
        }

        public void BindDropdown()
        {
            var List = _PropCatService.GetAll();

            var dropdownList = from item in List
                               select new { value = item.ID, text = item.CategoryName };

            ViewBag.CategoryId = new SelectList(dropdownList, "value", "text");

            ViewBag.CountryId = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text");

            ViewBag.Features = _featureService.GetAllPropertyFeature();
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult GetCities([System.Web.Http.FromUri] int CountryId)
        {
            var cities = _cityService.GetCities(countryId: CountryId);

            var dropdownList = from item in cities
                               select new { value = item.ID, text = item.Name };

            return Json(dropdownList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AssignFeature([System.Web.Http.FromUri] int PropId, [System.Web.Http.FromUri] int FeatureId)
        {
            PropertyFeature model = new PropertyFeature();
            model.FeatureId = FeatureId;
            model.PropertyId = PropId;

            if (_propfeatureService.AddFeature(model))
                return Json(new
                {
                    message = "Feature Assign Successfully"
                });

            return Json(new
            {
                message = "There is a problem assigning this feature, please try again later."
            });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveFeature([System.Web.Http.FromUri] int PropId, [System.Web.Http.FromUri] int FeatureId)
        {
            PropertyFeature model = _propfeatureService.GetFeature(PropId, FeatureId);

            if (_propfeatureService.RemoveFeature(model))
                return Json(new
                {
                    message = "Feature Remove Successfully"
                });

            return Json(new
            {
                message = "There is a problem removing this feature, please try again later."
            });

        }

        [AllowAnonymous]
        public ActionResult Thumbnail(long? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Property prop = _propService.GetById((int)id);
                if (prop == null)
                {
                    return HttpNotFound();
                }
                string filePath = !string.IsNullOrEmpty(prop.Thumbnail) ? prop.Thumbnail : string.Empty;


                string message = string.Empty;

                string absolutePath = Server.MapPath("~");
                string relativePath = string.Format("/Assets/AppFiles/Images/Property/{0}/", prop.ID);

                prop.Thumbnail = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "Thumbnail", ref message, "Image", compress: true, ApplyWatermark: true);

                if (string.IsNullOrEmpty(prop.Remarks))
                    prop.ApprovalStatusID = 1;
                else
                    prop.ApprovalStatusID = 4;

                if (_propService.UpdateProperty(ref prop, ref message, false))
                {

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        System.IO.File.Delete(Server.MapPath(filePath));
                    }
                    return Json(new
                    {
                        success = true,
                        message = message,
                        data = prop.Thumbnail
                    });
                }
                return Json(new { success = false, message = message });
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

        public ActionResult Video(long? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Property prop = _propService.GetById((int)id.Value);
                if (prop == null)
                {
                    return HttpNotFound();
                }
                string filePath = !string.IsNullOrEmpty(prop.Video) ? prop.Video : string.Empty;


                string message = string.Empty;

                string absolutePath = Server.MapPath("~");
                string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/Video/", prop.ID);

                prop.Video = Uploader.UploadVideo(Request.Files, absolutePath, relativePath, "Video", ref message, "Video");
                prop.ApprovalStatusID = 1;
                if (_propService.UpdateProperty(ref prop, ref message, false))
                {
                    //_propService.UpdateApprovalStatus(prop.ID);

                    //if (!string.IsNullOrEmpty(filePath))
                    //{
                    //    System.IO.File.Delete(Server.MapPath(filePath));
                    //}
                    return Json(new
                    {
                        success = true,
                        message = message,
                        data = prop.Video
                    });
                }
                return Json(new { success = false, message = message });
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

        [AllowAnonymous]
        public ActionResult CreateImage(long? id, int count)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Property property = _propService.GetById((int)id);
                if (property == null)
                {
                    return HttpNotFound();
                }

                string message = string.Empty;

                string absolutePath = Server.MapPath("~");
                string relativePath = string.Format("/Assets/AppFiles/Images/Property/{0}/Gallery/", id);

                List<string> Pictures = new List<string>();

                Dictionary<long, string> data = new Dictionary<long, string>();
                Uploader.UploadImages(Request.Files, absolutePath, relativePath, "g", ref Pictures, ref message, "GalleryImages", ApplyWatermark: true);
                foreach (var item in Pictures)
                {
                    PropertyImage propImage = new PropertyImage();
                    propImage.PropertyId = (int)id;
                    propImage.Path = item;
                    if (_propImageService.Add(ref propImage, ref message))
                    {
                        data.Add(propImage.ID, item);
                    }
                }

                return Json(new
                {
                    success = true,
                    message = message,
                    data = data.ToList()
                });
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Json(new
                {
                    success = false,
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }
        [AllowAnonymous]
        public ActionResult CreateVideo(long? id, int count)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Property property = _propService.GetById((int)id);
                if (property == null)
                {
                    return HttpNotFound();
                }

                string message = string.Empty;

                string absolutePath = Server.MapPath("~");
                string relativePath = string.Format("/Assets/AppFiles/Videos/Property/{0}/Gallery/", id);

                List<string> Pictures = new List<string>();

                Dictionary<long, string> data = new Dictionary<long, string>();
                Uploader.UploadVideo(Request.Files, absolutePath, relativePath, "g", ref message, "GalleryVideos");
                foreach (var item in Pictures)
                {
                    PropertyImage propImage = new PropertyImage();
                    propImage.PropertyId = (int)id;
                    propImage.Path = item;
                    if (_propImageService.Add(ref propImage, ref message))
                    {
                        data.Add(propImage.ID, item);
                    }
                }

                return Json(new
                {
                    success = true,
                    message = message,
                    data = data.ToList()
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
        [AllowAnonymous]
        [HttpPost, ActionName("DeleteImage")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            string filePath = string.Empty;
            if (_propImageService.Remove(id, ref message, ref filePath))
            {
                System.IO.File.Delete(Server.MapPath(filePath));
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult GetPropertyImages(long id)
        {
            var carImages = _propImageService.GetImagesByProperty((int)id).Select(i => new
            {
                id = i.ID,
                Image = i.Path,
            }).ToList();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                carImages = carImages
            }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult CreateFloorImage(long? id, int count)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Property property = _propService.GetById((int)id);
                if (property == null)
                {
                    return HttpNotFound();
                }

                string message = string.Empty;

                string absolutePath = Server.MapPath("~");
                string relativePath = string.Format("/Assets/AppFiles/Images/Property/{0}/FloorPlan/", id);

                List<string> Pictures = new List<string>();

                Dictionary<long, string> data = new Dictionary<long, string>();
                Uploader.UploadImages(Request.Files, absolutePath, relativePath, "g", ref Pictures, ref message, "FloorImages", ApplyWatermark: true);
                foreach (var item in Pictures)
                {
                    PropertyFloorPlan propImage = new PropertyFloorPlan();
                    propImage.PropertyId = (int)id;
                    propImage.Path = item;
                    if (_floorPlanService.Add(ref propImage, ref message))
                    {
                        data.Add(propImage.ID, item);
                    }
                }

                return Json(new
                {
                    success = true,
                    message = message,
                    data = data.ToList()
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
        [AllowAnonymous]
        [HttpPost, ActionName("DeleteFloorImage")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteFloorImage(long id)
        {
            string message = string.Empty;
            string filePath = string.Empty;
            if (_floorPlanService.Remove(id, ref message, ref filePath))
            {
                System.IO.File.Delete(Server.MapPath(filePath));
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult GetPropertyFloor(long id)
        {
            var carImages = _floorPlanService.GetFloorPlansByProperty((int)id).Select(i => new
            {
                id = i.ID,
                Image = i.Path,
            }).ToList();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                carImages = carImages
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BulkPublish(List<long> ids)
        {

            try
            {
                List<string> ErrorItems = new List<string>();
                foreach (var items in ids)
                {
                    bool flag = false;
                    var property = _propService.GetById((int)items);
                    //ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> <br><br>", car.Name));
                    if (property.PropertyImages.Count < 1)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Images Are Not Inserted.<br>", property.Title));
                        flag = true;
                    }
                    if (property.PropertyFeatures.Count < 1)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Features Are Not Selected.<br>", property.Title));
                        flag = true;
                    }
                    if (property.Thumbnail == null)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Thumbnail Not Inserted.<br>", property.Title));
                        flag = true;
                    }
                    if (property.IsActive.HasValue)
                    {
                        if (!property.IsActive.Value)
                        {
                            ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Property Is Not Active.<br>", property.Title));
                            flag = true;
                        }
                    }
                    else
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Property Is Not Active.<br>", property.Title));
                        flag = true;
                    }
                    if (property.Price == null || property.Description == null || property.DescriptionAr == null || property.Size == null || property.NoOfBaths == null ||
                           property.NoOfRooms == null || property.CityID == null ||
                           property.Address == null || property.Latitude == null || property.Longitude == null)

                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Details Are Not Inserted Properly.<br>", property.Title));
                        flag = true;
                    }
                    if (!flag)
                    {
                        if (property != null && (property.IsActive.HasValue && property.IsActive.Value == true) && (property.ApprovalStatusID == 3))
                        {

                            property.IsPublished = true;
                            property.PublishDate = Helpers.TimeZone.GetLocalDateTime();

                            string message = string.Empty;
                            _propService.UpdateProperty(ref property, ref message, false);
                        }
                    }
                }
                if (ErrorItems.Count == 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Selected Properties are published."
                    }, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    TempData["ErrorMessage"] = string.Format("{0} Documents Missing!", ErrorItems.Count());
                    TempData["MissingDataErrorMessage"] = string.Join<string>("<br>", ErrorItems);

                    return Json(new { success = false, message = TempData["MissingDataErrorMessage"], error = TempData["ErrorMessage"] }, JsonRequestBehavior.AllowGet);
                }
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

        public ActionResult Publish(long? id)
        {
            List<string> ErrorItems = new List<string>();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var property = _propService.GetById((int)id);
            //ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> <br><br>", car.Name));
            if (!property.IsPublished)
            {
                if (property.PropertyImages.Count < 1)
                {
                    ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Images Are Not Inserted.<br>", property.Title));
                }
                if (property.PropertyFeatures.Count < 1)
                {
                    ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Features Are Not Selected.<br>", property.Title));
                }
                if (property.Thumbnail == null)
                {
                    ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Thumbnail Not Inserted.<br>", property.Title));
                }
                if (property.IsActive.HasValue && property.IsActive.Value == false)
                {

                    ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Property Is Not Active.<br>", property.Title));
                }
                if (property.ApprovalStatusID != 3)
                    ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Property Is Not Approved By Admin.<br>", property.Title));

                if (property.Price == null || property.Description == null || property.DescriptionAr == null || property.Size == null || property.NoOfBaths == null ||
                            property.NoOfRooms == null || property.CityID == null ||
                            property.Address == null || property.Latitude == null || property.Longitude == null)

                {
                    ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Details Are Not Inserted Properly.Plz Check Price, Description, DescriptionAr, Size, NoOfBaths, NoOfRooms, CityID, Address<br>", property.Title));
                }
            }
            if (ErrorItems.Count() < 1)
            {
                if (property.IsPublished)
                {
                    property.IsPublished = false;
                }
                else
                {
                    property.IsPublished = true;
                    property.PublishDate = Helpers.TimeZone.GetLocalDateTime();
                }
                    

                string message = string.Empty;
                if (_propService.UpdateProperty(ref property, ref message))
                {
                    SuccessMessage = "Property " + (property.IsPublished ? "Published" : "Unpublished") + "  successfully ...";
                    return Json(new
                    {
                        success = true,
                        message = SuccessMessage,
                        data = new
                        {
                            IsPublished = property.IsPublished == false ? property.IsPublished.ToString() : bool.FalseString,
                            ID = property.ID
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ErrorMessage = "Oops! Something went wrong. Please try later.";
                }
            }

            if (ErrorItems.Count() > 0)
            {
                TempData["ErrorMessage"] = string.Format("{0} Documents Missing!", ErrorItems.Count());
                TempData["MissingDataErrorMessage"] = string.Join<string>("<br>", ErrorItems);
            }
            return Json(new
            {
                success = false,
                message = TempData["MissingDataErrorMessage"],
                error = TempData["ErrorMessage"]
            }, JsonRequestBehavior.AllowGet);

            //return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PropertyActivation(long? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var property = _propService.GetById((int)id);

            if (property == null)
                return HttpNotFound();


            if (property.IsActive.HasValue)
            {
                if (property.IsActive.Value)
                    property.IsActive = false;
                else
                {
                    property.IsActive = true;
                    property.IsPublished = false;
                }
            }
            else
            {
                property.IsActive = true;
                property.IsPublished = false;
            }


            string message = string.Empty;

            if (_propService.UpdateProperty(ref property, ref message))
            {
                //var city = _cityService.GetCity((long)area.CityID);
                //var country = _countryService.GetCountry((long)area.CountryID);
                SuccessMessage = "Property " + ((bool)property.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        IsActive = property.IsActive.HasValue ? property.IsActive.Value.ToString() : bool.FalseString,
                        ID = property.ID
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
                ErrorMessage = "Oops! Something went wrong. Please try later.";

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(long id)
        {
            string message = string.Empty;
            if (_propService.RemoveProperty((int)id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ApprovalIndex()
        {
            return View();
        }

        public ActionResult ApprovalList()
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);

            var feature = _propService.GetListByVendor(vendorId).OrderByDescending(x => x.ID);
            return PartialView(feature);
        }
        [HttpPost]
        public JsonResult ApprovalList(DataTableAjaxPostModel model, string filter, int? vendorid = null)
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            var searchBy = (model.search != null) ? model.search.value : "";
            int sortBy = 0;
            string sortDir = "";

            if (model.order != null)
            {
                sortBy = model.order[0].column;
                sortDir = model.order[0].dir.ToLower();
            }
            filter = "All";
            var Properties = _propService.GetVendorPropertiesApproval(model.length, model.start, sortBy, sortDir, searchBy, vendorId, true, filter);

            int filteredResultsCount = Properties != null && Properties != null && Properties.Count() > 0 ? (int)Properties.FirstOrDefault().FilteredResultsCount : 0;
            int totalResultsCount = Properties != null && Properties.Count() > 0 ? (int)Properties.FirstOrDefault().TotalResultsCount : 0;

            return Json(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = Properties
            });
        }

        [HttpPost]
        public ActionResult ToggleIsSold(int ID)
        {
            string featuremsg = string.Empty;
            var prop = _propService.GetById(ID);
            if (prop == null)
            {
                return HttpNotFound();
            }

            if (prop.IsSold.HasValue)
            {
                if (prop.IsSold.Value)
                {
                    prop.IsSold = false;
                    featuremsg = "Property set to un-sold successfully...";
                }
                else
                {
                    prop.SoldDate = NowBuySell.Service.Helpers.TimeZone.GetLocalDateTime();
                    prop.IsSold = true;
                    featuremsg = "Property sold successfully...";
                }
            }
            else
            {
                prop.SoldDate = NowBuySell.Service.Helpers.TimeZone.GetLocalDateTime();
                prop.IsSold = true;
                featuremsg = "Property sold successfully...";
            }

            string message = string.Empty;

            if (_propService.UpdateProperty(ref prop, ref message, false))
            {
                return Json(new { success = true, message = featuremsg }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IsApproved(long? id)
        {
            List<string> ErrorItems = new List<string>();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var property = _propService.GetById((int)id);
            //ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> <br><br>", car.Name));
            if (property.PropertyImages.Count < 1)
            {
                ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Images Are Not Inserted.<br>", property.Title));
            }
            if (property.PropertyFeatures.Count < 1)
            {
                ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Features Are Not Selected.<br>", property.Title));
            }
            if (property.Thumbnail == null)
            {
                ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Thumbnail Not Inserted.<br>", property.Title));
            }

            if (property.Price == null || property.Description == null || property.DescriptionAr == null || property.Size == null || property.NoOfBaths == null ||
                            property.NoOfRooms == null || property.CityID == null ||
                            property.Address == null || property.Latitude == null || property.Longitude == null)

            {
                ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Details Are Not Inserted Properly.<br>", property.Title));
            }


            //if (ca == null)
            //{
            //	ErrorItems.Add(string.Format("Car Images Are Not Inserted.<br>"));
            //}
            if (ErrorItems.Count() < 1)
            {
                property.ApprovalStatusID = 2;
                property.Remarks = "";
                string message = string.Empty;
                if (_propService.UpdateProperty(ref property, ref message))
                {
                    Notification not = new Notification();
                    not.Title = "Property Approval";
                    not.Description = "New prodouct added for approval ";
                    not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                    not.OriginatorName = Session["VendorUserName"].ToString();
                    not.Url = "/Admin/Property/Approvals";
                    not.Module = "Property";
                    not.OriginatorType = "Vendor";
                    not.RecordID = property.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", not.OriginatorID))
                        {
                        }
                    }
                    //SuccessMessage =  /*"Car " + (car.IsApproved.Value ? "Approved" : "disapproved") + "  successfully ..."*/;
                    SuccessMessage = "Property approval request sent successfully... ";

                    return Json(new
                    {
                        success = true,
                        message = SuccessMessage,
                        data = new
                        {
                            IsApproved = property.ApprovalStatusID,
                            ID = property.ID
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ErrorMessage = "Oops! Something went wrong. Please try later.";
                }
            }

            if (ErrorItems.Count() > 0)
            {
                TempData["ErrorMessage"] = string.Format("{0} Documents Missing!", ErrorItems.Count());
                TempData["MissingDataErrorMessage"] = string.Join<string>("<br>", ErrorItems);
            }
            return Json(new { success = false, message = TempData["MissingDataErrorMessage"], error = TempData["ErrorMessage"] }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CancelApproval(long id)
        {
            var property = _propService.GetById((int)id);
            if (property == null)
            {
                return HttpNotFound();
            }

            if (property.Remarks != null)
                property.ApprovalStatusID = 4;
            else if (property.Remarks == null)
                property.ApprovalStatusID = 1;

            string message = string.Empty;
            if (_propService.UpdateProperty(ref property, ref message))
            {
                SuccessMessage = "Property approval request canceled successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        IsApproved = property.ApprovalStatusID,
                        ID = property.ID
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BulkSendForApproval(List<long> ids)
        {
            try
            {
                List<string> ErrorItems = new List<string>();
                foreach (var items in ids)
                {
                    bool flag = false;
                    var property = _propService.GetById((int)items);
                    //ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> <br><br>", car.Name));
                    if (property.PropertyImages.Count < 1)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Images Are Not Inserted.<br>", property.Title));
                        flag = true;
                    }
                    if (property.PropertyFeatures.Count < 1)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Features Are Not Selected.<br>", property.Title));
                        flag = true;
                    }
                    if (property.Thumbnail == null)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Thumbnail Not Inserted.<br>", property.Title));
                        flag = true;
                    }
                    if (property.Price == null || property.Description == null || property.DescriptionAr == null || property.Size == null || property.NoOfBaths == null ||
                           property.NoOfRooms == null || property.CityID == null ||
                           property.Address == null || property.Latitude == null || property.Longitude == null)

                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Details Are Not Inserted Properly.<br>", property.Title));
                        flag = true;
                    }


                    if (!flag)
                    {
                        //ErrorItems.Add(string.Format("Sent successfully<br>"));
                        if (property != null && property.ApprovalStatusID != 3)
                        {
                            property.ApprovalStatusID = 2;
                            property.Remarks = "";
                            string message = string.Empty;

                            if (_propService.UpdateProperty(ref property, ref message, false))
                            {
                                Notification not = new Notification();
                                not.Title = "Property Approval";
                                not.TitleAr = "الموافقة على المنتج";
                                not.Description = "New prodouct added for approval ";
                                not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                                not.OriginatorName = Session["VendorUserName"].ToString();
                                not.Url = "/Admin/Property/Approvals";
                                not.Module = "Property";
                                not.OriginatorType = "Vendor";
                                not.RecordID = property.ID;
                                if (_notificationService.CreateNotification(not, ref message))
                                {
                                    _notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", not.OriginatorID);
                                }
                            }
                        }
                    }
                }

                if (ErrorItems.Count == 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Selected Properties are sent for approval."
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["ErrorMessage"] = string.Format("{0} Documents Missing!", ErrorItems.Count());
                    TempData["MissingDataErrorMessage"] = string.Join<string>("<br>", ErrorItems);

                    return Json(new { success = false, message = TempData["MissingDataErrorMessage"], error = TempData["ErrorMessage"] }, JsonRequestBehavior.AllowGet);
                }

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

        public ActionResult Remarks(long id)
        {
            ViewBag.BuildingID = id;
            var property = _propService.GetById((int)id);


            return View(property);
        }

        public ActionResult AddFeature()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddFeature(string Name, string NameAr, long CarID)
        {
            Feature feature = new Feature();
            feature.Name = Name;
            feature.NameAR = NameAr;
            feature.Image = "~/Assets/AppFiles/Images/default.png";
            feature.Category = "Property";
            string message = string.Empty;
            feature.VendorID = (long)Session["VendorID"];
            //carmake.IsApproved = false;
            if (_featureService.CreateFeature(feature, ref message))
            {
                var data = _vendorService.GetVendor((long)feature.VendorID);
                Notification not = new Notification();
                not.Title = "Property Feature";
                not.TitleAr = "Property Feature";
                not.Description = string.Format("New Property Feature has been added by {0}", feature.Name);
                not.DescriptionAr = string.Format("New Property Feature has been added by {0}", feature.Name);
                not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                not.OriginatorName = Session["VendorUserName"].ToString();
                not.Module = "CarFeature";
                not.Url = "/Admin/PropertyFeature/Index";
                not.OriginatorType = "Vendor";
                not.RecordID = feature.ID;
                if (_notificationService.CreateNotification(not, ref message))
                {
                    NotificationReceiver notRec = new NotificationReceiver();
                    //notRec.ReceiverID = order.CustomerID;
                    //notRec.ReceiverType = "Customer";
                    //notRec.NotificationID = not.ID;
                    if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", not.OriginatorID))
                    {

                    }

                }

                return Json(new
                {
                    success = true,
                    id = feature.ID,
                    name = feature.Name,
                    //FeatureID = carFeature.ID,
                    message = "Property Feature request sent!",

                });
            }


            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PropertiesReport()
        {
            var vendorId = Convert.ToInt64(Session["VendorID"]);
            string ImageServer = CustomURL.GetImageServer();
            var getAllProperties = _propService.GetApprovedListByVendor(vendorId).ToList();
            var getAllPropertiesVariations = _propService.GetApprovedListByVendor(vendorId).ToList();
            if (getAllProperties.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("Property");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Title"
                        ,"TitleAr"
                        ,"Slug"
                        ,"Price"
                        ,"SellerTransactionFee"
                        ,"BuyerTransactionFee"
                        ,"Description"
                        ,"DescriptionAr"
                        ,"Size"
                        ,"NoOfGarage"
                        ,"NoOfRooms"
                        ,"NoOfBaths"
                        ,"NoOfBaths"
                        ,"NoOfDinnings"
                        ,"NoOfLaundry"
                        ,"BuildYear"
                        ,"Category"

                        ,"Video"

                        ,"Status"
                       /* ,"Remarks"*/
                        
                        ,"City"
                        ,"Country"
                        ,"Area"
                        ,"State"
                        ,"ZipCode"
                        ,"Address"
                        ,"Latitude"
                        ,"Longitude"
                        ,"Thumbnail"
                        ,"OldPrice"

                        }
                    };
                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:AX1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["Property"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllProperties)
                    {

                        cellData.Add(new object[]
                        {

                         !string.IsNullOrEmpty(i.Title) ? i.Title : "-"
                        ,!string.IsNullOrEmpty(i.TitleAr) ? i.TitleAr : "-"
                        ,!string.IsNullOrEmpty(i.Slug) ? i.Slug : "-"
                        ,i.Price != null ? 0 : i.Price
                        ,i.SellerTransactionFee != null ? 0 : i.SellerTransactionFee
                        ,i.BuyerTransactionFee != null ? 0 : i.BuyerTransactionFee
                         ,i.Description != null ? (!string.IsNullOrEmpty(i.Description.ToString()) ? i.Description.ToString() : "-") : "-"
                        ,i.DescriptionAr != null ? (!string.IsNullOrEmpty(i.DescriptionAr.ToString()) ? i.DescriptionAr.ToString() : "-") : "-"
                        ,i.Size !=  null ? 0 : i.Size
                        ,i.NoOfGarage !=  null ? 0 : i.NoOfGarage
                        ,i.NoOfRooms !=  null ? 0 : i.NoOfRooms
                        ,i.NoOfBaths !=  null ? 0 : i.NoOfBaths
                        ,i.NoOfDinings !=  null ? 0 : i.NoOfDinings
                        ,i.NoOfLaundry !=  null ? 0 : i.NoOfLaundry
                        ,i.BuildYear != null ? (!string.IsNullOrEmpty(i.BuildYear.ToString()) ? i.BuildYear.ToString() : "-") : "-"
                        ,i.Category !=  null ? (!string.IsNullOrEmpty(i.Category.CategoryName) ? i.Category.CategoryName : "-") : "-"
                        ,i.Video != null ? (!string.IsNullOrEmpty(i.Video) ? i.Video : "-") : "-"
                        ,i.ForSale != true ? "For Rent" : "For Sell"
                        /*,i.Remarks != null ? ( !string.IsNullOrEmpty(i.Remarks) ? i.Remarks : "-") : "-"*/
                        ,i.City != null ? (!string.IsNullOrEmpty(i.City.Name) ? i.City.Name : "-") : "-"
                        ,i.Country != null ? (!string.IsNullOrEmpty(i.Country.Name) ? i.Country.Name : "-") : "-"
                        ,i.Area != null ? (!string.IsNullOrEmpty(i.Area) ? i.Area : "-") : "-"
                        ,i.State != null ? (!string.IsNullOrEmpty(i.State) ? i.State : "-") : "-"
                        ,i.ZipCode != null ? (!string.IsNullOrEmpty(i.ZipCode) ? i.ZipCode : "-") : "-"
                        ,i.Address != null ? (!string.IsNullOrEmpty(i.Address) ? i.Address : "-") : "-"
                        ,i.Latitude != null ? (!string.IsNullOrEmpty(i.Latitude) ? i.Latitude : "-") : "-"
                        ,i.Longitude != null ? (!string.IsNullOrEmpty(i.Longitude) ? i.Longitude : "-") : "-"
                        ,i.Thumbnail != null ? (!string.IsNullOrEmpty(i.Thumbnail) ? i.Thumbnail : "-") : "-"
                        ,i.OldPrice != null ? 0 : i.OldPrice





                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Property Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            BindDropdown();
            var prop = _propService.GetById(id);

            return View(prop);
        }
        public ActionResult BulkUpload()
        {
            //ViewBag.ShopID = new SelectList(_shopService.DropDownForShop(), "value", "text");
            return View();
        }

        [HttpPost]
        public ActionResult BulkUpload(HttpPostedFileBase FileUpload, string connectionId)
        {
            try
            {
                // long shopID = Convert.ToInt64(ShopID);

                if (FileUpload != null)
                {
                    if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        string filename = FileUpload.FileName;

                        if (filename.EndsWith(".xlsx"))
                        {
                            string targetpath = Server.MapPath("~/assets/AppFiles/Documents/ExcelFiles");
                            FileUpload.SaveAs(targetpath + filename);
                            string pathToExcelFile = targetpath + filename;

                            string sheetName = "BulkProperty";
                            long VendorID = (long)Session["VendorID"];


                            string data = "";
                            string message = "";
                            List<string> ErrorItems = new List<string>();
                            int count = 0;
                            var total = 0;
                            int successCount = 0;

                            var excelFile = new ExcelQueryFactory(pathToExcelFile);
                            IList<PropertyWorkSheet> Product = (from a in excelFile.Worksheet<PropertyWorkSheet>(sheetName) select a).ToList();
                            total = Product.Count();

                            if (total > 500)
                            {
                                excelFile.Dispose();
                                if (System.IO.File.Exists(pathToExcelFile))
                                {
                                    System.IO.File.Delete(pathToExcelFile);
                                }

                                return Json(new
                                {
                                    success = false,
                                    message = "Only 500 properties are allowed !"
                                }, JsonRequestBehavior.AllowGet);
                            }
                            Vendor vendor = _vendorService.GetVendor(VendorID);
                            long packageId=0;
                            if (vendor != null)
                            {
                                packageId = (long)vendor.VendorPackageID;
                            }
                            int Vendorlimit = _propService.GetVendorLimit(vendor.ID);
                            var packagelimit = _vendorPackagesService.GetPackageLimit(packageId);
                            DateTime currentDateTime = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
                            int ExtraLimit = _VendorAddPurchasesService.GetVendorLimitProperty(currentDateTime, (int)vendor.ID);
                            if (currentDateTime.Date < vendor.PackageEndDate.Value.Date)
                            {
                                if (packagelimit.PropertyLimit + ExtraLimit < Vendorlimit+total)
                                {
                                    return Json(new
                                    {
                                        success = false,
                                        message = "Your Ad Limit has been Exceeded!"
                                    }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            else
                            {
                                return Json(new
                                {
                                    success = false,
                                    message = "Your package has been expired !"
                                }, JsonRequestBehavior.AllowGet);
                            }

                            foreach (PropertyWorkSheet item in Product)
                            {
                                try
                                {
                                    var results = new List<ValidationResult>();
                                    var context = new ValidationContext(item, null, null);
                                    string numberRange = _numberRangeService.GetNextValueFromNumberRangeByName("PROPERTY");
                                    //item.AdsReferenceCode = _numberRangeService.GetNextValueFromNumberRangeByName("PROPERTYREFERENCE");
                                    item.Slug = Slugify.GenerateSlug(item.Title + "-" + numberRange);
                                    if (Validator.TryValidateObject(item, context, results))
                                    {
                                        //if (string.IsNullOrEmpty(item.VariantSKU))
                                        //{
                                        /* IF PRODUCT IS SIMPLE*/

                                        /*Upload Product Thumnail*/
                                        Property prop = _propService.GetByReferenceCode(item.AdsReferenceCode, VendorID);
                                        if (prop == null)
                                        {
                                            item.AdsReferenceCode = _numberRangeService.GetNextValueFromNumberRangeByName("PROPERTYREFERENCE");
                                        }
                                        string filePath = string.Empty;
                                        if (!string.IsNullOrEmpty(item.Thumbnail))
                                        {
                                            filePath = item.Thumbnail;

                                            message = string.Empty;

                                            string absolutePath = Server.MapPath("~");
                                            string relativePath = string.Format("/Assets/AppFiles/Images/Product/{0}/", item.AdsReferenceCode.Replace(" ", "_"));

                                            item.Thumbnail = Uploader.SaveImage(item.Thumbnail, absolutePath, relativePath, "PTI", ImageFormat.Jpeg, true);

                                        }
                                        if (!string.IsNullOrEmpty(item.Features))
                                        {
                                            item.Features = item.Features.Replace("&", "&amp;");
                                        }
                                        if (!string.IsNullOrEmpty(item.Images))
                                        {
                                            item.Images = item.Images.Replace("&", "&amp;");
                                        }
                                        if (!string.IsNullOrEmpty(item.FloorPlan))
                                        {
                                            item.FloorPlan = item.FloorPlan.Replace("&", "&amp;");
                                        }
                                        if (!string.IsNullOrEmpty(item.Description))
                                        {
                                            item.Description = item.Description.Replace("`", "'");
                                        }

                                        //if (!string.IsNullOrEmpty(item.ProductSports))
                                        //{
                                        //    item.ProductSports = item.ProductSports.Replace("&", "&amp;");
                                        //}

                                        if (_propService.PostExcelData(VendorID
                                                                            , item.Slug
                                                                            , item.Title
                                                                            , item.TitleAr
                                                                            , item.Price
                                                                            , item.Size
                                                                            , item.ForSale == "Sale" ? true : false
                                                                            , item.NoOfGarage
                                                                            , item.NoOfRooms
                                                                            , item.NoOfBaths
                                                                            , item.NoOfDinings
                                                                            , item.NoOfLanudry
                                                                            , item.BuildYear
                                                                            , item.Furnished == "Yes" ? true : false
                                                                            , item.Description
                                                                            , item.DescriptionAr
                                                                            , item.Category
                                                                            , item.City
                                                                            , item.Thumbnail
                                                                            , item.Features
                                                                            , item.Address
                                                                            , item.AdsReferenceCode
                                                                            , "New"
                                                                            , Convert.ToDecimal(item.Latitude)
                                                                            , Convert.ToDecimal(item.Longitude)
                                                                            , item.Area
                                                                            ))
                                        {
                                            Property product = _propService.GetByReferenceCode(item.AdsReferenceCode, VendorID);


                                            /*Uploading Product Images*/
                                            if (!string.IsNullOrEmpty(item.Images))
                                            {
                                                message = string.Empty;
                                                string absolutePath = Server.MapPath("~");
                                                string relativePath = string.Format("/Assets/AppFiles/Images/Property/{0}/Gallery/", product.AdsReferenceCode.Replace(" ", "_"));

                                                List<string> Pictures = new List<string>();

                                                Uploader.SaveImages(item.Images.Split(','), absolutePath, relativePath, "PGI", ImageFormat.Jpeg, ref Pictures);

                                                var imageCount = 0;
                                                foreach (var image in Pictures)
                                                {
                                                    PropertyImage productImage = new PropertyImage();
                                                    productImage.PropertyId = product.ID;
                                                    productImage.Path = image;
                                                    if (!_propImageService.Add(ref productImage, ref message))
                                                    {
                                                    }
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(item.Video))
                                            {

                                                string absolutePath = Server.MapPath("~");
                                                string relativePath = string.Format("/Assets/AppFiles/Images/Property/{0}/Gallery/", product.ID.ToString().Replace(" ", "_"));
                                                message = string.Empty;
                                                string Pictures = "";
                                                Uploader.SaveVideos(item.Video, absolutePath, relativePath, "PGI", ref Pictures, "Video");
                                                var imageCount = 0;
                                                product.Video = Pictures;
                                                _propService.UpdateProperty(ref product, ref message);

                                            }

                                            if (!string.IsNullOrEmpty(item.FloorPlan))
                                            {
                                                message = string.Empty;
                                                string absolutePath = Server.MapPath("~");
                                                string relativePath = string.Format("/Assets/AppFiles/Images/Property/{0}/Gallery/", product.AdsReferenceCode.Replace(" ", "_"));

                                                List<string> Pictures = new List<string>();

                                                Uploader.SaveImages(item.FloorPlan.Split(','), absolutePath, relativePath, "PGI", ImageFormat.Jpeg, ref Pictures);

                                                var imageCount = 0;
                                                foreach (var image in Pictures)
                                                {
                                                    PropertyFloorPlan productImage = new PropertyFloorPlan();
                                                    productImage.PropertyId = product.ID;
                                                    productImage.Path = image;
                                                    if (!_floorPlanService.Add(ref productImage, ref message))
                                                    {
                                                    }
                                                }
                                            }

                                            product = null;
                                            successCount++;
                                        }
                                        else
                                        {
                                            ErrorItems.Add(string.Format("Row Number {0} Not Inserted.<br>", count));

                                            if (!string.IsNullOrEmpty(item.Thumbnail))
                                            {
                                                System.IO.File.Delete(Server.MapPath(item.Thumbnail));
                                            }
                                        }
                                        //}
                                        //else
                                        //{
                                        //    /* IF Product is VARIATION*/
                                        //    Product product = _productService.GetProductbySKU(item.SKU, VendorID);
                                        //    if (product != null)
                                        //    {
                                        //        if (item.Type == "Variable" && item.VariantSKU != null)
                                        //        {
                                        //            /*Upload Product Variation Thumnail*/
                                        //            string filePath = string.Empty;
                                        //            if (!string.IsNullOrEmpty(item.VariantThumbnail))
                                        //            {
                                        //                filePath = item.VariantThumbnail;

                                        //                message = string.Empty;

                                        //                string absolutePath = Server.MapPath("~");
                                        //                string relativePath = string.Format("/Assets/AppFiles/Images/Product/{0}/Variations/{1}/", product.SKU.Replace(" ", "_"), item.VariantSKU.Replace(" ", "_"));

                                        //                item.VariantThumbnail = Uploader.SaveImage(item.VariantThumbnail, absolutePath, relativePath, "PVTI", ImageFormat.Jpeg, true);

                                        //            }

                                        //            _productVariationService.PostExcelData(product.ID, item.VariantRegularPrice, item.VariantSalePrice, item.VariantSalePriceFrom, item.VariantSalePriceTo, item.VariantStock, item.VariantThreshold, item.VariantStockStatus == "In Stock" ? 1 : 2, item.VariantSKU, item.VariantThumbnail, item.VariantWeight, item.VariantLength, item.VariantWidth, item.VariantHeight, item.VariantDescription, item.VariantSoldIndividually == "Yes" ? true : false, item.VariantIsManageStock == "Yes" ? true : false);

                                        //        }

                                        //        ProductVariation objProductVariation = _productVariationService.GetProductbySKU(item.VariantSKU);

                                        //        /*Creating Product Variation Attributes Images*/
                                        //        if (item.VariantAttributes != null)
                                        //        {
                                        //            string[] values = item.VariantAttributes.Split(',');
                                        //            for (int i = 0; i < values.Count(); i++)
                                        //            {
                                        //                string[] val1 = values[i].Split(':');
                                        //                if (val1 != null && val1.Count() > 1)
                                        //                {
                                        //                    var result = _productVariationAttributeService.PostExcelData(product.ID, val1[0], val1[1], objProductVariation.ID);
                                        //                }
                                        //            }
                                        //        }

                                        //        /*Uploading Product Variation Images*/
                                        //        if (!string.IsNullOrEmpty(item.VariantImages))
                                        //        {
                                        //            message = string.Empty;
                                        //            string absolutePath = Server.MapPath("~");

                                        //            string relativePath = string.Format("/Assets/AppFiles/Images/Product/{0}/Variations/{1}/Gallery/", product.SKU.Replace(" ", "_"), item.VariantSKU.Replace(" ", "_"));

                                        //            List<string> Pictures = new List<string>();

                                        //            Uploader.SaveImages(item.VariantImages.Split(','), absolutePath, relativePath, "PVGI", ImageFormat.Jpeg, ref Pictures, true);
                                        //            var imageCount = 0;
                                        //            foreach (var image in Pictures)
                                        //            {
                                        //                ProductVariationImage productVariationImage = new ProductVariationImage();
                                        //                productVariationImage.ProductID = product.ID;
                                        //                productVariationImage.ProductVariationID = objProductVariation.ID;
                                        //                productVariationImage.Image = image;
                                        //                productVariationImage.Position = ++imageCount;
                                        //                if (_productVariationImageService.CreateProductVariationImage(ref productVariationImage, ref message))
                                        //                {
                                        //                }
                                        //            }
                                        //        }

                                        //        objProductVariation = null;
                                        //        successCount++;
                                        //    }
                                        //    else
                                        //    {
                                        //        ErrorItems.Add(string.Format("<b>Row Number {0} Not Inserted:</b><br>Product not found, Please add product row first without variant", count));
                                        //    }
                                        //}
                                    }
                                    else
                                    {
                                        ErrorItems.Add(string.Format("<b>Row Number {0} Not Inserted:</b><br>{1}",
                                            count,
                                            string.Join<string>("<br>", results.Select(i => i.ErrorMessage).ToList())));
                                    }
                                    count++;
                                }
                                catch (Exception ex)
                                {
                                    ErrorItems.Add(string.Format("<b>Row Number {0} Not Inserted:</b><br>Internal Server Serror", count));
                                }

                                //CALLING A FUNCTION THAT CALCULATES PERCENTAGE AND SENDS THE DATA TO THE CLIENT
                                Functions.SendProgress(connectionId, "Upload in progress...", count, total);
                            }

                            excelFile.Dispose();
                            if (System.IO.File.Exists(pathToExcelFile))
                            {
                                System.IO.File.Delete(pathToExcelFile);
                            }

                            return Json(new
                            {
                                success = true,
                                successMessage = string.Format("{0} Properties uploaded!", (successCount)),
                                errorMessage = (ErrorItems.Count() > 0) ? string.Format("{0} Products are not uploaded!", total - successCount) : null,
                                detailedErrorMessages = (ErrorItems.Count() > 0) ? string.Join<string>("<br>", ErrorItems) : null,
                            }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Invalid file format, Only .xlsx format is allowed"
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Invalid file format, Only Excel file is allowed"
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {

                    return Json(new
                    {
                        success = false,
                        message = "Please upload Excel file first"
                    }, JsonRequestBehavior.AllowGet);
                }

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
        public ActionResult CompletedOrdersReport()
        {
            //DateTime EndDate = ToDate.AddMinutes(1439);
            //var VendorID = Convert.ToInt64(Session["VendorID"]);
            // var getAllOrders = _orderService.GetVendorOrderscompDateWise(VendorID, fromDate, ToDate);
            //if (getAllOrders.Count() > 0)
            //{
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Worksheets.Add("BulkProperty");

                var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Category"
                        ,"Title"
                        ,"TitleAr"

                        }
                    };

                // Determine the header range (e.g. A1:D1)
                string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                // Target a worksheet
                var worksheet = excel.Workbook.Worksheets["BulkProperty"];

                // Popular header row data
                worksheet.Cells[headerRange].LoadFromArrays(headerRow);


                //  var dv2 = ws.Cell("A4").DataValidation;
                //var options = new List<string> { "Option1", "Option2", "Option3" };
                //var validOptions = $"\"{String.Join(",", options)}\"";
                //worksheet.Cells[1, 1].DataValidation.(validOptions, true);
                //worksheet.Column[3].DataValidation.List(validOptions, true);
                //var cellData = new List<object[]>();



                //worksheet.Cells[1, 1].LoadFromArrays(validOptions);

                return File(excel.GetAsByteArray(), "application/msexcel", "BulkProperty.xlsx");
            }
            //}
            //return RedirectToAction("CompletedOrders");
        }
        public ActionResult MoveFloorImage(string sourcePath,int id)
        {

            try
            {
               
                string relativePath = string.Format("/Assets/AppFiles/Images/Property/{0}/FloorPlan/{1}", id, Path.GetFileName(sourcePath));
                bool exists = System.IO.Directory.Exists(Server.MapPath(string.Format("/Assets/AppFiles/Images/Property/{0}/FloorPlan/", id)));

                if (!exists)
                    System.IO.Directory.CreateDirectory(Server.MapPath(string.Format("/Assets/AppFiles/Images/Property/{0}/FloorPlan/", id)));
                System.IO.File.Copy(Server.MapPath(sourcePath), Server.MapPath(relativePath));
                if (id == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Property property = _propService.GetById((int)id);
                if (property == null)
                {
                    return HttpNotFound();
                }

                string message = string.Empty;


                List<string> Pictures = new List<string>();


                PropertyFloorPlan propImage = new PropertyFloorPlan();
                propImage.PropertyId = (int)id;
                propImage.Path = relativePath;
                if (_floorPlanService.Add(ref propImage, ref message))
                {
                    return Json(new
                    {
                        success = true,
                    });
                }
                return Json(new
                {
                    success = false,
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                });
            }
          


              
         
           
        }
        
    }
}