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
using NowBuySell.Web.ViewModels.Api.Vendor;

namespace NowBuySell.Web.Controllers.api.vendorApi
{
    [Authorize]
    [RoutePrefix("api/v1/vendor")]
    public class VendorGetInTouchController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGetInTouchService _getInTouchService;
        private readonly IGetInTouchRemarkService _IGetInTouchRemarkService;

        public VendorGetInTouchController(IGetInTouchService getInTouchService, IGetInTouchRemarkService iGetInTouchRemarkService)
        {
            _getInTouchService = getInTouchService;
            _IGetInTouchRemarkService = iGetInTouchRemarkService;
        }

        [HttpGet]
        [Route("GetInTouch")]
        public HttpResponseMessage GetInTouch(int pg = 1, int take = 20)
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long userId;
                long VendorID = 0;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out userId))
                {
                    string ImageServer = CustomURL.GetImageServer();

                    string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
                    if (ReceiverType == "Vendor")
                    {
                        VendorID = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
                    }

                    var List = _getInTouchService.GetListByVendor((int)VendorID);

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = "",
                        GetInTouchRequests = List.OrderByDescending(x => x.ID).Skip((pg - 1) * take).Take(take).Select(x => new
                        {
                            x.ID,
                            x.Name,
                            x.PhoneNo,
                            x.Email,
                            x.Comments,
                            x.MarkRead,
                            x.VendorID,
                            CreatedOn = GetDate(x.CreatedOn),
                            property = x.Property != null ? new
                            {
                                ID = x.PropertyID,
                                Name = x.Property.Title,
                                x.Property.AdsReferenceCode,
                                Thumbnail = ImageServer + x.Property.Thumbnail
                            } : null,
                            motor = x.Car != null ? new
                            {
                                ID = x.CarID,
                                Name = x.Car.Name,
                                x.Car.AdsReferenceCode,
                                Thumbnail = ImageServer + x.Car.Thumbnail
                            } : null,
                        }),

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
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpPut]
        [Route("GetInTouch/{Id}/MarkRead")]
        public HttpResponseMessage MarkRead(int Id)
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

                    var entity = _getInTouchService.GetById(Id);

                    if (entity.MarkRead.HasValue)
                    {
                        if (entity.MarkRead.Value)
                            entity.MarkRead = false;
                        else
                            entity.MarkRead = true;
                    }
                    else
                    {
                        entity.MarkRead = true;
                    }

                    if (_getInTouchService.UpdateMarkRead(entity))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = "Record mark as read successfully!"
                        });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "failure",
                        message = "Oops! Something went wrong. Please try later."
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = "Session invalid or expired!"
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }
        [HttpGet]
        [Route("GetInTouch/{Id}/Remarks")]
        public HttpResponseMessage Remarks(long Id)
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long userId;
                long VendorID = 0;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out userId))
                {
                    var remark = _IGetInTouchRemarkService.GetByGetInTouchId(Id);
                    if(remark != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            remarks = remark.OrderByDescending(x=>x.ID).Select(i=> new
                            {
                                i.ID,
                                //i.GetInTouchID,
                                i.VendorUser.Name,
                                CreatedOn = GetDate(i.CreatedOn),
                                i.Remarks
                            })
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "failure",
                            message = "Oops! Something went wrong. Please try later."
                        });
                    }  
             
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = "Session invalid or expired!"
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
          
        }
        [HttpPost]
        [Route("GetInTouch/{Id}/Remarks")]
        public HttpResponseMessage InsertRemarks(long Id,GetInTouchRemarkViewModel getInTouchRemark)
        {
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims;
            long userId;
            long VendorID = 0;
            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out userId))
            {
                if (getInTouchRemark.Remarks != null)
                {
                    string message = string.Empty;
                    GetInTouchRemark CreateRemark = new GetInTouchRemark();
                    CreateRemark.Remarks = getInTouchRemark.Remarks;
                    CreateRemark.GetInTouchID = (int?)Id;
                    CreateRemark.VendorUserID = Convert.ToInt32(userId);
                    CreateRemark.CreatedOn = Helpers.TimeZone.GetLocalDateTime();

                    if (_IGetInTouchRemarkService.AddRemarks(ref CreateRemark, ref message))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = "Property added successfully",
                            remarks = CreateRemark,
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "failure",
                        message = "Oops! Something went wrong. Please try later."
                    });
                }
            }
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                {
                    status = "error",
                    message = "Session invalid or expired!"
                });
            }
        }
         private string GetDate(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "-";

            string Month = dateTime.Value.ToString("MMM");

            return dateTime.Value.Day + " " + Month + " " + dateTime.Value.Year;
        }
    }
}