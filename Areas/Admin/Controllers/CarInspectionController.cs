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
    public class CarInspectionController : Controller
    {
        private readonly ICarInspectionService _carInspectionService;
        public CarInspectionController(ICarInspectionService carInspectionService)
        {
            this._carInspectionService = carInspectionService;
        }
        public ActionResult CreateInspection(long id)
        {
            ViewBag.CarID = id;
            return View();
        }
        [HttpPost]
        public ActionResult CreateInspection(string Name, long id)
        {
            string message = string.Empty;
            CarInspection data = new CarInspection();
            data.CarID = id;
            string absolutePath = Server.MapPath("~");
            string relativePath = string.Format("/Assets/AppFiles/Documents/Cars/{0}/", id.ToString().Replace(" ", "_"));
            data.Name = Name;
            data.Path = Uploader.UploadDocs(Request.Files, absolutePath, relativePath, "Document", ref message, "FileUpload");
            var inspection = _carInspectionService.GetInspectionByCarID(id).ToList();
            if (inspection.Count == 0)
            {
                if (_carInspectionService.CreateInspection(ref data, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        message = message,
                        data = data
                        //CarPackages = carPackages
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
                //CarPackages = carPackages
            }, JsonRequestBehavior.AllowGet);


        }

        public ActionResult DeleteCarInspection(long id)
        {

            string message = string.Empty;
            _carInspectionService.DeleteCarInspection(id, ref message);
            return Json(new { success = true, data = id, message });
        }

        [HttpGet]
        public ActionResult GetInspections(long id)
        {
            var inspection = _carInspectionService.GetInspectionByCarID(id).Select(i => new { id = i.ID, name = i.Name, path = i.Path });
            //var carPackages = _carPackageService.GetPackageByCarID(id).Select(i => new { id = i.ID, carID = i.CarID, packageId = i.PackageID, price = i.Price, kilometer = i.Kilometer }).ToList();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                document = inspection
                //CarPackages = carPackages
            }, JsonRequestBehavior.AllowGet);
        }
    }
}