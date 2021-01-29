using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.Models.Identity
{
    public class AppUser : IdentityUser
    {
        [StringLength(50, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 3)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 3)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        // this to contain the languages code, like : "ar, de, em,"
        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Selected Languages")]
        public string SelectedLanguages { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 3)]
        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Birthday")]
        public DateTime Birthday { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 3)]
        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Register Date")]
        public DateTime RegisterDate { get; set; }

        // if a password has been created by the Administrator this property will be true until the user change this password
        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Automated Passwoed")]
        public bool AutomatedPassword { get; set; }

        // each user is just going to have a single address.
        public Address Address { get; set; }

    }
}
