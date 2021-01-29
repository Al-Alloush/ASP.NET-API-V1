using Core.Models.Identity;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class IdentityServiceExtentions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            var builder = services.AddIdentityCore<AppUser>();
            builder = new IdentityBuilder(builder.UserType, builder.Services);

            // entity framework implementation of identity information stores. where we were adding the UserManager to
            // DefaultUsersAsync method in Infrastructure/Identity folder that kind of service is contained inside entity framework stores.
            builder.AddEntityFrameworkStores<AppDbContext>();
            builder.AddSignInManager<SignInManager<AppUser>>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    /* if we forget to add this, we might as well just leave anonymous authentication on and a user can 
                    send up any old token they want because we would never validate that the signing key is correct.*/
                    ValidateIssuerSigningKey = true,
                    /* tell it about our issue assigning key, we need to do the same encoding we did in TokenService.cs Constractor */
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Token:Key"])),
                    /* same as our token issuer that we're going to add to our configurations in TokenService.cs Constractor */
                    ValidIssuer = config["Token:Issuer"],
                    /* if not just accepts any issuer of any token so we'll set that to true as well.*/
                    ValidateIssuer = true,
                    /* */
                    ValidateAudience = false
                };
            });

            return services;
        }
    }
}
