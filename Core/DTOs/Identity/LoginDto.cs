using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Identity
{
    public class LoginDto
    {
        [Required(ErrorMessage = "field ({0}) is required")]
        [Display(Name = "Username or Email")]
        public string UserNameOrEmail { get; set; }

        [Required(ErrorMessage = "field ({0}) is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
