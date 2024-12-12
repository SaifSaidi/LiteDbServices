using LiteDbServices.Repositories;
using LiteDbServices.Services;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;

namespace LiteDbServices
{
    public static class ServicesExtension
    {

        public static IServiceCollection AddLiteDBServices(this IServiceCollection builder, string filename, string connection = "shared")
        {
            ArgumentNullException.ThrowIfNull(builder);
            // Add LiteDB
            builder.AddSingleton<ILiteDatabase>(x => new LiteDatabase($"Filename={filename};Connection={connection}"));

            // Add generic repository
            builder.AddScoped(typeof(IRepository<>), typeof(LiteDbRepository<>));

            // Add generic service
            builder.AddScoped(typeof(ILiteDBService<>), typeof(LiteDBService<>));

            builder.AddScoped<IFileService, FileService>();
            // Add cache
            builder.AddMemoryCache();
            builder.AddSingleton<ICacheService, MemoryCacheService>();

            return builder;
        }

    }
}
