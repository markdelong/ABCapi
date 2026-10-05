using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace ABC.Controllers
{
    [Produces("application/json")]
    [Route("api/Login")]
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generates a token for a authenticated user.
        /// </summary>
        /// <returns>A JwtSecurityToken</returns>
        [AllowAnonymous]
        [HttpPost]
        public IActionResult RequestToken()
        {
            try
            {

                // validate request:
                string username = Request.Form["ucLogIn1$txtU"].ToString();
                string password = Request.Form["ucLogIn1$txtP"].ToString();

                int userId = VSM.Groups.GroupManager.logMemberIn(username, password, 520, _configuration["ConnectionStrings:VSM"]);

                // Log member in. If > 0, validation successful.
                if (userId > 0)
                {

                    // Claim: Requester attributes (username and password)
                    var claims = new[]
                    {
                        
                        // Credential claims
                        new Claim("userid", userId.ToString()),
                        new Claim("username", username),    

                        // Role claim
                        new Claim("LoggedIn", "")
                    };

                    // Signature: secret credential used to sign the token
                    SigningCredentials signature = new SigningCredentials
                        (

                        // key
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Key"])),

                        // encryption and concatenation
                        SecurityAlgorithms.HmacSha256
                        );

                    // Token: return value
                    JwtSecurityToken jwt = new JwtSecurityToken
                        (

                        // TO: (validation settings in Startup)
                        issuer: "",

                        // FROM: (validation settings in Startup)
                        audience: "",

                        // (validation settings in Startup)
                        expires: DateTime.Now.AddDays(1),

                        // PAYLOAD:
                        claims: claims,

                        // SIGNATURE:
                        signingCredentials: signature
                        );

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(jwt)
                    });
                }

                // else: return bad request
                return BadRequest("Authentication failed: invalid credentials.");

            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e);

                return BadRequest("Authentication failed: null argument.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return BadRequest("Authentication failed: internal error.");
            }
        }

        // This was used to generate a token for VSM users on 5/9/2018.
        /*[AllowAnonymous]
        [HttpPost]
        [Route("gen")]
        public IActionResult OneTimeAdminTokenGeneratorVersonOnePointOh()
        {
            try
            {
                var username = "W795BMttuwJfrVyfnMgK4Z9DXoNbUBpx";
                var password = "87OpTjHEEZUS1YFctxacnXNytnHHzzYs";
                var claims = new[]
                {
                        new Claim(username, password),
                        new Claim("VSMadmin", "")
                    };
                var signature = new SigningCredentials
                    (
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Key"])),
                    SecurityAlgorithms.HmacSha256
                    );
                var jwt = new JwtSecurityToken
                    (
                    issuer: "www.vsmtools.com",
                    audience: "www.vsmtools.com",
                    expires: DateTime.Now.AddYears(100),
                    claims: claims,
                    signingCredentials: signature
                    );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(jwt)
                });
            }
            catch (ArgumentNullException e)
            {
                return BadRequest("Authentication failed: null argument.");
            }
            catch (Exception e)
            {
                return BadRequest("Authentication failed: internal error.");
            }
        }
*/
        [Authorize]
        [HttpGet]
        [Route("testauth")]
        public string TestAuth()
        {
            return User.Claims.First(claim => claim.Type == "userid").Value;

        }

    }

}