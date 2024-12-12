using LiteDbServices.Models;
using LiteDB; 
using System.Linq.Expressions; 

namespace LiteDbServices.Repositories
{
     
    public interface IRepository<T> where T : ILiteEntity
    {
        Task<T> GetByIdAsync(ObjectId id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<ObjectId> CreateAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(ObjectId id);
        Task<bool> ExistsAsync(ObjectId id); 
        Task<T> GetOneByQueryAsync(Expression<Func<T, bool>> query);
        Task<IEnumerable<T>> GetAllByQueryAsync(Expression<Func<T, bool>> query, int skip = 0, int limit = int.MaxValue);
        Task DeleteFileAsync(string fileName);
        Task<(Stream FileStream, string ContentType)> DownloadFileAsync(string fileName);
        Task<LiteFileInfo<string>> UploadFileAsync(string fileName, Stream fileStream);

    }


}
