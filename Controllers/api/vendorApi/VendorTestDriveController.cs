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
    public class VendorTestDriveController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ITestRunCustomerService _testRunCustomerService;

        public VendorTestDriveController(ITestRunCustomerService testRunCustomerService)
        {
            this._testRunCustomerService = testRunCustomerService;
        }

        [HttpGet]
        [Route("TestDrive")]
        public HttpResponseMessage TestDrive(int pg = 1, int take = 20)
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

                    var List = _testRunCustomerService.GetTestRunCustomers((int)VendorID, "Motor");

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = "success",
                        message = "",
                        TestDriveRequests = List.OrderByDescending(x => x.ID).Skip((pg - 1) * take).Take(take).Select(x => new
                        {
                            x.ID,
                            x.VendorID,
                            x.Description,
                            BookedDate = GetDate(x.BookedDate),
                            BookedTime = GetTime(x.BookedTime),
                            x.TrialBookingNo,
                            x.Status,
                            CreatedOn = GetDate(x.CreatedOn),
                            Customer = x.Customer != null ? new
                            {
                                CustomerID = x.CustomerID,
                                CustomerName = x.Customer.Name,
                                CustomerEmail = x.Customer.Email,
                                CustomerPhone = x.Customer.Contact,
                            } : null,
                            motor = x.Car != null ? new
                            {
                                CarID = x.MotorID,
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

        private string GetDate(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "-";

            string Month = dateTime.Value.ToString("MMM");

            return dateTime.Value.Day + " " + Month + " " + dateTime.Value.Year;
        }
        private string GetTime(TimeSpan? dateTime)
        {
            if (!dateTime.HasValue)
                return "-";

            DateTime time = DateTime.Today.Add((TimeSpan)dateTime);
            string Hours = time.ToString("hh");
            string Min = time.ToString("mm");
            string AMPM = time.ToString("tt");

            return Hours + ":" + Min + " " + AMPM;
        }
        [HttpPost]
        [Route("TestDriveStatus")]
        public HttpResponseMessage Status(int ID = 0, string Status = "")
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
                    var current = _testRunCustomerService.GetTestRunCustomer(ID);
                    current.Status = Status;
                    if (_testRunCustomerService.UpdateTestRunCustomer(ref current, ref message))
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