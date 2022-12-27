using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Service.Helpers;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.Api.Account;
using NowBuySell.Web.ViewModels.Api.Vendor;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class VendorsController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IVendorService _vendorService;
        private readonly IVendorUserService _vendorUserService;
        private readonly INumberRangeService _numberRangeService;
        private readonly IVendorUserRoleService _vendorUserRoleService;
        private readonly IMail _email;

        public VendorsController(IVendorService vendorService, IVendorUserService vendorUserService, INumberRangeService numberRangeService,
            IVendorUserRoleService vendorUserRoleService, IMail email)
        {
            this._vendorService = vendorService;
            _vendorUserService = vendorUserService;
            _numberRangeService = numberRangeService;

            _vendorUserRoleService = vendorUserRoleService;
            _email = email;
        }

        [HttpGet]
        [Route("{lang}/vendors/{vendorId}")]
        public HttpResponseMessage GetVendorDetails(long vendorId, string lang = "en")
        {
            try
            {
                var vendor = _vendorService.GetVendor(vendorId);

                if (vendor != null)
                {
                    string ImageServer = CustomURL.GetImageServer();

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        vendor = new
                        {
                            id = vendor.ID,
                            code = vendor.VendorCode,
                            name = lang == "en" ? vendor.Name : vendor.NameAr,
                            logo = !string.IsNullOrEmpty(vendor.Logo) ? ImageServer + vendor.Logo : string.Empty,
                            coverImage = !string.IsNullOrEmpty(vendor.CoverImage) ? ImageServer + vendor.CoverImage : string.Empty,
                            about = vendor.About,
                            whatsapp = vendor.Whatsapp,
                            facebook = vendor.Facebook,
                            instagram = vendor.Instagram,
                            snapchat = vendor.Snapchat,
                            linkedin = vendor.LinkedIn,
                            twitter = vendor.Twitter,
                            youtube = vendor.Youtube,
                            vendor.VendorPackage.hasMotorModule,
                            vendor.VendorPackage.hasPropertyModule
                        }
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { status = "error", message = "Invalid vendor id !" });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

        [HttpPost]
        [Route("vendors")]
        public HttpResponseMessage GetVendors(VendorFilterViewModel filterViewModel)
        {
            //string status = string.Empty;
            try
            {
                
                var vendors = _vendorService.GetVendorsByName(filterViewModel.Search,filterViewModel.VendorType);
                string ImageServer = CustomURL.GetImageServer();
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",

                    Vendors = filterViewModel.PageSize==null || filterViewModel.PageSize == 0 ?
                    vendors.Select(i => new
                    {

                        id = i.ID,
                        Name = i.Name,
                        Logo = ImageServer + i.Logo

                    }).OrderByDescending(i => i.id).ToList():
                    vendors.Select(i => new
                    {

                        id = i.ID,
                        Name = i.Name,
                        Logo = ImageServer + i.Logo

                    }).OrderByDescending(i => i.id).ToList().Skip((int)filterViewModel.PageSize * (filterViewModel.pgno - 1)).Take((int)filterViewModel.PageSize),
                });


            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

    }
}
