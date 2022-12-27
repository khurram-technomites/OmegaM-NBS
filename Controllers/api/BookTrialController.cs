using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.ViewModels.Api.BookATrial;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;
using System.Web.Routing;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class BookTrialController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;
        private readonly ICarService _carService;
        private readonly IPropertyService _propertyService;
        private readonly ITestRunCustomerService _testRunCustomerService;
        private readonly INumberRangeService _numberRangeService;
        private readonly INotificationReceiverService _notificationReceiverService;
        private readonly INotificationService _notificationService;
        private readonly IVendorUserService _vendorUserService;

        public BookTrialController(ICustomerService customerService, IVendorService vendorService, ICarService carService, IPropertyService propertyService, ITestRunCustomerService testRunCustomerService, INumberRangeService numberRangeService, IVendorUserService vendorUserService, INotificationReceiverService notificationReceiverService,INotificationService notificationService)
        {
            this._propertyService = propertyService;
            this._carService = carService;
            this._vendorService = vendorService;
            this._customerService = customerService;
            this._testRunCustomerService = testRunCustomerService;
            this._numberRangeService = numberRangeService;
            this._vendorUserService = vendorUserService;
            this._notificationReceiverService = notificationReceiverService;
            this._notificationService = notificationService;
        }
        [Authorize]
        [HttpPost]
        [Route("trialbooking")]
        public HttpResponseMessage CreateTrialBooking(BookTrialViewModel bookTrialViewModel)
        {
            try
            {
                string message = string.Empty,message2 = string.Empty;
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
                long customerId;
                //long customerID;
                long vendorid;
                var data = claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId);
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                    var customer = _customerService.GetCustomer(customerId);
                    if (bookTrialViewModel.Type == "Property")
                    {
                        var property = _propertyService.GetById((int)bookTrialViewModel.PropertyID);
                        vendorid = (long)property.VendorId;
                    }
                    else
                    {
                        var car = _carService.GetCar((int)bookTrialViewModel.MotorID);
                        vendorid = (long)car.VendorID;
                    }

                    TestRunCustomer testRunCustomer = new TestRunCustomer();
                    testRunCustomer.CustomerID = customerId;
                    testRunCustomer.CustomerName = customer.Name;
                    testRunCustomer.CustomerContact = customer.Contact;
                    testRunCustomer.CustomerEmail = customer.Email;
                    testRunCustomer.Type = bookTrialViewModel.Type;
                    testRunCustomer.Status = "Pending";
                    testRunCustomer.MotorID = bookTrialViewModel.MotorID;
                    testRunCustomer.PropertyID = bookTrialViewModel.PropertyID;
                    testRunCustomer.VendorID = vendorid;
                    testRunCustomer.BookedDate = bookTrialViewModel.BookedDate;
                    testRunCustomer.BookedTime = bookTrialViewModel.BookedTime;
                    testRunCustomer.Description = bookTrialViewModel.Description;
                    testRunCustomer.TrialBookingNo = _numberRangeService.GetNextValueFromNumberRangeByName("TRIALBOOKING"); ;

                    if (_testRunCustomerService.CreateTestRunCustomer(testRunCustomer, ref message))
                    {
                        Notification not = new Notification();
                        not.Title = bookTrialViewModel.Type == "Property" ? "Property Request" : "Car Request";
                        not.Description = bookTrialViewModel.Type == "Property" ? string.Format("{0} want to Tour Property", customer.Name) : string.Format("{0} want to Test Drive ", customer.Name);
                        //not.OriginatorID = customerId;
                        not.OriginatorName = "";
                        not.Url = bookTrialViewModel.Type == "Property" ? "/Vendor/TestRunCustomer/Index" : "/Vendor/TestRunCustomer/MotorTrialIndex";
                        //not.Module = getInTouch.CarID.HasValue ? "GetInTouchRequest" : "GetInTouchRequest";
                        not.Module = "TestRunCustomer";
                        not.OriginatorType = "Customer";
                        not.RecordID = testRunCustomer.ID;
                        if (_notificationService.CreateNotification(not, ref message2))
                        {
                            var vendorUser = _vendorUserService.GetUserByRole((long)vendorid, "Administrator");
                            NotificationReceiver notRec = new NotificationReceiver();
                            notRec.ReceiverID = vendorUser.ID;
                            notRec.ReceiverType = "Vendor";
                            notRec.NotificationID = not.ID;
                            _notificationReceiverService.CreateNotificationReceiver(notRec, ref message2);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            status = "success",
                            message = "Trial Booked successfully... "
                        });
                    }
                    //var bookings = _serviceBookingService.GetAllServiceOngoingBookingsbyStaffID(staffId).Skip((int)pageFilterViewModel.pageSize * ((int)pageFilterViewModel.pgno - 1)).Take((int)pageFilterViewModel.pageSize);
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "error", message = "Oops! Insert data properly..." });

                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
                }

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
