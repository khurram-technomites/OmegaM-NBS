using NowBuySell.Service;
using NowBuySell.Web.Helpers.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
	[RoutePrefix("api/v1")]
	public class VendorRatingController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICarRatingService _carRatingService;

		public VendorRatingController(ICarRatingService carRatingService, ICarRatingImageService carRatingImageService)
		{
			this._carRatingService = carRatingService;
		}

		[HttpGet]
		[Route("vendors/{vendorId}/ratings")]
		public HttpResponseMessage GetVendorRatings(long vendorId, int pg = 1, string lang = "en")
		{
			try
			{
				string ImageServer = CustomURL.GetImageServer();
				var ratings = _carRatingService.GetVendorRatings(vendorId, pg, ImageServer);
				return Request.CreateResponse(HttpStatusCode.OK, new
				{
					status = "success",
					overallrating = ratings.Count() > 0 ? ratings.FirstOrDefault().OverAllRating : 0,
					ratings = ratings.Select(i => new
					{
						id = i.ID,
						date = i.Date,
						customer = i.Name,
						rating = i.Rating,
						remarks = i.Remarks,
						images = i.Images.Split(',')
					})
				});
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}
	}
}
