using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Error;
using AutoMapper;
using Core.DTOs.Blogs;
using Core.Models.Blogs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.Blogs
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class BlogCategoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BlogCategoryController(AppDbContext context,
                                      IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/BlogCaregoryName/GetSourceBlogCategoryName
        [HttpGet("GetAllSourceBlogCategoryName")]
        public async Task<IEnumerable<BlogSourceCategoryName>> GetAllSourceBlogCategoryName()
        {
            List<BlogSourceCategoryName> categoriesNames = await _context.BlogSourceCategoryName.ToListAsync();

            return categoriesNames;
        }

        // GET api/<BlogCaregoryNameController>/5
        [HttpGet("GetSourceBlogCategoryName")]
        public async Task<BlogSourceCategoryName> GetSourceBlogCategoryName([FromForm] int id)
        {
            BlogSourceCategoryName categoriesName = await _context.BlogSourceCategoryName.Where(c => c.Id == id).FirstOrDefaultAsync();

            return categoriesName;

        }

        [HttpPut("PutSourceCategoryName")]
        public async Task<ActionResult<string>> PutSourceCategoryName([FromForm] BlogSourceCategoryDto sourcCate)
        {
            // check if this Source Name existing before
            var sourceName = await _context.BlogSourceCategoryName.FirstOrDefaultAsync(n => n.Name == sourcCate.Name);
            if (sourceName != null) return BadRequest(new ApiResponse(400, $"This {sourcCate.Name} category existing before!"));

            // check if this name not exist
            var name = await _context.BlogSourceCategoryName.FirstOrDefaultAsync(n => n.Id == sourcCate.Id);
            if (name != null)
            {
                name.Name = sourcCate.Name;

                _context.Entry(name).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(new ApiResponse(201, $"Update {sourcCate.Name} Source BlogCategory Successfully")); 
            }
            else if(name==null &&  sourcCate.Id != null  )
            {   // if user add id not exist
                return BadRequest(new ApiResponse(400, $"This Id {sourcCate.Id} not existing, If you want add new Source Blog's Category Set Id is empty or null!"));
            }
            else
            {
                var newSC = new BlogSourceCategoryName
                {
                    Name = sourcCate.Name
                };

                await _context.BlogSourceCategoryName.AddAsync(newSC);
                await _context.SaveChangesAsync();
                return Ok(new ApiResponse(201, $"Add {sourcCate.Name} Source BlogCategory successfully")); 
            }

        }

        [HttpDelete("DeleteSourceCategoryName")]
        public async Task<ActionResult<string>> DeleteSourceCategoryName([FromForm] int id)
        {
            // check if this name not exist
            var name = await _context.BlogSourceCategoryName.FirstOrDefaultAsync(n => n.Id == id);
            if (name == null) return BadRequest(new ApiResponse(400, "This category not exist!"));

            _context.Remove(name);
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse(201)); 
        }

        [HttpGet("GetAllCategoryNamesByLang")]
        public async Task<ActionResult<List<BlogCategoryDto>>> GetAllCategoryNamesByLanguage([FromForm] string lang)
        {
            List<BlogCategory> categories = await _context.BlogCategory.Where(b => b.LanguageId == lang).ToListAsync();

            var _categories = _mapper.Map<List<BlogCategory>, List<BlogCategoryDto>>(categories);

            return _categories;
        }

        [HttpGet("GetCategoryNames")]
        public async Task<ActionResult<BlogCategoryDto>> GetCategoryNames([FromForm] int id, [FromForm] string lang)
        {
            var category = await _context.BlogCategory.FirstOrDefaultAsync(b => b.LanguageId == lang && b.SourceCategoryId == id);

            var _category = _mapper.Map<BlogCategory, BlogCategoryDto>(category);
            return _category;
        }


        [HttpPut("PutBlogCategory")]
        public async Task<ActionResult<string>> PutBlogCategory( [FromForm] BlogCategoryDto category)
        {
            // check if Source BogCategory existing
            var sourceCategory = await _context.BlogSourceCategoryName.Select(s => s.Id).ToListAsync();
            if (!sourceCategory.Contains(category.SourceCategoryId))
                return BadRequest(new ApiResponse(400, "the Source Id of Blog's Category not Existing"));

            // check if this category existing
            var _category = await _context.BlogCategory.FirstOrDefaultAsync(c => c.SourceCategoryId == category.SourceCategoryId &&
                                                                            c.LanguageId == category.LanguageId);
            if (_category != null)
            {
                _category.Name = category.Name;

                _context.Entry(_category).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(new ApiResponse(201, $" Update {category.Name} Blog Category successfully")); 
            }
            else
            {
                var blogCategory = new BlogCategory
                {
                    Name = category.Name,
                    SourceCategoryId = category.SourceCategoryId,
                    LanguageId = category.LanguageId
                };
                await _context.BlogCategory.AddAsync(blogCategory);
                await _context.SaveChangesAsync();
                return Ok(new ApiResponse(201, $" Add {category.Name} Blog Category successfully"));
            }
        }

        [HttpDelete("DeleteBlogCategory")]
        public async Task<ActionResult<string>> DeleteBlogCategory([FromForm] int id)
        {
            // check if this name not exist
            var name = await _context.BlogCategory.FirstOrDefaultAsync(n => n.Id == id);
            if (name == null) return BadRequest(new ApiResponse(400, "This category not exist!"));

            _context.Remove(name);
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse(201));
        }
    }
}
