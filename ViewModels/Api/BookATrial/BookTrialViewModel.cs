using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.BookATrial
{
    public class BookTrialViewModel
    {
        public long? CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerContact { get; set; }
        public long? MotorID { get; set; }
        public long? PropertyID { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime BookedDate { get; set; }
        public TimeSpan BookedTime { get; set; }
    }
}