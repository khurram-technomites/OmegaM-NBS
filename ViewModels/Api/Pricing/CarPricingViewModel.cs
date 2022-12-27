using NowBuySell.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.Pricing
{
    public class CarPricingViewModel
    {
        public List<CarPackage> carPackage { get; set; }

        public long CarID { get; set; }

        public string ChargesType { get; set; }

        public decimal PricePerKilometer { get; set; }

        public decimal DeliveryChargesAmount { get; set; }
    }
}