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
    public async Task<T> ExecuteInTransaction<T>(Func<IMongoHelper, IClientSessionHandle, CancellationToken, Task<T>> callback,
                                                 TransactionOptions? transactionOptions = null,
                                                 CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        using var session = await Client.StartSessionAsync(cancellationToken: cancellationToken);
        return await session.WithTransactionAsync(async (sessionHandle, token) => await callback.Invoke(this, sessionHandle, token),
                                                  transactionOptions,
                                                  cancellationToken);
    }

    /// <inheritdoc/>
    public async Task ExecuteInTransaction(Func<IMongoHelper, IClientSessionHandle, CancellationToken, Task> callback,
                                           TransactionOptions? transactionOptions = null,
                                           CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        using var session = await Client.StartSessionAsync(cancellationToken: cancellationToken);
        await session.WithTransactionAsync<Object?>(async (sessionHandle, token) =>
                                                    {
                                                        await callback.Invoke(this, sessionHandle, token);
                                                        return null;
                                                    },
                                                    transactionOptions,
                                                    cancellationToken);
    }

    /// <inheritdoc/>
    public IMongoCollection<T> GetCollection<T>(MongoCollectionSettings? settings = null)
    {
        var collectionName = _collectionTypeMap.GetCollectionName<T>();
        return Database.GetCollection<T>(collectionName, settings);
    }
}
