using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NowBuySell.Web.ViewModels;
using NowBuySell.Service.Helpers;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class SubscribersController : Controller
    {
        private readonly ISubscribersService _subscribersService;
        private readonly ICustomerSuggestionService _customerSuggestionService;
        private readonly IMail _email;
        // GET: Admin/Subscribers
        public SubscribersController(ISubscribersService subscribersService, ICustomerSuggestionService customerSuggestionService, IMail email)
        {
            this._subscribersService = subscribersService;
            this._customerSuggestionService = customerSuggestionService;
            this._email = email;

        }
        public ActionResult Index()
        {
            ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
            ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-30).ToString("MM/dd/yyyy");
            return View();
        }
        public ActionResult List()
        {
            DateTime ToDate = Helpers.TimeZone.GetLocalDateTime();
            DateTime FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-30);
			var subscribers = _subscribersService.GetsubscribersDateWise(FromDate, ToDate).OrderByDescending(i => i.ID).ToList();

            return PartialView(subscribers);

        }
        [HttpPost]
        public ActionResult List(DateTime fromDate, DateTime ToDate)
        {
            DateTime EndDate = ToDate.AddMinutes(1439);
            var subscribers = _subscribersService.GetsubscribersDateWise(fromDate, EndDate).OrderByDescending(i => i.ID).ToList();
            return PartialView(subscribers);
        }
        public ActionResult SuggestionList()
        {
            var suggestions = _customerSuggestionService.GetCustomerSuggestions();
            return PartialView(suggestions);
        }

        public ActionResult SendEmailToSubscribers()
        {
            ViewBag.Email = new SelectList(_subscribersService.GetSubscribersForDropDown(), "value", "text");
            return View();
        }

        [HttpPost]
        public ActionResult SendEmailToSubscribers(SubscriberMailViewModel subscribermail )
        {
            string message = string.Empty;
            var path = Server.MapPath("~/");
            foreach (var item in subscribermail.Email)
            {
                Subscriber Email = _subscribersService.GetSubscriberByID(item);

                if (_email.SendPromoMail(Email.EmailID, subscribermail.Subject, subscribermail.Message, path))
                {
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/Subscribers/Index",
                        message = "Email Sent",
                        data = new
                        {
                            
                            Date = Email.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            Email = Email.EmailID,
                          


                        }
                    });

                }
            }
               
            return RedirectToAction("Index","Subscriber");
        }

        [HttpPost]
        public ActionResult DeleteSubscriber(int ID)
        {
            var model = _subscribersService.GetSubscriberByID((int)ID);

            if (_subscribersService.Delete(model))
            {
                return Json(new
                {
                    success = true,
                });
            }

            return Json(new
            {
                success = false,
            });
        }
    }
}