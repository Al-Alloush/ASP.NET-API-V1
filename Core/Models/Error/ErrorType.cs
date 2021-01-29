using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models.Error
{
    [Table(name: "ErrorType")]
    public class ErrorType
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [StringLength(50, ErrorMessage = "The {0} must be less than {1} characters.")]
        [Display(Name = "Error Type Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [StringLength(255, ErrorMessage = "The {0} must be less than {1} characters.")]
        [Display(Name = "Type Details")]
        public string Details { get; set; }

        // virtual to achieve lazy loading, get all Errors for ErrorType
        public virtual ICollection<AppError> Errors { get; set; }
    }
}
