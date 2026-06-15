using System.ComponentModel.DataAnnotations;

namespace GardenGroupIncidentSystem.Models
{
    // YuChang Huang
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }
    }
}

