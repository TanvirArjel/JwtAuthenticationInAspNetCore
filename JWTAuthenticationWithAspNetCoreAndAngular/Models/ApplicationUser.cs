using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace JWTAuthenticationWithAspNetCoreAndAngular.Models
{
    public class ApplicationUser : IdentityUser<long>
    {
    }
}
