using Core.Models.Settings;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Blogs
{
    public class BlogCreateDto
    {

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

        [Display(Name = "Commentable")]
        public bool Commentable { get; set; }

        [Display(Name = "AtTop")]
        public bool AtTop { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Release Date")]
        public DateTime ReleaseDate { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Blog's Language")]
        public string LanguageId { get; set; }

        [Display(Name = "UserId")]
        public string UserId { get; set; }

        public string Categories { get; set; }

        public List<IFormFile> Files { get; set; }
    }
}
