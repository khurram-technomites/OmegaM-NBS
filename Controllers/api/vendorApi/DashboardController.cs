using NowBuySell.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api.vendorApi
{

    [Authorize]
    [RoutePrefix("api/v1/vendor")]
    public class DashboardController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISPService _spService;

        private readonly IPropertyService _propertyService;
        private readonly ICarService _carService;
        private readonly ICarRequestsService _carRequestsService;
        private readonly IPropertyRequestsService _propertyRequestsService;

        public DashboardController(ISPService spService
            , IPropertyService propertyService
            , ICarService carService
            , ICarRequestsService carRequestsService
            , IPropertyRequestsService propertyRequestsService)
        {
            this._spService = spService;
            this._propertyService = propertyService;

            this._carService = carService;
            this._carRequestsService = carRequestsService;
            this._propertyRequestsService = propertyRequestsService;
        }

        [HttpGet]
        [Route("dashboard/stats")]
        public HttpResponseMessage Stats()
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long userId;
                long VendorID = 0;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out userId))
                {
                    string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
                    if (ReceiverType == "Vendor")
                    {
                        VendorID = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
                    }

                    var carRequests = _carRequestsService.GetRequestListByVendor(VendorID);
                    var propertyRequests = _propertyRequestsService.GetRequestListByVendor(VendorID);

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        stats = new
                        {
                            NoOfMotors = _carService.GetApprovedListByVendor(VendorID).Count(),
                            NoOfProperties = _propertyService.GetApprovedListByVendor(VendorID).Count(),
                            TotalRequest = carRequests.Count() + propertyRequests.Count(),
                        }
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
    }
}
