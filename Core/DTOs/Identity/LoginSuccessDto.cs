using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.Identity
{
    public class LoginSuccessDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<UserImagesDto> UserImagesList { get; set; }
        public string Token { get; set; }
    }
}
