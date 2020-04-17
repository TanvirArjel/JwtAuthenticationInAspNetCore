using System;

namespace JwtAuthentication.Domain.Entities
{
    public class EmailConfirmationCode
    {
        public long Id { get; set; }

        public string Email { get; set; }

        public string Code { get; set; }

        public DateTime SentAtUtc { get; set; }

        public DateTime? UsedAtUtc { get; set; }
    }
}
