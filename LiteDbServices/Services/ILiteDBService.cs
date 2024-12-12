using LiteDbServices.Models;
using LiteDB;
using System.Linq.Expressions;
 
namespace LiteDbServices.Services
{ 
    public interface ILiteDBService<T> where T : ILiteEntity
    {
        Task<T> GetByIdAsync(ObjectId id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<ObjectId> CreateAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(ObjectId id);
        Task<bool> ExistsAsync(ObjectId id);
        Task<T> GetOneByQueryAsync(Expression<Func<T, bool>> query);
    }
}
