using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPISecured.Areas.Identity.Data;
using WebAPISecured.Data;
using WebAPISecured.Models;

namespace WebAPISecured.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly WebAPISecuredContext _dbContext;
        private readonly UserManager<WebAPISecuredUser> _userManager;
        private readonly SignInManager<WebAPISecuredUser> _signInManager;

        public AccountController(IConfiguration configuration,
                                WebAPISecuredContext dbContext,
                                UserManager<WebAPISecuredUser> userManager,
                                SignInManager<WebAPISecuredUser> signInManager)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("Token")]
        public async Task<IActionResult> CreateToken([FromBody] MyLoginModel loginModel)
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Email == loginModel.Email);
            //var user = await userManager.FindByNameAsync(login.Email);  // Borde funka...
            if (user != null)
            {
                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password, false);
                if (signInResult.Succeeded)
                {
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var nowUtc = DateTime.Now.ToUniversalTime();
                    var expires = nowUtc.AddMinutes(double.Parse(_configuration["Tokens:ExpiryMinutes"])).ToUniversalTime();

                    var token = new JwtSecurityToken(
                    _configuration["Tokens:Issuer"],
                    _configuration["Tokens:Audience"],
                    null,
                    expires: expires,
                    signingCredentials: creds);

                    var response = new JwtSecurityTokenHandler().WriteToken(token);

                    //return Ok(response);
                    return Ok(new { Token = response });
                }
                else
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }

        [HttpPost("Register")]
        public async Task<ActionResult> Register([FromBody] MyLoginModel loginModel)
        {
            WebAPISecuredUser webApiSecuredUser = new WebAPISecuredUser()
            {
                Email = loginModel.Email,
                UserName = loginModel.Email,
                EmailConfirmed = false          // We don't need e-mail confirmation of new user IDs
            };

            var result = await _userManager.CreateAsync(webApiSecuredUser, loginModel.Password);
            if (result.Succeeded)
            {
                return Ok(new { Result = "Register Success" });
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var error in result.Errors)
                {
                    stringBuilder.Append(error.Description);
                }
                return BadRequest(new { Result = $"Register Fail: {stringBuilder}" });
            }
        }
    }
}
