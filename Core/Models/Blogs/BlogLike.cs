using Core.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models.Blogs
{ 
    [Table(name: "BlogLike")]
    public class BlogLike
    {
       
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        public DateTime AddedDateTime { get; set; }

        public bool Like { get; set; }
        public bool Dislike { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        public int BlogId { get; set; }
        public virtual Blog Blog { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        public string UserId { get; set; }
        public virtual AppUser User { get; set; }
    }
}
