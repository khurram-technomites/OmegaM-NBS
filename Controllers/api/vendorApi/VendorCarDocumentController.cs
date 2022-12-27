using Newtonsoft.Json;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.ViewModels.Api.CarDocument;
using NowBuySell.Web.ViewModels.Api.CustomerDocument;
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
    public class VendorCarDocumentController : ApiController
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICarDocumentService _carDocumentService;
		
		public VendorCarDocumentController(ICarDocumentService carDocumentService)
		{
			this._carDocumentService = carDocumentService;
		}

		[HttpGet]
		[Route("car/{carId}/document")]
		public HttpResponseMessage GetDocuments(long carId)
		{
            try {

			var document = _carDocumentService.GetDocumentByCarID(carId).Select(i => new { id = i.ID, name = i.Name, path = i.Path });

			return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", document = document });

		      }

			catch (Exception ex)
			{
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}


        [HttpPost]
        [Route("{id}/document/delete")]
        public HttpResponseMessage Delete(long id)
        {
            try
            {

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    string message = string.Empty;
                    if (_carDocumentService.DeleteDocument(id, ref message))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = "Document deleted successfully"
                        });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "error",
                        message = message
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = "Session invalid or expired !"
                    });
                }


            }
            catch (Exception ex)
            {
                //log.Error("Error", ex);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }


        [HttpPost]
        [Route("car/{carId}/document")]
        public async Task<HttpResponseMessage> CreateDocument(long carId)
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string message = string.Empty;
                    string status = string.Empty;
                    string filePath = string.Format("/Assets/AppFiles/Documents/Cars/{0}/", carId.ToString().Replace(" ", "_"));
                    string root = HttpContext.Current.Server.MapPath(filePath);
                    var provider = new CustomMultipartFormDataStreamProvider(root);


                    if (!Directory.Exists(root))
                    {
                        string dir = "/Assets/AppFiles/Documents/Cars/";
                        string dirCar = carId.ToString().Replace(" ", "_");

                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + dirCar);
                        
                    }

                    CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);

                    CarDocumentViewModel carDocumentViewModel = JsonConvert.DeserializeObject<CarDocumentViewModel>(provider.FormData.GetValues("documentdetails").FirstOrDefault());



                    var documentIsExist = _carDocumentService.GetDocumentByCarID(carId);

                    if (documentIsExist.Count() > 0)
                    {
                        foreach (var item in documentIsExist)
                        {
                            if (item.Name == carDocumentViewModel.Name && item.CarID == carId)
                            {
                                CarDocument documentUpdate = new CarDocument()
                                {
                                    ID = item.ID,
                                    Path = filePath + file.filePath,
                                };

                                if (_carDocumentService.UpdateDocument(ref documentUpdate, ref message))
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new
                                    {
                                        status = "success",
                                        message = "Document saved!"
                                    });
                                }

                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                                    {
                                        status = "error",
                                        message = "Document not saved! "
                                    });
                                }

                            }


                        }

                        CarDocument document = new CarDocument()
                        {
                            Name = carDocumentViewModel.Name,
                            CarID = carId,
                            Path = filePath + file.filePath,

                        };

                        if (_carDocumentService.CreateDocument(ref document, ref message))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = "Document added!"
                            });

                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = "error",
                                message = "Failed to upload "
                            });
                        }
                    }

                    else
                    {

                        CarDocument document = new CarDocument()
                        {
                            Name = carDocumentViewModel.Name,
                            CarID = carId,
                            Path = filePath + file.filePath,

                        };

                        if (_carDocumentService.CreateDocument(ref document, ref message))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = "Document added!"
                            });

                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                status = "error",
                                message = "Failed to upload "
                            });
                        }
                    }




                }
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "error",
                    message = "Failed to upload "
                });




            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }


    }
}
