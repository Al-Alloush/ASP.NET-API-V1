using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Blogs
{
    public class BlogUpdateDto
    {
        public int Id { get; set; }

        [StringLength(255, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 3)]
        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 3)]
        [Display(Name = "Short Title")]
        public string ShortTitle { get; set; }

        [StringLength(5000, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 10)]
        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Body")]
        public string Body { get; set; }

        [Display(Name = "Publish")]
        public bool Publish { get; set; }

        [Display(Name = "AtTop")]
        public bool AtTop { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Release Date")]
        public DateTime ReleaseDate { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Blog's Language")]
        public string LanguageId { get; set; }

        public string Categories { get; set; }

    }
}
