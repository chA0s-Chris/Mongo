// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using Microsoft.Extensions.Options;
using MongoDB.Driver;

/// <summary>
/// Provides a helper abstraction for working with MongoDB, including collection access and distributed locking.
/// </summary>
public sealed class MongoHelper : IMongoHelper
{
    private readonly ICollectionTypeMap _collectionTypeMap;
    private readonly String _holderId;
    private readonly String _lockCollectionName;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoHelper"/> class.
    /// </summary>
    /// <param name="connection">The MongoDB connection instance.</param>
    /// <param name="collectionTypeMap">The collection type map for resolving collection names.</param>
    /// <param name="timeProvider">The time provider for getting current time.</param>
    /// <param name="options">Optional MongoDB configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connection"/> or <paramref name="collectionTypeMap"/> is null.</exception>
    public MongoHelper(IMongoConnection connection,
                       ICollectionTypeMap collectionTypeMap,
                       TimeProvider timeProvider,
                       IOptions<MongoOptions>? options)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(collectionTypeMap);
        ArgumentNullException.ThrowIfNull(timeProvider);
        _collectionTypeMap = collectionTypeMap;
        _timeProvider = timeProvider;

        Client = connection.Client;
        Database = connection.Database;

        _holderId = options?.Value.HolderId ?? Guid.NewGuid().ToString();
        _lockCollectionName = options?.Value.LockCollectionName ?? MongoDefaults.LockCollectionName;
    }

    /// <inheritdoc/>
    public IMongoClient Client { get; }

    /// <inheritdoc/>
    public IMongoDatabase Database { get; }

    /// <summary>
    /// Releases a MongoDB distributed lock.
    /// </summary>
    /// <param name="lockName">The name of the lock to release.</param>
    /// <param name="holder">The holder ID that currently owns the lock.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
        var now = _timeProvider.GetUtcNow().UtcDateTime;
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
                return new MongoLock(lockName, leaseUntil, _timeProvider, async () => await ReleaseLockAsync(lockName, _holderId));

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
