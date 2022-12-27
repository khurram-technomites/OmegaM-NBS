using Newtonsoft.Json;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.Vendor;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{

    [RoutePrefix("api/v1/vendor")]
    public class VendorProfileController : ApiController
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IVendorService _vendorService;
        private readonly IVendorWalletShareService _vendorWalletShareService;
        private readonly ICountryService _countryService;
        private readonly ICityService _cityService;

        public VendorProfileController(IVendorService vendorService, IVendorWalletShareService vendorWalletShareService, ICountryService countryService, ICityService cityService)
        {
            this._vendorService = vendorService;
            this._vendorWalletShareService = vendorWalletShareService;
            this._countryService = countryService;
            this._cityService = cityService;
        }

        [HttpGet]
        [Route("profile")]
        public HttpResponseMessage VendorProfile()
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.FirstOrDefault().Value, out vendorId))
                {
                    var vendorWalletBalance = _vendorWalletShareService.GetDetails(vendorId);
                    Vendor vendor = _vendorService.GetVendor((long)vendorId);
                    var vendorCountry = vendor.CountryID != null ? _countryService.GetCountry((long)vendor.CountryID).Name : null;
                    var vendorCity = vendor.CityID != null ? _cityService.GetCity((long)vendor.CityID).Name : null;

                    string ImageServer = CustomURL.GetImageServer();
                    if (vendor != null)
                    {
                        var vendorModel = new
                        {
                            id = vendor.ID,
                            logo = ImageServer + vendor.Logo,
                            name = vendor.Name,
                            nameAr = vendor.NameAr,
                            vendorCode = vendor.VendorCode,
                            slug = vendor.Slug,
                            email = vendor.Email,
                            contact = vendor.Contact,
                            mobile = vendor.Mobile,
                            faxNo = vendor.FAX,
                            website = vendor.Website,
                            çountry = vendorCountry,
                            city = vendorCity,
                            address = vendor.Address,
                            servingKilometer = vendor.ServingKilometer,
                            commission = vendor.Commission,
                            idNo = vendor.IDNo,
                            trn = vendor.TRN,
                            licenseNo = vendor.License,
                            openingTiming = vendor.OpeningTime,
                            closingTiming = vendor.ClosingTime,
                            latitude = vendor.Latitude,
                            longitude = vendor.Longitude,
                            termsAndCondition = vendor.TermAndConditionWebEn,
                            termsAndConditionAr = vendor.TermAndConditionWebAr,
                            vendorPackageID = vendor.VendorPackageID,
                            facebook = vendor.Facebook,
                            instagram = vendor.Instagram,
                            whatsapp = vendor.Whatsapp,
                            snapChat = vendor.Snapchat,
                            linkedIn = vendor.LinkedIn,
                            youtube = vendor.Youtube,
                            IsPackageExpired = vendor.PackageEndDate.HasValue && vendor.PackageEndDate.Value.Date < Helpers.TimeZone.GetLocalDateTime().Date ? true : false
                        };


                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            vendor = vendorModel,
                            walletBalance = vendorWalletBalance.PendingAmount
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
                //log.Error("Error", ex);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpPut]
        [Route("profile/info")]
        public async Task<HttpResponseMessage> ProfileInfo()
        {
            try
            {

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {
                    var vendor = _vendorService.GetVendor(vendorId);
                    string message = string.Empty;
                    string status = string.Empty;
                    string relativePath = string.Format("/Assets/AppFiles/Images/Vendors/{0}", vendorId.ToString().Replace(" ", "_"));

                    string root = HttpContext.Current.Server.MapPath(relativePath);
                    var provider = new CustomMultipartFormDataStreamProvider(root);

                    if (!Directory.Exists(root))
                    {
                        string dir = relativePath;

                        if (!Directory.Exists(HttpContext.Current.Server.MapPath(dir)))
                        {
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir));
                        }
                    }

                    CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);

                    VendorProfileViewModel viewModel = JsonConvert.DeserializeObject<VendorProfileViewModel>(provider.FormData.GetValues("profile").FirstOrDefault());


                    if (vendor != null)
                    {


                        vendor.Name = viewModel.Name;
                        vendor.VendorCode = viewModel.VendorCode;
                        vendor.Slug = Slugify.GenerateSlug(viewModel.Name);
                        if (file.filePath.Contains("."))
                        {
                            vendor.Logo = relativePath + file.filePath;
                        }

                        if (_vendorService.UpdateVendor(ref vendor, ref message, false))
                        {

                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = "Account info updated!",

                            });

                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = "error",
                                message = "Vendor is already exist with this name"
                            });
                        }

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Conflict, new
                        {
                            status = "error",
                            message = "Vendor not found !"
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
        [Route("profile/contact")]
        public HttpResponseMessage ProfileContact(VendorProfileEditViewModel vendor)
        {
            try
            {

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {

                    string message = string.Empty;
                    string status = string.Empty;
                    var currentVendor = _vendorService.GetVendor(vendorId);

                    if (currentVendor != null)
                    {

                        currentVendor.Email = vendor.Email;
                        currentVendor.Contact = vendor.Contact;
                        currentVendor.Mobile = vendor.Mobile;
                        currentVendor.FAX = vendor.FAX;
                        currentVendor.Website = vendor.Website;



                        if (_vendorService.UpdateVendor(ref currentVendor, ref message, false))
                        {

                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = "Account contact updated!",

                            });

                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = "error",
                                message = "Vendor already exist with this email or name!"
                            });
                        }

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Conflict, new
                        {
                            status = "error",
                            message = "Vendor not found !"
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
        [Route("profile/business")]
        public HttpResponseMessage ProfileBusiness(VendorProfileEditViewModel vendor)
        {
            try
            {

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {

                    string message = string.Empty;
                    string status = string.Empty;
                    var currentVendor = _vendorService.GetVendor(vendorId);

                    if (currentVendor != null)
                    {

                        currentVendor.CountryID = vendor.CountryID;
                        currentVendor.CityID = vendor.CityID;
                        currentVendor.Address = vendor.Address;
                        currentVendor.ServingKilometer = vendor.ServingKilometer;
                        currentVendor.Commission = vendor.Commission;
                        currentVendor.Latitude = vendor.Latitude;
                        currentVendor.Longitude = vendor.Longitude;
                        currentVendor.OpeningTime = DateTime.Parse(vendor.OpeningTime, new CultureInfo("en-US")).TimeOfDay;
                        currentVendor.ClosingTime = DateTime.Parse(vendor.ClosingTime, new CultureInfo("en-US")).TimeOfDay;



                        if (_vendorService.UpdateVendor(ref currentVendor, ref message, false))
                        {

                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = "Account business updated!",

                            });

                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = "error",
                                message = "Something went wrong!"
                            });
                        }

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Conflict, new
                        {
                            status = "error",
                            message = "Vendor not found !"
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
        [Route("profile/legal")]
        public HttpResponseMessage ProfileLegal(VendorProfileEditViewModel vendor)
        {
            try
            {

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {
                    var currentVendor = _vendorService.GetVendor(vendorId);
                    string message = string.Empty;
                    string status = string.Empty;

                    if (currentVendor != null)
                    {

                        currentVendor.IDNo = vendor.IDNo;
                        currentVendor.TRN = vendor.TRN;
                        currentVendor.License = vendor.License;
                        currentVendor.TermAndConditionWebEn = vendor.TermAndConditionWebEn;
                        currentVendor.TermAndConditionWebAr = vendor.TermAndConditionWebAr;



                        if (_vendorService.UpdateVendor(ref currentVendor, ref message, false))
                        {

                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = "Account legal updated!",

                            });

                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = status,
                                message = "Something went wrong!"
                            });
                        }

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Conflict, new
                        {
                            status = "error",
                            message = "Vendor not found !"
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

        //public HttpResponseMessage ProfileInfo(Vendor vendor)
        //{
        //	try
        //	{
        //		var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
        //		var claims = identity.Claims;
        //		long vendorId;
        //		if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
        //		{
        //			var currentVendor = _vendorService.GetVendor(vendorId);
        //			string message = string.Empty;
        //			string status = string.Empty;
        //			if (vendor != null)
        //			{
        //				currentVendor.Name = vendor.Name;
        //				currentVendor.VendorCode = vendor.VendorCode;
        //				currentVendor.Slug = Slugify.GenerateSlug(vendor.Name);
        //				if (_vendorService.UpdateVendor(ref currentVendor, ref message))
        //				{
        //						return Request.CreateResponse(HttpStatusCode.OK, new
        //						{
        //							status = "success",
        //							message = "Profile updated!",
        //						});
        //				}
        //				else
        //				{
        //					return Request.CreateResponse(HttpStatusCode.InternalServerError, new
        //					{
        //						status = status,
        //						message = "Something went wrong!"
        //					});
        //				}
        //			}
        //			else
        //			{
        //				return Request.CreateResponse(HttpStatusCode.Conflict, new
        //				{
        //					status = "error",
        //					message = "Vendor not found !"
        //				});
        //			}
        //		}
        //		else
        //		{
        //			return Request.CreateResponse(HttpStatusCode.Unauthorized, new
        //			{
        //				status = "error",
        //				message = "Session invalid or expired !"
        //			});
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		log.Error("Error", ex);
        //		return Request.CreateResponse(HttpStatusCode.InternalServerError, new
        //		{
        //			status = "failure",
        //			message = "Oops! Something went wrong. Please try later."
        //		});
        //	}
        //}

    }
}
