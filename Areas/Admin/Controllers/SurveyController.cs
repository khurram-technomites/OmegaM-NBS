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
    public class SurveyController : Controller
    {
        private readonly ISurveyService _surveyService;
        public SurveyController (ISurveyService surveyService)
        {
            this._surveyService = surveyService;
        }
        // GET: Admin/Survey
        public ActionResult Index()
        {
            ViewBag.ToDate = Helpers.TimeZone.GetLocalDateTime().ToString("MM/dd/yyyy");
            ViewBag.FromDate = Helpers.TimeZone.GetLocalDateTime().AddDays(-30).ToString("MM/dd/yyyy");
            return View();
        }
        public ActionResult List()
        {
            var survey = _surveyService.GetAll().ToList();
            return PartialView(survey);

        }
        [HttpPost]
        public ActionResult List(DateTime fromDate, DateTime ToDate)
        {
            DateTime EndDate = ToDate.AddMinutes(1439);
            var survey = _surveyService.GetAll();
            return PartialView(survey);
        }
    }
}