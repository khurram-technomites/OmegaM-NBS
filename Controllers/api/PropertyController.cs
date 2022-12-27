using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using NowBuySell.Data;
using NowBuySell.Data.HelperClasses;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Property;
using NowBuySell.Web.ViewModels.Api.CustomRequest;
using Newtonsoft.Json;
using static NowBuySell.Data.HelperClasses.MapResponseViewModel;

namespace NowBuySell.Web.Controllers.api
{
    [Authorize]
    [RoutePrefix("api/v1")]
    public class PropertyController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IPropertyService _propService;
        private readonly ICityService _cityService;
        private readonly IVendorService _vendorService;
        private readonly ICountryService _countryService;
        private readonly ICategoryService _categoryService;
        private readonly IFeatureService _featureService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;
        private readonly IPropertyFeaturesService _propfeatureService;
        private readonly IPropertyImagesService _propImageService;
        private readonly IPropertyFloorPlanService _floorPlanService;
        private readonly IPropertyRequestsService _propRequestService;
        private readonly IPropertyFloorPlanService _propFloorPlanService;
        private readonly INearByPlaceService _nearByPlaceService;
        string ImageServer = string.Empty;

        public PropertyController(IPropertyService propService
            , ICityService cityService
            , ICountryService countryService
            , IVendorService vendorService
            , ICategoryService categoryService
            , IFeatureService featureService
            , INotificationReceiverService notificationReceiverService
            , INotificationService notificationService
            , IPropertyFeaturesService propfeatureService
            , IPropertyImagesService propImageService
            , IPropertyFloorPlanService floorPlanService
            , IPropertyRequestsService propRequestService
            , IPropertyFloorPlanService propFloorPlanService
             ,INearByPlaceService nearByPlaceService)
        {
            _propService = propService;
            _cityService = cityService;
            _vendorService = vendorService;
            _countryService = countryService;
            _categoryService = categoryService;
            _featureService = featureService;
            _notificationReceiverService = notificationReceiverService;
            _notificationService = notificationService;
            _propfeatureService = propfeatureService;
            _propImageService = propImageService;
            _floorPlanService = floorPlanService;
            _propRequestService = propRequestService;

            ImageServer = CustomURL.GetImageServer();
            _nearByPlaceService = nearByPlaceService;
        }

        [AllowAnonymous]
        [Route("places/{place}")]
        public async Task<object> GetPlaces(string Place)
        {

            HttpClient _client = new HttpClient();
            HttpResponseMessage httpResponse = await _client.GetAsync($"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={Place}&inputtype=textquery&key=AIzaSyAEmhLaFjth5xau57Gy1NwE1O6apk443xY&components=country:ae");
            ////HttpResponseMessage httpResponse = await _client.GetAsync($"https://maps.googleapis.com/maps/api/place/textsearch/json?query={Place}&key={_key}");


            var data = await httpResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<object>(data);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("property/filter/{lang}/get")]
        public HttpResponseMessage Get([FromBody] PropertyFilterViewModel Model, string lang = "en")
        {

            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims;
            long customerId = 0;
            string address = string.Empty;

            HttpClient _client = new HttpClient();


            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                Model.CustomerID = (int)customerId;

            if(Model.PageSize% 40==0)
            {
                var Skip = Model.Skip / 40;
                Model.Skip = Skip * 8;
                Model.PageSize = 8;

            }
            Model.Locations = new List<LocationModel>();
            if (Model.Longitude != 0 && Model.Latitude != 0)
            {
                LocationModel loc = new LocationModel();


                loc.Latitude = Model.Latitude;
                loc.Longitude = Model.Longitude;
                loc.Distance = Model.Distance == 0 ? 1 : Model.Distance;
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
            
              


            var result = _propService.GetPropertiesWithFilter(Model);
            List<PropertyResultList> resultList = new List<PropertyResultList>();
            List<VendorSocialLinks> VendorSocialLinks = new List<VendorSocialLinks>();
            if (Model.VendorID!=null)
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
                long WishlistID = 0;
                double OldPrice;
                string NoOfGarage;

                if (record.CustomerWishlists.Count > 0 && customerId != 0)
                {
                    var value = record.CustomerWishlists.Where(x => x.CustomerID == customerId).FirstOrDefault();
                    WishlistID = value != null ? value.ID : 0;
                }
                List<string> carImg = _propImageService.GetImagesByProperty((int)record.ID).Select(x => ImageServer + x.Path).ToList();
                NoOfGarage = (record.NoOfGarage.HasValue ? record.NoOfGarage.Value : 0) + " Garage";
                OldPrice = record.OldPrice.HasValue ? record.OldPrice.Value : 0;
               

                resultList.Add(new PropertyResultList
                {
                    Id = (long)record.ID,
                    CountryId = record.CountryID.HasValue ? (long)record.CountryID.Value : 0,
                    CityId = record.CityID.HasValue ? (long)record.CityID.Value : 0,
                    CategoryId = (long)record.CategoryId,
                    Title = lang == "ar" ? record.TitleAr : record.Title,
                    Description = lang == "ar" ? record.DescriptionAr : record.Description,
                    Address = record.Address,
                    Slug = record.Slug,
                    Rooms = lang == "ar" ? (record.NoOfRooms != null ? record.NoOfRooms.Value : 0) + " " + ArabicDictionary.Translate("Beds") : (record.NoOfRooms != null ? record.NoOfRooms.Value : 0) + " Beds",
                    Baths = lang == "ar" ? (record.NoOfBaths != null ? record.NoOfBaths.Value : 0) + " " + ArabicDictionary.Translate("Baths") : (record.NoOfBaths != null ? record.NoOfBaths.Value : 0) + " Baths",
                    Garages = lang == "ar" ? (record.NoOfGarage != null ? record.NoOfGarage.Value : 0) + " " + ArabicDictionary.Translate("Garage") : (record.NoOfGarage != null ? record.NoOfGarage.Value : 0) + " Garage",
                    Size = lang == "ar" ? (record.Size != null ? record.Size.Value : 0) + " " + ArabicDictionary.Translate("Sqft") : (record.Size != null ? record.Size.Value : 0) + " sqft",
                    Status = record.ForSale ? lang == "ar" ? ArabicDictionary.Translate("Sale") : "Sale" : lang == "ar" ? ArabicDictionary.Translate("Rent") : "Rent",
                    OldPrice = OldPrice,
                    Price = record.Price != null ? record.Price.Value : 0,
                    Thumbnail = ImageServer + "/" + record.Thumbnail,
                    WishlistId = WishlistID,
                    PropertyType = record.Category != null ? record.Category.PropertyType : "-",
                    VendorThumbnail = record.Customer != null ? ImageServer + "/" + record.Customer.Logo : record.Vendor != null ? ImageServer + "/" + record.Vendor.Logo : null,
                    VendorID = record.VendorId ?? 0,
                    VendorFacebook = record.Vendor.Facebook != null ? record.Vendor.Facebook : "-",
                    VendorInstagram = record.Vendor.Instagram != null ? record.Vendor.Instagram : "-",
                    VendorTwitter = record.Vendor.Twitter != null ? record.Vendor.Twitter : "-",
                    VendorWhatsapp = record.Vendor.Whatsapp != null ? record.Vendor.Whatsapp : "-",
                    VendorLinkedin = record.Vendor.LinkedIn != null ? record.Vendor.LinkedIn : "-",
                    VendorYoutube = record.Vendor.Youtube != null ? record.Vendor.Youtube : "-",
                    VendorSnapchat = record.Vendor.Snapchat != null ? record.Vendor.Snapchat : "-",
                    VendorTikTok = record.Vendor.TikTok != null ? record.Vendor.TikTok : "-",
                    VendorName = record.Vendor.Name,
                    VendorMobile = record.Vendor.Mobile,
                    PermitNo = record.Vendor.PermitNo,
                    VendorContact = record.Vendor.Contact,
                    AdsReferenceCode = record.AdsReferenceCode,
                    IsVerified = record.IsVerified == null || record.IsVerified == false ? false : true,
                    IsPremium = record.IsPremium == null || record.IsPremium == false ? false : true,
                    IsSold = record.IsSold == null || record.IsSold == false ? false : true,
                    Images = carImg.Take(4).ToList(),
                    ListingType = "Normal",
                    Latitude = record.Latitude,
                    Longitude = record.Longitude,
                    SoldDate = GetSoldDate(record.SoldDate, record.IsSold),
                });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                status = "success",
                address = Model.Locations.Select(x => new { x.PlaceID, x.Address }).ToArray(),
                Data = resultList,
                SocialLinks= VendorSocialLinks
            });
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("property/premium/{lang}")]
        public HttpResponseMessage GetPremiumProperties([FromBody] PropertyFilterViewModel Model, string lang = "en")
        {
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims;
            long customerId = 0;

            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                Model.CustomerID = (int)customerId;
            //bool ForSale = Model.ForSale.HasValue ? Model.ForSale.Value : true;

            var result = _propService.GetPremiumProperty(Model.ForSale);


            List<PropertyResultList> resultList = new List<PropertyResultList>();
            foreach (var record in result)
            {
                long WishlistID = 0;
                if (record.CustomerWishlists.Count > 0 && customerId != 0)
                {
                    var value = record.CustomerWishlists.Where(x => x.CustomerID == customerId).FirstOrDefault();
                    WishlistID = value != null ? value.ID : 0;
                }
                List<PropertyImage> images = new List<PropertyImage>();
                List<string> propertyImg = _propImageService.GetImagesByProperty((int)record.ID).Select(x => ImageServer + x.Path).ToList();

                resultList.Add(new PropertyResultList
                {
                    Id = (int)record.ID,
                    CountryId = record.CountryID.HasValue ? (int)record.CountryID.Value : 0,
                    CityId = record.CityID.HasValue ? (int)record.CityID.Value : 0,
                    CategoryId = (int)record.CategoryId,
                    Title = lang == "ar" ? record.TitleAr : record.Title,
                    Description = lang == "ar" ? record.DescriptionAr : record.Description,
                    Address = record.Address,
                    Slug = record.Slug,
                    Rooms = lang == "ar" ? (record.NoOfRooms != null ? record.NoOfRooms.Value : 0) + " " + ArabicDictionary.Translate("Beds") : (record.NoOfRooms != null ? record.NoOfRooms.Value : 0) + " Beds",
                    Baths = lang == "ar" ? (record.NoOfBaths != null ? record.NoOfBaths.Value : 0) + " " + ArabicDictionary.Translate("Baths") : (record.NoOfBaths != null ? record.NoOfBaths.Value : 0) + " Baths",
                    Garages = lang == "ar" ? (record.NoOfGarage != null ? record.NoOfGarage.Value : 0) + " " + ArabicDictionary.Translate("Garage") : (record.NoOfGarage != null ? record.NoOfGarage.Value : 0) + " Garage",
                    Size = lang == "ar" ? (record.Size != null ? record.Size.Value : 0) + " " + ArabicDictionary.Translate("Sqft") : (record.Size != null ? record.Size.Value : 0) + " sqft",
                    Status = record.ForSale ? lang == "ar" ? ArabicDictionary.Translate("Sale") : "Sale" : lang == "ar" ? ArabicDictionary.Translate("Rent") : "Rent",
                    OldPrice = record.OldPrice.HasValue ? record.OldPrice.Value : 0,
                    Price = record.Price != null ? record.Price.Value : 0,
                    PropertyType = record.Category != null ? record.Category.PropertyType : "-",
                    Thumbnail = ImageServer + "/" + record.Thumbnail,
                    WishlistId = WishlistID,
                    VendorThumbnail = record.Customer != null ? ImageServer + "/" + record.Customer.Logo : record.Vendor != null ? ImageServer + "/" + record.Vendor.Logo : null,
                    VendorID = (int)(record.Vendor != null ? record.Vendor.ID : 0),
                    VendorFacebook = record.Vendor.Facebook != null ? record.Vendor.Facebook : "-",
                    VendorInstagram = record.Vendor.Instagram != null ? record.Vendor.Instagram : "-",
                    VendorTwitter = record.Vendor.Twitter != null ? record.Vendor.Twitter : "-",
                    VendorWhatsapp = record.Vendor.Whatsapp != null ? record.Vendor.Whatsapp : "-",
                    VendorLinkedin = record.Vendor.LinkedIn != null ? record.Vendor.LinkedIn : "-",
                    VendorYoutube = record.Vendor.Youtube != null ? record.Vendor.Youtube : "-",
                    VendorSnapchat = record.Vendor.Snapchat != null ? record.Vendor.Snapchat : "-",
                    VendorTikTok = record.Vendor.TikTok != null ? record.Vendor.TikTok : "-",
                    VendorName = record.Vendor.Name,
                    VendorMobile = record.Vendor.Mobile,
                    PermitNo = record.Vendor.PermitNo,
                    VendorContact = record.Vendor.Contact,
                    AdsReferenceCode = record.AdsReferenceCode,
                    IsVerified = !record.IsVerified.HasValue ? false : record.IsVerified.Value,
                    IsPremium = !record.IsPremium.HasValue ? false : record.IsPremium.Value,
                    IsSold = !record.IsSold.HasValue ? false : record.IsSold.Value,
                    ForSale = record.ForSale,
                    Images = propertyImg.Take(4).ToList(),
                    ListingType = "Premium"
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
        [Route("property/Trending/{lang}/get")]
        public HttpResponseMessage GetTrending([FromBody] PropertyFilterViewModel Model, string lang = "en")
        {

            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims;
            long customerId = 0;
            double OldPrice;

            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                Model.CustomerID = (int)customerId;

            var result = _propService.GetTrendingList(Model);
            List<PropertyResultList> resultList = new List<PropertyResultList>();

            foreach (var record in result)
            {
                long WishlistID = 0;
                if (record.CustomerWishlists.Count > 0 && customerId != 0)
                {
                    var value = record.CustomerWishlists.Where(x => x.CustomerID == customerId).FirstOrDefault();
                    WishlistID = value != null ? value.ID : 0;
                }
                List<string> propertyImg = _propImageService.GetImagesByProperty((int)record.ID).Select(x => ImageServer + x.Path).ToList();
                OldPrice = record.OldPrice.HasValue ? record.OldPrice.Value : 0;

                resultList.Add(new PropertyResultList
                {
                    Id = (int)record.ID,
                    CountryId = record.CountryID.HasValue ? (int)record.CountryID.Value : 0,
                    CityId = record.CityID.HasValue ? (int)record.CityID.Value : 0,
                    CategoryId = (int)record.CategoryId,
                    Title = lang == "ar" ? record.TitleAr : record.Title,
                    Description = lang == "ar" ? record.DescriptionAr : record.Description,
                    Address = record.Address,
                    Slug = record.Slug,
                    Rooms = lang == "ar" ? (record.NoOfRooms != null ? record.NoOfRooms.Value : 0) + " " + ArabicDictionary.Translate("Beds") : (record.NoOfRooms != null ? record.NoOfRooms.Value : 0) + " Beds",
                    Baths = lang == "ar" ? (record.NoOfBaths != null ? record.NoOfBaths.Value : 0) + " " + ArabicDictionary.Translate("Baths") : (record.NoOfBaths != null ? record.NoOfBaths.Value : 0) + " Baths",
                    Garages = lang == "ar" ? (record.NoOfGarage != null ? record.NoOfGarage.Value : 0) + " " + ArabicDictionary.Translate("Garage") : (record.NoOfGarage != null ? record.NoOfGarage.Value : 0) + " Garage",
                    Size = lang == "ar" ? (record.Size != null ? record.Size.Value : 0) + " " + ArabicDictionary.Translate("Sqft") : (record.Size != null ? record.Size.Value : 0) + " sqft",
                    Status = record.ForSale ? lang == "ar" ? ArabicDictionary.Translate("Sale") : "Sale" : lang == "ar" ? ArabicDictionary.Translate("Rent") : "Rent",
                    OldPrice = OldPrice,
                    Price = record.Price != null ? record.Price.Value : 0,
                    PropertyType = record.Category != null ? record.Category.PropertyType : "-",
                    Thumbnail = ImageServer + "/" + record.Thumbnail,
                    WishlistId = WishlistID,
                    VendorThumbnail = record.Customer != null ? ImageServer + "/" + record.Customer.Logo : record.Vendor != null ? ImageServer + "/" + record.Vendor.Logo : null,
                    VendorID = Model.VendorID ?? 0,
                    VendorFacebook = record.Vendor.Facebook != null ? record.Vendor.Facebook : "-",
                    VendorInstagram = record.Vendor.Instagram != null ? record.Vendor.Instagram : "-",
                    VendorTwitter = record.Vendor.Twitter != null ? record.Vendor.Twitter : "-",
                    VendorWhatsapp = record.Vendor.Whatsapp != null ? record.Vendor.Whatsapp : "-",
                    VendorLinkedin = record.Vendor.LinkedIn != null ? record.Vendor.LinkedIn : "-",
                    VendorYoutube = record.Vendor.Youtube != null ? record.Vendor.Youtube : "-",
                    VendorSnapchat = record.Vendor.Snapchat != null ? record.Vendor.Snapchat : "-",
                    VendorTikTok = record.Vendor.TikTok != null ? record.Vendor.TikTok : "-",
                    VendorName = record.Vendor.Name,
                    VendorMobile = record.Vendor.Mobile,
                    PermitNo = record.Vendor.PermitNo,
                    VendorContact = record.Vendor.Contact,
                    AdsReferenceCode = record.AdsReferenceCode,
                    IsVerified = !record.IsVerified.HasValue ? false : record.IsVerified.Value,
                    IsPremium = !record.IsPremium.HasValue ? false : record.IsPremium.Value,
                    IsSold = !record.IsSold.HasValue ? false : record.IsSold.Value,
                    Images = propertyImg.Take(4).ToList(),
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
        [Route("property/getByID/{lang}/{id}")]
        public HttpResponseMessage GetByID(int id, string lang = "en")
        {
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims;
            long customerId = 0, WishlistID = 0;
            
            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
            { }

            var property = _propService.GetById(id);

            if (property == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new
                {
                    status = "error",
                    message = "Property not found!"
                });
            }

            if (property.CustomerWishlists.Count > 0 && customerId != 0)
            {
                var value = property.CustomerWishlists.Where(x => x.CustomerID == customerId).FirstOrDefault();
                WishlistID = value != null ? value.ID : 0;
            }
            List<GetPropetyAvgPriceByArea_Result> TrendLIst = new List<GetPropetyAvgPriceByArea_Result>();
            double AnnualPrice = 0;
            if (property.Area != null)
            {
                 TrendLIst = _propService.GetTrendPrice(property.Area, Helpers.TimeZone.GetLocalDateTime()).Select(x => new GetPropetyAvgPriceByArea_Result { Month = x.Month, Price = Math.Round((double)x.Price) }).ToList();
                 AnnualPrice =  _propService.GetAnnualPrice(property);
            }
          


            var propertyCategory = _categoryService.GetCategory((long)property.CategoryId);
            var Country = property.CountryID.HasValue ? _countryService.GetCountry((long)property.CountryID) : null;
            var City = property.CityID.HasValue ? _cityService.GetCity((long)property.CityID) : null;
            var propertyinspection = property.PropertyInspections.Count > 0 ? property.PropertyInspections.FirstOrDefault().Path : null;
            var propertyFeatures = _propfeatureService.GetAllFeatureByPropertyID(id, ImageServer);
            var propertyImages = _propImageService.GetImagesByProperty(id);
            var floorPlans = _floorPlanService.GetFloorPlansByProperty(id);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                status = "success",
                message = "",
                Data = new
                {

                    Id = (int)property.ID,
                    Slug = property.Slug,
                    Status = property.ForSale ? lang == "ar" ? ArabicDictionary.Translate("Sale") : "Sale" : lang == "ar" ? ArabicDictionary.Translate("Rent") : "Rent",
                    Thumbnail = !string.IsNullOrEmpty(property.Thumbnail) ? ImageServer + property.Thumbnail : "",
                    Title = lang == "ar" ? property.TitleAr : property.Title,
                    Description = lang == "ar" ? property.DescriptionAr : property.Description,

                    Rooms = lang == "ar" ? (property.NoOfRooms != null ? property.NoOfRooms.Value : 0) : (property.NoOfRooms != null ? property.NoOfRooms.Value : 0),
                    Baths = lang == "ar" ? (property.NoOfBaths != null ? property.NoOfBaths.Value : 0) : (property.NoOfBaths != null ? property.NoOfBaths.Value : 0),
                    Laundry = lang == "ar" ? (property.NoOfLaundry.HasValue ? property.NoOfLaundry.Value : 0) : (property.NoOfLaundry.HasValue ? property.NoOfLaundry.Value : 0),
                    Dinning = lang == "ar" ? (property.NoOfDinings.HasValue ? property.NoOfDinings.Value : 0) : (property.NoOfDinings.HasValue ? property.NoOfDinings.Value : 0),
                    Garages = lang == "ar" ? (property.NoOfGarage != null ? property.NoOfGarage.Value : 0) : (property.NoOfGarage != null ? property.NoOfGarage.Value : 0),
                    Size = lang == "ar" ? (property.Size != null ? property.Size.Value : 0) : (property.Size != null ? property.Size.Value : 0),
                    OldPrice = property.OldPrice.HasValue ? property.OldPrice.Value : 0,
                    Price = property.Price.HasValue == true ? property.Price.Value : 0,
                    PropertyType = property.Category != null ? property.Category.PropertyType : "-",
                    BuildYear = property.BuildYear.HasValue == true ? property.BuildYear.Value : 0,
                    Video = !string.IsNullOrEmpty(property.Video) ? ImageServer + property.Video : null,
                    Latitude = property.Latitude,
                    Longitude = property.Longitude,
                    IsFeatured = !property.IsFeatured.HasValue ? false : property.IsFeatured.Value,
                    IsPremium = !property.IsPremium.HasValue ? false : property.IsPremium.Value,
                    IsVerified = !property.IsVerified.HasValue ? false : property.IsVerified.Value,
                    IsSold = !property.IsSold.HasValue ? false : property.IsSold.Value,
                    WishlistId = WishlistID,
                    IsFurnished = property.IsFurnished ?? false,
                    AdPostedDate = GetDate(property.CreatedOn),
                    LastUpdationDate = GetDate(property.ModifiedDate),
                    AdsReferenceCode = property.AdsReferenceCode,
                    ListingType = property.IsPremium == true ? "Premium" : "Normal",
                    SoldDate = GetSoldDate(property.SoldDate, property.IsSold),
                    propertyInspection = !string.IsNullOrEmpty(propertyinspection) ? ImageServer + propertyinspection : null,
                    propertyInspectionName = property.PropertyInspections.Count > 0 ? property.PropertyInspections.FirstOrDefault().Name : null,
                    TrendPrice = TrendLIst,
                    AnnualPrice = AnnualPrice,
                    Vendor = property.Vendor != null ? new
                    {
                        id = property.Vendor.ID,
                        name = lang == "en" ? property.Vendor.Name : property.Vendor.NameAr,
                        image = !string.IsNullOrEmpty(property.Vendor.Logo) ? ImageServer + property.Vendor.Logo : null,
                        property.Vendor.Contact,
                        property.Vendor.Mobile,
                        property.Vendor.Facebook,
                        property.Vendor.Snapchat,
                        property.Vendor.Whatsapp,
                        property.Vendor.Twitter,
                        property.Vendor.Instagram,
                        property.Vendor.Youtube,
                        property.Vendor.LinkedIn,
                        property.Vendor.TikTok,
                        rerano = property.Vendor.RERANo,
                        permitno = property.Vendor.PermitNo,
                        dedno = property.Vendor.DEDNo,
                        property.Vendor.VendorPackage.hasMotorModule,
                        property.Vendor.VendorPackage.hasPropertyModule,

                    } : null,
                    NearByPlace = property.NearByPlaces.Where(X=>X.IsDeleted == false).Select(i => new
                    {
                        Name = lang == "ar" ? i.NameAr : i.Name,
                        Icon = ImageServer + "/" + i.NearByPlacesCategory.Image,
                        CategoryName = lang == "ar" ? i.NearByPlacesCategory.NameAr : i.NearByPlacesCategory.Name,
                        Latitude = i.Latitude,
                        Longitude = i.Longitude,
                        Distance = i.Distance,
                    }),
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
                        State = property.State,
                        Zipcode = property.ZipCode,
                        Area = property.Area,
                        Address = property.Address,
                        property.Latitude,
                        property.Longitude,
                    },
                    Category = propertyCategory != null ? new
                    {
                        id = propertyCategory.ID,
                        name = lang == "en" ? propertyCategory.CategoryName : propertyCategory.CategoryNameAr,
                    } : null,
                    Images = propertyImages.Select(i => new
                    {
                        id = i.ID,
                        image = !string.IsNullOrEmpty(i.Path) ? ImageServer + i.Path : null,
                    }),
                    floorPlans = floorPlans.Select(i => new
                    {
                        id = i.ID,
                        image = !string.IsNullOrEmpty(i.Path) ? ImageServer + i.Path : null,
                    }),
                    features = propertyFeatures,
                }
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("property/getBySlug/{lang}/{slug}")]
        public HttpResponseMessage GetBySlug(string slug, string lang = "en")
        {
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims;
            long customerId = 0, WishlistID = 0;
            
            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
            { }

            var record = _propService.GetBySlug(slug);

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
        
            List<GetPropetyAvgPriceByArea_Result> TrendLIst = new List<GetPropetyAvgPriceByArea_Result>();
            double AnnualPrice = 0;
            if (record.Area != null)
            {
                TrendLIst = _propService.GetTrendPrice(record.Area, Helpers.TimeZone.GetLocalDateTime()).Select(x => new GetPropetyAvgPriceByArea_Result {Month= x.Month,Price= Math.Round((double)x.Price )}).ToList();
                AnnualPrice = _propService.GetAnnualPrice(record);
            }
            var propertyinspection = record.PropertyInspections.Count > 0 ? record.PropertyInspections.FirstOrDefault().Path : null;
          
            PropertyResultList List = new PropertyResultList();

            List.Id = (int)record.ID;
            List.Title = lang == "ar" ? record.TitleAr : record.Title;
            List.Description = lang == "ar" ? record.DescriptionAr : record.Description;
            List.Rooms = lang == "ar" ? (record.NoOfRooms != null ? record.NoOfRooms.Value : 0) + " " + ArabicDictionary.Translate("Beds") : (record.NoOfRooms != null ? record.NoOfRooms.Value : 0) + " Beds";
            List.Baths = lang == "ar" ? (record.NoOfBaths != null ? record.NoOfBaths.Value : 0) + " " + ArabicDictionary.Translate("Baths") : (record.NoOfBaths != null ? record.NoOfBaths.Value : 0) + " Baths";
            List.Garages = lang == "ar" ? (record.NoOfGarage != null ? record.NoOfGarage.Value : 0) + " " + ArabicDictionary.Translate("Garage") : (record.NoOfGarage != null ? record.NoOfGarage.Value : 0) + " Garage";
            List.Size = lang == "ar" ? (record.Size != null ? record.Size.Value : 0) + " " + ArabicDictionary.Translate("Sqft") : (record.Size != null ? record.Size.Value : 0) + " sqft";
            List.Price = record.Price.HasValue == true ? record.Price.Value : 0;
            List.Thumbnail = ImageServer + "/" + record.Thumbnail;
            List.Address = record.Address;
            List.City = lang == "ar" ? _cityService.GetCity((int)record.CityID.Value).NameAR : _cityService.GetCity((int)record.CityID.Value).Name;
            List.Country = lang == "ar" ? record.CountryID.HasValue ? _countryService.GetCountry((int)record.CountryID.Value).NameAr : "دبي" : record.CountryID.HasValue ? _countryService.GetCountry((int)record.CountryID.Value).Name : "Dubai";
            List.Area = record.Area;
            List.State = record.State;
            List.Zipcode = record.ZipCode;
            List.Category = lang == "ar" ? _categoryService.GetCategory((int)record.CategoryId).CategoryNameAr : _categoryService.GetCategory((int)record.CategoryId).CategoryName;
            List.Status = record.ForSale ? lang == "ar" ? ArabicDictionary.Translate("Sale") : "Sale" : lang == "ar" ? ArabicDictionary.Translate("Rent") : "Rent";
            List.BuildYear = record.BuildYear.HasValue == true ? record.BuildYear.Value : 0;
            List.PropertyType = record.Category != null ? record.Category.PropertyType : "-";
            List.OldPrice = record.OldPrice.HasValue ? record.OldPrice.Value : 0;
            List.Address = record.Address;
            List.Slug = record.Slug;
            List.WishlistId = WishlistID;
            List.Video = !string.IsNullOrEmpty(record.Video) ? ImageServer + record.Video : null;
            List.Longitude = record.Longitude;
            List.Latitude = record.Latitude;
            List.IsFeatured = record.IsFeatured ?? false;
            List.IsFurnished = record.IsFurnished ?? false;
            List.IsPremium = !record.IsPremium.HasValue ? false : record.IsPremium.Value;
            List.IsSold = !record.IsSold.HasValue ? false : record.IsSold.Value;
            List.IsVerified = !record.IsVerified.HasValue ? false : record.IsVerified.Value;
            List.AdPostedDate = GetDate(record.CreatedOn);
            List.LastUpdationDate = GetDate(record.ModifiedDate);
            List.Dinings = lang == "ar" ? (record.NoOfDinings.HasValue ? record.NoOfDinings.Value : 0) + " " + ArabicDictionary.Translate("Dinings") : (record.NoOfDinings.HasValue ? record.NoOfDinings.Value : 0) + " Dinings";
            List.Laundries = lang == "ar" ? (record.NoOfLaundry.HasValue ? record.NoOfLaundry.Value : 0) + " " + ArabicDictionary.Translate("Laundry") : (record.NoOfLaundry.HasValue ? record.NoOfLaundry.Value : 0) + " Laundry";
            List.AdsReferenceCode = record.AdsReferenceCode;

            List.VendorID = (int)record.VendorId.Value;
            List.VendorThumbnail = record.CustomerID == null ? ImageServer + "/" + record.Vendor.Logo : ImageServer + "/" + record.Customer.Logo;
            List.VendorName = record.Vendor == null ? "" : record.Vendor.Name;
            List.VendorContact = record.Vendor.Contact;
            List.VendorMobile = record.Vendor.Mobile;
            List.PermitNo = record.Vendor.PermitNo;
            List.RERANo = record.Vendor.RERANo;
            List.DEDNo = record.Vendor.DEDNo;
            List.VendorWhatsapp = record.Vendor.Whatsapp;
            List.VendorTwitter = record.Vendor.Twitter;
            List.VendorInstagram = record.Vendor.Instagram;
            List.VendorFacebook = record.Vendor.Facebook;
            List.VendorLinkedin = record.Vendor.LinkedIn;
            List.VendorSnapchat = record.Vendor.Snapchat;
            List.VendorYoutube = record.Vendor.Youtube;
            List.VendorTikTok = record.Vendor.TikTok;
            List.ListingType = record.IsPremium == true ? "Premium" : "Normal";
            List.SoldDate = GetSoldDate(record.SoldDate, record.IsSold);
            List.propertyInspectionUrl = !string.IsNullOrEmpty(propertyinspection) ? ImageServer + propertyinspection : null;
            List.propertyInspectionName = record.PropertyInspections.Count > 0 ? record.PropertyInspections.FirstOrDefault().Name : null;
            List.TrendPrice = TrendLIst;
            List.AnnualPrice = AnnualPrice;

            foreach (var entry in record.PropertyImages)
                List.Images.Add(ImageServer + "/" + entry.Path);

            foreach (var entry in record.PropertyFloorPlans)
                List.FloorPlans.Add(ImageServer + "/" + entry.Path);

            foreach (var entry in record.PropertyFeatures)
            {
                if (_featureService.GetFeature(entry.FeatureId).IsDeleted == false)
                    List.Features.Add(new Featues
                    {
                        Name = lang == "ar" ? _featureService.GetFeature((int)entry.FeatureId).NameAR : _featureService.GetFeature((int)entry.FeatureId).Name,
                        Icon = ImageServer + "/" + entry.Feature.Image
                    }
                );
            }
            foreach (var entry in record.NearByPlaces)
            {
                if (entry.IsDeleted == false)
                    List.NearByPlaces.Add(new NearByPlaces
                    {
                        Name = lang == "ar" ? entry.NameAr : entry.Name,
                        Icon = ImageServer + "/" + entry.NearByPlacesCategory.Image,
                        CategoryName = lang == "ar" ? entry.NearByPlacesCategory.NameAr:entry.NearByPlacesCategory.Name,
                        Latitude = entry.Latitude,
                        Longitude = entry.Longitude,
                        Distance = entry.Distance,
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

        //[HttpPost]
        //[Route("property/addImage/{id}")]
        //public async Task<HttpResponseMessage> CreateImage(long id)
        //{
        //    try
        //    {
        //        PropertyImage propImage = new PropertyImage();
        //        string message = string.Empty;
        //        string filePath = string.Format("/Assets/AppFiles/Images/Property/{0}/Gallery/", id);
        //        string root = HttpContext.Current.Server.MapPath(filePath);
        //        var provider = new CustomMultipartFormDataStreamProvider(root);

        //        if (!Directory.Exists(HttpContext.Current.Server.MapPath(string.Format("/Assets/AppFiles/Images/Property/{0}/Gallery", id))))
        //        {
        //            Directory.CreateDirectory(HttpContext.Current.Server.MapPath(string.Format("/Assets/AppFiles/Images/Property/{0}/Gallery", id)));
        //        }

        //        CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);               

        //        string path = string.Empty;
        //        if (file.Contents.Count() > 0)
        //        {
        //            path = filePath + "/" + file.filePath;
        //            propImage.PropertyId = (int)id;
        //            propImage.Path = path.Replace("///", "/");

        //            if (_propImageService.Add(ref propImage, ref message))
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, new
        //                {
        //                    success = true,
        //                    message = message,
        //                    data = path
        //                });
        //            }
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new
        //        {
        //            success = false,
        //            message = "Oops! Something went wrong. Please try later."
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new
        //        {
        //            success = false,
        //            message = "Oops! Something went wrong. Please try later."
        //        });
        //    }
        //}



        //[HttpPost]
        //[Route("property/addFloorPlan/{id}")]
        //public async Task<HttpResponseMessage> CreateFloorImage(long id)
        //{
        //    try
        //    {
        //        PropertyFloorPlan propFloor = new PropertyFloorPlan();
        //        string message = string.Empty;
        //        string filePath = string.Format("/Assets/AppFiles/Images/Property/{0}/FloorPlan/", id);
        //        string root = HttpContext.Current.Server.MapPath(filePath);
        //        var provider = new CustomMultipartFormDataStreamProvider(root);

        //        if (!Directory.Exists(HttpContext.Current.Server.MapPath(string.Format("/Assets/AppFiles/Images/Property/{0}/FloorPlan", id))))
        //        {
        //            Directory.CreateDirectory(HttpContext.Current.Server.MapPath(string.Format("/Assets/AppFiles/Images/Property/{0}/FloorPlan", id)));
        //        }

        //        CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);

        //        string path = string.Empty;
        //        if (file.Contents.Count() > 0)
        //        {
        //            path = filePath + "/" + file.filePath;
        //            propFloor.PropertyId = (int)id;
        //            propFloor.Path = path.Replace("///", "/");

        //            if (_floorPlanService.Add(ref propFloor, ref message))
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, new
        //                {
        //                    success = true,
        //                    message = message,
        //                    data = path
        //                });
        //            }
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new
        //        {
        //            success = false,
        //            message = "Oops! Something went wrong. Please try later."
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new
        //        {
        //            success = false,
        //            message = "Oops! Something went wrong. Please try later."
        //        });
        //    }
        //}

        [AllowAnonymous]
        [HttpGet]
        [Route("property/masterdata")]

        public HttpResponseMessage MaxPrice()
        {
            double MaxPrice = _propService.GetMaxPrice();
            int MaxNoOfBeds = _propService.GetMaxBeds();
            int MaxNoOfBaths = _propService.GetMaxBaths();
            double MaxSize = _propService.GetMaxSize();

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                success = true,
                Price = MaxPrice,
                Beds = MaxNoOfBeds,
                Baths = MaxNoOfBaths,
                Size = MaxSize
            });
        }

        //[HttpPost]
        //[Route("properties/Request")]
        //public HttpResponseMessage CreatePropertyRequest(PropertyRequestViewModel propertyRequestViewModel)
        //{
        //    try
        //    {
        //        string message = string.Empty;
        //        var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
        //        var claims = identity.Claims;
        //        long customerId;
        //        if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
        //        {
        //            if (ModelState.IsValid)
        //            {
        //                PropertyRequest PropertyRequest = new PropertyRequest()
        //                {
        //                    Title = propertyRequestViewModel.Title,
        //                    Description = propertyRequestViewModel.Description,
        //                    Category = propertyRequestViewModel.CategoryID,
        //                    ForSale = propertyRequestViewModel.PropertyType == "Sale" ? true : false,
        //                    Price = propertyRequestViewModel.Price,
        //                    Size = propertyRequestViewModel.Size,
        //                    NoOfBedRooms = propertyRequestViewModel.NoOfBedRooms,
        //                    //BuildYear = propertyRequestViewModel.BuildYear,
        //                    City = propertyRequestViewModel.CityID,
        //                    Country = propertyRequestViewModel.CountryID,
        //                    State = propertyRequestViewModel.State,
        //                    Area = propertyRequestViewModel.State,
        //                    CustomerID = customerId
        //                };

        //                if (_propRequestService.AddPropertyRequests(PropertyRequest, ref message))
        //                {
        //                    IEnumerable<vw_PropertyRequests> VendorList = _propRequestService.GetVendorListFromVMByRequestID(PropertyRequest.ID);

        //                    Notification not = new Notification();
        //                    not.Title = "Property Request";
        //                    not.Description = "New property added for request ";
        //                    not.OriginatorID = customerId;
        //                    not.OriginatorName = "";
        //                    not.Url = "/Vendor/PropertyRequests/Index";
        //                    not.Module = "PropertyRequest";
        //                    not.OriginatorType = "Customer";
        //                    not.RecordID = PropertyRequest.ID;
        //                    if (_notificationService.CreateNotification(not, ref message))
        //                    {
        //                        foreach (var id in VendorList)
        //                        {
        //                            NotificationReceiver notRec = new NotificationReceiver();
        //                            notRec.ReceiverID = id.VendorId;
        //                            notRec.ReceiverType = "Vendor";
        //                            notRec.NotificationID = not.ID;
        //                            _notificationReceiverService.CreateNotificationReceiver(notRec, ref message);
        //                        }
        //                    }

        //                    return Request.CreateResponse(HttpStatusCode.OK, new
        //                    {
        //                        status = "success",
        //                        message = "Request Sent successfully"
        //                    });
        //                }
        //                else
        //                {
        //                    return Request.CreateResponse(HttpStatusCode.OK, new
        //                    {
        //                        status = "success",
        //                        message = message
        //                    });
        //                }
        //            }
        //            else
        //            {
        //                return Request.CreateResponse(HttpStatusCode.BadRequest, new
        //                {
        //                    status = "error",
        //                    message = "Bad request !",
        //                    description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
        //                });
        //            }
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.Unauthorized, new
        //            {
        //                status = "error",
        //                message = "Session invalid or expired !"
        //            });

        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        log.Error("Error", ex);
        //        //Logs.Write(ex);
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
        //    }
        //}       

        private string GetDate(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "";

            string Month = dateTime.Value.ToString("MMM");

            return dateTime.Value.Day + " " + Month + " " + dateTime.Value.Year;
        }

        [AllowAnonymous]
        [HttpPut]
        [Route("properties/{propertyId}/callcount")]
        public HttpResponseMessage UpdateCallCount(long propertyId, string lang = "en")
        {
            try
            {

                string message = string.Empty;
                var property = _propService.GetProperty(propertyId);
                if (property != null)
                {
                    if (property.CallCount.HasValue)
                    {
                        property.CallCount += 1;
                    }
                    else
                    {
                        property.CallCount = 1;
                    }

                    if (_propService.UpdateProperty(ref property, ref message))
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
                        message = lang == "en" ? "Property doesn't exist!" : ArabicDictionary.Translate("Property doesn't exist!", false)
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
