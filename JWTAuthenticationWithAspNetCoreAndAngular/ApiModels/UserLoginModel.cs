using System;
using System.ComponentModel.DataAnnotations;

namespace JWTAuthenticationWithAspNetCoreAndAngular.ApiModels
{
    public class UserLoginModel
    {
        [Required]
        public String Username { get; set; }

        [Required]
        public String Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
