using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.WishList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [Authorize]
    [RoutePrefix("api/v1")]
    public class WishListController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICustomerWishlistService _customerWishlistService;

        public WishListController(ICustomerWishlistService customerWishlistService)
        {
            this._customerWishlistService = customerWishlistService;
        }

        [HttpGet]
        [Route("{lang}/wishlist")]
        public HttpResponseMessage GetWishlist(string lang = "en")
        {
            List<WishListReturnList> PropertyList = new List<WishListReturnList>();
            List<WishListReturnList> CarList = new List<WishListReturnList>();
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    string ImageServer = CustomURL.GetImageServer();
                    //var wishlist = _customerWishlistService.GetCustomerWishlist(customerId, lang, ImageServer);
                    var wishlistProperty = _customerWishlistService.GetPropertyRecordForWishlistByCustomer(customerId);
                    var wishlistCar = _customerWishlistService.GetCarRecordForWishlistByCustomer(customerId);

                    if (wishlistProperty.Count() > 0 || wishlistCar.Count() > 0)
                    {
                        foreach (var entry in wishlistProperty)
                        {
                            List<string> images = new List<string>();
                            if (entry.Property.PropertyImages.Any())
                            {
                                foreach (var item in entry.Property.PropertyImages)
                                {
                                    images.Add(ImageServer + "/" + item.Path);
                                }
                            }

                            PropertyList.Add(new WishListReturnList
                            {
                                wishlistId = entry.ID,
                                ProductID = (long)entry.PropertyID,
                                CountryID = entry.Property.CountryID,
                                CityID = entry.Property.CityID,
                                Thumbnail = ImageServer + "/" + entry.Property.Thumbnail,
                                Address = entry.Property.Address,
                                VendorThumbnail = ImageServer + "/" + entry.Property.Vendor.Logo,
                                oldPrice = entry.Property.OldPrice == null ? 0.00 : entry.Property.OldPrice.Value,
                                price = entry.Property.Price == null ? 0.00 : entry.Property.Price.Value,
                                Title = lang == "ar" ? entry.Property.TitleAr : entry.Property.Title,
                                Description = lang == "ar" ? entry.Property.DescriptionAr : entry.Property.Description,
                                Slug = entry.Property.Slug,
                                Status = entry.Property.ForSale ? lang == "ar" ? ArabicDictionary.Translate("Sale") : "Sale" : lang == "ar" ? ArabicDictionary.Translate("Rent") : "Rent",
                                rooms = lang == "ar" ? (entry.Property.NoOfRooms != null ? entry.Property.NoOfRooms.Value : 0) + " " + ArabicDictionary.Translate("Beds") : (entry.Property.NoOfRooms != null ? entry.Property.NoOfRooms.Value : 0) + " Beds",
                                baths = lang == "ar" ? (entry.Property.NoOfBaths != null ? entry.Property.NoOfBaths.Value : 0) + " " + ArabicDictionary.Translate("Baths") : (entry.Property.NoOfBaths != null ? entry.Property.NoOfBaths.Value : 0) + " Baths",
                                NoOfGarage = lang == "ar" ? (entry.Property.NoOfGarage != null ? entry.Property.NoOfGarage.Value : 0) + " " + ArabicDictionary.Translate("Garage") : (entry.Property.NoOfGarage != null ? entry.Property.NoOfGarage.Value : 0) + " Garage",
                                Size = lang == "ar" ? (entry.Property.Size != null ? entry.Property.Size.Value : 0) + " " + ArabicDictionary.Translate("Sqft") : (entry.Property.Size != null ? entry.Property.Size.Value : 0) + " sqft",
                                IsVerified = !entry.Property.IsVerified.HasValue ? false : entry.Property.IsVerified.Value,
                                IsPremium = !entry.Property.IsPremium.HasValue ? false : entry.Property.IsPremium.Value,
                                IsSold = !entry.Property.IsSold.HasValue ? false : entry.Property.IsSold.Value,
                                Images = images,
                                VendorID = entry.Property.VendorId ?? 0,
                                VendorName = entry.Property.Vendor.Name,


                            });

                        }

                        foreach (var entry in wishlistCar)
                        {
                            List<string> images = new List<string>();
                            if (entry.Car.CarImages.Any())
                            {
                                foreach (var item in entry.Car.CarImages)
                                {
                                    images.Add(ImageServer + "/" + item.Image);
                                }
                            }
                            CarList.Add(new WishListReturnList
                            {
                                wishlistId = entry.ID,
                                ProductID = (long)entry.CarID,
                                Thumbnail = ImageServer + "/" + entry.Car.Thumbnail,
                                VendorThumbnail = ImageServer + "/" + entry.Car.Vendor.Logo,
                                //Address = entry.Car.Address,
                                oldPrice = entry.Car.RegularPrice == null ? 0.00 : (double)entry.Car.RegularPrice.Value,
                                price = entry.Car.SalePrice == null ? 0.00 : (double)entry.Car.SalePrice.Value,
                                Title = entry.Car.Name,
                                NoOfDoors = lang == "ar" ? ArabicDictionary.Translate(entry.Car.Doors) : entry.Car.Doors,
                                Transmission = lang == "ar" ? ArabicDictionary.Translate(entry.Car.Transmission) : entry.Car.Transmission,
                                HorsePower = lang == "ar" ? ArabicDictionary.Translate(entry.Car.HorsePower) : entry.Car.HorsePower,
                                Cylinders = lang == "ar" ? ArabicDictionary.Translate(entry.Car.Cylinders) : entry.Car.Cylinders,
                                Wheels = lang == "ar" ? ArabicDictionary.Translate(entry.Car.Wheels) : entry.Car.Wheels,
                                Door = lang == "ar" ? ArabicDictionary.Translate(entry.Car.Doors) : entry.Car.Doors,
                                Garage = entry.Car.Garage,
                                Slug = entry.Car.Slug,
                                Address = entry.Car.Address,
                                EngineDisplacement = lang == "ar" ? ArabicDictionary.Translate(entry.Car.EngineDisplacement) : entry.Car.EngineDisplacement,
                                Description = lang == "ar" ? entry.Car.LongDescriptionAr : entry.Car.LongDescription,
                                IsVerified = !entry.Car.IsVerified.HasValue ? false : entry.Car.IsVerified.Value,
                                IsPremium = !entry.Car.IsPremium.HasValue ? false : entry.Car.IsPremium.Value,
                                IsSold = !entry.Car.IsSold.HasValue ? false : entry.Car.IsSold.Value,
                                Images = images,
                                VendorID = entry.Car.VendorID ?? 0,
                                VendorName = entry.Car.Vendor.Name,
                                Mileage = entry.Car.FuelEconomy,
                            });
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            Properties = PropertyList,
                            Cars = CarList
                        });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        wishlist = "No item added in Wishlist"
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

        [HttpGet]
        [Route("{Type}/{lang}/wishlist")]
        public HttpResponseMessage GetWishlistByType(string Type, string lang = "en")
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    string ImageServer = CustomURL.GetImageServer();
                    var wishlist = _customerWishlistService.GetWishListByTypeandCustomer(customerId, Type);

                    if (Type.ToLower() == "property")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            wishlist = wishlist.Select(x => x.PropertyID)
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            wishlist = wishlist.Select(x => x.CarID)
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }


        [HttpPost]
        [Route("wishlist")]
        public HttpResponseMessage Create(WishListViewModel wishListViewModel, string lang = "en")
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                long? carID, propertyID;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    string message = string.Empty;
                    if (ModelState.IsValid)
                    {
                        if (wishListViewModel.CarID == 0)
                            carID = null;
                        else
                            carID = wishListViewModel.CarID;

                        if (wishListViewModel.PropertyID == 0)
                            propertyID = null;
                        else
                            propertyID = wishListViewModel.PropertyID;

                        CustomerWishlist customerWishlist = new CustomerWishlist()
                        {
                            CarID = carID,
                            PropertyID = propertyID,
                            CustomerID = customerId,
                        };

                        if (_customerWishlistService.CreatecustomerWishlist(ref customerWishlist, ref message))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = lang == "en" ? message : ArabicDictionary.Translate(message),
                                data = customerWishlist
                            });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = lang == "en" ? message : ArabicDictionary.Translate(message)
                            });
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new
                        {
                            status = "error",
                            message = lang == "en" ? "Bad request!" : ArabicDictionary.Translate("Bad request!", false),
                            description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = lang == "en" ? "Session invalid or expired" : ArabicDictionary.Translate("Session invalid or expired", false) });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = lang == "en" ? "Authorization failed for current request" : ArabicDictionary.Translate("Authorization failed for current request", false) });
            }
        }

        [HttpDelete]
        [Route("wishlist/{wishId}")]
        public HttpResponseMessage Delete(long wishId, string lang = "en")
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    string message = string.Empty;
                    string status = string.Empty;
                    if (_customerWishlistService.DeleteCustomerWishlist(wishId, ref message))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = lang == "en" ? message : ArabicDictionary.Translate(message)
                        });
                    }
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = lang == "en" ? "Authorization failed for current request" : ArabicDictionary.Translate("Authorization failed for current request", false) });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = lang == "en" ? "Session invalid or expired" : ArabicDictionary.Translate("Session invalid or expired", false) });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false) });
            }
        }

        [HttpDelete]
        [Route("Deletewishlist")]
        public HttpResponseMessage DeleteWishlist(WishListViewModel wishListViewModel, string lang = "en")
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    string message = string.Empty;
                    string status = string.Empty;
                    if (_customerWishlistService.DeleteWishlistByCustomerAndProduct(customerId, wishListViewModel.PropertyID, wishListViewModel.CarID, ref message))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = message
                        });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", message = lang == "en" ? message : ArabicDictionary.Translate(message, false) });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = lang == "en" ? "Session invalid or expired" : ArabicDictionary.Translate("Session invalid or expired", false) });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false) });
            }
        }

    }
}