using NowBuySell.Service;
using NowBuySell.Web.ViewModels.Account;
using System;
using System.Web.Mvc;

namespace NowBuySell.Web.Controllers
{

    public class CustomerController : Controller
    {        
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            this._customerService = customerService;
        }
        // GET: Customer
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Verify()
        {
            var AuthCode = Request.QueryString["auth"];
            if (AuthCode == "" || AuthCode == null)
            {
                ViewBag.ErrorMessage = "Invalid Session!";
            }
            else
            {
                string message = string.Empty;
                string status = string.Empty;
                var customer = _customerService.GetByAuthCode(AuthCode.Replace(" ", "+"));
                if (customer != null)
                {
                    customer.IsEmailVerified = true;
                    if (_customerService.UpdateCustomer(ref customer, ref message, ref status))
                    {
                        ViewBag.SuccessMessage = "Your Account is Verified!";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Oops! Something went wrong. Please try later.";
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid Session!";
                }
            }

            return View();
        }

        public ActionResult ResetPassword()
        {
            var AuthCode = Request.QueryString["auth"];
            if (AuthCode == "" || AuthCode == null)
            {
                ViewBag.ErrorMessage = "Invalid Session!";
            }
            else
            {
                var customer = _customerService.GetByAuthCode(AuthCode);
                if (customer != null)
                {
                    if (customer.AuthorizationExpiry >= Helpers.TimeZone.GetLocalDateTime())
                    {
                        Session["CustomerResetID"] = 183;
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Session expired!";
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid Session!";
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            string Message = string.Empty;

            if (ModelState.IsValid)
            {
                if (Session["CustomerResetID"] != null)
                {
                    Int64 CustomerId = Convert.ToInt64(Session["CustomerResetID"]);

                    if (_customerService.ResetPassword(resetPasswordViewModel.NewPassword, CustomerId, ref Message))
                    {
                        Session.Remove("CustomerResetID");
                        string url = "https://nowbuysell.com/";
                        return Json(new
                        {
                            success = true,
                            url = url,
                            message = Message
                        });
                    }
                    else
                    {
                        ErrorMessage = Message;
                    }
                }
                else
                {
                    ErrorMessage = "Session expired!";
                }
            }
            else
            {
                ErrorMessage = "Please fill the form properly ...";
            }
            return Json(new
            {
                success = false,
                message = ErrorMessage
            });
        }
    }
}