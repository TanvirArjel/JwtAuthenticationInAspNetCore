using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTAuthenticationWithAspNetCoreAndAngular.DTOs
{
    public class JsonWebToken
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public string ExpireIn { get; set; }
    }
}
