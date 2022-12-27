using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.ViewModels.Api.CustomRequest;
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
    public class CustomerRequestsController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ICarRequestsService _carRequestService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;
        private readonly IPropertyRequestsService _propRequestService;

        public CustomerRequestsController(ICarRequestsService carRequestService, INotificationReceiverService notificationReceiverService,
            INotificationService notificationService, IPropertyRequestsService propRequestService)
        {
            _carRequestService = carRequestService;
            _notificationReceiverService = notificationReceiverService;
            _notificationService = notificationService;
            _propRequestService = propRequestService;
        }

        [HttpPost]
        [Route("Car/Request")]
        public HttpResponseMessage CreateCarRequest(CarRequestViewModel carRequestViewModel, string lang = "en")
        {
            string message = string.Empty;
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims.ToList();
            long customerId;
            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
            {
                if (ModelState.IsValid)
                {
                    CarRequest CarRequest = new CarRequest()
                    {
                        CustomerID = customerId,
                        CategoryID = carRequestViewModel.CategoryID,
                        Title = carRequestViewModel.Title,
                        Description = carRequestViewModel.Description,
                        MakeID = carRequestViewModel.MakeID,
                        ModelID = carRequestViewModel.ModelID,
                        Color = carRequestViewModel.Color,
                        Doors = carRequestViewModel.Doors,
                        Cylinders = carRequestViewModel.Cylinders,

                        Transmission = carRequestViewModel.Transmission,

                        MinYear = carRequestViewModel.MinYear,
                        MaxYear = carRequestViewModel.MaxYear,

                        MinPrice = carRequestViewModel.MinPrice,
                        MaxPrice = carRequestViewModel.MaxPrice,

                        MinKilometers = carRequestViewModel.MinKilometers,
                        MaxKilometers = carRequestViewModel.MaxKilometers,

                        RegionalSpecification = carRequestViewModel.RegionalSpecification,
                        Warranty = carRequestViewModel.Warranty,
                        CreatedOn = Helpers.TimeZone.GetLocalDateTime()
                    };

                    if (_carRequestService.AddCarRequests(CarRequest, ref message))
                    {
                        IEnumerable<vw_CarRequests> VendorList = _carRequestService.GetVendorListFromVMByRequestID(CarRequest.ID);

                        if (VendorList.Count() > 0)
                        {
                            Notification not = new Notification();
                            not.Title = "Car Request";
                            not.Description = "New car added for request ";
                            not.OriginatorID = customerId;
                            not.OriginatorName = "";
                            not.Url = "/Vendor/CarRequests/Index";
                            not.Module = "CarRequest";
                            not.OriginatorType = "Customer";
                            not.RecordID = CarRequest.ID;
                            if (_notificationService.CreateNotification(not, ref message))
                            {
                                foreach (var id in VendorList)
                                {
                                    NotificationReceiver notRec = new NotificationReceiver();
                                    notRec.ReceiverID = id.VendorID;
                                    notRec.ReceiverType = "Vendor";
                                    notRec.NotificationID = not.ID;
                                    _notificationReceiverService.CreateNotificationReceiver(notRec, ref message);
                                }
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = lang == "en" ? "Request Sent successfully" : ArabicDictionary.Translate("Request Sent successfully")
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        status = "error",
                        message = lang == "en" ? "Bad request!" : ArabicDictionary.Translate("Bad request!", false),
                        description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "failed",
                    message = lang == "en" ? message : ArabicDictionary.Translate(message)
                });
            }

            return Request.CreateResponse(HttpStatusCode.Unauthorized, new
            {
                status = "failed",
                message = lang == "en" ? "Authorization failed for current request" : ArabicDictionary.Translate("Authorization failed for current request", false)
            });
        }

        [HttpPost]
        [Route("properties/Request")]
        public HttpResponseMessage CreatePropertyRequest(PropertyRequestViewModel propertyRequestViewModel, string lang = "en")
        {
            try
            {
                string message = string.Empty;
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    if (ModelState.IsValid)
                    {
                        PropertyRequest PropertyRequest = new PropertyRequest()
                        {
                            CustomerID = customerId,
                            Title = propertyRequestViewModel.Title,
                            Description = propertyRequestViewModel.Description,
                            CategoryID = propertyRequestViewModel.CategoryID,
                            ForSale = propertyRequestViewModel.PropertyType == "Sale" ? true : false,
                            MinPrice = propertyRequestViewModel.MinPrice,
                            MaxPrice = propertyRequestViewModel.MaxPrice,
                            NoOfRooms = propertyRequestViewModel.NoOfRooms,
                            NoOfBathRooms = propertyRequestViewModel.NoOfBathRooms,
                            Size = propertyRequestViewModel.Size,
                            Latitude = propertyRequestViewModel.Latitude,
                            Longitude = propertyRequestViewModel.Longitude,
                            Address = propertyRequestViewModel.Address,
                        };

                        if (_propRequestService.AddPropertyRequests(PropertyRequest, ref message))
                        {
                            IEnumerable<vw_PropertyRequests> VendorList = _propRequestService.GetVendorListFromVMByRequestID(PropertyRequest.ID);

                            Notification not = new Notification();
                            not.Title = "Property Request";
                            not.Description = "New property added for request ";
                            not.OriginatorID = customerId;
                            not.OriginatorName = "";
                            not.Url = "/Vendor/PropertyRequests/Index";
                            not.Module = "PropertyRequest";
                            not.OriginatorType = "Customer";
                            not.RecordID = PropertyRequest.ID;
                            if (_notificationService.CreateNotification(not, ref message))
                            {
                                foreach (var id in VendorList)
                                {
                                    NotificationReceiver notRec = new NotificationReceiver();
                                    notRec.ReceiverID = id.VendorId;
                                    notRec.ReceiverType = "Vendor";
                                    notRec.NotificationID = not.ID;
                                    _notificationReceiverService.CreateNotificationReceiver(notRec, ref message);
                                }
                            }

                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "success",
                                message = lang == "en" ? "Request Sent successfully" : ArabicDictionary.Translate("Request Sent successfully")
                            });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                status = "failure",
                                message = lang == "en" ? message : ArabicDictionary.Translate(message)
                            });
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new
                        {
                            status = "error",
                            message = lang == "en" ? "Bad request!" : ArabicDictionary.Translate("Bad request!", false),
                            description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        status = "error",
                        message = lang == "en" ? "Session invalid or expired" : ArabicDictionary.Translate("Session invalid or expired", false)
                    });

                }
            }
            catch (Exception ex)
            {

                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false)
                });
            }
        }

        [HttpGet]
        [Route("customer/Requests")]
        public HttpResponseMessage GetCustomerRequestListing()
        {
            string message = string.Empty;
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claims = identity.Claims.ToList();
            long customerId;
            if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
            {
                var PropertyListing = _propRequestService.GetRequestListByCustomer(customerId).OrderByDescending(x => x.ID);
                var CarListing = _carRequestService.GetRequestListByCustomer(customerId).OrderByDescending(x => x.ID);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",
                    PropertiesForSale = PropertyListing.Where(x => x.ForSale).Select(i => new
                    {
                        i.ID,
                        i.Title,
                        i.Description,
                        i.IsFulFilled,
                        creationDate = GetDate(i.CreatedOn)
                    }),

                    PropertiesForRent = PropertyListing.Where(x => !x.ForSale).Select(i => new
                    {
                        i.ID,
                        i.Title,
                        i.Description,
                        i.IsFulFilled,
                        creationDate = GetDate(i.CreatedOn)
                    }),

                    Motors = CarListing.Select(i => new
                    {
                        i.ID,
                        i.Title,
                        i.Description,
                        i.IsFulFilled,
                        creationDate = GetDate(i.CreatedOn)
                    })
                });
            }
            return Request.CreateResponse(HttpStatusCode.Unauthorized, new
            {
                status = "failed",
                message = "Authorization failed for current request"
            });
        }

        [HttpPut]
        [Route("cars/Request/{RequestID}/close")]
        public HttpResponseMessage MarkedCarFulfilled(int RequestID)
        {
            try
            {
                string message = string.Empty;
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    var request = _carRequestService.GetRequestByID(RequestID);

                    if (request != null)
                    {
                        if (request.IsFulFilled)
                            return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", message = "Request already marked as fulfilled" });
                        else
                            request.IsFulFilled = true;

                        if (_carRequestService.UpdateCarRequests(ref request, ref message))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", message = "Request marked as fulfilled" });
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", message = "Request not found!" });
                }

                return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                {
                    status = "error",
                    message = "Session invalid or expired !"
                });
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

        [HttpPut]
        [Route("properties/Request/{RequestID}/close")]
        public HttpResponseMessage MarkedPropertyFulfilled(int RequestID)
        {
            try
            {
                string message = string.Empty;
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    var request = _propRequestService.GetRequestByID(RequestID);

                    if (request != null)
                    {
                        if (request.IsFulFilled)
                            return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", message = "Request already marked as fulfilled" });
                        else
                            request.IsFulFilled = true;

                        if (_propRequestService.UpdatePropertyRequests(ref request, ref message))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", message = "Request marked as fulfilled" });
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", message = "Request not found!" });
                }

                return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                {
                    status = "error",
                    message = "Session invalid or expired !"
                });
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
            }
        }

        [HttpGet]
        [Route("properties/Request/{RequestID}")]
        public HttpResponseMessage GetPropRequestByID(int RequestID)
        {
            try
            {
                string message = string.Empty;
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    var request = _propRequestService.GetRequestByID(RequestID);

                    if (request == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, new
                        {
                            status = "error",
                            message = "No record found"
                        });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        data = new
                        {
                            request.ID,
                            request.Title,
                            request.Description,
                            Category = request.Category != null ? new
                            {
                                id = request.Category.ID,
                                name = request.Category.CategoryName
                            } : null,
                            status = request.ForSale ? "Sale" : "Rent",
                            MinPrice = request.MinPrice.HasValue ? request.MinPrice.Value : 0,
                            MaxPrice = request.MaxPrice.HasValue ? request.MaxPrice.Value : 0,
                            request.Size,
                            NoOfRooms = request.NoOfRooms.HasValue ? request.NoOfRooms.Value : 0,
                            NoOfBathRooms = request.NoOfBathRooms.HasValue ? request.NoOfBathRooms.Value : 0,
                            Latitude = request.Latitude.HasValue ? request.Latitude.Value : 0,
                            Longitude = request.Longitude.HasValue ? request.Longitude.Value : 0,
                            request.Address,
                            request.IsFulFilled,
                            creationDate = GetDate(request.CreatedOn)
                        }
                    });
                }
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                {
                    status = "failed",
                    message = "Authorization failed for current request"
                });
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        [HttpGet]
        [Route("cars/Request/{RequestID}")]
        public HttpResponseMessage GetCarRequestByID(int RequestID)
        {
            try
            {
                string message = string.Empty;
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    var request = _carRequestService.GetRequestByID(RequestID);

                    if (request == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, new
                        {
                            status = "error",
                            message = "No record found"
                        });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        data = new
                        {
                            request.ID,
                            request.Title,
                            request.Description,
                            Category = request.Category != null ? new
                            {
                                id = request.Category.ID,
                                name = request.Category.CategoryName
                            } : null,
                            Make = request.CarMake != null ? new
                            {
                                id = request.CarMake.ID,
                                name = request.CarMake.Name
                            } : null,
                            Model = request.CarModel != null ? new
                            {
                                id = request.CarModel.ID,
                                name = request.CarModel.Name
                            } : null,
                            request.Color,
                            request.Doors,
                            request.Cylinders,
                            request.Transmission,

                            MinYear = request.MinYear,
                            MaxYear = request.MaxYear,

                            MinPrice = request.MinPrice.HasValue ? request.MinPrice.Value : 0,
                            MaxPrice = request.MaxPrice.HasValue ? request.MaxPrice.Value : 0,

                            MinKilometers = request.MinKilometers.HasValue ? request.MinKilometers.Value : 0,
                            MaxKilometers = request.MaxKilometers.HasValue ? request.MaxKilometers.Value : 0,

                            request.RegionalSpecification,
                            Warranty = request.Warranty.HasValue ? request.Warranty.Value : false,

                            request.IsFulFilled,
                            creationDate = GetDate(request.CreatedOn)
                        }
                    });
                }
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                {
                    status = "failed",
                    message = "Authorization failed for current request"
                });
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                //Logs.Write(ex);
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
    }
}