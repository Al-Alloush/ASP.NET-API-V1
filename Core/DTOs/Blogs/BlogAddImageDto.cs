using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.Blogs
{
    public class BlogAddImageDto
    {
        public List<IFormFile> Files { get; set; }
        public string BlogId { get; set; }
    }
}
