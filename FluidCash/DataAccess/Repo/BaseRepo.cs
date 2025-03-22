using FluidCash.DataAccess.DbContext;
using FluidCash.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace FluidCash.DataAccess.Repo;

public sealed class BaseRepo<T> : IBaseRepo<T> where T : class, IBaseEntity
{
    private readonly DataContext _dataContext;
    private readonly DbSet<T> _dbSet;

    public BaseRepo(DataContext dataContext)
    {
        _dataContext = dataContext;
        _dbSet = _dataContext.Set<T>();
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
            .Where(t => !t.IsDeleted)
            .AsNoTracking();
    }

    public IQueryable<T>
        GetNonDeletedByCondition
        (Expression<Func<T, bool>> condition)
    {
        return _dbSet.Where(condition)
            .Where(t => !t.IsDeleted);

    }

    public async Task<bool>
        ExistsByConditionAsync
        (Expression<Func<T, bool>> condition)
    {
        return await _dbSet.Where(t => !t.IsDeleted)
            .AnyAsync(condition);

    }

    public async Task 
        AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task 
        AddRangeAsync 
        (IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void 
        UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);        
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public void
        SoftDelete(T entity)
    {
        entity.IsDeleted = true;
    }

    public void
        SoftDeleteRange
        (IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _dataContext.SaveChangesAsync();
    }
}
