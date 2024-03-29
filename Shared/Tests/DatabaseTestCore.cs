using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SharedLibrary.Actions.Database;
using SharedLibrary.Models;
using SharedLibrary.Services;

namespace SharedLibraryTests;

public abstract class DatabaseTestCore
{
    protected abstract bool UseMongoDb { get; }

    protected ServiceProvider Services;

    [TearDown]
    public void Teardown()
    {
        Services?.Dispose();
    }

    protected abstract void OnSetupServices(IServiceCollection services);

    protected abstract Task OnPostSetup();

    [SetUp]
    public async Task Setup()
    {
        if (UseMongoDb && Environment.GetEnvironmentVariable("EXECUTE_MONGODB_NUNIT_TESTS") != "yes")
        {
            Assert.Ignore("not runnig database tests");

            return;
        }

        var services = new ServiceCollection();

        services.AddLogging(l => l.AddProvider(NullLoggerProvider.Instance));

        OnSetupServices(services);

        if (UseMongoDb)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.test.json")
                .Build();

            services.AddSingleton(configuration.GetSection("MongoDB").Get<MongoDbSettings>()!);

            services.AddSingleton<IMongoDbDatabaseService, MongoDbDatabaseService>();
            services.AddTransient(typeof(IObjectCollectionFactory<>), typeof(MongoDbCollectionFactory<>));
            services.AddTransient(typeof(IHistoryCollectionFactory<>), typeof(MongoDbHistoryCollectionFactory<>));

            services.AddSingleton<ICounterCollectionFactory, MongoDbCountersFactory>();
        }
        else
        {
            services.AddTransient(typeof(IObjectCollectionFactory<>), typeof(InMemoryCollectionFactory<>));
            services.AddTransient(typeof(IHistoryCollectionFactory<>), typeof(InMemoryHistoryCollectionFactory<>));

            services.AddSingleton<ICounterCollectionFactory, InMemoryCountersFactory>();
        }

        Services = services.BuildServiceProvider();

        await OnPostSetup();
    }
}
