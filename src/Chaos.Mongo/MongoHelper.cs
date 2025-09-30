// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using Microsoft.Extensions.Options;
using MongoDB.Driver;

public sealed class MongoHelper : IMongoHelper
{
    private readonly ICollectionTypeMap _collectionTypeMap;
    private readonly String _holderId;
    private readonly String _lockCollectionName;

    public MongoHelper(IMongoConnection connection,
                       ICollectionTypeMap collectionTypeMap,
                       IOptions<MongoOptions>? options)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(collectionTypeMap);
        _collectionTypeMap = collectionTypeMap;

        Client = connection.Client;
        Database = connection.Database;

        _holderId = options?.Value.HolderId ?? Guid.NewGuid().ToString();
        _lockCollectionName = options?.Value.LockCollectionName ?? MongoDefaults.LockCollectionName;
    }

    /// <inheritdoc/>
    public IMongoClient Client { get; }

    /// <inheritdoc/>
    public IMongoDatabase Database { get; }

    internal async Task ReleaseLockAsync(String lockName, String holder)
    {
        var lockCollection = Database.GetCollection<MongoLockDocument>(_lockCollectionName);

        // Only delete the lock if we are still the holder
        var filter = Builders<MongoLockDocument>.Filter.Eq(x => x.Id, lockName) &
                     Builders<MongoLockDocument>.Filter.Eq(x => x.Holder, holder);

        await lockCollection.DeleteOneAsync(filter);
    }

    /// <inheritdoc/>
    public IMongoCollection<T> GetCollection<T>(MongoCollectionSettings? settings = null)
    {
        var collectionName = _collectionTypeMap.GetCollectionName<T>();
        return Database.GetCollection<T>(collectionName, settings);
    }

    /// <inheritdoc/>
    public async Task<IMongoLock?> TryAcquireLockAsync(String lockName, TimeSpan? leaseTime = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lockName);
        leaseTime ??= MongoDefaults.LockLeaseTime;

        var lockCollection = Database.GetCollection<MongoLockDocument>(_lockCollectionName);
        var now = DateTime.UtcNow;
        var leaseUntil = now.Add(leaseTime.Value);

        // Try to atomically take or extend the lock if expired
        var filter = Builders<MongoLockDocument>.Filter.Eq(x => x.Id, lockName) &
                     Builders<MongoLockDocument>.Filter.Lte(x => x.LeaseUntilUtc, now);

        var update = Builders<MongoLockDocument>.Update
                                                .SetOnInsert(x => x.Id, lockName)
                                                .Set(x => x.Holder, _holderId)
                                                .Set(x => x.LeaseUntilUtc, leaseUntil);

        var options = new FindOneAndUpdateOptions<MongoLockDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        try
        {
            var lockDocument = await lockCollection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);

            // Verify we successfully acquired the lock
            if (lockDocument?.Holder == _holderId)
                return new MongoLock(lockName, leaseUntil, async () => await ReleaseLockAsync(lockName, _holderId));

            return null;
        }
        catch (MongoException ex)
            when (ex is MongoCommandException { Code: 11000 } or
                        MongoWriteException { WriteError.Category: ServerErrorCategory.DuplicateKey })
        {
            // Duplicate key error - another process created the lock during our upsert attempt
            return null;
        }
    }
}
