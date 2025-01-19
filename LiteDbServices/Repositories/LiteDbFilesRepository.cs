using LiteDbServices.Exceptions;

namespace LiteDbServices.Repositories
{
    using LiteDB;
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq.Expressions;
    using System.Runtime.Serialization.Formatters;
    using System.Threading.Tasks;

    public class LiteDbFilesRepository : IFileRepository
    {
         
        private readonly ILiteStorage<string> _storage;
        public LiteDbFilesRepository(LiteDatabase db)
        { 
            _storage = db?.FileStorage ?? throw new Exception(nameof(db));
            this.Storage = _storage;
        }

        public ILiteStorage<string> Storage { get; init; } 

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            var result = await Task.FromResult(_storage.Delete(fileName));
            if (!result)
            {
                throw new EntityNotFoundException("FileStorage", fileName);
            }
            return result;
        }

        public async Task<(Stream FileContents, string ContentType, string FileName)> DownloadFileAsync(string fileName)
        {
            var fileInfo = _storage.FindById(fileName);
            if (fileInfo == null)
            {
                throw new EntityNotFoundException("FileStorage", fileName);
            }
            return await Task.FromResult((fileInfo.OpenRead(), fileInfo.MimeType, fileInfo.Filename));
        }

        public async Task<LiteFileInfo<string>> GetFileInfoAsync(string fileName)
        {
            var fileInfo = _storage.FindById(fileName);
            if (fileInfo == null)
            {
                throw new EntityNotFoundException("FileStorage", fileName);
            }
            return await Task.FromResult(fileInfo);
        }
         public async Task<IEnumerable<LiteFileInfo<string>>> ListFilesAsync(Expression<Func<LiteFileInfo<string>, bool>> predicate)
        {
            var fileInfo = _storage.Find(predicate);
           
            return await Task.FromResult(fileInfo);
        }

        public async Task<IEnumerable<LiteFileInfo<string>>> ListFilesAsync()
        {
            var files = _storage.FindAll();
            return await Task.FromResult(files);
        }

        public async Task<LiteFileInfo<string>> UploadFileAsync(IFormFile formFile)
        {
            using var stream = formFile.OpenReadStream();
            

            var fileInfo = _storage.Upload(formFile.FileName, formFile.FileName, stream);
            return await Task.FromResult(fileInfo);
        }
         
    }

     

}
