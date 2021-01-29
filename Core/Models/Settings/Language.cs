using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models.Settings
{
    [Table("Languages")]
    public class Language
    {
        [Key]
        [StringLength(10)]
        [Display(Name = "CodeIs")]
        public string CodeId { get; set; }

        [StringLength(50)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        // ltr/rtl
        [StringLength(10)]
        [Display(Name = "Language Direction")]
        public string LanguageDirection { get; set; }

    }
}
