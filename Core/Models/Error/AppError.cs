using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models.Error
{
    [Table(name: "AppError")]
    public class AppError
    {
        public int Id { get; set; }

        [StringLength(255, ErrorMessage = "The {0} must be less than {1} characters.")]
        [Display(Name = "Message")]
        public string Message { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "StatusCode")]
        public int StatusCode { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Details")]
        public string Details { get; set; }

        [Required(ErrorMessage = "{0} is Required")]
        [Display(Name = "Error Type")]
        public int ErrorTypeId { get; set; }
        public ErrorType ErrorType { get; set; }

        [Required]
        public DateTime ErrorDate { get; set; }
    }
}
