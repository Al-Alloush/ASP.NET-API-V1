using Core.Models.Identity;
using Core.Models.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models.Blogs
{

    // BlogCategoryName is the parent of all BlogCategory in any languages
    [Table(name: "BlogSourceCategoryName")]
    public class BlogSourceCategoryName
    {
        public int Id { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be less than {1} characters.")]
        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Name")]
        public string Name { get; set; }

    }

    [Table(name: "BlogCategory")]
    public class BlogCategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Category Name Id")]
        public int SourceCategoryId { get; set; }
        public virtual BlogSourceCategoryName SourceCategory { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 3)]
        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Blog's Language")]
        public string LanguageId { get; set; }
        public virtual Language Language { get; set; }


    }
}
