using JWTAuthenticationWithAspNetCoreAndAngular.Data;
using JWTAuthenticationWithAspNetCoreAndAngular.DTOs;
using JWTAuthenticationWithAspNetCoreAndAngular.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JWTAuthenticationWithAspNetCoreAndAngular.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtAuthenticationDbContext _dbContext;
        public IConfiguration Configuration { get; }

        public AuthenticationController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            JwtAuthenticationDbContext dbContext, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            Configuration = configuration;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto registerModel)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = registerModel.UserName, Email = registerModel.Email };
                var identityResult = await _userManager.CreateAsync(user, registerModel.Password);

                if (identityResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Ok(GenerateJsonWebToken(user));
                }
                return BadRequest(identityResult.Errors);

            }
            return BadRequest(ModelState);

        }

        [HttpGet]
        public async Task<ActionResult<bool>> IsEmailAlreadyExists(string email)
        {
            bool statusResult = await _dbContext.ApplicationUsers.AnyAsync(au => au.Email == email);
            return statusResult;
        }

        [HttpGet]
        public async Task<ActionResult<bool>> IsUserNameAlreadyExists(string userName)
        {
            bool statusResult = await _dbContext.ApplicationUsers.AnyAsync(au => au.UserName == userName);
            return statusResult;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            if (ModelState.IsValid)
            {
                var loginResult = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, isPersistent: loginDto.RememberMe, lockoutOnFailure: false);

                if (!loginResult.Succeeded)
                {
                    return BadRequest(loginResult);
                }

                var user = await _userManager.FindByNameAsync(loginDto.Username);
                JsonWebToken jsonWebToken = GenerateJsonWebToken(user);
                return Ok(jsonWebToken);
            }

            return BadRequest(ModelState);
        }

        private JsonWebToken GenerateJsonWebToken(ApplicationUser user)
        {
            var utcNow = DateTime.UtcNow;

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, utcNow.ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("Jwt:Key")));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                signingCredentials: signingCredentials,
                claims: claims,
                notBefore: utcNow,
                expires: utcNow.AddSeconds(Configuration.GetValue<int>("Jwt:Lifetime")),
                audience: Configuration.GetValue<string>("Jwt:Issuer"),
                issuer: Configuration.GetValue<string>("Jwt:Issuer")
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            JsonWebToken jsonWebToken = new JsonWebToken()
            {
                UserName = user.UserName,
                Email = user.Email,
                AccessToken = accessToken,
                ExpireIn = Configuration.GetValue<int>("Jwt:Lifetime").ToString()
            };

            return jsonWebToken;

        }
    }
}