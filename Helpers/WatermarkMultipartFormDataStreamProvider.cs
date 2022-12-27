using NowBuySell.Web.Helpers.Routing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace NowBuySell.Web.Helpers
{
    public class WatermarkMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public string filePath { set; get; }
        public List<PathNameList> filePathList { set; get; } = new List<PathNameList>();
        public WatermarkMultipartFormDataStreamProvider(string path) : base(path) { }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            //if (headers.ContentDisposition.FileName.Contains("."))
            //{

            var fileName = string.Format("/{0}-{1}", Guid.NewGuid().ToString(), headers.ContentDisposition.FileName.Replace("\"", string.Empty));
            filePath = fileName;

            filePathList.Add(new PathNameList
            {
                Filename = string.IsNullOrEmpty(headers.ContentDisposition.FileName) ? "" : headers.ContentDisposition.FileName,
                Type = headers.ContentDisposition.Name,
                LocalPath = fileName,
                UploadedFileName = headers.ContentDisposition.FileName.Replace("\"", string.Empty)
            });
            return fileName;
        }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            //byte[] bytes = parent.ReadAsStreamAsync().Result;
            //using (Stream stream = new MemoryStream(parent.ReadAsByteArrayAsync().Result))
            //{
            //    string ImageServer = CustomURL.GetImageServer();
            //    Image img = Image.FromStream(stream, false, true);
            //    using (Image watermarkImage = Image.FromFile(HttpContext.Current.Server.MapPath(("/Assets/images/Group 2986.png"))))
            //    using (Graphics imageGraphics = Graphics.FromImage(img))
            //    using (TextureBrush watermarkBrush = new TextureBrush(watermarkImage))
            //    {
            //        int x = (img.Width / 2 - watermarkImage.Width / 2);
            //        int y = (img.Height / 2 - watermarkImage.Height / 2);
            //        watermarkBrush.TranslateTransform(x, y);
            //        imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(watermarkImage.Width + 1, watermarkImage.Height)));
            //        //imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(0, 0), img.Size));
            //        //img.Save(absolutePath);
            //    }

            //    Stream ms = new MemoryStream();
            //    img.Save(ms, ImageFormat.Png);
            //    return ms;
            //}           
            var stream = new MemoryStream(parent.ReadAsByteArrayAsync().Result);
            Image img = Image.FromStream(stream, false, true);
            Image watermarkImage = Image.FromFile(HttpContext.Current.Server.MapPath(("/Assets/images/NBS-Watermark.png")));
            Graphics imageGraphics = Graphics.FromImage(img);
            using (TextureBrush watermarkBrush = new TextureBrush(watermarkImage))
            {
                //x and y co-ordinates of watermark.
                int x = (img.Width / 2 - watermarkImage.Width / 2);
                int y = (img.Height / 2 - watermarkImage.Height / 2);

                watermarkBrush.TranslateTransform(x, y);

                //drawing watermark on image
                imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(watermarkImage.Width, watermarkImage.Height)));

                //imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(0, 0), img.Size));
                //img.Save(path);

                Stream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Png);
                return ms;
                //var image = new Bitmap(img);
                //image.Save(path);
            }
        }
    }

}