using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Error;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]/*to not display https://localhost:5001/swagger/ as an error in api swagger*/
    public class BuggyController : ControllerBase
    {
        private readonly AppDbContext _context;
        public BuggyController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("testAuth")]
        [Authorize]
        public ActionResult<string> GetSecretText()
        {
            return "secret stuff";
        }

        [HttpGet("notfound")]
        public ActionResult GetNotFoundRequest()
        {
            var user = _context.Address.Find(-1);

            if (user == null)
            {
                return NotFound(new ApiResponse(404));

            }
            return Ok();
        }

        [HttpGet("badRequest/{id}")]
        public ActionResult GetNotFoundRequest(int id)
        {
            // validation Error
            return Ok();
        }

        [HttpGet("badRequest")]
        public ActionResult GetBadRequest()
        {
            return BadRequest(new ApiResponse(400));
        }

        [HttpGet("serverError")]
        public ActionResult GetServerError()
        {
            // when get an exception
            var user = _context.Users.Find(-1);
            var thingToReturn = user.ToString();
            return Ok(thingToReturn);
        }

        // test with end point not exist too
        // it will return status case 404 Not Found
    }
}
