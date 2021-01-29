using Core.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.Models.Uploads
{
    public class Upload
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [StringLength(100, ErrorMessage = "The {0} must be less than {1} characters.")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Path")]
        public string Path { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        public DateTime AddedDateTime { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        public string UserId { get; set; }
        public AppUser User { get; set; }

    }
}
