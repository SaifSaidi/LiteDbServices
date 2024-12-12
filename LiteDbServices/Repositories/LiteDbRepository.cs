using LiteDbServices.Exceptions;
using LiteDbServices.Models;
using LiteDB;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace LiteDbServices.Repositories
{

    public class LiteDbRepository<T> : IRepository<T> where T : ILiteEntity
    {
        private readonly ILiteDatabase _database;
        private readonly ILiteCollection<T> _collection;
        private readonly ILiteStorage<string> _storage;

        public LiteDbRepository(ILiteDatabase database)
        {
            _database = database;
            _collection = database.GetCollection<T>(typeof(T).Name);
            _storage = database.FileStorage;
        }


        public async Task<T> GetByIdAsync(ObjectId id)
        { 
            var entity = await Task.FromResult(_collection.FindById(id));
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(T).Name, id);
            }
            return entity;
        }

        public async Task<T> GetOneByQueryAsync(Expression<Func<T, bool>> query)
        { 
            var entity = await Task.FromResult(_collection.FindOne(query));
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(T).Name, entity!.Id);
            }
            return entity;
        }

        public async Task<IEnumerable<T>> GetAllByQueryAsync(Expression<Func<T, bool>> query,
            int skip = 0, int limit = int.MaxValue)
        {  
            return await Task.FromResult(_collection.Find(query, skip, limit));
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Task.FromResult(_collection.FindAll());
        }

        public async Task<ObjectId> CreateAsync(T entity)
        {
            var validationResults = entity.Validate();
            if (validationResults.Any())
            {
                throw new ValidationException(string.Join(", ", validationResults.Select(r => r.ErrorMessage)));
            }
            var bsonValue = _collection.Insert(entity); 
            return await Task.FromResult(bsonValue);
        }

        public async Task UpdateAsync(T entity)
        {
            var validationResults = entity.Validate();
            if (validationResults.Any())
            {
                throw new ValidationException(string.Join(", ", validationResults.Select(r => r.ErrorMessage)));
            }
            var result = await Task.FromResult(_collection.Update(entity));
            if (!result)
            {
                throw new EntityNotFoundException(typeof(T).Name, entity.Id);
            }
        }

        public async Task DeleteAsync(ObjectId id)
        {
            var result = await Task.FromResult(_collection.Delete(id));
            if (!result)
            {
                throw new EntityNotFoundException(typeof(T).Name, id);
            }
        }

        public async Task<bool> ExistsAsync(ObjectId id)
        {
            return await Task.FromResult(_collection.Exists(x => x.Id == id));
        }

        public async Task<LiteFileInfo<string>> UploadFileAsync(string fileName, Stream fileStream)
        {
            LiteFileInfo<string> file = _storage.Upload(fileName, fileName, fileStream);
            return await Task.FromResult(file);
        }

        public async Task<(Stream FileStream, string ContentType)> DownloadFileAsync(string fileName)
        {
            var fileInfo = _storage.FindById(fileName);
            if (fileInfo == null)
            {
                throw new EntityNotFoundException("File", fileName);
            }
            return await Task.FromResult((fileInfo.OpenRead(), fileInfo.MimeType));
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var result = await Task.FromResult(_storage.Delete(fileName));
            if (!result)
            {
                throw new EntityNotFoundException("File", fileName);
            }
        }

    }

}
