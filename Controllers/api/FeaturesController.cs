using NowBuySell.Data;
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
    public class FeaturesController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IFeatureService _featureService;
        private readonly ICarFeatureService _carFeatureService;
        private readonly IPropertyFeaturesService _propFeatureService;

        string ImageServer = string.Empty;

        public FeaturesController(IFeatureService featureService, ICarFeatureService carFeatureService, IPropertyFeaturesService propFeatureService)
        {
            this._featureService = featureService;
            this._carFeatureService = carFeatureService;
            _propFeatureService = propFeatureService;

            ImageServer = CustomURL.GetImageServer();
        }

        [HttpGet]
        [Route("{lang}/features")]
        public HttpResponseMessage GetFeatures(string lang)
        {
            try
            {
                var features = _featureService.GetAllCarFeature().Select(i => new
                {
                    id = i.ID,
                    name = lang == "en" ? i.Name : i.NameAR

                });

                return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", features = features });

            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }


        [HttpPost]
        [Route("features/delete")]
        public HttpResponseMessage Delete(CarFeature carPackage)
        {
            try
            {

                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    string status = string.Empty;
                    string message = string.Empty;
                    var features = _carFeatureService.GetCarFeaturebyCarIDAndFeatureID((long)carPackage.CarID, (long)carPackage.FeatureID);
                    if (features != null)
                    {
                        if (_carFeatureService.DeleteCarFeature(features.ID, ref message))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = "Package deleted successfully"
                            });
                        }

                        else
                        {

                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "error",
                                message = "Something went wrong"
                            });
                        }

                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = "Car features not found!"
                    });

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = "Session invalid or expired !"
                    });
                }


            }
            catch (Exception ex)
            {
                //log.Error("Error", ex);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpGet]
        [Route("{module}/filter/{lang}/features")]
        public HttpResponseMessage Get(string module, string lang = "en", string Type = "All")
        {
            if (module.ToLower() == "property")
            {
                var List = _featureService.GetAllPropertyFeature().Select(i => new
                {
                    id = i.ID,
                    name = lang == "ar" ? i.NameAR : i.Name,
                    image = !string.IsNullOrEmpty(i.Image) ? ImageServer + i.Image : "",
                    Icon = !string.IsNullOrEmpty(i.Icon) ? ImageServer + i.Icon : "",
                    count = _propFeatureService.GetCount((int)i.ID, Type)
                });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = true,
                    data = List
                });
            }
            else if (module.ToLower() == "car")
            {
                var List = _featureService.GetAllCarFeature().Select(i => new
                {
                    id = i.ID,
                    name = lang == "ar" ? i.NameAR : i.Name,
                    image = !string.IsNullOrEmpty(i.Image) ? ImageServer + i.Image : "",
                    count = _carFeatureService.GetCount((int)i.ID)
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
