using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebAPISecured.Areas.Identity.Data;
using WebAPISecured.Data;
using WebAPISecured.Models;

namespace WebAPISecured.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ValuesController : ControllerBase
    {
        private readonly WebAPISecuredContext _dbContext;
        private readonly UserManager<WebAPISecuredUser> _userManager;
        private readonly SignInManager<WebAPISecuredUser> _signInManager;

        public ValuesController(WebAPISecuredContext dbContext,
                                UserManager<WebAPISecuredUser> userManager,
                                SignInManager<WebAPISecuredUser> signInManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("getFruits")]
        [AllowAnonymous]
        public ActionResult GetFruits()
        {
            List<string> myList = new List<string>() { "apples", "bananas" };
            return Ok(myList);
        }

        [HttpGet("getFruitsAuthenticated")]
        public ActionResult GetFruitsAuthenticated()
        {
            List<string> myList = new List<string>() { "orgenic apples", "organic bananas" };
            return Ok(myList);
        }

        [HttpPost("getToken")]
        [AllowAnonymous]
        public async Task<ActionResult> GetToken([FromBody] MyLoginModelType myLoginModel)
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Email == myLoginModel.Email);
            if (user != null)
            {
                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, myLoginModel.Password, false);
                if (signInResult.Succeeded)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();

                    // TODO: Hämta från appsettings.json
                    var key = Encoding.ASCII.GetBytes("MIN_STORA_HEMLIGA_KRYPTERINGSNYCKEL_iyufgTREDAS9876)=(/&");

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                        new Claim(ClaimTypes.Name, myLoginModel.Email)
                        }),
                        Expires = DateTime.UtcNow.AddDays(1),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenString = tokenHandler.WriteToken(token);

                    return Ok(new { Token = tokenString });
                }
                else
                {
                    return Ok("Failed, try again");
                }
            }
            return Ok("Failed, try again");
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] MyLoginModelType myLoginModel)
        {
            var webApiSecuredUser = new WebAPISecuredUser()
            {
                Email = myLoginModel.Email,
                UserName = myLoginModel.Email,
                EmailConfirmed = false          // We don't need e-mail confirmation of new user IDs, so turn it off
            };

            var result = await _userManager.CreateAsync(webApiSecuredUser, myLoginModel.Password);

            if (result.Succeeded)
            {
                return Ok(new { Result = "Register Success" });
            }
            else
            {
                StringBuilder errorString = new StringBuilder();
                foreach (var error in result.Errors)
                {
                    errorString.Append(error.Description);
                }
                return Ok(new { Result = $"Register fail: {errorString.ToString()}" });
            }
        }

    }
}
