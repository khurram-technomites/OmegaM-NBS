using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.Pricing;
using NowBuySell.Web.ViewModels.Api.Vendor.Motor;
using NowBuySell.Web.ViewModels.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api.vendorApi
{
    [Authorize(Roles = "Vendor")]
    [RoutePrefix("api/v1/vendor")]
    public class VendorCarController : ApiController
    {
        string ImageServer = CustomURL.GetImageServer();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICarService _carService;
        private readonly ICarMakeService _carMakeService;
        private readonly ICarModelService _carModelService;
        private readonly IBodyTypeService _carBodyTypeService;
        private readonly ICarPackageService _carPackageService;
        private readonly ICarFeatureService _carFeatureService;
        private readonly ICategoryService _categoryService;
        private readonly ICarImageService _carImageService;
        private readonly ICarInsuranceService _carInsuranceService;
        private readonly IVendorService _vendorService;
        private readonly ICarTagService _carTagService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;
        private readonly INumberRangeService _numberRangeService;
        private readonly IVendorPackagesService _vendorPackagesService;
        private readonly IVendorAddPurchasesService _VendorAddPurchasesService;

        public VendorCarController(ICarService carService
            , ICategoryService categoryService
            , ICarMakeService carMakeService
            , ICarModelService carModelService
            , IBodyTypeService bodyTypeService
            , ICarPackageService carPackageService
            , ICarFeatureService carFeatureService
            , ICarImageService carImageService
            , ICarInsuranceService carInsuranceService
            , IVendorService vendorService
            , ICarTagService tagService
            , INotificationReceiverService notificationReceiverService
            , INotificationService notificationService,
            INumberRangeService numberRangeService,
            IVendorPackagesService vendorPackagesService,
            IVendorAddPurchasesService vendorAddPurchasesService)
        {
            this._carService = carService;
            this._categoryService = categoryService;
            this._carMakeService = carMakeService;
            this._carModelService = carModelService;
            this._carBodyTypeService = bodyTypeService;
            this._carPackageService = carPackageService;
            this._carFeatureService = carFeatureService;
            this._carImageService = carImageService;
            this._carInsuranceService = carInsuranceService;
            this._vendorService = vendorService;
            this._carTagService = tagService;
            _notificationReceiverService = notificationReceiverService;
            _notificationService = notificationService;
            _numberRangeService = numberRangeService;
            _vendorPackagesService = vendorPackagesService;
            _VendorAddPurchasesService = vendorAddPurchasesService;
        }

        [HttpGet]
        [Route("cars")]
        public HttpResponseMessage GetAllCars(int pg = 1)
        {
            HTMLTagsSplitter splitHelper = new HTMLTagsSplitter();
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                long vendorId = 0;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {
                    int pageSize = 20;
                    string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
                    if (ReceiverType == "Vendor")
                    {
                        vendorId = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
                    }
                    var getVendorCars = _carService.GetVendorCarsByVendorID(vendorId);
                    
                    if (getVendorCars.Count() > 0)
                    {

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            cars = getVendorCars.OrderByDescending(x => x.ID).Select(i => new
                            {
                                i.ID,
                                Thumbnail = !string.IsNullOrEmpty(i.Thumbnail) ? ImageServer + i.Thumbnail : "",
                                i.Name,
                                i.NameAr,
                                i.SKU,
                                i.LicensePlate,
                                remarks = splitHelper.ToSplitList(i.Remarks),
                                i.IsActive,
                                i.ApprovalStatus,
                                i.IsPublished,
                                regionspecification = i.RegionalSpecification,
                                i.LongDescription,
                                i.LongDescriptionAr,
                                year = i.Year,
                                i.RegularPrice,
                                i.SalePrice,
                                i.Warranty,
                                i.ServiceHistory,
                                i.Wheels,
                                i.RegionalSpecification,
                                i.MechanicalCondition,
                                EngineDisplacement = !string.IsNullOrEmpty(i.EngineDisplacement) ? i.EngineDisplacement.Replace(" CC", "") : "",
                                i.SteeringSide,
                                i.IsFeatured,
                                //i.Transmission,
                                i.AdsReferenceCode,
                                //inspection =  new 
                                //{
                                //   Id= i.CarInspections.Count() > 0 ? i.CarInspections.FirstOrDefault().ID : 0,
                                //   Image = i.CarInspections.Count() > 0 ? ImageServer + i.CarInspections.FirstOrDefault().Path : null,
                                //},
                                inspection=i.CarInspections.Count() == 0 ? null : new
                                {
                                    Id = i.CarInspections.Count() > 0 ? i.CarInspections.FirstOrDefault().ID : 0,
                                    Image = i.CarInspections.Count() > 0 ? ImageServer + i.CarInspections.FirstOrDefault().Path : null,
                                },

                                //transmission = i.Transmission,
                                transmission = new
                                {
                                    id = 0,
                                    name = i.Transmission
                                },
                                Condition = new
                                {
                                    id = 0,
                                    name = i.Condition
                                },
                                fueleconomy = i.FuelEconomy,
                                fueltype = new
                                {
                                    id = 0,
                                    name = i.FuelType
                                },
                                capacity = new
                                {
                                    id = 0,
                                    name = i.Capacity
                                },
                                Doors = new
                                {
                                    id = 0,
                                    name = i.Doors
                                },
                                Cylinders = new
                                {
                                    id = 0,
                                    name = i.Cylinders
                                },
                                HorsePower = new
                                {
                                    id = 0,
                                    name = i.HorsePower
                                },
                                Category = i.Category != null ? new
                                {
                                    id = i.CategoryID,
                                    name = i.Category.CategoryName
                                } : null,

                                CarMake = i.CarMake != null ? new
                                {
                                    id = i.CarMakeID,
                                    name = i.CarMake.Name
                                } : null,
                                CarModel = i.CarModel != null ? new
                                {
                                    id = i.CarModelID,
                                    name = i.CarModel.Name
                                } : null,
                                BodyType = i.BodyType != null ? new
                                {
                                    id = i.BodyTypeID,
                                    name = i.BodyType.Name
                                } : null,
                                Country = i.Country != null ? new
                                {
                                    id = i.CountryID,
                                    name = i.Country.Name
                                } : null,
                                City = i.City != null ? new
                                {
                                    id = i.CityID,
                                    name = i.City.Name
                                } : null,
                                i.Longitude,
                                i.Latitude,
                                i.Address,
                                i.ZipCode,
                                i.Area,
                                i.IsVerified,
                                i.IsPremium,
                                Video = new
                                {
                                    video = string.IsNullOrEmpty(i.Video) ? null : ImageServer + i.Video
                                },
                                images = i.CarImages.Select(j => new
                                {
                                    id = j.ID,
                                    image = !string.IsNullOrEmpty(j.Image) ? ImageServer + j.Image : null,
                                }),
                                document = i.CarDocuments.Select(j => new
                                {
                                    j.ID,
                                    image = !string.IsNullOrEmpty(j.Path) ? ImageServer + j.Path : null,
                                }),
                                features = _carFeatureService.GetAllFeatureByCarID(i.ID, ImageServer)
                            }).Skip(pageSize * (pg - 1)).Take(pageSize).ToList()
                        });
                    }

                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "error",
                            message = "No car available"

                        });
                    }
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = "Session invalid or expired !"
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpPost]
        [Route("cars")]
        public HttpResponseMessage Insert(CarViewModel carViewModel)
        {
            try
            {
                Vendor vendor = new Vendor();
                DateTime currentDateTime = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                long vendorId = 0;
                if (claims.Count() == 4 && Int64.TryParse(claims.FirstOrDefault().Value, out vendorUserId))
                {

                    string message = string.Empty;
                    string featureMessage = string.Empty;

                    string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
                    if (ReceiverType == "Vendor")
                    {
                        vendorId = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
                        vendor = _vendorService.GetVendor(vendorId);
                    }

                    if (ModelState.IsValid)
                    {
                        int Vendorlimit = _carService.GetVendorLimit(vendorId);
                        var packagelimit = _vendorPackagesService.GetPackageLimit(vendor.VendorPackageID.Value);
                        int ExtraLimit = _VendorAddPurchasesService.GetVendorLimit(currentDateTime, (int)vendor.ID);
                        if (currentDateTime.Date < vendor.PackageEndDate.Value.Date)
                        {

                            if (packagelimit.MotorLimit + ExtraLimit > Vendorlimit)
                            {
                                Car car = new Car();

                                string numberRange = _numberRangeService.GetNextValueFromNumberRangeByName("CAR");

                                car.AdsReferenceCode = _numberRangeService.GetNextValueFromNumberRangeByName("MOTORREFERENCE");

                                switch (carViewModel.Transmission)
                                {
                                    case "Manual transmission":
                                        carViewModel.Transmission = "Manual";
                                        break;
                                    case "Automatic transmission":
                                        carViewModel.Transmission = "Automatic";
                                        break;
                                    case "Continuously variable transmission (CVT)":
                                        carViewModel.Transmission = "CVT";
                                        break;
                                    case "Semi-automatic and dual-clutch transmissions":
                                        carViewModel.Transmission = "DCT";
                                        break;
                                }

                                car.Slug = Slugify.GenerateSlug(car.Name + "-" + numberRange);

                                car.Name = carViewModel.Name;
                                car.NameAr = carViewModel.NameAr;

                                car.CarMakeID = carViewModel.CarMakeID;
                                car.CarModelID = carViewModel.CarModelID;
                                car.CategoryID = carViewModel.CategoryID;
                                car.BodyTypeID = carViewModel.BodyTypeID;

                                //car.RegularPrice = carViewModel.RegularPrice;
                                car.SKU = carViewModel.SKU;
                                car.Year = carViewModel.Year;
                                car.FuelEconomy = carViewModel.FuelEconomy;
                                car.SalePrice = carViewModel.SalePrice;

                                car.FuelType = carViewModel.FuelType;
                                car.Transmission = carViewModel.Transmission;
                                car.Doors = carViewModel.Doors;
                                car.Wheels = carViewModel.Wheels;
                                car.Capacity = carViewModel.Capacity;
                                car.SteeringSide = carViewModel.SteeringSide;
                                car.Condition = carViewModel.Condition;
                                car.MechanicalCondition = carViewModel.MechanicalCondition;
                                car.Cylinders = carViewModel.Cylinders;
                                car.Area = carViewModel.Area;
                                car.Warranty = carViewModel.Warranty;
                                car.ServiceHistory = carViewModel.ServiceHistory;

                                car.HorsePower = carViewModel.HorsePower;
                                car.RegionalSpecification = carViewModel.RegionalSpecification;
                                car.EngineDisplacement = carViewModel.EngineDisplacementVolumes + " CC";

                                car.LongDescription = carViewModel.LongDescription;
                                car.LongDescriptionAr = carViewModel.LongDescriptionAr;

                                car.IsActive = true;
                                car.IsPublished = false;
                                car.IsDeleted = false;
                                car.ApprovalStatus = 2;
                                car.VendorID = vendorId;

                                car.CityID = carViewModel.CityID;
                                car.Address = carViewModel.Address;
                                car.Latitude = carViewModel.Latitude;
                                car.Longitude = carViewModel.Longitude;


                                foreach (var features in carViewModel.CarFeatures)
                                {
                                    car.CarFeatures.Add(new CarFeature()
                                    {
                                        FeatureID = features.FeatureID
                                    });
                                }

                                if (_carService.CreateCar(car, ref message))
                                {

                                    Notification not = new Notification();
                                    not.Title = "Car Approval";
                                    not.Description = "New car added for approval";
                                    not.OriginatorID = vendorId;
                                    not.OriginatorName = "";
                                    not.Url = "/Admin/Car/Approvals";
                                    not.Module = "Car";
                                    not.OriginatorType = "Customer";
                                    not.RecordID = car.ID;
                                    if (_notificationService.CreateNotification(not, ref message))
                                    {
                                        if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", not.OriginatorID))
                                        {
                                        }
                                    }

                                    return Request.CreateResponse(HttpStatusCode.OK, new
                                    {
                                        status = "success",
                                        message = "Motor created successfully",
                                        car = new
                                        {
                                            id = car.ID,
                                            Name = car.Name,
                                            NameAr = car.NameAr
                                        }
                                    });
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new
                                    {
                                        status = "error",
                                        message = message
                                    });
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

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "error",
                            message = message
                        });

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new
                        {
                            status = "error",
                            message = "Bad request!",
                            description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = "Session invalid or expired!"
                    });
                }
            }
            catch (Exception ex)
            {
                //log.Error("Error", ex);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpPut]
        [Route("cars/{carId}")]
        public HttpResponseMessage Update(long carId, CarViewModel carViewModel)
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                long vendorId = 0;
                if (claims.Count() == 4 && Int64.TryParse(claims.FirstOrDefault().Value, out vendorUserId))
                {

                    string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
                    if (ReceiverType == "Vendor")
                    {
                        vendorId = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
                    }

                    Car car = _carService.GetCar(carId);
                    if (car == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, new
                        {
                            status = "error",
                            message = "Car not found!"
                        });
                    }

                    string message = string.Empty;
                    if (ModelState.IsValid)
                    {
                        IEnumerable<CarFeature> carFeatures = _carFeatureService.GetCarFeature(carId);

                        foreach (var entry in carFeatures)
                        {
                            _carFeatureService.DeleteCarFeature(entry.ID, ref message);
                        }

                        switch (carViewModel.Transmission)
                        {
                            case "Manual transmission":
                                carViewModel.Transmission = "Manual";
                                break;
                            case "Automatic transmission":
                                carViewModel.Transmission = "Automatic";
                                break;
                            case "Continuously variable transmission (CVT)":
                                carViewModel.Transmission = "CVT";
                                break;
                            case "Semi-automatic and dual-clutch transmissions":
                                carViewModel.Transmission = "DCT";
                                break;
                        }

                        car.Slug = car.Slug.Replace(Slugify.GenerateSlug(car.Name), Slugify.GenerateSlug(carViewModel.Name));

                        car.Name = carViewModel.Name;
                        car.NameAr = carViewModel.NameAr;

                        car.CarMakeID = carViewModel.CarMakeID;
                        car.CarModelID = carViewModel.CarModelID;
                        car.CategoryID = carViewModel.CategoryID;
                        car.BodyTypeID = carViewModel.BodyTypeID;

                        //car.RegularPrice = carViewModel.RegularPrice;
                        car.SKU = carViewModel.SKU;
                        car.Year = carViewModel.Year;
                        car.FuelEconomy = carViewModel.FuelEconomy;
                        car.SalePrice = carViewModel.SalePrice;

                        car.FuelType = carViewModel.FuelType;
                        car.Transmission = carViewModel.Transmission;
                        car.Doors = carViewModel.Doors;
                        car.Wheels = carViewModel.Wheels;
                        car.Capacity = carViewModel.Capacity;
                        car.SteeringSide = carViewModel.SteeringSide;
                        car.Condition = carViewModel.Condition;
                        car.MechanicalCondition = carViewModel.MechanicalCondition;
                        car.Cylinders = carViewModel.Cylinders;
                        car.Area = carViewModel.Area;
                        car.Warranty = carViewModel.Warranty;
                        car.ServiceHistory = carViewModel.ServiceHistory;

                        car.HorsePower = carViewModel.HorsePower;
                        car.RegionalSpecification = carViewModel.RegionalSpecification;
                        car.EngineDisplacement = carViewModel.EngineDisplacementVolumes + " CC";

                        car.LongDescription = carViewModel.LongDescription;
                        car.LongDescriptionAr = carViewModel.LongDescriptionAr;

                        car.IsActive = true;
                        car.IsPublished = false;
                        car.IsDeleted = false;
                        car.ApprovalStatus = 2;
                        car.VendorID = vendorId;

                        car.CityID = carViewModel.CityID;
                        car.Address = carViewModel.Address;
                        car.Latitude = carViewModel.Latitude;
                        car.Longitude = carViewModel.Longitude;


                        foreach (var features in carViewModel.CarFeatures)
                        {
                            car.CarFeatures.Add(new CarFeature()
                            {
                                FeatureID = features.FeatureID
                            });
                        }

                        if (_carService.UpdateCar(ref car, ref message))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = "Car updated successfully",
                                Data = new
                                {
                                    car.ID,
                                    Name = car.Name,
                                    NameAr = car.NameAr
                                }
                            });
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "error",
                            message = message
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new
                        {
                            status = "error",
                            message = "Bad request !",
                            description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = "Session invalid or expired!"
                    });
                }
            }
            catch (Exception ex)
            {
                //log.Error("Error", ex);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpGet]
        [Route("cars/{carId}")]
        public HttpResponseMessage GetCarDetails(long carId)
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {
                    string lang = "en";
                    string ImageServer = CustomURL.GetImageServer();
                    var car = _carService.GetCar(carId);
                    if (car == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, new
                        {
                            status = "error",
                            message = "Car Not Found"
                        });
                    }

                    var carFeature = _carFeatureService.GetAllFeatureByCarID(carId, ImageServer);
                    var carMake = car.CarMakeID.HasValue ? _carMakeService.GetCarMake((long)car.CarMakeID) : null;
                    var carModel = car.CarModelID.HasValue ? _carModelService.GetCarModel((long)car.CarModelID) : null;
                    var bodyType = car.BodyTypeID.HasValue ? _carBodyTypeService.GetBodyType((long)car.BodyTypeID) : null;
                    var carImage = _carImageService.GetCarImages(carId);
                    var vendor = _vendorService.GetVendor((long)car.VendorID);
                    var carTags = _carTagService.GetAllTagsByCarID(carId, lang);
                    var CarCategory = car.CategoryID.HasValue ? _categoryService.GetCategory((long)car.CategoryID) : null;

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {

                        status = "success",
                        car = new
                        {
                            id = car.ID,
                            name = car.Name,
                            nameAr = car.NameAr,
                            licenseplate = car.LicensePlate,
                            Thumbnail = !string.IsNullOrEmpty(car.Thumbnail) ? ImageServer + car.Thumbnail : null,
                            doors = car.Doors,
                            cylinders = car.Cylinders,
                            horsepower = car.HorsePower,
                            regionspecification = car.RegionalSpecification,
                            car.LongDescription,
                            car.LongDescriptionAr,
                            year = car.Year,
                            transmission = car.Transmission,
                            fueleconomy = car.FuelEconomy,
                            capacity = car.Capacity,
                            car.Condition,
                            car.MechanicalCondition,
                            car.RegularPrice,
                            car.SalePrice,
                            car.IsActive,
                            car.ApprovalStatus,
                            car.IsPublished,
                            car.Latitude,
                            car.Longitude,
                            car.IsVerified,
                            car.IsPremium,
                            Category = CarCategory != null ? new
                            {
                                id = CarCategory.ID,
                                name = lang == "en" ? CarCategory.CategoryName : CarCategory.CategoryNameAr,
                            } : null,
                            CarMake = carMake != null ? new
                            {
                                id = carMake.ID,
                                name = lang == "en" ? carMake.Name : carMake.NameAR,
                            } : null,
                            CarModel = carModel != null ? new
                            {
                                id = carModel.ID,
                                name = lang == "en" ? carModel.Name : carModel.NameAR,
                            } : null,
                            bodyType = bodyType != null ? new
                            {
                                id = bodyType.ID,
                                name = lang == "en" ? bodyType.Name : carModel.NameAR,
                            } : null,
                            image = carImage.Select(i => new
                            {
                                id = i.ID,
                                image = !string.IsNullOrEmpty(i.Image) ? ImageServer + i.Image : null,
                            }),
                            feature = carFeature,
                            tags = carTags,
                        }
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

        [HttpGet]
        [Route("cars/{carId}/remarks")]
        public HttpResponseMessage GetCarRemarks(long carId)
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {
                    string lang = "en";
                    string ImageServer = CustomURL.GetImageServer();
                    var car = _carService.GetCar(carId);
                    if (car == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, new
                        {
                            status = "error",
                            message = "Car Not Found"
                        });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {

                        status = "success",
                        car = new
                        {
                            car.ID,
                            car.Name,
                            licenseplate = car.LicensePlate,
                            Thumbnail = !string.IsNullOrEmpty(car.Thumbnail) ? ImageServer + car.Thumbnail : null,
                            car.Remarks,
                            car.IsActive,
                            car.ApprovalStatus,
                            car.IsPublished
                        }
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

        [HttpPut]
        [Route("cars/{carId}/sendforapproval")]
        public HttpResponseMessage SendCarForApproval(long carId)
        {
            try
            {
                string message = string.Empty;

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {

                    var car = _carService.GetCar((long)carId);

                    car.IsApproved = false;
                    car.Status = "2";
                    car.ApprovalStatus = 2;


                    if (_carService.UpdateCarStatuses(ref car, ref message))
                    {
                        string SuccessMessage = "Car approval request sent successfully ...";
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = SuccessMessage
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "error",
                            message = message
                        });
                    }
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = "Session invalid or expired !"
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpPut]
        [Route("cars/{carId}/approval/cancel")]
        public HttpResponseMessage CancelApprovalRequest(long carId)
        {
            try
            {
                string message = string.Empty;

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {

                    var car = _carService.GetCar((long)carId);

                    if (string.IsNullOrEmpty(car.Remarks))
                    {
                        car.IsApproved = false;
                        car.ApprovalStatus = 4;
                        car.Status = "4";
                    }
                    else
                    {

                        car.IsApproved = false;
                        car.ApprovalStatus = 1;
                        car.Status = "1";
                    }

                    if (_carService.UpdateCarStatuses(ref car, ref message))
                    {
                        string SuccessMessage = "Car approval request canceled!";
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = SuccessMessage

                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                        {
                            status = "error",
                            message = message
                        });
                    }
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = "Session invalid or expired !"
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpPut]
        [Route("cars/{carId}/publish")]
        public HttpResponseMessage PublishCar(long carId)
        {
            try
            {
                string message = string.Empty;

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {
                    var car = _carService.GetCar((long)carId);

                    if (!car.IsPublished.HasValue || !car.IsPublished.Value)
                        car.IsPublished = true;
                    else
                        car.IsPublished = false;

                    if (_carService.UpdateCarStatuses(ref car, ref message))
                    {
                        string SuccessMessage = string.Format("Car {0} successfully ...", car.IsPublished.Value == true ? "Published" : "UnPublished");
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = SuccessMessage
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                        {
                            status = "error",
                            message = message
                        });
                    }
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = "Session invalid or expired !"
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpPut]
        [Route("cars/{carId}/Sold")]
        public HttpResponseMessage SoldCar(long carId)
        {
            try
            {
                string message = string.Empty;

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {
                    var car = _carService.GetCar((long)carId);

                    if ((bool)!car.IsSold)
                    {
                        car.SoldDate = NowBuySell.Service.Helpers.TimeZone.GetLocalDateTime();
                        car.IsSold = true;
                    }
                    else 
                    {
                        car.IsSold = false;
                    }

                    if (_carService.UpdateCarStatuses(ref car, ref message))
                    {
                        string SuccessMessage = string.Format("Car {0} successfully ...", car.IsSold.Value == true ? "Sold" : "Un Sold");
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = SuccessMessage
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                        {
                            status = "error",
                            message = message
                        });
                    }
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = "Session invalid or expired !"
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }
    }
}
