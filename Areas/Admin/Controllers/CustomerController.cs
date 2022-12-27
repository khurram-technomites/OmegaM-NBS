using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CustomerController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ICustomerService _customerService;
        private readonly ICityService _cityService;
        private readonly ICountryService _countryService;
        private readonly IAreaService _areaService;
        private readonly IGetInTouchService _getInTouchService;
        private readonly ICustomerLoyaltySettingService _customerLoyaltySettingService;
        private readonly IScheduleMeetingService _meetingService;
        private readonly ITestRunCustomerService _testRunCustomerService;
        private readonly ICarRequestsService _carservice;
        private readonly IPropertyRequestsService _propertyservice;


        public CustomerController(ICustomerService customerService, ICountryService countryService, ICityService cityService, IAreaService areaService, ICustomerLoyaltySettingService customerLoyaltySettingService, IGetInTouchService getInTouchService, IScheduleMeetingService meetingService, ITestRunCustomerService testRunCustomerService, ICarRequestsService carRequestsService, IPropertyRequestsService propertyRequestsService)
        {
            this._customerService = customerService;
            this._countryService = countryService;
            this._cityService = cityService;
            this._areaService = areaService;
            this._customerLoyaltySettingService = customerLoyaltySettingService;
            this._getInTouchService = getInTouchService;
            this._meetingService = meetingService;
            this._carservice = carRequestsService;
            this._propertyservice = propertyRequestsService;
            this._testRunCustomerService = testRunCustomerService;
        }


        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }
        public ActionResult Customer360(int id)
        {

           
            Customer customer = _customerService.GetCustomer((Int16)id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["CustomerId"] = id;
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View(customer);
        }

        //Motors
        public ActionResult GetInTouchListMotors()
        {

            int CustomerId = Convert.ToInt32(TempData["CustomerId"]);
            var getintouch = _getInTouchService.GetListByCustomerAndMotor(CustomerId);
            return PartialView(getintouch);
           
           
        }
        public ActionResult GetInTouchListDetails(long Id)
        {
            ViewBag.BuildingID = Id;
            var getintouch = _getInTouchService.GetById((int)Id);
            return PartialView(getintouch);
        }
        public ActionResult MotorMeetingList()
        {
            int CustomerId = Convert.ToInt32(TempData["CustomerId"]);
            var Meeting = _meetingService.GetListByCustomerAndMotors(CustomerId).OrderByDescending(x => x.ID);
            return PartialView(Meeting);
        }
        public ActionResult MotorDriveList()
        {
            int CustomerId = Convert.ToInt32(TempData["CustomerId"]);
            var type = "Motor";
            var customers = _testRunCustomerService.GetTestRunCustomersByCustomerID(CustomerId, type);
            return PartialView(customers);
        }
        public ActionResult MotorRequest()
        {
            List<CarRequest> model = new List<CarRequest>();
            int CustomerId = Convert.ToInt32(TempData["CustomerId"]);
            model = _carservice.GetRequestListByCustomer(CustomerId).ToList();
            return PartialView(model);
        }
        public ActionResult MotorRequestDetails(long RequestId)
        {
            List<VendorRequest> model = new List<VendorRequest>();
            var carrequest = _carservice.GetRequestByID((int)RequestId);
            return PartialView(carrequest);
        }

        //Properties
        public ActionResult GetInTouchListProperty()
        {
          int CustomerId = Convert.ToInt32(TempData["CustomerId"]);
          var getintouch = _getInTouchService.GetListByCustomerAndProperty(CustomerId);
          return PartialView(getintouch);
          
           

        }
        public ActionResult PropertyMeetingList()
        {
            int CustomerId = Convert.ToInt32(TempData["CustomerId"]);

            var Meeting = _meetingService.GetListByCustomerAndProperty(CustomerId).OrderByDescending(x => x.ID);
            return PartialView(Meeting);
        }
        public ActionResult PropertyTourList()
        {
            int CustomerId = Convert.ToInt32(TempData["CustomerId"]);
            var type = "Property";
            var customers = _testRunCustomerService.GetTestRunCustomersByCustomerID(CustomerId, type);
            return PartialView(customers);
        }
        public ActionResult PropertyRequest()
        {
            List<PropertyRequest> model = new List<PropertyRequest>();
            int CustomerId = Convert.ToInt32(TempData["CustomerId"]);
            model = _propertyservice.GetRequestListByCustomer(CustomerId).ToList();
            return PartialView(model);
        }
        public ActionResult PropertyRequestDetails(long RequestId)
        {
            List<VendorRequest> model = new List<VendorRequest>();
            var propertyrequest = _propertyservice.GetRequestByID((int)RequestId);
          
            return PartialView(propertyrequest);
        }

        public ActionResult List()
        {
            var customers = _customerService.GetCustomers();
            return PartialView(customers);
        }

        public ActionResult ListReport()
        {
            var customers = _customerService.GetCustomers();
            return View(customers);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = _customerService.GetCustomer((Int16)id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }
        [HttpGet]
        public ActionResult GetCitiesByCountry(long id)
        {
            var cities = _cityService.GetCitiesForDropDown(id);

            return Json(new { success = true, message = "Data recieved successfully!", data = cities }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetAreasByCity(long id)
        {
            var areas = _areaService.GetAreasForDropDown(id);

            return Json(new { success = true, message = "Data recieved successfully!", data = areas }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            ViewBag.CountryID = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text");
            //ViewBag.CityID = new SelectList(_cityService.GetCitiesForDropDown(), "value", "text");
            ViewBag.AccountType = new SelectList(_customerLoyaltySettingService.GetCustomerTypeForDropDown(), "value", "text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Customer customer)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                customer.AccountType = "Basic";
                customer.Logo = "/Assets/AppFiles/Customer/default.png";
                customer.Points = 0;
                if (_customerService.CreateCustomer(ref customer, string.Empty, ref message, true, true))
                {
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/Customer/Index",
                        message = message,
                        data = new
                        {
                            ID = customer.ID,
                            Date = customer.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            Name = customer.Name,
                            Contact = customer.Contact,
                            Email = customer.Email,
                            Address = customer.Address,
                            IsActive = customer.IsActive.HasValue ? customer.IsActive.Value.ToString() : bool.FalseString

                        }
                    });
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }

            return Json(new { success = false, message = message });
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = _customerService.GetCustomer((long)id);
            ViewBag.CountryID = new SelectList(_countryService.GetCountriesForDropDown(), "value", "text");
            ViewBag.CityID = new SelectList(_cityService.GetCitiesForDropDown(), "value", "text", customer.CityID);
            ViewBag.AreaID = new SelectList(_areaService.GetAreasForDropDown(), "value", "text", customer.AreaID);
            ViewBag.AccountType = new SelectList(_customerLoyaltySettingService.GetCustomerTypeForDropDown(), "value", "text", customer.AccountType);
            if (customer == null)
            {
                return HttpNotFound();
            }

            TempData["CustomerID"] = id;
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Customer customer)
        {
            string message = string.Empty;
            string status = string.Empty;
            if (ModelState.IsValid)
            {
                long Id;
                if (TempData["CustomerID"] != null && Int64.TryParse(TempData["CustomerID"].ToString(), out Id) && customer.ID == Id)
                {
                    if (_customerService.UpdateCustomer(ref customer, ref message, ref status))
                    {
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/Customer/Index",
                            message = "Customer updated successfully ...",
                            data = new
                            {
                                ID = customer.ID,
                                Date = customer.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                                Name = customer.Name,
                                Contact = customer.Contact,
                                Email = customer.Email,
                                Address = customer.Address,
                                IsActive = customer.IsActive.HasValue ? customer.IsActive.Value.ToString() : bool.FalseString
                            }
                        });
                    }
                }
                else
                {
                    message = "Oops! Something went wrong. Please try later.";
                }
            }
            else
            {
                message = "Please fill the form properly ...";
            }
            return Json(new { success = false, message = message });
        }

        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = _customerService.GetCustomer((long)id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            if (!(bool)customer.IsActive)
                customer.IsActive = true;
            else
            {
                customer.IsActive = false;
            }
            string message = string.Empty;
            string status = string.Empty;
            if (_customerService.UpdateCustomer(ref customer, ref message, ref status))
            {
                SuccessMessage = "Customer " + ((bool)customer.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        ID = customer.ID,
                        Date = customer.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        Name = customer.Name,
                        Contact = customer.Contact,
                        Email = customer.Email,
                        Address = customer.Address,
                        IsActive = customer.IsActive.HasValue ? customer.IsActive.Value.ToString() : bool.FalseString

                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = _customerService.GetCustomer((Int16)id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            TempData["CustomerID"] = id;
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_customerService.DeleteCustomer((Int16)id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomersReport()
        {
            var getAllCustomers = _customerService.GetCustomers().ToList();
            if (getAllCustomers.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("CustomersReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Name"
                        ,"Contact"
                        ,"Email"
                        ,"Address"
                        ,"Country"
                        ,"City"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["CustomersReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    if (getAllCustomers.Count != 0)
                        getAllCustomers = getAllCustomers.OrderByDescending(x => x.ID).ToList();

                    foreach (var i in getAllCustomers)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
                        ,!string.IsNullOrEmpty(i.Contact) ? i.Contact: "-"
                        ,!string.IsNullOrEmpty(i.Email) ? i.Email : "-"
                        ,!string.IsNullOrEmpty(i.Address) ? i.Address : "-"
                        ,i.Country != null ? (!string.IsNullOrEmpty(i.Country.Name) ? i.Country.Name : "-") : "-"
                        ,i.City != null ? (!string.IsNullOrEmpty(i.City.Name) ? i.City.Name : "-") : "-"
                        ,i.IsActive == true ? "Active" : "InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Customers Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }
    }
}