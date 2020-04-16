using System.ComponentModel.DataAnnotations;

namespace JwtAuthentication.Api.ApiModels
{
    public class UserRegistrationModel
    {
        [Required]
        [EmailAddress]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Email should be between 5 to 50 characters")]
        public string Email { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "User Name should be between 5 to 20 characters")]
        [RegularExpression(@"^[a-zA-Z]([a-zA-Z0-9])+$", ErrorMessage = "User Name should starts with a character and contain only characters and numbers!")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
