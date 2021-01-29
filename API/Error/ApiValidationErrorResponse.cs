using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Error
{

    public class ApiValidationErrorResponse : ApiResponse
    {
        // use the status code 400, that we know this statusCode
        public ApiValidationErrorResponse() : base(400)
        {
        }

        // Store list of Errors, these validation responses are typically going to be generated in response to a user submitting a
        // form with some data on and there could be more than one validation error.
        public IEnumerable<string> Errors { get; set; }
    }
}
