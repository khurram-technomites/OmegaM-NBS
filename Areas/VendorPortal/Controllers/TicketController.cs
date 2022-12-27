using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.ViewModels.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NowBuySell.Web.AuthorizationProvider;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    [AuthorizeVendor]
    public class TicketController : Controller
    {

        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;
        private readonly ITicketService _ticketService;
        private readonly ITicketMessageService _ticketMessageService;
        private readonly ITicketDocumentService _ticketDocumentService;
        private readonly INotificationService _notificationService;
        private readonly INumberRangeService _numberRangeService;
        private readonly INotificationReceiverService _notificationReceiverService;

        public TicketController(ITicketService ticketService , ITicketMessageService ticketMessageService , ITicketDocumentService ticketDocumentService, INotificationService notificationService , INotificationReceiverService notificationReceiverService, INumberRangeService numberRangeService)
        {
            this._ticketService = ticketService;
            this._ticketMessageService = ticketMessageService;
            this._ticketDocumentService = ticketDocumentService;
            this._notificationService = notificationService;
            this._notificationReceiverService = notificationReceiverService;
            this._numberRangeService = numberRangeService;
        }

        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        } 
        
        public ActionResult List()
        {
        
            long vendorID = Convert.ToInt64(Session["VendorID"]);
			var ticket = _ticketService.GetTicketsByVendor(vendorID);
            return PartialView(ticket);
        }

        public ActionResult Details(long? id)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            long test = (long)(Session["VendorID"]);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var conversation = _ticketMessageService.GetTicketConversation((int)id);
            var ticket = _ticketService.GetTicket((long)id);
            TicketDetailsViewModel Details = new TicketDetailsViewModel();


            Details.CreatedOn = (DateTime)ticket.CreatedOn;
            Details.TicketID = ticket.ID;
            Details.TicketNo = ticket.TicketNo;
            Details.Priority = ticket.Priority;
            Details.Status = ticket.Status;
            Details.Description = ticket.Description;
            Details.ticketConversation = conversation.ToList();


            if (Details == null)
            {
                return HttpNotFound();
            }

            ViewBag.Referrer = Request.UrlReferrer;
            return View(Details);
        }

        [HttpPost]
        public ActionResult Message(string Message, long TicketID , string SenderName)
        {
            if(Message != "")
            { 
            HttpFileCollectionBase file = Request.Files;
            string message = string.Empty;
            long vendorID = Convert.ToInt64(Session["VendorID"]);
            TicketMessage ticketMessage = new TicketMessage();
            ticketMessage.Message = Message;
            ticketMessage.TicketID = TicketID;
            ticketMessage.SenderType = "Vendor";
            ticketMessage.SenderID = vendorID;
            ticketMessage.CreatedOn = Helpers.TimeZone.GetLocalDateTime();



            if (_ticketMessageService.CreateTicketMessage(ticketMessage, ref message))
            {
                if (Request.Files["Image"] !=  null)
                {
                    string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/Tickets/"), ticketMessage.ID, "/attachment");
                    TicketDocument document = new TicketDocument();
                    string absolutePath = Server.MapPath("~");
                    string relativePath = string.Format("/Assets/AppFiles/Images/Tickets/{0}/", ticketMessage.ID);

                    document.Url = Uploader.UploadImageAndDocs(Request.Files, absolutePath, relativePath, "Image", ref message);
                    if (_ticketDocumentService.CreateTicketDocument(document, ref message))
                    {
                        ticketMessage.TicketDocumentID = document.ID;
                        if (_ticketMessageService.UpdateTicketMessage(ref ticketMessage, ref message))
                        {
                            return Json(new
                            {
                                success = true,
                                message = "Message sent successfully ...",
                                response = new
                                {
                                    senderName = SenderName,
                                    date = ticketMessage.CreatedOn.ToString("dd MMM yyyy, h: mm tt"),
                                    file = document.Url,
                                    message = ticketMessage.Message,
                                }
                            }) ;
                        }
                    }

                   
                }

                return Json(new
                {
                    success = true,
                    message = "Message sent successfully ...",
                    response = new
                    {
                        senderName = SenderName,
                        date = ticketMessage.CreatedOn.ToString("dd MMM yyyy, h: mm tt"),
                        file = "No Image",
                        message = ticketMessage.Message,
                        ID = ticketMessage.ID
                    }
                });


            }

            else
            {
                return Json(new
                {
                    success = true,
                    message = "Message failed ...",
                   
                });

            }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Please enter message ...",
                });
            }


        }

        [HttpGet]
		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Create(Ticket ticket)
		{
     
 
            string message = string.Empty;
			string message2 = string.Empty;
			if (ModelState.IsValid)
			{
				ticket.VendorID = Convert.ToInt64(Session["VendorID"]);
				ticket.Status = "OPEN";
				ticket.TicketNo = _numberRangeService.GetNextValueFromNumberRangeByName("TICKET"); 
				if (_ticketService.CreateTicket(ticket, ref message))
				{

                    //var customer = _customerService.GetCustomer((long)ticket.CustomerID);
                    Notification not = new Notification();
                    not.Title = "Ticket";
                    not.Description = "New ticket generated";
                    not.OriginatorID = Convert.ToInt64(Session["VendorID"]);
                    not.OriginatorName = Session["VendorUserName"].ToString();
                    not.Url = "/Admin/Ticket/Index";
                    not.Module = "Ticket";
                    not.OriginatorType = "Vendor";
                    not.RecordID = ticket.ID;
                    if (_notificationService.CreateNotification(not, ref message))
                    {
                        if (_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", not.OriginatorID))
                        {
                        }
                    }
                    return Json(new
                    {
                        success = true,
                        url = "/Vendor/Ticket/Index",
                        data = new
                        {
                            Date = ticket.CreatedOn.ToString("dd MMM yyyy, h: mm tt"),
                            TicketNo = ticket.TicketNo,
                            Status = ticket.Status,
                            Priority = ticket.Priority,
                            Description = ticket.Description,
                            ID = ticket.ID
                        },
                        message = "Ticket added successfully"
                    });
				}
			}
			else
			{
				message = "Please fill the form properly ...";
			}

			return Json(new { success = false, message = message });
		}
      
        public ActionResult StatusChange(long Id)
        {
            Ticket ticket = _ticketService.GetTicket(Id);
            TempData["TicketID"] = Id;
            return PartialView(ticket);
        }

        [HttpPost]
        public ActionResult StatusChange(Ticket ticket, string status)
        {
            string message = string.Empty;
            Ticket current = _ticketService.GetTicket((long)ticket.ID);
            current.Status = status;
            if (_ticketService.UpdateTicket(ref current, ref message))
            {

                var VendorID = Convert.ToInt64(Session["VendorID"]);
                return Json(new
                {
                    success = true,
                    url = "/Vendor/Ticket/Index",
                    message = "Status updated successfully ...",
                    data = new
                    {
                        Date = current.CreatedOn.ToString("dd MMM yyyy, h: mm tt"),
                        TicketNo = current.TicketNo,
                        Status = current.Status,
                        Priority = current.Priority,
                        Description = current.Description,
                        ID = current.ID
                    }
                });
            }
            else
            
            return Json(new
            {
                success = false,
                message = "Ooops! something went wrong..."
            });
        }
    }
}