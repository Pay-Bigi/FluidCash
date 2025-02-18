using FluidCash.DataAccess.DbContext;
using FluidCash.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace FluidCash.DataAccess.Repo;

public sealed class BaseRepo<T> : IBaseRepo<T> where T : class, IBaseEntity
{
        private readonly DbSet<T> _dbSet;

        public BaseRepo(DataContext dataContext)
        {
            _dbSet = dataContext.Set<T>();
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet.AsNoTracking();
        }

        public IQueryable<T>
            GetByCondition
            (Expression<Func<T, bool>> conditionExpression)
        {
            return _dbSet.Where(conditionExpression)
                .AsNoTracking();
        }

        public IQueryable<T>
            GetAllNonDeleted()
        {
            return _dbSet
                .Where(t => !t.IsDeleted.Value)
                .AsNoTracking();
        }

        public IQueryable<T>
            GetNonDeletedByCondition
            (Expression<Func<T, bool>> condition)
        {
            return _dbSet.Where(condition)
                .Where(t => !t.IsDeleted.Value)
                .AsNoTracking();

        }
}
