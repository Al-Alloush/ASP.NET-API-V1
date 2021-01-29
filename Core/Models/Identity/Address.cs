using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models.Identity
{
    [Table(name: "Address")]
    public class Address : BaseClass
    {
        [StringLength(50, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 3)]
        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Street")]
        public string Street { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 3)]
        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Building Number")]
        public string BuildingNum { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 3)]
        [Display(Name = "Flore")]
        public string Flore { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 3)]
        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Zip code")]
        public string Zipcode { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 3)]
        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "City")]
        public string City { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be less than {1} and more than {2} characters.", MinimumLength = 3)]
        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Country")]
        public string Country { get; set; }

        // If not required will add new row wit every update and the old one will stay in database but with empty AppUserId
        // this relation is one to one relation
        [Required(ErrorMessage = "{0} is Required")]
        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
