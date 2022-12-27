using Newtonsoft.Json;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1/vendor")]
    public class VendorCarImagesController : ApiController
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICarImageService _carImageService;
		private readonly ICarService _carService;


		public VendorCarImagesController(ICarImageService carImageService, ICarService carService)
		{
			this._carService = carService;
			this._carImageService = carImageService;
		}

		//[HttpGet]
		//[Route("cars/{carId}/images")]
		//public HttpResponseMessage GetImages(long carId)
		//{
		//	try
		//	{
		//		var document = _carImageService.GetCarImages(carId).Select(i => new { id = i.ID, title = i.Title, path = i.Position ,image = i.Image });

		//		return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", document = document });
		//	}

		//	catch (Exception ex)
		//	{
		//		log.Error("Error", ex);
		//		return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
		//	}
		//}
        
  //      [HttpPut]
  //      [Route("cars/{carId}/images")]
  //      public async Task<HttpResponseMessage> UploadGallery(long carId)
  //      {
  //          try
  //          {
  //              int count = 0;
  //              var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
  //              var claims = identity.Claims;
  //              long vendorId;
  //              if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
  //              {

  //                  var car = _carService.GetCar(carId);

  //                  if (!Request.Content.IsMimeMultipartContent())
  //                  {
  //                      throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
  //                  }
  //                  string message = string.Empty;
  //                  string status = string.Empty;
  //                  string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/Gallery/", car.SKU.Replace(" ", "_"));
                   
  //                  string root = HttpContext.Current.Server.MapPath(relativePath);

  //                  if (!Directory.Exists(root))
  //                  {
  //                      string dir = "/Assets/AppFiles/Images/Car/";
                       
  //                      string sku = car.SKU.ToString().Replace(" ", "_");
                        
  //                      if (!Directory.Exists(HttpContext.Current.Server.MapPath(dir) + sku))
  //                      {
  //                          Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + sku);
  //                      }

  //                      Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + sku + "/Gallery");
  //                  }
                    
  //                  var provider = new CustomMultipartFormDataStreamProvider(root);
  //                  CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);


  //                  var documentIsExist = _carImageService.GetCarImages(carId);

  //                  if (documentIsExist.Count() >= 0)
  //                  {
  //                      string path = string.Empty;
  //                      for (int i = 0; i < file.FileData.Count; i++)
  //                      {
  //                          var name = file.FileData[i].LocalFileName;
  //                          path = relativePath + "/" + name.Substring(name.IndexOf("Gallery\\") + 13);
  //                          CarImage carImage = new CarImage()
  //                          {
  //                              CarID = carId,
  //                              Image = path,
  //                              Position = ++count
  //                          };

  //                          _carImageService.CreateCarImage(ref carImage, ref message);
  //                      }
  //                  }

  //                  else
  //                  {
  //                      return Request.CreateResponse(HttpStatusCode.InternalServerError, new
  //                      {
  //                          status = "error",
  //                          message = "Images not saved! "
  //                      });
  //                  }

  //                  return Request.CreateResponse(HttpStatusCode.InternalServerError, new
  //                  {
  //                      status = "error",
  //                      message = "Images saved! "
  //                  });

  //              }
  //              else
  //              {
  //                  return Request.CreateResponse(HttpStatusCode.BadRequest, new { status = "Session expired", message = "Oops! Something went wrong. Please try later." });
  //              }

  //          }
  //          catch (Exception ex)
  //          {
  //              log.Error("Error", ex);
  //              //Logs.Write(ex);
  //              return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
  //          }
  //      }

  //      [HttpPut]
  //      [Route("cars/{carId}/thumbnail")]
  //      public async Task<HttpResponseMessage> UploadThumbnail(long carId)
  //      {
  //          try
  //          {
            
  //              var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
  //              var claims = identity.Claims;
  //              long vendorId;
  //              if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
  //              {

  //                  var car = _carService.GetCar(carId);

  //                  if (!Request.Content.IsMimeMultipartContent())
  //                  {
  //                      throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
  //                  }
  //                  string message = string.Empty;
  //                  string status = string.Empty;
  //                  string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}", car.SKU.Replace(" ", "_"));

  //                  string root = HttpContext.Current.Server.MapPath(relativePath);

  //                  if (!Directory.Exists(root))
  //                  {
  //                      string dir = "/Assets/AppFiles/Images/Car/";

  //                      string sku = car.SKU.ToString().Replace(" ", "_");

  //                      Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + sku);
                        
  //                  }

  //                  var provider = new CustomMultipartFormDataStreamProvider(root);
  //                  CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);

                

  //                  if (car != null)
  //                  {
  //                      car.Thumbnail = relativePath + file.filePath;

  //                      if (_carService.UpdateCar(ref car , ref message , false))
  //                      {
  //                          return Request.CreateResponse(HttpStatusCode.InternalServerError, new
  //                          {
  //                              status = "success",
  //                              message = "Thumbnail saved! "
  //                          });
  //                      }
  //                  }

  //                  else
  //                  {
  //                      return Request.CreateResponse(HttpStatusCode.InternalServerError, new
  //                      {
  //                          status = "error",
  //                          message = "Thumbnail not saved! "
  //                      });
  //                  }

  //                  return Request.CreateResponse(HttpStatusCode.InternalServerError, new
  //                  {
  //                      status = "error",
  //                      message = "Invalid id! "
  //                  });

  //              }
  //              else
  //              {
  //                  return Request.CreateResponse(HttpStatusCode.BadRequest, new { status = "Session expired", message = "Oops! Something went wrong. Please try later." });
  //              }

  //          }
  //          catch (Exception ex)
  //          {
  //              log.Error("Error", ex);
  //              //Logs.Write(ex);
  //              return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
  //          }
  //      }



    }
}
