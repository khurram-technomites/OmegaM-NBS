using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NowBuySell.Service;
using NowBuySell.Web.ViewModels.Api.Navigation;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class NavigationController : ApiController
    {
        private readonly ICategoryService _categoryService;

        public NavigationController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("navigation/{lang}")]
        public HttpResponseMessage PropertyNaviigation(string lang = "en")
        {
            var RentList = _categoryService.GetRentCategoriesForNavigation().Select(x=> new {ID = x.ID, Slug = x.Slug, Name = lang == "ar" ? x.CategoryNameAr : x.CategoryName });
            var SaleList = _categoryService.GetSaleCategoriesForNavigation().Select(x=> new {ID = x.ID, Slug = x.Slug, Name = lang == "ar" ? x.CategoryNameAr : x.CategoryName });
            var CarList = _categoryService.GetMotorCategoriesForNavigation().Select(x => new { ID = x.ID, Slug = x.Slug, Name = lang == "ar" ? x.CategoryNameAr : x.CategoryName });

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                status = true,
                PropertyRentList = RentList,
                PropertySaleList = SaleList,
                MotorList = CarList
            });
        }        
    }
}
