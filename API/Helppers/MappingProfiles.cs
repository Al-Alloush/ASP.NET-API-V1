using AutoMapper;
using Core.DTOs.Blogs;
using Core.DTOs.Identity;
using Core.Models.Blogs;
using Core.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helppers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Identity
            CreateMap<RegisterDto, AppUser>();
            CreateMap<AppUser, LoginSuccessDto>();
            CreateMap<AppUser, UserDto>();
            CreateMap<Address, AddressDto>();
            CreateMap<AddressDto, Address>();
            CreateMap<Address, AddressDto>();

            // Blogs
            CreateMap<Blog, BlogCardDto>();
            CreateMap<BlogCardDto, Blog>();
            CreateMap<Blog, BlogDto>();
            CreateMap<BlogDto, Blog>();
            CreateMap<BlogCreateDto, Blog>();
            CreateMap<Blog, BlogCreateDto>();
            CreateMap<BlogCategoryList, BlogCategoryListDto>();
            CreateMap<BlogCategoryListDto, BlogCategoryList>();
            CreateMap<BlogComment, BlogCommentDto>();
            CreateMap<BlogCommentDto, BlogComment>();
            CreateMap<BlogCategory, BlogCategoryDto>();
            CreateMap<BlogCategoryDto, BlogCategory>();
            CreateMap<BlogUpdateDto, Blog>();
            CreateMap<Blog, BlogUpdateDto>();
        }
    }
}
