using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models.Identity;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            /***********************************************************************************************
           change CreateHostBuilder to create migrations then update the database with startup the programm.
           then add default or seed data in database or Programm.

           change the old mithod  CreateHostBuilder(args).Build().Run(); To:
           *******************************************************************/
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                try
                {
                    /****************************************************************************************************
                    the next to code line are for Create a migratons and update the database with startup the program,
                    for now I don't need them.
                    *****************************************************************************************************/
                    /* create and AppIdentityDbContext service to create Migration */
                    //var identityContext = services.GetRequiredService<AppDbContext>();
                    /* Update the database with startup the Application*/
                    //await identityContext.Database.MigrateAsync();

                    //after Create database and create all tables, add the defult Roles
                    var AddRoles = await InitializeDefaultData.AddDefaultRolesInDatabase(services);
                    // if there are no Roles in table insert default Roles and return true
                    if (AddRoles)
                    {
                        // add Default Upload Types
                        await InitializeDefaultData.AddDefaultUplodTypes(services);
                        // add the SuperAdmin in first time startup this program
                        await InitializeDefaultData.AddSuperAdminUser(services);
                        // Add Default Error Types
                        await InitializeDefaultData.AddDefaultErrorTypes(services);
                        // add Default Languages
                        await InitializeDefaultData.AddDefaultLanguages(services);
                        // add Default Blogs Catetories
                        await InitializeDefaultData.AddDefaultBlogCategories(services);

                        /****************************** Add Seed Data ********************************/
                        // add seed users data for test and developer mode
                        await InitializeDefaultData.AddSeedUsersData(services);
                        // Add Seed Blogs
                        await InitializeDefaultData.AddSeedBlogs(services);
                    }
                }
                catch (Exception ex)
                {
                    // Display the error in Console/Rermainal
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "An error occured during adding default Data in API/program.cs");
                }
                host.Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
