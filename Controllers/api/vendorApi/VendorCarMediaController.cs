using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api.vendorApi
{
    [Authorize(Roles = "Vendor")]
    [RoutePrefix("api/v1/vendor")]
    public class VendorCarMediaController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICarImageService _carImageService;
        private readonly ICarService _carService;
        private readonly ICarDocumentService _carDocumentService;
        private readonly ICarInspectionService _carInspectionService;
        public VendorCarMediaController(ICarImageService carImageService
            , ICarService carService
            , ICarDocumentService carDocumentService, ICarInspectionService carInspectionService)
        {
            this._carService = carService;
            this._carImageService = carImageService;
            this._carDocumentService = carDocumentService;
            this._carInspectionService = carInspectionService;
        }

        /*Gallery Images*/
        [HttpGet]
        [Route("cars/{carId}/images")]
        public HttpResponseMessage GetGalleryImages(long carId)
        {
            try
            {
                string ImageServer = CustomURL.GetImageServer();

                var images = _carImageService.GetCarImages(carId).Select(i => new
                {
                    id = i.ID,
                    title = i.Title,
                    image = !string.IsNullOrEmpty(i.Image) ? ImageServer + i.Image : ""
                });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",
                    images = images
                });
            }

            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpPut]
        [Route("cars/{carId}/images")]
        public async Task<HttpResponseMessage> UploadGalleryImages(long carId)
        {
            try
            {
                int count = 0;
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {

                    var car = _carService.GetCar(carId);

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string message = string.Empty;
                    string status = string.Empty;
                    string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/Gallery/", car.SKU.Replace(" ", "_"));

                    string root = HttpContext.Current.Server.MapPath(relativePath);

                    if (!Directory.Exists(root))
                    {
                        string dir = "/Assets/AppFiles/Images/Car/";

                        string sku = car.SKU.ToString().Replace(" ", "_");

                        if (!Directory.Exists(HttpContext.Current.Server.MapPath(dir) + sku))
                        {
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + sku);
                        }

                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + sku + "/Gallery");
                    }

                    var provider = new CustomMultipartFormDataStreamProvider(root);
                    CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);

                    var documentIsExist = _carImageService.GetCarImages(carId);

                    if (documentIsExist.Count() >= 0)
                    {
                        string path = string.Empty;
                        for (int i = 0; i < file.FileData.Count; i++)
                        {
                            var name = file.FileData[i].LocalFileName;
                            path = relativePath + "/" + name.Substring(name.IndexOf("Gallery\\") + 11);
                            CarImage carImage = new CarImage()
                            {
                                CarID = carId,
                                Image = path,
                                Position = ++count
                            };

                            _carImageService.CreateCarImage(ref carImage, ref message);
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                        {
                            status = "error",
                            message = "Images not saved! "
                        });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = "Images saved!"
                    });

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "Session expired",
                        essage = "Oops! Something went wrong. Please try later."
                    });
                }

            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpDelete]
        [Route("carImages/{imageId}")]
        public HttpResponseMessage DeleteGalleryImage(long imageId)
        {
            try
            {
                string message = string.Empty;
                string filePath = string.Empty;
                if (_carImageService.DeleteCarImage(imageId, ref message, ref filePath))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + filePath);
                    System.IO.File.Delete(path);
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = true,
                        message = "Gallery image deleted"
                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    message = "Oops! Something went wrong. Please try later."
                });

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new
                {
                    success = false,
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        /*Thumbnail*/
        [HttpPut]
        [Route("cars/{carId}/thumbnail")]
        public async Task<HttpResponseMessage> UploadThumbnail(long carId)
        {
            try
            {

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {

                    var car = _carService.GetCar(carId);

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string message = string.Empty;
                    string status = string.Empty;
                    string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}", car.SKU.Replace(" ", "_"));

                    string root = HttpContext.Current.Server.MapPath(relativePath);

                    if (!Directory.Exists(root))
                    {
                        string dir = "/Assets/AppFiles/Images/Car/";

                        string sku = car.SKU.ToString().Replace(" ", "_");

                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + sku);

                    }

                    var provider = new CustomMultipartFormDataStreamProvider(root);
                    CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);

                    if (car != null)
                    {
                        car.Thumbnail = relativePath + file.filePath;

                        if (_carService.UpdateCar(ref car, ref message, false))
                        {
                            string ImageServer = CustomURL.GetImageServer();
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = "Thumbnail saved! ",
                                Thumbnail = ImageServer + car.Thumbnail
                            });
                        }
                    }

                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                        {
                            status = "error",
                            message = "Thumbnail not saved! "
                        });
                    }

                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                    {
                        status = "error",
                        message = "Invalid id! "
                    });

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "Session expired",
                        message = "Oops! Something went wrong. Please try later."
                    });
                }

            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpDelete]
        [Route("cars/{carId}/thumbnail")]
        public HttpResponseMessage DeleteThumbnail(long carId)
        {
            try
            {
                string message = string.Empty;
                string filepath = string.Empty;

                Car model = _carService.GetCar(carId);
                if (!string.IsNullOrEmpty(model.Thumbnail))
                {
                    filepath = model.Thumbnail;
                    model.Thumbnail = null;

                    if (_carService.UpdateCar(ref model, ref message))
                    {
                        string path = HttpContext.Current.Server.MapPath("~" + filepath);
                        System.IO.File.Delete(path);
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            success = true,
                            message = "Thumbnail deleted!"
                        });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = false,
                        message = message
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = false,
                        message = "Thumbnail already deleted!"
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        /*Video*/
        [HttpPut]
        [Route("cars/{carId}/video")]
        public async Task<HttpResponseMessage> UploadVideo(long carId)
        {
            try
            {

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {

                    var car = _carService.GetCar(carId);

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string message = string.Empty;
                    string status = string.Empty;
                    string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}", car.SKU.Replace(" ", "_"));

                    string root = HttpContext.Current.Server.MapPath(relativePath);

                    if (!Directory.Exists(root))
                    {
                        string dir = "/Assets/AppFiles/Images/Car/";

                        string sku = car.SKU.ToString().Replace(" ", "_");

                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + sku);

                    }

                    var provider = new CustomMultipartFormDataStreamProvider(root);
                    CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);

                    if (car != null)
                    {
                        car.Video = relativePath + file.filePath;

                        if (_carService.UpdateCar(ref car, ref message, false))
                        {
                            string ImageServer = CustomURL.GetImageServer();
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = "Video uploaded! ",
                                Video = ImageServer + car.Video
                            });
                        }
                    }

                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                        {
                            status = "error",
                            message = "Thumbnail not saved! "
                        });
                    }

                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                    {
                        status = "error",
                        message = "Invalid id! "
                    });

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "Session expired",
                        message = "Oops! Something went wrong. Please try later."
                    });
                }

            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpDelete]
        [Route("cars/{id}/Video")]
        public HttpResponseMessage DeleteVideo(long id)
        {
            try
            {
                string message = string.Empty;
                string filepath = string.Empty;

                Car model = _carService.GetCar(id);
                if (!string.IsNullOrEmpty(model.Video))
                {
                    filepath = model.Video;
                    model.Video = null;

                    if (_carService.UpdateCar(ref model, ref message))
                    {
                        string path = HttpContext.Current.Server.MapPath("~" + filepath);
                        System.IO.File.Delete(path);
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            success = true,
                            message = "Video deleted!"
                        });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = false,
                        message = message
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = false,
                        message = "Video already deleted!"
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpDelete]
        [Route("cars/inspection/{id}")]
        public HttpResponseMessage DeleteInspection(long id)
        {
            string message = string.Empty;
            string filePath = string.Empty;
            if (_carInspectionService.DeleteCarInspection(id, ref message))
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = message });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Oops! Something went wrong. Please try later." });
        }
        /*Documents*/
        [HttpGet]
        [Route("cars/{carId}/documents")]
        public HttpResponseMessage GetDocuments(long carId)
        {
            try
            {
                var document = _carDocumentService.GetDocumentByCarID(carId).Select(i => new
                {
                    i.ID,
                    i.Name,
                    path = i.Path
                });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",
                    documents = document
                });
            }

            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpPut]
        [Route("cars/{carId}/documents")]
        public async Task<HttpResponseMessage> UploadDocuments(long carId)
        {
            try
            {
                int count = 0;
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {

                    var car = _carService.GetCar(carId);

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string message = string.Empty;
                    string status = string.Empty;
                    string relativePath = string.Format("/Assets/AppFiles/Images/Car/{0}/Documents/", car.SKU.Replace(" ", "_"));

                    string root = HttpContext.Current.Server.MapPath(relativePath);

                    if (!Directory.Exists(root))
                    {
                        string dir = "/Assets/AppFiles/Images/Car/";

                        string sku = car.SKU.ToString().Replace(" ", "_");

                        if (!Directory.Exists(HttpContext.Current.Server.MapPath(dir) + sku))
                        {
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + sku);
                        }

                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + sku + "/Documents");
                    }

                    var provider = new CustomMultipartFormDataStreamProvider(root);
                    CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);


                    var documentIsExist = _carDocumentService.GetDocumentByCarID(carId);

                    if (documentIsExist.Count() >= 0)
                    {
                        string path = string.Empty;
                        for (int i = 0; i < file.FileData.Count; i++)
                        {
                            var name = file.FileData[i].LocalFileName;
                            path = relativePath + "/" + name.Substring(name.IndexOf("Documents\\") + 12);
                            CarDocument carDocument = new CarDocument()
                            {
                                CarID = carId,
                                Name = name,
                                Path = path,
                            };

                            _carDocumentService.CreateDocument(ref carDocument, ref message);
                        }
                    }

                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                        {
                            status = "error",
                            message = "Documents not uploaded! "
                        });
                    }

                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                    {
                        status = "error",
                        message = "Document uploaded! "
                    });

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "Session expired",
                        message = "Oops! Something went wrong. Please try later."
                    });
                }

            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpDelete]
        [Route("carDocuments/{documentId}")]
        public HttpResponseMessage DeleteDocument(long documentId)
        {
            try
            {
                string message = string.Empty;
                string filePath = string.Empty;

                CarDocument document = _carDocumentService.GetDocument(documentId);
                filePath = document.Path;

                if (_carDocumentService.DeleteDocument(documentId, ref message))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + filePath);
                    System.IO.File.Delete(path);
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = true,
                        message = "Document deleted!"
                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    message = message
                });
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        /*Bulk Media Upload*/
        [HttpPost]
        [Route("cars/{id}/Images")]
        public async Task<HttpResponseMessage> Thumbnail(long id)
        {
            string message = string.Empty;
            string Thumbnailpath = string.Empty, Videopath = string.Empty, Gallerypath = string.Empty, messages = string.Empty;
            List<string> ErrorList = new List<string>();
            List<string> SuccessList = new List<string>();
            string ImageServer = CustomURL.GetImageServer();

            try
            {
                Car car = _carService.GetCar(id);
                if (car == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                string ThumbnailfilePath = string.Format("/Assets/AppFiles/Images/Car/{0}/", car.SKU.Replace(" ", "_"));
                string rootThumbnail = HttpContext.Current.Server.MapPath(ThumbnailfilePath);
                var providerThumbnail = new CustomMultipartFormDataStreamProvider(rootThumbnail);

                if (!Directory.Exists(HttpContext.Current.Server.MapPath(string.Format("/Assets/AppFiles/Images/Car/{0}/", car.SKU.Replace(" ", "_")))))
                {
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(string.Format("/Assets/AppFiles/Images/Car/{0}/", car.SKU.Replace(" ", "_"))));
                }

                CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(providerThumbnail);
                var httpRequest = HttpContext.Current.Request;
                if (file.Contents.Count() > 0)
                {
                    List<PathNameList> list = file.filePathList;

                    List<PathNameList> gallery = list.Where(x => x.Type == "\"gallery\"" && x.Filename != "\"\"").ToList();
                    PathNameList thumbnail = list.Where(x => x.Type == "\"thumbnail\"" && x.Filename != "\"\"").FirstOrDefault();
                    PathNameList video = list.Where(x => x.Type == "\"video\"" && x.Filename != "\"\"").FirstOrDefault();
                    List<PathNameList> document = list.Where(x => x.Type == "\"plan\"" && x.Filename != "\"\"").ToList();
                    PathNameList inspection = list.Where(x => x.Type == "\"inspection\"" && x.Filename != "\"\"").FirstOrDefault();

                    if (thumbnail != null)
                    {
                        Thumbnailpath = ThumbnailfilePath + "/" + thumbnail.LocalPath;
                        string path = HttpContext.Current.Server.MapPath(Thumbnailpath.Replace("///", "/"));                        

                        var stream = new MemoryStream(File.ReadAllBytes(path));
                        Image img = Image.FromStream(stream);
                        Image watermarkImage = Image.FromFile(HttpContext.Current.Server.MapPath(("/Assets/images/NBS-Watermark.png")));
                        Bitmap bitmap = new Bitmap(watermarkImage);
                        Image resizeWatermark = resizeImage(bitmap, new Size(img.Size.Width / 2, img.Size.Height / 2));

                        using (Graphics imageGraphics = Graphics.FromImage(img))
                        using (TextureBrush watermarkBrush = new TextureBrush(resizeWatermark))
                        {
                            int x = (img.Width / 2 - resizeWatermark.Width / 2);
                            int y = (img.Height / 2 - resizeWatermark.Height / 2);
                            watermarkBrush.TranslateTransform(x, y);
                            imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(resizeWatermark.Width, resizeWatermark.Height)));
                            //imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(0, 0), img.Size));
                            img.Save(path);
                        }

                        car.Thumbnail = Thumbnailpath.Replace("///", "/");

                        if (_carService.UpdateCar(ref car, ref message, false))
                            SuccessList.Add("Thumbnail uploaded successfully");
                    }
                    else
                    {
                        ErrorList.Add("Thumbnail not found");
                    }

                    if (video != null)
                    {
                        Videopath = ThumbnailfilePath + "/" + video.LocalPath;
                        car.Video = Videopath.Replace("///", "/");

                        if (_carService.UpdateCar(ref car, ref message, false))
                            SuccessList.Add("Video uploaded successfully");
                    }
                    else
                    {
                        ErrorList.Add("Video not found");
                    }

                    if (gallery != null && gallery.Count() > 0)
                    {
                        foreach (var entry in gallery)
                        {

                            CarImage imageModel = new CarImage();

                            Gallerypath = ThumbnailfilePath + "/" + entry.LocalPath;
                            string path = HttpContext.Current.Server.MapPath(Gallerypath.Replace("///", "/"));

                            //var stream = new MemoryStream(File.ReadAllBytes(path));
                            //Image img = Image.FromStream(stream);
                            //Image watermarkImage = Image.FromFile(HttpContext.Current.Server.MapPath(("/Assets/images/NBS-Watermark.png")));
                            //Graphics imageGraphics = Graphics.FromImage(img);
                            //using (TextureBrush watermarkBrush = new TextureBrush(watermarkImage))
                            //{
                            //    //x and y co-ordinates of watermark.
                            //    int x = (img.Width / 2 - watermarkImage.Width / 2);
                            //    int y = (img.Height / 2 - watermarkImage.Height / 2);

                            //    watermarkBrush.TranslateTransform(x, y);

                            //    //drawing watermark on image
                            //    imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(watermarkImage.Width, watermarkImage.Height)));

                            //    img.Save(path);
                            //}

                            var stream = new MemoryStream(File.ReadAllBytes(path));
                            Image img = Image.FromStream(stream);
                            Image watermarkImage = Image.FromFile(HttpContext.Current.Server.MapPath(("/Assets/images/NBS-Watermark.png")));
                            Bitmap bitmap = new Bitmap(watermarkImage);
                            Image resizeWatermark = resizeImage(bitmap, new Size(img.Size.Width / 2, img.Size.Height / 2));

                            using (Graphics imageGraphics = Graphics.FromImage(img))
                            using (TextureBrush watermarkBrush = new TextureBrush(resizeWatermark))
                            {
                                int x = (img.Width / 2 - resizeWatermark.Width / 2);
                                int y = (img.Height / 2 - resizeWatermark.Height / 2);
                                watermarkBrush.TranslateTransform(x, y);
                                imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(resizeWatermark.Width, resizeWatermark.Height)));
                                //imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(0, 0), img.Size));
                                img.Save(path);
                            }

                            imageModel.CarID = id;
                            imageModel.Image = Gallerypath.Replace("///", "/");

                            if (_carImageService.CreateCarImage(ref imageModel, ref messages))
                                SuccessList.Add(string.Format("{0} image uploaded successfully", entry.UploadedFileName));
                            else
                                ErrorList.Add(string.Format("There is a problem uploading {0} image, Please try again", entry.UploadedFileName));
                        }
                    }
                    else
                        ErrorList.Add("Gallery images not found");
                    if (inspection != null)
                    {

                        CarInspection data = new CarInspection();
                        data.CarID = id;
                        var inspections = _carInspectionService.GetInspectionByCarID(id).ToList();
                        string absolutePath = HttpContext.Current.Server.MapPath("~");
                        string relativePath = string.Format("/Assets/AppFiles/Documents/Car/{0}/", id.ToString().Replace(" ", "_"));
                        var inspeactionfile = httpRequest.Files["inspection"];
                        string fileExtension = System.IO.Path.GetExtension(inspeactionfile.FileName);
                        string FilePath = string.Format("{0}{1}{2}{3}", relativePath, "Document", Guid.NewGuid().ToString(), fileExtension);
                        Directory.CreateDirectory(absolutePath + relativePath);
                        absolutePath += FilePath;
                        inspeactionfile.SaveAs(absolutePath);
                        data.Path = FilePath;
                        if (inspections.Count == 0)
                        {
                            if (_carInspectionService.CreateInspection(ref data, ref message))
                            {
                                SuccessList.Add(string.Format("{0} car inspection uploaded successfully", inspeactionfile.FileName));
                            }
                            else
                                ErrorList.Add(string.Format("There is a problem uploading {0} Doc, Please try again", inspeactionfile.FileName));
                        }
                        else
                        {
                            ErrorList.Add("You cannot add more than one Inspection!");
                        }



                    }
                    else
                        ErrorList.Add("Property Inspection not found");
                    if (document != null && document.Count() > 0)
                    {
                        foreach (var entry in document)
                        {

                            CarDocument documentModel = new CarDocument();

                            Gallerypath = ThumbnailfilePath + "/" + entry.LocalPath;

                            documentModel.CarID = id;
                            documentModel.Path = Gallerypath.Replace("///", "/");

                            if (_carDocumentService.CreateDocument(ref documentModel, ref messages))
                                SuccessList.Add(string.Format("{0} document uploaded successfully", entry.UploadedFileName));
                            else
                                ErrorList.Add(string.Format("There is a problem uploading {0} document, Please try again", entry.UploadedFileName));
                        }
                    }
                    else
                        ErrorList.Add("documents not found");

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        success = SuccessList,
                        errors = ErrorList
                    });
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, new
                {
                    success = false,
                    message = "Kindly upload image."
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new
                {
                    success = false,
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        private static Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            //Get the image current width  
            int sourceWidth = imgToResize.Width;
            //Get the image current height  
            int sourceHeight = imgToResize.Height;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            //Calulate  width with new desired size  
            nPercentW = ((float)size.Width / (float)sourceWidth);
            //Calculate height with new desired size  
            nPercentH = ((float)size.Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //New Width  
            int destWidth = (int)(sourceWidth * nPercent);
            //New Height  
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // Draw image with new width and height  
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return (System.Drawing.Image)b;
        }

    }
}
