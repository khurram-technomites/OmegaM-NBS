using LinqToExcel;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.POCO;
using NowBuySell.Web.Helpers.Routing;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    public class MotorTrialBookingController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ITestRunCustomerService _testRunCustomerservice;
        private readonly IVendorRequestsService _vendorRequestService;

        public MotorTrialBookingController(ITestRunCustomerService testRunCustomerservice, IVendorRequestsService vendorRequestService)
        {
            _testRunCustomerservice = testRunCustomerservice;
            _vendorRequestService = vendorRequestService;
        }
    }
}