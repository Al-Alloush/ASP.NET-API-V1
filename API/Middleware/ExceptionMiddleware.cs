using API.Error;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger; // to write in Console
        private readonly IHostEnvironment _env; // to check and see if Development mode is running or not

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _env = env;
            _logger = logger;
            _next = next;
        }

        // middleware method
        public async Task InvokeAsync(HttpContext context)
        {
            // because this is going to be our exception handling middleware use a try catch block
            try
            {
                // if there is no exception we want the middleware to move on to the next piece/line of middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                // we want to catch an exception.

                _logger.LogError(ex, ex.Message);

                // write our own response into the context response, that we can send it to the client.
                context.Response.ContentType = "application/json"; // response type formatted as JSON
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // set the status code to be a five hundred internal server error 500

                //check if we use Development mode
                var response = _env.IsDevelopment()
                    ? new ApiException((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace.ToString())/* pass ex.Message and ex.StackTrace just in Development mode*/
                    : new ApiException((int)HttpStatusCode.InternalServerError);/* In Production mode*/

                // to change Jason response back from Pascal case  to camel case
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}
