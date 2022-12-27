using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.GetInTouch
{
    public class GetInTouchViewModel
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string PhoneNo { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Comments { get; set; }
        public Nullable<bool> MarkRead { get; set; }
        [Required]
        public int VendorID { get; set; }
        public Nullable<long> CarID { get; set; }
        public Nullable<long> PropertyID { get; set; }
    }
}