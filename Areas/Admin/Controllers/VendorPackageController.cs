using OfficeOpenXml;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class VendorPackageController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IVendorPackagesService _vendorPackageService;
     
        public VendorPackageController(IVendorPackagesService vendorPackageService)
        {
            this._vendorPackageService = vendorPackageService;
        }

        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        public ActionResult List()
        {
            var packages = _vendorPackageService.GetAll(true);
            return PartialView(packages);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VendorPackage package = _vendorPackageService.GetById((Int16)id);
            if (package == null)
            {
                return HttpNotFound();
            }
            return View(package);
        }
        [HttpGet]
        public ActionResult Create()
        {
            VendorPackage vendorPackage = new VendorPackage();
            return View(vendorPackage);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VendorPackage package)
        {
            DateTime currentDateTime = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
            string message = string.Empty;
            package.IsDeleted = false;
            package.IsActive = true;
            if (ModelState.IsValid)
            {
                switch(package.BillingPeriod.ToLower())
                {
                    case "monthly":
                        package.MonthCount = 1;
                        break;
                    case "quarterly":
                        package.MonthCount = 3;
                        break;
                    case "half yearly":
                        package.MonthCount = 6;
                        break;
                    case "yearly":
                        package.MonthCount = 12;
                        break;
                }
                if (package.IsFree == true)
                {
                    package.Price = 0;
                }
               /* if (currentDateTime >= )
                {

                }*/


                if (_vendorPackageService.AddPackage(package, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/VendorPackage/Index",
                        message = message,
                        data = new
                        {
                            Date = package.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            Name = package.Name,
                            IsFree = package.IsFree,
                            PropertyLimit = package.PropertyLimit,
                            MotorLimit = package.MotorLimit,
                            Price = package.Price,
                            IsActive = package.IsActive.HasValue ? package.IsActive.Value.ToString() : bool.FalseString,
                            ID = package.ID
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
        [HttpGet]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VendorPackage package = _vendorPackageService.GetById((int)id);
            if (package == null)
            {
                return HttpNotFound();
            }

            TempData["VendorPackage"] = id;
            return View(package);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VendorPackage package)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                long Id;
                if (TempData["VendorPackage"] != null && Int64.TryParse(TempData["VendorPackage"].ToString(), out Id) && package.ID == Id)
                {
                    switch (package.BillingPeriod.ToLower())
                    {
                        case "monthly":
                            package.MonthCount = 1;
                            break;
                        case "quarterly":
                            package.MonthCount = 3;
                            break;
                        case "half yearly":
                            package.MonthCount = 6;
                            break;
                        case "yearly":
                            package.MonthCount = 12;
                            break;
                    }
                    if (_vendorPackageService.UpdatePackage(ref package, ref message))
                    {
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/VendorPackage/Index",
                            message = "Package updated successfully ...",
                            data = new
                            {
                                Date = package.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                                Name = package.Name,
                                IsFree = package.IsFree,
                                PropertyLimit = package.PropertyLimit.HasValue ? package.PropertyLimit.Value.ToString() : bool.FalseString,
                                MotorLimit = package.MotorLimit.HasValue ? package.MotorLimit.Value.ToString() : bool.FalseString,
                                Price = package.Price,
                                IsActive = package.IsActive.HasValue ? package.IsActive.Value.ToString() : bool.FalseString,
                                ID = package.ID
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
            var package = _vendorPackageService.GetById((int)id);
            if (package == null)
            {
                return HttpNotFound();
            }

            if (package.IsActive.HasValue)
            {
                package.IsActive = package.IsActive == true ? false : true;
            }            
            else
            {
                package.IsActive = true;
            }
            string message = string.Empty;
            if (_vendorPackageService.UpdatePackage(ref package, ref message))
            {
                SuccessMessage = "Package " + ((bool)package.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        Date = package.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                        Name = package.Name,
                        NameAR = package.NameAr,
                        Price = package.Price,
                        IsActive = package.IsActive.HasValue ? package.IsActive.Value.ToString() : bool.FalseString,
                        ID = package.ID
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_vendorPackageService.DeletePackage((Int16)id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VendorPackageReport()
        {
            var package = _vendorPackageService.GetAll().ToList();
            if (package.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("PackageReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Name"
                        ,"NameAr"
                        ,"Price"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["PackageReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    if (package.Count != 0)
                        package = package.OrderByDescending(x => x.ID).ToList();

                    foreach (var i in package)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name :"-"
                        ,!string.IsNullOrEmpty(i.NameAr) ? i.NameAr :"-"
                        , i.Price.HasValue ? i.Price.Value : 0
                        ,i.IsActive == true ? "Active" :"InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Package Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }


    }
}