using NowBuySell.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{

	[RoutePrefix("api/v1")]
	public class FiltersController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICategoryService _categoryService;

		public FiltersController(ICategoryService categoryService)
		{
			this._categoryService = categoryService;
		}

		[HttpGet]
		[Route("{lang}/filters")]
		public HttpResponseMessage GetFilters(string lang = "en")
		{
			try
			{
				var filters = _categoryService.GetFilters(lang);
				var categories = filters.Select(i => i.CategoryID).Distinct().ToList();
				return Request.CreateResponse(HttpStatusCode.OK, new
				{
					status = "success",
					filters = new
					{
						categories = categories.Select(i => new
						{
							id = i,
							attributes = filters.Where(j => j.CategoryID == i && j.AttributeValues != null).Select(k => new
							{
								id = k.AttributeID,
								name = k.AttributeName,
								values = k.AttributeValues != null ? k.AttributeValues.Split(',') : null
							}),
							brands = filters.Where(j => j.CategoryID == i && j.Brands != null).FirstOrDefault() != null ? filters.Where(j => j.CategoryID == i && j.Brands != null).FirstOrDefault().Brands.Split(',').ToList() : null
						})
					}
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
