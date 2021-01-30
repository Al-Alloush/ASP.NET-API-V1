using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Error;
using API.Extensions;
using API.Helppers;
using API.Middleware;
using AutoMapper;
using Core.Interfaces.Service;
using Core.Models.Identity;
using Infrastructure.Data;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<AppDbContext>(x =>
            {
                x.UseSqlite(Configuration.GetConnectionString("DbConnection"));
            });

            /*****************************************************************
            to use UserManager & IdentityRole in application:
                Add service for type 'Microsoft.AspNetCore.Identity.UserManager:
                    - .AddIdentityCore<AppUser>()
                Add IdentityRole service in Application:
                    - .AddRoles<IdentityRole>()
                to avoid error :Unable to resolve service for type 'Microsoft.AspNetCore.Identity.IUserStore`1 
                    - AddEntityFrameworkStores<AppDbContext>()
                Add SignInManager Service to Login Action
                    - .AddSignInManager<SignInManager<AppUser>>()
                        to inject SignInManager service need to inject another service : 
                        - services.AddAuthentication() 
                to use Microsoft.AspNetCore.Identity Token provider to use function like:UserManager.GenerateEmailConfirmationTokenAsync()
                    - .AddDefaultTokenProviders() */
            services.AddIdentityCore<AppUser>()
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders()
                    .AddSignInManager<SignInManager<AppUser>>();
            /********************************************************************/

            // Add Mapping Tools
            services.AddAutoMapper(typeof(MappingProfiles));


            // add JWT Service
            services.AddScoped<ITokenService, TokenService>();
            services.AddIdentityServices(Configuration);

            // to use EmailSmsSender Service in API project
            services.AddScoped<EmailSmsSenderService>();

            // API/Extensions/SwaggerServiceExtensions.cs
            services.AddSwaggerDocumentation(Configuration);


            //************************************************ override the behavior of ``[ ApiController ]
            // Configer the ApiBehaviorOptions type service
            services.Configure<ApiBehaviorOptions>(options =>
            {// pass some option what we want configer
                options.InvalidModelStateResponseFactory = actionContext =>
                { /*inside the actionContext is where we can get our model state errors and that's what the API attribute is 
                    using to populate any errors that are related to validation and add them into a model state dictionar*/

                    /*extract the errors if there are any and populates the error messages into an array and 
                    that's the array will pass into our ApiValidationErrorResponse class into the errors property */
                    var errors = actionContext.ModelState       /* ModelState is a dictionary type of object. */
                        .Where(e => e.Value.Errors.Count > 0)   /* check if here any Error */
                        .SelectMany(x => x.Value.Errors)        /* select all of the errors */
                        .Select(x => x.ErrorMessage).ToArray(); /* select just the error messages */
                    var errorResponse = new ApiValidationErrorResponse
                    {
                        Errors = errors
                    };
                    return new BadRequestObjectResult(errorResponse); /* pass ApiValidationErrorResponse with all errors*/
                };
            });

            // Add Global Function to use it in all apps
            services.AddScoped<GlobalFunctions>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // handling exceptions just in developer mode.
            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            // }
            app.UseMiddleware<ExceptionMiddleware>();

            // if request commes into API Server don't have and EndPoint match that request, this middleware redirect to ErrorController.cs 
            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            app.UseHttpsRedirection();

            app.UseRouting();

            // to work with Static files like images and html files 
            app.UseStaticFiles();
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
                RequestPath = "/wwwroot",
                EnableDefaultFiles = true
            });

            // to worke Authentication JWT
            app.UseAuthentication();
            app.UseAuthorization();

            // API/Extensions/SwaggerServiceExtensions.cs
            app.UseSwaggerDocumention(Configuration);


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
