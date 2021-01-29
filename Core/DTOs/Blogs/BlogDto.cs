using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Blogs
{
    public class BlogDto
    {
        public int Id { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Short Title")]
        public string ShortTitle { get; set; }

        [Display(Name = "Body")]
        public string Body { get; set; }

        [Display(Name = "Language Id")]
        public string LanguageId { get; set; }

        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public int CommentsCount { get; set; }

        public List<BlogImageDto> BlogImagesList { get; set; }
        public List<BlogCategoryListDto> BlogCategoriesList { get; set; }
        public List<BlogCommentDto> BlogComments { get; set; }

        
    }
}
