using LinqToExcel;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.POCO;
using NowBuySell.Web.Helpers.PushNotification;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.Util;
using NowBuySell.Web.ViewModels.Car;
using NowBuySell.Web.ViewModels.DataTables;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using static NowBuySell.Web.Helpers.Enumerations.Enumeration;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CarController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ICarService _carService;
        private readonly IVendorService _vendorService;
        private readonly IAttributeService _attributeService;
        private readonly INotificationService _notificationService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly IBrandsService _brandService;
        private readonly IPackagesService _packagesService;
        private readonly ICarPackageService _carPackageService;
        private readonly ICarInsuranceService _insuranceService;
        private readonly ICarDocumentService _carDocumentService;
        private readonly IFeatureService _featureService;
        private readonly ICarFeatureService _carFeatureService;
        private readonly ICarImageService _carImageService;
        private readonly ICategoryService _categoryService;
        private readonly IBodyTypeService _bodyTypeService;
        private readonly ICarMakeService _carMakeService;
        private readonly ICarModelService _carModelService;
        private readonly ICustomerService _customerService;
        private readonly ICountryService _countryService;
        private readonly ICityService _cityService;
        private readonly IVendorSessionService _vendorSessionService;
        private readonly INumberRangeService _numberRangeService;
        private readonly IVendorPackagesService _vendorPackagesService;
        private readonly IVendorAddPurchasesService _VendorAddPurchasesService;

        public CarController(ICarService carService, IVendorService vendorService, IAttributeService attributeService, INotificationService notificationService,
            INotificationReceiverService notificationReceiverService, IBrandsService brandService, IPackagesService packagesService, ICarPackageService carPackageService,
            ICarInsuranceService insuranceService, ICarDocumentService carDocumentService, IFeatureService featureService, ICarFeatureService carFeatureService,
            ICarImageService carImageService, ICategoryService categoryService, IBodyTypeService bodyTypeService, ICarMakeService carMakeService, ICarModelService carModelService,
            ICustomerService customerService, ICountryService countryService, ICityService cityService, IVendorSessionService vendorSessionService, INumberRangeService numberRangeService, IVendorAddPurchasesService vendorAddPurchasesService, IVendorPackagesService vendorPackagesService)
        {
            this._carService = carService;
            this._vendorService = vendorService;
            this._attributeService = attributeService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
            this._brandService = brandService;
            this._packagesService = packagesService;
            this._carPackageService = carPackageService;
            this._insuranceService = insuranceService;
            this._carDocumentService = carDocumentService;
            this._featureService = featureService;
            this._carFeatureService = carFeatureService;
            this._carImageService = carImageService;
            this._categoryService = categoryService;
            this._bodyTypeService = bodyTypeService;
            this._carMakeService = carMakeService;
            this._carModelService = carModelService;
            _customerService = customerService;
            _countryService = countryService;
            _cityService = cityService;
            _vendorSessionService = vendorSessionService;
            _numberRangeService = numberRangeService;
            _VendorAddPurchasesService = vendorAddPurchasesService;
            _vendorPackagesService = vendorPackagesService;
        }

        #region Car

        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        public ActionResult List()
        {
            var List = _vendorService.GetVendorsForMotors(true);
            var dropdownList = (from item in List.AsEnumerable()
                                select new SelectListItem
                                {
                                    Text = item.Name,
                                    Value = item.ID.ToString()
                                }).ToList();
            //dropdownList.Add(new SelectListItem() { Text = "Select Vendor", Value = null });
            ViewBag.Vendor = new SelectList(dropdownList, "Value", "Text");
            return PartialView();
        }
        //Caradinfo
        public ActionResult Caradinfo()
        {
            ViewBag.Message = String.Format("Hello!.\\n This Page Is Under Development Process :(");
            return View();
        }

        [HttpPost]
        public JsonResult List(DataTableAjaxPostModel model, string filter,int? vendorid = null)
        {
            var searchBy = (model.search != null) ? model.search.value : "";
            int sortBy = 0;
            string sortDir = "";

            if (model.order != null)
            {
                sortBy = model.order[0].column;
                sortDir = model.order[0].dir.ToLower();
            }
            var Cars = _carService.GetVendorCars(model.length, model.start, sortBy, sortDir, searchBy, vendorid, true,filter);

            int filteredResultsCount = Cars != null && Cars != null && Cars.Count() > 0 ? (int)Cars.FirstOrDefault().FilteredResultsCount : 0;
            int totalResultsCount = Cars != null && Cars.Count() > 0 ? (int)Cars.FirstOrDefault().TotalResultsCount : 0;
            
            return Json(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = Cars
            });
        }

        public ActionResult Thumbnail(long? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Car car = _carService.GetCar((long)id);
                if (car == null)
                {
                    return HttpNotFound();
                }
                string filePath = !string.IsNullOrEmpty(car.Thumbnail) ? car.Thumbnail : string.Empty;


                string message = string.Empty;

                string absolutePath = Server.MapPath("~");
                string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/", car.SKU.Replace(" ", "_"));

                car.Thumbnail = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "Thumbnail", ref message, "Image", ApplyWatermark: true);

                if (_carService.UpdateCar(ref car, ref message, false))
                {
                    _carService.UpdateCarApprovalStatus(car.ID);

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        System.IO.File.Delete(Server.MapPath(filePath));
                    }
                    return Json(new
                    {
                        success = true,
                        message = message,
                        data = car.Thumbnail
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
                Car car = _carService.GetCar((long)id);
                if (car == null)
                {
                    return HttpNotFound();
                }
                string filePath = !string.IsNullOrEmpty(car.Video) ? car.Video : string.Empty;


                string message = string.Empty;

                string absolutePath = Server.MapPath("~");
                string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/Video/", car.SKU.Replace(" ", "_"));

                car.Video = Uploader.UploadVideo(Request.Files, absolutePath, relativePath, "Video", ref message, "Video");

                if (_carService.UpdateCar(ref car, ref message, false))
                {
                    _carService.UpdateCarApprovalStatus(car.ID);

                    //if (!string.IsNullOrEmpty(filePath))
                    //{
                    //    System.IO.File.Delete(Server.MapPath(filePath));
                    //}
                    return Json(new
                    {
                        success = true,
                        message = message,
                        data = car.Video
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


        public ActionResult InsuranceDetails(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var data = _insuranceService.GetInsuranceByID((long)id);
            if (data == null)
            {
                return HttpNotFound();
            }
            return View(data);
        }

        [HttpPost]
        public ActionResult ToggleFeatured(int ID)
        {
            string featuremsg = string.Empty;
            var prop = _carService.GetCar(ID);
            if (prop == null)
            {
                return HttpNotFound();
            }

            if (prop.IsFeatured.HasValue)
            {
                if (prop.IsFeatured.Value)
                {
                    prop.IsFeatured = false;
                    featuremsg = "Motor unfeatured successfully";
                }
                else
                {
                    prop.IsFeatured = true;
                    featuremsg = "Motor featured successfully";
                }
            }
            else
            {
                prop.IsFeatured = true;
                featuremsg = "Motor featured successfully";
            }

            string message = string.Empty;

            if (_carService.UpdateCar(ref prop, ref message, false, ForceApproval: true))
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
            var prop = _carService.GetCar(ID);
            if (prop == null)
            {
                return HttpNotFound();
            }

            if (prop.IsVerified.HasValue)
            {
                if (prop.IsVerified.Value)
                {
                    prop.IsVerified = false;
                    featuremsg = "Motor unverified successfully...";
                }
                else
                {
                    prop.IsVerified = true;
                    featuremsg = "Motor verified successfully...";
                }
            }
            else
            {
                prop.IsVerified = true;
                featuremsg = "Motor verified successfully...";
            }

            string message = string.Empty;

            if (_carService.UpdateCar(ref prop, ref message, false, ForceApproval: true))
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
            var prop = _carService.GetCar(ID);
            if (prop == null)
            {
                return HttpNotFound();
            }

            if (prop.IsPremium.HasValue)
            {
                if (prop.IsPremium.Value)
                {
                    prop.IsPremium = false;
                    featuremsg = "Motor has been removed from premium category...";
                }
                else
                {
                    prop.IsPremium = true;
                    featuremsg = "Motor has been set to premium category...";
                }
            }
            else
            {
                prop.IsPremium = true;
                featuremsg = "Motor has been set to premium category...";
            }

            string message = string.Empty;

            if (_carService.UpdateCar(ref prop, ref message, false, ForceApproval: true))
            {
                return Json(new { success = true, message = featuremsg }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListReport()
        {
            var cars = _carService.GetCars();
            return View(cars);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetCarPackages(long id)
        {
            var packages = _packagesService.GetPackages().Select(i => new { id = i.ID, name = i.PackageName });
            var carPackages = _carPackageService.GetPackageByCarID(id).Select(i => new { id = i.ID, carID = i.CarID, packageId = i.PackageID, price = i.Price, kilometer = i.Kilometer }).ToList();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                packages = packages,
                CarPackages = carPackages
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddCarPackage(CarPackage data)
        {
            string message = string.Empty;
            var Record = _carPackageService.GetPackageByCarIDandPackageID(data.CarID, (long)data.PackageID);
            if (Record == null)
            {
                if (_carPackageService.CreatePackage(ref data, ref message))
                {
                    return Json(new { success = true, message });
                }
            }
            else
            {
                if (_carPackageService.UpdatePackage(ref data, ref message))
                {

                }
            }
            return Json(new { success = true });
        }

        public ActionResult DeleteCarPackage(CarPackage data)
        {
            string message = string.Empty;
            _carPackageService.DeletePackage(data, ref message);
            return Json(new { success = true, message });
        }

        [HttpGet]
        public ActionResult GetInsurances(long id)
        {
            var insurance = _insuranceService.GetInsuranceByCarID(id).Select(i => new { id = i.ID, name = i.Name, namear = i.NameAr, car = i.Car.Name, details = i.Details, IsActive = i.IsActive });
            //var carPackages = _carPackageService.GetPackageByCarID(id).Select(i => new { id = i.ID, carID = i.CarID, packageId = i.PackageID, price = i.Price, kilometer = i.Kilometer }).ToList();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                insurance = insurance
                //CarPackages = carPackages
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDocuments(long id)
        {
            var document = _carDocumentService.GetDocumentByCarID(id).Select(i => new { id = i.ID, name = i.Name, path = i.Path });
            //var carPackages = _carPackageService.GetPackageByCarID(id).Select(i => new { id = i.ID, carID = i.CarID, packageId = i.PackageID, price = i.Price, kilometer = i.Kilometer }).ToList();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                document = document
                //CarPackages = carPackages
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateDocuments(long id)
        {
            ViewBag.CarID = id;
            return View();
        }

        [HttpPost]
        public ActionResult CreateDocuments(string Name, long id)
        {
            string message = string.Empty;
            CarDocument data = new CarDocument();
            data.CarID = id;
            string absolutePath = Server.MapPath("~");
            string relativePath = string.Format("/Assets/AppFiles/Documents/Cars/{0}/", id.ToString().Replace(" ", "_"));
            data.Name = Name;
            data.Path = Uploader.UploadDocs(Request.Files, absolutePath, relativePath, "Document", ref message, "FileUpload");

            if (_carDocumentService.CreateDocument(ref data, ref message))
            {

            }
            //string message = string.Empty;
            //data.CarID = (long)TempData["CarID"];
            //if (data != null)
            //{
            //	if (_carDocumentService.CreateDocument(ref data, ref message))
            //	{

            //	}
            //}
            return Json(new
            {
                success = true,
                message = "Document added successfully!",
                data = data
                //CarPackages = carPackages
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteCarDocument(long id)
        {

            string message = string.Empty;
            _carDocumentService.DeleteDocument(id, ref message);
            return Json(new { success = true, data = id, message });
        }

        public JsonResult GetCarModelsByCarMakeID(int CarMakeID)
        {
            string message = string.Empty;
            if (CarMakeID > 0)
            {
                var qry = _carService.GetCarModelsForDropDown(CarMakeID);
                if (qry != null)
                {
                    return Json(qry);
                }
                else
                {
                    message = "Model not assigned yet";
                    return Json(new { success = false, data = 0, message = message });
                }
            }
            else
            {
                return Json(new { sucess = false, message = "Data not found" });
            }
        }

        public ActionResult Details(long? id)

        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Car car = _carService.GetCar((long)id);
            if (car == null)
            {
                return HttpNotFound();
            }
            //ViewBag.Years = Enumerable.Range(DateTime.Now.Year, -4);
            ViewBag.CategoryID = new SelectList(_categoryService.GetCarCategoriesForDropDown(), "value", "text", car.CategoryID);
            if (car.CarMakeID != null)
            {
                ViewBag.CarModelID = new SelectList(_carModelService.GetCarModelForDropDown((long)car.CarMakeID, "en"), "value", "text", car.CarModelID);
            }
            else
            {
                ViewBag.CarModelID = new SelectList(_carModelService.GetCarModelForDropDown(), "value", "text", car.CarModelID);
            }
            ViewBag.BodyTypeID = new SelectList(_bodyTypeService.GetBodyTypeForDropDown(), "value", "text", car.BodyTypeID);
            ViewBag.CarMakeID = new SelectList(_carMakeService.GetCarMakeForDropDown(), "value", "text", car.CarMakeID);
            ViewBag.AttributeID = new SelectList(_attributeService.GetAttributesForDropDown(), "value", "text");
            ViewBag.BrandID = new SelectList(_brandService.GetBrandsForDropDown(), "value", "text", car.BrandID);
            ViewBag.CountryId = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text");
            ViewBag.CityID = new SelectList(_cityService.GetCitiesForDropDown(), "value", "text", car.CityID);
            TempData["CarID"] = id;
            return View(car);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Car car)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_carService.CreateCar(car, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/Car/Index",
                        message = message,
                        data = new
                        {
                            ID = car.ID,
                            Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            SKU = car.SKU,
                            Name = car.Name,
                            NameAr = car.NameAr,
                            LongDescription = car.LongDescription,
                            LongDescriptionAr = car.LongDescriptionAr,
                            Remark = car.Remarks,
                            IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString

                        }
                    });
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }

            return Json(new { success = false, message = message });
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Car car = _carService.GetCar((long)id);
            if (car == null)
            {
                return HttpNotFound();
            }
            //ViewBag.Years = Enumerable.Range(DateTime.Now.Year, -4);
            ViewBag.CategoryID = new SelectList(_categoryService.GetCarCategoriesForDropDown(), "value", "text", car.CategoryID);
            if (car.CarMakeID != null)
            {
                ViewBag.CarModelID = new SelectList(_carModelService.GetCarModelForDropDown((long)car.CarMakeID, "en"), "value", "text", car.CarModelID);
            }
            else
            {
                ViewBag.CarModelID = new SelectList(_carModelService.GetCarModelForDropDown(), "value", "text", car.CarModelID);
            }
            ViewBag.BodyTypeID = new SelectList(_bodyTypeService.GetBodyTypeForDropDown(), "value", "text", car.BodyTypeID);
            ViewBag.CarMakeID = new SelectList(_carMakeService.GetCarMakeForDropDown(), "value", "text", car.CarMakeID);
            ViewBag.AttributeID = new SelectList(_attributeService.GetAttributesForDropDown(), "value", "text");
            ViewBag.BrandID = new SelectList(_brandService.GetBrandsForDropDown(), "value", "text", car.BrandID);
            ViewBag.CountryId = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text");
            ViewBag.CityID = new SelectList(_cityService.GetCitiesForDropDown(), "value", "text", car.CityID);
            TempData["CarID"] = id;
            return View(car);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Update(Car car)
        {
            string message = string.Empty;
            //if (ModelState.IsValid)
            //{
            car.Slug = car.Slug.Replace(Slugify.GenerateSlug(car.Name), Slugify.GenerateSlug(car.Name));
            if (_carService.UpdateCar(ref car, ref message, ForceApproval: true))
            {
                _carService.UpdateCarApprovalStatus(car.ID);

                return Json(new
                {
                    success = true,
                    //url = "/Admin/Car/Index",
                    message = "Motor updated successfully ...",
                    data = new
                    {
                        ID = car.ID,
                        Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        SKU = car.SKU,
                        Name = car.Name,
                        LongDescription = car.LongDescription,
                        Remark = car.Remarks,
                        IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString
                    }
                });
            }
            //}
            //else
            //{
            //    message = "Please fill the form properly ...";
            //}
            return Json(new { success = false, message = message });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateLocation(Car car)
        {

            string message = string.Empty;
            if (ModelState.IsValid)
            {
                var currentCar = _carService.GetCar(car.ID);

                currentCar.CountryID = car.CountryID;
                currentCar.CityID = car.CityID;
                currentCar.Area = car.Area;
                currentCar.Address = car.Address;
                currentCar.Latitude = car.Latitude;
                currentCar.Longitude = car.Longitude;

                currentCar.IsPublished = false;

                if ((currentCar.ApprovalStatus == 3) || (currentCar.ApprovalStatus == 4))
                {
                    currentCar.ApprovalStatus = 2;
                }
                else
                {
                    currentCar.ApprovalStatus = 1;
                }

                currentCar.ModifiedDate = Helpers.TimeZone.GetLocalDateTime();

                car = null;

                if (_carService.UpdateCar(ref currentCar, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        message = "Motor location updated successfully ...",
                    });
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }
            return Json(new
            {
                success = false,
                message = message
            });
        }



        //public ActionResult Edit(long? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Car car = _carService.GetCar((long)id);
        //    if (car == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    TempData["CarID"] = id;
        //    return View(car);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(Car car)
        //{
        //    string message = string.Empty;
        //    if (ModelState.IsValid)
        //    {
        //        long Id;
        //        if (TempData["CarID"] != null && Int64.TryParse(TempData["CarID"].ToString(), out Id) && car.ID == Id)
        //        {
        //            if (_carService.UpdateCar(ref car, ref message))
        //            {
        //                return Json(new
        //                {
        //                    success = true,
        //                    url = "/Admin/Car/Index",
        //                    message = "Car updated successfully ...",
        //                    data = new
        //                    {
        //                        ID = car.ID,
        //                        Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
        //                        SKU = car.SKU,
        //                        Name = car.Name,
        //                        NameAr = car.NameAr,
        //                        LongDescription = car.LongDescription,
        //                        LongDescriptionAr = car.LongDescriptionAr,
        //                        Remark = car.Remarks,
        //                        IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString

        //                    }
        //                });
        //            }

        //        }
        //        else
        //        {
        //            message = "Oops! Something went wrong. Please try later.";
        //        }
        //    }
        //    else
        //    {
        //        message = "Please fill the form properly ...";
        //    }
        //    return Json(new { success = false, message = message });
        //}

        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Car car = _carService.GetCar((Int16)id);
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
            if (_carService.DeleteCar((Int16)id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
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

                            string sheetName = "BulkMotor";
                            //long VendorID = (long)Session["VendorID"];


                            string data = "";
                            string message = "";
                            List<string> ErrorItems = new List<string>();
                            int count = 0;
                            var total = 0;
                            int successCount = 0;



                            var excelFile = new ExcelQueryFactory(pathToExcelFile);
                            IList<CarWorkSheet> Car = (from a in excelFile.Worksheet<CarWorkSheet>(sheetName) select a).ToList();
                            total = Car.Count();
                            //if (Car.Count() != Car.Select(i => i.Slug).Distinct().Count())
                            //{
                            //	return Json(new
                            //	{
                            //		success = false,
                            //		message = "All Cars Must Contain"
                            //	}, JsonRequestBehavior.AllowGet);
                            //}
                            Vendor vendor = _vendorService.GetVendor(VendorID);
                            long packageId = 0;
                            if (vendor != null)
                            {
                                packageId = (long)vendor.VendorPackageID;
                            }
                            int Vendorlimit = _carService.GetVendorLimit(vendor.ID);
                            var packagelimit = _vendorPackagesService.GetPackageLimit(packageId);
                            DateTime currentDateTime = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
                            int ExtraLimit = _VendorAddPurchasesService.GetVendorLimit(currentDateTime, (int)vendor.ID);
                            if (currentDateTime.Date < vendor.PackageEndDate.Value.Date)
                            {
                                if (packagelimit.MotorLimit + ExtraLimit < Vendorlimit + total)
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
                            string numberRange = _numberRangeService.GetNextValueFromNumberRangeByName("CAR");
                            foreach (CarWorkSheet item in Car)
                            {
                                try
                                {
                                    var results = new List<ValidationResult>();
                                    var context = new ValidationContext(item, null, null);
                                    if (Validator.TryValidateObject(item, context, results))
                                    {
                                        //if (string.IsNullOrEmpty(item.VariantSKU))
                                        //{
                                        /* IF CAR IS SIMPLE*/

                                        /*Upload Car Thumnail*/

                                        string filePath = string.Empty;

                                        Car currentCar = _carService.GetByReferenceCode(item.AdsReferenceCode, vendorid);
                                        if (currentCar == null)
                                        {
                                            item.AdsReferenceCode = _numberRangeService.GetNextValueFromNumberRangeByName("MOTORREFERENCE");
                                        }
                                        //item.AdsReferenceCode = _numberRangeService.GetNextValueFromNumberRangeByName("MOTORREFERENCE");
                                        if (!string.IsNullOrEmpty(item.Thumbnail))
                                        {
                                            filePath = item.Thumbnail;

                                            message = string.Empty;
                                            item.SKU = item.ChasisNo;
                                            string absolutePath = Server.MapPath("~");
                                            string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/", item.AdsReferenceCode.Replace(" ", "_"));

                                            item.Thumbnail = Uploader.SaveImage(item.Thumbnail, absolutePath, relativePath, "PTI", ImageFormat.Jpeg);

                                        }

                                        if (!string.IsNullOrEmpty(item.Features))
                                        {
                                            item.Features = item.Features.Replace("&", "&amp;");
                                        }
                                        if (!string.IsNullOrEmpty(item.Documents))
                                        {
                                            item.Documents = item.Documents.Replace("&", "&amp;");
                                        }
                                        if (!string.IsNullOrEmpty(item.Description))
                                        {
                                            item.Description = item.Description.Replace("`", "'");
                                        }
                                        item.BodyCondition = item.BodyCondition != null ? item.BodyCondition.Replace("to", "-") : null;
                                        if (_carService.PostExcelData(vendorid
                                                                        , item.SKU
                                                                        , string.IsNullOrEmpty(item.Slug) ? Slugify.GenerateSlug(item.Name + "-" + numberRange) : item.Slug
                                                                        , item.Name
                                                                        , item.NameAr
                                                                        , item.Thumbnail
                                                                        , item.Category
                                                                        , item.Make
                                                                        , item.BodyType
                                                                        , item.Model
                                                                        , item.IsFeatured == "Yes" ? true : false
                                                                        , item.Features
                                                                        , item.Doors, item.Cylinders, item.HorsePower, item.RegionalSpecification
                                                                        , item.Transmission
                                                                        , item.Capacity
                                                                        , item.KilometersDriven
                                                                        //, item.EnableDelivery == "Yes" ? true : false
                                                                        , Convert.ToDecimal(item.RegularPrice)
                                                                        , item.Year
                                                                        , item.Description
                                                                        , item.DescriptionAr
                                                                        , item.AdsReferenceCode
                                                                        , item.ServiceHistory == "Yes" ? true : false
                                                                        , item.MechanicalCondition
                                                                        , item.Warranty == "Yes" ? true : false
                                                                        , item.SteeringSide
                                                                        , item.Wheels
                                                                        , item.EngineCC
                                                                        , item.BodyCondition
                                                                        , item.Address
                                                                        , Convert.ToDecimal(item.Price)
                                                                        , item.FuelType
                                                                        , 3
                                                                        , true
                                                                        , Convert.ToDecimal(item.Latitude)
                                                                        , Convert.ToDecimal(item.Longitude)
                                                                        , item.Area
                                                                        , item.City
                                                                        ))
                                        {
                                            Car car = _carService.GetByReferenceCode(item.AdsReferenceCode, vendorid);

                                            if (!string.IsNullOrEmpty(item.Video))
                                            {

                                                string absolutePath = Server.MapPath("~");
                                                string relativePath = string.Format("/Assets/AppFiles/Images/Cars/{0}/", car.ID.ToString().Replace(" ", "_"));
                                                message = string.Empty;
                                                string Pictures = "";
                                                Uploader.SaveVideos(item.Video, absolutePath, relativePath, "PGI", ref Pictures, "Video");
                                                var imageCount = 0;
                                                car.Video = Pictures;
                                                _carService.UpdateCar(ref car, ref message);

                                            }
                                            /*Uploading Car Images*/
                                            if (!string.IsNullOrEmpty(item.Images))
                                            {
                                                message = string.Empty;
                                                string absolutePath = Server.MapPath("~");
                                                string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/Gallery/", car.AdsReferenceCode.Replace(" ", "_"));

                                                List<string> Pictures = new List<string>();

                                                Uploader.SaveImages(item.Images.Split(','), absolutePath, relativePath, "PGI", ImageFormat.Jpeg, ref Pictures);
                                                var imageCount = 0;
                                                foreach (var image in Pictures)
                                                {
                                                    CarImage carImage = new CarImage();
                                                    carImage.CarID = car.ID;
                                                    carImage.Image = image;
                                                    carImage.Position = ++imageCount;
                                                    if (!_carImageService.CreateCarImage(ref carImage, ref message))
                                                    {
                                                    }
                                                }
                                            }
                                            //if (!string.IsNullOrEmpty(item.Video))
                                            //{

                                            //    string absolutePath = Server.MapPath("~");
                                            //    string relativePath = string.Format("/Assets/AppFiles/Images/Cars/{0}/", car.ID.ToString().Replace(" ", "_"));
                                            //    message = string.Empty;
                                            //    List<string> Pictures = new List<string>();
                                            //    Uploader.SaveDocx(item.Documents.Split(','), absolutePath, relativePath, "PGI", ImageFormat.Jpeg, ref Pictures);
                                            //    var imageCount = 0;
                                            //    foreach (var image in Pictures)
                                            //    {
                                            //        CarDocument cd = new CarDocument();
                                            //        cd.CarID = car.ID;
                                            //        cd.Path = image;
                                            //        if (!_carDocumentService.CreateDocument(ref cd, ref message))
                                            //        {
                                            //        }
                                            //    }
                                            //}
                                            if (!string.IsNullOrEmpty(item.Documents))
                                            {

                                                string absolutePath = Server.MapPath("~");
                                                string relativePath = string.Format("/Assets/AppFiles/Documents/Cars/{0}/", car.ID.ToString().Replace(" ", "_"));
                                                message = string.Empty;
                                                List<string> Pictures = new List<string>();
                                                Uploader.SaveDocx(item.Documents.Split(','), absolutePath, relativePath, "PGI", ImageFormat.Jpeg, ref Pictures);
                                                var imageCount = 0;
                                                foreach (var image in Pictures)
                                                {
                                                    CarDocument cd = new CarDocument();
                                                    cd.CarID = car.ID;
                                                    cd.Path = image;
                                                    cd.Name = "Papers";
                                                    if (!_carDocumentService.CreateDocument(ref cd, ref message))
                                                    {
                                                    }
                                                }
                                            }

                                            car = null;
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
                                        //	/* IF Car is VARIATION*/
                                        //	Car car = _carService.GetCarbySKU(item.SKU);
                                        //	if (item.Type == "Variable" && item.VariantSKU != null)
                                        //	{
                                        //		/*Upload Car Variation Thumnail*/
                                        //		string filePath = string.Empty;
                                        //		if (!string.IsNullOrEmpty(item.VariantThumbnail))
                                        //		{
                                        //			filePath = item.VariantThumbnail;

                                        //			message = string.Empty;

                                        //			string absolutePath = Server.MapPath("~");
                                        //			string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/Variations/{1}/", car.SKU.Replace(" ", "_"), item.VariantSKU.Replace(" ", "_"));

                                        //			item.VariantThumbnail = Uploader.SaveImage(item.VariantThumbnail, absolutePath, relativePath, "PVTI", ImageFormat.Jpeg);

                                        //		}

                                        //		_carVariationService.PostExcelData(car.ID, item.VariantRegularPrice, item.VariantSalePrice, item.VariantSalePriceFrom, item.VariantSalePriceTo, item.VariantStock, item.VariantThreshold, item.VariantStockStatus == "In Stock" ? 1 : 2, item.VariantSKU, item.VariantThumbnail, item.VariantWeight, item.VariantLength, item.VariantWidth, item.VariantHeight, item.VariantDescription, item.VariantSoldIndividually == "Yes" ? true : false, item.VariantIsManageStock == "Yes" ? true : false);

                                        //	}

                                        //	CarVariation objCarVariation = _carVariationService.GetCarbySKU(item.VariantSKU);

                                        //	/*Creating Car Variation Attributes Images*/
                                        //	if (item.VariantAttributes != null)
                                        //	{
                                        //		string[] values = item.VariantAttributes.Split(',');
                                        //		for (int i = 0; i < values.Count(); i++)
                                        //		{
                                        //			string[] val1 = values[i].Split(':');
                                        //			if (val1 != null && val1.Count() > 1)
                                        //			{
                                        //				var result = _carVariationAttributeService.PostExcelData(car.ID, val1[0], val1[1], objCarVariation.ID);
                                        //			}
                                        //		}
                                        //	}

                                        //	/*Uploading Car Variation Images*/
                                        //	if (!string.IsNullOrEmpty(item.VariantImages))
                                        //	{
                                        //		message = string.Empty;
                                        //		string absolutePath = Server.MapPath("~");

                                        //		string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/Variations/{1}/Gallery/", car.SKU.Replace(" ", "_"), item.VariantSKU.Replace(" ", "_"));

                                        //		List<string> Pictures = new List<string>();

                                        //		Uploader.SaveImages(item.VariantImages.Split(','), absolutePath, relativePath, "PVGI", ImageFormat.Jpeg, ref Pictures);
                                        //		var imageCount = 0;
                                        //		foreach (var image in Pictures)
                                        //		{
                                        //			CarVariationImage carVariationImage = new CarVariationImage();
                                        //			carVariationImage.CarID = car.ID;
                                        //			carVariationImage.CarVariationID = objCarVariation.ID;
                                        //			carVariationImage.Image = image;
                                        //			carVariationImage.Position = ++imageCount;
                                        //			if (_carVariationImageService.CreateCarVariationImage(ref carVariationImage, ref message))
                                        //			{
                                        //			}
                                        //		}
                                        //	}

                                        //	objCarVariation = null;
                                        //	successCount++;
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
                            System.IO.File.Delete(targetpath + filename);
                            return Json(new
                            {
                                success = true,
                                successMessage = string.Format("{0} Motors uploaded!", (successCount)),
                                errorMessage = (ErrorItems.Count() > 0) ? string.Format("{0} Motors are not uploaded!", total - successCount) : null,
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

        #endregion

        #region Approval
        public ActionResult Approvals()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        public ActionResult CarAdIndex()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.MissingDataErrorMessage = TempData["MissingDataErrorMessage"];
            return View();
        }

        public ActionResult CarAdList()
        {
            var cars = _carService.GetAd();
            return View(cars);
        }
        [HttpGet]
        public ActionResult ApprovalsList()
        {
            var cars = _carService.GetCars().OrderByDescending(x => x.ID);
            var List = _vendorService.GetVendorsForMotors(true);
            var dropdownList = (from item in List.AsEnumerable()
                                select new SelectListItem
                                {
                                    Text = item.Name,
                                    Value = item.ID.ToString()
                                }).ToList();
            //dropdownList.Add(new SelectListItem() { Text = "Select Vendor", Value = null });
            ViewBag.Vendor = new SelectList(dropdownList, "Value", "Text");
            return PartialView(cars);
        }
        //[HttpPost]
        //public ActionResult ApprovalsList(long VendorId)
        //{
        //    var cars = _carService.GetCarsForApprovalByVendor(VendorId).OrderByDescending(x => x.ID);
        //    var List = _vendorService.GetVendorsForMotors(true);
        //    var dropdownList = (from item in List.AsEnumerable()
        //                        select new SelectListItem
        //                        {
        //                            Text = item.Name,
        //                            Value = item.ID.ToString()
        //                        }).ToList();
        //    //dropdownList.Add(new SelectListItem() { Text = "Select Vendor", Value = null });
        //    ViewBag.Vendor = new SelectList(dropdownList, "Value", "Text");
        //    return PartialView(cars);
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
            var Cars = _carService.GetCarsApprovalsByVendors(model.length, model.start, sortBy, sortDir, searchBy, vendorid, true, filter);

            int filteredResultsCount = Cars != null && Cars != null && Cars.Count() > 0 ? (int)Cars.FirstOrDefault().FilteredResultsCount : 0;
            int totalResultsCount = Cars != null && Cars.Count() > 0 ? (int)Cars.FirstOrDefault().TotalResultsCount : 0;
            
            return Json(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = Cars
            });
        }

        [HttpGet]
        public ActionResult Reject(long id)
        {
            ViewBag.BuildingID = id;
            ViewBag.ApprovalStatus = 4;

            var car = _carService.GetCar((long)id);

            return View(car);
        }

        [HttpGet]
        public ActionResult Approve(CarApprovalFormViewModel carApprovalFormViewModel)
        {

            var car = _carService.GetCar((int)carApprovalFormViewModel.ID);
            if (car == null)
            {
                return HttpNotFound();
            }
            car.IsActive = true;
            car.IsApproved = true;
            car.ApprovalStatus = 3;
            car.IsActive = true;

            car.Remarks = car.Remarks + "<hr />" + Helpers.TimeZone.GetLocalDateTime().ToString("dd MMM yyyy, h:mm tt") + "<br />" + carApprovalFormViewModel.Remarks;

            string message = string.Empty;

            if (_carService.UpdateCar(ref car, ref message, false))
            {
                SuccessMessage = "Motor " + ((bool)car.IsActive ? "Approved" : "Rejected") + "  successfully ...";

                if (car.VendorID != null)
                {
                    var vendor = _vendorService.GetVendor((long)car.VendorID);

                    Notification not = new Notification();

                    if (car.ApprovalStatus == 3)
                    {
                        not.Title = "Motor Approval";
                        not.TitleAr = "الموافقة على المنتج";
                        not.Description = "Your motor " + car.SKU + " have been approved ";
                        not.Url = "/Vendor/Car/Index";
                    }
                    else
                    {
                        not.Title = "Motor Rejected";
                        not.TitleAr = "سيارة مرفوضة";
                        not.Description = "Your " + car.SKU + " have been rejected ";
                        not.Url = "/Vendor/Car/ApprovalIndex";
                    }
                    not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                    not.OriginatorName = Session["UserName"].ToString();
                    not.Module = "Car";
                    not.OriginatorType = "Admin";
                    not.RecordID = car.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        NotificationReceiver notRec = new NotificationReceiver();
                        notRec.ReceiverID = car.VendorID;
                        notRec.ReceiverType = "Vendor";
                        notRec.NotificationID = not.ID;
                        if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                        {
                            var tokens = _vendorSessionService.GetVendorSessionFirebaseTokens(car.VendorID.Value);
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
                            ID = car.ID,
                            Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            Vendor = vendor.Logo + "|" + vendor.VendorCode + "|" + vendor.Name,
                            Car = car.Name + "|" + car.SKU,
                            ApprovalStatus = car.ApprovalStatus,
                            IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString,
                            IsApproved = car.IsApproved.HasValue ? car.IsApproved.Value.ToString() : bool.FalseString
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, message = SuccessMessage }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }
            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(CarApprovalFormViewModel carApprovalFormViewModel)
        {

            var car = _carService.GetCar(carApprovalFormViewModel.ID);
            if (car == null)
            {
                return HttpNotFound();
            }

            if (carApprovalFormViewModel.IsApproved)
            {
                car.IsApproved = true;
                car.IsActive = true;
                car.ApprovalStatus = (int)CarApprovalStatus.Approved;
            }
            else
            {
                car.IsApproved = false;
                car.IsActive = false;
                car.ApprovalStatus = (int)CarApprovalStatus.Rejected;
            }
            car.ApprovalStatus = 4;
            car.Remarks = car.Remarks + "<hr />" + Helpers.TimeZone.GetLocalDateTime().ToString("dd MMM yyyy, h:mm tt") + "<br />" + carApprovalFormViewModel.Remarks;

            string message = string.Empty;

            if (_carService.UpdateCar(ref car, ref message, false))
            {
                SuccessMessage = "Motor " + ((bool)car.IsActive ? "Approved" : "Rejected") + "  successfully ...";

                if (car.VendorID != null)
                {
                    var vendor = _vendorService.GetVendor((long)car.VendorID);

                    Notification not = new Notification();

                    if (car.ApprovalStatus == 3)
                    {
                        not.Title = "Motor Approval";
                        not.TitleAr = "الموافقة على المنتج";
                        not.Description = "Your motor " + car.SKU + " have been approved ";
                        not.Url = "/Vendor/Car/Index";
                    }
                    else
                    {
                        not.Title = "Motor Rejected";
                        not.TitleAr = "سيارة مرفوضة";
                        not.Description = "Your " + car.SKU + " have been rejected ";
                        not.Url = "/Vendor/Car/ApprovalIndex";
                    }
                    not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                    not.OriginatorName = Session["UserName"].ToString();
                    not.Module = "Car";
                    not.OriginatorType = "Admin";
                    not.RecordID = car.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        NotificationReceiver notRec = new NotificationReceiver();
                        notRec.ReceiverID = car.VendorID;
                        notRec.ReceiverType = "Vendor";
                        notRec.NotificationID = not.ID;
                        if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                        {
                            var tokens = _vendorSessionService.GetVendorSessionFirebaseTokens(car.VendorID.Value);
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
                            ID = car.ID,
                            Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            Vendor = vendor.Logo + "|" + vendor.VendorCode + "|" + vendor.Name,
                            Car = car.Name + "|" + car.SKU,
                            ApprovalStatus = car.ApprovalStatus,
                            IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString,
                            IsApproved = car.IsApproved.HasValue ? car.IsApproved.Value.ToString() : bool.FalseString
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, message = SuccessMessage }, JsonRequestBehavior.AllowGet);
                }

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
                var car = _carService.GetCar(items);

                car.IsApproved = true;
                car.IsActive = true;
                car.ApprovalStatus = (int)CarApprovalStatus.Approved;
                car.IsActive = true;


                //car.Remarks = car.Remarks + "<hr />" + Helpers.TimeZone.GetLocalDateTime().ToString("dd MMM yyyy, h:mm tt") + "<br />" + carApprovalFormViewModel.Remarks;

                string message = string.Empty;

                if (_carService.UpdateCar(ref car, ref message, false))
                {
                    SuccessMessage = "Motor " + ((bool)car.IsActive ? "Approved" : "Rejected") + "  successfully ...";

                    if (car.VendorID != null)
                    {

                        var vendor = _vendorService.GetVendor((long)car.VendorID);
                        Notification not = new Notification();
                        not.Title = "Motor Approval";
                        not.TitleAr = "الموافقة على المنتج";
                        if (car.ApprovalStatus == 3)
                        {
                            not.Description = "Your motor " + car.SKU + " have been approved ";
                            not.Url = "/Vendor/Car/Index";
                        }
                        else
                        {
                            not.Description = "Your " + car.SKU + " have been rejected ";
                            not.Url = "/Vendor/Car/ApprovalIndex";
                        }
                        not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                        not.OriginatorName = Session["UserName"].ToString();
                        not.Module = "Car";
                        not.OriginatorType = "Admin";
                        not.RecordID = car.ID;
                        if (_notificationService.CreateNotification(not, ref message))
                        {
                            NotificationReceiver notRec = new NotificationReceiver();
                            notRec.ReceiverID = car.VendorID;
                            notRec.ReceiverType = "Vendor";
                            notRec.NotificationID = not.ID;
                            if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                            {
                                var tokens = _vendorSessionService.GetVendorSessionFirebaseTokens(car.VendorID.Value);
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
                        else
                        {
                            ErrorMessage = "Oops! Something went wrong. Please try later.";
                        }
                    }
                }
            }
            // return RedirectToAction("Index");
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult RejectAll(List<long> ids)
        {
            foreach (var items in ids)
            {
                var car = _carService.GetCar(items);

                car.IsApproved = false;
                car.IsActive = false;
                car.ApprovalStatus = (int)CarApprovalStatus.Rejected;


                //car.Remarks = car.Remarks + "<hr />" + Helpers.TimeZone.GetLocalDateTime().ToString("dd MMM yyyy, h:mm tt") + "<br />" + carApprovalFormViewModel.Remarks;

                string message = string.Empty;

                if (_carService.UpdateCar(ref car, ref message, false))
                {
                    SuccessMessage = "Motor " + ((bool)car.IsActive ? "Approved" : "Rejected") + "  successfully ...";

                    if (car.VendorID != null)
                    {

                        var vendor = _vendorService.GetVendor((long)car.VendorID);
                        Notification not = new Notification();
                        not.Title = "Motor Rejection";
                        not.TitleAr = "الموافقة على المنتج";
                        if (car.ApprovalStatus == 4)
                        {
                            not.Description = "Your motor " + car.SKU + " have been rejected ";
                            not.Url = "/Vendor/Car/ApprovalIndex";
                        }
                        else
                        {
                            not.Description = "Your " + car.SKU + " have been rejected ";
                            not.Url = "/Vendor/Car/ApprovalIndex";
                        }
                        not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                        not.OriginatorName = Session["UserName"].ToString();
                        not.Module = "Car";
                        not.OriginatorType = "Admin";
                        not.RecordID = car.ID;
                        if (_notificationService.CreateNotification(not, ref message))
                        {
                            NotificationReceiver notRec = new NotificationReceiver();
                            notRec.ReceiverID = car.VendorID;
                            notRec.ReceiverType = "Vendor";
                            notRec.NotificationID = not.ID;
                            if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                            {
                                var tokens = _vendorSessionService.GetVendorSessionFirebaseTokens(car.VendorID.Value);
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
                    {
                        return Json(new { success = true, message = SuccessMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    ErrorMessage = "Oops! Something went wrong. Please try later.";
                }
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var car = _carService.GetCar((long)id);
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
            if (_carService.UpdateCar(ref car, ref message, ForceApproval: true))
            {
                SuccessMessage = "Motor " + ((bool)car.IsActive ? "activated" : "deactivated") + "  successfully ...";
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

        #endregion

        #region Reports

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApprovalReport()
        {
            var getAllApprovals = _carService.GetCars().ToList();
            if (getAllApprovals.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("ApprovalReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Vendor Code"
                        ,"Vendor Name"
                        ,"SKU"
                        ,"Car Name"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["ApprovalReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllApprovals)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Vendor.VendorCode) ? i.Vendor.VendorCode : "-"
                        ,!string.IsNullOrEmpty(i.Vendor.Name) ? i.Vendor.Name : "-"
                        ,!string.IsNullOrEmpty(i.SKU) ? i.SKU : "-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
                        ,i.ApprovalStatus != null ? ((CarApprovalStatus)i.ApprovalStatus).ToString() : "-"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Motors Approval Report.xlsx");
                }
            }
            return RedirectToAction("Approvals");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdApprovalReport()
        {
            var getAllApprovals = _carService.GetAd().ToList();
            if (getAllApprovals.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("AdReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Vendor Code"
                        ,"Vendor Name"
                        ,"SKU"
                        ,"Car Name"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["AdReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllApprovals)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Vendor.VendorCode) ? i.Vendor.VendorCode : "-"
                        ,!string.IsNullOrEmpty(i.Vendor.Name) ? i.Vendor.Name : "-"
                        ,!string.IsNullOrEmpty(i.SKU) ? i.SKU : "-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
                        ,i.ApprovalStatus != null ? ((CarApprovalStatus)i.ApprovalStatus).ToString() : "-"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Motors Approval Report.xlsx");
                }
            }
            return RedirectToAction("Approvals");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CarsReport()
        {
            long? vendorId = null;
            string ImageServer = CustomURL.GetImageServer();
            var getAllCars = _carService.GetAllForReport().ToList();
            var getAllCarsVariations = _carService.GetDetailCarsVariations(ImageServer, vendorId).ToList();
            if (getAllCars.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("Motors");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Name"
                        ,"NameAr"
                        ,"Slug"
                        ,"ChasisNumber"
                        ,"Category"
                        ,"CarMake"
                        ,"BodyType"
                        ,"MotorModel"
                        ,"Doors"
                        ,"Cylinders"
                        ,"HorsePower"
                        ,"RegionalSpecification"
                        ,"LicensePlate"
                        ,"Year"
                        ,"Capacity"
                        ,"Transmission"
                        ,"FuelEconomy"
                        /*,"EnableDelivery"*/
                        ,"RegularPrice"
                        /*,"ChargesType"*/
                        /*,"DeliveryCharges"*/
                        ,"Motor Thumbnail"
                        ,"Motor Images"
                        /*,"Specification"*/
                        /*,"SpecificationAr"*/
                        /*,"IsFeatured"
                        ,"IsPublished"*/
                        ,"MotorFeatures"
                        ,"Country"
                        ,"City"
                        ,"Warranty End Date"
                        ,"fuel Type"
                        ,"Area"
                        ,"Address"
                        ,"Documents"
                        ,"Condition"
                        ,"Sale Price"
                        ,"Description"
                        ,"Description AR"
                        /*,"MotorTags"*/
                        /*,"HourlyKilometer"
                        ,"HourlyPrice"*/
                        /*,"DailyKilometer"
                        ,"DailyPrice"*/
                       /* ,"WeeklyKilometer"
                        ,"WeeklyPrice"*/
                       /* ,"MonthlyKilometer"
                        ,"MonthlyPrice"*/
                        /*,"InsuranceName"
                        ,"InsuranceNameAr"
                        ,"InsuranceDetails"
                        ,"InsuranceDetailsAr"*/
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:AX1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["Motors"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllCars)
                    {
                        /*var carInsurance = _insuranceService.GetInsuranceByCarID((long)i.ID).FirstOrDefault();
                        var carPackages = _carPackageService.GetPackageByCarID((long)i.ID).OrderBy(x => x.PackageID).ToList();*/
                        cellData.Add(new object[]
                        {
						//i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-",
						 !string.IsNullOrEmpty(i.Name) ? i.Name : "-"
                        ,!string.IsNullOrEmpty(i.NameAr) ? i.NameAr : "-"
                        ,!string.IsNullOrEmpty(i.Slug) ? i.Slug : "-"
                        ,!string.IsNullOrEmpty(i.SKU) ? i.SKU : "-"
                        ,i.Category !=  null ? (!string.IsNullOrEmpty(i.Category.CategoryName) ? i.Category.CategoryName : "-") : "-"
                        ,i.CarMake != null ? (!string.IsNullOrEmpty(i.CarMake.Name) ? i.CarMake.Name : "-") : "-"
                        ,i.BodyType != null ? (!string.IsNullOrEmpty(i.BodyType.Name) ? i.BodyType.Name : "-") : "-"
                        ,i.CarModel != null ? ( !string.IsNullOrEmpty(i.CarModel.Name) ? i.CarModel.Name : "-") : "-"
                        ,!string.IsNullOrEmpty(i.Doors) ? i.Doors : "-"
                        ,!string.IsNullOrEmpty(i.Cylinders) ? i.Cylinders : "-"
                        ,!string.IsNullOrEmpty(i.HorsePower) ? i.HorsePower : "-"
                        ,!string.IsNullOrEmpty(i.RegionalSpecification) ? i.RegionalSpecification : "-"
                        ,i.LicensePlate != null ? (!string.IsNullOrEmpty(i.LicensePlate) ? i.LicensePlate : "-") : "-"
                        ,i.Year != null ? (!string.IsNullOrEmpty(i.Year) ? i.Year : "-") : "-"
                        ,i.Capacity != null ? (!string.IsNullOrEmpty(i.Capacity) ? i.Capacity : "-") : "-"
                        ,i.Transmission != null ? (!string.IsNullOrEmpty(i.Transmission) ? i.Transmission : "-") : "-"
                        ,i.FuelEconomy != null ? (!string.IsNullOrEmpty(i.FuelEconomy) ? i.FuelEconomy : "-") : "-"
                        ,i.RegularPrice != null ? (!string.IsNullOrEmpty(i.RegularPrice.ToString()) ? i.RegularPrice.ToString() : "-") : "-"
                        /*,i.EnableDelivery == true ? "Yes" : "No"*/
                       /* ,!string.IsNullOrEmpty(i.chargesType) ? i.chargesType : "-"*/
                        /*,!string.IsNullOrEmpty(i.DeliveryCharges.ToString()) ? i.DeliveryCharges.ToString() : "-"*/
                        ,i.Thumbnail != null ? (!string.IsNullOrEmpty(i.Thumbnail.ToString()) ? i.Thumbnail.ToString() : "-") : "-"
                        ,i.CarImages != null ? (!string.IsNullOrEmpty(i.CarImages.ToString()) ? i.CarImages.ToString() : "-") : "-"
                       /* ,i.Specification != null ? (!string.IsNullOrEmpty(i.Specification) ? i.Specification : "-") : "-"
                        ,i.SpecificationAr != null ? (!string.IsNullOrEmpty(i.SpecificationAr) ? i.SpecificationAr : "-") : "-"*/
                        /*,i.IsFeatured == true ? "Yes" : "No"
                        ,i.IsPublished == true ? "Yes" : "No"*/
                        ,!string.IsNullOrEmpty(i.CarFeatures.ToString()) ? i.CarFeatures.ToString() : "-"
                        /*,!string.IsNullOrEmpty(i.Tags) ? i.Tags : "-"*/
                        ,i.Country != null ? (!string.IsNullOrEmpty(i.Country.Name) ? i.Country.Name : "-") : "-"
                        ,i.City != null ? (!string.IsNullOrEmpty(i.City.Name) ? i.City.Name : "-") : "-"
                        ,i.WarrentyEndDate != null ? (!string.IsNullOrEmpty(i.WarrentyEndDate.ToString()) ? i.WarrentyEndDate.ToString() : "-") : "-"
                        ,i.FuelType != null ? (!string.IsNullOrEmpty(i.FuelType) ? i.FuelType : "-") : "-"
                        ,i.Area != null ? (!string.IsNullOrEmpty(i.Area) ? i.Area : "-") : "-"
                        ,i.Address != null ? (!string.IsNullOrEmpty(i.Address) ? i.Address : "-") : "-"
                        ,i.CarDocuments != null  ? (!string.IsNullOrEmpty(i.CarDocuments.ToString()) ? i.CarDocuments.ToString() : "-") : "-"
                        ,i.Condition != null ? (!string.IsNullOrEmpty(i.Condition) ? i.Condition : "-") : "-"
                        ,i.SalePrice != null ? (!string.IsNullOrEmpty(i.SalePrice.ToString()) ? i.SalePrice.ToString() : "-") : "-"
                        ,i.LongDescription != null ? (!string.IsNullOrEmpty(i.LongDescription.ToString()) ? i.LongDescription.ToString() : "-") : "-"
                        ,i.LongDescriptionAr != null ? (!string.IsNullOrEmpty(i.LongDescriptionAr.ToString()) ? i.LongDescriptionAr.ToString() : "-") : "-"
                        /*,carPackages.Count()>0 && carPackages[0].Price.HasValue? carPackages[0].Price.Value.ToString(): ""*/
                        /*,carPackages.Count()>0 && carPackages[0].Kilometer.HasValue? carPackages[0].Kilometer.Value.ToString(): ""*/

                        /*,carPackages.Count()>1 && carPackages[1].Price.HasValue? carPackages[1].Price.Value.ToString(): ""
                        ,carPackages.Count()>1 && carPackages[1].Kilometer.HasValue? carPackages[1].Kilometer.Value.ToString(): ""*/

                       /* ,carPackages.Count()>2 && carPackages[2].Price.HasValue? carPackages[2].Price.Value.ToString(): ""
                        ,carPackages.Count()>2 && carPackages[2].Kilometer.HasValue? carPackages[2].Kilometer.Value.ToString(): ""*/

                        /*,carPackages.Count()>3 && carPackages[3].Price.HasValue? carPackages[3].Price.Value.ToString(): ""
                        ,carPackages.Count()>3 && carPackages[3].Kilometer.HasValue? carPackages[3].Kilometer.Value.ToString(): ""*/

                        /*,carInsurance !=null && !string.IsNullOrEmpty(carInsurance.Name) ? carInsurance.Name : "-"
                        ,carInsurance !=null &&!string.IsNullOrEmpty(carInsurance.NameAr) ? carInsurance.NameAr : "-"
                        ,carInsurance !=null &&!string.IsNullOrEmpty(carInsurance.Details) ? carInsurance.Details : "-"
                        ,carInsurance !=null &&!string.IsNullOrEmpty(carInsurance.DetailsAr) ? carInsurance.DetailsAr : "-"*/



                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Motor Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

        #endregion
    }
}