using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using OfficeOpenXml;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CustomerLoyaltySettingController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ICustomerLoyaltySettingService _CustomerLoyaltySettingService;
        public CustomerLoyaltySettingController(ICustomerLoyaltySettingService customerloyaltysettingService)
        {
            this._CustomerLoyaltySettingService = customerloyaltysettingService;
        }
        // GET: Admin/CustomerLoyaltySetting
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult List()
        {
            var getList = _CustomerLoyaltySettingService.GetList();
            return PartialView(getList);
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(CustomerLoyaltySetting data)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_CustomerLoyaltySettingService.CreateCustomerLoyaltySetting(data, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/CustomerLoyaltySetting/Index",
                        message = message,
                        data = new
                        {
                            Date = data.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            Name = data.CustomerType,
                            PGRatio = data.PGRatio,
                            PRRatio = data.PRRatio,
                            PointsLimit = data.PointsLimit,
                            IsActive = data.IsActive.HasValue ? data.IsActive.Value.ToString() : bool.FalseString,
                            ID = data.ID
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
            CustomerLoyaltySetting setting = _CustomerLoyaltySettingService.GetSettings((long)id);
            if (setting == null)
            {
                return HttpNotFound();
            }

            TempData["SettingID"] = id;
            return View(setting);
        }
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerLoyaltySetting setting = _CustomerLoyaltySettingService.GetSettings((long)id);
            if (setting == null)
            {
                return HttpNotFound();
            }
            return View(setting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CustomerLoyaltySetting data)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                long Id;
                if (TempData["SettingID"] != null && Int64.TryParse(TempData["SettingID"].ToString(), out Id) && data.ID == Id)
                {
                    if (_CustomerLoyaltySettingService.UpdateCustomerLoyaltySettings(ref data, ref message))
                    {
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/Country/Index",
                            message = "Customer Loyalty updated successfully ...",
                            data = new
                            {
                                Date = data.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                                Name = data.CustomerType,
                                PGRatio = data.PGRatio,
                                PRRatio = data.PRRatio,
                                PointsLimit = data.PointsLimit,
                                IsActive = data.IsActive.HasValue ? data.IsActive.Value.ToString() : bool.FalseString,
                                ID = data.ID
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
            var Settings = _CustomerLoyaltySettingService.GetSettings((long)id);
            if (Settings == null)
            {
                return HttpNotFound();
            }

            if (!(bool)Settings.IsActive)
                Settings.IsActive = true;
            else
            {
                Settings.IsActive = false;
            }
            string message = string.Empty;
            if (_CustomerLoyaltySettingService.UpdateCustomerLoyaltySettings(ref Settings, ref message))
            {
                SuccessMessage = "Customer loyalty settings " + ((bool)Settings.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        Date = Settings.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                        Name = Settings.CustomerType,
                        PGRatio=Settings.PGRatio,
                        PRRatio= Settings.PRRatio,
                        PointsLimit=Settings.PointsLimit,
                        IsActive = Settings.IsActive.HasValue ? Settings.IsActive.Value.ToString() : bool.FalseString,
                        ID = Settings.ID
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
            CustomerLoyaltySetting settings = _CustomerLoyaltySettingService.GetSettings((Int16)id);
            if (settings == null)
            {
                return HttpNotFound();
            }
            TempData["settingsID"] = id;
            return View(settings);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_CustomerLoyaltySettingService.DeleteCustomerLoyaltySettig((Int16)id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerLoyaltySettingsReport()
        {
            var getAllCatagories = _CustomerLoyaltySettingService.GetList().ToList();
            if (getAllCatagories.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("LoyaltySettings");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Customer Type"
                        ,"Points Generation Ratio"
                        ,"Points Redemption Ratio"
                        ,"Points Limit"
                        ,"Customer Max Slab"
                        ,"Referral Point"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["LoyaltySettings"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    if (getAllCatagories.Count != 0)
                        getAllCatagories = getAllCatagories.OrderByDescending(x => x.ID).ToList();

                    foreach (var i in getAllCatagories)
                    {
                        
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.CustomerType) ? i.CustomerType : "-"
                        ,i.PGRatio ?? 0
                        ,i.PRRatio ?? 0
                        ,i.PointsLimit ?? 0
                        ,i.CustomerTypeMaxSlab ?? 0
                        ,i.ReferralPoint ?? 0
                        ,i.IsActive == true ? "Active" : "InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Customer Loyalty Settings Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }
    }
}