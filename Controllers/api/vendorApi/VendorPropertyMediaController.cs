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
    public class VendorPropertyMediaController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IPropertyService _propService;
        private readonly ICityService _cityService;
        private readonly ICountryService _countryService;
        private readonly ICategoryService _categoryService;
        private readonly IFeatureService _featureService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;
        private readonly IPropertyFeaturesService _propfeatureService;
        private readonly IPropertyImagesService _propImageService;
        private readonly IPropertyFloorPlanService _floorPlanService;
        private readonly IPropertyRequestsService _propRequestService;
        private readonly IPropertyInspectionService _PropertyInspectionService;

        string ImageServer = string.Empty;

        public VendorPropertyMediaController(IPropertyService propService
            , ICityService cityService
            , ICountryService countryService
            , ICategoryService categoryService
            , IFeatureService featureService
            , INotificationReceiverService notificationReceiverService
            , INotificationService notificationService
            , IPropertyFeaturesService propfeatureService
            , IPropertyImagesService propImageService
            , IPropertyFloorPlanService floorPlanService
            , IPropertyRequestsService propRequestService,
IPropertyInspectionService propertyInspectionService)
        {
            _propService = propService;
            _cityService = cityService;
            _countryService = countryService;
            _categoryService = categoryService;
            _featureService = featureService;
            _notificationReceiverService = notificationReceiverService;
            _notificationService = notificationService;
            _propfeatureService = propfeatureService;
            _propImageService = propImageService;
            _floorPlanService = floorPlanService;
            _propRequestService = propRequestService;
            ImageServer = CustomURL.GetImageServer();
            _PropertyInspectionService = propertyInspectionService;
        }

        /*Gallery Images Start*/
        [HttpGet]
        [Route("properties/{propertyId}/images")]
        public HttpResponseMessage GetGalleryImages(long propertyId)
        {
            try
            {
                string ImageServer = CustomURL.GetImageServer();

                var images = _propImageService.GetImagesByProperty((int)propertyId).Select(i => new
                {
                    id = i.ID,
                    image = !string.IsNullOrEmpty(i.Path) ? ImageServer + i.Path : ""
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
        [Route("properties/{propertyId}/images")]
        public async Task<HttpResponseMessage> UploadGalleryImages(long propertyId)
        {
            try
            {
                int count = 0;
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {

                    var prop = _propService.GetById((int)propertyId);

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string message = string.Empty;
                    string status = string.Empty;
                    string relativePath = string.Format("/Assets/AppFiles/Images/Property/{0}/Gallery/", prop.ID.ToString());

                    string root = HttpContext.Current.Server.MapPath(relativePath);

                    if (!Directory.Exists(root))
                    {
                        string dir = "/Assets/AppFiles/Images/Property/";

                        string path = prop.ID.ToString();

                        if (!Directory.Exists(HttpContext.Current.Server.MapPath(dir) + path))
                        {
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + path);
                        }

                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + path + "/Gallery");
                    }

                    var provider = new CustomMultipartFormDataStreamProvider(root);
                    CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);

                    if (file.Contents.FirstOrDefault().Headers.ContentDisposition.FileName == "\"\"")
                        return Request.CreateResponse(HttpStatusCode.NotFound, new
                        {
                            status = "error",
                            message = "Kindle upload image! "
                        });

                    var documentIsExist = _propImageService.GetImagesByProperty((int)propertyId);

                    if (documentIsExist.Count() >= 0)
                    {
                        string path = string.Empty;
                        for (int i = 0; i < file.FileData.Count; i++)
                        {
                            var name = file.FileData[i].LocalFileName;
                            path = relativePath + "/" + name.Substring(name.IndexOf("Gallery") + 8);
                            PropertyImage propImage = new PropertyImage()
                            {
                                PropertyId = propertyId,
                                Path = path
                            };

                            _propImageService.Add(ref propImage, ref message);
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
                        message = "Images saved! "
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
        [Route("propertyImages/{id}")]
        public HttpResponseMessage DeleteImage(long id)
        {
            string message = string.Empty;
            string filePath = string.Empty;
            if (_propImageService.Remove(id, ref message, ref filePath))
            {
                string path = HttpContext.Current.Server.MapPath("~" + filePath);
                System.IO.File.Delete(path);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = message });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Oops! Something went wrong. Please try later." });
        }
        /*Gallery Images End*/

        /*FloorPlan Images Start*/
        [HttpGet]
        [Route("properties/{propertyId}/floorPlanImages")]
        public HttpResponseMessage GetFloorImages(long propertyId)
        {
            try
            {
                string ImageServer = CustomURL.GetImageServer();

                var images = _floorPlanService.GetFloorPlansByProperty((int)propertyId).Select(i => new
                {
                    id = i.ID,
                    image = !string.IsNullOrEmpty(i.Path) ? ImageServer + i.Path : ""
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
        [Route("properties/{propertyId}/floorPlanImages")]
        public async Task<HttpResponseMessage> UploadFloorPlanImages(long propertyId)
        {
            try
            {
                int count = 0;
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long vendorId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out vendorId))
                {

                    var prop = _propService.GetById((int)propertyId);

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string message = string.Empty;
                    string status = string.Empty;
                    string relativePath = string.Format("/Assets/AppFiles/Images/Property/{0}/FloorPlan/", prop.ID.ToString());

                    string root = HttpContext.Current.Server.MapPath(relativePath);

                    if (!Directory.Exists(root))
                    {
                        string dir = "/Assets/AppFiles/Images/Property/";

                        string path = prop.ID.ToString();

                        if (!Directory.Exists(HttpContext.Current.Server.MapPath(dir) + path))
                        {
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + path);
                        }

                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir) + path + "/FloorPlan");
                    }

                    var provider = new CustomMultipartFormDataStreamProvider(root);
                    CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);

                    if (file.Contents.FirstOrDefault().Headers.ContentDisposition.FileName == "\"\"")
                        return Request.CreateResponse(HttpStatusCode.NotFound, new
                        {
                            status = "error",
                            message = "Kindle upload image! "
                        });

                    var documentIsExist = _floorPlanService.GetFloorPlansByProperty((int)propertyId);

                    if (documentIsExist.Count() >= 0)
                    {
                        string path = string.Empty;
                        for (int i = 0; i < file.FileData.Count; i++)
                        {
                            var name = file.FileData[i].LocalFileName;
                            path = relativePath + "/" + name.Substring(name.IndexOf("FloorPlan") + 10);
                            PropertyFloorPlan propImage = new PropertyFloorPlan()
                            {
                                PropertyId = propertyId,
                                Path = path
                            };

                            _floorPlanService.Add(ref propImage, ref message);
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
                        message = "Images saved! "
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
        [Route("propertyFloorPlanImages/{id}")]
        public HttpResponseMessage DeleteFloorPlan(long id)
        {
            string message = string.Empty;
            string filePath = string.Empty;
            if (_floorPlanService.Remove(id, ref message, ref filePath))
            {
                string path = HttpContext.Current.Server.MapPath("~" + filePath);
                System.IO.File.Delete(path);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = message });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Oops! Something went wrong. Please try later." });
        }
        /*FloorPlan Images End*/

        [HttpDelete]
        [Route("properties/Thumbnail/{id}")]
        public HttpResponseMessage DeleteThumbnail(long id)
        {
            string message = string.Empty;
            string filepath = string.Empty;

            Property model = _propService.GetById((int)id);
            filepath = model.Thumbnail;
            model.Thumbnail = null;

            if (_propService.UpdateProperty(ref model, ref message))
            {
                string path = HttpContext.Current.Server.MapPath("~" + filepath);
                if (File.Exists(path))
                    System.IO.File.Delete(path);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = message });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Oops! Something went wrong. Please try later." });
        }

        [HttpDelete]
        [Route("properties/Video/{id}")]
        public HttpResponseMessage DeleteVideo(long id)
        {
            string message = string.Empty;
            string filepath = string.Empty;

            Property model = _propService.GetById((int)id);
            filepath = model.Video;
            model.Video = null;

            if (_propService.UpdateProperty(ref model, ref message))
            {
                string path = HttpContext.Current.Server.MapPath("~" + filepath);
                System.IO.File.Delete(path);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = message });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Oops! Something went wrong. Please try later." });
        }
        
        [HttpDelete]
        [Route("properties/inspection/{id}")]
        public HttpResponseMessage DeleteInspection(long id)
        {
           string message = string.Empty;
            string filePath = string.Empty;
            if (_PropertyInspectionService.DeletePropertyInspection(id, ref message))
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = message });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Oops! Something went wrong. Please try later." });
        }

        [HttpPost]
        [Route("properties/{id}/Image")]
        public async Task<HttpResponseMessage> Images(long id)
        {
            string message = string.Empty, responseMessage = string.Empty;
            string Thumbnailpath = string.Empty, Videopath = string.Empty, Gallerypath = string.Empty, inspectionPath = string.Empty, messages = string.Empty;
            List<string> ErrorList = new List<string>();
            List<string> SuccessList = new List<string>();

            try
            {
                Property prop = _propService.GetById((int)id);
                if (prop == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                string ThumbnailfilePath = string.Format("/Assets/AppFiles/Images/Property/{0}/", prop.ID);
                string rootThumbnail = HttpContext.Current.Server.MapPath(ThumbnailfilePath);
                var providerThumbnail = new CustomMultipartFormDataStreamProvider(rootThumbnail);

                if (!Directory.Exists(HttpContext.Current.Server.MapPath(string.Format("/Assets/AppFiles/Images/Property/{0}/", prop.ID))))
                {
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(string.Format("/Assets/AppFiles/Images/Property/{0}/", prop.ID)));
                }

                CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(providerThumbnail);
                var httpRequest = HttpContext.Current.Request;
                if (file.Contents.Count() > 0)
                {

                    List<PathNameList> list = file.filePathList;

                    List<PathNameList> gallery = list.Where(x => x.Type == "\"gallery\"" && x.Filename != "\"\"").ToList();
                    PathNameList thumbnail = list.Where(x => x.Type == "\"thumbnail\"" && x.Filename != "\"\"").FirstOrDefault();
                    PathNameList video = list.Where(x => x.Type == "\"video\"" && x.Filename != "\"\"").FirstOrDefault();
                    List<PathNameList> floor = list.Where(x => x.Type == "\"plan\"" && x.Filename != "\"\"").ToList();
                    PathNameList inspection = list.Where(x => x.Type == "\"inspection\"" && x.Filename != "\"\"").FirstOrDefault();

                    if (thumbnail != null)
                    {
                        Thumbnailpath = ThumbnailfilePath + "/" + thumbnail.LocalPath;
                        string path = HttpContext.Current.Server.MapPath(Thumbnailpath.Replace("///", "/"));

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

                        prop.Thumbnail = Thumbnailpath.Replace("///", "/");

                        if (_propService.UpdateProperty(ref prop, ref message, false))
                        {
                            SuccessList.Add("Thumbnail added successfully.");
                        }
                    }
                    else
                    {
                        ErrorList.Add("Thumbnail not found");
                    }
                    if (inspection != null)
                    {

                        PropertyInspection data = new PropertyInspection();
                        data.PropertyID = id;
                        var inspections = _PropertyInspectionService.GetInspectionByPropertyID(id).ToList();
                        string absolutePath = HttpContext.Current.Server.MapPath("~");
                        string relativePath = string.Format("/Assets/AppFiles/Documents/Propertys/{0}/", id.ToString().Replace(" ", "_"));
                        var inspeactionfile = httpRequest.Files["inspection"];
                        string fileExtension = System.IO.Path.GetExtension(inspeactionfile.FileName);
                        string FilePath = string.Format("{0}{1}{2}{3}", relativePath, "Document", Guid.NewGuid().ToString(), fileExtension);
                        Directory.CreateDirectory(absolutePath + relativePath);
                        absolutePath += FilePath;
                        inspeactionfile.SaveAs(absolutePath);
                        data.Path = FilePath;
                        if (inspections.Count == 0)
                        {
                            if (_PropertyInspectionService.CreateInspection(ref data, ref message))
                            {
                                SuccessList.Add(string.Format("{0} property inspection uploaded successfully", inspeactionfile.FileName));
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
                    if (video != null)
                    {
                        Videopath = ThumbnailfilePath + "/" + video.LocalPath;
                        prop.Video = Videopath.Replace("///", "/");

                        if (_propService.UpdateProperty(ref prop, ref message, false))
                        {
                            SuccessList.Add("Video added successfully.");
                        }
                    }
                    else
                    {
                        ErrorList.Add("Video not found");
                    }

                    if (gallery != null && gallery.Count() > 0)
                    {
                        foreach (var entry in gallery)
                        {
                            PropertyImage imageModel = new PropertyImage();

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

                            imageModel.PropertyId = id;
                            imageModel.Path = Gallerypath.Replace("///", "/");

                            if (_propImageService.Add(ref imageModel, ref messages))
                                SuccessList.Add(string.Format("{0} image uploaded successfully", entry.UploadedFileName));
                            else
                                ErrorList.Add(string.Format("There is a problem uploading {0} image, Please try again", entry.UploadedFileName));
                        }
                    }
                    else
                        ErrorList.Add("Gallery images not found");

                    if (floor != null && floor.Count() > 0)
                    {
                        foreach (var entry in floor)
                        {
                            PropertyFloorPlan floorModel = new PropertyFloorPlan();

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

                            floorModel.PropertyId = id;
                            floorModel.Path = Gallerypath.Replace("///", "/");

                            if (_floorPlanService.Add(ref floorModel, ref messages))
                                SuccessList.Add(string.Format("{0} floor image uploaded successfully", entry.UploadedFileName));
                            else
                                ErrorList.Add(string.Format("There is a problem uploading {0} floor image, Please try again", entry.UploadedFileName));
                        }
                    }
                    else
                        ErrorList.Add("Floor images not found");

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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    message = "There is a problem uploading file. Please try again"
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
