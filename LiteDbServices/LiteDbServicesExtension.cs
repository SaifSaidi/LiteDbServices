using LiteDbServices.Repositories;
using LiteDbServices.Services;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using LiteDbServices.Services.Cache;
using LiteDbServices.Models;
using LiteDbServices.Services.Files;

namespace LiteDbServices
{
    public static class LiteDbServicesExtension
    {

        public static IServiceCollection AddLiteDBServices(this IServiceCollection builder, string filename, string connection = "shared", string? password = null)
        {
            ArgumentNullException.ThrowIfNull(builder);

            var connectionString = $"Filename={filename};Connection={connection}";
            if(!string.IsNullOrEmpty(password))
            {
                connection += $";Password={password}";
            }
            // Add LiteDB
            builder.AddSingleton<LiteDatabase>(x => new LiteDatabase(connectionString));

            // Add generic repository
            builder.AddScoped(typeof(IRepository<,>), typeof(LiteDbRepository<,>));

            // Add generic service
            builder.AddScoped(typeof(ILiteDBService<,>), typeof(LiteDBService<,>));

            //builder.AddScoped(typeof(ILiteStorage));
            builder.AddScoped<IFileRepository, LiteDbFilesRepository>();
            builder.AddScoped<IFileService, FileService>();

            // Add memory cache
            builder.AddMemoryCache();
            builder.AddSingleton<ICacheService, MemoryCacheService>();
            return builder;
        }

    }
}
