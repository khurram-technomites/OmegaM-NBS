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

namespace NowBuySell.Web.Controllers.api.vendorApi
{
    [Authorize]
    [RoutePrefix("api/v1/vendor")]
    public class CustomerRequestController : ApiController
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICarRequestsService _carRequestsService;
        private readonly IPropertyRequestsService _propertyRequestsService;
        private readonly IVendorRequestsService _vendorRequestService;

        public CustomerRequestController(ICarRequestsService carRequestsService
            , IPropertyRequestsService propertyRequestsService, IVendorRequestsService vendorRequestService)
        {
            this._carRequestsService = carRequestsService;
            this._propertyRequestsService = propertyRequestsService;

            _vendorRequestService = vendorRequestService;
        }

        [HttpGet]
        [Route("customerrequest")]
        public HttpResponseMessage GetNewCustomerRequest(int pg = 1, int take = 20)
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

                    var ImageServer = CustomURL.GetImageServer();
                    var carRequests = _carRequestsService.GetRequestListByVendor(VendorID).OrderByDescending(x => x.VendorRequests.Count()).Skip((pg - 1) * take).Take(take).Select(i => new
                    {
                        i.ID,
                        i.Title,
                        i.Description,
                        i.Price,
                        MaxPrice = i.Price.HasValue ? i.Price.Value : 0,
                        MinPrice = i.MinPrice.HasValue ? i.MinPrice.Value : 0,
                        Brand = i.Brand != null ? new
                        {
                            i.Brand.ID,
                            i.Brand.Name
                        } : null,
                        Make = i.CarMake != null ? new
                        {
                            i.CarMake.ID,
                            i.CarMake.Name
                        } : null,
                        Model = i.CarModel != null ? new
                        {
                            i.CarModel.ID,
                            i.CarModel.Name
                        } : null,
                        i.Color,
                        i.Doors,
                        Year = i.Year.HasValue ? i.Year.Value.Year.ToString() : "",
                        i.Cylinders,
                        i.Horsepower,
                        i.Transmission,
                        i.RegionalSpecification,
                        MinYear = i.MinYear,
                        MaxYear = i.MaxYear,
                        MinKilometer = i.MinKilometers.HasValue ? i.MinKilometers.Value.ToString() : "-",
                        MaxKilometer = i.MaxKilometers.HasValue ? i.MaxKilometers.Value.ToString() : "-",
                        Warranty = i.Warranty.HasValue ? i.Warranty.Value : false,
                        Category = i.Category.CategoryName,
                        isFlaged = i.VendorRequests.Count() > 0 ? true : false,
                        CreatedOn = GetDate(i.CreatedOn),
                        Customer = i.Customer != null ? new
                        {
                            i.Customer.ID,
                            Image = ImageServer + i.Customer.Logo,
                            i.Customer.Name,
                            i.Customer.Contact
                        } : null,
                        Type = "Car"

                    });

                    var propertyRequests = _propertyRequestsService.GetRequestListByVendor(VendorID).OrderByDescending(x => x.VendorRequests.Count()).Skip((pg - 1) * take).Take(take).Select(i => new
                    {
                        i.ID,
                        i.Title,
                        i.Description,
                        i.Price,
                        MaxPrice = i.Price.HasValue ? i.Price.Value : 0,
                        MinPrice = i.MinPrice.HasValue ? i.MinPrice.Value : 0,
                        Latitude = i.Latitude.HasValue ? i.Latitude.Value : 0,
                        Longitude = i.Longitude.HasValue ? i.Longitude.Value : 0,
                        i.Address,
                        NoOfBathRooms = i.NoOfBathRooms.HasValue ? i.NoOfBathRooms.Value : 0,
                        i.Size,
                        i.NoOfBedRooms,
                        BuildYear = i.BuildYear.HasValue ? i.BuildYear.Value.Year.ToString() : "",
                        Country = i.Country.HasValue ? i.Country1.Name : "",
                        City = i.City.HasValue ? i.City1.Name : "",
                        i.Area,
                        i.Category.CategoryName,
                        isFlaged = i.VendorRequests.Count() > 0 ? true : false,
                        i.ForSale,
                        CreatedOn = GetDate(i.CreatedOn),
                        Customer = i.Customer != null ? new
                        {
                            i.Customer.ID,
                            Image = ImageServer + i.Customer.Logo,
                            i.Customer.Name,
                            i.Customer.Contact
                        } : null,
                        Type = "Property"

                    });

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        carRequests = carRequests,
                        propertyRequests = propertyRequests
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
        [Route("car/customerrequests")]
        public HttpResponseMessage GetCarCustomerRequest(int pg = 1, int take = 20)
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

                    var ImageServer = CustomURL.GetImageServer();
                    var carRequests = _carRequestsService.GetRequestListByVendor(VendorID).OrderByDescending(x => x.VendorRequests.Count()).Skip((pg - 1) * take).Take(take).Select(i => new
                    {
                        i.ID,
                        i.Title,
                        i.Description,
                        i.Price,
                        MaxPrice = i.MaxPrice.HasValue ? i.MaxPrice.Value : 0,
                        MinPrice = i.MinPrice.HasValue ? i.MinPrice.Value : 0,
                        Brand = i.Brand != null ? new
                        {
                            i.Brand.ID,
                            i.Brand.Name
                        } : null,
                        Make = i.CarMake != null ? new
                        {
                            i.CarMake.ID,
                            i.CarMake.Name
                        } : null,
                        Model = i.CarModel != null ? new
                        {
                            i.CarModel.ID,
                            i.CarModel.Name
                        } : null,
                        i.Color,
                        i.Doors,
                        Year = i.Year.HasValue ? i.Year.Value.Year.ToString() : "",
                        i.Cylinders,
                        i.Horsepower,
                        i.Transmission,
                        i.RegionalSpecification,                        
                        MinYear = i.MinYear,
                        MaxYear = i.MaxYear,
                        MinKilometer = i.MinKilometers.HasValue ? i.MinKilometers.Value.ToString() : "-",
                        MaxKilometer = i.MaxKilometers.HasValue ? i.MaxKilometers.Value.ToString() : "-",
                        Warranty = i.Warranty.HasValue ? i.Warranty.Value : false,
                        Category = i.Category.CategoryName,
                        isFlaged = i.VendorRequests.Count() > 0 ? true : false,
                        CreatedOn = GetDate(i.CreatedOn),
                        Customer = i.Customer != null ? new
                        {
                            i.Customer.ID,
                            Image = ImageServer + i.Customer.Logo,
                            i.Customer.Name,
                            i.Customer.Contact
                        } : null,
                        Type = "Car"

                    });

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        carRequests = carRequests
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
        [Route("property/customerrequests")]
        public HttpResponseMessage GetPropertyustomerRequest(int pg = 1, int take = 20)
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

                    var ImageServer = CustomURL.GetImageServer();

                    var propertyRequests = _propertyRequestsService.GetRequestListByVendor(VendorID).OrderByDescending(x => x.VendorRequests.Count()).Skip((pg - 1) * take).Take(take).Select(i => new
                    {
                        i.ID,
                        i.Title,
                        i.Description,
                        i.Price,
                        MaxPrice = i.MaxPrice.HasValue ? i.MaxPrice.Value : 0,
                        MinPrice = i.MinPrice.HasValue ? i.MinPrice.Value : 0,
                        Latitude = i.Latitude.HasValue ? i.Latitude.Value : 0,
                        Longitude = i.Longitude.HasValue ? i.Longitude.Value : 0,
                        i.Address,
                        NoOfBathRooms = i.NoOfBathRooms.HasValue ? i.NoOfBathRooms.Value : 0,
                        i.Size,
                        i.NoOfRooms,
                        BuildYear = i.BuildYear.HasValue ? i.BuildYear.Value.Year.ToString() : "",
                        Country = i.Country.HasValue ? i.Country1.Name : "",
                        City = i.City.HasValue ? i.City1.Name : "",
                        i.Area,
                        i.Category.CategoryName,
                        isFlaged = i.VendorRequests.Count() > 0 ? true : false,
                        i.ForSale,
                        CreatedOn = GetDate(i.CreatedOn),
                        Customer = i.Customer != null ? new
                        {
                            i.Customer.ID,
                            Image = ImageServer + i.Customer.Logo,
                            i.Customer.Name,
                            i.Customer.Contact
                        } : null,
                        Type = "Property"

                    });

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        propertyRequests = propertyRequests
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
        [Route("customerrequest/{RequestID}/car/flag")]
        public HttpResponseMessage FlagCarRequest(int RequestID)
        {
            try
            {
                string message = string.Empty;
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

                    var requestModel = _carRequestsService.GetRequestByID(RequestID);

                    if (requestModel == null)
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            success = false,
                            message = "Request not found",
                        });

                    var request = _vendorRequestService.GetCarRequestByVendor((int)VendorID).Where(x => x.CarID == RequestID).FirstOrDefault();

                    if (request != null)
                    {
                        if (_vendorRequestService.Delete(request))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                success = true,
                                message = "Request unflagged successfully",
                            });
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            success = false,
                            message = "Ops! something went wrong",
                        });

                    }


                    VendorRequest model = new VendorRequest();

                    model.CarID = RequestID;
                    model.VendorID = VendorID;
                    model.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();

                    if (_vendorRequestService.AddVendorRequest(model, ref message))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            success = true,
                            message = "Request Flagged Successfully",
                        });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = false,
                        message = "Ops! something went wrong",
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        success = false,
                        message = "Authorization has been failed for this request",
                    });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    message = "Ops! something went wrong",
                });
            }
        }

        [HttpPut]
        [Route("customerrequest/{RequestID}/property/flag")]
        public HttpResponseMessage FlagPropertyRequest(int RequestID)
        {
            try
            {
                string message = string.Empty;
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

                    var requestModel = _propertyRequestsService.GetRequestByID(RequestID);

                    if (requestModel == null)
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            success = false,
                            message = "Request not found",
                        });

                    var request = _vendorRequestService.GetPropertyRequestByVendor((int)VendorID).Where(x => x.PropertyID == RequestID).FirstOrDefault();

                    if (request != null)
                    {
                        if (_vendorRequestService.Delete(request))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                success = true,
                                message = "Request unflagged successfully",
                            });
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            success = false,
                            message = "Ops! something went wrong",
                        });
                    }

                    VendorRequest model = new VendorRequest();

                    model.PropertyID = RequestID;
                    model.VendorID = VendorID;
                    model.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();

                    if (_vendorRequestService.AddVendorRequest(model, ref message))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            success = true,
                            message = "Request Flagged Successfully",
                        });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = false,
                        message = "Ops! something went wrong",
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        success = false,
                        message = "Authorization has been failed for this request",
                    });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    message = "Ops! something went wrong",
                });
            }
        }

        private string GetDate(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "";

            return dateTime.Value.ToString("dd MMM yyyy, h:mm tt");
        }
    }
}
