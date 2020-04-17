using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JwtAuthentication.Api.ApiModels.AuthenticationModels;
using JwtAuthentication.Application.Services;
using JwtAuthentication.Domain.Entities;
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
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAuthenticationService authenticationService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authenticationService = authenticationService;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegistrationModel registerModel)
        {
            ApplicationUser applicationUser = new ApplicationUser { UserName = registerModel.UserName, Email = registerModel.Email };
            IdentityResult identityResult = await _userManager.CreateAsync(applicationUser, registerModel.Password);

            if (identityResult.Succeeded)
            {
                await _authenticationService.SendEmailConfirmationCodeAsync(applicationUser.Email);
                return Ok();
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

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResendEmailConfirmationCode(ResendEmailConfirmationCodeModel model)
        {
            ApplicationUser applicationUser = await _userManager.FindByEmailAsync(model.Email);
            IdentityError identityError = new IdentityError();

            if (applicationUser == null)
            {
                identityError.Code = "Email";
                identityError.Description = "Provided email is not related to any account.";
                return BadRequest(identityError);
            }

            if (applicationUser.EmailConfirmed)
            {
                identityError.Code = "Email";
                identityError.Description = "Email is already confirmed.";
                return BadRequest(identityError);
            }

            bool isExists = await _authenticationService.HasActiveEmailConfirmationCodeAsync(model.Email);

            if (isExists)
            {
                identityError.Code = "Email";
                identityError.Description = "You already have an active code. Please wait! You may receive the code in your email. If not, please try again after sometimes.";
                return BadRequest(identityError);
            }

            await _authenticationService.SendEmailConfirmationCodeAsync(model.Email);
            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(EmailConfirmationModel model)
        {
            IdentityError identityError = await _authenticationService.ConfirmEmailAsync(model.Email, model.Code);

            if (identityError == null)
            {
                return Ok();
            }

            return BadRequest(identityError);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
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

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SendPasswordResetCode(ForgotPasswordModel forgotPasswordModel)
        {
            await _authenticationService.SendPasswordResetCodeAsync(forgotPasswordModel.Email);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            IdentityError identityError = await _authenticationService.ResetPasswordAsync(model.Email, model.Code, model.Password);
            if (identityError != null)
            {
                return BadRequest(identityError);
            }

            return Ok();
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
                issuer: Configuration.GetValue<string>("Jwt:Issuer"));

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