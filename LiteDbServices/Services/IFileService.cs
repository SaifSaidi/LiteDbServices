using LiteDbServices.Repositories;
using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using File = LiteDbServices.Models.File;
namespace LiteDbServices.Services
{
    public interface IFileService : ILiteDBService<Models.File>
    {
        Task<ObjectId> UploadFileAsync(IFormFile formFile);
        Task<(Stream FileContents, string ContentType, string FileName)> DownloadFileAsync(ObjectId id);
    }


    public class FileService : LiteDBService<Models.File>, IFileService
    {
        private readonly LiteDbRepository<File> _repository;

        public FileService(IRepository<File> repository, ICacheService cacheService, ILogger<FileService> logger)
            : base(repository, cacheService, logger)
        {
            _repository = repository as LiteDbRepository<File>;
        }

        public async Task<ObjectId> UploadFileAsync(IFormFile formFile)
        {
            using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var file = new File
            {
                FileName = formFile.FileName,
                ContentType = formFile.ContentType,
                FileSize = formFile.Length,
                UploadDate = DateTime.UtcNow
            };

            var id = await CreateAsync(file);
            await _repository.UploadFileAsync(file.FileName, memoryStream);

            return id;
        }

        public async Task<(Stream FileContents, string ContentType, string FileName)> DownloadFileAsync(ObjectId id)
        {
            var file = await GetByIdAsync(id);
            var (fileStream, contentType) = await _repository.DownloadFileAsync(file.FileName);
            return (fileStream, contentType, file.FileName);
        }

        public new async Task DeleteAsync(ObjectId id)
        {
            var file = await GetByIdAsync(id);
            await _repository.DeleteFileAsync(file.FileName);
            await base.DeleteAsync(id);
        }
    }
}
