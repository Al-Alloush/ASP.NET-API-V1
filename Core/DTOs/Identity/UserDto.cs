using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.Identity
{
    public class UserDto
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTime Birthday { get; set; }

        public AddressDto Address { get; set; }

        public List<UserImagesDto> UserImagesList { get; set; }

    }
}
