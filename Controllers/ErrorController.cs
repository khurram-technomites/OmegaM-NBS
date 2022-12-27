using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Controllers
{
    public class ErrorController : Controller
    {
        private readonly HttpContextBase _httpContext;
        public ErrorController()
        {
        }

        // GET: Error
        //[Route("page-not-found")]
        public ActionResult PageNotFound()
        {
            return View();
        }

        public ActionResult InternalServerError()
        {
            return View();
        }

    }
}