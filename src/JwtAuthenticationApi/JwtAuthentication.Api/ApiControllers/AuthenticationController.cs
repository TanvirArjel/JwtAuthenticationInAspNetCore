using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JwtAuthentication.Api.ApiModels;
using JwtAuthentication.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace JwtAuthentication.Api.ApiControllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegistrationModel registerModel)
        {
            ApplicationUser user = new ApplicationUser { UserName = registerModel.UserName, Email = registerModel.Email };
            IdentityResult identityResult = await _userManager.CreateAsync(user, registerModel.Password);

            if (identityResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Ok(GenerateJsonWebToken(user));
            }

            return BadRequest(identityResult.Errors);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> IsEmailAlreadyExists(string email)
        {
            bool statusResult = await _userManager.Users.AnyAsync(au => au.Email == email);
            return statusResult;
        }

        [HttpGet]
        public async Task<ActionResult<bool>> IsUserNameAlreadyExists(string userName)
        {
            bool statusResult = await _userManager.Users.AnyAsync(au => au.UserName == userName);
            return statusResult;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginModel loginModel)
        {
            IdentityError identityError = new IdentityError();

            ApplicationUser applicationUser = await _userManager.Users
                .Where(u => u.Email == loginModel.UserName || u.UserName == loginModel.UserName).FirstOrDefaultAsync();

            if (applicationUser == null)
            {
                identityError.Code = "Email";
                identityError.Description = "The email does not exist.";
                return BadRequest(identityError);
            }

            SignInResult signinResult = await _signInManager.PasswordSignInAsync(
                     applicationUser.UserName,
                     loginModel.Password,
                     isPersistent: loginModel.RememberMe,
                     lockoutOnFailure: false);

            if (signinResult.Succeeded)
            {
                JsonWebTokenModel jsonWebToken = GenerateJsonWebToken(applicationUser);
                return Ok(jsonWebToken);
            }

            if (signinResult.IsNotAllowed)
            {
                if (!await _userManager.IsEmailConfirmedAsync(applicationUser))
                {
                    identityError.Code = "Email";
                    identityError.Description = "The email is not confirmed yet.";
                    return BadRequest(identityError);
                }

                if (!await _userManager.IsPhoneNumberConfirmedAsync(applicationUser))
                {
                    identityError.Code = "PhoneNumber";
                    identityError.Description = "The phone number is not confirmed yet.";
                    return BadRequest(identityError);
                }
            }
            else if (signinResult.IsLockedOut)
            {
                identityError.Code = "LockOut";
                identityError.Description = "The account is locked.";
                return BadRequest(identityError);
            }
            else if (signinResult.RequiresTwoFactor)
            {
                identityError.Code = "TwoFactor";
                identityError.Description = "Require two factor authentication.";
                return BadRequest(identityError);
            }
            else
            {
                identityError.Code = "Passowrd";
                identityError.Description = "Password is incorrect.";
                return BadRequest(identityError);
            }

            return BadRequest(identityError);
        }

        private JsonWebTokenModel GenerateJsonWebToken(ApplicationUser user)
        {
            DateTime utcNow = DateTime.UtcNow;

            Claim[] claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, utcNow.ToString(CultureInfo.InvariantCulture))
            };

            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("Jwt:Key")));
            SigningCredentials signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken jwt = new JwtSecurityToken(
                signingCredentials: signingCredentials,
                claims: claims,
                notBefore: utcNow,
                expires: utcNow.AddSeconds(Configuration.GetValue<int>("Jwt:Lifetime")),
                audience: Configuration.GetValue<string>("Jwt:Issuer"),
                issuer: Configuration.GetValue<string>("Jwt:Issuer")
            );

            string accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            JsonWebTokenModel jsonWebToken = new JsonWebTokenModel()
            {
                UserName = user.UserName,
                Email = user.Email,
                AccessToken = accessToken,
                ExpireIn = Configuration.GetValue<int>("Jwt:Lifetime").ToString(CultureInfo.InvariantCulture)
            };

            return jsonWebToken;
        }
    }
}