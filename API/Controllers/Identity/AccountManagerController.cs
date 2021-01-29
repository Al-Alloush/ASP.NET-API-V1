using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Error;
using AutoMapper;
using Core.DTOs.Identity;
using Core.Models.Identity;
using Core.Models.Uploads;
using Infrastructure.Data;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace API.Controllers.Identity
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountManagerController : ControllerBase
    {
        private const string USER_IMAGE_DIRECTORY = "/Uploads/Images/";
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailSmsSenderService _sendEmailsSmsService;
        private readonly IMapper _mapper;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private string UrlProtocol;
        private string ApiUrl;

        public AccountManagerController(UserManager<AppUser> userManager,
                                        EmailSmsSenderService sendEmailsSmsService,
                                        AppDbContext context,
                                        IMapper mapper,
                                        SignInManager<AppUser> signInManager,
                                        IWebHostEnvironment webHostEnvironment,
                                        IConfiguration config)
        {
            _sendEmailsSmsService = sendEmailsSmsService;
            _mapper = mapper;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _config = config;
            UrlProtocol = _config["ApiUrl:UrlProtocol"];
            ApiUrl = _config["ApiUrl:Url"];
        }


        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            var _user = _mapper.Map<AppUser, UserDto>(user);

            // get the baseurl(Domain) of website
            var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            // get all user's images
            IQueryable<UserImagesDto> images = from ui in _context.UploadUserImagesList
                                               join up in _context.Upload on ui.UploadId equals up.Id
                                               join typ in _context.UploadType on ui.UploadTypeId equals typ.Id
                                               where ui.UserId == user.Id && ui.Default == true
                                               select new UserImagesDto
                                               {
                                                   Id = ui.Id,
                                                   Path = url + up.Path,
                                                   Type = typ.Name,
                                                   Default = ui.Default
                                               };
            _user.UserImagesList = images.ToList();
            return _user;
        }


        [HttpGet("GetCurrentUserAddress")]
        public async Task<ActionResult<AddressDto>> GetCurrentUserAddress()
        {
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            var address = await _context.Address.SingleOrDefaultAsync(a => a.UserId == user.Id);

            var _address = _mapper.Map<Address, AddressDto>(address);
            return _address;

        }

        [HttpPut("UpdateUserAddress")]
        public async Task<ActionResult<AddressDto>> UpdateUserAddress([FromForm] AddressDto address)
        {
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            var _address = await _context.Address.SingleOrDefaultAsync(a => a.UserId == user.Id);

            _mapper.Map(address, _address);

            try
            {
                _context.Entry(_address).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok($"Update Address Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message, true, ex.ToString(),1));
            }
        }

        [HttpPut("UploadDefaultUserImage")]
        public async Task<ActionResult<string>> UploadDefaultUserImage([FromForm] IFormFile file, [FromForm] int typeId)
        {
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            // 1 = imageProfile,  2 = imageCover 
            if (typeId !=1 && typeId !=2)
                return BadRequest(new ApiResponse(400, "select profile or cover image. 1,2"));

            try
            {
                // this path webHostEnvironment.WebRootPath is under wwwroot folder & BLOG_IMAGE_DIRECTORY is where Blog Image Folder
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath + USER_IMAGE_DIRECTORY);
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

                // Create uniqu file name to avoid overwrite old image with same name
                string fileName = (Guid.NewGuid().ToString().Substring(0, 8)) + "_" + file.FileName;
                filePath = Path.Combine(filePath, fileName);
                using (FileStream fileStream = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fileStream);
                    // Clears buffers for this stream and causes any buffered data to be written to the file.
                    fileStream.Flush();

                    var upload = new Upload
                    {
                        Name = fileName,
                        Path = USER_IMAGE_DIRECTORY + fileName,
                        AddedDateTime = DateTime.Now,
                        UserId = user.Id
                    };
                    await _context.Upload.AddAsync(upload);
                    await _context.SaveChangesAsync();

                    // change the old default image to false and set new one
                    var defaultImage = await _context.UploadUserImagesList.Where(g => g.UserId == user.Id && g.Default == true && g.UploadTypeId == typeId).FirstOrDefaultAsync();
                    defaultImage.Default = false;
                    _context.Entry(defaultImage).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    // set the new image
                    var userImage = new UploadUserImagesList
                    {
                        UploadId = upload.Id,
                        UserId = user.Id,
                        UploadTypeId = typeId,
                        Default = true
                    };
                    await _context.UploadUserImagesList.AddAsync(userImage);
                    await _context.SaveChangesAsync();
                    return Ok($"Update Image Successfully");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, $"Error: {ex.Message}"));
            }
        }

        [HttpPut("UpdateDefaultUserImage")]
        public async Task<ActionResult<string>> UpdateDefaultUserImage([FromForm] int imageId, [FromForm] int typeId)
        {
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            // 1 = imageProfile,  2 = imageCover 
            if (typeId != 1 && typeId != 2)
                return BadRequest(new ApiResponse(400, "select profile or cover image. 1,2"));

            List<UploadUserImagesList> userImages = await _context.UploadUserImagesList.Where(g => g.UserId == user.Id).ToListAsync();
            foreach(var img in userImages)
            {
                if (img.Id == imageId)
                {
                    img.Default = true;
                    img.UploadTypeId = typeId;
                }
                else if (img.Id != imageId && img.Default == true && img.UploadTypeId == typeId)
                    img.Default = false;

                _context.Entry(img).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            return Ok("update Default image successfully");
        }

        [HttpDelete("DeleteUserImage")]
        public async Task<ActionResult<string>> DeleteUserImage([FromForm] int imageId)
        {
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            // check if image existing in UploadBlogImagesList
            var image = await _context.UploadUserImagesList.FirstOrDefaultAsync(i => i.Id == imageId && i.UserId == user.Id);
            if (image == null) return NotFound(new ApiResponse(404, "no image to delete"));

            // check if upload existing
            var upload = await _context.Upload.FindAsync(image.UploadId);
            if (upload == null) return NotFound(new ApiResponse(404, "no image to delete"));

            _context.Remove(upload);
            await _context.SaveChangesAsync();
            // delete Image file from server
            System.IO.File.Delete(_webHostEnvironment.WebRootPath + upload.Path);
            return Ok(new ApiResponse(201)); // Successfully Delete Image
        }


        [HttpPut("AddPhoneNumber")]
        public async Task<ActionResult<string>> AddPhoneNumber([FromForm] string phoneNumber)
        {

            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            if (user.PhoneNumber == phoneNumber && await _userManager.IsPhoneNumberConfirmedAsync(user))
                    return Ok("this Number is confirmed before");// to avoid re Confirm the phone number already confirmed before


            user.PhoneNumber = phoneNumber;
            // if user add new phone number after confirm old number, set PhoneNumberConfirmed to false.
            user.PhoneNumberConfirmed = false;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                if (await SmsSender(user, phoneNumber))
                    return Ok($"the verification code has been sent to the following number: {phoneNumber}");
            }
            return BadRequest(new ApiResponse(400, $"Phone Number: {phoneNumber} is invalid "));
        }

       
        [AllowAnonymous]
        [HttpGet("ConfirmPhoneNumber")]
        public async Task<ActionResult<string>> ConfirmPhoneNumber(string userEmail, string phoneNumber, string token)
        {
            try
            {
                if (userEmail == null || phoneNumber == null || token == null)
                    return BadRequest(new ApiResponse(400));

                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user != null)
                {
                    var confirmed = await _userManager.IsPhoneNumberConfirmedAsync(user);
                    if(user.PhoneNumber == phoneNumber && confirmed)
                    {
                        return Ok($"this Phone number confirmed before");
                    }

                    var result = await _userManager.ChangePhoneNumberAsync(user, phoneNumber, token);
                    if (result.Succeeded)
                        return Ok($"The phone number: {phoneNumber} has been confirmed");
                }
                return BadRequest(new ApiResponse(400, $"Failed to Confirm the Phone Number"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, $"Error: {ex.Message}", true, ex.ToString()));
            }
        }

        [HttpDelete("DeletePhoneNumber")]
        public async Task<ActionResult<string>> DeletePhoneNumber()
        {
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            var result = await _userManager.SetPhoneNumberAsync(user, "");
            if (result.Succeeded)
            {
                user.PhoneNumberConfirmed = false;
                var updateResult = await _userManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                    return Ok("Delete Phone number Successfully"); // SuccessfullyDelete Phone number 
            }
            return BadRequest(new ApiResponse(400, "Failed to delete Phone Number", true,
                "Error to delete or set a empty value in Phone number ", 4,
                filePath: new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName(),/* Get File Name */
                errorLine: (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        [HttpPut("UpdateAccountPassword")]
        public async Task<ActionResult<string>> UpdateAccountPassword([FromForm] UpdatePasswordDto pass)
        {
            // check if this user is the same user who whant update the password
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            if (pass.NewPassword == pass.ConfirmPassword)
            {
                IdentityResult create = await _userManager.ChangePasswordAsync(user, pass.OldPassword, pass.NewPassword);

                if (create.Succeeded)
                {
                    return Ok("update password successfully");
                }
            }
            return Ok("somthing wrong!, or old password not right");
        }

        [HttpPost("SendUpdateEmail")]
        public async Task<ActionResult<string>> SendUpdateEmail([FromForm] string newEmail)
        {
            // get an email form Token Claim, that has been added in TockenServices.cs
            var userEmail = HttpContext.User?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (newEmail == userEmail) return "This email is the same email you used for your account";

            // get user to update new Email
            var user = await _userManager.Users.Include(x => x.Address).SingleOrDefaultAsync(x => x.Email == userEmail);
            if (user == null) return Unauthorized(new ApiResponse(401));

            // check if this email not used
            var check_user = await _userManager.FindByEmailAsync(newEmail);
            if (check_user != null)
                return BadRequest(new ApiResponse(400, $"this user existing before"));


            var sent = await SendUpdateEmailAsync(user, newEmail);
            if (sent)
                return Ok( $"A confirmation email has been sent to {newEmail}");

            return BadRequest(new ApiResponse(400, $"somthing wrong!"));
        }



        private async Task<bool> SmsSender(AppUser user, string _phoneNumber)
        {
            // Generate the token and send it
            var _token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, _phoneNumber);
            string confirmationLink = this.Url.ActionLink("ConfirmPhoneNumber", "AccountManager", new { userEmail = user.Email, phoneNumber = _phoneNumber, token = _token }, UrlProtocol, ApiUrl, Request.Scheme);
            var sendSmsResult = await _sendEmailsSmsService.SmsSender(_phoneNumber, user.UserName, _token, confirmationLink);
            if (!sendSmsResult)
                return false;
            return true;
        }

        private async Task<bool> SendUpdateEmailAsync(AppUser user, string _newEmail)
        {
            // Start send Confirm Email
            string token = await _userManager.GenerateChangeEmailTokenAsync(user, _newEmail);

            // to get the confirmation link we need to add the ConfirmEmail Action, it's required to get a Confirm link. 
            // Request.Scheme required if need to generate an absolut URL
            string confirmationLink = this.Url.ActionLink("UpdateEmail", "AccountManager", new { userId = user.Id, newEmail = _newEmail,  token = token }, UrlProtocol, ApiUrl, Request.Scheme);
            string messageBody = "to update your email please click <a href=\"" + confirmationLink + "\">here</a><br>";
            var sendEmailResult = await _sendEmailsSmsService.SendGridApiEmail("update your Email", _newEmail, messageBody);
            // if confirm email not sent, delete the user and reten BadRequest
            if (!sendEmailResult)
                return false;

            return true;
        }

        [AllowAnonymous]
        [HttpGet("UpdateEmail")]
        public async Task<ActionResult<string>> UpdateEmail(string userId, string newEmail, string token)
        {
            if(!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(newEmail) && !string.IsNullOrEmpty(token) )
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return Unauthorized(new ApiResponse(401));

                var update = await _userManager.ChangeEmailAsync(user, newEmail, token);
                if(update.Succeeded)
                    return Ok($"update Email successfully");
            }
            return Ok($"update Email failed!");
        }

        [HttpPut("UpdateSelectedLanguages")]
        public async Task<ActionResult<string>> UpdateSelectedLanguages([FromForm] SelectedLangueagesDto langu)
        {
            var user = await GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            // if this is the new language and in the App languages list
            // check if this languageCode exist in App languages
            var appLanguList = await _context.Language.Select(l => l.CodeId).ToListAsync();
            if (!appLanguList.Contains(langu.LanguageCode))
                return BadRequest(new ApiResponse(400, "This language not exist"));

            // get the languages were selected from user
            var langList = user.SelectedLanguages.Split(",").ToList();

            // chaeck if this language was not selected from user before
            if(!langList.Contains(langu.LanguageCode.Trim()))
            {
                // if not selected before and value is true
                if(langu.Value == true)
                    langList.Add(langu.LanguageCode.Trim()); // add this language
                else
                    return BadRequest(new ApiResponse(400,"you can't delete language not added!"));
            } // check if this languages is selected before
            else if(langList.Contains(langu.LanguageCode.Trim()))
            {
                // if selected before and value is false, that mean Delete this language
                if (langu.Value == false)
                {
                    int index = langList.IndexOf(langu.LanguageCode.Trim());
                    langList.RemoveAt(index);
                }
                else
                    return BadRequest(new ApiResponse(400, "this language is already selected"));
            }

            // add new Language in database
            var reAddLang = "";
            // create string of languages Cods
            foreach (var lang in langList)
            {
                // to not add an empty string
                if (!string.IsNullOrEmpty(lang))
                    reAddLang = reAddLang + lang + ",";
            }
            // update the user Selected language
            user.SelectedLanguages = reAddLang;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok($"Update {langu.LanguageCode} language successfully");
            
            // return an BadRequest and register this error in database
            return BadRequest(new ApiResponse(400, $"Update {langu.LanguageCode} language failed!",true, null,1,
                new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName(),/* Get File Name */
                (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber())); /* Get Error Line */
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
