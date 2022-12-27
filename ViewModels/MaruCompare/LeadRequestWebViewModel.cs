using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.MaruCompare
{
    public class LeadRequestWebViewModel
    {

        [Required(ErrorMessage = "The Car ID is required")]
        public long ServiceCarID { get; set; }

        public long? CustomerID { get; set; }

        [Required(ErrorMessage = "The Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The Email Address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The Company is required")]
        public string Company { get; set; }

        [Required(ErrorMessage = "The Nationality is required")]
        public string Nationality { get; set; }

        [Required(ErrorMessage = "The Phone is required")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "The Address is required")]
        public string Address { get; set; }

    }
}