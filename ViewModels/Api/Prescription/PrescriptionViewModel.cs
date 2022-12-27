using System.ComponentModel.DataAnnotations;

namespace NowBuySell.Web.ViewModels.Api.Prescription
{
    public class PrescriptionViewModel
    {
        [Required]
        public string Description { get; set; }
    }
}