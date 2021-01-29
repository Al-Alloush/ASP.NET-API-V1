using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Blogs
{
    public class BlogSourceCategoryDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "field ({0}) is required")]
        [StringLength(100, ErrorMessage = "The length of the {0} must not be less than {2} letters.")]
        [Display(Name = "Name")]
        public string Name { get; set; }

    }
}
