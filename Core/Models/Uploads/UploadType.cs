using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models.Uploads
{
    public class UploadType
    {
        public int Id { get; set; }

        // Defult Type: ImageProfile, ImageCover, ImageBlog
        public string Name { get; set; }
    }
}
