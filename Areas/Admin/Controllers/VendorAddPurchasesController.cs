using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class VendorAddPurchasesController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IVendorAddPurchasesService _VendorAddPurchasesService;
        private readonly IVendorService _vendorService;
        private readonly IPropertyService _propService;

        private readonly ICarService _carService;
        public VendorAddPurchasesController (IVendorAddPurchasesService vendorAddPurchasesService, IVendorService vendorService, IPropertyService propertyService,ICarService carservice)
        {
            this._VendorAddPurchasesService = vendorAddPurchasesService;
            _vendorService = vendorService;
            _propService = propertyService;
            _carService = carservice;
        }
        // GET: Admin/VendorAddPurchases
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult List()
        {
            var vendorAddPurchas = _VendorAddPurchasesService.GetAll();
            var List = _vendorService.GetVendors(true);
            var dropdownList = (from item in List.AsEnumerable()
                                select new SelectListItem
                                {
                                    Text = item.Name,
                                    Value = item.ID.ToString()
                                }).ToList();
         

            ViewBag.Vendor = new SelectList(dropdownList, "Value", "Text");
            return PartialView(vendorAddPurchas);
        }
        [HttpPost]
        public ActionResult List(int VendorId)
        {
            var vendorAddPurchas = _VendorAddPurchasesService.GetByVendorId(VendorId);
            var List = _vendorService.GetVendors(true);
            var dropdownList = (from item in List.AsEnumerable()
                                select new SelectListItem
                                {
                                    Text = item.Name,
                                    Value = item.ID.ToString()
                                }).ToList();


            ViewBag.Vendor = new SelectList(dropdownList, "Value", "Text");
            return PartialView(vendorAddPurchas);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var vendorAddPurchas = new VendorAddPurchas();
            var List = _vendorService.GetVendors(true);
            var dropdownList = (from item in List.AsEnumerable()
                                select new SelectListItem
                                {
                                    Text = item.Name,
                                    Value = item.ID.ToString()
                                }).ToList();
            //dropdownList.Add(new SelectListItem() { Text = "Select Vendor", Value = "0" });

            ViewBag.Vendor = new SelectList(dropdownList, "Value", "Text");
            return PartialView(vendorAddPurchas);
        }
        [HttpPost]
        public ActionResult Create(VendorAddPurchas vendorAddPurchas)
        
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                vendorAddPurchas.CreatedBy = (long)Session["AdminUserID"];
                if (_VendorAddPurchasesService.CreatevendorAddPurchas(vendorAddPurchas, ref message))
                {
                    if(vendorAddPurchas.NoOfProperty != null && vendorAddPurchas.NoOfProperty != 0)
                    {
                      
                            _propService.UpdatePropertyStatus((int)vendorAddPurchas.NoOfProperty, (int)vendorAddPurchas.VendorId);


                    }
                    if (vendorAddPurchas.NoOfMotor != null && vendorAddPurchas.NoOfMotor != 0)
                    {

                        _carService.UpdateMotorStatus((int)vendorAddPurchas.NoOfMotor, (int)vendorAddPurchas.VendorId);


                    }
                    vendorAddPurchas.Vendor= _vendorService.GetVendor((long)vendorAddPurchas.VendorId);
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/VendorAddPurchases/Index",
                        message = message,
                        data = new
                        {
                            ID = vendorAddPurchas.Id,
                            Date = vendorAddPurchas.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                            Vendor = vendorAddPurchas.Vendor != null ? vendorAddPurchas.Vendor.Name : "",
                            Price = vendorAddPurchas.Price,
                            NoOfMotor = vendorAddPurchas.NoOfMotor != null ? vendorAddPurchas.NoOfMotor : 0,
                            NoOfProperty = vendorAddPurchas.NoOfProperty != null ? vendorAddPurchas.NoOfProperty : 0,
                            ExpiryDate = vendorAddPurchas.ExpiryDate.Value.ToString("dd MMM yyyy, h: mm tt")
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
        public ActionResult GetVendorDetails(int Id)
        {
            var Vendor = _vendorService.GetVendor(Id);
            return Json(new { success = true,
                data = new
                {
                    PackageName = Vendor.VendorPackage!= null ? Vendor.VendorPackage.Name : "",
                    NoOfMotor = Vendor.VendorPackage != null ? Vendor.VendorPackage.MotorLimit : 0,
                    NoOfProperty = Vendor.VendorPackage != null ? Vendor.VendorPackage.PropertyLimit : 0,
                    hasMotorModule = Vendor.VendorPackage != null ? Vendor.VendorPackage.hasMotorModule : false,
                    hasPropertyModule = Vendor.VendorPackage != null ? Vendor.VendorPackage.hasPropertyModule : false,
                }
               
                 }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Details(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VendorAddPurchas vendorAdd = _VendorAddPurchasesService.GetById(id);
            if (vendorAdd == null)
            {
                return HttpNotFound();
            }
            return PartialView(vendorAdd);
        }

    }
}