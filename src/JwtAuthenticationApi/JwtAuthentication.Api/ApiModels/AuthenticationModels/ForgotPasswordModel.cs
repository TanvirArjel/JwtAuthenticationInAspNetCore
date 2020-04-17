using System.ComponentModel.DataAnnotations;

namespace JwtAuthentication.Api.ApiModels.AuthenticationModels
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
