using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Vendor
{
    public class VendorPackageViewModel
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string PriceToPay { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string BillingPeriod { get; set; }
        public Nullable<bool> hasPropertyModule { get; set; }
        public Nullable<bool> hasMotorModule { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<long> MotorLimit { get; set; }
        public Nullable<long> PropertyLimit { get; set; }
        public Nullable<bool> IsFree { get; set; }
        public Nullable<int> MonthCount { get; set; }
        public string CostForDaysLeft { get; set; }
        public string CostPerDay { get; set; }
        public int TotalNumberOfDays { get; set; }
        public int NoOfDaysLeft { get; set; }
        public decimal CurrentPackagePrice { get; set; }
        public decimal DemandedPackagePrice { get; set; }
        public bool IsAllowed { get; set; } = true;
        public string PropOverflowMessage { get; set; }
        public string CarOverflowMessage { get; set; }
        public decimal PackagePrice { get; set; }
    }
}