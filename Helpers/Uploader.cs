using NowBuySell.Web.Helpers.Routing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;

namespace NowBuySell.Web.Helpers
{
    public class Uploader
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string UploadImages(HttpFileCollectionBase files, string absolutePath, string relativePath, string prefix, ref List<string> pictures, ref string ErrorMessage, string fileName = "Images", bool compress = false, bool ApplyWatermark = false)
        {
            int i = 0;
            ErrorMessage = string.Empty;
            for (int index = 0; index < files.Count; index++)
            {
                //int index = 0;
                try
                {
                    var file = files[index];
                    if (file != null)
                    {
                        string[] SupportedImageFormat = { ".jpeg", ".png", ".jpg",".webp" };
                        String fileExtension = System.IO.Path.GetExtension(file.FileName);
                        string FilePath;
                        string MainDirectory = string.Empty;
                        if (file.ContentType.Contains("image"))
                        {
                            if (SupportedImageFormat.Contains(fileExtension.ToLower()))
                            {
                                Regex pattern = new Regex("[~`!@#$%^&*()+<>?:,.]");
                                string relPath = pattern.Replace(relativePath, "_");
                                FilePath = string.Format("{0}{1}{2}{3}", relPath, prefix, Guid.NewGuid().ToString(), fileExtension);
                                Directory.CreateDirectory(absolutePath + relativePath);
                                //absolutePath += FilePath;
                                if (compress)
                                {
                                    Image img = Image.FromStream(file.InputStream, false, true);
                                    ImageCompressor.SaveJpeg(absolutePath + FilePath, img, 30);
                                }
                                else if (ApplyWatermark)
                                {
                                    string ImageServer = CustomURL.GetImageServer();
                                    Image img = Image.FromStream(file.InputStream, false, true);
                                    using (Image watermarkImage = Image.FromFile(HttpContext.Current.Server.MapPath(("/Assets/images/NBS-Watermark.png"))))
                                    {
                                        Bitmap bitmap = new Bitmap(watermarkImage);
                                        Image resizeWatermark = resizeImage(bitmap, new Size(img.Size.Width / 6, img.Size.Height / 6));

                                        using (Graphics imageGraphics = Graphics.FromImage(img))
                                        using (TextureBrush watermarkBrush = new TextureBrush(resizeWatermark))
                                        {
                                            //Center
                                            //int x = (img.Width / 2 - resizeWatermark.Width / 2);
                                            //int y = (img.Height - (resizeWatermark.Height + resizeWatermark.Height / 2));

                                            //Left
                                            int x = resizeWatermark.Height / 2;
                                            int y = (img.Height - (resizeWatermark.Height + resizeWatermark.Height / 2));

                                            //Right
                                            //int x = (img.Width - (resizeWatermark.Width + resizeWatermark.Height / 2));
                                            //int y = (img.Height - (resizeWatermark.Height + resizeWatermark.Height / 2));

                                            watermarkBrush.TranslateTransform(x, y);
                                            imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(resizeWatermark.Width, resizeWatermark.Height)));
                                            //imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(0, 0), img.Size));

                                            img.Save(absolutePath + FilePath);
                                            //ImageCompressor.SaveJpeg(absolutePath + FilePath, img, 50);
                                            //ImageCompressor.Compressimagesize(0.5, strm, absolutePath + FilePath);
                                        }
                                        

                                    }
                                }
                                else
                                {
                                    file.SaveAs(absolutePath + FilePath);
                                }
                                pictures.Add(FilePath);
                            }
                            else
                            {
                                ErrorMessage += string.Format("Image {0} format not supported !<br>", index + 1);
                            }
                        }
                        else
                        {
                            ErrorMessage += string.Format("Wrong format for image {0} !<br>", index + 1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error", ex);
                }
            }
            return string.Empty;
        }

        //public static string UploadImagesInWebpFormate(HttpFileCollectionBase files, string absolutePath, string relativePath, string prefix, ref List<string> pictures, ref string ErrorMessage, string fileName = "Images", bool compress = false, bool ApplyWatermark = false)
        //{
        //    int i = 0;
        //    ErrorMessage = string.Empty;
        //    for (int index = 0; index < files.Count; index++)
        //    {
        //        //int index = 0;
        //        try
        //        {
        //            var file = files[index];
        //            if (file != null)
        //            {
        //                string[] SupportedImageFormat = { ".jpeg", ".png", ".jpg", ".webp" };
        //                String fileExtension = System.IO.Path.GetExtension(file.FileName);
        //                string FilePath;
        //                string MainDirectory = string.Empty;
        //                if (file.ContentType.Contains("image"))
        //                {
        //                    if (SupportedImageFormat.Contains(fileExtension.ToLower()))
        //                    {
        //                        Regex pattern = new Regex("[~`!@#$%^&*()+<>?:,.]");
        //                        string relPath = pattern.Replace(relativePath, "_");
        //                        FilePath = string.Format("{0}{1}{2}{3}", relPath, prefix, Guid.NewGuid().ToString(), fileExtension);
        //                        Directory.CreateDirectory(absolutePath + relativePath);
        //                        //absolutePath += FilePath;
        //                        if (compress)
        //                        {
        //                            Image img = Image.FromStream(file.InputStream, false, true);
        //                            ImageCompressor.SaveJpeg(absolutePath + FilePath, img, 30);
        //                        }
        //                        else if (ApplyWatermark)
        //                        {
        //                            string ImageServer = CustomURL.GetImageServer();
        //                            Image img = Image.FromStream(file.InputStream, false, true);
        //                            using (Image watermarkImage = Image.FromFile(HttpContext.Current.Server.MapPath(("/Assets/images/NBS-Watermark.png"))))
        //                            {
        //                                Bitmap bitmap = new Bitmap(watermarkImage);
        //                                Image resizeWatermark = resizeImage(bitmap, new Size(img.Size.Width / 6, img.Size.Height / 6));

        //                                using (Graphics imageGraphics = Graphics.FromImage(img))
        //                                using (TextureBrush watermarkBrush = new TextureBrush(resizeWatermark))
        //                                {
        //                                    //Center
        //                                    //int x = (img.Width / 2 - resizeWatermark.Width / 2);
        //                                    //int y = (img.Height - (resizeWatermark.Height + resizeWatermark.Height / 2));

        //                                    //Left
        //                                    int x = resizeWatermark.Height / 2;
        //                                    int y = (img.Height - (resizeWatermark.Height + resizeWatermark.Height / 2));

        //                                    //Right
        //                                    //int x = (img.Width - (resizeWatermark.Width + resizeWatermark.Height / 2));
        //                                    //int y = (img.Height - (resizeWatermark.Height + resizeWatermark.Height / 2));

        //                                    watermarkBrush.TranslateTransform(x, y);
        //                                    imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(resizeWatermark.Width, resizeWatermark.Height)));
        //                                    //imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(0, 0), img.Size));


        //                                    //img.Save(absolutePath + FilePath);
        //                                    string webPFileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName) + ".jpeg";
        //                                    string webPImagePath = string.Format("{0}{1}{2}{3}", relPath, prefix, Guid.NewGuid().ToString(), webPFileName);
        //                                    ImageCompressor.CompressImage(absolutePath + webPImagePath, "", img);
        //                                    FilePath = webPImagePath;
        //                                }

        //                                var FilePathCompress = absolutePath + FilePath;
        //                                double scaleFactor = 0.5;
        //                                Stream strm = file.InputStream;
        //                                using (var image = System.Drawing.Image.FromStream(strm))
        //                                {
        //                                    var imgnewwidth = (int)(img.Width * scaleFactor);
        //                                    var imgnewheight = (int)(img.Height * scaleFactor);
        //                                    var imgthumnail = new Bitmap(imgnewwidth, imgnewheight);
        //                                    var imgthumbgraph = Graphics.FromImage(imgthumnail);
        //                                    imgthumbgraph.CompositingQuality = CompositingQuality.HighQuality;
        //                                    imgthumbgraph.SmoothingMode = SmoothingMode.HighQuality;
        //                                    imgthumbgraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //                                    var imageRectangle = new Rectangle(0, 0, imgnewwidth, imgnewheight);
        //                                    imgthumbgraph.DrawImage(img, imageRectangle);
        //                                    imgthumnail.Save(FilePathCompress, img.RawFormat);
        //                                }

        //                                using (var ms = new MemoryStream()) 
        //                                {
        //                                    var targetImagePath = FilePath;
        //                                    byte[] image = new byte[file.ContentLength];
        //                                    file.InputStream.Read(image, 0, image.Length);
        //                                    ImageCompressor.CompressImage(targetImagePath, "", image);
        //                                }


        //                            }
        //                        }
        //                        else
        //                        {
        //                            file.SaveAs(absolutePath + FilePath);
        //                        }
        //                        pictures.Add(FilePath);
        //                    }
        //                    else
        //                    {
        //                        ErrorMessage += string.Format("Image {0} format not supported !<br>", index + 1);
        //                    }
        //                }
        //                else
        //                {
        //                    ErrorMessage += string.Format("Wrong format for image {0} !<br>", index + 1);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            log.Error("Error", ex);
        //        }
        //    }
        //    return string.Empty;
        //}
        public static string UploadImage(HttpFileCollectionBase files, string absolutePath, string relativePath, string prefix, ref string ErrorMessage, string fileName = "Image", bool compress = false, bool ApplyWatermark = false)
        {
            ErrorMessage = string.Empty;
            if (files.Count > 0)
            {
                var file = files[fileName];
                if (file != null)
                {
                    string[] SupportedImageFormat = { ".jpeg", ".png", ".jpg" };
                    String fileExtension = System.IO.Path.GetExtension(file.FileName);
                    string FilePath;
                    string MainDirectory = string.Empty;
                    if (file.ContentType.Contains("image") || file.ContentType.Contains("octet-stream"))
                    {
                        if (SupportedImageFormat.Contains(fileExtension.ToLower()))
                        {
                            Regex pattern = new Regex("[~`!@#$%^&*()+<>?:,.]");
                            string relPath = pattern.Replace(relativePath, "_");
                            FilePath = string.Format("{0}{1}{2}{3}", relPath, prefix, Guid.NewGuid().ToString(), fileExtension);
                            Directory.CreateDirectory(absolutePath + relPath);
                            absolutePath += FilePath;

                            if (compress)
                            {
                                Image img = Image.FromStream(file.InputStream, false, true);
                                ImageCompressor.SaveJpeg(absolutePath, img, 30);
                            }
                            else if (ApplyWatermark)
                            {
                                string ImageServer = CustomURL.GetImageServer();
                                Image img = Image.FromStream(file.InputStream, false, true);
                                using (Image watermarkImage = Image.FromFile(HttpContext.Current.Server.MapPath(("/Assets/images/NBS-Watermark.png"))))
                                {
                                    Bitmap bitmap = new Bitmap(watermarkImage);
                                    Image resizeWatermark = resizeImage(bitmap, new Size(img.Size.Width / 4, img.Size.Height / 4));

                                    using (Graphics imageGraphics = Graphics.FromImage(img))
                                    using (TextureBrush watermarkBrush = new TextureBrush(resizeWatermark))
                                    {
                                        int x = resizeWatermark.Height / 2;
                                        int y = (img.Height - (resizeWatermark.Height + resizeWatermark.Height / 2));
                                        watermarkBrush.TranslateTransform(x, y);
                                        imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(resizeWatermark.Width, resizeWatermark.Height)));
                                        //imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(0, 0), img.Size));
                                        img.Save(absolutePath);
                                    }

                                }
                            }
                            else
                            {
                                file.SaveAs(absolutePath);
                            }

                            return FilePath;
                        }
                        else
                        {
                            ErrorMessage = "Image Format Not supported !";
                        }
                    }
                    else
                    {
                        ErrorMessage = "Wrong format for image !";
                    }
                }
                else
                {
                    ErrorMessage = "Please Select an image first !";
                }
            }
            else
            {
                return "/Assets/AppFiles/Images/default.png";
            }
            return string.Empty;
        }

        public static string UploadVideo(HttpFileCollectionBase files, string absolutePath, string relativePath, string prefix, ref string ErrorMessage, string fileName = "Video", bool compress = false)
        {
            ErrorMessage = string.Empty;
            if (files.Count > 0)
            {
                var file = files[fileName];
                if (file != null)
                {
                    string[] SupportedImageFormat = { ".mp4", ".MKV", ".FLV", ".MOV" };
                    String fileExtension = System.IO.Path.GetExtension(file.FileName);
                    string FilePath;
                    string MainDirectory = string.Empty;
                    if (file.ContentType.Contains("video"))
                    {
                        if (SupportedImageFormat.Contains(fileExtension.ToLower()))
                        {
                            FilePath = string.Format("{0}{1}{2}{3}", relativePath, prefix, Guid.NewGuid().ToString(), fileExtension);
                            Directory.CreateDirectory(absolutePath + relativePath);
                            absolutePath += FilePath;

                            if (compress)
                            {
                                Image img = Image.FromStream(file.InputStream);
                                ImageCompressor.SaveJpeg(absolutePath, img, 30);
                            }
                            else
                            {
                                file.SaveAs(absolutePath);
                            }

                            return FilePath;
                        }
                        else
                        {
                            ErrorMessage = "Image Format Not supported !";
                        }
                    }
                    else
                    {
                        ErrorMessage = "Wrong format for image !";
                    }
                }
                else
                {
                    ErrorMessage = "Please Select an image first !";
                }
            }
            else
            {
                return "/Assets/AppFiles/Images/default.png";
            }
            return string.Empty;
        }

        public static string UploadDocs(HttpFileCollectionBase files, string absolutePath, string relativePath, string prefix, ref string ErrorMessage, string fileName = "Image", bool compress = false)
        {
            ErrorMessage = string.Empty;
            if (files.Count > 0)
            {
                var file = files[fileName];
                if (file != null)
                {
                    string[] SupportedImageFormat = { ".docx", ".pdf", ".jpg", ".png", ".jpeg", ".txt" };
                    String fileExtension = System.IO.Path.GetExtension(file.FileName);
                    string FilePath;
                    string MainDirectory = string.Empty;
                    if (file.ContentType.Contains("text/plain"))
                    {
                        if (SupportedImageFormat.Contains(fileExtension.ToLower()))
                        {
                            FilePath = string.Format("{0}{1}{2}{3}", relativePath, prefix, Guid.NewGuid().ToString(), fileExtension);
                            Directory.CreateDirectory(absolutePath + relativePath);
                            absolutePath += FilePath;

                            if (compress)
                            {
                                Image img = Image.FromStream(file.InputStream);
                                ImageCompressor.SaveJpeg(absolutePath, img, 30);
                            }
                            else
                            {
                                file.SaveAs(absolutePath);
                            }

                            return FilePath;
                        }
                        else
                        {
                            ErrorMessage = "Document Format Not supported !";
                            //ErrorMessage = Resources.Resources.CVFormatNotsupported;
                        }
                    }
                    else if (file.ContentType.Contains("application/vnd.openxmlformats-officedocument.wordprocessingml.document") || file.ContentType.Contains("application/pdf"))
                    {
                        if (SupportedImageFormat.Contains(fileExtension.ToLower()))
                        {
                            FilePath = string.Format("{0}{1}{2}{3}", relativePath, prefix, Guid.NewGuid().ToString(), fileExtension);
                            Directory.CreateDirectory(absolutePath + relativePath);
                            absolutePath += FilePath;

                            file.SaveAs(absolutePath);


                            return FilePath;
                        }
                        else
                        {
                            ErrorMessage = "Document Format Not supported !";
                            //ErrorMessage = Resources.Resources.CVFormatNotsupported;
                        }
                    }
                    else
                    {
                        ErrorMessage = "Wrong format for Document !";
                        //ErrorMessage = Resources.Resources.WrongFormatforCV;
                    }
                }
                else
                {

                    ErrorMessage = "Please choose correct file !";
                    //ErrorMessage = Resources.Resources.PleaseChooseCorrectFile; ;
                }
            }
            else
            {
                return "/Assets/AppFiles/Images/default.png";
            }
            return string.Empty;
        }

        public static string UploadImageAndDocs(HttpFileCollectionBase files, string absolutePath, string relativePath, string prefix, ref string ErrorMessage, string fileName = "Image", bool compress = false, bool ApplyWatermark = false)
        {
            ErrorMessage = string.Empty;
            string FilePath = string.Empty;
            if (files.Count > 0)
            {
                var file = files[fileName];
                if (file != null)
                {
                    string[] SupportedImageFormat = { ".jpeg", ".png", ".jpg" };
                    String fileExtension = System.IO.Path.GetExtension(file.FileName);

                    string MainDirectory = string.Empty;
                    if (file.ContentType.Contains("image"))
                    {
                        if (SupportedImageFormat.Contains(fileExtension.ToLower()))
                        {
                            Regex pattern = new Regex("[~`!@#$%^&*()+<>?:,.]");
                            string relPath = pattern.Replace(relativePath, "_");
                            FilePath = string.Format("{0}{1}{2}{3}", relPath, prefix, Guid.NewGuid().ToString(), fileExtension);
                            Directory.CreateDirectory(absolutePath + relPath);
                            absolutePath += FilePath;

                            if (compress)
                            {
                                Image img = Image.FromStream(file.InputStream);
                                ImageCompressor.SaveJpeg(absolutePath, img, 30);
                            }
                            else if (ApplyWatermark)
                            {
                                string ImageServer = CustomURL.GetImageServer();
                                Image img = Image.FromStream(file.InputStream, false, true);
                                using (Image watermarkImage = Image.FromFile(HttpContext.Current.Server.MapPath(("/Assets/images/NBS-Watermark.png"))))
                                {
                                    Bitmap bitmap = new Bitmap(watermarkImage);
                                    Image resizeWatermark = resizeImage(bitmap, new Size(img.Size.Width / 4, img.Size.Height / 4));

                                    using (Graphics imageGraphics = Graphics.FromImage(img))
                                    using (TextureBrush watermarkBrush = new TextureBrush(resizeWatermark))
                                    {
                                        int x = resizeWatermark.Height / 2;
                                        int y = (img.Height - (resizeWatermark.Height + resizeWatermark.Height / 2));
                                        watermarkBrush.TranslateTransform(x, y);
                                        imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(resizeWatermark.Width, resizeWatermark.Height)));
                                        //imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(0, 0), img.Size));
                                        img.Save(absolutePath);
                                    }
                                }
                            }
                            else
                            {
                                file.SaveAs(absolutePath);
                            }

                            return FilePath;
                        }
                        else
                        {
                            FilePath = UploadDocs(files, absolutePath, relativePath, prefix, ref ErrorMessage, fileName = "Image", compress = false);
                        }
                    }
                    else
                    {
                        FilePath = UploadDocs(files, absolutePath, relativePath, prefix, ref ErrorMessage, fileName = "Image", compress = false);
                    }
                }
                else
                {
                    ErrorMessage = "Please Select an image first !";
                }
            }
            else
            {
                return "No File Found";
            }

            return FilePath == "/Assets/AppFiles/Images/default.png" ? null : FilePath;
        }

        public static string Uploadpdfandimg(HttpFileCollectionBase files, string absolutePath, string relativePath, string prefix, ref string ErrorMessage, string fileName = "Image", bool compress = false, bool ApplyWatermark = false)
        {
            ErrorMessage = string.Empty;
            if (files.Count > 0)
            {
                var file = files[fileName];
                if (file != null)
                {
                    string[] SupportedImageFormat = { ".docx", ".pdf", ".jpg", ".jpeg" };
                    String fileExtension = System.IO.Path.GetExtension(file.FileName);
                    string FilePath;
                    string MainDirectory = string.Empty;
                    if (file.ContentType.Contains("image/jpeg") || file.ContentType.Contains("image/jpg"))
                    {
                        if (SupportedImageFormat.Contains(fileExtension.ToLower()))
                        {
                            FilePath = string.Format("{0}{1}{2}{3}", relativePath, prefix, Guid.NewGuid().ToString(), fileExtension);
                            Directory.CreateDirectory(absolutePath + relativePath);
                            absolutePath += FilePath;

                            if (compress)
                            {
                                Image img = Image.FromStream(file.InputStream);
                                ImageCompressor.SaveJpeg(absolutePath, img, 30);
                            }
                            else if (ApplyWatermark)
                            {
                                string ImageServer = CustomURL.GetImageServer();
                                Image img = Image.FromStream(file.InputStream, false, true);
                                using (Image watermarkImage = Image.FromFile(HttpContext.Current.Server.MapPath(("/Assets/images/NBS-Watermark.png"))))
                                using (Graphics imageGraphics = Graphics.FromImage(img))
                                using (TextureBrush watermarkBrush = new TextureBrush(watermarkImage))
                                {
                                    int x = (img.Width / 2 - watermarkImage.Width / 2);
                                    int y = (img.Height / 2 - watermarkImage.Height / 2);
                                    watermarkBrush.TranslateTransform(x, y);
                                    imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(watermarkImage.Width + 1, watermarkImage.Height)));
                                    //imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(0, 0), img.Size));
                                    img.Save(absolutePath);
                                }
                            }
                            else
                            {
                                file.SaveAs(absolutePath);
                            }

                            return FilePath;
                        }
                        else
                        {
                            ErrorMessage = "Document Format Not supported !";
                            //ErrorMessage = Resources.Resources.CVFormatNotsupported;
                        }
                    }
                    else if (file.ContentType.Contains("application/vnd.openxmlformats-officedocument.wordprocessingml.document") || file.ContentType.Contains("application/pdf"))
                    {
                        if (SupportedImageFormat.Contains(fileExtension.ToLower()))
                        {
                            FilePath = string.Format("{0}{1}{2}{3}", relativePath, prefix, Guid.NewGuid().ToString(), fileExtension);
                            Directory.CreateDirectory(absolutePath + relativePath);
                            absolutePath += FilePath;

                            file.SaveAs(absolutePath);


                            return FilePath;
                        }
                        else
                        {
                            ErrorMessage = "Document Format Not supported !";
                            //ErrorMessage = Resources.Resources.CVFormatNotsupported;
                        }
                    }
                    else
                    {
                        ErrorMessage = "Wrong format for Document !";
                        //ErrorMessage = Resources.Resources.WrongFormatforCV;
                    }
                }
                else
                {

                    ErrorMessage = "Please choose correct file !";
                    //ErrorMessage = Resources.Resources.PleaseChooseCorrectFile; ;
                }
            }
            else
            {
                return "/Assets/AppFiles/Images/default.png";
            }
            return string.Empty;
        }

        public static string SaveImage(string sourceImageUrl, string absolutePath, string relativePath, string prefix, ImageFormat format)
        {
            try
            {
                bool ApplyWatermark = true;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons
                string FilePath;
                string MainDirectory = string.Empty;
                FilePath = string.Format("{0}{1}{2}.{3}", relativePath, prefix, Guid.NewGuid().ToString(), format.ToString());
                Directory.CreateDirectory(absolutePath + relativePath);
                absolutePath += FilePath;

                WebClient client = new WebClient();
                Stream stream = client.OpenRead(sourceImageUrl);


                if (ApplyWatermark)
                {
                    string ImageServer = CustomURL.GetImageServer();
                    Image img = Image.FromStream(stream, false, true);
                    using (Image watermarkImage = Image.FromFile(HttpContext.Current.Server.MapPath(("/Assets/images/NBS-Watermark.png"))))
                    {
                        Bitmap bitmap = new Bitmap(watermarkImage);
                        Image resizeWatermark = resizeImage(bitmap, new Size(img.Size.Width / 4, img.Size.Height / 4));

                        using (Graphics imageGraphics = Graphics.FromImage(img))
                        using (TextureBrush watermarkBrush = new TextureBrush(resizeWatermark))
                        {
                            int x = resizeWatermark.Height / 2;
                            int y = (img.Height - (resizeWatermark.Height + resizeWatermark.Height / 2));
                            watermarkBrush.TranslateTransform(x, y);
                            imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(resizeWatermark.Width, resizeWatermark.Height)));
                            //imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(0, 0), img.Size));
                            img.Save(absolutePath);
                        }
                    }
                }
                else
                {
                    Bitmap bitmap; bitmap = new Bitmap(stream);
                    if (bitmap != null)
                    {


                        bitmap.Save(absolutePath);
                        //file.SaveAs(absolutePath);
                    }
                }

                stream.Flush();
                stream.Close();
                client.Dispose();

                return FilePath;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static bool SaveImages(string[] sourceImageUrls, string absolutePath, string relativePath, string prefix, ImageFormat format, ref List<string> pictures)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons

                foreach (var sourceImageUrl in sourceImageUrls)
                {
                    string FilePath;
                    string MainDirectory = string.Empty;
                    FilePath = string.Format("{0}{1}{2}.{3}", relativePath, prefix, Guid.NewGuid().ToString(), format.ToString());
                    Directory.CreateDirectory(absolutePath + relativePath);
                    //absolutePath += FilePath;

                    WebClient client = new WebClient();
                    Stream stream = client.OpenRead(sourceImageUrl);
                    bool ApplyWatermark = true;
                    if (ApplyWatermark)
                    {
                        string ImageServer = CustomURL.GetImageServer();
                        Image img = Image.FromStream(stream, false, true);
                        using (Image watermarkImage = Image.FromFile(HttpContext.Current.Server.MapPath(("/Assets/images/NBS-Watermark.png"))))
                        {
                            Bitmap bitmap = new Bitmap(watermarkImage);
                            Image resizeWatermark = resizeImage(bitmap, new Size(img.Size.Width / 4, img.Size.Height / 4));

                            using (Graphics imageGraphics = Graphics.FromImage(img))
                            using (TextureBrush watermarkBrush = new TextureBrush(resizeWatermark))
                            {
                                int x = resizeWatermark.Height / 2;
                                int y = (img.Height - (resizeWatermark.Height + resizeWatermark.Height / 2));
                                watermarkBrush.TranslateTransform(x, y);
                                imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(resizeWatermark.Width, resizeWatermark.Height)));
                                //imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(0, 0), img.Size));
                                img.Save(absolutePath + FilePath);
                                pictures.Add(FilePath);
                            }
                        }
                    }
                    else
                    {
                        Bitmap bitmap; bitmap = new Bitmap(stream);
                        if (bitmap != null)
                        {


                            bitmap.Save(absolutePath);
                            //file.SaveAs(absolutePath);
                        }
                    }
                    //Bitmap bitmap; bitmap = new Bitmap(stream);

                    //if (bitmap != null)
                    //{
                    //    bitmap.Save(absolutePath + FilePath, format);
                    //    pictures.Add(FilePath);
                    //}

                    stream.Flush();
                    stream.Close();
                    client.Dispose();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool SaveDocx(string[] sourceImageUrls, string absolutePath, string relativePath, string prefix, ImageFormat format, ref List<string> pictures)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons

                foreach (var sourceImageUrl in sourceImageUrls)
                {
                    string FilePath;
                    string MainDirectory = string.Empty;
                    string[] SupportedImageFormat = { ".docx", ".pdf", ".jpg", ".png", ".jpeg", ".txt", ".mp4" };
                    String fileExtension = System.IO.Path.GetExtension(sourceImageUrl);
                    FilePath = string.Format("{0}{1}{2}{3}", relativePath, prefix, Guid.NewGuid().ToString(), fileExtension);
                    Directory.CreateDirectory(absolutePath + relativePath);
                    // absolutePath += FilePath;
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(sourceImageUrl, absolutePath + FilePath);
                    }
                    pictures.Add(FilePath);

                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool SaveVideos(string sourceImageUrls, string absolutePath, string relativePath, string prefix, ref string pictures, string fileName = "Video")
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons

                //foreach (var sourceImageUrl in sourceImageUrls)
                //{
                string FilePath;
                string MainDirectory = string.Empty;
                string[] SupportedImageFormat = { ".mp4", ".MKV", ".FLV", ".MOV" };
                String fileExtension = System.IO.Path.GetExtension(sourceImageUrls);
                FilePath = string.Format("{0}{1}{2}{3}", relativePath, prefix, Guid.NewGuid().ToString(), fileExtension);
                Directory.CreateDirectory(absolutePath + relativePath);
                // absolutePath += FilePath;
                using (var client = new WebClient())
                {
                    client.DownloadFile(sourceImageUrls, absolutePath + FilePath);
                }
                pictures = FilePath;

                //}
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
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

        public static string SaveImage(string sourceImageUrl, string absolutePath, string relativePath, string prefix, ImageFormat format, bool deleteSource = false)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons
                string FilePath;
                string MainDirectory = string.Empty;
                FilePath = string.Format("{0}{1}{2}.{3}", relativePath, prefix, Guid.NewGuid().ToString(), format.ToString());
                Directory.CreateDirectory(absolutePath + relativePath);
                absolutePath += FilePath;

                WebClient client = new WebClient();
                Stream stream = client.OpenRead(sourceImageUrl);


                bool ApplyWatermark = true;
                if (ApplyWatermark)
                {
                    string ImageServer = CustomURL.GetImageServer();
                    Image img = Image.FromStream(stream, false, true);
                    using (Image watermarkImage = Image.FromFile(HttpContext.Current.Server.MapPath(("/Assets/images/NBS-Watermark.png"))))
                    {
                        Bitmap bitmap = new Bitmap(watermarkImage);
                        Image resizeWatermark = resizeImage(bitmap, new Size(img.Size.Width / 4, img.Size.Height / 4));

                        using (Graphics imageGraphics = Graphics.FromImage(img))
                        using (TextureBrush watermarkBrush = new TextureBrush(resizeWatermark))
                        {
                            int x = resizeWatermark.Height / 2;
                            int y = (img.Height - (resizeWatermark.Height + resizeWatermark.Height / 2));
                            watermarkBrush.TranslateTransform(x, y);
                            imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(resizeWatermark.Width, resizeWatermark.Height)));
                            //imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(0, 0), img.Size));
                            img.Save(absolutePath);
                        }
                    }
                }
                else
                {
                    Bitmap bitmap; bitmap = new Bitmap(stream);
                    if (bitmap != null)
                    {


                        bitmap.Save(absolutePath);
                        //file.SaveAs(absolutePath);
                    }
                }

                //Bitmap bitmap; bitmap = new Bitmap(stream);

                //if (bitmap != null)
                //{
                //    bitmap.Save(absolutePath);
                //}

                stream.Flush();
                stream.Close();
                client.Dispose();

                if (deleteSource)
                {
                    var Path = sourceImageUrl.Replace(CustomURL.GetImageServer(), "");
                    if (System.IO.File.Exists(absolutePath + Path))
                    {
                        System.IO.File.Delete(absolutePath + Path);
                    }
                }

                return FilePath;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

    }
}