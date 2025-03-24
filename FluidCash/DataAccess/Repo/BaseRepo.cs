using FluidCash.DataAccess.DbContext;
using FluidCash.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
namespace FluidCash.DataAccess.Repo;

public sealed class BaseRepo<T> : IBaseRepo<T> where T : class, IBaseEntity
{
    private readonly DataContext _dataContext;
    private readonly DbSet<T> _dbSet;
    private IDbContextTransaction? _currentTransaction;

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

    public async Task<bool>
        ExistsByConditionAsync
        (Expression<Func<T, bool>> condition)
    {
        return await _dbSet.Where(t => !t.IsDeleted.Value)
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

    public async Task BeginTransactionAsync()
    {
        _currentTransaction = await _dataContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.CommitAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _dataContext.SaveChangesAsync();
    }
}
