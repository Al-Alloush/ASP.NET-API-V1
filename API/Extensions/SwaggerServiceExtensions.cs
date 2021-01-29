
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class SwaggerServiceExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, IConfiguration config)
        {

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(config.GetValue<string>("ConstantNames:APIVersion"),
                  new OpenApiInfo
                  {
                      Title = config.GetValue<string>("ConstantNames:APIProjectName"),
                      Version = config.GetValue<string>("ConstantNames:APIVersion")
                  });
            });

            return services;
        }

        //need to be before app.UseEndpoints
        public static IApplicationBuilder UseSwaggerDocumention(this IApplicationBuilder app, IConfiguration config)
        {
            //need to be before app.UseEndpoints
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint(config.GetValue<string>("ConstantNames:SwaggerURL"), config.GetValue<string>("ConstantNames:SwaggerAPINameAndVirsion")));
            return app;
        }
    }
}
