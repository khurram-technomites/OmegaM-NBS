using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using NowBuySell.Web.Helpers.POCO;
using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class SuggestionController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly ICustomerSuggestionService _customerSuggestionService;
		public SuggestionController(ICustomerSuggestionService customerSuggestionService)
		{
			this._customerSuggestionService = customerSuggestionService;
		}

		// GET: Admin/Suggestion
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
			var subscribers = _customerSuggestionService.GetCustomerSuggestionDateWise(FromDate, ToDate).OrderByDescending(x => x.ID).ToList();
			return PartialView(subscribers);
		}
		public ActionResult SuggestionList()
		{
			var suggestions = _customerSuggestionService.GetCustomerSuggestions();
			return PartialView(suggestions);
		}

		[HttpPost]
		public ActionResult List(DateTime fromDate, DateTime ToDate)
		{
			DateTime EndDate = ToDate.AddMinutes(1439);
			var Suggestions = _customerSuggestionService.GetCustomerSuggestionDateWise(fromDate, EndDate).OrderByDescending(x => x.ID).ToList();
			return PartialView(Suggestions);
		}

		[HttpPost]
		public ActionResult ShowOnWebsite(long id, bool status)
		{
			string message = string.Empty;
			var getdata = _customerSuggestionService.GetCustomerSuggestion(id);

			getdata.IsShowOnWebsite = status;
			if (_customerSuggestionService.UpdateCustomerSuggestion(ref getdata, ref message))
			{
				SuccessMessage = "Suggestion " + ((bool)getdata.IsShowOnWebsite ? "Show" : "Hide") + "  successfully ...";

				return Json(new
				{
					success = true,
					message = SuccessMessage,
					data = new
					{
						ID = getdata.ID,
						Date = getdata.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
						Name = !string.IsNullOrEmpty(getdata.Name) ? getdata.Name : (getdata.Customer != null ? getdata.Customer.Name : "-"),
						Email = !string.IsNullOrEmpty(getdata.Email) ? getdata.Email : (getdata.Customer != null ? getdata.Customer.Email : "-"),
						Contact = getdata.Customer != null ? getdata.Customer.Contact : "-",
						Rating = getdata.Rating > 0 ? getdata.Rating : 0,
						Message = !string.IsNullOrEmpty(getdata.Suggestion) ? getdata.Suggestion : "-",
						RatingHidden = getdata.Rating > 0 ? getdata.Rating : 0,
						IsShown = getdata.IsShowOnWebsite.HasValue ? getdata.IsShowOnWebsite.Value.ToString() : bool.FalseString,
					}
				}, JsonRequestBehavior.AllowGet);
			}
			else
			{
				ErrorMessage = "Oops! Something went wrong. Please try later.";
			}

			return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);

		}
	}
}