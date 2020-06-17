using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage="First name is required.")]
        [MinLength(2, ErrorMessage="First name must be at least 2 characters.")]
        [MaxLength(20, ErrorMessage="Server space isn't free, you know. Please keep first name to 20 characters.")]
        [Display(Name="First Name: ")]
        public string FirstName { get; set; }

        [Required(ErrorMessage="Last name is required.")]
        [MinLength(2, ErrorMessage="Last name must be at least 2 characters.")]
        [MaxLength(40, ErrorMessage="Server space isn't free, you know. Please keep last name to 40 characters.")]
        [Display(Name="Last Name: ")]
        public string LastName { get; set; }

        [Required(ErrorMessage="Email is required.")]
        [EmailAddress(ErrorMessage="Invalid email address")]
        [Display(Name="Email: ")]
        public string Email { get; set; }

        [Required(ErrorMessage="Password is required.")]
        [MinLength(8, ErrorMessage="Password must be at least 8 characters.")]
        [RegularExpression("^(?=.*[0-9])(?=.*[a-zA-Z])([a-zA-Z0-9]+)$", ErrorMessage="Password must contain at least 1 letter and 1 number")]
        [Display(Name="Password: ")]
        [DataType(DataType.Password)]
        [Compare("Confirm", ErrorMessage="Passwords do not match.")]
        public string Password { get; set; }

        [NotMapped]
        [Display(Name="Confirm Password: ")]
        [DataType(DataType.Password)]
        public string Confirm { get; set; }

        public List<Wedding> WeddingsPlanned { get; set; }

        public List<RSVP> WeddingsAttending { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}