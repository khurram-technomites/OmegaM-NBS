using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.VendorUser;
using System;
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

    [Authorize]
    [RoutePrefix("api/v1/vendor")]
    public class VendorUserController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IVendorUserService _vendorUserService;
        private readonly IVendorService _vendorService;

        public VendorUserController(IVendorUserService vendorUserService, IVendorService vendorService)
        {
            this._vendorUserService = vendorUserService;
            _vendorService = vendorService;
        }

        [HttpGet]
        [Route("account/profile")]
        public HttpResponseMessage VendorUserProfile()
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {
                    VendorUser vendorUser = _vendorUserService.GetVendorUser((long)vendorUserId);
                    var Vendor = vendorUser.Vendor;
                    string ImageServer = CustomURL.GetImageServer();
                    if (vendorUser != null && Vendor != null)
                    {
                        var vendorUserModel = new
                        {
                            vendorUser.ID,
                            vendorUser.Name,
                            image = ImageServer + vendorUser.Image,
                            vendorUser.EmailAddress,
                            vendorUser.MobileNo,
                            vendorPackageID = Vendor.VendorPackageID,
                            IsPackageExpired = Vendor.PackageEndDate.HasValue && Vendor.PackageEndDate.Value.Date < Helpers.TimeZone.GetLocalDateTime().Date ? true : false
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            profile = vendorUserModel,
                            vendor = new
                            {
                                Vendor.ID,
                                Vendor.Name,
                                AccountStatus = "Active",
                                Vendor.Latitude,
                                Vendor.Longitude,
                                Vendor.Whatsapp,
                                Vendor.Facebook,
                                Vendor.Instagram,
                                Vendor.Snapchat,
                                Vendor.LinkedIn,
                                Vendor.Twitter,
                                Vendor.Youtube,
                                Package = Vendor.VendorPackage != null ? new
                                {
                                    Vendor.VendorPackageID,
                                    StartDate = Vendor.PackageStartDate,
                                    EndDate = Vendor.PackageEndDate,
                                    Vendor.VendorPackage.IsFree,
                                    Vendor.VendorPackage.hasMotorModule,
                                    Vendor.VendorPackage.MotorLimit,
                                    Vendor.VendorPackage.hasPropertyModule,
                                    Vendor.VendorPackage.PropertyLimit,
                                } : null
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
        [Route("account/profile")]
        public HttpResponseMessage VendorUserProfile(VendorUserProfileViewModel vendorUserProfile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                    var claims = identity.Claims;
                    long vendorUserId;
                    if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                    {
                        string message = string.Empty;
                        string status = string.Empty;
                        VendorUser vendorUser = _vendorUserService.GetVendorUser(vendorUserId);
                        if (vendorUser != null)
                        {
                            if (vendorUser.VendorUserRole.Name == "Administrator")
                            {
                                Vendor vendor = _vendorService.GetVendor(vendorUser.VendorID.Value);

                                vendor.Name = vendorUserProfile.Name;
                                vendor.Mobile = vendorUserProfile.MobileNo;

                                if(_vendorService.UpdateVendor(ref vendor, ref message)) { }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                                    {
                                        status = "error",
                                        message = message
                                    });
                                }
                            }

                            vendorUser.Name = vendorUserProfile.Name;
                            vendorUser.EmailAddress = vendorUserProfile.EmailAddress;
                            vendorUser.MobileNo = vendorUserProfile.MobileNo;

                            if (_vendorUserService.UpdateVendorUser(ref vendorUser, ref message))
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new
                                {
                                    status = "success",
                                    message = "Profile updated!",
                                    profile = new
                                    {
                                        vendorUser.ID,
                                        vendorUser.Name,
                                        vendorUser.EmailAddress,
                                        vendorUser.MobileNo
                                    }
                                });
                            }
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = status,
                                message = message
                            });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.Conflict, new
                            {
                                status = "error",
                                message = "Account already exist !"
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
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpPut]
        [Route("account/profile/photo")]
        public async Task<HttpResponseMessage> UpdateProfilePhoto()
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorUserId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorUserId))
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string message = string.Empty;
                    string status = string.Empty;
                    string filePath = "/Assets/AppFiles/VendorUser";
                    string root = HttpContext.Current.Server.MapPath(filePath);

                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("/Assets/AppFiles/VendorUser")))
                    {
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("/Assets/AppFiles/VendorUser"));
                    }

                    var provider = new CustomMultipartFormDataStreamProvider(root);
                    CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);
                    string path = string.Empty;
                    if (file.Contents.Count() > 0)
                    {
                        path = filePath + "/" + file.filePath;
                        var vendorUser = _vendorUserService.GetVendorUser(vendorUserId);

                        if (vendorUser.VendorUserRole.Name == "Administrator")
                        {
                            Vendor vendor = _vendorService.GetVendor(vendorUser.VendorID.Value);

                            vendor.Logo = path;

                            if (_vendorService.UpdateVendor(ref vendor, ref message)) { }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                                {
                                    status = "error",
                                    message = message
                                });
                            }
                        }

                        vendorUser.Image = path;
                        if (_vendorUserService.UpdateVendorUser(ref vendorUser, ref message))
                        {
                            string ImageServer = CustomURL.GetImageServer();
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = "Profile image updated!",
                                image = ImageServer + vendorUser.Image
                            });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = status,
                                message = message
                            });
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new
                        {
                            status = "error",
                            message = "Bad request !",
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
