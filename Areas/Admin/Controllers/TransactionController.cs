using NowBuySell.Service;
using NowBuySell.Web.Areas.Admin.ViewModels;
using NowBuySell.Web.AuthorizationProvider;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NowBuySell.Data;


namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            this._transactionService = transactionService;

        }
        // GET: Admin/Transaction
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
            var transactions = _transactionService.GetTransactions(FromDate, ToDate).OrderByDescending(x => x.ID);
            return PartialView(transactions);
        }
        [HttpPost]
        public ActionResult List(DateTime fromDate, DateTime ToDate)
        {
            var transactions = _transactionService.GetTransactions(fromDate, ToDate);
            return PartialView(transactions);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TransactionsReport(DateTime? FromDate, DateTime? ToDate)
        {
            DateTime EndDate = ToDate.Value.AddMinutes(1439);
            var getAllTransactions = _transactionService.GetTransactions(FromDate.Value, EndDate).ToList();
            if (getAllTransactions.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("TransactionReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Booking #"
                        ,"Name On Card"
                        ,"Mask card #"
                        ,"Transaction Status"
                        ,"Amount"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["TransactionReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    if (getAllTransactions.Count != 0)
                        getAllTransactions = getAllTransactions.OrderByDescending(x => x.ID).ToList();

                    foreach (var i in getAllTransactions)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Order.OrderNo) ? i.Order.OrderNo:"-"
                        ,!string.IsNullOrEmpty(i.NameOnCard) ? i.NameOnCard :"-"
                        ,!string.IsNullOrEmpty(i.MaskCardNo) ? i.MaskCardNo:"-"
                        ,!string.IsNullOrEmpty(i.TransactionStatus) ? i.TransactionStatus:"-"
                        ,i.Amount.HasValue ? i.Amount.Value : 0
						//,i.IsActive == true ? "Active" :"InActive"
						});
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Transaction Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }
    }
}