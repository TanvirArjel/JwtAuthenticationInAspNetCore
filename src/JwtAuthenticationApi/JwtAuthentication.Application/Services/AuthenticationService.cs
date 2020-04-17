using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthentication.Application.Infrastructure;
using JwtAuthentication.Application.Services.ViewRenderService;
using JwtAuthentication.Domain.Entities;
using JwtAuthentication.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthentication.Application.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IViewRenderService _viewRenderService;
        private readonly IEmailSender _emailSender;
        private IPasswordHasher<ApplicationUser> _passwordHasher;

        public AuthenticationService(
            IUnitOfWork unitOfWork,
            IViewRenderService viewRenderService,
            IEmailSender emailSender,
            IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _viewRenderService = viewRenderService;
            _emailSender = emailSender;
            _passwordHasher = passwordHasher;
        }

        public async Task SendEmailConfirmationCodeAsync(string email)
        {
            try
            {
                Random generator = new Random();
                string verificationCode = generator.Next(0, 1000000).ToString("D6", CultureInfo.InvariantCulture);

                EmailConfirmationCode emailVerificationCode = new EmailConfirmationCode()
                {
                    Code = verificationCode,
                    Email = email,
                    SentAtUtc = DateTime.UtcNow
                };
                await _unitOfWork.Repository<EmailConfirmationCode>().InsertEntityAsync(emailVerificationCode);
                await _unitOfWork.SaveChangesAsync();

                (string email, string verificationCode) model = (email, verificationCode);
                string emailBody = await _viewRenderService.RenderViewToStringAsync("EmailTemplates/ConfirmRegistrationTemplate", model);
                EmailObject emailObject = new EmailObject()
                {
                    ReceiverEmail = email,
                    ReceiverName = email,
                    SenderEmail = "noreply@glotsalot.com",
                    SenderName = "Glotsalot",
                    Subject = "Gltosalot Registration",
                    MailBody = emailBody
                };
                await _emailSender.SendEmailAsync(emailObject);
            }
            catch (Exception exception)
            {
                // Handle exception here.
                throw;
            }
        }

        public async Task<bool> HasActiveEmailConfirmationCodeAsync(string email)
        {
            try
            {
                bool isExists = await _unitOfWork.Repository<EmailConfirmationCode>()
                .IsEntityExistsAsync(evc => evc.SentAtUtc.AddMinutes(5) > DateTime.UtcNow);
                return isExists;
            }
            catch (Exception exception)
            {
                // Log exception here.
                throw;
            }
        }

        public async Task<IdentityError> ConfirmEmailAsync(string email, string code)
        {
            try
            {
                IdentityError identityError = null;

                EmailConfirmationCode emailVerificationCode = await _unitOfWork.Repository<EmailConfirmationCode>().Entities
                    .Where(evc => evc.Email == email && evc.Code == code && evc.UsedAtUtc == null).FirstOrDefaultAsync();

                if (emailVerificationCode == null)
                {
                    identityError = new IdentityError()
                    {
                        Code = "Code",
                        Description = "Either email or password reset code is incorrect."
                    };

                    return identityError;
                }

                if (DateTime.UtcNow > emailVerificationCode.SentAtUtc.AddMinutes(5))
                {
                    identityError = new IdentityError()
                    {
                        Code = "Code",
                        Description = "The code is expired."
                    };

                    return identityError;
                }

                ApplicationUser applicationUser = await _unitOfWork.Repository<ApplicationUser>().Entities
                    .Where(au => au.Email == email).FirstOrDefaultAsync();

                if (applicationUser == null)
                {
                    identityError = new IdentityError()
                    {
                        Code = "Email",
                        Description = "The provided email is not related to any account."
                    };
                }
                else
                {
                    applicationUser.EmailConfirmed = true;
                    _unitOfWork.Repository<ApplicationUser>().UpdateEntity(applicationUser);

                    emailVerificationCode.UsedAtUtc = DateTime.UtcNow;
                    _unitOfWork.Repository<EmailConfirmationCode>().UpdateEntity(emailVerificationCode);

                    await _unitOfWork.SaveChangesAsync();
                }

                return identityError;
            }
            catch (Exception exception)
            {
                // Log exception here.
                throw;
            }
        }

        public async Task SendPasswordResetCodeAsync(string email)
        {
            try
            {
                Random generator = new Random();
                string verificationCode = generator.Next(0, 1000000).ToString("D6", CultureInfo.InvariantCulture);

                PasswordResetCode emailVerificationCode = new PasswordResetCode()
                {
                    Code = verificationCode,
                    Email = email,
                    SentAtUtc = DateTime.UtcNow
                };
                await _unitOfWork.Repository<PasswordResetCode>().InsertEntityAsync(emailVerificationCode);
                await _unitOfWork.SaveChangesAsync();

                (string email, string verificationCode) model = (email, verificationCode);
                string emailBody = await _viewRenderService.RenderViewToStringAsync("EmailTemplates/PasswordResetTemplate", model);
                EmailObject emailObject = new EmailObject()
                {
                    ReceiverEmail = email,
                    ReceiverName = email,
                    SenderEmail = "noreply@glotsalot.com",
                    SenderName = "Glotsalot",
                    Subject = "Reset Password",
                    MailBody = emailBody
                };
                await _emailSender.SendEmailAsync(emailObject);
            }
            catch (Exception exception)
            {
                // Log exception here.
                throw;
            }
        }

        public async Task<IdentityError> ResetPasswordAsync(string email, string code, string newPassword)
        {
            try
            {
                IdentityError identityError = null;

                PasswordResetCode emailVerificationCode = await _unitOfWork.Repository<PasswordResetCode>().Entities
                    .Where(evc => evc.Email == email && evc.Code == code && evc.UsedAtUtc == null).FirstOrDefaultAsync();

                if (emailVerificationCode == null)
                {
                    identityError = new IdentityError()
                    {
                        Code = "Code",
                        Description = "Either email or password reset code is incorrect."
                    };

                    return identityError;
                }

                if (DateTime.UtcNow > emailVerificationCode.SentAtUtc.AddMinutes(5))
                {
                    identityError = new IdentityError()
                    {
                        Code = "Code",
                        Description = "The code is expired."
                    };
                }
                else
                {
                    ApplicationUser applicationUser = await _unitOfWork.Repository<ApplicationUser>().Entities
                        .Where(au => au.Email == email).FirstOrDefaultAsync();

                    if (applicationUser == null)
                    {
                        identityError = new IdentityError()
                        {
                            Code = "Email",
                            Description = "The provided email is not related to any account."
                        };
                    }
                    else
                    {
                        string newHashedPassword = _passwordHasher.HashPassword(applicationUser, newPassword);
                        applicationUser.PasswordHash = newHashedPassword;
                        _unitOfWork.Repository<ApplicationUser>().UpdateEntity(applicationUser);

                        emailVerificationCode.UsedAtUtc = DateTime.UtcNow;
                        _unitOfWork.Repository<PasswordResetCode>().UpdateEntity(emailVerificationCode);

                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                return identityError;
            }
            catch (Exception exception)
            {
                // Log exception here.
                throw;
            }
        }
    }
}
