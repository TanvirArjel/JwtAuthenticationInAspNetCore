using System.Threading.Tasks;
using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace JwtAuthentication.Domain.Interfaces
{
    [ScopedService]
    public interface IUnitOfWork
    {
        IRepository<T> Repository<T>()
            where T : class;

        int ExecuteSqlCommand(string sql, params object[] parameters);

        Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters);

        void ResetContextState();

        Task SaveChangesAsync();
    }
}
