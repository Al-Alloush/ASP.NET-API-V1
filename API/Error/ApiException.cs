using Infrastructure.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Error
{
    /*we want to extend our ApiResponse to accommodate the extra field that we want to send down with a response*/
    public class ApiException : ApiResponse
    {

        // because we don't have a parameter constructor in ApiResponse, then we have to provide a constructor inside the class that's deriving from it.
        public ApiException(int statusCode, string message = null, string details = null) : base(statusCode, message)
        {
            Details = details;

            AddError(message, statusCode, 2, Details).Wait();
        }

        // to contain the stack trace that return in Server Error response
        public string Details { get; set; }
    }
}
