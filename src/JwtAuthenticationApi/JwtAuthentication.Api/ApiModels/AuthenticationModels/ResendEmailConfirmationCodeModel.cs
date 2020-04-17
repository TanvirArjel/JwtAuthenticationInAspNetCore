using System.ComponentModel.DataAnnotations;

namespace JwtAuthentication.Api.ApiModels.AuthenticationModels
{
    public class ResendEmailConfirmationCodeModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
