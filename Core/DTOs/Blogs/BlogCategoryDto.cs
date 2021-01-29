using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Blogs
{
    public class BlogCategoryDto
    {
        [Required(ErrorMessage = "field ({0}) is required")]
        [StringLength(150, ErrorMessage = "The length of the {0} must not be less than {2} letters.")]
        [Display(Name = "Language Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "field ({0}) is required")]
        [Display(Name = "Source Category Id")]
        public int SourceCategoryId { get; set; }

        [Required(ErrorMessage = "field ({0}) is required")]
        [Display(Name = "LanguageId")]
        public string LanguageId { get; set; }

    }
}
