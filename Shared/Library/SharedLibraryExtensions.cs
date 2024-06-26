using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Actions;
using SharedLibrary.Actions.Database;
using SharedLibrary.Actions.User;
using SharedLibrary.DomainSpecific;
using SharedLibrary.ExceptionHandling;
using SharedLibrary.Models;
using SharedLibrary.Models.Logging;
using SharedLibrary.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedLibrary;

/// <summary>
/// 
/// </summary>
public static class SharedLibraryConfiguration
{
    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var mongoDb = configuration.GetSection("MongoDB").Get<MongoDbSettings>();

        if (!string.IsNullOrEmpty(mongoDb?.ServerName) && !string.IsNullOrEmpty(mongoDb?.DatabaseName))
        {
            services.AddSingleton(mongoDb);
            services.AddSingleton<IMongoDbDatabaseService, MongoDbDatabaseService>();

            services.AddTransient(typeof(IFilesCollectionFactory<,>), typeof(MongoDbFilesCollectionFactory<,>));
            services.AddTransient(typeof(IFilesCollectionFactory<>), typeof(MongoDbFilesCollectionFactory<>));
            services.AddTransient(typeof(IHistoryCollectionFactory<,>), typeof(MongoDbHistoryCollectionFactory<,>));
            services.AddTransient(typeof(IHistoryCollectionFactory<>), typeof(MongoDbHistoryCollectionFactory<>));
            services.AddTransient(typeof(IObjectCollectionFactory<,>), typeof(MongoDbCollectionFactory<,>));
            services.AddTransient(typeof(IObjectCollectionFactory<>), typeof(MongoDbCollectionFactory<>));

            services.AddTransient<ICounterCollectionFactory, MongoDbCountersFactory>();
        }
        else
        {
            services.AddTransient(typeof(IFilesCollectionFactory<,>), typeof(InMemoryFilesFactory<,>));
            services.AddTransient(typeof(IFilesCollectionFactory<>), typeof(InMemoryFilesFactory<>));
            services.AddTransient(typeof(IHistoryCollectionFactory<,>), typeof(InMemoryHistoryCollectionFactory<,>));
            services.AddTransient(typeof(IHistoryCollectionFactory<>), typeof(InMemoryHistoryCollectionFactory<>));
            services.AddTransient(typeof(IObjectCollectionFactory<,>), typeof(InMemoryCollectionFactory<,>));
            services.AddTransient(typeof(IObjectCollectionFactory<>), typeof(InMemoryCollectionFactory<>));

            services.AddTransient(typeof(NoopCollectionInitializer<>));
            services.AddTransient(typeof(NoopFilesInitializer<>));

            services.AddSingleton(typeof(InMemoryCollection<,>.StateFactory));
            services.AddSingleton(typeof(InMemoryHistoryCollection<,>.InitializerFactory));

            services.AddSingleton(typeof(InMemoryFiles<,>.StateFactory));

            services.AddTransient<ICounterCollectionFactory, InMemoryCountersFactory>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static void UseSharedLibrary(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureDatabase(services, configuration);

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddTransient<ILoggingHttpClient, LoggingHttpClient>();
    }

    /// <summary>
    /// 
    /// </summary>
    public static void UseSharedLibrary(this MvcOptions options)
    {
        options.Filters.Add<DatabaseErrorFilter>();
    }

    /// <summary>
    /// 
    /// </summary>
    public static void UseSharedLibrary(this IMvcBuilder builder)
    {
        builder.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new DomainSpecificNumber.Converter()));
    }

    /// <summary>
    /// 
    /// </summary>
    public static void UseSharedLibrary(this SwaggerGenOptions options)
    {
        options.SchemaFilter<DomainSpecificNumber.Filter>();

        SwaggerModelExtender
            .AddType<InterfaceLogEntry>()
            .AddType<SamDatabaseError>()
            .AddType<SamDetailExtensions>()
            .AddType<SamGlobalErrors>()
            .Register(options);
    }
}