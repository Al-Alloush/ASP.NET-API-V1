using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Identity
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "field ({0}) is required")]
        [StringLength(150, ErrorMessage = "The length of the {0} must not be less than {2} letters and be more than {1} letters.", MinimumLength = 9)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "field ({0}) is required")]
        [StringLength(50, ErrorMessage = "The length of the {0} must not be less than {2} letters and be more than {1} letters.", MinimumLength = 6)]
        [Display(Name = "Username")]
        public string UserName { get; set; }


        [Required(ErrorMessage = "field ({0}) is required")]
        [Display(Name = "Birthday")]
        public DateTime Birthday { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Selected Languages")]
        public string SelectedLanguages { get; set; }

        [Required(ErrorMessage = "field ({0}) is required")]
        [StringLength(100, ErrorMessage = "The length of the {0} must not be less than {2} letters and be more than {1} letters.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "field ({0}) is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
