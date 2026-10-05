using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ABC.Model;

namespace ABC.Controllers
{

    [Produces("application/json")]
    [Route("api/Default")]
    public class DefaultController : Controller
    {

        public static List<string> stuff = new List<string>();

        // GET: api/Default
        [Authorize]
        [HttpGet]
        [Route("test")]
        public IActionResult Get()
        {
            DTO<string> result = new DTO<string>();
            try
            {
                result.Data = stuff;
                result.Status = 200;
                result.Message = "Ok";
                return Ok(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result.Data = new List<string>(); // empty result
                result.Status = 500;
                result.Message = "Error";
                return BadRequest(result);
            }
        }

        // GET: api/Default/5
        [Authorize(Policy = "IsLoggedIn")]
        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(int id)
        {

            DTO<string> result = new DTO<string>();
            try
            {
                result.Data.Add(stuff[id]);
                result.Status = 200;
                result.Message = "Ok";
                return Ok(result);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
                result.Data = new List<string>(); // empty result
                result.Status = 404;
                result.Message = "Invalid id";
                return BadRequest(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result.Data = new List<string>(); // empty result
                result.Status = 500;
                result.Message = "Error";
                return BadRequest(result);
            }
        }

        // POST: api/Default/new
        [HttpPost]
        [Authorize(Policy = "IsVSMadmin")]
        [Route("new")]
        public void Post([FromBody]string value)
        {
            stuff.Add(value);
        }

        // PUT: api/Default/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
            stuff[id] = value;
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "IsLoggedIn")]
        public void Delete(int id)
        {
            stuff.Remove(stuff[id]);
        }

        /*
         * -----SECURITY TEST------------
         */

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor. Sets the config file for authentication.
        /// </summary>
        /// <param name="configuration"></param>
        public DefaultController(IConfiguration configuration)
        {
            // test data
            stuff.Add("Aaron");
            stuff.Add("Betty");
            stuff.Add("Corey");
            stuff.Add("Daisy");

            _configuration = configuration;
        }

    }
}
