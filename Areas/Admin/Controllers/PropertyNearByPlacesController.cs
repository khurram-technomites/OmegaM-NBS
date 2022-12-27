using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NowBuySell.Data;
using NowBuySell.Service;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    public class PropertyNearByPlacesController : Controller
    {
        private readonly INearByPlaceService _NearByPlaceService;
        private readonly INearByPlacesCategoryService _NearByPlacesCategoryService;
        private readonly IPropertyService _propertyService;

        public PropertyNearByPlacesController(INearByPlaceService nearByPlace, INearByPlacesCategoryService nearByPlacesCategory, IPropertyService propertyService)
        {
            this._NearByPlaceService = nearByPlace;
            this._NearByPlacesCategoryService = nearByPlacesCategory;
            this._propertyService = propertyService;
        }
        public ActionResult NearByPlacesCreate(long id)
        {
            var Property = _propertyService.GetById((int)id);

            ViewBag.PropertyLatitude = Property.Latitude;
            ViewBag.PropertyLongitude = Property.Longitude;
            ViewBag.PropertyAddress = Property.Address;
            ViewBag.NearByPlacesCategoryID = new SelectList(_NearByPlacesCategoryService.GetNearByPlacesCategoryForDropDown(), "value", "text");
            ViewBag.PropertyID = id;
            return View();
        }
        [HttpPost]
        public ActionResult NearByPlacesCreate(NearByPlace nearByPlace)
        {
            string message = string.Empty;
            NearByPlace data = new NearByPlace();
            data.PropertyID = nearByPlace.PropertyID;
            data.NearByPlacesCategoryID = nearByPlace.NearByPlacesCategoryID;
            data.Name = nearByPlace.Name;
            data.NameAr = nearByPlace.NameAr;
            data.Distance = nearByPlace.Distance;
            data.Latitude = nearByPlace.Latitude;
            data.Longitude = nearByPlace.Longitude;

            if (_NearByPlaceService.CreateNearByPlace(ref data, ref message))
            {
                var NearByPlaceCategory = _NearByPlacesCategoryService.GetNearByPlacesCategory((long)data.NearByPlacesCategoryID);
                return Json(new
                {
                    success = true,
                    message = message,
                    data = new
                    {
                        ID = data.ID,
                        Category = NearByPlaceCategory.Name == null ? "#" : NearByPlaceCategory.Name,
                        Name = data.Name,
                        Distance = data.Distance,
                        Image = NearByPlaceCategory.Image == null ? "#" : NearByPlaceCategory.Image
                    }
                    //PropertyPackages = PropertyPackages
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = false,
                message = "NearByPlaces not added!",
                data = data
                //PropertyPackages = PropertyPackages
            }, JsonRequestBehavior.AllowGet);


        }
        public ActionResult Details(int id)
        {
            var nearByPlace = _NearByPlaceService.GetNearByPlace(id);
            return View(nearByPlace);
        }
        public ActionResult NearByPlacesDelete(long id)
        {

            string message = string.Empty;
            _NearByPlaceService.DeleteNearByPlace(id, ref message, false);
            return Json(new { success = true, data = id, message });
        }

        [HttpGet]
        public ActionResult GetNearByPlaces(long id)
        {
            var NearByPlaces = _NearByPlaceService.GetNearByPlacesByPropertyID(id).Select(i => new { ID = i.ID, Name = i.Name, Category = i.NearByPlacesCategory.Name, Distance = i.Distance, Image = i.NearByPlacesCategory.Image });
            //var PropertyPackages = _PropertyPackageService.GetPackageByPropertyID(id).Select(i => new { id = i.ID, PropertyID = i.PropertyID, packageId = i.PackageID, price = i.Price, kilometer = i.Kilometer }).ToList();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                data = NearByPlaces
                //PropertyPackages = PropertyPackages
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeletePropertyNearByPlaces(long id)
        {

            string message = string.Empty;
            _NearByPlaceService.DeleteNearByPlace(id, ref message);
            return Json(new { success = true, data = id, message });
        }
    }
}