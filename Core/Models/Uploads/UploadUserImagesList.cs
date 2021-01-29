using Core.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.Models.Uploads
{
    public class UploadUserImagesList
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        public string UserId { get; set; }
        public AppUser User { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        public int UploadId { get; set; }
        public Upload Upload { get; set; }

        public bool Default { get; set; }

        public int UploadTypeId { get; set; }
        public UploadType UploadType { get; set; }
    }
}
