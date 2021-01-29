using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Error;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /*  to get a request that's come into  API and get it passed to this particular controller, wee need to add in **middleware** in ``startup.cs`` class:
        app.UseStatusCodePagesWithReExecute("/errors/{0}");
            
     */

    [ApiController]
    // override the routes that we get from our base API controller
    // override Route in BaseApiController
    [Route("errors/{code}")]
    [ApiExplorerSettings(IgnoreApi = true)]/*to not display https://localhost:5001/swagger/ as an error in api swagger*/
    public class ErrorController : ControllerBase
    {
        public IActionResult Error(int code)
        {
            return new ObjectResult(new ApiResponse(code));
        }

    }
}
