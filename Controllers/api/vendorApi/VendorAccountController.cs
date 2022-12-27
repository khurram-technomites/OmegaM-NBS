using Newtonsoft.Json;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Service.Helpers;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Encryption;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.Account;
using NowBuySell.Web.ViewModels.Api.Customer;
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

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1/vendor")]
    public class VendorAccountController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private readonly IVendorUserService _vendorUserService;
        private readonly IBusinessSettingService _businessSettingService;
        private readonly INotificationService _notificationService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly IVendorSessionService _vendorSessionService;
        private readonly IVendorService _vendorService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerDocumentService _customerDocumentService;
        private readonly INumberRangeService _numberRangeService;
        private readonly IVendorUserRoleService _vendorUserRoleService;
        private readonly IVendorSignupService _vendorSignUpService;
        private readonly IMail _email;

        public VendorAccountController(IVendorUserService vendorUserService
            , IBusinessSettingService businessSettingService
            , INotificationService notificationService
            , INotificationReceiverService notificationReceiverService
            , IVendorSessionService vendorSessionService
            , IVendorService vendorService
            , ICustomerService customerService
            , ICustomerDocumentService customerDocumentService
            , INumberRangeService numberRangeService
            , IVendorUserRoleService vendorUserRoleService,
            IVendorSignupService vendorSignUpService
            , IMail email)
        {
            //this._vendorService = vendorService;
            this._vendorUserService = vendorUserService;
            this._businessSettingService = businessSettingService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
            //this._couponService = couponService;
            //this._customerCouponsService = customerCouponsService;
            this._vendorSessionService = vendorSessionService;
            this._vendorService = vendorService;
            this._customerService = customerService;
            this._customerDocumentService = customerDocumentService;
            this._numberRangeService = numberRangeService;
            this._vendorUserRoleService = vendorUserRoleService;
            _vendorSignUpService = vendorSignUpService;
            this._email = email;
        }

        [HttpPost]
        [Route("Register")]
        public HttpResponseMessage Register(SignupViewModel signupViewModel, string lang = "en")
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    if (_vendorUserService.GetUserByEmail(signupViewModel.Email) == null)
                    {
                        //if (_vendorSignUpService.IsContactInUse(signupViewModel.Contact))
                        //    return Request.CreateResponse(HttpStatusCode.OK, new
                        //    {
                        //        status = "success",
                        //        message = lang == "en" ? "Contact already in use" : ArabicDictionary.Translate("Contact already in use", false),

                        //    });

                        //if (_vendorSignUpService.IsContactVerified(signupViewModel.Contact))
                        //{
                            Vendor vendor = new Vendor();

                            vendor.VendorCode = _numberRangeService.GetNextValueFromNumberRangeByName("VENDOR");
                            vendor.Logo = "/assets/images/vendor/Dafault-vendor-logo.png";
                            vendor.Name = signupViewModel.Name;
                            vendor.Contact = signupViewModel.Contact;
                            vendor.Mobile = signupViewModel.Contact;
                            vendor.Email = signupViewModel.Email;
                            vendor.IsApproved = false;
                            vendor.ApprovalStatusID = 1;
                            vendor.Slug = Slugify.GenerateSlug(vendor.Name);

                            Crypto objCrypto = new NowBuySell.Web.Helpers.Encryption.Crypto();

                            vendor.AuthorizationCode = objCrypto.Random(225);

                            if (_vendorService.CreateVendor(ref vendor, ref message))
                            {

                                //VendorSignup vendorSignup = _vendorSignUpService.GetByContact(vendor.Contact).FirstOrDefault();

                                //vendorSignup.IsContactInUse = true;
                                //_vendorSignUpService.UpdateVendorContact(ref vendorSignup);

                                /*Vendor Administrator User Creation*/
                                long VendorID = vendor.ID;
                                string UserEmail = signupViewModel.Email;
                                string UserPassword = signupViewModel.Password;
                                string Role = "Administrator";
                                var UserRoleID = _vendorUserRoleService.GetVendorUserRoleByName(Role);
                                var vendoruser = new VendorUser();

                                vendoruser.VendorID = VendorID;
                                vendoruser.Name = vendor.Name;
                                vendoruser.MobileNo = vendor.Contact;
                                vendoruser.EmailAddress = UserEmail;
                                vendoruser.Password = UserPassword;
                                vendoruser.UserRoleID = UserRoleID.ID;

                                if (_vendorUserService.CreateVendorUser(vendoruser, ref message, true))
                                {
                                    var path = System.Web.Hosting.HostingEnvironment.MapPath("~/");

                                    if (_email.SendVendorCreationMail(signupViewModel.Name, signupViewModel.Email, vendoruser.EmailAddress, UserPassword, CustomURL.GetFormatedURL("/Vendor/Account/Login"), path) &&
                                        _email.SendVerificationMail(vendor.Name, vendor.Email, CustomURL.GetFormatedURL("/Vendors/Verify?auth=" + vendor.AuthorizationCode), path))
                                    {
                                        vendor.IsEmailSent = true;
                                        _vendorService.UpdateVendor(ref vendor, ref message);
                                    }
                                VendorUserRole vendorUserRole = new VendorUserRole();
                                vendorUserRole.VendorID = VendorID;
                                vendorUserRole.Name = "Lead Manager";

                                if (_vendorUserRoleService.CreateVendorUserRole(vendorUserRole, ref message))
                                {
                                    string directory = HttpContext.Current.Server.MapPath(string.Format("/AuthorizationProvider/Privileges/Vendor/{0}/", VendorID));
                                    Directory.CreateDirectory(directory);
                                    TextWriter textWriter = new StreamWriter(directory + vendorUserRole.Name + ".txt");
                                    textWriter.Close();
                                }
                            }

                                return Request.CreateResponse(HttpStatusCode.OK, new
                                {
                                    status = "success",
                                    message = lang == "en" ? "Account created successfully" : ArabicDictionary.Translate("Account created successfully", false),

                                });
                            }
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = "error",
                                message = lang == "en" ? message : ArabicDictionary.Translate(message)
                            });
                        //}
                        //else
                        //{
                        //    return Request.CreateResponse(HttpStatusCode.OK, new
                        //    {
                        //        status = "error",
                        //        message = lang == "en" ? "Verify your contact first" : ArabicDictionary.Translate("Verify your contact first"),

                        //    });
                        //}
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "error",
                            message = lang == "en" ? "Vendor already exist" : ArabicDictionary.Translate("Vendor already exist"),

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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                });
            }
        }

        [HttpPost]
        [Route("logout")]
        public HttpResponseMessage logout(LogoutViewModel logoutViewModel)
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {
                    _vendorSessionService.ExpireSession(vendorId, logoutViewModel.DeviceID);

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = "Logged out successfully"
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

        [HttpPost]
        [Route("changepassword")]
        public HttpResponseMessage ChangePassword(ViewModels.Api.Account.ChangePasswordViewModel changePasswordViewModel)
        {
            try
            {
                string message = string.Empty;
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {
                    var vendorUser = _vendorUserService.GetVendorUserByVendorID(vendorId);

                    if (_vendorUserService.ChangePassword(changePasswordViewModel.CurrentPassword, changePasswordViewModel.NewPassword, vendorUser.ID, ref message))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = "Password saved successfully"
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new
                        {
                            status = "error",
                            message = "Incorrect old password"
                        });
                    }
                }

                return Request.CreateResponse(HttpStatusCode.BadRequest, new
                {
                    status = "error",
                    message = "Session expired"
                });

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

        [HttpPost]
        [Route("forgotpassword")]
        public HttpResponseMessage ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            try
            {
                string message = string.Empty;
                var path = System.Web.Hosting.HostingEnvironment.MapPath("~/");
                if (_vendorUserService.ForgotPassword(forgotPasswordViewModel.EmailAddress, ref message, path))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = message
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "error",
                        message = message
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

        [HttpPost]
        [Route("profile/update")]
        public async Task<HttpResponseMessage> VendorProfile()
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
                    string relativePath = string.Format("/Assets/AppFiles/Images/Vendors/{0}/", vendor.ID);
                    string root = HttpContext.Current.Server.MapPath(relativePath);
                    var provider = new CustomMultipartFormDataStreamProvider(root);

                    CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);
                    ProfileViewModel viewModel = JsonConvert.DeserializeObject<ProfileViewModel>(provider.FormData.GetValues("profile").FirstOrDefault());


                    VendorUser vendorUser = _vendorUserService.GetVendorUserByContact(viewModel.Contact);

                    if (vendorUser != null)
                    {
                        vendorUser.Name = viewModel.Name;
                        vendorUser.MobileNo = viewModel.Contact;

                        if (_vendorUserService.UpdateVendorUser(ref vendorUser, ref message))
                        {

                            var currentVendor = _vendorService.GetVendor(vendorId);
                            currentVendor.Logo = relativePath;
                            if (_vendorService.UpdateVendor(ref currentVendor, ref message, false))
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new
                                {
                                    status = "success",
                                    message = "Profile updated!",

                                });

                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new
                                {
                                    status = "error",
                                    message = "Profile not updated!",

                                });
                            }


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

        [HttpPost]
        [Route("SendOTP")]
        public async Task<HttpResponseMessage> Contact(string contact, string lang = "en")
        {
            try
            {
                if (_vendorSignUpService.IsContactInUse(contact))
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = lang == "en" ? "Contact already in use" : ArabicDictionary.Translate("Contact already in use", false)
                    });

                VendorSignup signupModel = new VendorSignup();
                signupModel.ContactNo = contact;

                if (await _vendorSignUpService.AddVendorContact(signupModel)) //also sending otp in this service method
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = lang == "en" ? "OTP sent successfully" : ArabicDictionary.Translate("OTP sent successfully", false) 
                    });
                }

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                });
            }
        }

        [HttpPut]
        [Route("verifyContact")]
        public HttpResponseMessage VerifyContact(string contact, string OTP, string lang = "en")
        {
            bool ResendOTP = false;
            string message = string.Empty;
            try
            {
                if (_vendorSignUpService.VerifyVendorContact(contact, OTP, ref ResendOTP, ref message))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = lang == "en" ? message : ArabicDictionary.Translate(message, false)
                    });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "failure",
                    message = lang == "en" ? message : ArabicDictionary.Translate(message, false)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                });
            }
        }

        [HttpPut]
        [Route("resendOTP")]
        public async Task<HttpResponseMessage> ResendOTP(string contact, string lang = "en")
        {
            try
            {
                if (await _vendorSignUpService.ResendOTP(contact))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = lang == "en" ? "OTP resend successfully" : ArabicDictionary.Translate("OTP resend successfully", false)
                    });
                }

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                });
            }
        }
    }
}
