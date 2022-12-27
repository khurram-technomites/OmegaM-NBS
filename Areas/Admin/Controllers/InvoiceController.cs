using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class InvoiceController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IInvoiceService _invoiceService;
        public InvoiceController(IInvoiceService _invoiceService)
        {
            this._invoiceService = _invoiceService;
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult InvoiceDetail(long? id)
        {
            if (id != null && id > 0)
            {
                var invoice = _invoiceService.GetInvoiceByOrder((long)id);

                if (invoice == null)
                {
                    return HttpNotFound();
                }
                return View(invoice);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
    }
}