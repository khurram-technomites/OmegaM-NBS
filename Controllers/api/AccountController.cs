using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.ViewModels.Api.Account;
using System;
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
    [RoutePrefix("api/v1")]
    public class AccountController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICustomerService _customerService;
        private readonly ICustomerSessionService _customerSessionService;

        private readonly IBusinessSettingService _businessSettingService;
        private readonly ICouponService _couponService;
        private readonly ICustomerCouponsService _customerCouponsService;
        private readonly INotificationService _notificationService;
        private readonly INotificationReceiverService _notificationReceiverService;

        public AccountController(ICustomerService customerService, ICustomerSessionService customerSessionService, IBusinessSettingService businessSettingService, ICouponService couponService, ICustomerCouponsService customerCouponsService, INotificationService notificationService, INotificationReceiverService notificationReceiverService)
        {
            this._customerService = customerService;
            this._customerSessionService = customerSessionService;
            this._businessSettingService = businessSettingService;
            this._couponService = couponService;
            this._customerCouponsService = customerCouponsService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("signup")]
        public async Task<HttpResponseMessage> OtpSignUp(SignupViewModel signupViewModel, string lang = "en")
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    Customer customer = new Customer();
                    if (!_customerService.IsCustomerByContact(signupViewModel.Contact) && _customerService.GetCustomerByEmail(signupViewModel.Email) == null)
                    {

                        customer.Contact = signupViewModel.Contact;
                        customer.Password = signupViewModel.Password;
                        customer.Email = signupViewModel.Email;
                        customer.Name = signupViewModel.UserName;

                        customer.AccountType = "Basic";
                        customer.Logo = "/Assets/AppFiles/Customer/default.jpg";
                        customer.IsEmailVerified = true;

                        var path = HttpContext.Current.Server.MapPath("~/");
                        if (_customerService.CreateCustomer(ref customer, path, ref message, false, true))
                        {

                            bool isSent = await _customerService.SendOTP(customer.Contact);

                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = lang == "en" ? "Account created successfully" : ArabicDictionary.Translate("Account created successfully", false),

                            });
                        }
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                        {
                            status = "error",
                            message = lang == "en" ? message : ArabicDictionary.Translate(message, false)
                        });
                    }
                    else
                    {
                        //bool isSent = await _customerService.SendOTP(signupViewModel.Contact);
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "Failed",
                            message = lang == "en" ? "User already exists" : ArabicDictionary.Translate("User already exists"),

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
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                //log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                });
            }
        }

        //		[HttpPost]
        //		[AllowAnonymous]
        //		[Route("signup")]
        //		public async Task<HttpResponseMessage> SignUp(SignupViewModel signupViewModel)
        //		{
        //			try
        //			{
        //				string message = string.Empty;
        //				if (ModelState.IsValid)
        //				{
        //					Customer customer = new Customer();
        //					if (!_customerService.IsCustomer(signupViewModel.Email, signupViewModel.Contact))
        //					{
        //						bool isSent = false;
        //						customer.Name = signupViewModel.UserName;
        //						customer.Contact = signupViewModel.Contact;
        //						customer.Email = signupViewModel.Email;
        //						customer.Password = signupViewModel.Password;
        //						customer.AccountType = "Basic";
        //						customer.Logo = "/Assets/AppFiles/Customer/default.png";
        //						customer.ReferredBy = signupViewModel.ReferralID;
        //						customer.IsSrilankan = signupViewModel.IsSrilankan;

        //						var path = HttpContext.Current.Server.MapPath("~/");
        //						if (_customerService.CreateCustomer(ref customer, path, ref message, false, true))
        //						{
        //							if (signupViewModel.ReferralID.HasValue)
        //							{
        //								_customerService.UpdateReferralPoints(signupViewModel.ReferralID.Value);
        //							}
        //							if (customer.IsSrilankan.HasValue && customer.IsSrilankan.Value)
        //							{
        //								isSent = await _customerService.SendOTP(customer.Contact);
        //							}

        //							var CouponID = _businessSettingService.GetDefaultBusinessSetting().CouponID;
        //							if (CouponID.HasValue)
        //							{
        //								CustomerCoupon customerCoupon = new CustomerCoupon();
        //								customerCoupon.CustomerID = customer.ID;
        //								customerCoupon.CouponsID = CouponID;

        //								if (_customerCouponsService.CreateCustomerCoupon(customerCoupon, ref message))
        //								{
        //									var coupon = _couponService.GetCoupon((long)customerCoupon.CouponsID);
        //									Notification not = new Notification();
        //									if (!coupon.DicountAmount.HasValue)
        //									{
        //										not.Title = coupon.DicountPercentage + "% Discount";
        //										not.TitleAr = coupon.DicountPercentage + "% Discount";
        //										not.Description = string.Format("{0}% off your very first purchase, use promo code: {1}", coupon.DicountPercentage, coupon.CouponCode);
        //										not.DescriptionAr = string.Format("{0}% off your very first purchase, use promo code: {1}", coupon.DicountPercentage, coupon.CouponCode);
        //										not.OriginatorID = 0;
        //										not.OriginatorName = "System";
        //										not.Module = "Coupon";
        //										not.OriginatorType = "Admin";
        //									}
        //									else
        //									{
        //										not.Title = coupon.DicountAmount + " AED Discount";
        //										not.TitleAr = coupon.DicountAmount + " AED Discount";
        //										not.Description = string.Format("{0} AED off your very first purchase, use promo code: {1}", coupon.DicountAmount, coupon.CouponCode);
        //										not.DescriptionAr = string.Format("{0} AED off your very first purchase, use promo code: {1}", coupon.DicountAmount, coupon.CouponCode);
        //										not.OriginatorID = 0;
        //										not.OriginatorName = "System";
        //										not.Module = "Coupon";
        //										not.OriginatorType = "Admin";

        //									}
        //									not.RecordID = coupon.ID;

        //									if (_notificationService.CreateNotification(not, ref message))
        //									{
        //										NotificationReceiver notRec = new NotificationReceiver();
        //										notRec.ReceiverID = customerCoupon.CustomerID;
        //										notRec.ReceiverType = "Customer";
        //										notRec.NotificationID = not.ID;
        //										if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
        //										{
        //										}
        //									}
        //								}
        //							}

        //							return Request.CreateResponse(HttpStatusCode.OK, new
        //							{
        //								status = "success",
        //								message = "Account created!",
        //								OTPSend = isSent,
        //							});
        //						}
        //						return Request.CreateResponse(HttpStatusCode.InternalServerError, new
        //						{
        //							status = "error",
        //							message = message
        //						});
        //					}
        //					else
        //					{
        //						return Request.CreateResponse(HttpStatusCode.Conflict, new
        //						{
        //							status = "error",
        //							message = "Account already exist !"
        //						});
        //					}
        //				}
        //				else
        //				{
        //					return Request.CreateResponse(HttpStatusCode.BadRequest, new
        //					{
        //						status = "error",
        //						message = "Bad request !",
        //						description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
        //					});
        //				}
        //			}
        //#pragma warning disable CS0168 // The variable 'ex' is declared but never used
        //			catch (Exception ex)
        //#pragma warning restore CS0168 // The variable 'ex' is declared but never used
        //			{
        //				//log.Error("Error", ex);
        //				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
        //				{
        //					status = "failure",
        //					message = "Oops! Something went wrong. Please try later."
        //				});
        //			}
        //		}

        [HttpPost]
        [AllowAnonymous]
        [Route("otpverification")]
        public HttpResponseMessage verifyOTP(VerifyOTPViewModel verifyOTPViewModel, string lang = "en")
        {
            try
            {
                string Message = string.Empty;
                string Status = string.Empty;
                if (ModelState.IsValid)
                {
                    if (_customerService.verifyOTP(verifyOTPViewModel.Contact, verifyOTPViewModel.otp, ref Status, ref Message))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = lang == "en" ? Message : ArabicDictionary.Translate(Message, false)
                        });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = Status,
                        message = lang == "en" ? Message : ArabicDictionary.Translate(Message, false)
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "error",
                        message = lang == "en" ? "Phone Number required" : ArabicDictionary.Translate("Phone Number required"),
                        description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
                    });
                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                //log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false) });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("resendotp/{contact}")]
        public async Task<HttpResponseMessage> resendOTP(string contact, string lang = "en")
        {
            try
            {
                string Message = string.Empty;
                string Status = string.Empty;
                if (contact != null)
                {

                    bool isSent = await _customerService.SendOTP(contact);

                    if (isSent)
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = lang == "en" ? "OTP Sent" : ArabicDictionary.Translate("OTP Sent", false)
                        });
                    else
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "error",
                            message = lang == "en" ? "Failed to sent OTP. Something went wrong!" : ArabicDictionary.Translate("Failed to sent OTP. Something went wrong!", false)
                        });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "error",
                        message = lang == "en" ? "Contact Number required" : ArabicDictionary.Translate("Contact Number required", false),
                        //description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
                    });
                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                //log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false) });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("referral")]
        public HttpResponseMessage Referral(NowBuySell.Web.ViewModels.Account.ReferralViewModel referral)
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    var customer = _customerService.GetCustomerByEmail(referral.Email);
                    if (customer != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            success = true,
                            message = "Data recieved successfully!",
                            ReferralID = customer.ID,
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            success = false,
                            message = "Invalid Referral!"
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "error",
                        message = "Email required !",
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

        [HttpPost]
        [AllowAnonymous]
        [Route("forgotpassword")]
        public HttpResponseMessage ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel, string lang = "en")
        {
            try
            {
                string Message = string.Empty;
                string Status = string.Empty;
                if (ModelState.IsValid)
                {
                    var path = HttpContext.Current.Server.MapPath("~/");
                    if (_customerService.ForgotPassword(forgotPasswordViewModel.EmailAddress, path, ref Status, ref Message))
                    {

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = lang == "en" ? Message : ArabicDictionary.Translate(Message, false)
                        });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = Status,
                        message = Message
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "error",
                        message = lang == "en" ? "Email required" : ArabicDictionary.Translate("Email required"),
                        description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
                    });
                }
            }
            catch (Exception ex)
            {
                //log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false) });
            }
        }

        [HttpPost]
        [Route("logout")]
        public HttpResponseMessage logout(LogoutViewModel logoutViewModel, string lang = "en")
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    _customerSessionService.ExpireSession(customerId, logoutViewModel.DeviceID);

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = lang == "en" ? "Logged out successfully" : ArabicDictionary.Translate("Logged out successfully", false)
                    });

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
        [Authorize]
        [Route("account")]
        public HttpResponseMessage Delete(SignupViewModel signupViewModel, string lang = "en")
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

                    if (_customerService.VerifyPassword(signupViewModel.Password, (long)customerId,ref message))
                    {
                        if (_customerService.DeleteCustomer(customerId, ref message))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = lang == "en" ? message : ArabicDictionary.Translate(message)
                            });
                        }
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = lang == "en" ? "Authorization failed for current request" : ArabicDictionary.Translate("Authorization failed for current request", false) });
                    }
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message });
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
