using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class AttributeController : Controller
    {
        // GET: Admin/Attribute
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IAttributeService _attributeService;

        public AttributeController(IAttributeService attributeService)
        {
            this._attributeService = attributeService;
        }

        [HttpGet]
        public ActionResult GetAttributes(long id)
        {
            var attributes = _attributeService.GetAttributesForDropDown();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                attributes = attributes
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NowBuySell.Data.Attribute attribute)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_attributeService.CreateAttribute(ref attribute, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            id = attribute.ID,
                            name = attribute.Name
                        },
                        message = "Car category assigned.",
                    });
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }
            return Json(new { success = false, message = message });
        }
    }
}