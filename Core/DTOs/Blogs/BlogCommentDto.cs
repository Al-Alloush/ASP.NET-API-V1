using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.Blogs
{
    public class BlogCommentDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Comment { get; set; }
        public string PathUserProfileImage { get; set; }
    }
}
