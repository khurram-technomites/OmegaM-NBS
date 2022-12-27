using NowBuySell.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    public class CustomerDocumentController : Controller
    {
        private readonly ICustomerService _customerService;
        public CustomerDocumentController(ICustomerService customerService)
        {
            this._customerService = customerService;
            
        }

        // GET: VendorPortal/CustomerDocument
        public ActionResult CustomerDocumentDetails(long id)
        {
            var customerDocument = _customerService.GetCustomer(id);
            return View(customerDocument);
        }
    }
}