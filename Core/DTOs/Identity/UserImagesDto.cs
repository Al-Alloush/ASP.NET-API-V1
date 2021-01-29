using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.Identity
{
    public class UserImagesDto
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public bool Default { get; set; }

    }
}
