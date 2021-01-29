using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.Identity
{
    public class AddressDto
    {
        public string Street { get; set; }

        public string BuildingNum { get; set; }

        public string Flore { get; set; }

        public string Zipcode { get; set; }

        public string City { get; set; }

        public string Country { get; set; }
    }
}
