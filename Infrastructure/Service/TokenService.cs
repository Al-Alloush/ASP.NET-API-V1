using Core.Interfaces.Service;
using Core.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;

        // Symmetric security key as a type of encryption where only one a secret key which we're 
        // going to store on our server is used to both encrypt and decrypt our signature in the token.
        // It's essential that this never leaves our server, otherwise anybody's going to be able to impersonate any user on our system.
        private readonly SymmetricSecurityKey _key;
        public TokenService(IConfiguration config, UserManager<AppUser> userManager)
        {
            _config = config;
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Token:Key"]));
        }

        public async Task<string> CreateToken(AppUser user)
        {
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            // to add the role in Web token to use id with Authorize
            IdentityOptions _option = new IdentityOptions();
            // each user is going to have a list of their claims inside this JWT
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(_option.ClaimsIdentity.RoleClaimType, role)
            };

            //
            var Credential = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = Credential,
                Issuer = _config["Token:Issuer"]
            };

            // generate a Token with all tokenDescriptor
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
