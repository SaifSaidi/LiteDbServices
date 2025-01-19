using LiteDB;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace LiteDbServices.Repositories
{
    public interface IFileRepository
    {
        ILiteStorage<string> Storage { get; init; }

        Task<LiteFileInfo<string>> UploadFileAsync(IFormFile formFile);
        Task<(Stream FileContents, string ContentType, string FileName)> DownloadFileAsync(string fileName);
        Task<bool> DeleteFileAsync(string fileName);
        Task<IEnumerable<LiteFileInfo<string>>> ListFilesAsync();
        Task<LiteFileInfo<string>> GetFileInfoAsync(string fileName);
        Task<IEnumerable<LiteFileInfo<string>>> ListFilesAsync(Expression<Func<LiteFileInfo<string>, bool>> predicate);
    }


}
