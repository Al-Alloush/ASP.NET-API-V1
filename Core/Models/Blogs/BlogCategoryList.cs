using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.Models.Blogs
{
    public class BlogCategoryList
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Category Name Id")]
        public int BlogId { get; set; }
        public virtual Blog Blog { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Category Name Id")]
        public int BlogCategoryId { get; set; }
        public virtual BlogCategory BlogCategory { get; set; }
    }
}
