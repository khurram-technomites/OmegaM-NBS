using System.Collections.Generic;

namespace NowBuySell.Web.ViewModels.Car
{
    public class GetInTouchModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int PhoneNo { get; set; }
        public string Email { get; set; }
        public string Comments { get; set; }
        public System.DateTime CreatedOn { get; set; } 
        public bool MarkRead { get; set; }

    }
}