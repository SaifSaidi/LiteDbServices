using LiteDbServices.Models;
using LiteDB;
using System.Linq.Expressions;
 
namespace LiteDbServices.Services
{ 
    public interface ILiteDBService<T, TKey> where T : LiteEntity<TKey>
    {
        Task<T> GetByIdAsync(TKey id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> CreateAsync(T entity);
        Task UpsertAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(TKey id);
        Task<bool> ExistsAsync(TKey id);
        Task<bool> ExistsAsync(BsonExpression predicate);
        Task<int> CountAsync();
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);

        // Bulk operations
        Task<IEnumerable<T>> CreateBulkAsync(IEnumerable<T> entities);
        Task UpdateBulkAsync(IEnumerable<T> entities);
        Task DeleteBulkAsync(IEnumerable<TKey> ids);
        Task<bool> EnsureIndexAsync(Expression<Func<T, object>> indexExpression, bool unique = false);
        Task IncludeAsync<TInclude>(Expression<Func<T, TInclude>> includeExpression);
        Task<ILiteQueryable<T>> QueryAsync();
        Task<IEnumerable<T>> FindAsync(BsonExpression BsonExpression, int skip = 0, int limit = int.MaxValue);
        Task<T> FirstOrDefaultAsync(BsonExpression BsonExpression);
        Task<T> SingleOrDefaultAsync(BsonExpression BsonExpression);
        Task<int> ClearAsync();
        Task<int> DeleteBulkAsync(BsonExpression BsonExpression);

    }
}
