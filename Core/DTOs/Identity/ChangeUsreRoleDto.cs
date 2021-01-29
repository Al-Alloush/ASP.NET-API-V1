using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Identity
{
    public class ChangeUsreRoleDto
    {
        [Required(ErrorMessage = "field ({0}) is required")]
        [Display(Name = "Username or Email")]
        public string UserNameOrEmail { get; set; }

        [Required(ErrorMessage = "field ({0}) is required")]
        [Display(Name = "Role")]
        public string Role { get; set; }
    }
}
