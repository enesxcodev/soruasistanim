using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SoruHavuzu.Domain.Entities.Base;
using SoruHavuzu.Domain.Interfaces;
using SoruHavuzu.Infrastructure.Persistence;

namespace SoruHavuzu.Infrastructure.Repositories;

public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T> where T : PureEntity
{
    protected readonly AppDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> Where(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<IReadOnlyList<T>> FindAsyncInclude(
      Expression<Func<T, bool>> predicate,
      Func<IQueryable<T>, IQueryable<T>>? include = null,
      CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsQueryable();

        if (include != null)
        {
            query = include(query);
        }

        return await query
            .Where(predicate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
