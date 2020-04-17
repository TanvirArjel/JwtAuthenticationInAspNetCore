using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthentication.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthentication.Infrastructure.Data.Repositories
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly JwtAuthenticationDbContext _dbContext;
        private Hashtable _repositories;

        public UnitOfWork(JwtAuthenticationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IRepository<T> Repository<T>()
            where T : class
        {
            if (_repositories == null)
            {
                _repositories = new Hashtable();
            }

            string type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                Type repositoryType = typeof(Repository<>);

                object repositoryInstance =
                    Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _dbContext);

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)_repositories[type];
        }

        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return _dbContext.Database.ExecuteSqlRaw(sql, parameters);
        }

        public async Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
        {
            return await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public void ResetContextState()
        {
            _dbContext.ChangeTracker.Entries().Where(e => e.Entity != null).ToList()
                .ForEach(e => e.State = EntityState.Detached);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
