using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers.Routing;
using NowBuySell.Web.ViewModels.DataTables;
using NowBuySell.Web.ViewModels.Car;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using static NowBuySell.Web.Helpers.Enumerations.Enumeration;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class LeadRequestController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ILeadRequestService _leadRequestService;
        private readonly ICustomerService _customerService;
        private readonly IServicesCarService _servicesCarService;
        private readonly INotificationService _notificationService;
        private readonly INotificationReceiverService _notificationReceiverService;

        public LeadRequestController(ILeadRequestService leadRequestService, ICustomerService customerService, IServicesCarService servicesCarService, INotificationService notificationService, INotificationReceiverService notificationReceiverService)
        {
            this._leadRequestService = leadRequestService;
            this._customerService = customerService;
            this._servicesCarService = servicesCarService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
        }

        public ActionResult Index()
        {
            ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
            ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-7).ToString("MM/dd/yyyy");
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }


        public ActionResult List()
        {
            var requests = _leadRequestService.GetLeadRequests();
            return PartialView(requests);
        }

        [HttpPost]
        public ActionResult List(DateTime fromDate, DateTime ToDate)
        {
            DateTime EndDate = ToDate.AddMinutes(1439);
            var requests = _leadRequestService.GetLeadRequestsFilter(fromDate, EndDate);
            return PartialView(requests);
        }

        public ActionResult StatusChange(long id)
        {
            var request = _leadRequestService.GetLeadRequest((long)id);
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StatusChange(LeadRequest leadRequest)
        {
            try
            {
                string message = string.Empty;
                string error = string.Empty;
                if (_leadRequestService.UpdateRequestStatus(ref leadRequest, ref message, ref error))
                {
                    if (leadRequest.CustomerID != null)
                    {
                        Notification not = new Notification();

                        if (leadRequest.Status == "Pending")
                        {
                            not.Title = "Lead Request Placed";
                            not.TitleAr = "Lead Request  Placed";
                            not.Description = string.Format("Your Lead Request # {0} has been placed. You can check the Request status via my lead requests", leadRequest.RequestNo);
                            not.DescriptionAr = string.Format("Your Lead Request # {0} has been placed. You can check the Request status via my lead requests", leadRequest.RequestNo);

                        }
                        else if (leadRequest.Status == "Processing")
                        {
                            not.Title = "Lead Request Completed";
                            not.TitleAr = "Lead Request  Completed";
                            not.Description = string.Format("Your Lead Request # {0} has been processing. You can check the Request status via my lead requests", leadRequest.RequestNo);
                            not.DescriptionAr = string.Format("Your Lead Request # {0} has been processing. You can check the Request status via my lead requests", leadRequest.RequestNo);
                        }
                        else if (leadRequest.Status == "Canceled")
                        {
                            not.Title = "Lead Request Canceled";
                            not.TitleAr = "Lead Request  Canceled";
                            not.Description = string.Format("Your Lead Request # {0} has been canceled. You can check the Request status via my lead requests", leadRequest.RequestNo);
                            not.DescriptionAr = string.Format("Your Lead Request # {0} has been canceled. You can check the Request status via my lead requests", leadRequest.RequestNo);
                        }
                        else if (leadRequest.Status == "Closed")
                        {
                            not.Title = "Lead Request Closed";
                            not.TitleAr = "Lead Request  Closed";
                            not.Description = string.Format("Your Lead Request # {0} has been closed. You can check the Request status via my lead requests", leadRequest.RequestNo);
                            not.DescriptionAr = string.Format("Your Lead Request # {0} has been closed. You can check the Request status via my lead requests", leadRequest.RequestNo);
                        }

                        not.OriginatorID = Convert.ToInt64(Session["AdminUserID"]);
                        not.OriginatorName = Session["UserName"].ToString();
                        not.Module = "LeadRequest";
                        not.OriginatorType = "Admin";
                        not.RecordID = leadRequest.ID;
                        if (_notificationService.CreateNotification(not, ref message))
                        {
                            NotificationReceiver notRec = new NotificationReceiver();
                            notRec.ReceiverID = leadRequest.CustomerID;
                            notRec.ReceiverType = "Customer";
                            notRec.NotificationID = not.ID;
                            if (_notificationReceiverService.CreateNotificationReceiver(notRec, ref message))
                            {
                            }
                        }
                    }
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/LeadRequest/Index",
                        message = "Lead Request status updated successfully ...",
                        data = new
                        {
                            Date = leadRequest.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            RequestNo = leadRequest.RequestNo,
                            Customer = leadRequest.Name,
                            Car = leadRequest.ServiceCar.Image + "|" + leadRequest.ServiceCar.Title,
                            Status = leadRequest.Status,
                            ID = leadRequest.ID
                        }
                    });
                }
                else
                {

                    return Json(new
                    {
                        success = false,
                        message = "Oops! Something went wrong. Please try later."
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Oops! Something went wrong. Please try later."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var request = _leadRequestService.GetLeadRequest((Int16)id);
            if (request == null)
            {
                return HttpNotFound();
            }
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LeadRequestReport(DateTime fromDate, DateTime ToDate)
        {
            DateTime EndDate = ToDate.AddMinutes(1439); 
            var leadRequest = _leadRequestService.GetLeadRequestsFilter(fromDate, EndDate);
            if (leadRequest.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("LeadRequestReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Request No"
                        ,"Solicitor Name"
                        ,"Company"
                        ,"Nationality"
                        ,"Phone"
                        ,"Address"
                        ,"Customer Email"
                        ,"Service Car"
                        ,"Service Name"
                        ,"Service Category"
                        ,"Remarks"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["LeadRequestReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in leadRequest)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.RequestNo) ? i.RequestNo : "-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
                        ,!string.IsNullOrEmpty(i.Company) ? i.Company : "-"
                        ,!string.IsNullOrEmpty(i.Nationality) ? i.Nationality : "-"
                        ,!string.IsNullOrEmpty(i.Phone) ? i.Phone : "-"
                        ,!string.IsNullOrEmpty(i.Address) ? i.Address : "-"
                        ,!string.IsNullOrEmpty(i.Email) ? i.Email : "-"
                        ,i.ServiceCar != null ? ( !string.IsNullOrEmpty(i.ServiceCar.Title) ? i.ServiceCar.Title : "-" ) : "-"
                        ,i.ServiceCar != null ? ( i.ServiceCar.ServiceCompare != null ? ( !string.IsNullOrEmpty(i.ServiceCar.ServiceCompare.Name) ? i.ServiceCar.ServiceCompare.Name : "-" ) : "-" ) : "-"
                        ,i.ServiceCar != null ? ( i.ServiceCar.ServiceCompare != null ? ( i.ServiceCar.ServiceCompare.ServiceCategory != null ? ( !string.IsNullOrEmpty(i.ServiceCar.ServiceCompare.ServiceCategory.Name) ? i.ServiceCar.ServiceCompare.ServiceCategory.Name : "-" ) : "-" ) : "-" ) : "-"
                        ,!string.IsNullOrEmpty(i.Remarks) ? i.Remarks : "-"
                        ,i.Status != null ? i.Status : "-"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Lead Requests Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }
    }
}