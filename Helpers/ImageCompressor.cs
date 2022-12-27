using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace NowBuySell.Web.Helpers
{
	public class ImageCompressor
	{
		public static void SaveJpeg(string path, Image img, int quality)
		{
			if (quality < 0 || quality > 100)
				throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");

			// Encoder parameter for image quality 
			EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);
			// JPEG image codec 
			ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
			EncoderParameters encoderParams = new EncoderParameters(1);
			encoderParams.Param[0] = qualityParam;
			img.Save(path, jpegCodec, encoderParams);
		}
		//public static void CompressImage(string targetPath, string filename, Byte[] byteArrayIn)
		//{
  //          try 
		//	{
		//		System.Drawing.ImageConverter converter = new System.Drawing.ImageConverter();
		//		using (MemoryStream memstr = new MemoryStream(byteArrayIn)) 
		//		{
		//			using (var image = Image.FromStream(memstr)) 
		//			{
		//				float maxHeight = 900.0f;
		//				float maxWidth = 900.0f;
		//				int newWidth;
		//				int newHight;
		//				string extention;
		//				Bitmap originalBMP = new Bitmap(memstr);
		//				int originalWidth = originalBMP.Width;
		//				int originalHeight = originalBMP.Height;

		//				if (originalWidth > maxWidth || originalHeight > maxHeight)
		//				{
		//					float ratioX = (float)maxWidth / (float)originalWidth;
		//					float ratioY = (float)maxHeight / (float)originalHeight;
		//					float ration = Math.Min(ratioX, ratioY);
		//					newWidth = (int)(originalWidth * ration);
		//					newHight = (int)(originalHeight * ration);
		//				}
		//				else 
		//				{
		//					newWidth = (int)originalWidth;
		//					newHight = (int)originalHeight;
		//				}
		//				Bitmap bitMAP1 = new Bitmap(originalBMP, newWidth, newHight);
		//				Graphics imgGraph = Graphics.FromImage(bitMAP1);
		//				extention = Path.GetExtension(targetPath);
  //                      if (extention.ToLower() == ".png" || extention.ToLower() == ".gif" || extention.ToLower() == ".jpeg") 
		//				{
		//					imgGraph.SmoothingMode = SmoothingMode.AntiAlias;
		//					imgGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
		//					imgGraph.DrawImage(originalBMP, 0, 0, newWidth, newHight);

		//				}
		//				else if (extention.ToLower() == ".jpg")
		//				{
		//				imgGraph.SmoothingMode = SmoothingMode.AntiAlias;
		//				imgGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
		//				imgGraph.DrawImage(originalBMP, 0, 0, newWidth, newHight);
		//				ImageCodecInfo jpgEncoder = GetEncoderInfo("image/jpeg");
		//				System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
		//				EncoderParameters myEncoderParameters = new EncoderParameters(1);
		//				EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
		//				myEncoderParameters.Param[0] = myEncoderParameter;
		//				bitMAP1.Save(targetPath, jpgEncoder, myEncoderParameters);
		//				bitMAP1.Dispose();
		//				imgGraph.Dispose();
		//				}
		//			}
		//		}
  //          }
  //          catch 
		//	{
		//	}
		//}


		//public static void Compressimagesize(double scaleFactor, Stream sourcePath, string targetPath)
		//{
		//	using (var image = System.Drawing.Image.FromStream(sourcePath))
		//	{
		//		var imgnewwidth = (int)(image.Width * scaleFactor);
		//		var imgnewheight = (int)(image.Height * scaleFactor);
		//		var imgthumnail = new Bitmap(imgnewwidth, imgnewheight);
		//		var imgthumbgraph = Graphics.FromImage(imgthumnail);
		//		imgthumbgraph.CompositingQuality = CompositingQuality.HighQuality;
		//		imgthumbgraph.SmoothingMode = SmoothingMode.HighQuality;
		//		imgthumbgraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
		//		var imageRectangle = new Rectangle(0, 0, imgnewwidth, imgnewheight);
		//		imgthumbgraph.DrawImage(image, imageRectangle);
		//		imgthumnail.Save(targetPath, image.RawFormat);
		//	}
		//}
		/// <summary> 
		/// Returns the image codec with the given mime type 
		/// </summary> 
		private static ImageCodecInfo GetEncoderInfo(string mimeType)
		{
			// Get image codecs for all image formats 
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

			// Find the correct image codec 
			for (int i = 0; i < codecs.Length; i++)
				if (codecs[i].MimeType == mimeType)
					return codecs[i];

			return null;
		}
	}
}