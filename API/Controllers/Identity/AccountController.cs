using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Error;
using AutoMapper;
using Core.DTOs.Identity;
using Core.Interfaces.Service;
using Core.Models.Identity;
using Infrastructure.Data;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace API.Controllers.Identity
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private EmailSmsSenderService _sendEmailsSmsService;
        private readonly IConfiguration _config;
        private readonly GlobalFunctions _globalFunc;
        private readonly AppDbContext _context;
        private string UrlProtocol;
        private string ApiUrl;

        public AccountController(   UserManager<AppUser> userManager,
                                    IMapper mapper,
                                    ITokenService tokenService,
                                    SignInManager<AppUser> signInManager,
                                    EmailSmsSenderService sendEmailsSmsService,
                                    IConfiguration config,
                                    GlobalFunctions globalFunc,
                                    AppDbContext context)
        {
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _sendEmailsSmsService = sendEmailsSmsService;
            _config = config;
            _globalFunc = globalFunc;
            _context = context;
            UrlProtocol = _config["ApiUrl:UrlProtocol"];
            ApiUrl = _config["ApiUrl:Url"];
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register([FromForm] RegisterDto regDto)
        {
            // check if username or Email existing before or not, if exist stop Register
            var check_user = await _userManager.FindByEmailAsync(regDto.Email);
            check_user = check_user ?? await _userManager.FindByNameAsync(regDto.UserName);
            if (check_user != null)
                return BadRequest(new ApiResponse(400, $"this user existing before"));

            // check if user age is allowed to register
            if (_globalFunc.NotAllowedAge(regDto.Birthday))
                return BadRequest(new ApiResponse(400, $"Your age is under the allowed age to register on the website"));

            // The role of the registered user on the method
            var UserTypeDefault = "Visitor";

            var user = _mapper.Map<RegisterDto, AppUser>(regDto);
            user.RegisterDate = DateTime.Now;
            var result = await _userManager.CreateAsync(user, regDto.Password);
            if (result.Succeeded)
            {
                // add user Role
                _userManager.AddToRoleAsync(user, UserTypeDefault).Wait();
                // Send verification Email
                if (!await SendConfirmEmailAsync(user))
                {
                    /************************* ADD Error In AppError Table ****************************/
                    await _globalFunc.ErrorRegister(msg: "Error with Send Confirmation Email to new Register",
                        statusCode: 0,
                        errorTypeId: 3, /* the error type: 1: Exception, 2:Server, 3: third party, 4: Other */
                        filePath: new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName(),/* Get File Name */
                        errorLine: (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()); /* Get Error Line */
                    /************************* END ADD Error In AppError Table ************************/
                    // If you fail to send an account verification email, the user will be deleted
                    await _userManager.DeleteAsync(user);
                    return BadRequest(new ApiResponse(400, "Something wrong!, please call the support"));
                }
                return Ok($"successfully registered, Please visit your email to Activate this Account");
            }
            return BadRequest(new ApiResponse(400,$"Register failed"));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginSuccessDto>> Login([FromForm] LoginDto logDto)
        {
            // login using UserName or Email
            var user = await _userManager.Users.Include(x => x.Address).SingleOrDefaultAsync(x => x.UserName == logDto.UserNameOrEmail);
            user = user ?? await _userManager.Users.Include(x => x.Address).SingleOrDefaultAsync(x => x.Email == logDto.UserNameOrEmail);
            if (user == null)
                return NotFound(new ApiResponse(404));

            // chack the password
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await _signInManager.CheckPasswordSignInAsync(user, logDto.Password, true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return BadRequest(new ApiResponse(400, "you have made a bad request, your Account is LockedOut"));
                }
                return BadRequest(new ApiResponse(400,"you have made a bad request"));
            }

            // check if the email is confirming, .
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                // if not Confirmed sending a new Confirmation email and return a message
                if (!await SendConfirmEmailAsync(user))
                    return BadRequest(new ApiResponse(400, $"Somthing wronge!, please call the seport"));

                return Ok("Please visit your account to activate your Email");
            }

            // check if this user Created by Admin or not, if akutomatedPassword was trure ask user to set new password
            var akutomatedPassword = user.AutomatedPassword;
            if (akutomatedPassword)
            {
                if (await SendResetPassword(user))
                    return Ok($"to login you need to Reset your Password, rest password sent to: {user.Email}");
            }

            var _user = _mapper.Map<AppUser, LoginSuccessDto>(user);
            _user.Token = await _tokenService.CreateToken(user);

            // get the baseurl(Domain) of website
            var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            // get user's profile and cover images 
            IQueryable<UserImagesDto> images = from ui in _context.UploadUserImagesList
                                               join up in _context.Upload on ui.UploadId equals up.Id
                         join typ in _context.UploadType on ui.UploadTypeId equals typ.Id
                    where ui.UserId == user.Id && (ui.UploadTypeId == 1 ||ui.UploadTypeId == 2) && ui.Default == true
                    select new UserImagesDto
                    {
                        Path = url + up.Path,
                        Type = typ.Name,
                        Default = ui.Default 
                    };

            _user.UserImagesList = await images.ToListAsync();

            // if result.Succeeded return user
            return _user;
        }


        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public async Task<ActionResult<string>> ForgotPassword([FromForm] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(new ApiResponse(404));

            if (await SendResetPassword(user))
                return Ok($"Reset Password Email Sent to: {email}");

            return BadRequest(new ApiResponse(400));
        }


        private async Task<bool> SendConfirmEmailAsync(AppUser user)
        {
            // Start send Confirm Email
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // to get the confirmation link we need to add the ConfirmEmail Action, it's required to get a Confirm link. 
            // Request.Scheme required if need to generate an absolut URL
            string confirmationLink = this.Url.ActionLink("ConfirmEmail", "Account", new { userId = user.Id, token = token }, UrlProtocol, ApiUrl, Request.Scheme);
            string messageBody = "Please Confirm your Email by clicking <a href=\"" + confirmationLink + "\">here</a><br>";
            var sendEmailResult = await _sendEmailsSmsService.SendGridApiEmail("Confirm your account", user.Email, messageBody);
            // if confirm email not sent, delete the user and reten BadRequest
            if (!sendEmailResult)
                return false;

            return true;
        }

        private async Task<bool> SendResetPassword(AppUser user)
        {
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // ResetPasswordPage is a Action to open static file resetPassword.html to reset passwoed
            string confirmationLink = this.Url.ActionLink("ResetPasswordPage", "Account", new { userId = user.Id, token = token }, UrlProtocol, ApiUrl, Request.Scheme);
            string messageBody = "Please, reset a password by click <a href=\"" + confirmationLink + "\">here</a><br>";

            var sendEmailResult = await _sendEmailsSmsService.SendGridApiEmail("Reset Password", user.Email, messageBody);
            if (sendEmailResult)
                return true;

            return false;
        }

        [HttpGet("ConfirmEmail")]
        public async Task<ActionResult<string>> ConfirmEmail(string userId, string token)
        {
            // if this any data is null or empty
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(new ApiResponse(400, $"user Id or token was empty"));

            // get user 
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiResponse(404));

            // Confrim the accoutn
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return Ok("Email has Confirmed");
            else
            {
                // again check if email is not confirming, if not, again send a new Confirmation email and return a response message.
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Send Confirmation Email
                    if (!await SendConfirmEmailAsync(user))
                        return BadRequest(new ApiResponse(400, $"Somthing wronge!, please call the seport"));
                    else
                        return Ok("this is not validation link, we sent new Confirmation Email, please visit your account to activate your Account");
                }
                return BadRequest(new ApiResponse(400, $"Somthing wronge!, please call the seport"));
            }
        }

        [AllowAnonymous]
        [HttpGet("ResetPasswordPage")]
        public async Task<RedirectResult> ResetPasswordPage(string userId, string token)
        {
            /* after user click on linke in his email get this fuction then redirect to static html pages */

            // get the baseurl(Domain) of website
            var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            if (userId == null || token == null)
                return Redirect($"{url}/wwwroot/Views/error.html?errorCode=204");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Redirect($"{url}/wwwroot/Views/error.html?errorCode=400");

            // Open Reset password Page, pass the token and user Id to page to use them in Form reset password
            return Redirect($"{url}/wwwroot/Views/AccountManager/resetPassword.html?userId={userId}&token={token}");
        }

        [AllowAnonymous]
        [HttpPost("SetResetPasswordConfirmation")]
        public async Task<ActionResult<string>> SetResetPasswordConfirmation([FromForm] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(resetPasswordDto.UserId);
                if (user == null)
                    return NotFound(new ApiResponse(404));

                // because we pass the token with URL parameters, that will replace all (+) sign to space, for that, here change all space to (+) sign
                var Token = resetPasswordDto.Token.Replace(" ", "+");
                var result = await _userManager.ResetPasswordAsync(user, Token, resetPasswordDto.NewPassword);
                if (!result.Succeeded)
                    return NotFound(new ApiResponse(404));

                // update AutomatedPasswoed to false after first login if this user create by Admin
                if (user.AutomatedPassword)
                {
                    user.AutomatedPassword = false;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        return BadRequest(new ApiResponse(400, "Update AutomatedPasssword not success",true, null,4,
                             new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName(),/* Get File Name */
                            (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber())); /* Get Error Line */
                    }
                }
                return Ok("Reset Password Succeded, you can now to close this page");
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, $"ex: {ex.Message}"));
            }
        }

        
    }
}
