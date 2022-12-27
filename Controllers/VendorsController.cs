using NowBuySell.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Controllers
{
    public class VendorsController : Controller  
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;
        private readonly IVendorService _Service;

        public VendorsController(IVendorService Service)
        {
            _Service = Service;
        }
        // GET: Vendor
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
                var vendor = _Service.GetVendorByAuth(AuthCode.Replace(" ", "+"));
                if (vendor != null)
                {
                    vendor.IsEmailVerified = true;
                    if (_Service.UpdateVendor(ref vendor, ref message))
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
    }
}