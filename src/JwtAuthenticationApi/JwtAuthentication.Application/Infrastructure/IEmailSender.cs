using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AspNetCore.ServiceRegistration.Dynamic.Interfaces;
using Microsoft.AspNetCore.Http;

namespace JwtAuthentication.Application.Infrastructure
{
    public interface IEmailSender : IScopedService
    {
        Task SendEmailAsync(EmailObject emailObject);
    }

    public class EmailObject
    {
        [Required]
        public string ReceiverEmail { get; set; }

        [Required]
        public string ReceiverName { get; set; }

        [Required]
        public string SenderEmail { get; set; }

        [Required]
        public string SenderName { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string MailBody { get; set; }

        public List<IFormFile> Attachments { get; } = new List<IFormFile>();
    }
}
