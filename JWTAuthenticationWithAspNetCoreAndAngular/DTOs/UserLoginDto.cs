using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JWTAuthenticationWithAspNetCoreAndAngular.DTOs
{
    public class UserLoginDto
    {
        [Required]
        public String Username { get; set; }

        [Required]
        public String Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
