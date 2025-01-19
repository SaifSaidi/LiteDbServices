using LiteDbServices.Models;
namespace LiteDbServices.Repositories
{
    using LiteDB;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    public class LiteDbRepository<T, TKey> : IRepository<T, TKey> where T : LiteEntity<TKey>
    {
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<T> _collection;

        private TKey GetGenerateId()
        {
            if (typeof(TKey) == typeof(Guid))
            {
                return (TKey)(object)Guid.NewGuid();
            }
            else if (typeof(TKey) == typeof(ObjectId))
            {
                return (TKey)(object)ObjectId.NewObjectId();
            }
            else if (typeof(TKey) == typeof(Int32))
            {
                return (TKey)(object)new BsonValue().AsInt32;
            }
            else if (typeof(TKey) == typeof(Int64))
            {
                return (TKey)(object)new BsonValue().AsInt64;
            }
            else
            {
                throw new InvalidOperationException($"Cannot generate ID for type {typeof(TKey)}. Please provide an ID.");
            }
        }

        public LiteDbRepository(LiteDatabase db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            var collectionName = typeof(T).Name;
            _collection = _db.GetCollection<T>(collectionName);
             
            var database = db.GetCollection("$database");

            var databaseInfo = database.FindAll();

            foreach (var info in databaseInfo)
            {
                Console.WriteLine($"database: {info}"); 
                Console.WriteLine();
            }

            var indexes = db.GetCollection("$indexes");
            var allIndexes = indexes.FindAll();

            foreach (var index in allIndexes)
            {
                Console.WriteLine($"Collection: {index}");
                Console.WriteLine();
            }
        }

        public async Task<T> GetByIdAsync(TKey id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            return await Task.FromResult(_collection.FindById(new BsonValue(id)));
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Task.FromResult(_collection.FindAll());
        }

        public async Task<T> CreateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if(entity.Id == null )
            {
                entity.Id = GetGenerateId();

            }
            _collection.Insert(entity);
            return await Task.FromResult(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var updated = _collection.Update(new BsonValue(entity.Id), entity);
            if (!updated)
            {
                throw new KeyNotFoundException($"Entity with ID {entity.Id} not found.");
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(TKey id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var deleted = _collection.Delete(new BsonValue(id));
            if (!deleted)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found.");
            }
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(TKey id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id)); 
            return await Task.FromResult(_collection.Exists(Query.EQ("_id", new BsonValue(id))));
        }
        public async Task<bool> ExistsAsync(BsonExpression BsonExpression)
        {

            if (BsonExpression == null) throw new ArgumentNullException(nameof(BsonExpression));
            return await Task.FromResult(_collection.Exists(BsonExpression));
        }

       
        public async Task<int> CountAsync()
        {
            return await Task.FromResult(_collection.Count());
        }

        public async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var results = _collection.Find(Query.All(), (pageNumber - 1) * pageSize, pageSize);
            return await Task.FromResult(results);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var results = _collection.Find(predicate, skip, limit);
            return await Task.FromResult(results);
        }
         public async Task<IEnumerable<T>> FindAsync(BsonExpression BsonExpression, int skip = 0, int limit = int.MaxValue)
        {
            if (BsonExpression == null) throw new ArgumentNullException(nameof(BsonExpression));
            var results = _collection.Find(BsonExpression, skip, limit);
            return await Task.FromResult(results);
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var result = _collection.FindOne(predicate);
            return await Task.FromResult(result);
        }
        public async Task<T> FirstOrDefaultAsync(BsonExpression BsonExpression)
        {
            if (BsonExpression == null) throw new ArgumentNullException(nameof(BsonExpression));
            var result = _collection.FindOne(BsonExpression);
            return await Task.FromResult(result);
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var result = _collection.FindOne(predicate);
            return await Task.FromResult(result);
        } public async Task<T> SingleOrDefaultAsync(BsonExpression BsonExpression)
        {
            if (BsonExpression == null) throw new ArgumentNullException(nameof(BsonExpression));
            var result = _collection.FindOne(BsonExpression);
            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<T>> CreateBulkAsync(IEnumerable<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                entity.Id = GetGenerateId();
            }

            _collection.InsertBulk(entities);
            return await Task.FromResult(entities);
        }

        public async Task UpdateBulkAsync(IEnumerable<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                var updated = _collection.Update(entity);
                if (!updated)
                {
                    throw new KeyNotFoundException($"Entity with ID {entity.Id} not found.");
                }
            }
            await Task.CompletedTask;
        }

        public async Task DeleteBulkAsync(IEnumerable<TKey> ids)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));

            foreach (var id in ids)
            {
                var deleted = _collection.Delete(new BsonValue(id));
                if (!deleted)
                {
                    throw new KeyNotFoundException($"Entity with ID {id} not found.");
                }
            }
            await Task.CompletedTask;
        }
        
        public async Task<int> DeleteBulkAsync(BsonExpression BsonExpression)
        {
            var deleted = _collection.DeleteMany(BsonExpression);
            if (deleted > 0)
            {
                return await Task.FromResult(deleted);
            }
            return await Task.FromResult(0);
        }

        public async Task<int> ClearAsync()
        {
            var deleted = _collection.DeleteAll();
            if (deleted > 0)
            {
                return await Task.FromResult(deleted);
            }
            return await Task.FromResult(0);
        }

        public async Task IncludeAsync<TInclude>(Expression<Func<T, TInclude>> includeExpression)
        {
            if (includeExpression == null) throw new ArgumentNullException(nameof(includeExpression));
            _collection.Include(includeExpression);
            await Task.CompletedTask;
        }

        public async Task UpsertAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _collection.Upsert(entity);
            await Task.CompletedTask;
        }

        public async Task<bool> EnsureIndexAsync(Expression<Func<T, object>> indexExpression, bool unique = false)
        {
            if (indexExpression == null) throw new ArgumentNullException(nameof(indexExpression));
            
           return await Task.FromResult(_collection.EnsureIndex(indexExpression, unique));
             
        }

        public async Task<ILiteQueryable<T>> QueryAsync()
        {
            return await Task.FromResult(_collection.Query());
        }
    }

     

}
