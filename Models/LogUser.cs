using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models
{
    public class LogUser
    {
        [Required(ErrorMessage="C'mon, really?")]
        [EmailAddress(ErrorMessage="Please log in with an email address.")]
        [Display(Name="Email: ")]
        public string Email { get; set; }

        [Required(ErrorMessage="Bruh.")]
        [DataType(DataType.Password)]
        [Display(Name="Password: ")]
        public string Password { get; set; }
    }
}