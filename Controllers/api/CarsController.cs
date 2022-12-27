using Newtonsoft.Json;
using NowBuySell.Data;
using NowBuySell.Data.HelperClasses;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Cars;
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
using static NowBuySell.Data.HelperClasses.MapResponseViewModel;

namespace NowBuySell.Web.Controllers.api
{

    [RoutePrefix("api/v1")]
    public class CarsController : ApiController
    {

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
        private readonly ICarRequestsService _carRequestService;
        private readonly ICityService _cityService;
        private readonly ICountryService _countryService;
        private readonly IFeatureService _featureService;
        private readonly ICarDocumentService _carDocumentService;


        string ImageServer = string.Empty;


        public CarsController(ICarService carService, ICategoryService categoryService, ICarMakeService carMakeService,
            ICarModelService carModelService, IBodyTypeService bodyTypeService, ICarPackageService carPackageService,
            ICarFeatureService carFeatureService, ICarImageService carImageService, ICarInsuranceService carInsuranceService,
            IVendorService vendorService, ICarTagService tagService, INotificationReceiverService notificationReceiverService,
            INotificationService notificationService, ICarRequestsService carRequestService, ICityService cityService,
            ICountryService countryService, IFeatureService featureService, ICarDocumentService carDocumentService)
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
            _cityService = cityService;
            _countryService = countryService;
            _featureService = featureService;
            _carDocumentService = carDocumentService;

            _notificationReceiverService = notificationReceiverService;
            _notificationService = notificationService;
            _carRequestService = carRequestService;
            ImageServer = CustomURL.GetImageServer();
        }
        [AllowAnonymous]
        [Route("places")]
        public async Task<object> GetPlaces(string Place)
        {
            
            HttpClient _client = new HttpClient();
            HttpResponseMessage httpResponse = await  _client.GetAsync($"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={Place}&inputtype=textquery&key=AIzaSyAEmhLaFjth5xau57Gy1NwE1O6apk443xY&components=country:ae");
            ////HttpResponseMessage httpResponse = await _client.GetAsync($"https://maps.googleapis.com/maps/api/place/textsearch/json?query={Place}&key={_key}");


            var data = await httpResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<object>(data);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("car/filter/{lang}/get")]
        public HttpResponseMessage Get([FromBody] CarFilterViewModel Model, string lang = "en")
        {

            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims;
            long customerId = 0;
            HttpClient _client = new HttpClient();
            string address = string.Empty;
           
            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                Model.CustomerID = (int)customerId;

            if (Model.PageSize % 40 == 0)
            {
                var Skip = Model.Skip / 40;
                Model.Skip = Skip * 8;
                Model.PageSize = 8;

            }
            Model.Locations = new List<LocationModel>();
            if(Model.Longitude !=0 && Model.Latitude != 0)
            {
                LocationModel loc = new LocationModel();

               
                    loc.Latitude = Model.Latitude;
                    loc.Longitude = Model.Longitude;
                    loc.Distance = Model.Distance == 0? 1 :Model.Distance;
                    Model.Locations.Add(loc);
                
            }
            else if (Model.PlaceID != null)
            {
                
                foreach (var PlaceId in Model.PlaceID)
                {
                    if (!string.IsNullOrEmpty(PlaceId))
                    {
                        HttpResponseMessage httpResponse = _client.GetAsync($"https://maps.googleapis.com/maps/api/place/details/json?key=AIzaSyAEmhLaFjth5xau57Gy1NwE1O6apk443xY&place_id={PlaceId}&fields=geometry%2Caddress_components").Result;

                        var data = httpResponse.Content.ReadAsStringAsync().Result;

                        Root mapViewModel = JsonConvert.DeserializeObject<Root>(data);
                        LocationModel loc = new LocationModel();
                        
                        if (mapViewModel.status == "OK")
                        {
                            loc.Latitude = (decimal)mapViewModel.result.geometry.location.lat;
                            loc.Longitude = (decimal)mapViewModel.result.geometry.location.lng;
                            var mapArea = mapViewModel.result.address_components.Where(x => x.types.Contains("sublocality")).FirstOrDefault();
                            loc.Area = mapArea != null ? mapArea.short_name : "";
                            var addr = mapViewModel.result.address_components.Where(x => x.types.Contains("premise")).FirstOrDefault();
                            loc.Address = addr != null ? addr.short_name : mapViewModel.result.address_components.FirstOrDefault().short_name;
                            loc.PlaceID = PlaceId;
                            loc.Distance = 1;
                            Model.Locations.Add(loc);
                        }

                    }
                }
            }
          
          
          
            
            
            var result = _carService.GetCarsFilter(Model);

            

            List<CarResultList> resultList = new List<CarResultList>();
            List<VendorSocialLinks> VendorSocialLinks = new List<VendorSocialLinks>();

            if (Model.VendorID != null)
            {
                var vendor = _vendorService.GetVendor((long)Model.VendorID);
                VendorSocialLinks.Add(new VendorSocialLinks
                {

                    Facebook = vendor.Facebook != null ? vendor.Facebook : "-",
                    Instagram = vendor.Instagram != null ? vendor.Instagram : "-",
                    Twitter = vendor.Twitter != null ? vendor.Twitter : "-",
                    Whatsapp = vendor.Whatsapp != null ? vendor.Whatsapp : "-",
                    LinkedIn = vendor.LinkedIn != null ? vendor.LinkedIn : "-",
                    Youtube = vendor.Youtube != null ? vendor.Youtube : "-",
                    Snapchat = vendor.Snapchat != null ? vendor.Snapchat : "-",
                    TikTok = vendor.TikTok != null ? vendor.TikTok : "-",

                });
            }

            foreach (var record in result)
            {
                List<CarImages> img = new List<CarImages>();
                long WishlistID = 0;
              
                if (record.CustomerWishlists.Count > 0 && customerId != 0)
                {
                    var value = record.CustomerWishlists.Where(x => x.CustomerID == customerId).FirstOrDefault();
                    WishlistID = value != null ? value.ID : 0;
                }
                List<string> carImg = _carImageService.GetCarImages(record.ID).Select(x => ImageServer + x.Image).ToList();

               
                //foreach (var entry in carImg)
                //{
                //    img.Add(new CarImages
                //    {
                //        imageID = entry.ID,
                //        imagePath = ImageServer + entry.Image
                //    });
                //}

                resultList.Add(new CarResultList
                {
                    Id = (int)record.ID,
                    CountryId = record.CountryID.HasValue ? (int)record.CountryID.Value : 0,
                    CityId = record.CityID.HasValue ? (int)record.CityID.Value : 0,
                    CategoryId = record.CategoryID.HasValue ? (int)record.CategoryID.Value : 0,
                    Title = lang == "ar" ? record.NameAr : record.Name,
                    Description = lang == "ar" ? record.LongDescriptionAr : record.LongDescription,
                    Address = record.Address,
                    Slug = record.Slug,
                    Transmission = lang == "ar" ? ArabicDictionary.Translate(record.Transmission) : record.Transmission,
                    Door = lang == "ar" ? ArabicDictionary.Translate(record.Doors) : record.Doors,
                    Cylinder = record.Cylinders,
                    Capacity = lang == "ar" ? ArabicDictionary.Translate(record.Capacity) : record.Capacity,
                    OldPrice = record.RegularPrice.HasValue ? record.RegularPrice.Value : 0,
                    Price = record.SalePrice.HasValue ? record.SalePrice.Value : 0,
                    Thumbnail = ImageServer + "/" + record.Thumbnail,
                    WishlistId = WishlistID,
                    VendorThumbnail = record.Customer != null ? ImageServer + "/" + record.Customer.Logo : record.Vendor != null ? ImageServer + "/" + record.Vendor.Logo : null,
                    Wheels = lang == "ar" ? ArabicDictionary.Translate(record.Wheels) : record.Wheels,
                    EngineDisplacement = lang == "ar" ? ArabicDictionary.Translate(record.EngineDisplacement) : record.EngineDisplacement,
                    HorsePower = record.HorsePower,
                    VendorFacebook = record.Vendor.Facebook != null ? record.Vendor.Facebook : "-",
                    VendorInstagram = record.Vendor.Instagram != null ? record.Vendor.Instagram : "-",
                    VendorTwitter = record.Vendor.Twitter != null ? record.Vendor.Twitter : "-",
                    VendorWhatsapp = record.Vendor.Whatsapp != null ? record.Vendor.Whatsapp : "-",
                    VendorLinkedin = record.Vendor.LinkedIn != null ? record.Vendor.LinkedIn : "-",
                    VendorTikTok = record.Vendor.TikTok != null ? record.Vendor.TikTok : "-",
                    VendorYoutube = record.Vendor.Youtube != null ? record.Vendor.Youtube : "-",
                    VendorSnapchat = record.Vendor.Snapchat != null ? record.Vendor.Snapchat : "-",

                    IsVerified = record.IsVerified == null || record.IsVerified == false ? false : true,
                    IsPremium = record.IsPremium == null || record.IsPremium == false ? false : true,
                    IsSold = record.IsSold == null || record.IsSold == false ? false : true,
                    VendorID = record.VendorID ?? 0,
                    VendorName =  record.Vendor.Name,
                    VendorMobile = record.Vendor.Mobile,
                    PermitNo = record.Vendor.PermitNo,
                    VendorContact = record.Vendor.Contact,
                    AdsReferenceCode = record.AdsReferenceCode,
                    Mileage = record.FuelEconomy,
                    Images = carImg.Take(4).ToList(),
                    ListingType = "Normal",
                    Latitude = record.Latitude,
                    Longitude = record.Longitude,
                    SoldDate = GetSoldDate(record.SoldDate,record.IsSold),
                    
                    //CarImagespath = carImg.Select(j => new
                    //{
                    //    imageID = j.ID,
                    //    imagePath = !string.IsNullOrEmpty(j.Image) ? ImageServer + j.Image : null,
                    //}).ToList(),
                });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                status = "success",
                address= Model.Locations.Select(x => new { x.PlaceID, x.Address }).ToArray(),
                Data = resultList,
                SocialLinks = VendorSocialLinks
            });
        }

        [AllowAnonymous]
        [Route("car/premium/{lang}/get")]
        public HttpResponseMessage GetPremiumCars( string lang = "en")
        {

            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims;
            long customerId = 0;

            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                customerId = (int)customerId;

            var result = _carService.GetPremiumCars();
            List<CarResultList> resultList = new List<CarResultList>();


            foreach (var record in result)
            {
                List<CarImages> img = new List<CarImages>();
                long WishlistID = 0;
                if (record.CustomerWishlists.Count > 0 && customerId != 0)
                {
                    var value = record.CustomerWishlists.Where(x => x.CustomerID == customerId).FirstOrDefault();
                    WishlistID = value != null ? value.ID : 0;
                }
                List<string> carImg = _carImageService.GetCarImages(record.ID).Select(x => ImageServer + x.Image).ToList();

                //foreach (var entry in carImg)
                //{
                //    img.Add(new CarImages
                //    {
                //        imageID = entry.ID,
                //        imagePath = ImageServer + entry.Image
                //    });
                //}

                resultList.Add(new CarResultList
                {
                    Id = (int)record.ID,
                    CountryId = record.CountryID.HasValue ? (int)record.CountryID.Value : 0,
                    CityId = record.CityID.HasValue ? (int)record.CityID.Value : 0,
                    CategoryId = record.CategoryID.HasValue ? (int)record.CategoryID.Value : 0,
                    Title = lang == "ar" ? record.NameAr : record.Name,
                    Description = lang == "ar" ? record.LongDescriptionAr : record.LongDescription,
                    Address = record.Address,
                    Slug = record.Slug,
                    Transmission = lang == "ar" ? ArabicDictionary.Translate(record.Transmission) : record.Transmission,
                    Door = lang == "ar" ? ArabicDictionary.Translate(record.Doors) : record.Doors,
                    Cylinder = record.Cylinders,
                    Capacity = lang == "ar" ? ArabicDictionary.Translate(record.Capacity) : record.Capacity,
                    OldPrice = record.RegularPrice.HasValue ? record.RegularPrice.Value : 0,
                    Price = record.SalePrice.HasValue ? record.SalePrice.Value : 0,
                    Thumbnail = ImageServer + "/" + record.Thumbnail,
                    WishlistId = WishlistID,
                    VendorThumbnail = record.Customer != null ? ImageServer + "/" + record.Customer.Logo : record.Vendor != null ? ImageServer + "/" + record.Vendor.Logo : null,
                    Wheels = lang == "ar" ? ArabicDictionary.Translate(record.Wheels) : record.Wheels,
                    EngineDisplacement = lang == "ar" ? ArabicDictionary.Translate(record.EngineDisplacement) : record.EngineDisplacement,
                    HorsePower = record.HorsePower,
                    VendorFacebook = record.Vendor.Facebook != null ? record.Vendor.Facebook : "-",
                    VendorInstagram = record.Vendor.Instagram != null ? record.Vendor.Instagram : "-",
                    VendorTwitter = record.Vendor.Twitter != null ? record.Vendor.Twitter : "-",
                    VendorWhatsapp = record.Vendor.Whatsapp != null ? record.Vendor.Whatsapp : "-",
                    VendorLinkedin = record.Vendor.LinkedIn != null ? record.Vendor.LinkedIn : "-",
                    VendorYoutube = record.Vendor.Youtube != null ? record.Vendor.Youtube : "-",
                    VendorSnapchat = record.Vendor.Snapchat != null ? record.Vendor.Snapchat : "-",
                    VendorTikTok = record.Vendor.TikTok != null ? record.Vendor.TikTok : "-",
                    VendorName = record.Vendor.Name,
                    VendorID = (int)(record.Vendor != null ? record.Vendor.ID : 0),
                    VendorMobile = record.Vendor.Mobile,
                    PermitNo = record.Vendor.PermitNo,
                    VendorContact = record.Vendor.Contact,
                    IsVerified = record.IsVerified == null || record.IsVerified == false ? false : true,
                    IsPremium = record.IsPremium == null || record.IsPremium == false ? false : true,
                    IsSold = record.IsSold == null || record.IsSold == false ? false : true,
                    Images = carImg.Take(4).ToList(),
                    AdsReferenceCode = record.AdsReferenceCode,
                    ListingType = "Premium"

                    //CarImagespath = carImg.Select(j => new
                    //{
                    //    imageID = j.ID,
                    //    imagePath = !string.IsNullOrEmpty(j.Image) ? ImageServer + j.Image : null,
                    //}).ToList(),
                });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                status = "success",
                Data = resultList
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("car/Trending/{lang}/get")]
        public HttpResponseMessage GetTrending([FromBody] CarFilterViewModel Model, string lang = "en")
        {

            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims;
            long customerId = 0;

            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                Model.CustomerID = (int)customerId;


            var result = _carService.GetTrendingList(Model);
            List<CarResultList> resultList = new List<CarResultList>();

            foreach (var record in result)
            {
                long WishlistID = 0;
                if (record.CustomerWishlists.Count > 0 && customerId != 0)
                {
                    var value = record.CustomerWishlists.Where(x => x.CustomerID == customerId).FirstOrDefault();
                    WishlistID = value != null ? value.ID : 0;
                }
                List<string> carImg = _carImageService.GetCarImages(record.ID).Select(x => ImageServer + x.Image).ToList();
                resultList.Add(new CarResultList
                {
                    Id = (int)record.ID,
                    CountryId = record.CountryID.HasValue ? (int)record.CountryID.Value : 0,
                    CityId = record.CityID.HasValue ? (int)record.CityID.Value : 0,
                    CategoryId = record.CategoryID.HasValue ? (int)record.CategoryID.Value : 0,
                    Title = lang == "ar" ? record.NameAr : record.Name,
                    Description = lang == "ar" ? record.LongDescriptionAr : record.LongDescription,
                    Address = record.Address,
                    Slug = record.Slug,
                    Transmission = lang == "ar" ? ArabicDictionary.Translate(record.Transmission) : record.Transmission,
                    Door = lang == "ar" ? ArabicDictionary.Translate(record.Doors) : record.Doors,
                    Cylinder = record.Cylinders,
                    Capacity = lang == "ar" ? ArabicDictionary.Translate(record.Capacity) : record.Capacity,
                    OldPrice = record.RegularPrice.HasValue ? record.RegularPrice.Value : 0,
                    Price = record.SalePrice.HasValue ? record.SalePrice.Value : 0,
                    Thumbnail = ImageServer + "/" + record.Thumbnail,
                    WishlistId = WishlistID,
                    VendorThumbnail = record.Customer != null ? ImageServer + "/" + record.Customer.Logo : record.Vendor != null ? ImageServer + "/" + record.Vendor.Logo : null,
                    Wheels = lang == "ar" ? ArabicDictionary.Translate(record.Wheels) : record.Wheels,
                    EngineDisplacement = lang == "ar" ? ArabicDictionary.Translate(record.EngineDisplacement) : record.EngineDisplacement,
                    HorsePower = record.HorsePower,
                    VendorFacebook = record.Vendor.Facebook != null ? record.Vendor.Facebook : "-",
                    VendorInstagram = record.Vendor.Instagram != null ? record.Vendor.Instagram : "-",
                    VendorTwitter = record.Vendor.Twitter != null ? record.Vendor.Twitter : "-",
                    VendorWhatsapp = record.Vendor.Whatsapp != null ? record.Vendor.Whatsapp : "-",
                    VendorLinkedin = record.Vendor.LinkedIn != null ? record.Vendor.LinkedIn : "-",
                    VendorYoutube = record.Vendor.Youtube != null ? record.Vendor.Youtube : "-",
                    VendorSnapchat = record.Vendor.Snapchat != null ? record.Vendor.Snapchat : "-",
                    VendorTikTok = record.Vendor.TikTok != null ? record.Vendor.TikTok : "-",
                    VendorID = Model.VendorID ?? 0,
                    AdsReferenceCode = record.AdsReferenceCode,
                    VendorName = record.Vendor.Name,
                    VendorMobile = record.Vendor.Mobile,
                    PermitNo = record.Vendor.PermitNo,
                    VendorContact = record.Vendor.Contact,
                    IsVerified = !record.IsVerified.HasValue ? false : record.IsVerified.Value,
                    IsPremium = !record.IsPremium.HasValue ? false : record.IsPremium.Value,
                    IsSold = !record.IsSold.HasValue ? false : record.IsSold.Value,
                    Images = carImg.Take(4).ToList(),

                });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                status = "success",
                Data = resultList
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("car/getBySlug/{lang}/{slug}")]
        public HttpResponseMessage GetBySlug(string slug, string lang = "en")
        {
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims;
            long customerId = 0, WishlistID = 0;
            
            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
            { }

            var record = _carService.GetCarbySlug(slug);

            if (record == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new
                {
                    status = "success",
                    message = "No Record Found",
                    Data = new { }
                });
            }

            if (record.CustomerWishlists.Count > 0 && customerId != 0)
            {
                var value = record.CustomerWishlists.Where(x => x.CustomerID == customerId).FirstOrDefault();
                WishlistID = value != null ? value.ID : 0;
            }
           
            var carinspection = record.CarInspections.Count > 0 ? record.CarInspections.FirstOrDefault().Path : null;

            CarResultList List = new CarResultList();

            List.Id = (int)record.ID;
            List.Title = lang == "ar" ? record.NameAr : record.Name;
            List.Description = lang == "ar" ? record.LongDescriptionAr : record.LongDescription;
            List.Door = lang == "ar" ? ArabicDictionary.Translate(record.Doors) : record.Doors;
            List.Cylinder = record.Cylinders;
            List.HorsePower = lang == "ar" ? ArabicDictionary.Translate(record.HorsePower) : record.HorsePower;
            List.Capacity = lang == "ar" ? ArabicDictionary.Translate(record.Capacity) : record.Capacity;
            List.Price = record.SalePrice.HasValue ? record.SalePrice.Value : record.RegularPrice.Value;
            List.Thumbnail = ImageServer + "/" + record.Thumbnail;
            List.VendorThumbnail = ImageServer + "/" + record.Vendor.Logo;
            List.Color = record.Color;
            List.City = lang == "ar" ? _cityService.GetCity((int)record.CityID.Value).NameAR : _cityService.GetCity((int)record.CityID.Value).Name;
            List.Country = lang == "ar" ? record.CountryID.HasValue ? _countryService.GetCountry((int)record.CountryID.Value).NameAr : "دبي" : record.CountryID.HasValue ? _countryService.GetCountry((int)record.CountryID.Value).Name : "Dubai";
            List.Area = record.Area;
            List.Address = record.Address;
            List.Condition = record.Condition;
            List.FuelType = lang == "ar" ? ArabicDictionary.Translate(record.FuelType) : record.FuelType;
            List.WarrentyEndDate = GetDate(record.WarrentyEndDate);
            List.Category = lang == "ar" ? _categoryService.GetCategory((int)record.CategoryID).CategoryNameAr : _categoryService.GetCategory((int)record.CategoryID).CategoryName;
            List.LicensePlate = record.LicensePlate;
            List.VendorName = record.Vendor.Name;
            List.PermitNo = record.Vendor.PermitNo;
            List.DEDNo = record.Vendor.DEDNo;
            List.RERANo = record.Vendor.RERANo;
            List.Slug = record.Slug;
            List.VendorID = record.VendorID.Value;
            List.Garage = record.Garage;
            List.GarageSize = record.GarageSize;
            List.EngineDisplacement = lang == "ar" ? ArabicDictionary.Translate(record.EngineDisplacement) : record.EngineDisplacement;
            List.State = record.Area;
            List.ZipCode = record.ZipCode;
            List.WishlistId = WishlistID;
            List.Video = !string.IsNullOrEmpty(record.Video) ? ImageServer + record.Video : null;
            List.Make = lang == "ar" ? record.CarMake.NameAR : record.CarMake.Name;
            List.Model = lang == "ar" ? record.CarModel.NameAR : record.CarModel.Name;
            List.Year = record.Year;
            List.Latitude = record.Latitude;
            List.Longitude = record.Longitude;
            List.Transmission = lang == "ar" ? ArabicDictionary.Translate(record.Transmission) : record.Transmission;
            List.IsFeatured = !record.IsFeatured.HasValue ? false : record.IsFeatured.Value;
            List.IsPremium = !record.IsPremium.HasValue ? false : record.IsPremium.Value;
            List.IsSold = !record.IsSold.HasValue ? false : record.IsSold.Value;
            List.IsVerified = !record.IsVerified.HasValue ? false : record.IsVerified.Value;
            List.AdPostedDate = GetDate(record.CreatedOn);
            List.BodyType = record.BodyTypeID != null ? record.BodyType.Name : "";
            List.SteeringSide = lang == "ar" ? ArabicDictionary.Translate(record.SteeringSide, false) : record.SteeringSide;
            List.MechanicalCondition = lang == "ar" ? ArabicDictionary.Translate(record.MechanicalCondition, false) : record.MechanicalCondition;
            List.ServiceHistory = !record.ServiceHistory.HasValue ? false : record.ServiceHistory.Value;
            List.RegionalSpecification = lang == "ar" ? ArabicDictionary.Translate(record.RegionalSpecification, false) : record.RegionalSpecification;
            List.Mileage = record.FuelEconomy;
            List.Warranty = !record.Warranty.HasValue ? false : record.Warranty.Value;
            List.Wheels = lang == "ar" ? ArabicDictionary.Translate(record.Wheels) : record.Wheels;
            List.LastUpdationDate = GetDate(record.ModifiedDate);
            List.VendorMobile = record.Vendor.Mobile;
            List.VendorContact = record.Vendor.Contact;
            List.VendorFacebook = record.Vendor.Facebook != null ? record.Vendor.Facebook : "-";
            List.VendorInstagram = record.Vendor.Instagram != null ? record.Vendor.Instagram : "-";
            List.VendorTwitter = record.Vendor.Twitter != null ? record.Vendor.Twitter : "-";
            List.VendorWhatsapp = record.Vendor.Whatsapp != null ? record.Vendor.Whatsapp : "-";
            List.VendorLinkedin = record.Vendor.LinkedIn != null ? record.Vendor.LinkedIn : "-";
            List.VendorSnapchat = record.Vendor.Snapchat != null ? record.Vendor.Snapchat : "-";
            List.VendorYoutube = record.Vendor.Youtube != null ? record.Vendor.Youtube : "-";
            List.VendorTikTok = record.Vendor.TikTok != null ? record.Vendor.TikTok : "-";
            List.AdsReferenceCode = record.AdsReferenceCode;
            List.ListingType = record.IsPremium == true ? "Premium" : "Normal";
            List.SoldDate = GetSoldDate(record.SoldDate,record.IsSold);
            List.carInspection = !string.IsNullOrEmpty(carinspection) ? ImageServer + carinspection : null;
            List.carInspectionName = record.CarInspections.Count > 0 ? record.CarInspections.FirstOrDefault().Name : null;

            foreach (var entry in record.CarImages)
                List.Images.Add(ImageServer + "/" + entry.Image);

            foreach (var entry in record.CarFeatures)
            {
                if (_featureService.GetFeature(entry.FeatureID.Value).IsDeleted == false)
                    List.Features.Add(new NowBuySell.Web.ViewModels.Cars.Featues
                    {
                        Name = lang == "ar" ? _featureService.GetFeature((int)entry.FeatureID).NameAR : _featureService.GetFeature((int)entry.FeatureID).Name,
                        Icon = ImageServer + "/" + entry.Feature.Image
                    }
                );
            }


            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                status = "success",
                message = "",
                Data = List
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("car/getById/{lang}/{Id}")]
        public HttpResponseMessage GetByid(long Id, string lang = "en")
        {
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims;
            long customerId = 0, WishlistID = 0;
           
            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
            { }

            var car = _carService.GetCar(Id);

            if (car == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new
                {
                    status = "error",
                    message = "Motor not found!"
                });
            }

            if (car.CustomerWishlists.Count > 0 && customerId != 0)
            {
                var value = car.CustomerWishlists.Where(x => x.CustomerID == customerId).FirstOrDefault();
                WishlistID = value != null ? value.ID : 0;
            }
            
           

            var carFeature = _carFeatureService.GetAllFeatureByCarID(Id, ImageServer);
            var carMake = car.CarMakeID.HasValue ? _carMakeService.GetCarMake((long)car.CarMakeID) : null;
            var carModel = car.CarModelID.HasValue ? _carModelService.GetCarModel((long)car.CarModelID) : null;
            var bodyType = car.BodyTypeID.HasValue ? _carBodyTypeService.GetBodyType((long)car.BodyTypeID) : null;
            var carinspection = car.CarInspections.Count>0? car.CarInspections.FirstOrDefault().Path: null;
            var carImage = _carImageService.GetCarImages(Id);
            var vendor = _vendorService.GetVendor((long)car.VendorID);
            var carTags = _carTagService.GetAllTagsByCarID(Id, lang);
            var CarCategory = car.CategoryID.HasValue ? _categoryService.GetCategory((long)car.CategoryID) : null;
            var Country = car.CountryID.HasValue ? _countryService.GetCountry((long)car.CountryID) : null;
            var City = car.CityID.HasValue ? _cityService.GetCity((long)car.CityID) : null;


            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                status = "success",
                message = "",
                Data = new
                {
                    Id = car.ID,
                    Slug = car.Slug,
                    LicensePlate = car.LicensePlate,
                    Thumbnail = !string.IsNullOrEmpty(car.Thumbnail) ? ImageServer + car.Thumbnail : "",
                    Title = lang == "ar" ? car.NameAr : car.Name,
                    Description = lang == "ar" ? car.LongDescriptionAr : car.LongDescription,
                    Transmission = lang == "ar" ? ArabicDictionary.Translate(car.Transmission) : car.Transmission,
                    Door = lang == "ar" ? ArabicDictionary.Translate(car.Doors) : car.Doors,
                    Cylinder = car.Cylinders,
                    HorsePower = lang == "ar" ? ArabicDictionary.Translate(car.HorsePower) : car.HorsePower,
                    Wheels = lang == "ar" ? ArabicDictionary.Translate(car.Wheels) : car.Wheels,
                    Capacity = lang == "ar" ? ArabicDictionary.Translate(car.Capacity) : car.Capacity,
                    Price = car.SalePrice.HasValue ? car.SalePrice.Value : car.RegularPrice.Value,
                    Color = car.Color,
                    Condition = car.Condition,
                    FuelType = lang == "ar" ? ArabicDictionary.Translate(car.FuelType) : car.FuelType,
                    year = car.Year,
                    WarrentyEndDate = GetDate(car.WarrentyEndDate),
                    Video = !string.IsNullOrEmpty(car.Video) ? ImageServer + car.Video : null,
                    WishlistId = WishlistID,
                    Latitude = car.Latitude,
                    Longitude = car.Longitude,
                    IsFeatured = !car.IsFeatured.HasValue ? false : car.IsFeatured.Value,
                    IsPremium = !car.IsPremium.HasValue ? false : car.IsPremium.Value,
                    IsSold = !car.IsSold.HasValue ? false : car.IsSold.Value,
                    IsVerified = !car.IsVerified.HasValue ? false : car.IsVerified.Value,
                    AdPostedDate = GetDate(car.CreatedOn),
                    SteeringSide = lang == "ar" ? ArabicDictionary.Translate(car.SteeringSide, false) : car.SteeringSide,
                    MechanicalCondition = lang == "ar" ? ArabicDictionary.Translate(car.MechanicalCondition, false) : car.MechanicalCondition,
                    ServiceHistory = !car.ServiceHistory.HasValue ? false : car.ServiceHistory.Value,
                    RegionalSpecification = lang == "ar" ? ArabicDictionary.Translate(car.RegionalSpecification, false) : car.RegionalSpecification,
                    Mileage = car.FuelEconomy,
                    Warranty = !car.Warranty.HasValue ? false : car.Warranty.Value,
                    EngineDisplacement = lang == "ar" ? ArabicDictionary.Translate(car.EngineDisplacement) : car.EngineDisplacement,
                    AdsReferenceCode = car.AdsReferenceCode,
                    ListingType = car.IsPremium == true ? "Premium" : "Normal",
                    SoldDate = GetSoldDate(car.SoldDate,car.IsSold),
                    carInspection = !string.IsNullOrEmpty(carinspection) ? ImageServer + carinspection : null,
                    carInspectionName = car.CarInspections.Count > 0 ? car.CarInspections.FirstOrDefault().Name : null,
                    Vendor = vendor != null ? new
                    {
                        id = vendor.ID,
                        name = lang == "en" ? vendor.Name : vendor.NameAr,
                        image = !string.IsNullOrEmpty(vendor.Logo) ? ImageServer + vendor.Logo : null,
                        vendor.Contact,
                        vendor.Mobile,
                        vendor.Facebook,
                        vendor.Whatsapp,
                        vendor.Snapchat,
                        vendor.Instagram,
                        vendor.Twitter,
                        vendor.TikTok,
                        vendor.Youtube,
                        vendor.LinkedIn,
                        rerano = vendor.RERANo,
                        dedno = vendor.DEDNo,
                        permitno = vendor.PermitNo,
                        vendor.VendorPackage.hasMotorModule,
                        vendor.VendorPackage.hasPropertyModule,
                    } : null,
                    Address = new
                    {
                        Country = Country != null ? new
                        {
                            id = Country.ID,
                            name = lang == "en" ? Country.Name : Country.NameAr,
                        } : null,
                        City = City != null ? new
                        {
                            id = City.ID,
                            name = lang == "en" ? City.Name : City.NameAR,
                        } : null,
                        Area = car.Area,
                        Address = car.Address,
                    },
                    Category = CarCategory != null ? new
                    {
                        id = CarCategory.ID,
                        name = lang == "en" ? CarCategory.CategoryName : CarCategory.CategoryNameAr,
                    } : null,
                    Make = carMake != null ? new
                    {
                        id = carMake.ID,
                        name = lang == "en" ? carMake.Name : carMake.NameAR,
                    } : null,
                    Model = carModel != null ? new
                    {
                        id = carModel.ID,
                        name = lang == "en" ? carModel.Name : carModel.NameAR,
                    } : null,
                    BodyType = bodyType != null ? new
                    {
                        id = bodyType.ID,
                        name = lang == "en" ? bodyType.Name : carModel.NameAR,
                    } : null,
                    Images = carImage.Select(i => new
                    {
                        id = i.ID,
                        image = !string.IsNullOrEmpty(i.Image) ? ImageServer + i.Image : null,
                    }),
                    features = carFeature,
                }
            });
        }

        [HttpGet]
        [Route("{lang:maxlength(2)}/cars/{carId}")]
        public HttpResponseMessage GetCarDetails(long carId, string lang = "en")
        {
            try
            {
                string ImageServer = CustomURL.GetImageServer();
                var car = _carService.GetCarDetails(carId, lang, ImageServer);
                var carFeature = _carFeatureService.GetAllFeatureByCarID(carId, ImageServer);
                var carPackage = _carPackageService.GetPriceByCarID(carId);
                var carMake = _carMakeService.GetCarMake((long)car.CarMakeID);
                var carModel = _carModelService.GetCarModel((long)car.CarModelID);
                var bodyType = _carBodyTypeService.GetBodyType((long)car.BodyTypeID);
                var insurance = _carInsuranceService.GetInsuranceByCarID(carId);
                var carImage = _carImageService.GetCarImages(carId);
                var vendor = _vendorService.GetVendor((long)car.VendorID);
                var carTags = _carTagService.GetAllTagsByCarID(carId, lang);


                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",
                    car = new
                    {
                        id = car.ID,
                        sku = car.SKU,
                        title = car.Title,
                        licenseplate = car.LicensePlate,
                        vendor = new
                        {
                            id = vendor.ID,
                            name = lang == "en" ? vendor.Name : vendor.NameAr,
                            image = !string.IsNullOrEmpty(vendor.Logo) ? ImageServer + vendor.Logo : null,
                            latitude = vendor.Latitude,
                            longitude = vendor.Longitude,
                            address = vendor.Address,
                            VendorFacebook = vendor.Facebook != null ? vendor.Facebook : "-",
                            VendorInstagram =vendor.Instagram != null ? vendor.Instagram : "-",
                            VendorTwitter = vendor.Twitter != null ? vendor.Twitter : "-",
                            VendorWhatsapp = vendor.Whatsapp != null ? vendor.Whatsapp : "-",
                            VendorLinkedin = vendor.LinkedIn != null ? vendor.LinkedIn : "-",
                            VendorTikTok = vendor.TikTok != null ? vendor.TikTok : "-",
                        },
                        package = carPackage,
                        feature = carFeature,
                        tags = carTags,
                        Thumbnail = car.Thumbnail,
                        door = car.Doors,
                        cylinders = car.Cylinders,
                        hoursepower = car.HorsePower,
                        regionspecification = car.RegionalSpecification,
                        termsandcondition = car.TermsAndCondition,
                        cancelationpolicy = car.CancelationPolicy,
                        specification = car.Specification,
                        year = car.Year,
                        transmission = car.Transmission,
                        fueleconomy = car.FuelEconomy,
                        capacity = car.Capacity,
                        enableDelivery = car.EnableDelivery,
                        deliveryChargesAmount = car.DeliveryChargesAmount,
                        deliveryChargesType = car.ChargesType,
                        pricePerKilometer = car.PricePerKilometer,
                        IsSold = car.IsSold,
                        IsPremium = car.IsPremium,
                        IsVerified = car.IsVerified,
                        CarMake = new
                        {
                            id = carMake.ID,
                            name = lang == "en" ? carMake.Name : carMake.NameAR,
                        },
                        CarModel = new
                        {
                            id = carModel.ID,
                            name = lang == "en" ? carModel.Name : carModel.NameAR,
                        },
                        bodyType = new
                        {
                            id = bodyType.ID,
                            name = lang == "en" ? bodyType.Name : carModel.NameAR,
                        },
                        insurance = insurance.Select(i => new
                        {
                            id = i.ID,
                            name = lang == "en" ? i.Name : i.NameAr,
                            price = i.Price,
                            description = lang == "en" ? i.Details : i.DetailsAr,
                        }),
                        image = carImage.Select(i => new
                        {
                            id = i.ID,
                            image = ImageServer + i.Image,
                            position = i.Position,
                            title = i.Title,
                        }),

                    }
                }); ;
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
        [Route("{lang}/category/cars")]
        public HttpResponseMessage GetCategories(string lang = "en", int pg = 1)
        {
            try
            {
                string ImageServer = CustomURL.GetImageServer();
                var Categories = _categoryService.GetCategories(ImageServer, lang).Select(i => new
                {
                    id = i.ID,
                    name = i.Name,
                    cars = _carService.GetCategorywsieCars(null, i.ID, lang, ImageServer)
                });
                return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", categories = Categories });
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

        [HttpGet]
        [Route("{lang}/cars/{carId}/relatedcars")]
        public HttpResponseMessage GetrelatedCars(long carId, string lang = "en")
        {
            try
            {
                string ImageServer = CustomURL.GetImageServer();
                var Cars = _carService.GetRelatedCars(carId, lang, ImageServer);
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",
                    cars = Cars
                });
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

        [HttpGet]
        [Route("years")]
        public HttpResponseMessage GetYear()
        {
            try
            {
                var cars = _carService.GetCars();
                var years =
                   (from car in cars
                    orderby car.Year ascending
                    select car.Year).Distinct();

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",
                    year = years
                });
            }

            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }



        }

        [HttpPost]
        [Route("car/Image/{id}")]
        public async Task<HttpResponseMessage> Thumbnail(long id)
        {
            string message = string.Empty;
            string Thumbnailpath = string.Empty, Videopath = string.Empty, Gallerypath = string.Empty, messages = string.Empty;

            try
            {
                Car car = _carService.GetCar(id);
                if (car == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                string ThumbnailfilePath = string.Format("/Assets/AppFiles/Images/Car/{0}/", car.SKU.Replace(" ", "_"));
                string rootThumbnail = HttpContext.Current.Server.MapPath(ThumbnailfilePath);
                var providerThumbnail = new CustomMultipartFormDataStreamProvider(rootThumbnail);

                if (!Directory.Exists(HttpContext.Current.Server.MapPath(string.Format("/Assets/AppFiles/Images/Car/{0}/", car.SKU.Replace(" ", "_")))))
                {
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(string.Format("/Assets/AppFiles/Images/Car/{0}/", car.SKU.Replace(" ", "_"))));
                }

                CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(providerThumbnail);

                List<PathNameList> list = file.filePathList;

                List<PathNameList> gallery = list.Where(x => x.Type == "\"gallery\"").ToList();
                PathNameList thumbnail = list.Where(x => x.Type == "\"thumbnail\"").FirstOrDefault();
                PathNameList video = list.Where(x => x.Type == "\"video\"").FirstOrDefault();
                List<PathNameList> document = list.Where(x => x.Type == "\"document\"").ToList();

                if (thumbnail != null)
                {
                    Thumbnailpath = ThumbnailfilePath + "/" + thumbnail.LocalPath;
                    car.Thumbnail = Thumbnailpath.Replace("///", "/");

                    _carService.UpdateCar(ref car, ref message, false);
                }

                if (video != null)
                {

                    Videopath = ThumbnailfilePath + "/" + video.LocalPath;
                    car.Video = Videopath.Replace("///", "/");

                    _carService.UpdateCar(ref car, ref message, false);
                }

                if (gallery != null && gallery.Count() > 0)
                {
                    foreach (var entry in gallery)
                    {

                        CarImage imageModel = new CarImage();

                        Gallerypath = ThumbnailfilePath + "/" + entry.LocalPath;

                        imageModel.CarID = id;
                        imageModel.Image = Gallerypath.Replace("///", "/");

                        _carImageService.CreateCarImage(ref imageModel, ref messages);
                    }
                }

                if (document != null && document.Count() > 0)
                {
                    foreach (var entry in document)
                    {

                        CarDocument documentModel = new CarDocument();

                        Gallerypath = ThumbnailfilePath + "/" + entry.LocalPath;

                        documentModel.CarID = id;
                        documentModel.Path = Gallerypath.Replace("///", "/");

                        _carDocumentService.CreateDocument(ref documentModel, ref messages);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    message = "There is a problem removing thumbnail, please try again later."
                });
            }
        }

        [HttpDelete]
        [Route("car/gallery/delete/{id}")]
        public HttpResponseMessage Delete(long id)
        {
            string message = string.Empty;
            CarImage car = _carImageService.GetCarImage(id);

            string Path = HttpContext.Current.Server.MapPath(car.Image);

            if (File.Exists(Path))
                File.Delete(Path);

            if (_carImageService.DeleteCarImage(id, ref message, ref Path))
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = true,
                    message = message,
                });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                success = false,
                message = message,
            });
        }

        [HttpPost]
        [Route("car/addImage/{id}")]
        public async Task<HttpResponseMessage> CreateImage(long id)
        {
            try
            {
                Car car = _carService.GetCar((long)id);
                if (car == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                CarImage carImage = new CarImage();
                string message = string.Empty;
                string filePath = string.Format("/Assets/AppFiles/Images/Car/{0}/Gallery/", car.SKU.Replace(" ", "_"));
                string root = HttpContext.Current.Server.MapPath(filePath);
                var provider = new CustomMultipartFormDataStreamProvider(root);

                if (!Directory.Exists(HttpContext.Current.Server.MapPath(string.Format("/Assets/AppFiles/Images/Car/{0}/Gallery/", car.SKU.Replace(" ", "_")))))
                {
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(string.Format("/Assets/AppFiles/Images/Car/{0}/Gallery/", car.SKU.Replace(" ", "_"))));
                }

                CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);

                string path = string.Empty;
                if (file.Contents.Count() > 0)
                {
                    path = filePath + "/" + file.filePath;
                    carImage.CarID = id;
                    carImage.Image = path.Replace("///", "/");

                    if (_carImageService.CreateCarImage(ref carImage, ref message))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            success = true,
                            message = message,
                            data = path
                        });
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    message = "Oops! Something went wrong. Please try later."
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("car/maxValue")]
        public HttpResponseMessage MaxPrice()
        {
            decimal MaxPrice = _carService.GetMaxPrice();

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, Value = MaxPrice });
        }

        //[HttpPost]
        //[Route("Car/Request")]
        //public HttpResponseMessage CreateCarRequest(CarRequest Model)
        //{
        //    string message = string.Empty;
        //    var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
        //    var claims = identity.Claims.ToList();
        //    long customerId;
        //    if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
        //    {
        //        Model.CustomerID = customerId;
        //        Model.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
        //        if (_carRequestService.AddCarRequests(Model, ref message))
        //        {
        //            IEnumerable<vw_CarRequests> VendorList = _carRequestService.GetVendorListFromVMByRequestID(Model.ID);

        //            if (VendorList.Count() > 0)
        //            {
        //                Notification not = new Notification();
        //                not.Title = "Car Request";
        //                not.Description = "New car added for request ";
        //                not.OriginatorID = customerId;
        //                not.OriginatorName = "";
        //                not.Url = "/Vendor/CarRequests/Index";
        //                not.Module = "CarRequest";
        //                not.OriginatorType = "Customer";
        //                not.RecordID = Model.ID;
        //                if (_notificationService.CreateNotification(not, ref message))
        //                {
        //                    foreach (var id in VendorList)
        //                    {
        //                        NotificationReceiver notRec = new NotificationReceiver();
        //                        notRec.ReceiverID = id.VendorID;
        //                        notRec.ReceiverType = "Vendor";
        //                        notRec.NotificationID = not.ID;
        //                        _notificationReceiverService.CreateNotificationReceiver(notRec, ref message);
        //                    }
        //                }
        //            }
        //            return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", message = "Request Sent successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { status = "failed", message = message });
        //    }

        //    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "failed", message = "Authorization failed for current request" });
        //}

        private string GetDate(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "";

            string Month = dateTime.Value.ToString("MMM");

            return dateTime.Value.Day + " " + Month + " " + dateTime.Value.Year;
        }

        [HttpPut]
        [Route("cars/{carId}/callcount")]
        public HttpResponseMessage UpdateCallCount(long carId, string lang = "en")
        {
            try
            {
                string message = string.Empty;
                var car = _carService.GetCar(carId);
                if (car != null)
                {
                    if (car.CallCount.HasValue)
                    {
                        car.CallCount += 1;
                    }
                    else
                    {
                        car.CallCount = 1;
                    }

                    if (_carService.UpdateCar(ref car, ref message, true, true))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = lang == "en" ? "Call count updated!" : ArabicDictionary.Translate("Call count updated!", false)
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                        {
                            status = "failure",
                            message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                        });
                    }

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, new
                    {
                        status = "error",
                        message = lang == "en" ? "Car doesn't exist!" : ArabicDictionary.Translate("Car doesn't exist!", false)
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
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