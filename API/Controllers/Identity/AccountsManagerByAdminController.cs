using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Error;
using API.Helppers;
using AutoMapper;
using Core.DTOs.Identity;
using Core.Models.Identity;
using Infrastructure.Data;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.Identity
{
    [ApiController]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Route("api/[Controller]")]
    public class AccountsManagerByAdminController : ControllerBase
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly GlobalFunctions _globalFunc;
        private readonly AppDbContext _context;

        public AccountsManagerByAdminController(UserManager<AppUser> userManager,
                                                RoleManager<IdentityRole> roleManager,
                                                IMapper mapper,
                                                GlobalFunctions globalFunc,
                                                AppDbContext context)
        {
            _roleManager = roleManager;
            _mapper = mapper;
            _globalFunc = globalFunc;
            _context = context;
            _userManager = userManager;
        }

        private async Task<string> SuperAdminId()
        {
            // app has just one SuperAdmin, we don't want retrun it with any query
            List<AppUser> superAdmin = new List<AppUser>(await _userManager.GetUsersInRoleAsync("SuperAdmin"));
            return superAdmin[0].Id; 
        }

        [HttpGet("GetUsersList")]
        public async Task<ActionResult<Pagination<UserDto>>> GetUsersList([FromForm] SpecificParameters par)
        {
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            var pageFilter = new SpecificParameters(par.PageIndex, par.PageSize);
            IQueryable<AppUser> users;

            // app has just one SuperAdmin, we don't want retrun it with any query
            string superAdminId = await SuperAdminId();

            // check if current user is SuperAdmin
            var currentUserRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            if (currentUserRole == "SuperAdmin")
                users = _context.Users;// return all user include SuperAdmin user
            else
                users = _context.Users.Where(u => u.Id != superAdminId); // return all user without SuperAdmin user

            try
            {
                if (!string.IsNullOrEmpty(par.Search))
                    if (par.Search.Length > 2)
                    {
                        // Search in UserName
                        if ( !string.IsNullOrEmpty(par.Search) && !string.IsNullOrEmpty(par.SearchInColumnName))
                        {
                            if (par.SearchInColumnName.ToLower() == "email")
                            {
                                // Search in UserName
                                users = from u in users
                                        where u.Email.ToLower().Contains(par.Search)
                                        select u;
                            }
                            else if(par.SearchInColumnName.ToLower() == "username")
                            {
                                // Search in UserName
                                users = from u in users
                                        where u.UserName.ToLower().Contains(par.Search)
                                        select u;
                            
                            }
                            
                        }
                    }

                // to get users under specific Roles
                if (!string.IsNullOrEmpty(par.Role))
                {
                    // request directly to the EF Database to get users with this Role
                    users = from usr in users
                            join userRole in _context.UserRoles on usr.Id equals userRole.UserId
                            join role in _context.Roles on userRole.RoleId equals role.Id
                            where role.Name.ToLower() == par.Role.ToLower()
                            select usr;
                }


                int totalItem = users.Count();

                // .ToListAsync() Convert the IQueryable<Entty> to List<Entity> and executing in the database
                // .Skip() and Take() It must be at the end in order for pages to be created after filtering and searching
                List<AppUser> _users = await users.Include(u=>u.Address)
                                            /*  how many do we want to Skip:
                                                minus one here because we want start from 0, PageSize=5 (PageIndex=1 - 1)=0
                                                5x0=0 this is start page*/
                                            .Skip((pageFilter.PageIndex - 1) * pageFilter.PageSize)
                                            .Take(pageFilter.PageSize)
                                            .ToListAsync();
                // if pass page not contain any data return Bad Request
                if (_users.Count() <= 0)
                    return NoContent();

                List<UserDto> userData = _mapper.Map<List<AppUser>, List<UserDto>>(_users);

                // get the baseurl(Domain) of website
                var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                // get the default images for users
                foreach (var _user in userData)
                {
                    IQueryable<UserImagesDto> images = from ui in _context.UploadUserImagesList
                                                       join up in _context.Upload on ui.UploadId equals up.Id
                                                       join typ in _context.UploadType on ui.UploadTypeId equals typ.Id
                                                       where ui.UserId == _user.Id && ui.Default == true
                                                       select new UserImagesDto
                                                       {
                                                           Id = ui.Id,
                                                           Path = url + up.Path,
                                                           Type = typ.Name,
                                                           Default = ui.Default
                                                       };

                    _user.UserImagesList = images.ToList();
                }
                
                return new Pagination<UserDto>(par.PageIndex, par.PageSize, totalItem, userData);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }
        }


        [HttpPost("ChangeUserRole")]
        public async Task<ActionResult<string>> ChangeUserRole([FromForm] ChangeUsreRoleDto cuDto)
        {
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            // get user using UserName or Email to change his Roles
            var userChacnged = await _userManager.Users.Include(x => x.Address).SingleOrDefaultAsync(x => x.UserName == cuDto.UserNameOrEmail);
            userChacnged = userChacnged ?? await _userManager.Users.Include(x => x.Address).SingleOrDefaultAsync(x => x.Email == cuDto.UserNameOrEmail);
            if (userChacnged == null) return NotFound(new ApiResponse(404));

            var newRole = cuDto.Role;
            // Prevent any of the users to have Role SuperAdmin, We have just one
            var roles = _roleManager.Roles.Where(r => r.Name != "SuperAdmin").ToList();

            if (!roles.Any(r => r.Name.Contains(newRole)))
                return BadRequest(new ApiResponse(400, "this Role not exist!"));

            // there a possibility to the user has more than one Role,
            // but in this app we need just one role for every user
            var oldRole = (await _userManager.GetRolesAsync(userChacnged)).FirstOrDefault();
            // get old role, to chack if new Role is the same new role and remove it
            if (oldRole == newRole)
                return Ok("this user has the same role");

            // Prevent change Admin user if the current user has Admin Role too
            var cuUserRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            if (cuUserRole == "Admin" && oldRole == "Admin")
                return BadRequest(new ApiResponse(400, "with your Role can't change Admin Role, you need SuperAdmin Role"));

            // remove old role, then add the new one
            await _userManager.RemoveFromRoleAsync(userChacnged, oldRole);
            await _userManager.AddToRoleAsync(userChacnged, newRole);

            // test if the user role now is updated
            if (newRole == (await _userManager.GetRolesAsync(userChacnged)).FirstOrDefault())
                return Ok(new ApiResponse(201, "Role updated Successfully")); //  
            else
                return BadRequest(new ApiResponse(400));
        }

        [HttpDelete("DeleteUser")]
        public async Task<ActionResult<string>> DeleteUser([FromForm]string userNameOrEmail)
        {
            // get current user, if was Admin, or SuperAdmin, SuperAdmin can Delete all user except his Account,
            // and Admin can delete all users except Admin and SuperAdmin users
            // get Current user using Email in Token
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));


            var userDeleted = await _userManager.Users.Include(x => x.Address).SingleOrDefaultAsync(x => x.UserName == userNameOrEmail);
            userDeleted = userDeleted ?? await _userManager.Users.Include(x => x.Address).SingleOrDefaultAsync(x => x.Email == userNameOrEmail);
            if (userDeleted == null) return NotFound(new ApiResponse(404));

            // SuperAdmin is not  Deletable, and Admin Can't Delete Another Admin, Just SuperAdmin Can Do that
            var userRole = (await _userManager.GetRolesAsync(userDeleted)).FirstOrDefault();
            var currentUserRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            if (userRole == "SuperAdmin" || (userRole == "Admin" && currentUserRole == "Admin"))
                return BadRequest(new ApiResponse(400, $"you can't delete this user {userNameOrEmail} with your {currentUserRole} Role."));
            try
            {
                // if user hase images or files in server, nust delete them too Cascade Referential Action.
                await _userManager.DeleteAsync(userDeleted);
                return Ok(new ApiResponse(201, "Delete user Successfully")); //  
            }
            catch (Exception ex)
            {
                new ApiResponse(0, ex.Message, true, ex.ToString(), 1,
                   new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName(),/* Get File Name */
                   (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()); /* Get Error Line */

                return BadRequest(new ApiResponse(400, $"Exception: {ex.Message}"));
            }
        }

        ///<summary> Deletion of all registered users a month before today's date, and no email or phone confirmation has been made</summary>
        //Delete all users are not confirmed email and PhoneNumber
        [HttpGet("DeleteUsersNotConfirmed")]
        public async Task<ActionResult<string>> DeleteUsersNotConfirmed()
        {
            var beforOneMonth = DateTime.Today.AddMonths(-1);
            // Delete all users who were registered one month ago and have not yet activated their accounts
            var userUnauthorizeIdList = _userManager.Users.Where(u => u.EmailConfirmed == false && u.PhoneNumberConfirmed == false && u.RegisterDate < beforOneMonth).Select(u => u.Id).ToList();
            foreach (var id in userUnauthorizeIdList)
            {
                var user = await _userManager.FindByIdAsync(id);
                await _userManager.DeleteAsync(user);
            }
            return Ok("Delete all users, that not Confirm email and Phone number");
        }

        [AllowAnonymous]
        [HttpPost("registerUser")]
        public async Task<ActionResult<string>> registerUser([FromForm] RegisterDto regDto)
        {
            // check if username or Email existing before or not, if exist stop Register
            var user = await _userManager.FindByEmailAsync(regDto.Email);
            user = user ?? await _userManager.FindByNameAsync(regDto.UserName);
            if (user != null) return BadRequest(new ApiResponse(400, $"this user existing before"));

            // check if user age is allowed to register
            if (_globalFunc.NotAllowedAge(regDto.Birthday))
                return BadRequest(new ApiResponse(400, $"this age is under the allowed age to register on the website"));


            // The role of the registered user on the method
            var UserTypeDefault = "Visitor";

            var userRegister = _mapper.Map<RegisterDto, AppUser>(regDto);
            userRegister.RegisterDate = DateTime.Now;
            userRegister.AutomatedPassword = true;

            var result = await _userManager.CreateAsync(userRegister, regDto.Password);
            if (result.Succeeded)
            {
                // add user Role
                _userManager.AddToRoleAsync(userRegister, UserTypeDefault).Wait();

                return Ok($"successfully registered");
            }
            return BadRequest(new ApiResponse(400, $"Register failed"));
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
