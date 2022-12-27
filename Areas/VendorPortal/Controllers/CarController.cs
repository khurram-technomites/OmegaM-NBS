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

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    [AuthorizeVendor]
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
        private readonly ICarVariationService _carVariationService;
        private readonly ICarVariationImageService _carVariationImageService;
        private readonly ICarCategoryService _carCategoryService;
        private readonly ICarTagService _carTagService;
        private readonly ICarAttributeService _carAttributeService;
        private readonly ICarVariationAttributeService _carVariationAttributeService;
        private readonly ICarImageService _carImageService;
        private readonly ICategoryService _categoryService;
        private readonly IBodyTypeService _bodyTypeService;
        private readonly ICarMakeService _carMakeService;
        private readonly ICarModelService _carModelService;
        private readonly IPackagesService _packagesService;
        private readonly ICarPackageService _carPackageService;
        private readonly ICarInsuranceService _insuranceService;
        private readonly ICarDocumentService _carDocumentService;
        private readonly IVendorPackagesService _vendorPackagesService;
        private readonly IFeatureService _featureService;
        private readonly ICarFeatureService _carFeatureService;
        private readonly ICountryService _countryService;
        private readonly ICityService _cityService;
        private readonly INumberRangeService _numberRangeService;
        private readonly IVendorAddPurchasesService _VendorAddPurchasesService;

        public CarController(ICountryService countryService
            , ICarVariationAttributeService carVariationAttributeService
            , ICarAttributeService carAttributeService
            , ICarTagService carTagService
            , ICarCategoryService carCategoryService
            , ICarVariationService carVariationService
            , ICarVariationImageService carVariationImageService
            , ICarService carService
            , IVendorService vendorService
            , IAttributeService attributeService
            , INotificationService notificationService
            , INotificationReceiverService notificationReceiverService
            , IBrandsService brandService
            , ICarImageService carImageService
            , ICategoryService categoryService
            , IBodyTypeService bodyTypeService
            , ICarMakeService carMakeService
            , ICarModelService carModelService
            , IPackagesService packagesService
            , ICarPackageService carPackageService
            , ICarInsuranceService insuranceService
            , ICarDocumentService carDocumentService
            , IVendorPackagesService vendorPackagesService
            , IFeatureService featureService
            , ICarFeatureService carFeatureService
            , ICityService cityService
            , INumberRangeService numberRangeService, IVendorAddPurchasesService vendorAddPurchasesService)
        {
            this._carVariationAttributeService = carVariationAttributeService;
            this._carAttributeService = carAttributeService;
            this._carTagService = carTagService;
            this._carCategoryService = carCategoryService;
            this._carVariationService = carVariationService;
            this._carVariationImageService = carVariationImageService;
            this._carService = carService;
            this._vendorService = vendorService;
            this._attributeService = attributeService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
            this._brandService = brandService;
            this._carImageService = carImageService;
            this._categoryService = categoryService;
            this._bodyTypeService = bodyTypeService;
            this._carMakeService = carMakeService;
            this._carModelService = carModelService;
            this._packagesService = packagesService;
            this._carPackageService = carPackageService;
            this._insuranceService = insuranceService;
            this._carDocumentService = carDocumentService;
            this._vendorPackagesService = vendorPackagesService;
            this._featureService = featureService;
            this._carFeatureService = carFeatureService;
            _numberRangeService = numberRangeService;
            this._VendorAddPurchasesService = vendorAddPurchasesService;

            _countryService = countryService;
            this._cityService = cityService;
        }

        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        public ActionResult List()
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);
            var list = _carService.GetApprovedListByVendor(vendorId);
            return PartialView(list);
        }

        [HttpPost]
        public JsonResult List(DataTableAjaxPostModel model,string filter)
        {

            var vendorId = Convert.ToInt64(Session["VendorID"]);

            var searchBy = (model.search != null) ? model.search.value : "";
            int sortBy = 0;
            string sortDir = "";

            if (model.order != null)
            {
                sortBy = model.order[0].column;
                sortDir = model.order[0].dir.ToLower();
            }
            var Cars = _carService.GetVendorCars(model.length, model.start, sortBy, sortDir, searchBy, vendorId,true,filter);

            int filteredResultsCount = Cars != null && Cars.Count() > 0 ? (int)Cars.FirstOrDefault().FilteredResultsCount : 0;
            int totalResultsCount = Cars != null && Cars.Count() > 0 ? (int)Cars.FirstOrDefault().TotalResultsCount : 0;

            return Json(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = Cars
            });
        }

        public ActionResult CarApprovalIndex()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.MissingDataErrorMessage = TempData["MissingDataErrorMessage"];
            return View();
        }

        public ActionResult ApprovalIndex()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.MissingDataErrorMessage = TempData["MissingDataErrorMessage"];
            return View();
        }

        public ActionResult CarApprovalList()
        {
            return PartialView();
        }

        public ActionResult ApprovalList()
        {
            int vendorId = Convert.ToInt32(Session["VendorID"]);

            var feature = _carService.GetListByVendor(vendorId);
            return PartialView(feature);
        }

        [HttpPost]
        public JsonResult ApprovalList(DataTableAjaxPostModel model)
        {

            var vendorId = Convert.ToInt64(Session["VendorID"]);

            var searchBy = (model.search != null) ? model.search.value : "";
            int sortBy = 0;
            string sortDir = "";

            if (model.order != null)
            {
                sortBy = model.order[0].column;
                sortDir = model.order[0].dir.ToLower();
            }
            var Cars = _carService.GetVendorCarsUnapproved(model.length, model.start, sortBy, sortDir, searchBy, vendorId, false);

            int filteredResultsCount = Cars != null && Cars.Count() > 0 ? (int)Cars.FirstOrDefault().FilteredResultsCount : 0;
            int totalResultsCount = Cars != null && Cars.Count() > 0 ? (int)Cars.FirstOrDefault().TotalResultsCount : 0;

            return Json(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = Cars
            });
        }

        public ActionResult ListReport()
        {
            var cars = _carService.GetCars();
            return View(cars);
        }

        /*public ActionResult List()
		{
			int vendorId = Convert.ToInt32(Session["VendorID"]);

			var feature = _propService.GetApprovedListByVendor(vendorId);
			return PartialView(feature);
		}*/

        public ActionResult Details(long? id)
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
            ViewBag.AttributeID = new SelectList(_attributeService.GetAttributesForDropDown(), "value", "text");
            ViewBag.BrandID = new SelectList(_brandService.GetBrandsForDropDown(), "value", "text", car.BrandID);

            return View(car);
        }

        public ActionResult Create(long? id)
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

            ViewBag.AttributeID = new SelectList(_attributeService.GetAttributesForDropDown(), "value", "text");
            ViewBag.BrandID = new SelectList(_brandService.GetBrandsForDropDown(), "value", "text");
            //TempData["CarID"] = id;
            return View(car);
        }

        public ActionResult QuickCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuickCreate(Car car)
        {
            string message = string.Empty;
            string message2 = string.Empty;
            Vendor vendor = _vendorService.GetVendor(Convert.ToInt32(Session["VendorID"]));
            string numberRange = _numberRangeService.GetNextValueFromNumberRangeByName("CAR");
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

                if (packagelimit.MotorLimit + ExtraLimit > Vendorlimit)
                {
                    if (ModelState.IsValid)
                    {
                        car.VendorID = vendor.ID;
                        car.Status = "1";
                        car.ApprovalStatus = 1;
                        car.Type = "1";
                        car.Slug = Slugify.GenerateSlug(car.Name + "-" + numberRange);
                        car.AdsReferenceCode = _numberRangeService.GetNextValueFromNumberRangeByName("MOTORREFERENCE");
                        CarInsurance insurance = new CarInsurance();
                        if (_carService.CreateCar(car, ref message))
                        {
                            insurance.CarID = car.ID;
                            if (_insuranceService.CreateInsurance(ref insurance, ref message2))
                            {

                            }
                            return Json(new
                            {
                                success = true,
                                url = "/Vendor/Car/Edit/" + car.ID,
                                message = message
                            });
                        }
                        else
                        {
                            message = "Motor with same license plate already exists.";
                        }
                    }
                    else
                    {
                        message = "Please fill the form properly ...";
                    }
                }
                else
                {

                    message = "Your limit has been exceeded.";
                }
            }
            else
            {
                message = "Your package has been expired.";
            }
            return Json(new
            {
                success = false,
                messageswl = message,

            });
            /*return View();*/
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

            if (car.WarrentyEndDate.HasValue)
                car.WarrentyEndDate = car.WarrentyEndDate.Value.Date;

            return View(car);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Update(Car car)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                var currentCar = _carService.GetCar(car.ID);

                currentCar.Slug = currentCar.Slug.Replace(Slugify.GenerateSlug(currentCar.Name), Slugify.GenerateSlug(car.Name));

                currentCar.Name = car.Name;
                currentCar.NameAr = car.NameAr;

                currentCar.CarMakeID = car.CarMakeID;
                currentCar.CarModelID = car.CarModelID;
                currentCar.CategoryID = car.CategoryID;
                currentCar.BodyTypeID = car.BodyTypeID;

                currentCar.SKU = car.SKU;
                currentCar.Year = car.Year;
                currentCar.FuelEconomy = car.FuelEconomy;
                currentCar.SalePrice = car.SalePrice;

                currentCar.FuelType = car.FuelType;
                currentCar.Transmission = car.Transmission;
                currentCar.Doors = car.Doors;
                currentCar.Wheels = car.Wheels;
                currentCar.Capacity = car.Capacity;
                currentCar.SteeringSide = car.SteeringSide;
                currentCar.Condition = car.Condition;
                currentCar.MechanicalCondition = car.MechanicalCondition;
                currentCar.Cylinders = car.Cylinders;

                currentCar.Warranty = car.Warranty;
                currentCar.ServiceHistory = car.ServiceHistory;

                currentCar.HorsePower = car.HorsePower;
                currentCar.RegionalSpecification = car.RegionalSpecification;
                currentCar.EngineDisplacement = car.EngineDisplacement;

                currentCar.LongDescription = car.LongDescription;
                currentCar.LongDescriptionAr = car.LongDescriptionAr;

                currentCar.IsPublished = car.IsPublished;

                currentCar.ModifiedDate = Helpers.TimeZone.GetLocalDateTime();

                car = null;

                if (_carService.UpdateCar(ref currentCar, ref message))
                {
                    _carService.UpdateCarApprovalStatus(currentCar.ID);

                    return Json(new
                    {
                        success = true,
                        //url = "/Admin/Car/Index",
                        message = "Motor updated successfully ...",
                        //data = new
                        //{
                        //    ID = car.ID,
                        //    Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        //    SKU = car.SKU,
                        //    Name = car.Name,
                        //    LongDescription = car.LongDescription,
                        //    Remark = car.Remarks,
                        //    IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString
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
                string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/", car.AdsReferenceCode);

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
                string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/Video/", car.AdsReferenceCode);

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
        [HttpPost]
        public ActionResult ToggleIsSold(int ID)
        {
            string featuremsg = string.Empty;
            var prop = _carService.GetCar(ID);
            if (prop == null)
            {
                return HttpNotFound();
            }

            if (prop.IsSold.HasValue)
            {
                if (prop.IsSold.Value)
                {
                    prop.IsSold = false;
                    featuremsg = "Motor set to un-sold successfully...";
                }
                else
                {
                    prop.SoldDate = NowBuySell.Service.Helpers.TimeZone.GetLocalDateTime();
                    prop.IsSold = true;
                    featuremsg = "Motor sold successfully...";
                }
            }
            else
            {
                prop.SoldDate = NowBuySell.Service.Helpers.TimeZone.GetLocalDateTime();
                prop.IsSold = true;
                featuremsg = "Motor sold successfully...";
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

        public ActionResult Publish(long? id)
        {
            List<string> ErrorItems = new List<string>();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var car = _carService.GetCar((long)id);
            if (car == null)
            {
                return HttpNotFound();
            }
            if (car.IsPublished != true)
            {

                //if (car.CarDocuments.Count < 1)
                //{
                //    ErrorItems.Add(string.Format("Motor Documents Are Not Inserted.<br>"));
                //}
                if (car.CarImages.Count < 1)
                {
                    ErrorItems.Add(string.Format("Motor Images Are Not Inserted.<br>"));
                }
                if (car.CarFeatures.Count < 1)
                {
                    ErrorItems.Add(string.Format("Motor features Are Not Inserted.<br>"));
                }
                /*if (car.CarInsurances.FirstOrDefault().Name == null)
				{
					ErrorItems.Add(string.Format("Car Insurance is Not Inserted.<br>"));
				}*/
                if (car.Thumbnail == null)
                {
                    ErrorItems.Add(string.Format("Motor Thumbnail Not Inserted.<br>"));
                }

                if (car.CarMakeID == null || car.BodyTypeID == null || car.CarModelID == null || car.Doors == null || car.Cylinders == null ||
                car.HorsePower == null || car.RegionalSpecification == null || car.Year == null || car.Capacity == null ||
                car.Transmission == null || car.FuelEconomy == null || car.SalePrice == null || car.Address == null || car.Latitude == null || car.Longitude == null)

                {
                    ErrorItems.Add(string.Format("Motor Details Are Not Inserted Properly.<br>"));
                }
                /*if (car.CategoryID == null || car.CarMakeID == null || car.BodyTypeID == null || car.CarModelID == null || car.Doors == null || car.Cylinders == null || car.HorsePower == null || car.RegionalSpecification == null || car.PricePerKilometer == null || car.LicensePlate == null || car.Year == null || car.Capacity == null || car.Transmission == null || car.FuelEconomy == null || car.Specification == null || car.RegularPrice == null || car.SpecificationAr == null || car.TermsAndCondition == null || car.TermsAndConditionAr == null || car.CancelationPolicy == null || car.CancelationPolicyAr == null || car.EnableDelivery == null)

				{
					ErrorItems.Add(string.Format("Car Details Are Not Inserted Properly.<br>"));
				}*/

                //if (car.EnableDelivery == false && car.ChargesType != null && car.DeliveryChargesAmount != null)
                //{
                //    ErrorItems.Add(string.Format("Motor Delivery Details Are Not Inserted Properly.<br>"));
                //}

                /*		if (car.EnableDelivery == null)
                        {
                            ErrorItems.Add(string.Format("Car Details Are Not Inserted Properly.<br>"));

                        }*/
            }
            if (ErrorItems.Count() < 1)
            {
                if (car.IsPublished.HasValue)
                {
                    if (car.IsPublished.Value)
                    {
                        car.IsPublished = false;
                    }
                    else
                    {
                        car.IsPublished = true;
                        car.PublishDate = Helpers.TimeZone.GetLocalDateTime();
                    }
                }
                else
                {
                    car.IsPublished = true;
                    car.PublishDate = Helpers.TimeZone.GetLocalDateTime();
                }

                string message = string.Empty;
                if (_carService.UpdateCar(ref car, ref message, ForceApproval: true))
                {
                    SuccessMessage = "Motor " + (car.IsPublished.Value ? "Published" : "Unpublished") + "  successfully ...";
                    return Json(new
                    {
                        success = true,
                        message = SuccessMessage,
                        data = new
                        {
                            IsPublished = car.IsPublished.HasValue ? car.IsPublished.Value.ToString() : bool.FalseString,
                            ID = car.ID
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

            //return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
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

        public ActionResult IsApproved(long? id)
        {
            List<string> ErrorItems = new List<string>();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var car = _carService.GetCar((long)id);
            if (car == null)
            {
                return HttpNotFound();
            }
            //if (car.CarDocuments.Count < 1)
            //{
            //	ErrorItems.Add(string.Format("Car Documents Are Not Inserted.<br>"));
            //}
            if (car.CarImages.Count < 1)
            {
                ErrorItems.Add(string.Format("Motor Images Are Not Inserted.<br>"));
            }
            if (car.CarFeatures.Count < 1)
            {
                ErrorItems.Add(string.Format("Motor features Are Not Inserted.<br>"));
            }
            //if (car.CarInsurances.FirstOrDefault().Name == null)
            //{
            //	ErrorItems.Add(string.Format("Car Insurance is Not Inserted.<br>"));
            //}
            if (car.Thumbnail == null)
            {
                ErrorItems.Add(string.Format("Motor Thumbnail Not Inserted.<br>"));
            }
            if (car.CarMakeID == null || car.BodyTypeID == null || car.CarModelID == null || car.Doors == null || car.Cylinders == null ||
                car.HorsePower == null || car.RegionalSpecification == null || car.Year == null || car.Capacity == null ||
                car.Transmission == null || car.FuelEconomy == null || car.SalePrice == null || car.Address == null || car.Latitude == null || car.Longitude == null)

            {
                ErrorItems.Add(string.Format("Motor Details Are Not Inserted Properly.<br>"));
            }

            //if (car.EnableDelivery == false && car.ChargesType != null && car.DeliveryChargesAmount != null)
            //{
            //	ErrorItems.Add(string.Format("Car Delivery Details Are Not Inserted Properly.<br>"));

            //}

            //if (car.EnableDelivery == null)
            //{
            //	ErrorItems.Add(string.Format("Car Details Are Not Inserted Properly.<br>"));

            //}


            //if (ca == null)
            //{
            //	ErrorItems.Add(string.Format("Car Images Are Not Inserted.<br>"));
            //}
            if (ErrorItems.Count() < 1)
            {
                if (car.IsApproved.HasValue)
                {
                    car.IsApproved = false;
                    car.Status = "2";
                    car.ApprovalStatus = 2;
                }

                string message = string.Empty;
                if (_carService.UpdateCar(ref car, ref message, ForceApproval: true))
                {
                    Notification not = new Notification();
                    not.Title = "Motor Approval";
                    not.Description = "New prodouct added for approval ";
                    not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                    not.OriginatorName = Session["VendorUserName"].ToString();
                    not.Url = "/Admin/Car/Approvals";
                    not.Module = "Car";
                    not.OriginatorType = "Vendor";
                    not.RecordID = car.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", not.OriginatorID))
                        {
                        }
                    }
                    //SuccessMessage =  /*"Car " + (car.IsApproved.Value ? "Approved" : "disapproved") + "  successfully ..."*/;
                    SuccessMessage = "Motor approval request sent successfully... ";

                    return Json(new
                    {
                        success = true,
                        message = SuccessMessage,
                        data = new
                        {
                            IsApproved = car.IsApproved.HasValue ? car.IsApproved.Value.ToString() : bool.FalseString,
                            ID = car.ID
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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var car = _carService.GetCar((long)id);
            if (car == null)
            {
                return HttpNotFound();
            }

            if (car.Remarks != null)
            {
                /*if (car.IsApproved.HasValue)
                {*/
                car.IsApproved = false;
                car.ApprovalStatus = 4;
                car.Status = "4";
                /* }*/
            }

            else if (car.Remarks == null)
            {
                /* if (car.IsApproved.HasValue)
                 {*/
                car.IsApproved = false;
                car.ApprovalStatus = 1;
                car.Status = "1";
                /*}*/
            }

            string message = string.Empty;
            if (_carService.UpdateCar(ref car, ref message, ForceApproval: true))
            {
                SuccessMessage = "Motor approval request canceled successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        IsApproved = car.IsApproved.HasValue ? car.IsApproved.Value.ToString() : bool.FalseString,
                        ID = car.ID
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Remarks(long id)
        {
            ViewBag.BuildingID = id;
            var car = _carService.GetCar((long)id);


            return View(car);
        }

        public ActionResult BulkUpload()
        {

            return View();

        }

        [HttpPost]
        public ActionResult BulkUpload(HttpPostedFileBase FileUpload, string connectionId)
        {
            try
            {

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
                            long VendorID = (long)Session["VendorID"];


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

                                       
                                        Car currentCar = _carService.GetByReferenceCode(item.AdsReferenceCode, VendorID);
                                        if (currentCar == null)
                                        {
                                            item.AdsReferenceCode = _numberRangeService.GetNextValueFromNumberRangeByName("MOTORREFERENCE");
                                        }
                                        string filePath = string.Empty;
                                        if (!string.IsNullOrEmpty(item.Thumbnail))
                                        {
                                            filePath = item.Thumbnail;

                                            message = string.Empty;
                                            item.SKU = item.ChasisNo;
                                            string absolutePath = Server.MapPath("~");
                                            string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/", item.AdsReferenceCode.Replace(" ", "_"));

                                            item.Thumbnail = Uploader.SaveImage(item.Thumbnail, absolutePath, relativePath, "PTI", ImageFormat.Jpeg);

                                        }
                                        //item.AdsReferenceCode= _numberRangeService.GetNextValueFromNumberRangeByName("MOTORREFERENCE");


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
                                        item.BodyCondition = item.BodyCondition!=null? item.BodyCondition.Replace("to", "-"):null;
                                        if (_carService.PostExcelData(VendorID
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
                                                                        , 1
                                                                        , false
                                                                        , Convert.ToDecimal(item.Latitude)
                                                                        , Convert.ToDecimal(item.Longitude)
                                                                        , item.Area
                                                                        , item.City
                                                                        ))
                                        {
                                            Car car = _carService.GetByReferenceCode(item.AdsReferenceCode,VendorID);
                                            if (!string.IsNullOrEmpty(item.Video))
                                            {

                                                string absolutePath = Server.MapPath("~");
                                                string relativePath = string.Format("/Assets/AppFiles/Images/Cars/{0}/", car.ID.ToString().Replace(" ", "_"));
                                                message = string.Empty;
                                                string Pictures = "";
                                                Uploader.SaveVideos(item.Video, absolutePath, relativePath, "PGI", ref Pictures, "Video" );
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
                                                    cd.Name= "Papers";
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

        [HttpPost]
        public ActionResult BulkSendForApproval(List<long> ids)
        {
            try
            {
                List<string> ErrorItems = new List<string>();
                foreach (var items in ids)
                {
                    bool flag = false;
                    var car = _carService.GetCar(items);
                    //ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> <br><br>", car.Name));
                    //if (car.CarDocuments.Count < 1)
                    //{
                    //	ErrorItems.Add(string.Format(" <strong><u>{0} </u></strong> Documents Are Not Inserted.<br>", car.Name));
                    //	flag = true;
                    //}

                    if (car.CarImages.Count < 1)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Images Are Not Inserted.<br>", car.Name));
                        flag = true;
                    }
                    if (car.CarFeatures.Count < 1)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong>features Are Not Inserted.<br>", car.Name));
                        flag = true;
                    }
                    //if (car.CarInsurances.FirstOrDefault().Name == null)
                    //{
                    //	ErrorItems.Add(string.Format("Car Insurance is Not Inserted.<br>"));
                    //}
                    if (car.Thumbnail == null)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Thumbnail Not Inserted.<br>", car.Name));
                        flag = true;
                    }
                    if (car.CarMakeID == null || car.BodyTypeID == null || car.CarModelID == null || car.Doors == null || car.Cylinders == null ||
                        car.HorsePower == null || car.RegionalSpecification == null || car.Year == null || car.Capacity == null ||
                        car.Transmission == null || car.FuelEconomy == null || car.SalePrice == null || car.Address == null || car.Latitude == null || car.Longitude == null)

                    {
                        ErrorItems.Add(string.Format("Motor Details Are Not Inserted Properly.<br>"));
                    }
                    //{
                    //    ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Details Are Not Inserted Properly.<br>", car.Name));
                    //    flag = true;
                    //}

                    if (car.CarImages.Count < 1)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Images Are Not Inserted.<br>", car.Name));
                        flag = true;
                    }
                    if (car.CarFeatures.Count < 1)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong>features Are Not Inserted.<br>", car.Name));
                        flag = true;
                    }



                    if (!flag)
                    {
                        //ErrorItems.Add(string.Format("Sent successfully<br>"));
                        if (car != null && car.IsApproved.HasValue)
                        {
                            car.IsApproved = false;
                            car.Status = "2";
                            car.ApprovalStatus = (int)CarApprovalStatus.Processing;

                            string message = string.Empty;

                            if (_carService.UpdateCar(ref car, ref message, false))
                            {
                                Notification not = new Notification();
                                not.Title = "Motor Approval";
                                not.TitleAr = "الموافقة على المنتج";
                                not.Description = "New prodouct added for approval ";
                                not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                                not.OriginatorName = Session["VendorUserName"].ToString();
                                not.Url = "/Admin/Car/Approvals";
                                not.Module = "Car";
                                not.OriginatorType = "Vendor";
                                not.RecordID = car.ID;
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
                        message = "Selected motors are sent for approval."
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

        [HttpPost]
        public ActionResult BulkPublish(List<long> ids)
        {

            try
            {
                List<string> ErrorItems = new List<string>();
                foreach (var items in ids)
                {
                    bool flag = false;
                    var car = _carService.GetCar(items);
                    //ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> <br><br>", car.Name));

                    if (car.CarImages.Count < 1)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Images Are Not Inserted.<br>", car.Name));
                        flag = true;
                    }
                    if (car.CarFeatures.Count < 1)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong>features Are Not Inserted.<br>", car.Name));
                        flag = true;
                    }
                    //if (car.CarInsurances.FirstOrDefault().Name == null)
                    //{
                    //	ErrorItems.Add(string.Format("Car Insurance is Not Inserted.<br>"));
                    //}
                    if (car.Thumbnail == null)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Thumbnail Not Inserted.<br>", car.Name));
                        flag = true;
                    }
                    if (car.CarMakeID == null || car.BodyTypeID == null || car.CarModelID == null || car.Doors == null || car.Cylinders == null ||
                        car.HorsePower == null || car.RegionalSpecification == null || car.Year == null || car.Capacity == null ||
                        car.Transmission == null || car.FuelEconomy == null || car.SalePrice == null || car.Address == null || car.Latitude == null || car.Longitude == null)

                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Details Are Not Inserted Properly.<br>", car.Name));
                        flag = true;
                    }

                    if (car.CarImages.Count < 1)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong> Images Are Not Inserted.<br>", car.Name));
                        flag = true;
                    }
                    if (car.CarFeatures.Count < 1)
                    {
                        ErrorItems.Add(string.Format("<strong><u>{0} </u></strong>features Are Not Inserted.<br>", car.Name));
                        flag = true;
                    }
                    if (!flag)
                    {
                        if (car != null && (car.IsActive.HasValue && car.IsActive.Value == true) && (car.IsApproved.HasValue && car.IsApproved.Value == true))
                        {

                            car.IsPublished = true;
                            car.PublishDate = Helpers.TimeZone.GetLocalDateTime();
                            string message = string.Empty;
                            _carService.UpdateCar(ref car, ref message, false, ForceApproval: true);
                        }
                    }
                }
                if (ErrorItems.Count == 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Selected motors are published."
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

        [HttpPost]
        public ActionResult BulkUnPublish(List<long> ids)
        {
            try
            {
                foreach (var items in ids)
                {
                    var car = _carService.GetCar(items);

                    if (car != null)
                    {
                        car.IsPublished = false;

                        string message = string.Empty;
                        _carService.UpdateCar(ref car, ref message, false, ForceApproval: true);
                    }
                }

                return Json(new
                {
                    success = true,
                    message = "Selected cars are unpublished."
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

        [HttpGet]
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

        public ActionResult CreateInsurance(long id)
        {
            TempData["CarID"] = id;
            return View();

        }

        [HttpPost]
        public ActionResult CreateInsurance(CarInsurance insurance)
        {
            string message = string.Empty;
            insurance.CarID = (long)TempData["CarID"];
            if (insurance != null)
            {
                if (_insuranceService.CreateInsurance(ref insurance, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        url = "/Vendor/Car/Edit",
                        message = message,
                        data = new
                        {
                            ID = insurance.ID,
                            //Date = area.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            Name = insurance.Name != null ? insurance.Name : "",
                            NameAr = insurance.NameAr != null ? insurance.Name : "",
                            Price = insurance.Price,
                            IsActive = insurance.IsActive.HasValue ? insurance.IsActive.Value.ToString() : bool.FalseString
                        }
                    });
                }
            }
            return View();
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

        public ActionResult EditInsurance(long? id)
        {
            var data = _insuranceService.GetInsuranceByID((long)id);


            return View(data);
        }

        [HttpPost]
        public ActionResult EditInsurance(CarInsurance insurance)
        {
            string message = string.Empty;
            //insurance.CarID = (long)TempData["CarID"];
            CarInsurance data = _insuranceService.GetInsuranceByID((long)insurance.ID);
            data.Price = insurance.Price;
            data.Details = insurance.Details;
            data.DetailsAr = insurance.DetailsAr;
            data.Name = insurance.Name;
            data.NameAr = insurance.NameAr;
            if (insurance != null)
            {
                if (_insuranceService.UpdateInsurance(ref data, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        url = "/Vendor/Car/Edit",
                        message = message,
                        data = new
                        {
                            ID = data.ID,
                            //Date = area.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            Name = data.Name != null ? insurance.Name : "",
                            NameAr = data.NameAr != null ? insurance.Name : "",
                            car = data.Car.Name,
                            IsActive = data.IsActive.HasValue ? data.IsActive.Value.ToString() : bool.FalseString
                        }
                    });
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteInsuranceConfirmed(long id)
        {
            string message = string.Empty;
            if (_insuranceService.DeleteInsurance((Int16)id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CarActivation(long? id)
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
                //var city = _cityService.GetCity((long)area.CityID);
                //var country = _countryService.GetCountry((long)area.CountryID);
                SuccessMessage = "motor " + ((bool)car.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString,
                        ID = car.ID
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var area = _insuranceService.GetInsuranceByID((long)id);
            if (area == null)
            {
                return HttpNotFound();
            }

            if (!(bool)area.IsActive)
                area.IsActive = true;
            else
            {
                area.IsActive = false;
            }
            string message = string.Empty;
            if (_insuranceService.UpdateInsurance(ref area, ref message))
            {
                //var city = _cityService.GetCity((long)area.CityID);
                //var country = _countryService.GetCountry((long)area.CountryID);
                SuccessMessage = "Insurance " + ((bool)area.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        ID = area.ID,
                        //Date = area.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        Name = area.Name != null ? area.Name : "",
                        NameAr = area.NameAr != null ? area.Name : "",
                        Price = area.Price,
                        IsActive = area.IsActive.HasValue ? area.IsActive.Value.ToString() : bool.FalseString
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
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
                return Json(new
                {
                    success = true,
                    message = message,
                    data = data
                    //CarPackages = carPackages
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = false,
                message = "Document not added!",
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

        #region Reports

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApprovalReport()
        {
            var vendorId = Convert.ToInt64(Session["VendorID"]);
            bool isApproved = false;

            var getAllApprovals = _carService.GetCarsUnApproved(vendorId, isApproved).ToList();
            if (getAllApprovals.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("ApprovalReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Chasis /Vin Number"
                        ,"Name"
                        ,"Categories"
                        ,"Status"
                        }
                    };

                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    var worksheet = excel.Workbook.Worksheets["ApprovalReport"];

                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllApprovals)
                    {
                        var categories = "-";
                        if (i.CarCategories.Count() == 0)
                            categories = string.Join(", ", i.CarCategories.Select(x => x.Category.CategoryName));

                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.SKU) ? i.SKU : "-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
                        ,!string.IsNullOrEmpty(categories) ? categories : "-"
                        ,i.ApprovalStatus != null ? ((CarApprovalStatus)i.ApprovalStatus).ToString() : "-"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Motors Approval Report.xlsx");
                }
            }
            return RedirectToAction("ApprovalIndex");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CarsReport()
        {
            var vendorId = Convert.ToInt64(Session["VendorID"]);
            string ImageServer = CustomURL.GetImageServer();
            var getAllCars = _carService.GetApprovedListByVendor(vendorId).ToList();
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
                        ,"MotorMake"
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
                        /*,i.Specification != null ? (!string.IsNullOrEmpty(i.Specification) ? i.Specification : "-") : "-"
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

        public ActionResult AddCarMake()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddCarMake(CarMake carmake)
        {
            string message = string.Empty;
            carmake.VendorID = (long)Session["VendorID"];
            //carmake.IsApproved = false;
            if (_carMakeService.CreateCarMake(carmake, ref message))
            {
                var data = _vendorService.GetVendor((long)carmake.VendorID);
                Notification not = new Notification();
                not.Title = "Motor Make";
                not.TitleAr = "Motor Make";
                not.Description = string.Format("New Motor Make has been added by {0}", data.Name);
                not.DescriptionAr = string.Format("New Motor Make has been added by {0}", data.Name);
                not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                not.OriginatorName = Session["VendorUserName"].ToString();
                not.Module = "CarMake";
                not.Url = "/Admin/CarMake/Index";
                not.OriginatorType = "Vendor";
                not.RecordID = carmake.ID;
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
                    id = carmake.ID,
                    value = carmake.Name,
                    message = "Motor Make request sent!",

                });
            }


            return Json(new { success = false, message = message });
        }

        public ActionResult AddBodyType()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddBodyType(BodyType bodytype)
        {
            string message = string.Empty;
            bodytype.VendorID = (long)Session["VendorID"];
            //carmake.IsApproved = false;
            if (_bodyTypeService.CreateBodyType(bodytype, ref message))
            {
                var data = _vendorService.GetVendor((long)bodytype.VendorID);
                Notification not = new Notification();
                not.Title = "Motor Make";
                not.TitleAr = "Motor Make";
                not.Description = string.Format("New Body Type has been added by {0}", data.Name);
                not.DescriptionAr = string.Format("New Body Type has been added by {0}", data.Name);
                not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                not.OriginatorName = Session["VendorUserName"].ToString();
                not.Module = "BodyType";
                not.Url = "/Admin/BodyType/Index";
                not.OriginatorType = "Vendor";
                not.RecordID = bodytype.ID;
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
                    id = bodytype.ID,
                    value = bodytype.Name,
                    message = "Body type request sent!",

                });
            }


            return Json(new { success = false, message = message });
        }

        public ActionResult AddCarModel()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddCarModel(CarModel carmodel)
        {
            string message = string.Empty;
            carmodel.VendorID = (long)Session["VendorID"];
            //carmake.IsApproved = false;
            if (_carModelService.CreateCarModel(ref carmodel, ref message))
            {
                var data = _vendorService.GetVendor((long)carmodel.VendorID);
                Notification not = new Notification();
                not.Title = "Motor Model";
                not.TitleAr = "Motor Model";
                not.Description = string.Format("New Motor Model has been added by {0}", data.Name);
                not.DescriptionAr = string.Format("New Motor Model has been added by {0}", data.Name);
                not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                not.OriginatorName = Session["VendorUserName"].ToString();
                not.Module = "CarModel";
                not.Url = "/Admin/CarModel/Index";
                not.OriginatorType = "Vendor";
                not.RecordID = carmodel.ID;
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
                    id = carmodel.ID,
                    value = carmodel.Name,
                    message = "Motor Model request sent!",

                });
            }


            return Json(new { success = false, message = message });
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
            feature.Category = "Car";
            string message = string.Empty;
            feature.VendorID = (long)Session["VendorID"];
            //carmake.IsApproved = false;
            if (_featureService.CreateFeature(feature, ref message))
            {
                CarFeature carFeature = new CarFeature();
                carFeature.CarID = CarID;
                carFeature.FeatureID = feature.ID;
                if (_carFeatureService.CreateCarFeature(ref carFeature, ref message))
                {
                    var data = _vendorService.GetVendor((long)feature.VendorID);
                    Notification not = new Notification();
                    not.Title = "Motor Feature";
                    not.TitleAr = "Motor Feature";
                    not.Description = string.Format("New Motor Feature has been added by {0}", feature.Name);
                    not.DescriptionAr = string.Format("New Motor Feature has been added by {0}", feature.Name);
                    not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                    not.OriginatorName = Session["VendorUserName"].ToString();
                    not.Module = "CarFeature";
                    not.Url = "/Admin/Feature/Index";
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
                        FeatureID = carFeature.ID,
                        message = "Motor Feature request sent!",

                    });
                }
            }


            return Json(new { success = false });
        }
        #endregion
    }
}