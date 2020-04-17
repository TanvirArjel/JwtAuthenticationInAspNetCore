using System.Threading.Tasks;
using AspNetCore.ServiceRegistration.Dynamic.Attributes;
using Microsoft.AspNetCore.Identity;

namespace JwtAuthentication.Application.Services
{
    [ScopedService]
    public interface IAuthenticationService
    {
        Task SendEmailConfirmationCodeAsync(string email);

        Task<bool> HasActiveEmailConfirmationCodeAsync(string email);

        Task<IdentityError> ConfirmEmailAsync(string email, string code);

        Task SendPasswordResetCodeAsync(string email);

        Task<IdentityError> ResetPasswordAsync(string email, string code, string newPassword);
    }
}
