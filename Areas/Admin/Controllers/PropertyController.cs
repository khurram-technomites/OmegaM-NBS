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
using NowBuySell.Web.Helpers.PushNotification;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class PropertyController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IPropertyCategoryService _PropCatService;
        private readonly IPropertyService _propService;
        private readonly IVendorService _vendorService;
        private readonly ICountryService _countryService;
        private readonly ICityService _cityService;
        private readonly IFeatureService _featureService;
        private readonly IPropertyFeaturesService _propfeatureService;
        private readonly IPropertyImagesService _propImageService;
        private readonly IPropertyFloorPlanService _floorPlanService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;
        private readonly IVendorSessionService _vendorSessionService;
        private readonly INumberRangeService _numberRangeService;
        private readonly IVendorPackagesService _vendorPackagesService;
        private readonly IVendorAddPurchasesService _VendorAddPurchasesService;


        public PropertyController(IPropertyCategoryService PropCatService, IPropertyService propService, ICountryService countryService,
            ICityService cityService, IFeatureService featureService, IPropertyFeaturesService propfeatureService, IPropertyImagesService propImageService,
            IPropertyFloorPlanService floorPlanService, INotificationReceiverService notificationReceiverService, INotificationService notificationService,
            IVendorService vendorService, IVendorSessionService vendorSessionService, INumberRangeService numberRangeService,IVendorPackagesService packagesService, IVendorAddPurchasesService vendorAddPurchasesService)
        {
            _PropCatService = PropCatService;
            _propService = propService;
            _countryService = countryService;

            _cityService = cityService;
            _featureService = featureService;
            _propfeatureService = propfeatureService;
            _propImageService = propImageService;
            _floorPlanService = floorPlanService;
            _notificationReceiverService = notificationReceiverService;
            _notificationService = notificationService;
            _vendorService = vendorService;
            _vendorSessionService = vendorSessionService;
            _numberRangeService = numberRangeService;
            _vendorPackagesService = packagesService;
            _VendorAddPurchasesService = vendorAddPurchasesService;
        }
        // GET: Admin/Property
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Approvals()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
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

                prop.Thumbnail = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "Thumbnail", ref message, "Image", ApplyWatermark: true);

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
        public ActionResult ApprovalsList()
        {
            var properties = _propService.GetForApproval(0).OrderByDescending(x => x.ID);
            var List = _vendorService.GetVendorsForProperties(true);
            var dropdownList = (from item in List.AsEnumerable()
                                select new SelectListItem
                                {
                                    Text = item.Name,
                                    Value = item.ID.ToString()
                                }).ToList();
            //dropdownList.Add(new SelectListItem() { Text = "Select Vendor", Value = "0" });

            ViewBag.Vendor = new SelectList(dropdownList, "Value", "Text");
            return PartialView(properties);
        }
        //[HttpPost]
        //public ActionResult ApprovalsList(long VendorId)
        //{
        //    var properties = _propService.GetForApproval(VendorId).OrderByDescending(x => x.ID);
        //    var List = _vendorService.GetVendorsForProperties(true);
        //    var dropdownList = (from item in List.AsEnumerable()
        //                        select new SelectListItem
        //                        {
        //                            Text = item.Name,
        //                            Value = item.ID.ToString()
        //                        }).ToList();
        //    //dropdownList.Add(new SelectListItem() { Text = "Select Vendor", Value = "0" });

        //    ViewBag.Vendor = new SelectList(dropdownList, "Value", "Text");
        //    return PartialView(properties);
        //}
        [HttpPost]
        public JsonResult ApprovalsList(DataTableAjaxPostModel model, string filter, int? vendorid = null)
        {
            var searchBy = (model.search != null) ? model.search.value : "";
            int sortBy = 0;
            string sortDir = "";

            if (model.order != null)
            {
                sortBy = model.order[0].column;
                sortDir = model.order[0].dir.ToLower();
            }
            filter = "All";
            var Properties = _propService.GetPropertiesApproval(model.length, model.start, sortBy, sortDir, searchBy, vendorid, true, filter);

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

        public ActionResult List()
        {
            IEnumerable<Property> feature = new List<Property>();
            var List = _vendorService.GetVendorsForProperties(true);
            var dropdownList = (from item in List.AsEnumerable()
                            select new SelectListItem
                            {
                                Text = item.Name,
                                Value = item.ID.ToString()
                            }).ToList();
            //dropdownList.Add(new SelectListItem() { Text = "Select Vendor", Value = "0" });

             ViewBag.Vendor = new SelectList(dropdownList, "Value", "Text");
            
            return PartialView(feature);
        }
         //[HttpPost]
        //public ActionResult List(string Type,int Vendor)
        //{
           

        //    var feature = _propService.GetPropertiesByFilter(Type, Vendor).OrderByDescending(x => x.ID);
        //    var List = _vendorService.GetVendors(true);
        //    var dropdownList = (from item in List.AsEnumerable()
        //                        select new SelectListItem
        //                        {
        //                            Text = item.Name,
        //                            Value = item.ID.ToString()
        //                        }).ToList();
        //    //dropdownList.Add(new SelectListItem() { Text = "Select Vendor", Value = "0" });

        //    ViewBag.Vendor = new SelectList(dropdownList, "Value", "Text");
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
            var Properties = _propService.GetVendorProperties(model.length, model.start, sortBy, sortDir, searchBy, vendorid, true, filter);

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
        public ActionResult PropertyActivation(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var property = _propService.GetById((int)id);
            if (property == null)
            {
                return HttpNotFound();
            }

            if (property.IsActive.HasValue)
            {
                if (property.IsActive.Value)
                {
                    property.IsActive = false;
                }
                else
                {
                    property.IsActive = true;
                }
            }

            else
                property.IsActive = true;

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
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int Id)
        {
            BindDropdown();

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

                currentProperty.IsPublished = false;

                currentProperty.ModifiedDate = Helpers.TimeZone.GetLocalDateTime();
                currentProperty.Slug = currentProperty.Slug.Replace(Slugify.GenerateSlug(currentProperty.Title), Slugify.GenerateSlug(Entity.Title));

                Entity = null;

                if (_propService.UpdateProperty(ref currentProperty, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        //url = "/Admin/Car/Index",
                        message = "Property updated successfully ...",
                        //data = new
                        //{
                        //    message = message
                        //    //ID = Entity.ID,
                        //    //Date = Entity.CreatedOn.ToString("dd MMM yyyy, h: mm tt"),
                        //    //SKU = Entity.SKU,
                        //    //Name = Entity.Name,
                        //    //LongDescription = Entity.LongDescription,
                        //    //Remark = Entity.Remarks,
                        //    //IsActive = Entity.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString
                        //}
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

        [HttpGet]
        public ActionResult Reject(long id)
        {
            ViewBag.BuildingID = id;
            ViewBag.ApprovalStatus = 4;

            var car = _propService.GetById((int)id);

            return View(car);
        }

        [HttpGet]
        public ActionResult Approve(int id)
        {

            var prop = _propService.GetById(id);
            if (prop == null)
            {
                return HttpNotFound();
            }

            prop.IsActive = true;
            prop.ApprovalStatusID = 3;

            string message = string.Empty;

            if (_propService.UpdateProperty(ref prop, ref message, false))
            {
                SuccessMessage = "Property " + (prop.ApprovalStatusID == 3 ? "Approved" : "Rejected") + "  successfully ...";

                if (prop.VendorId != null)
                {

                    var vendor = _vendorService.GetVendor((long)prop.VendorId);

                    Notification not = new Notification();
                    not.Title = "Property Approval";
                    not.TitleAr = "الموافقة على المنتج";
                    if (prop.ApprovalStatusID == 3)
                    {
                        not.Description = "Your property " + prop.Title + " have been approved ";
                        not.Url = "/Vendor/Property/Index";
                    }
                    else
                    {
                        not.Description = "Your " + prop.Title + " have been rejected ";
                        not.Url = "/Vendor/Property/ApprovalIndex";
                    }
                    not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                    not.OriginatorName = Session["UserName"].ToString();
                    not.Module = "Property";
                    not.OriginatorType = "Admin";
                    not.RecordID = prop.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        NotificationReceiver notRec = new NotificationReceiver();
                        notRec.ReceiverID = prop.VendorId;
                        notRec.ReceiverType = "Vendor";
                        notRec.NotificationID = not.ID;
                        if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                        {
                            var tokens = _vendorSessionService.GetVendorSessionFirebaseTokens(prop.VendorId.Value);
                            if (tokens.Length > 0)
                            {
                                var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
                                {
                                    Module = not.Module,
                                    //RecordID = notificationModel.PropertyID,
                                    NotificationID = notRec.ID
                                }, false);
                            }
                        }

                    }


                    return Json(new
                    {

                        success = true,
                        message = SuccessMessage,
                        data = new
                        {
                            ID = prop.ID,
                            IsPublished = prop.IsPublished,
                            Date = prop.CreatedOn.ToString("dd MMM yyyy, h: mm tt"),
                            Vendor = vendor.Name + "|" + vendor.VendorCode,
                            Thumbnail = prop.Thumbnail + "|" + prop.Title + "|" + prop.Address,
                            Property = prop.Title + "|" + prop.Address,
                            ApprovalStatus = prop.ApprovalStatusID,
                            IsActive = prop.IsActive.HasValue ? prop.IsActive.Value.ToString() : bool.FalseString
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { success = true, message = SuccessMessage }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(Property Model)
        {

            var prop = _propService.GetById((int)Model.ID);
            if (prop == null)
            {
                return HttpNotFound();
            }

            prop.ApprovalStatusID = 4;

            prop.Remarks = prop.Remarks + "<hr />" + Helpers.TimeZone.GetLocalDateTime().ToString("dd MMM yyyy, h:mm tt") + "<br />" + Model.Remarks;

            string message = string.Empty;

            if (_propService.UpdateProperty(ref prop, ref message, false))
            {
                SuccessMessage = "Property " + (prop.ApprovalStatusID == 3 ? "Approved" : "Rejected") + "  successfully ...";

                if (prop.VendorId != null)
                {
                    var vendor = _vendorService.GetVendor((long)prop.VendorId);

                    Notification not = new Notification();
                    not.Title = "Property Rejection";
                    not.TitleAr = "الموافقة على المنتج";
                    if (prop.ApprovalStatusID == 3)
                    {
                        not.Description = "Your property " + prop.Title + " have been approved ";
                        not.Url = "/Vendor/Property/Index";
                    }
                    else
                    {
                        not.Description = "Your " + prop.Title + " have been rejected ";
                        not.Url = "/Vendor/Property/ApprovalIndex";
                    }
                    not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                    not.OriginatorName = Session["UserName"].ToString();
                    not.Module = "Property";
                    not.OriginatorType = "Admin";
                    not.RecordID = prop.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        NotificationReceiver notRec = new NotificationReceiver();
                        notRec.ReceiverID = prop.VendorId;
                        notRec.ReceiverType = "Vendor";
                        notRec.NotificationID = not.ID;
                        if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                        {
                            var tokens = _vendorSessionService.GetVendorSessionFirebaseTokens(prop.VendorId.Value);
                            if (tokens.Length > 0)
                            {
                                var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
                                {
                                    Module = not.Module,
                                    //RecordID = notificationModel.PropertyID,
                                    NotificationID = notRec.ID
                                }, false);
                            }
                        }

                    }


                    return Json(new
                    {

                        success = true,
                        message = SuccessMessage,
                        data = new
                        {
                            ID = prop.ID,
                            Date = prop.CreatedOn.ToString("dd MMM yyyy, h: mm tt"),
                            Vendor = vendor.Logo + "|" + vendor.VendorCode + "|" + vendor.Name,
                            Property = prop.Title + "|" + prop.Address,
                            ApprovalStatus = prop.ApprovalStatusID,
                            IsActive = prop.IsActive.HasValue ? prop.IsActive.Value.ToString() : bool.FalseString
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { success = true, message = SuccessMessage }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ToggleFeatured(int ID)
        {
            string featuremsg = string.Empty;
            var prop = _propService.GetById(ID);
            if (prop == null)
            {
                return HttpNotFound();
            }

            if (prop.IsFeatured.HasValue)
            {
                if (prop.IsFeatured.Value)
                {
                    prop.IsFeatured = false;
                    featuremsg = "Property unfeatured successfully";
                }
                else
                {
                    prop.IsFeatured = true;
                    featuremsg = "Property featured successfully";
                }
            }
            else
            {
                prop.IsFeatured = true;
                featuremsg = "Property featured successfully";
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

        [HttpPost]
        public ActionResult ToggleVerify(int ID)
        {
            string featuremsg = string.Empty;
            var prop = _propService.GetById(ID);
            if (prop == null)
            {
                return HttpNotFound();
            }

            if (prop.IsVerified.HasValue)
            {
                if (prop.IsVerified.Value)
                {
                    prop.IsVerified = false;
                    featuremsg = "Property unverified successfully...";
                }
                else
                {
                    prop.IsVerified = true;
                    featuremsg = "Property verified successfully...";
                }
            }
            else
            {
                prop.IsVerified = true;
                featuremsg = "Property verified successfully...";
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

        [HttpPost]
        public ActionResult TogglePremium(int ID)
        {
            string featuremsg = string.Empty;
            var prop = _propService.GetById(ID);
            if (prop == null)
            {
                return HttpNotFound();
            }

            if (prop.IsPremium.HasValue)
            {
                if (prop.IsPremium.Value)
                {
                    prop.IsPremium = false;
                    featuremsg = "Property has been removed from premium category...";
                }
                else
                {
                    prop.IsPremium = true;
                    featuremsg = "Property has been set to premium category...";
                }
            }
            else
            {
                prop.IsPremium = true;
                featuremsg = "Property has been set to premium category...";
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
        [HttpPost]
        public ActionResult ApproveAll(List<long> ids)
        {
            foreach (var items in ids)
            {
                var prop = _propService.GetById((int)items);
                if (prop == null)
                {
                    return HttpNotFound();
                }

                prop.IsActive = true;
                prop.ApprovalStatusID = 3;

                string message = string.Empty;

                if (_propService.UpdateProperty(ref prop, ref message, false))
                {
                    SuccessMessage = "Property " + (prop.ApprovalStatusID == 3 ? "Approved" : "Rejected") + "  successfully ...";

                    if (prop.VendorId != null)
                    {

                        var vendor = _vendorService.GetVendor((long)prop.VendorId);

                        Notification not = new Notification();
                        not.Title = "Property Approval";
                        not.TitleAr = "الموافقة على المنتج";
                        if (prop.ApprovalStatusID == 3)
                        {
                            not.Description = "Your property " + prop.Title + " have been approved ";
                            not.Url = "/Vendor/Property/Index";
                        }
                        else
                        {
                            not.Description = "Your " + prop.Title + " have been rejected ";
                            not.Url = "/Vendor/Property/ApprovalIndex";
                        }
                        not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                        not.OriginatorName = Session["UserName"].ToString();
                        not.Module = "Property";
                        not.OriginatorType = "Admin";
                        not.RecordID = prop.ID;
                        if (_notificationService.CreateNotification(not, ref message))
                        {
                            NotificationReceiver notRec = new NotificationReceiver();
                            notRec.ReceiverID = prop.VendorId;
                            notRec.ReceiverType = "Vendor";
                            notRec.NotificationID = not.ID;
                            if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                            {
                                var tokens = _vendorSessionService.GetVendorSessionFirebaseTokens(prop.VendorId.Value);
                                if (tokens.Length > 0)
                                {
                                    var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
                                    {
                                        Module = not.Module,
                                        //RecordID = notificationModel.PropertyID,
                                        NotificationID = notRec.ID
                                    }, false);
                                }
                            }

                        }
                    }
                    else
                        return Json(new { success = true, message = SuccessMessage }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ErrorMessage = "Oops! Something went wrong. Please try later.";
                }
            }
            return Json(new
            {

                success = true,
                message = SuccessMessage,
                data = new
                {
                    //ID = prop.ID,
                    //Date = prop.CreatedOn.ToString("dd MMM yyyy, h: mm tt"),
                    //Vendor = vendor.Logo + "|" + vendor.VendorCode + "|" + vendor.Name,
                    //Property = prop.Title + "|" + prop.Address,
                    //ApprovalStatus = prop.ApprovalStatusID,
                    //IsActive = prop.IsActive.HasValue ? prop.IsActive.Value.ToString() : bool.FalseString
                }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RejectAll(List<long> ids)
        {
            foreach (var items in ids)
            {
                var prop = _propService.GetById((int)items);
                if (prop == null)
                {
                    return HttpNotFound();
                }

                prop.ApprovalStatusID = 4;

                string message = string.Empty;

                if (_propService.UpdateProperty(ref prop, ref message, false))
                {
                    SuccessMessage = "Property " + (prop.ApprovalStatusID == 3 ? "Approved" : "Rejected") + "  successfully ...";

                    if (prop.VendorId != null)
                    {

                        var vendor = _vendorService.GetVendor((long)prop.VendorId);

                        Notification not = new Notification();
                        not.Title = "Property Rejection";
                        not.TitleAr = "الموافقة على المنتج";
                        if (prop.ApprovalStatusID == 3)
                        {
                            not.Description = "Your property " + prop.Title + " have been Rejected ";
                            not.Url = "/Vendor/Property/Index";
                        }
                        else
                        {
                            not.Description = "Your " + prop.Title + " have been rejected ";
                            not.Url = "/Vendor/Property/ApprovalIndex";
                        }
                        not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                        not.OriginatorName = Session["UserName"].ToString();
                        not.Module = "Property";
                        not.OriginatorType = "Admin";
                        not.RecordID = prop.ID;
                        if (_notificationService.CreateNotification(not, ref message))
                        {
                            NotificationReceiver notRec = new NotificationReceiver();
                            notRec.ReceiverID = prop.VendorId;
                            notRec.ReceiverType = "Vendor";
                            notRec.NotificationID = not.ID;
                            if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                            {
                                var tokens = _vendorSessionService.GetVendorSessionFirebaseTokens(prop.VendorId.Value);
                                if (tokens.Length > 0)
                                {
                                    var response = PushNotification.SendPushNotification(tokens, not.Title, not.Description, new
                                    {
                                        Module = not.Module,
                                        //RecordID = notificationModel.PropertyID,
                                        NotificationID = notRec.ID
                                    }, false);
                                }
                            }
                        }
                    }
                }
                else
                {
                    ErrorMessage = "Oops! Something went wrong. Please try later.";
                }
            }
            // return RedirectToAction("Index");
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var prop = _propService.GetById((int)id);
            if (prop == null)
            {
                return HttpNotFound();
            }

            if (prop.IsActive.HasValue)
            {
                if (!(bool)prop.IsActive)
                    prop.IsActive = true;
                else
                {
                    prop.IsActive = false;
                }
            }
            else
                prop.IsActive = true;

            string message = string.Empty;
            if (_propService.UpdateProperty(ref prop, ref message))
            {
                SuccessMessage = "Property " + ((bool)prop.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(int id)
        {
            BindDropdown();

            Property model = _propService.GetById(id);

            var List = _PropCatService.GetAll();

            var dropdownList = from item in List
                               select new { value = item.ID, text = item.CategoryName };

            ViewBag.CategoryId = new SelectList(dropdownList, "value", "text", model.CategoryId);

            ViewBag.CountryId = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text", model.CountryID);

            ViewBag.CityID = new SelectList(_cityService.GetCitiesForDropDown(), "value", "text", model.CityID);

            ViewBag.Features = _featureService.GetAllPropertyFeature();


            return View(model);
        }

        public ActionResult BulkUpload()
        {
            ViewBag.VendorID = new SelectList(_vendorService.GetVendorsForDropDown(), "value", "text");
            return View();
        }

        [HttpPost]
        public ActionResult BulkUpload(HttpPostedFileBase FileUpload, string connectionId, string VendorID)
        {
            try
            {
                long vendorid = Convert.ToInt64(VendorID);

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
                            //long VendorID = (long)Session["VendorID"];


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
                                if (packagelimit.PropertyLimit + ExtraLimit < Vendorlimit + total)
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
                                        Property prop = _propService.GetByReferenceCode(item.AdsReferenceCode, vendorid);
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

                                        //if (!string.IsNullOrEmpty(item.ProductSports))
                                        //{
                                        //    item.ProductSports = item.ProductSports.Replace("&", "&amp;");
                                        //}
                                        if (!string.IsNullOrEmpty(item.Description))
                                        {
                                            item.Description = item.Description.Replace("`", "'");
                                        }
                                        if (_propService.PostExcelData(vendorid
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
                                                                            ,"Approval"
                                                                            , Convert.ToDecimal(item.Latitude)
                                                                            , Convert.ToDecimal(item.Longitude)
                                                                            , item.Area
                                                                            ))
                                        {
                                            Property product = _propService.GetByReferenceCode(item.AdsReferenceCode, vendorid);


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

        //  [HttpPost]
        //  [ValidateAntiForgeryToken]
        //  public ActionResult CarsReport()
        //  {
        //      var properties = _propService.GetForApproval();
        //      if (properties.Count() > 0)
        //      {
        //          using (ExcelPackage excel = new ExcelPackage())
        //          {
        //              excel.Workbook.Worksheets.Add("Properties");

        //              var headerRow = new List<string[]>()
        //              {
        //              new string[]{
        //                  "Title"
        //                  ,"TitleAr"
        //                  ,"Price"
        //                  ,"SellerTransactionFee"
        //                  ,"BuyerTransactionFee"
        //                  ,"Vendor"
        //                  ,"Description"
        //                  ,"DescriptionAr"
        //                  ,"ApprovalStatus"
        //                  ,"Published"
        //                  ,"Size"
        //                  ,"NoOfGarage"
        //                  ,"NoOfRooms"
        //                  ,"NoOfBaths"
        //                  ,"NoOfDinning"
        //                  ,"NoOfLaundry"
        //                  ,"BuildYear"
        //                  ,"Category"
        //                  ,"Video"
        //                  ,"ForSale"
        //                  ,"Remarks"
        //                  ,"Area"
        //                  ,"State"
        //                  ,"ZipCode"
        //                  ,"ZonedFor"
        //                  ,"Address"
        //                  ,"Active"                        
        //}

        //          };

        //              // Determine the header range (e.g. A1:D1)
        //              string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

        //              // Target a worksheet
        //              var worksheet = excel.Workbook.Worksheets["Cars"];

        //              // Popular header row data
        //              worksheet.Cells[headerRange].LoadFromArrays(headerRow);

        //              var cellData = new List<object[]>();

        //              foreach (var i in properties)
        //              {
        //                  cellData.Add(new object[]
        //                  {
        ////i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-",
        // !string.IsNullOrEmpty(i.Title) ? i.Title : "-"
        //                  ,!string.IsNullOrEmpty(i.TitleAr) ? i.TitleAr : "-"
        //                  ,i.Price.HasValue == true ? "-" : i.Price
        //                  ,!string.IsNullOrEmpty(i.SellerTransactionFee) ? i.SellerTransactionFee : "-"
        //                  ,!string.IsNullOrEmpty(i.CarMake) ? i.CarMake : "-"
        //                  ,!string.IsNullOrEmpty(i.BodyType) ? i.BodyType : "-"
        //                  ,!string.IsNullOrEmpty(i.CarModel) ? i.CarModel : "-"
        //                  ,!string.IsNullOrEmpty(i.Doors) ? i.Doors : "-"
        //                  ,!string.IsNullOrEmpty(i.Cylinders) ? i.Cylinders : "-"
        //                  ,!string.IsNullOrEmpty(i.HorsePower) ? i.HorsePower : "-"
        //                  ,!string.IsNullOrEmpty(i.RegionalSpecification) ? i.RegionalSpecification : "-"
        //                  ,!string.IsNullOrEmpty(i.LicensePlate) ? i.LicensePlate : "-"
        //                  ,!string.IsNullOrEmpty(i.Year) ? i.Year : "-"
        //                  ,!string.IsNullOrEmpty(i.Capacity) ? i.Capacity : "-"
        //                  ,!string.IsNullOrEmpty(i.Transmission) ? i.Transmission : "-"
        //                  ,!string.IsNullOrEmpty(i.FuelEconomy) ? i.FuelEconomy : "-"
        //                  ,i.EnableDelivery == true ? "Yes" : "No"
        //                  ,!string.IsNullOrEmpty(i.chargesType) ? i.chargesType : "-"
        //                  ,!string.IsNullOrEmpty(i.DeliveryCharges.ToString()) ? i.DeliveryCharges.ToString() : "-"
        //                  ,!string.IsNullOrEmpty(i.Thumbnail) ? i.Thumbnail : "-"
        //                  ,!string.IsNullOrEmpty(i.Images) ? i.Images : "-"
        //                  ,!string.IsNullOrEmpty(i.Specification) ? i.Specification : "-"
        //                  ,i.IsFeatured == true ? "Yes" : "No"
        //                  ,i.IsPublished == true ? "Yes" : "No"
        //                  ,!string.IsNullOrEmpty(i.CarFeature) ? i.CarFeature : "-"
        //                  ,!string.IsNullOrEmpty(i.Tags) ? i.Tags : "-"


        //                  ,carPackages.Count()>0 && carPackages[0].Price.HasValue? carPackages[0].Price.Value.ToString(): ""
        //                  ,carPackages.Count()>0 && carPackages[0].Kilometer.HasValue? carPackages[0].Kilometer.Value.ToString(): ""

        //                  ,carPackages.Count()>1 && carPackages[1].Price.HasValue? carPackages[1].Price.Value.ToString(): ""
        //                  ,carPackages.Count()>1 && carPackages[1].Kilometer.HasValue? carPackages[1].Kilometer.Value.ToString(): ""

        //                  ,carPackages.Count()>2 && carPackages[2].Price.HasValue? carPackages[2].Price.Value.ToString(): ""
        //                  ,carPackages.Count()>2 && carPackages[2].Kilometer.HasValue? carPackages[2].Kilometer.Value.ToString(): ""

        //                  ,carPackages.Count()>3 && carPackages[3].Price.HasValue? carPackages[3].Price.Value.ToString(): ""
        //                  ,carPackages.Count()>3 && carPackages[3].Kilometer.HasValue? carPackages[3].Kilometer.Value.ToString(): ""

        //                  ,carInsurance !=null && !string.IsNullOrEmpty(carInsurance.Name) ? carInsurance.Name : "-"
        //                  ,carInsurance !=null &&!string.IsNullOrEmpty(carInsurance.NameAr) ? carInsurance.NameAr : "-"
        //                  ,carInsurance !=null &&!string.IsNullOrEmpty(carInsurance.Details) ? carInsurance.Details : "-"
        //                  ,carInsurance !=null &&!string.IsNullOrEmpty(carInsurance.DetailsAr) ? carInsurance.DetailsAr : "-"



        //                  });
        //                  //if (i.Type == "2")
        //                  //{
        //                  //    var variants = getAllCarsVariations.Where(x => x.CarID == i.ID).ToList();
        //                  //    foreach (var j in variants)
        //                  //    {
        //                  //        cellData.Add(new object[]
        //                  //            {
        //                  //            i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
        //                  //            ,!string.IsNullOrEmpty(i.SKU) ? i.SKU : "-"
        //                  //            ,!string.IsNullOrEmpty(i.Slug) ? i.Slug : "-"
        //                  //            ,!string.IsNullOrEmpty(i.Thumbnail) ? i.Thumbnail : "-"
        //                  //            ,!string.IsNullOrEmpty(i.Images) ? i.Images : "-"
        //                  //            ,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
        //                  //            ,!string.IsNullOrEmpty(i.ShortDescription) ? i.ShortDescription : "-"
        //                  //            ,!string.IsNullOrEmpty(i.LongDescription) ? i.LongDescription : "-"
        //                  //            ,!string.IsNullOrEmpty(i.MobileDescription) ? i.MobileDescription : "-"
        //                  //            ,!string.IsNullOrEmpty(i.Brand) ? i.Brand : "-"
        //                  //            ,!string.IsNullOrEmpty(i.CarCategories) ? i.CarCategories : "-"
        //                  //            ,!string.IsNullOrEmpty(i.Tags) ? i.Tags : "-"
        //                  //            ,!string.IsNullOrEmpty(i.Attributes) ? i.Attributes : "-"
        //                  //            ,i.RegularPrice ?? 0
        //                  //            ,i.SalePrice ?? 0
        //                  //            ,i.SalePriceFrom.HasValue ? i.SalePriceFrom.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
        //                  //            ,i.SalePriceTo.HasValue ? i.SalePriceTo.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
        //                  //            ,i.Stock ?? 0
        //                  //            ,i.Threshold ?? 0
        //                  //            ,!string.IsNullOrEmpty(i.Type) ? i.Type : "-"
        //                  //            ,i.Weight ?? 0
        //                  //            ,i.Length ?? 0
        //                  //            ,i.Width ?? 0
        //                  //            ,i.Height ?? 0
        //                  //            ,i.IsSoldIndividually == true ? "Yes" : "No"
        //                  //            ,i.AllowMultiplePerOrder == true ? "Yes" : "No"
        //                  //            ,i.IsManageStock == true ? "Yes" : "No"
        //                  //            ,i.StockStatus == null ? "-" : (i.StockStatus == 1 ? "In Stock" : "Out Of Stock")
        //                  //            ,i.IsPublished == true ? "Yes" : "No"
        //                  //            ,i.IsFeatured == true ? "Yes" : "No"
        //                  //            ,i.IsApproved == true ? "Yes" : "No"
        //                  //            ,!string.IsNullOrEmpty(i.Remarks) ? i.Remarks : "-"
        //                  //            ,i.IsActive == true ? "Yes" : "No"
        //                  //            ,!string.IsNullOrEmpty(j.VariantSKU) ? j.VariantSKU : "-"
        //                  //            ,!string.IsNullOrEmpty(j.VariantThumbnail) ? j.VariantThumbnail : "-"
        //                  //            ,!string.IsNullOrEmpty(j.VariantImages) ? j.VariantImages : "-"
        //                  //            ,j.VariantRegularPrice ?? 0
        //                  //            ,j.VariantSalePrice ?? 0
        //                  //            ,j.VariantSalePriceFrom.HasValue ? j.VariantSalePriceFrom.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
        //                  //            ,j.VariantSalePriceTo.HasValue ? j.VariantSalePriceTo.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
        //                  //            ,j.VariantStock ?? 0
        //                  //            ,j.VariantThreshold ?? 0
        //                  //            ,i.IsManageStock == true ? "Yes" : "No"
        //                  //            ,!string.IsNullOrEmpty(j.VariantStockStatus) ? (j.VariantStockStatus == "1" ? "In Stock" : "Out Of Stock") : "-"
        //                  //            ,j.VariantWeight ?? 0
        //                  //            ,j.VariantLength ?? 0
        //                  //            ,j.VariantWidth ?? 0
        //                  //            ,j.VariantHeight ?? 0
        //                  //            ,!string.IsNullOrEmpty(j.VariantDescription) ? j.VariantDescription : "-"
        //                  //            ,!string.IsNullOrEmpty(j.VariantAttributes) ? j.VariantAttributes : "-"
        //                  //            ,i.IsSoldIndividually == true ? "Yes" : "No"
        //                  //            ,i.IsActive == true ? "Yes" : "No"
        //                  //        });
        //                  //    }
        //                  //}
        //              }

        //              worksheet.Cells[2, 1].LoadFromArrays(cellData);

        //              return File(excel.GetAsByteArray(), "application/msexcel", "Cars Report.xlsx");
        //          }
        //      }
        //      return RedirectToAction("Index");
        //  }


        public void BindDropdown()
        {
            var List = _PropCatService.GetAll();

            var dropdownList = from item in List
                               select new { value = item.ID, text = item.CategoryName };

            ViewBag.CategoryId = new SelectList(dropdownList, "value", "text");

            ViewBag.CountryId = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text");

            ViewBag.Features = _featureService.GetAllPropertyFeature();
        }

        public ActionResult PropertyAdIndex()
        {
            return View();
        }
        public ActionResult AdList()
        {
            /*var feature = _propService.GetApprovedAds();*/
            var feature = _propService.GetApprovedAds();
            return PartialView(feature);
        }
        [HttpPost]

        public ActionResult PropertiesReport()
        {
            /*var vendorId = Convert.ToInt64(Session["VendorID"]);*/
            string ImageServer = CustomURL.GetImageServer();
            var getAllProperties = _propService.GetAll().ToList();
            /* var getAllPropertiesVariations = _propService.GetAll().ToList();*/
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
                        /*,"Remarks"*/

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
    }
}