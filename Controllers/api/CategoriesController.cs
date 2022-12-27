using NowBuySell.Service;
using NowBuySell.Web.Helpers.Routing;
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
	[RoutePrefix("api/v1")]
	public class CategoriesController : ApiController
	{
        string ImageServer = string.Empty;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICategoryService _categoryService;

		public CategoriesController(ICategoryService categoryService)
		{
			this._categoryService = categoryService;
            ImageServer = CustomURL.GetImageServer();
        }

		[HttpGet]
		[Route("{lang}/categories")]
		public HttpResponseMessage GetCategories(string lang = "en", int pg = 1)
		{
			try
			{
				/*string ImageServer = CustomURL.GetImageServer();*/
				var Categories = _categoryService.GetCategories(ImageServer, lang).Select(i => new
				{
                    id = i.ID,
                    image = i.Cover,
					name = i.Name,
                    slug = i.Slug,
					carCount = i.CarCount != null ? i.CarCount : 0 
					
				});
				return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", categories = Categories });
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}

        [HttpGet]
        [Route("{module}/filter/{lang}/categories")]
        public HttpResponseMessage Get(string module, string lang = "en", string Type = "all")
        {
            
            if (module.ToLower() == "property")
            {
                var List = _categoryService.GetPropertyCategories(Type).Select(i => new
                {
                    id = i.ID,
                    name = lang == "ar" ? i.CategoryNameAr : i.CategoryName,
                    //nameAr = i.CategoryNameAr,
                    description = lang == "ar" ? i.DescriptionAR : i.Description,
                    //descriptionAr = i.DescriptionAR,
                    image = ImageServer + "/" + i.Image,
                    icon = ImageServer + "/" + i.Icon,
                    slug = i.Slug,
                    PropertyType = i.PropertyType,
                    count = _categoryService.GetCount((int)i.ID, Type)
                });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = true,
                    data = List
                });
            }
            else if (module.ToLower() == "car")
            {

                var List = _categoryService.GetCarCategories().Select(i => new
                {
                    id = i.ID,
                    name = lang == "ar" ? i.CategoryNameAr : i.CategoryName,
                    description = lang == "ar" ?  i.DescriptionAR : i.Description,
                    image = ImageServer + "/" + i.Image,
                    icon = ImageServer + "/" +  i.Icon ,
                    slug = i.Slug,
                    count = _categoryService.GetCarCount((int)i.ID)

                });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = true,
                    data = List
                });
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, new
            {
                status = false,
                message = "Unspecified module"
            });
        }
    }
}
