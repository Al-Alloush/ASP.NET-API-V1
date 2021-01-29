using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Blogs
{
    public class BlogCardDto
    {
        public int Id { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Short Title")]
        public string ShortTitle { get; set; }

        [Display(Name = "Publish")]
        public bool Publish { get; set; }

        [Display(Name = "AtTop")]
        public bool AtTop { get; set; }

        [Display(Name = "Release Date")]
        public DateTime ReleaseDate { get; set; }

        [Display(Name = "Blog's Language")]
        public string LanguageId { get; set; }

        public string DefaultBlogImage { get; set; }

        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public int CommentsCount { get; set; }



    }
}
