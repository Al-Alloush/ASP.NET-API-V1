using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Identity
{
    public class BasePasswordEntityDto
    {
        [Required(ErrorMessage = "field ({0}) is required")]
        [RegularExpression("(?=^.{6,100}$)(?=.*?\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&amp;*()_+}{&quot;:;'?/&gt;.&lt;,])(?!.*\\s).*$", ErrorMessage = "Password must have one Uppercase, one Lowercase and one Alphanumeric character")]
        [StringLength(100, ErrorMessage = "The length of the {0} must not be less than {2} letters and not be more than {1} letters.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "field ({0}) is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordDto : BasePasswordEntityDto
    {
        [Required(ErrorMessage = "field ({0}) is required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "field ({0}) is required")]
        public string Token { get; set; }
    }

    public class UpdatePasswordDto : BasePasswordEntityDto
    {
        [Required(ErrorMessage = "field ({0}) is required")]
        [StringLength(100, ErrorMessage = "The length of the {0} must not be less than {2} letters and not be more than {1} letters.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; }
    }
}
