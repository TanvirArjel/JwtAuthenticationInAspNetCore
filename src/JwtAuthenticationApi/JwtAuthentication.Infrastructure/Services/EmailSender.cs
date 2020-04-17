using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using JwtAuthentication.Application.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using Attachment = System.Net.Mail.Attachment;

namespace JwtAuthentication.Infrastructure.Services
{
    internal class EmailSender : IEmailSender
    {
        public EmailSender(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public async Task SendEmailAsync(EmailObject emailObject)
        {
            await SendUsingSendGridAsync(emailObject);
        }

        private async Task SendUsingSendGridAsync(EmailObject emailObject)
        {
            ValidateEmailObject(emailObject);

            string apiKey = Configuration.GetSection("SendGrid:ApiKey").Value;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ApplicationException("The send grid api key is either null or empty.");
            }

            SendGridClient sendGridClient = new SendGridClient(apiKey);

            SendGridMessage message = new SendGridMessage()
            {
                From = new EmailAddress(emailObject.SenderEmail, emailObject.SenderName),
                Subject = emailObject.Subject,
                ReplyTo = new EmailAddress(emailObject.SenderEmail, emailObject.SenderName),
                HtmlContent = emailObject.MailBody,
            };

            if (emailObject.Attachments != null && emailObject.Attachments.Any())
            {
                foreach (IFormFile attachment in emailObject.Attachments)
                {
                    if (attachment != null && attachment.Length > 0)
                    {
                        string fileName = Path.GetFileName(attachment.FileName);
                        await message.AddAttachmentAsync(fileName, attachment.OpenReadStream());
                    }
                }
            }

            message.AddTo(new EmailAddress(emailObject.ReceiverEmail, emailObject.ReceiverName));

            Response response = await sendGridClient.SendEmailAsync(message);
        }

        private async Task SendUsingSmtpClientAsync(EmailObject emailObject)
        {
            ValidateEmailObject(emailObject);

            using MailMessage mailMessage = new MailMessage();

            mailMessage.To.Add(new MailAddress(emailObject.ReceiverEmail, emailObject.ReceiverName));
            mailMessage.From = new MailAddress(emailObject.SenderEmail, emailObject.SenderName);
            mailMessage.Subject = emailObject.Subject;
            mailMessage.Body = emailObject.MailBody;
            mailMessage.ReplyToList.Add(new MailAddress(emailObject.SenderEmail, emailObject.SenderName));

            if (emailObject.Attachments != null && emailObject.Attachments.Any())
            {
                foreach (IFormFile attachment in emailObject.Attachments)
                {
                    if (attachment != null && attachment.Length > 0)
                    {
                        string fileName = Path.GetFileName(attachment.FileName);
                        mailMessage.Attachments.Add(new Attachment(attachment.OpenReadStream(), fileName));
                    }
                }
            }

            mailMessage.IsBodyHtml = true;

            using SmtpClient smtp = new SmtpClient
            {
                // Smtp configuration.You can also do it in web.config file
                Host = "smtp.gmail.com",
                EnableSsl = true,
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential("testemail@gmail.com", "password"),
                Port = 587 // This for gmail
            };
            await smtp.SendMailAsync(mailMessage);
        }

        private void ValidateEmailObject(EmailObject emailObject)
        {
            ValidationContext context = new ValidationContext(emailObject, serviceProvider: null, items: null);
            List<ValidationResult> validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(emailObject, context, validationResults, true);

            if (isValid == false)
            {
                ValidationResult validationResult = validationResults.FirstOrDefault();
                string errorMessage = validationResult?.ErrorMessage ?? "EmailObject validation failed!";
                throw new ApplicationException(errorMessage);
            }
        }
    }
}
