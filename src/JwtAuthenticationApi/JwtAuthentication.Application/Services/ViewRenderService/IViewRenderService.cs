using System.Threading.Tasks;
using AspNetCore.ServiceRegistration.Dynamic.Interfaces;

namespace JwtAuthentication.Application.Services.ViewRenderService
{
    public interface IViewRenderService : IScopedService
    {
        Task<string> RenderViewToStringAsync(string viewNameOrPath, object model);
    }
}
