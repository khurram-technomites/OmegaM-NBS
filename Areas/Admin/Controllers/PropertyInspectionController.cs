using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
namespace NowBuySell.Web.Areas.Admin.Controllers
{
    public class PropertyInspectionController : Controller
    {
        private readonly IPropertyInspectionService _PropertyInspectionService;

        public PropertyInspectionController(IPropertyInspectionService PropertyInspectionService)
        {
            this._PropertyInspectionService = PropertyInspectionService;
        }
        public ActionResult CreateInspection(long id)
        {
            ViewBag.PropertyID = id;
            return View();
        }
        [HttpPost]
        public ActionResult CreateInspection(string Name, long id)
        {
            string message = string.Empty;
            PropertyInspection data = new PropertyInspection();
            data.PropertyID = id;
            string absolutePath = Server.MapPath("~");
            string relativePath = string.Format("/Assets/AppFiles/Documents/Propertys/{0}/", id.ToString().Replace(" ", "_"));
            data.Name = Name;
            data.Path = Uploader.UploadDocs(Request.Files, absolutePath, relativePath, "Document", ref message, "FileUpload");
            var inspection = _PropertyInspectionService.GetInspectionByPropertyID(id).ToList();
            if (inspection.Count == 0)
            {
                if (_PropertyInspectionService.CreateInspection(ref data, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        message = message,
                        data = data
                        //PropertyPackages = PropertyPackages
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {

                return Json(new
                {
                    success = false,
                    message = "You cannot add more than one Inspection!",
                    data = data
                    //CarPackages = carPackages
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = false,
                message = "Inspection not added!",
                data = data
                //PropertyPackages = PropertyPackages
            }, JsonRequestBehavior.AllowGet);


        }

        public ActionResult DeletePropertyInspection(long id)
        {

            string message = string.Empty;
            _PropertyInspectionService.DeletePropertyInspection(id, ref message);
            return Json(new { success = true, data = id, message });
        }

        [HttpGet]
        public ActionResult GetInspections(long id)
        {
            var inspection = _PropertyInspectionService.GetInspectionByPropertyID(id).Select(i => new { id = i.ID, name = i.Name, path = i.Path });
            //var PropertyPackages = _PropertyPackageService.GetPackageByPropertyID(id).Select(i => new { id = i.ID, PropertyID = i.PropertyID, packageId = i.PackageID, price = i.Price, kilometer = i.Kilometer }).ToList();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                document = inspection
                //PropertyPackages = PropertyPackages
            }, JsonRequestBehavior.AllowGet);
        }
    }
}