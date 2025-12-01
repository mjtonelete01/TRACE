using TraceWebApi.Models.Common;

namespace TraceWebApi.Interfaces;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(Guid id);
    TEntity? Get(Guid id);
    IEnumerable<TEntity> GetList();
}
