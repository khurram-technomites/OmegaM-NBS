using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.VendorUser
{
    public class VendorUserProfileViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string MobileNo { get; set; }
    }
}