using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.ScheduleMeeting
{
    public class ScheduleMeetingViewModel
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "VendorID is required")]
        public int VendorID { get; set; }
        public int CustomerID { get; set; }
        public long? CarID { get; set; }
        public long? PropertyID { get; set; }
        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; }

        [Required(ErrorMessage = "Meeting Date is required")]
        public DateTime MeetingDate { get; set; }
    }
}