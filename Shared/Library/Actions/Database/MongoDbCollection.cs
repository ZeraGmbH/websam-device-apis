
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedLibrary.Models;
using SharedLibrary.Services;

namespace SharedLibrary.Actions.Database;

/// <summary>
/// MongoDb collection.
/// </summary>
/// <typeparam name="TItem">Type of the related document.</typeparam>
public sealed class MongoDbCollection<TItem> : IObjectCollection<TItem> where TItem : IDatabaseObject
{
    /// <summary>
    /// Name of the collection to use.
    /// </summary>
    public readonly string CollectionName;

    private readonly IMongoDbDatabaseService _database;

    private readonly ILogger<MongoDbCollection<TItem>> _logger;

    /// <summary>
    /// Initializes a new collection.
    /// </summary>
    /// <param name="collectionName"></param>
    /// <param name="database">Database connection to use.</param>
    /// <param name="logger">Logging instance to use.</param>
    public MongoDbCollection(string collectionName, IMongoDbDatabaseService database, ILogger<MongoDbCollection<TItem>> logger)
    {
        CollectionName = collectionName;

        _database = database;
        _logger = logger;
    }

    private IMongoCollection<T> GetCollection<T>() => _database.Database.GetCollection<T>(CollectionName);

    private IMongoCollection<TItem> GetCollection() => GetCollection<TItem>();

    /// <inheritdoc/>
    public Task<TItem> AddItem(TItem item, string user) => GetCollection()
        .InsertOneAsync(item)
        .ContinueWith((task) => item, TaskContinuationOptions.OnlyOnRanToCompletion);

    /// <inheritdoc/>
    public Task<TItem> UpdateItem(TItem item, string user) => GetCollection()
        .FindOneAndReplaceAsync(Builders<TItem>.Filter.Eq(nameof(IDatabaseObject.Id), item.Id), item, new() { ReturnDocument = ReturnDocument.After })
        .ContinueWith(t => t.Result ?? throw new ArgumentException("item not found", nameof(item)), TaskContinuationOptions.OnlyOnRanToCompletion);

    /// <inheritdoc/>
    public Task<TItem> DeleteItem(string id, string user) => GetCollection()
        .FindOneAndDeleteAsync(Builders<TItem>.Filter.Eq(nameof(IDatabaseObject.Id), id))
        .ContinueWith(t => t.Result ?? throw new ArgumentException("item not found", nameof(id)), TaskContinuationOptions.OnlyOnRanToCompletion);

    /// <summary>
    /// Remove all content.
    /// </summary>
    /// <returns>Conroller of the outstanding operation.</returns>
    public Task<long> RemoveAll() => GetCollection<BsonDocument>()
        .DeleteManyAsync(FilterDefinition<BsonDocument>.Empty)
        .ContinueWith(t => t.Result.DeletedCount, TaskContinuationOptions.OnlyOnRanToCompletion);

    /// <inheritdoc/>
    public IQueryable<TItem> CreateQueryable() => GetCollection().AsQueryable();
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TItem"></typeparam>
public class MongoDbCollectionFactory<TItem> : IObjectCollectionFactory<TItem> where TItem : IDatabaseObject
{
    private readonly ILogger<MongoDbCollection<TItem>> _logger;

    private readonly IMongoDbDatabaseService _database;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="database"></param>
    /// <param name="logger"></param>
    public MongoDbCollectionFactory(IMongoDbDatabaseService database, ILogger<MongoDbCollection<TItem>> logger)
    {
        _database = database;
        _logger = logger;
    }

    /// <inheritdoc/>
    public IObjectCollection<TItem> Create(string uniqueName) => new MongoDbCollection<TItem>(uniqueName, _database, _logger);
}
