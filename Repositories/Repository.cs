using Microsoft.EntityFrameworkCore;
using TraceWebApi.EntityFrameworkCore;
using TraceWebApi.Interfaces;
using TraceWebApi.Models.Common;

namespace TraceWebApi.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    private readonly TraceAppDbContext _context;

    public Repository(TraceAppDbContext context)
    {
        _context = context;
    }

    public void Add(TEntity entity)
    {
        _context.Add(entity);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var entity = _context.Find<TEntity>(id);
        if (entity is not null)
        {
            _context.Remove(entity);
            _context.SaveChanges();
        }
    }

    public TEntity? Get(Guid id)
    {
        return _context.Find<TEntity>(id);
    }

    public IEnumerable<TEntity> GetList()
    {
        return _context.Set<TEntity>().ToList();
    }

    public void Update(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
        _context.SaveChanges();
    }
}
