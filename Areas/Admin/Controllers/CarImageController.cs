using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CarImageController : Controller
    {
        // GET: Admin/CarImage
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ICarImageService _carImageService;
        private readonly ICarService _carService;

        public CarImageController(ICarImageService carImageService, ICarService carService)
        {
            this._carImageService = carImageService;
            this._carService = carService;
        }



        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetCarImages(long id)
        {
            var carImages = _carImageService.GetCarImages(id).Select(i => new
            {
                id = i.ID,
                i.Title,
                Image = i.Image,
                position = i.Position,
            }).ToList();

            return Json(new
            {
                success = true,
                message = "Data recieved successfully!",
                carImages = carImages
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Create(long? id, int count)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Car car = _carService.GetCar((long)id);
                if (car == null)
                {
                    return HttpNotFound();
                }

                string message = string.Empty;

                string absolutePath = Server.MapPath("~");
                string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/Gallery/", car.SKU.Replace(" ", "_"));

                List<string> Pictures = new List<string>();

                Dictionary<long, string> data = new Dictionary<long, string>();
                Uploader.UploadImages(Request.Files, absolutePath, relativePath, "g", ref Pictures, ref message, "GalleryImages", ApplyWatermark: true);
                foreach (var item in Pictures)
                {
                    CarImage carImage = new CarImage();
                    carImage.CarID = id;
                    carImage.Image = item;
                    carImage.Position = ++count;
                    if (_carImageService.CreateCarImage(ref carImage, ref message))
                    {
                        data.Add(carImage.ID, item);
                    }
                }

                return Json(new
                {
                    success = true,
                    message = message,
                    data = data.ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            string filePath = string.Empty;
            if (_carImageService.DeleteCarImage(id, ref message, ref filePath))
            {
                System.IO.File.Delete(Server.MapPath(filePath));
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }



        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(long id)
        //{
        //    string message = string.Empty;
        //    string filePath = string.Empty;
        //    if (_carImageService.DeleteCarImage(id, ref message, ref filePath))
        //    {
        //        System.IO.File.Delete(Server.MapPath(filePath));
        //        return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
        //    }
        //    return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        //}
    }
}