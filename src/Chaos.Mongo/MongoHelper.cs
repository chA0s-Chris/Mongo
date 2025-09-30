// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

public sealed class MongoHelper : IMongoHelper
{
    private readonly ICollectionTypeMap _collectionTypeMap;

    public MongoHelper(IMongoConnection connection, ICollectionTypeMap collectionTypeMap)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(collectionTypeMap);
        _collectionTypeMap = collectionTypeMap;

        Client = connection.Client;
        Database = connection.Database;
    }

    /// <inheritdoc/>
    public IMongoClient Client { get; }

    /// <inheritdoc/>
    public IMongoDatabase Database { get; }

    /// <inheritdoc/>
    public IMongoCollection<T> GetCollection<T>(MongoCollectionSettings? settings = null)
    {
        var collectionName = _collectionTypeMap.GetCollectionName<T>();
        return Database.GetCollection<T>(collectionName, settings);
    }
}
