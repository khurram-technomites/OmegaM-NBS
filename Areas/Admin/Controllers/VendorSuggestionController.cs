using NowBuySell.Service;
using System.Web.Mvc;
using NowBuySell.Web.AuthorizationProvider;
using System.Net;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class VendorSuggestionController : Controller
    {
        private readonly IVendorSuggestionService _vendorSuggestionService;

        public VendorSuggestionController(IVendorSuggestionService vendorSuggestionService)
        {
            _vendorSuggestionService = vendorSuggestionService;
        }

        public ActionResult Index()
        {
            var List = _vendorSuggestionService.GetAll();
            return View(List);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var vendorSuggestion = _vendorSuggestionService.GetById((long)id);
            if (vendorSuggestion == null)
            {
                return HttpNotFound();
            }
            return View(vendorSuggestion);
        }
    }
}