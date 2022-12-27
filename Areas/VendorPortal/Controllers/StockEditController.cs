using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Areas.VendorPortal.ViewModels;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Linq;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
    [AuthorizeVendor]
    public class StockEditController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ICarService _carService;

        public StockEditController(ICarService carService)
        {
            this._carService = carService;
        }
        // GET: VendorPortal/StockEdit
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult List()
		{
            var vendorId = Convert.ToInt64(Session["VendorID"]);
            var car = _carService.GetVendorCarsByVendorID(vendorId);
            return PartialView(car);
		}
        [HttpPost]
        public ActionResult EditStock(StockEditViewModel stockViewModel)
        {
            string message = string.Empty;
            var car = _carService.GetCar(stockViewModel.ID);
            car.RegularPrice = stockViewModel.RegularPrice;
            car.SalePrice = stockViewModel.SalePrice;
            car.Stock = stockViewModel.Stock;
            if (_carService.UpdateCar(ref car, ref message))
			{
                return Json(new
                {
                    success = true,
                    //url = "/Admin/Car/Index",
                    message = "Car updated successfully ...",
                    data = new
                    {
                        ID = car.ID,
                        Date = car.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        SKU = car.SKU,
                        Name = car.Name,
                        RegularPrice = car.RegularPrice,
                        SalePrice = car.SalePrice,
                        Stock = car.Stock,
                       // IsActive = car.IsActive.HasValue ? car.IsActive.Value.ToString() : bool.FalseString
                    }
                });
            }
            else
            {
                message = "Please fill the form properly ...";
            }
            return Json(new { success = false, message = message });
        }
    }
}