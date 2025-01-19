using LiteDbServices.Repositories;
using LiteDB;
using Microsoft.AspNetCore.Http;
using LiteDbServices.Models;
using System.Linq.Expressions;

namespace LiteDbServices.Services.Files
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _repository;

        public FileService(IFileRepository repository)
        {
            _repository = repository;
        }

        public ILiteStorage<string> Storage => _repository.Storage;

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            return await _repository.DeleteFileAsync(fileName);
        }

        public async Task<(Stream FileContents, string ContentType, string FileName)> DownloadFileAsync(string fileName)
        {
            return await _repository.DownloadFileAsync(fileName);
        }

        public async Task<LiteFileInfo<string>> GetFileInfoAsync(string fileName)
        {
            return await _repository.GetFileInfoAsync(fileName);
        }

        public async Task<IEnumerable<LiteFileInfo<string>>> ListFilesAsync(Expression<Func<LiteFileInfo<string>, bool>> predicate)
        {
            return await _repository.ListFilesAsync(predicate);
        }

        public async Task<IEnumerable<LiteFileInfo<string>>> ListFilesAsync()
        {
            return await _repository.ListFilesAsync();
        }

        public async Task<LiteFileInfo<string>> UploadFileAsync(IFormFile formFile)
        {
            return await _repository.UploadFileAsync(formFile).ConfigureAwait(false);
        }
    }

}
