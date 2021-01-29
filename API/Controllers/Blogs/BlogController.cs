using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Error;
using API.Helppers;
using AutoMapper;
using Core.DTOs.Blogs;
using Core.Models.Blogs;
using Core.Models.Identity;
using Core.Models.Uploads;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.Blogs
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, Admin, Editor")]
    public class BlogController : ControllerBase
    {
        private const string BLOG_IMAGE_DIRECTORY = "/Uploads/Images/";

        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BlogController(UserManager<AppUser> userManager,
                                AppDbContext context,
                                IMapper mapper,
                                IWebHostEnvironment webHostEnvironment)
        {
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("GetAllBlogCardList")]
        public async Task<ActionResult<Pagination<BlogCardDto>>> GetAllBlogCardList([FromForm] SpecificParameters par)
        {
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));


            /// _context.Blog.OrderByDescending(b => b.ReleaseDate).Take(N) to get the last N rows 
            // query just Publiched Blogs
            IQueryable<Blog> blogs;
            if (par.CategoryId != null)
                // det all blogs that have the specific category id
                blogs = from b in _context.Blog.OrderByDescending(b => b.ReleaseDate).Take(5000)
                        join c in _context.BlogCategoryList on b.Id equals c.BlogId
                        where c.BlogCategoryId == par.CategoryId && b.Publish == true
                        select b;
            else
                // return all blogs
                blogs = from b in _context.Blog.OrderByDescending(b=>b.ReleaseDate).Take(5000)
                        where b.Publish == true
                        select b;




            // get the Blogs with the same languages were user selected
            var langList = user.SelectedLanguages.Split(",").ToList();
            // create a new variable to add all Blogs with specific language that passed with langList variable
            IQueryable<Blog> langBlog = blogs.Where(l => l.LanguageId == langList[0]);
            foreach (var lang in langList)
            {
                // langList[0] has query with initialze
                if (lang != langList[0] && !string.IsNullOrEmpty(lang))
                    // Concatenates old query with new one
                    langBlog = blogs.Concat(blogs.Where(l => l.LanguageId == lang));
            }
            blogs = langBlog;


            // At the beginning of the sorting, the blogs are placed to remain at the top and then sorted by title or date of issue
            if (!string.IsNullOrEmpty(par.Sort))
            {
                switch (par.Sort)
                {
                    case "titleAsc":
                        blogs = blogs.OrderByDescending(b => b.AtTop).ThenBy(b => b.Title);
                        break;
                    case "titleDesc":
                        blogs = blogs.OrderByDescending(b => b.AtTop).ThenByDescending(b => b.Title);
                        break;
                    case "dateAsc":
                        blogs = blogs.OrderByDescending(b => b.AtTop).ThenBy(b => b.ReleaseDate);
                        break;
                }
            }
            else
                blogs = blogs.OrderByDescending(b => b.AtTop).ThenByDescending(b => b.ReleaseDate);


            // Search 
            if (!string.IsNullOrEmpty(par.Search))
            {
                // Search in Titles
                blogs = from bl in blogs
                        where bl.Title.ToLower().Contains(par.Search)
                        select bl;
            }

            int totalItem = blogs.Count();

            var pageFilter = new SpecificParameters(par.PageIndex, par.PageSize);
            // .Skip() and .Take() It must be at the end in order for pages to be created after filtering and searching
            List<Blog> _blogs = await blogs /*  how many do we want to Skip:
                                                minus one here because we want start from 0, PageSize=5 (PageIndex=1 - 1)=0
                                                5x0=0 this is start page*/
                                            .Skip((pageFilter.PageIndex - 1) * pageFilter.PageSize)
                                            .Take(pageFilter.PageSize)
                                            .ToListAsync();

            // if pass page not contain any data return Bad Request
            if (_blogs.Count() <= 0)
                return NoContent();

            List<BlogCardDto> blogsData = _mapper.Map<List<Blog>, List<BlogCardDto>>(_blogs);

            // add some data in return BlogCardDto class**************
            // get the baseurl(Domain) of website
            var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            foreach (var blog in blogsData)
            {
                // count Likes/dislikes of this Blogs
                blog.LikesCount = _context.BlogLike.Where(b => b.Like == true && b.BlogId == blog.Id).Count();
                blog.DislikesCount = _context.BlogLike.Where(b => b.Dislike == true && b.BlogId == blog.Id).Count();
                // count the commints
                blog.CommentsCount = _context.BlogComment.Where(b => b.BlogId == blog.Id).Count();

                // return default image for this blog
                var defaultImage = await _context.UploadBlogImagesList.Where(b => b.BlogId == blog.Id && b.Default == true).Select(img => img.Upload.Path).FirstOrDefaultAsync();
                blog.DefaultBlogImage = defaultImage == null ? null : url + defaultImage;
            }

            return new Pagination<BlogCardDto>(par.PageIndex, par.PageSize, totalItem, blogsData);
        }


        [HttpGet("GetBlogDetails")]
        public async Task<ActionResult<BlogDto>> GetBlogDetails([FromForm] int id)
        {
            // get blog if Published
            var blog = await _context.Blog.Include(c => c.BlogCategoriesList)
                                           .Include(c => c.BlogComments)
                                           .FirstOrDefaultAsync(b => b.Id == id && b.Publish == true);
            // check id Blog existing
            if (blog == null) return NotFound(new ApiResponse(404));


            // if (blog == null) return NotFound(new ApiResponse(404));
            var _blog = _mapper.Map<Blog, BlogDto>(blog);
            // return like and Dislike count with Blog info

            _blog.LikesCount = _context.BlogLike.Where(b => b.Like == true && b.BlogId == blog.Id).Count();
            _blog.DislikesCount = _context.BlogLike.Where(b => b.Dislike == true && b.BlogId == blog.Id).Count();
            _blog.CommentsCount = _context.BlogComment.Where(b => b.BlogId == blog.Id).Count();

            // get category name with same blog language
            foreach (var item in _blog.BlogCategoriesList)
                item.Name = await _context.BlogCategory.Where(c => c.SourceCategoryId == item.BlogCategoryId && c.LanguageId == blog.LanguageId).Select(c => c.Name).FirstOrDefaultAsync();

            // get the baseurl(Domain) of website
            var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            // get the username of Commenter
            foreach (var item in _blog.BlogComments)
            {
                var blogUsreId = await _context.BlogComment.Where(c => c.Id == item.Id).Select(c => c.UserId).FirstOrDefaultAsync();
                item.UserName = await _context.Users.Where(c => c.Id == blogUsreId).Select(c => c.UserName).FirstOrDefaultAsync();
            }
            // get the User Image who has commint for this blog
            IQueryable<BlogImageDto> images = from bi in _context.UploadBlogImagesList
                                              join up in _context.Upload on bi.UploadId equals up.Id
                                              join typ in _context.UploadType on bi.UploadTypeId equals typ.Id
                                              where bi.BlogId == _blog.Id && bi.UploadTypeId == 3
                                              select new BlogImageDto
                                              {
                                                  Id = bi.Id,
                                                  Path = url + up.Path,
                                                  Default = bi.Default,
                                                  Type = typ.Name
                                              };
            _blog.BlogImagesList = await images.ToListAsync();



            return _blog;
        }

        // Post: Create a Blog
        [HttpPost("CreateBlog")]
        public async Task<ActionResult<string>> CreateBlog([FromForm] BlogCreateDto blog)
        {
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            try
            {
                //check id category existing, example: Convert string "[1, 2, 3]" to int list
                List<int> _categoriesIds = blog.Categories.Trim('[', ']').Split(',').Select(int.Parse).ToList();
                foreach (var id in _categoriesIds)
                {
                    var category = await _context.BlogCategory.FindAsync(id);
                    if (category == null) return NotFound(new ApiResponse(404, "Category Id:" + id + ", Not exist!"));
                }

                var newBlog = _mapper.Map<BlogCreateDto, Blog>(blog);
                newBlog.UserId = user.Id;
                newBlog.AddedDateTime = DateTime.Now;

                await _context.Blog.AddAsync(newBlog);
                await _context.SaveChangesAsync();

                // Add Categories
                foreach (var id in _categoriesIds)
                {
                    var newCat = new BlogCategoryList
                    {
                        BlogId = newBlog.Id,
                        BlogCategoryId = id
                    };
                    await _context.BlogCategoryList.AddAsync(newCat);
                }
                await _context.SaveChangesAsync();

                var defaultImage = true;
                if (blog.Files.Count > 0)
                {
                    foreach (var img in blog.Files)
                    {
                        if (img != null && img.Length > 0)
                        {
                            // this path webHostEnvironment.WebRootPath is under wwwroot folder
                            string filePath = Path.Combine(_webHostEnvironment.WebRootPath + BLOG_IMAGE_DIRECTORY);
                            if (!Directory.Exists(filePath))
                                Directory.CreateDirectory(filePath);

                            string fileName = (Guid.NewGuid().ToString().Substring(0, 8)) + "_" + img.FileName;
                            filePath = Path.Combine(filePath, fileName);

                            using (FileStream fileStream = System.IO.File.Create(filePath))
                            {
                                img.CopyTo(fileStream);
                                fileStream.Flush();

                                var upload = new Upload
                                {
                                    Name = fileName,
                                    Path = BLOG_IMAGE_DIRECTORY + fileName,
                                    AddedDateTime = DateTime.Now,
                                    UserId = user.Id
                                };
                                await _context.Upload.AddAsync(upload);
                                await _context.SaveChangesAsync();

                                var imgBlog = new UploadBlogImagesList
                                {
                                    UploadId = upload.Id,
                                    BlogId = newBlog.Id,
                                    UploadTypeId = 3,
                                    Default = defaultImage
                                };
                                // to set the first image as a default image
                                defaultImage = false;

                                await _context.UploadBlogImagesList.AddAsync(imgBlog);
                                await _context.SaveChangesAsync();

                                
                            }

                        }
                    }
                    return Ok(new ApiResponse(201)); // Successfully Add Blog, Uploaded Image successfully.");
                }
                return Ok("Add Blog successfully, but No File to Upload it!");
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, $"Error: {ex.Message}", true,
                    "somthin wrong! in BlogCreate Action!", 1,
                    new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName(),/* Get File Name */
                    (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            }
        }

        // Put: update the Blog
        [HttpPut("UpdateBlog")]
        public async Task<ActionResult<string>> UpdateBlog([FromForm]BlogUpdateDto blog)
        {
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            // check id Blog existing
            var _blog = await _context.Blog.FindAsync(blog.Id);
            if (_blog == null) return NotFound(new ApiResponse(404));

            //check id category existing, example: Convert string "[1, 2, 3]" to int list
            List<int> _categoriesIds = blog.Categories.Trim('[', ']').Split(',').Select(int.Parse).ToList();
            foreach (var id in _categoriesIds)
            {
                var category = await _context.BlogCategory.FindAsync(id);
                if (category == null) return NotFound(new ApiResponse(404, "Category Id:" + id + ", Not exist!"));
            }

            // check the Permission
            var permission = await PermissionsManagement(user, _blog);
            if(!permission)
                return BadRequest(new ApiResponse(400, "current User doesn't has a permation to update this blog"));

            try
            {
                // return Blog class (_blog)
                _mapper.Map(blog, _blog);
                _context.Entry(_blog).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // update Bog's Categories after delete old categories
                var oldCats = await _context.BlogCategoryList.Where(c => c.BlogId == _blog.Id).ToArrayAsync();
                foreach (var oldCat in oldCats)
                    _context.Remove(oldCat);

                await _context.SaveChangesAsync();

                // Add new Categories
                foreach (var id in _categoriesIds)
                {
                    var newCat = new BlogCategoryList
                    {
                        BlogId = _blog.Id,
                        BlogCategoryId = id
                    };
                    await _context.BlogCategoryList.AddAsync(newCat);
                }
                await _context.SaveChangesAsync();

                return Ok($"Update Blog Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, $"Error: {ex.Message}"));
            }
        }


        // Post: Add New Blog's Image
        [HttpPost("AddNewBlogImages")]
        public async Task<ActionResult<string>> AddNewBlogImages([FromForm] BlogAddImageDto blog)
        {
            var defaultImage = true;
            if (blog.Files.Count > 0)
            {
                foreach (var img in blog.Files)
                {
                    if (img != null && img.Length > 0)
                    {

                        var user = await GetCurrentUserAsync(HttpContext.User);
                        if (user == null) return Unauthorized(new ApiResponse(401));

                        // check id Blog existing
                        var _blog = await _context.Blog.FindAsync(int.Parse(blog.BlogId));
                        if (_blog == null) return NotFound(new ApiResponse(404));

                        // check the Permission
                        var permission = await PermissionsManagement(user, _blog);
                        if (!permission)
                            return BadRequest(new ApiResponse(400, "current User doesn't has a permation to Add any images"));

                        try
                        {
                            // this path webHostEnvironment.WebRootPath is under wwwroot folder & BLOG_IMAGE_DIRECTORY is where Blog Image Folder
                            string filePath = Path.Combine(_webHostEnvironment.WebRootPath + BLOG_IMAGE_DIRECTORY);
                            if (!Directory.Exists(filePath))
                                Directory.CreateDirectory(filePath);

                            //check if blog has Default image before.
                            var image = _context.UploadBlogImagesList.FirstOrDefaultAsync(img => img.Default == true && img.BlogId == _blog.Id);
                            if (image != null)
                                defaultImage = false;

                            // Create uniqu file name to avoid overwrite old image with same name
                            string fileName = (Guid.NewGuid().ToString().Substring(0, 8)) + "_" + img.FileName;
                            filePath = Path.Combine(filePath, fileName);
                            using (FileStream fileStream = System.IO.File.Create(filePath))
                            {
                                img.CopyTo(fileStream);
                                // Clears buffers for this stream and causes any buffered data to be written to the file.
                                fileStream.Flush();

                                var upload = new Upload
                                {
                                    Name = fileName,
                                    Path = BLOG_IMAGE_DIRECTORY + fileName,
                                    AddedDateTime = DateTime.Now,
                                    UserId = user.Id
                                };
                                await _context.Upload.AddAsync(upload);
                                await _context.SaveChangesAsync();

                                var imgBlog = new UploadBlogImagesList
                                {
                                    UploadId = upload.Id,
                                    BlogId = _blog.Id,
                                    UploadTypeId = 3,
                                    Default = defaultImage
                                };
                                // if blog has no default image before set the first one as defautl image
                                defaultImage = false;

                                await _context.UploadBlogImagesList.AddAsync(imgBlog);
                                await _context.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            return BadRequest(new ApiResponse(400, $"Error: {ex.Message}"));
                        }
                    }
                }
                return Ok(new ApiResponse(201)); // Successfully Uploaded Image 
            }
            return BadRequest(new ApiResponse(400," No Images for Upload!"));
        }

        [HttpDelete("DeleteBlogImage")]
        public async Task<ActionResult<string>> DeleteBlogImage([FromForm] int imageId)
        {

            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            // check if image existing in UploadBlogImagesList
            var image = await _context.UploadBlogImagesList.FirstOrDefaultAsync(i => i.Id == imageId);
            if (image == null) return NotFound(new ApiResponse(404));

            // check id Blog existing
            var _blog = await _context.Blog.FindAsync(image.BlogId);
            if (_blog == null) return NotFound(new ApiResponse(404));

            // check if upload existing
            var upload = await _context.Upload.FindAsync(image.UploadId);
            if (upload == null) return NotFound(new ApiResponse(404));

            // check the Permission
            var permission = await PermissionsManagement(user, _blog);
            if (!permission)
                return BadRequest(new ApiResponse(400, "current User doesn't has a permation to Delete this Image"));

            _context.Remove(upload);
            await _context.SaveChangesAsync();
            // delete Image file from server
            System.IO.File.Delete(_webHostEnvironment.WebRootPath + upload.Path);
            return Ok(new ApiResponse(201)); // Successfully Delete Image
        }

        // Delete: Delete the Blog
        [HttpDelete("DeleteBlog")]
        public async Task<ActionResult<string>> DeleteBlog([FromForm]int id)
        {
            Blog _blog = await _context.Blog.FindAsync(id);
            if (_blog == null) return Ok($"this Blog not exsit!");

            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            // check the Permission
            var permission = await PermissionsManagement(user, _blog);
            if (!permission)
                return BadRequest(new ApiResponse(400, "current User doesn't has a permation to Delete this Blog"));

            try
            {
                // getting the upload Ids to delete them from Upload Table then delete images files in the server after deleting the Blog
                var imagesList = await _context.UploadBlogImagesList.Where(b => b.BlogId == id).ToListAsync();
                List<Upload> uploads = new List<Upload>();
                foreach (var img in imagesList)
                {
                    var upload = await _context.Upload.FirstOrDefaultAsync(u => u.Id == img.UploadId);
                    uploads.Add(upload);
                }

                // that the entities that relate to this Blog like Comments, Likes, and Images are deleted when the Blog is deleted. Referential Action is CASCADE
                _context.Remove(_blog);
                await _context.SaveChangesAsync();

                // Delete all Uploads from Upload table and delete upload image from the server.
                foreach (var upload in uploads)
                {
                    // delete all upload rows
                    _context.Remove(upload);
                    await _context.SaveChangesAsync();
                    //  delete image files from server
                    System.IO.File.Delete(_webHostEnvironment.WebRootPath + upload.Path);
                }
                return Ok(new ApiResponse(201)); // Successfully Delete Blog 
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, $"Error: {ex.Message}"));
            }
        }

        private async Task<bool> PermissionsManagement(AppUser currentUser, Blog blog)
        {
            var superAdmin = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var superAdminId = superAdmin.Select(u => u.Id).FirstOrDefault();
            // if current user is SuperAdmin update this blog
            if (currentUser.Id != superAdminId)
            {
                // delete this iamge if current user created this Blog.
                if (blog.UserId != currentUser.Id)
                {
                    // get Blog creater's Role
                    var currentUserRole = (await _userManager.GetRolesAsync(currentUser)).FirstOrDefault();

                    var blogCreater = await _userManager.FindByIdAsync(blog.UserId);
                    var blogCreaterRole = (await _userManager.GetRolesAsync(blogCreater)).FirstOrDefault();

                    if ((currentUserRole == "Admin" && blogCreaterRole == "Admin") ||
                        (currentUserRole == "Admin" && blogCreaterRole == "SuperAdmin") ||
                        (currentUserRole == "Editor" && blogCreaterRole == "Editor") ||
                        (currentUserRole == "Editor" && blogCreaterRole == "Admin") ||
                        (currentUserRole == "Editor" && blogCreaterRole == "SuperAdmin"))
                        return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Inside WebToken there is an Email, from this email Get User from user associated on controller and HttpContext absract class.
        /// </summary>
        /// <returns>
        /// If ClaimsPrincipal httpContextUser = HttpContext.User; retrun an User Object 
        /// else retrun null</returns> 
        private async Task<AppUser> GetCurrentUserAsync(ClaimsPrincipal httpContextUser)
        {
            if (httpContextUser != null)
            {
                // get an email form Token Claim, that has been added in TockenServices.cs
                var email = httpContextUser?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
                var user = await _userManager.Users.Include(x => x.Address).SingleOrDefaultAsync(x => x.Email == email);
                return user;
            }
            else
            {
                return null;
            }
        }

    }
}
