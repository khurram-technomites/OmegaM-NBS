using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.Vendor.Property;
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
    public class VendorPropertyController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IPropertyService _propService;
        private readonly ICityService _cityService;
        private readonly ICountryService _countryService;
        private readonly ICategoryService _categoryService;
        private readonly IFeatureService _featureService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;
        private readonly IPropertyFeaturesService _propfeatureService;
        private readonly IPropertyImagesService _propImageService;
        private readonly IPropertyFloorPlanService _floorPlanService;
        private readonly IPropertyRequestsService _propRequestService;
        private readonly INumberRangeService _numberRangeService;
        private readonly IVendorService _vendorService;
        private readonly IVendorPackagesService _vendorPackagesService;
        string ImageServer = string.Empty;
        string VideoServer = string.Empty;
        private readonly IVendorAddPurchasesService _VendorAddPurchasesService;
        private readonly INearByPlacesCategoryService _nearByPlacesCategoryService;
        private readonly INearByPlaceService _NearByPlaceService;

        public VendorPropertyController(IPropertyService propService
            , ICityService cityService
            , ICountryService countryService
            , ICategoryService categoryService
            , IFeatureService featureService
            , INotificationReceiverService notificationReceiverService
            , INotificationService notificationService
            , IPropertyFeaturesService propfeatureService
            , IPropertyImagesService propImageService
            , IPropertyFloorPlanService floorPlanService
            , IPropertyRequestsService propRequestService,
            INumberRangeService numberRangeService,
            IVendorPackagesService vendorPackagesService,
            IVendorService vendorService,
            IVendorAddPurchasesService vendorAddPurchasesService,
            INearByPlacesCategoryService nearByPlacesCategoryService,
            INearByPlaceService nearByPlaceService)
        {
            _propService = propService;
            _cityService = cityService;
            _countryService = countryService;
            _categoryService = categoryService;
            _featureService = featureService;
            _notificationReceiverService = notificationReceiverService;
            _notificationService = notificationService;
            _propfeatureService = propfeatureService;
            _propImageService = propImageService;
            _floorPlanService = floorPlanService;
            _propRequestService = propRequestService;
            _numberRangeService = numberRangeService;
            _vendorPackagesService = vendorPackagesService;
            _vendorService = vendorService;
            ImageServer = CustomURL.GetImageServer();
            _VendorAddPurchasesService = vendorAddPurchasesService;
            _nearByPlacesCategoryService = nearByPlacesCategoryService;
            _NearByPlaceService = nearByPlaceService;
        }


        [HttpPost]
        [Route("properties")]
        public HttpResponseMessage Insert(PropertyViewModel propertyViewModel)
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
                        string numberRange = _numberRangeService.GetNextValueFromNumberRangeByName("PROPERTY");
                        int Vendorlimit = _propService.GetVendorLimit(vendorId);
                        var packagelimit = _vendorPackagesService.GetPackageLimit(vendor.VendorPackageID.Value);
                        int ExtraLimit = _VendorAddPurchasesService.GetVendorLimitProperty(currentDateTime, (int)vendor.ID);
                        if (currentDateTime.Date < vendor.PackageEndDate.Value.Date)
                        {

                            if (packagelimit.MotorLimit + ExtraLimit > Vendorlimit)
                            {
                                Property property = new Property();

                                property.CategoryId = propertyViewModel.CategoryID;
                                property.Title = propertyViewModel.Title;
                                property.TitleAr = propertyViewModel.TitleAr;
                                //property.OldPrice = propertyViewModel.OldPrice;
                                property.Price = propertyViewModel.Price;
                                property.Description = propertyViewModel.Description;
                                property.DescriptionAr = propertyViewModel.DescriptionAr;
                                property.CountryID = propertyViewModel.CountryID;
                                property.CityID = propertyViewModel.CityID;
                                property.Area = propertyViewModel.Area;
                                property.State = propertyViewModel.State;
                                property.Address = propertyViewModel.Address;
                                property.BuildYear = propertyViewModel.BuildYear;
                                property.Size = propertyViewModel.Size;
                                property.NoOfRooms = Convert.ToInt32(propertyViewModel.NoOfRooms.Replace("+", ""));
                                property.NoOfDinings = Convert.ToInt32(propertyViewModel.NoOfDinings.Replace("+", ""));
                                property.NoOfBaths = Convert.ToInt32(propertyViewModel.NoOfBaths.Replace("+", ""));
                                property.NoOfLaundry = Convert.ToInt32(propertyViewModel.NoOfLaundry.Replace("+", ""));
                                property.NoOfGarage = Convert.ToInt32(propertyViewModel.NoOfGarage.Replace("+", ""));
                                property.ZipCode = propertyViewModel.ZipCode;
                                property.ForSale = property.ForSale;

                                property.IsActive = true;
                                property.IsPublished = false;
                                property.IsDeleted = false;
                                property.ApprovalStatusID = 2;
                                property.VendorId = vendorId;
                                property.Latitude = propertyViewModel.Latitude;
                                property.Longitude = propertyViewModel.Longitude;
                                property.Slug = Slugify.GenerateSlug(property.Title + "-" + numberRange);
                                property.IsFurnished = propertyViewModel.IsFurnished;
                                property.AdsReferenceCode = _numberRangeService.GetNextValueFromNumberRangeByName("PROPERTYREFERENCE");

                                foreach (var features in propertyViewModel.PropertyFeatures)
                                {
                                    property.PropertyFeatures.Add(new PropertyFeature()
                                    {
                                        FeatureId = features.FeatureID
                                    });
                                }
                                if(propertyViewModel.NearByPlace != null)
                                {
                                    foreach (var nearbyplaces in propertyViewModel.NearByPlace)
                                    {
                                        NearByPlace data = new NearByPlace();

                                        property.NearByPlaces.Add(new NearByPlace()
                                        {
                                            PropertyID = nearbyplaces.PropertyID,
                                            NearByPlacesCategoryID = nearbyplaces.NearByPlacesCategoryID,
                                            Name = nearbyplaces.Name,
                                            NameAr = nearbyplaces.NameAr,
                                            Distance = nearbyplaces.Distance,
                                            Latitude = nearbyplaces.Latitude,
                                            Longitude = nearbyplaces.Longitude,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreatedOn = Helpers.TimeZone.GetLocalDateTime()
                                        });

                                    }
                                }

                                if (_propService.AddProperty(property, ref message))
                                {
                                    Notification not = new Notification();
                                    not.Title = "Property Approval";
                                    not.Description = "New Property added for approval";
                                    not.OriginatorID = vendorId;
                                    not.OriginatorName = "";
                                    not.Url = "/Admin/Property/Approvals";
                                    not.Module = "Property";
                                    not.OriginatorType = "Customer";
                                    not.RecordID = property.ID;
                                    if (_notificationService.CreateNotification(not, ref message))
                                    {
                                        if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", not.OriginatorID))
                                        {
                                        }
                                    }

                                    return Request.CreateResponse(HttpStatusCode.OK, new
                                    {
                                        status = "success",
                                        message = "Property added successfully",
                                        Property = new
                                        {
                                            ID = property.ID,
                                            Title = property.Title,
                                            TitleAr = property.TitleAr,
                                            Description = property.Description,
                                            DescriptionAr = property.DescriptionAr,
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

        [HttpPut]
        [Route("properties/{propertyId}")]
        public HttpResponseMessage Update(long propertyId, PropertyViewModel propertyViewModel)
        {
            try
            {
                //if (!propertyId.HasValue)
                //{
                //    propertyId = propertyViewModel.ID;
                //}


                Property property = _propService.GetProperty(propertyId);
                if (property == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new
                    {
                        status = "error",
                        message = "Property not found!"
                    });
                }

                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    IEnumerable<PropertyFeature> propertyFeatures = _propfeatureService.GetPropertyFeature(propertyId);
                    IEnumerable<NearByPlace> nearByPlaces = _NearByPlaceService.GetNearByPlacesByPropertyID(propertyId);

                    foreach (var propertyFeature in propertyFeatures)
                    {
                        _propfeatureService.DeletePropertyFeature(propertyFeature.ID, ref message);
                    }
                    foreach (var nearByPlace in nearByPlaces)
                    {
                        _NearByPlaceService.DeleteNearByPlace(nearByPlace.ID, ref message);
                    }
                    property.Slug.Replace(Slugify.GenerateSlug(property.Title), Slugify.GenerateSlug(propertyViewModel.Title));
                    property.CategoryId = propertyViewModel.CategoryID;
                    property.Title = propertyViewModel.Title;
                    property.TitleAr = propertyViewModel.TitleAr;
                    property.OldPrice = propertyViewModel.OldPrice;
                    property.Price = propertyViewModel.Price;
                    property.Description = propertyViewModel.Description;
                    property.DescriptionAr = propertyViewModel.DescriptionAr;
                    property.CountryID = propertyViewModel.CountryID;
                    property.CityID = propertyViewModel.CityID;
                    property.Area = propertyViewModel.Area;
                    property.State = propertyViewModel.State;
                    property.Address = propertyViewModel.Address;
                    property.BuildYear = propertyViewModel.BuildYear;
                    property.Size = propertyViewModel.Size;
                    property.NoOfRooms = Convert.ToInt32(propertyViewModel.NoOfRooms.Replace("+", ""));
                    property.NoOfDinings = Convert.ToInt32(propertyViewModel.NoOfDinings.Replace("+", ""));
                    property.NoOfBaths = Convert.ToInt32(propertyViewModel.NoOfBaths.Replace("+", ""));
                    property.NoOfLaundry = Convert.ToInt32(propertyViewModel.NoOfLaundry.Replace("+", ""));
                    property.NoOfGarage = Convert.ToInt32(propertyViewModel.NoOfGarage.Replace("+", ""));
                    property.Latitude = propertyViewModel.Latitude;
                    property.Longitude = propertyViewModel.Longitude;
                    property.ApprovalStatusID = 2;
                    property.ZipCode = propertyViewModel.ZipCode;

                    foreach (var features in propertyViewModel.PropertyFeatures)
                    {
                        property.PropertyFeatures.Add(new PropertyFeature()
                        {
                            FeatureId = features.FeatureID
                        });
                    }
                    if(propertyViewModel.NearByPlace != null)
                    {
                        foreach (var nearbyplaces in propertyViewModel.NearByPlace)
                        {
                            NearByPlace data = new NearByPlace();

                            property.NearByPlaces.Add(new NearByPlace()
                            {
                                PropertyID = nearbyplaces.PropertyID,
                                NearByPlacesCategoryID = nearbyplaces.NearByPlacesCategoryID,
                                Name = nearbyplaces.Name,
                                NameAr = nearbyplaces.NameAr,
                                Distance = nearbyplaces.Distance,
                                Latitude = nearbyplaces.Latitude,
                                Longitude = nearbyplaces.Longitude,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedOn = Helpers.TimeZone.GetLocalDateTime()
                            });
                        }
                    }


                        if (_propService.UpdateProperty(ref property, ref message))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = "Property updated successfully",
                            Data = new
                            {
                                property.ID,
                                Title = property.Title,
                                TitleAr = property.TitleAr,
                                Description = property.Description,
                                DescriptionAr = property.DescriptionAr,
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

        //[HttpDelete]
        //[Route("properties/gallery/{id}")]
        //public HttpResponseMessage DeleteImage(long id)
        //{
        //    string message = string.Empty;
        //    string filePath = string.Empty;
        //    if (_propImageService.Remove(id, ref message, ref filePath))
        //    {
        //        string path = HttpContext.Current.Server.MapPath("~" + filePath);
        //        System.IO.File.Delete(path);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = message });
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Oops! Something went wrong. Please try later." });
        //}

        //[HttpDelete]
        //[Route("properties/floorplan/{id}")]
        //public HttpResponseMessage DeleteFloorPlan(long id)
        //{
        //    string message = string.Empty;
        //    string filePath = string.Empty;
        //    if (_floorPlanService.Remove(id, ref message, ref filePath))
        //    {
        //        string path = HttpContext.Current.Server.MapPath("~" + filePath);
        //        System.IO.File.Delete(path);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = message });
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Oops! Something went wrong. Please try later." });
        //}

        //[HttpDelete]
        //[Route("properties/thumbnail/{id}")]
        //public HttpResponseMessage DeleteThumbnail(long id)
        //{
        //    string message = string.Empty;
        //    string filepath = string.Empty;

        //    Property model = _propService.GetById((int)id);
        //    filepath = model.Thumbnail;
        //    model.Thumbnail = null;

        //    if (_propService.UpdateProperty(ref model, ref message))
        //    {
        //        string path = HttpContext.Current.Server.MapPath("~" + filepath);
        //        System.IO.File.Delete(path);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = message });
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Oops! Something went wrong. Please try later." });
        //}

        //[HttpDelete]
        //[Route("properties/Video/{id}")]
        //public HttpResponseMessage DeleteVideo(long id)
        //{
        //    string message = string.Empty;
        //    string filepath = string.Empty;

        //    Property model = _propService.GetById((int)id);
        //    filepath = model.Video;
        //    model.Video = null;

        //    if (_propService.UpdateProperty(ref model, ref message))
        //    {
        //        string path = HttpContext.Current.Server.MapPath("~" + filepath);
        //        System.IO.File.Delete(path);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = message });
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Oops! Something went wrong. Please try later." });
        //}

        [HttpGet]
        [Route("properties")]
        public HttpResponseMessage GetAllProperties(int pg = 1)
        {
            try
            {
                HTMLTagsSplitter splitHelper = new HTMLTagsSplitter();
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                long vendorId = 0;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {
                    int pageSize = 10;
                    string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
                    if (ReceiverType == "Vendor")
                    {
                        vendorId = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
                    }
                    var VendorProperties = _propService.GetPropertiesByVendorID(vendorId);
                    if (VendorProperties.Count() > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            Properties = VendorProperties.OrderByDescending(x => x.ID).Select(i => new
                            {
                                i.ID,
                                Thumbnail = !string.IsNullOrEmpty(i.Thumbnail) ? ImageServer + i.Thumbnail : "",
                                i.Title,
                                i.TitleAr,
                                Slug = i.Slug,
                                i.IsActive,
                                ApprovalStatus = i.ApprovalStatusID,
                                i.IsPublished,
                                Remarks = splitHelper.ToSplitList(i.Remarks),
                                i.Description,
                                i.DescriptionAr,
                                Rooms = i.NoOfRooms.HasValue == true ? i.NoOfRooms.Value == 12 ? i.NoOfRooms.Value.ToString() + "+" : i.NoOfRooms.Value.ToString() : "0",
                                Baths = i.NoOfBaths.HasValue == true ? i.NoOfBaths.Value == 5 ? i.NoOfBaths.Value.ToString() + "+" : i.NoOfBaths.Value.ToString() : "0",
                                Garages = i.NoOfGarage.HasValue == true ? i.NoOfGarage.Value : 0,

                                NoOfDinings = i.NoOfDinings.HasValue == true ? i.NoOfDinings.Value : 0,
                                NoOfLaundry = i.NoOfLaundry.HasValue == true ? i.NoOfLaundry.Value : 0,

                                BuildYear = i.BuildYear.HasValue == true ? i.BuildYear.Value : 0,
                                Size = i.Size.HasValue == true ? i.Size.Value : 0,
                                Status = i.ForSale == true ? "Sale" : "Rent",
                                OldPrice = i.OldPrice.HasValue ? i.OldPrice.Value : 0,
                                Price = i.Price.HasValue == true ? i.Price.Value : 0,
                                i.CategoryId,
                                i.IsFurnished,
                                i.AdsReferenceCode,
                                i.IsFeatured,
                                inspection = i.PropertyInspections.Count() == 0 ? null : new
                                {
                                    Id = i.PropertyInspections.Count() > 0 ? i.PropertyInspections.FirstOrDefault().ID : 0,
                                    Image = i.PropertyInspections.Count() > 0 ? ImageServer + i.PropertyInspections.FirstOrDefault().Path : null,
                                },
                                NearByPlace = i.NearByPlaces.Where(X => X.IsDeleted == false).Select(j => new
                                {
                                    Id = i.ID,
                                    Name = j.Name,
                                    Icon = ImageServer + "/" + j.NearByPlacesCategory.Image,
                                    CategoryName = j.NearByPlacesCategory.Name,
                                    Latitude = j.Latitude,
                                    Longitude = j.Longitude,
                                    Distance = j.Distance,
                                }),
                                Category = i.Category != null ? new
                                {
                                    id = i.CategoryId,
                                    name = i.Category.CategoryName
                                } : null,
                                i.CountryID,
                                Country = i.Country != null ? new
                                {
                                    id = i.CountryID,
                                    name = i.Country.Name
                                } : null,
                                i.CityID,
                                City = i.City != null ? new
                                {
                                    id = i.CityID,
                                    name = i.City.Name
                                } : null,
                                State = i.State,
                                Area = i.Area,
                                Address = i.Address,
                                ZipCode = i.ZipCode,
                                i.Latitude,
                                i.Longitude,
                                i.IsVerified,
                                i.IsPremium,
                                IsSold = i.IsSold == null || i.IsSold == false ? false : true,
                                SoldDate = GetSoldDate(i.SoldDate,i.IsSold),
                                Video = new
                                {
                                    video = string.IsNullOrEmpty(i.Video) ? null : ImageServer + i.Video
                                },
                                Images = i.PropertyImages.Where(j => !string.IsNullOrEmpty(j.Path)).Select(k => new
                                {
                                    k.ID,
                                    image = ImageServer + k.Path
                                }).ToList(),
                                /*                                Videos = i.Video.Where(j => !string.IsNullOrEmpty(j.Video)).Select(k => new
                                                                {
                                                                    k.ID,
                                                                    video = ImageServer + k.Path
                                                                }).ToList(),*/
                                FloorPlans = i.PropertyFloorPlans.Where(j => !string.IsNullOrEmpty(j.Path)).Select(k => new
                                {
                                    k.ID,
                                    image = ImageServer + k.Path
                                }).ToArray(),
                                Features = i.PropertyFeatures.Where(j => j.Feature != null).Select(k => new
                                {
                                    k.Feature.ID,
                                    k.Feature.Name,
                                    k.Feature.NameAR,
                                    Icon = !string.IsNullOrWhiteSpace(k.Feature.Icon) ? ImageServer + k.Feature.Icon : ""
                                }).ToList(),
                            }).Skip(pageSize * (pg - 1)).Take(pageSize).ToList()
                        });
                    }

                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "error",
                            message = "No properties available"
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

        [HttpGet]
        [Route("properties/{propertyId}")]
        public HttpResponseMessage GetPropertyDetails(long propertyId)
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
                    var property = _propService.GetProperty(propertyId);
                    if (property == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, new
                        {
                            status = "error",
                            message = "Property Not Found"
                        });
                    }

                    var Category = _categoryService.GetCategory((int)property.CategoryId);
                    //var Country = _countryService.GetCountry((int)property.CountryID.Value);
                    var City = _cityService.GetCity((int)property.CityID.Value);

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {

                        status = "success",
                        property = new
                        {
                            Id = property.ID,
                            property.Title,
                            property.TitleAr,
                            Slug = property.Slug,
                            Thumbnail = !string.IsNullOrWhiteSpace(property.Thumbnail) ? ImageServer + "/" + property.Thumbnail : "",
                            property.Description,
                            property.DescriptionAr,
                            Rooms = property.NoOfRooms.HasValue == true ? property.NoOfRooms.Value == 12 ? property.NoOfRooms.Value.ToString() + "+" : property.NoOfRooms.Value.ToString() : "0",
                            Baths = property.NoOfBaths.HasValue == true ? property.NoOfBaths.Value == 5 ? property.NoOfBaths.Value.ToString() + "+" : property.NoOfBaths.Value.ToString() : "0",
                            Garages = property.NoOfGarage.HasValue == true ? property.NoOfGarage.Value : 0,
                            BuildYear = property.BuildYear.HasValue == true ? property.BuildYear.Value : 0,
                            Size = property.Size.HasValue == true ? property.Size.Value : 0,
                            Status = property.ForSale == true ? "Sale" : "Rent",
                            OldPrice = property.OldPrice.HasValue ? property.OldPrice.Value : 0,
                            Price = property.Price.HasValue == true ? property.Price.Value : 0,
                            property.Latitude,
                            property.Longitude,
                            property.IsVerified,
                            property.IsPremium,
                            Category = Category != null ? new
                            {
                                id = Category.ID,
                                name = Category.CategoryName,
                            } : null,

                            //Country = Country != null ? new
                            //{
                            //    id = Country.ID,
                            //    name = Country.Name,
                            //} : null,

                            City = City != null ? new
                            {
                                id = City.ID,
                                name = City.Name,
                            } : null,
                            State = property.State,
                            Area = property.Area,
                            Address = property.Address,
                            Zipcode = property.ZipCode,
                            Images = property.PropertyImages.Where(i => !string.IsNullOrEmpty(i.Path)).Select(i => ImageServer + i.Path).ToArray(),
                            FloorPlans = property.PropertyFloorPlans.Where(i => !string.IsNullOrEmpty(i.Path)).Select(i => ImageServer + i.Path).ToArray(),
                            Features = property.PropertyFeatures.Where(i => i.Feature != null).Select(i => new
                            {
                                i.Feature.Name,
                                i.Feature.NameAR,
                                Icon = !string.IsNullOrWhiteSpace(i.Feature.Icon) ? ImageServer + i.Feature.Icon : ""
                            }).ToList(),
                        }
                    });
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

        [HttpGet]
        [Route("properties/{propertyId}/remarks")]
        public HttpResponseMessage GetPropertyRemarks(long propertyId)
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
                    var prop = _propService.GetById((int)propertyId);
                    if (prop == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, new
                        {
                            status = "error",
                            message = "Property Not Found"
                        });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {

                        status = "success",
                        car = new
                        {
                            prop.ID,
                            prop.Title,
                            prop.Address,
                            Thumbnail = !string.IsNullOrEmpty(prop.Thumbnail) ? ImageServer + prop.Thumbnail : null,
                            prop.Remarks,
                            prop.IsActive,
                            prop.ApprovalStatusID,
                            prop.IsPublished
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
        [Route("properties/{propertyId}/sendforapproval")]
        public HttpResponseMessage SendCarForApproval(long propertyId)
        {
            try
            {
                string message = string.Empty;

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {

                    var prop = _propService.GetById((int)propertyId);

                    prop.ApprovalStatusID = 2;


                    if (_propService.UpdateProperty(ref prop, ref message))
                    {
                        string SuccessMessage = "Property approval request sent successfully ...";
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
                            message = "Something went wrong"

                        });
                    }
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
        [Route("properties/{propertyId}/approval/cancel")]
        public HttpResponseMessage CancelApprovalRequest(long propertyId)
        {
            try
            {
                string message = string.Empty;

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {

                    var prop = _propService.GetById((int)propertyId);

                    if (string.IsNullOrEmpty(prop.Remarks))
                    {
                        prop.ApprovalStatusID = 4;
                    }
                    else
                    {
                        prop.ApprovalStatusID = 1;
                    }

                    if (_propService.UpdateProperty(ref prop, ref message))
                    {
                        string SuccessMessage = "Property approval request canceled!";
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
                            message = "Something went wrong"

                        });
                    }
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
        [Route("properties/{propertyId}/publish")]
        public HttpResponseMessage PublishProperty(long propertyId)
        {
            try
            {
                string message = string.Empty;

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {
                    var property = _propService.GetProperty((long)propertyId);

                    //property.IsPublished = true;

                    if (!property.IsPublished)
                        property.IsPublished = true;
                    else
                        property.IsPublished = false;

                    if (_propService.UpdatePropertyStatuses(ref property, ref message))
                    {
                        string SuccessMessage = string.Format("Property {0} successfully ...", property.IsPublished == true ? "Published" : "UnPublished");

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
        [Route("properties/{propertyId}/unpublish")]
        public HttpResponseMessage UnPublishProperty(long propertyId)
        {
            try
            {
                string message = string.Empty;

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {
                    var property = _propService.GetProperty((long)propertyId);

                    property.IsPublished = false;

                    if (_propService.UpdatePropertyStatuses(ref property, ref message))
                    {
                        string SuccessMessage = "Property published successfully ...";
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
        [Route("properties/{propertyId}/Sold")]
        public HttpResponseMessage SoldProperty(long propertyId)
        {
            try
            {
                string message = string.Empty;

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {
                    var property = _propService.GetProperty((long)propertyId);

                    //property.IsPublished = true;

                    if ((bool)!property.IsSold)
                    {
                        property.SoldDate = NowBuySell.Service.Helpers.TimeZone.GetLocalDateTime();
                        property.IsSold = true;
                    }
                    else 
                    {
                        property.IsSold = false;
                    }

                    if (_propService.UpdatePropertyStatuses(ref property, ref message))
                    {
                        string SuccessMessage = string.Format("Property {0} successfully ...", property.IsSold == true ? "Sold" : "Un Sold");

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
        [HttpGet]
        [Route("properties/nearbyplacescategory")]
        public HttpResponseMessage NearByPlaceCategory()
        {
            try
            {
                string message = string.Empty;

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {
                    var nearByPlacesCategory = _nearByPlacesCategoryService.GetAllNearByPlacesCategory();
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        nearByPlacesCategory = nearByPlacesCategory.OrderByDescending(x=>x.ID).Select(i=> new
                        {
                            i.ID,
                            i.Name,
                            i.NameAr,
                            Image= string.IsNullOrEmpty(i.Image) ? null : ImageServer + i.Image,

                        }
                        
                        )
                    });
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
        public string GetSoldDate(DateTime? SoldDates, bool? IsSold)
        {
            string SoldDate = string.Empty;
            if (SoldDates != null && IsSold == true && IsSold != null)
            {
                DateTime? compareTo = SoldDates;
                DateTime now = Helpers.TimeZone.GetLocalDateTime();
                double TotalMonth = 0;
                double TotalDays = Math.Round((Helpers.TimeZone.GetLocalDateTime() - SoldDates).Value.TotalDays);
                TotalMonth = (Helpers.TimeZone.GetLocalDateTime().Year - SoldDates.Value.Year) * 12 + Helpers.TimeZone.GetLocalDateTime().Month - SoldDates.Value.Month;
                double finalHours = Math.Round((Helpers.TimeZone.GetLocalDateTime() - SoldDates).Value.TotalHours);
                TimeSpan? ts = now - compareTo;
                if (TotalDays != 0)
                {
                    SoldDate = TotalMonth != 0 ? "Sold " + Math.Abs(Math.Round(TotalMonth)) + " months ago" : "Sold " + Math.Abs(TotalDays) + " days ago";
                }
                else
                {
                    if (Math.Abs(finalHours) > 0)
                    {
                        SoldDate = "Sold " + Math.Abs(finalHours) + " hours ago";
                    }
                    else
                    {
                        SoldDate = "Sold " + Math.Round(ts.Value.TotalMinutes) + " Minutes ago";
                    }
                }
            }
            return SoldDate;
        }

    }
  
}
