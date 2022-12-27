using NowBuySell.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1/vendor")]
    public class VendorCarTagsController : ApiController
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICarTagService _carTagService;
		private readonly ITagService _tagService;


		public VendorCarTagsController(ICarTagService carTagService, ITagService tagService)
		{
			this._carTagService = carTagService;
			this._tagService = tagService; 
		}

		[HttpGet]
		[Route("{lang}/tag")]
		public HttpResponseMessage GetAllTags(string lang)
		{
			try
			{
				var tag = _tagService.GetTags().Select(i=> new 
				{
					id = i.ID,
					name = lang == "en" ? i.Name : i.NameAr

				});

				return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", tags = tag });

			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}


		[HttpGet]
		[Route("cartags/{id}")]
		public HttpResponseMessage GetCarTags(long id)
		{
			try
			{
				var carTags = _carTagService.GetCarTags(id).Select(i => new
				{
					id = i.TagID,
					value = i.Tag.Name,
					cartagId = i.ID
				});

				return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", cartags = carTags });

			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}
	}
}
