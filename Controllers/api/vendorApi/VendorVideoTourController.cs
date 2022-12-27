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

namespace NowBuySell.Web.Controllers.api.vendorApi
{
    [Authorize]
    [RoutePrefix("api/v1/vendor")]
    public class VendorVideoTourController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGetInTouchService _getInTouchService;
        private readonly IScheduleMeetingService _meetingService;

        public VendorVideoTourController(IGetInTouchService getInTouchService, IScheduleMeetingService meetingService)
        {
            _getInTouchService = getInTouchService;
            _meetingService = meetingService;

        }

        [HttpGet]
        [Route("VideoTourMotor")]
        public HttpResponseMessage VideoTourMotor(int pg = 1, int take = 20)
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

                    var List = _meetingService.GetListByVendorAndMotors((int)VendorID);

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = "",
                        VideoTourMotorRequests = List.OrderByDescending(x => x.ID).Skip((pg - 1) * take).Take(take).Select(x => new
                        {
                            x.ID,
                            x.VendorID,
                            x.Message,
                            MeetingDate = GetDate(x.MeetingDate),
                            x.Status,
                            CreatedOn = GetDate(x.CreatedOn),
                            customer = x.Customer != null ? new
                            {
                                CustomerID = x.CustomerID,
                                CustomerName = x.Customer.Name,
                                CustomerEmail = x.Customer.Email,
                                CustomerPhone = x.Customer.Contact,
                            } : null,
                            motor = x.Car != null ? new
                            {
                                CarID = x.CarID,
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

        [HttpGet]
        [Route("VideoTourProperty")]
        public HttpResponseMessage VideoTourProperty(int pg = 1, int take = 20)
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

                    var List = _meetingService.GetListByVendorAndProperty((int)VendorID);

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = "",
                        VideoTourPropertyRequests = List.OrderByDescending(x => x.ID).Skip((pg - 1) * take).Take(take).Select(x => new
                        {
                            x.ID,
                            x.VendorID,
                            x.Message,
                            MeetingDate = GetDate(x.MeetingDate),
                            x.Status,
                            CreatedOn = GetDate(x.CreatedOn),
                            customer = x.Customer != null ? new
                            {
                                CustomerID = x.CustomerID,
                                CustomerName = x.Customer.Name,
                                CustomerEmail = x.Customer.Email,
                                CustomerPhone = x.Customer.Contact,
                            } : null,
                            Property = x.Property != null ? new
                            {
                                PropertyID = x.PropertyID,
                                Name = x.Property.Title,
                                AdsReferenceCode = x.Property.AdsReferenceCode,
                                Thumbnail = ImageServer + x.Property.Thumbnail
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


        private string GetDate(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "-";

            string Month = dateTime.Value.ToString("MMM");

            return dateTime.Value.Day + " " + Month + " " + dateTime.Value.Year;
        }


        [HttpPost]
        [Route("VideoTourStatus")]
        public HttpResponseMessage Status(int ID = 0,string Status = "")
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


                    string message = string.Empty;
                    var current = _meetingService.GetByID(ID);
                    current.Status = Status;
                    if (_meetingService.UpdateMeeting(ref current, ref message))
                    {
                        string SuccessMessage = "Status Change successfully ...";
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = SuccessMessage,
                            BookingStatus = current.Status
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "error",
                            message = message
                        });
                    }
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
    }
}