// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

/// <summary>
/// Extensions for <see cref="IMongoHelper"/>.
/// </summary>
public static class MongoHelperExtensions
{
    /// <summary>
    /// Acquire a distributed lock in MongoDB with automatic retry on contention.
    /// </summary>
    /// <remarks>
    /// This method will retry indefinitely until the lock is acquired or the <paramref name="cancellationToken"/> is cancelled.
    /// The lock is automatically released when the returned <see cref="IMongoLock"/> is disposed.
    /// If the lock is not explicitly released, it will automatically expire after <paramref name="leaseTime"/>.
    /// </remarks>
    /// <param name="helper">The MongoDB helper instance.</param>
    /// <param name="lockName">The name of the lock to acquire.</param>
    /// <param name="leaseTime">Optional lease duration. Defaults to <see cref="MongoDefaults.LockLeaseTime"/>.</param>
    /// <param name="retryDelay">Optional delay between retry attempts. Defaults to <see cref="MongoDefaults.RetryDelay"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token to stop lock acquisition attempts.</param>
    /// <returns>A lock instance that must be disposed to release the lock.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="lockName"/> is null or whitespace.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is cancelled.</exception>
    public static async Task<IMongoLock> AcquireLockAsync(this IMongoHelper helper,
                                                          String lockName,
                                                          TimeSpan? leaseTime = null,
                                                          TimeSpan? retryDelay = null,
                                                          CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(helper);
        ArgumentException.ThrowIfNullOrWhiteSpace(lockName);

        retryDelay ??= MongoDefaults.RetryDelay;

        // Retry until lock is acquired or cancellation requested
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var lockInstance = await helper.TryAcquireLockAsync(lockName, leaseTime, cancellationToken);
            if (lockInstance is not null)
                return lockInstance;

            // Lock not available, wait and retry
            await Task.Delay(retryDelay.Value, cancellationToken);
        }
    }

    /// <summary>
    /// Execute code within a MongoDB transaction with automatic retry on transient errors.
    /// </summary>
    /// <remarks>
    /// If the code completes without error the transaction will be committed.
    /// Use <see cref="IClientSessionHandle"/> to abort the transaction when necessary.
    /// When transient errors are encountered the transaction will be retried. This only works if
    /// <paramref name="callback"/> does not suppress <see cref="MongoException"/>s.
    /// </remarks>
    /// <param name="helper">The MongoDB helper instance.</param>
    /// <param name="callback">Code to execute within the transaction.</param>
    /// <param name="transactionOptions">Optional transaction options.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <typeparam name="TResult">Type of the return value.</typeparam>
    /// <returns>Value returned by <paramref name="callback"/>.</returns>
    public static async Task<TResult> ExecuteInTransaction<TResult>(this IMongoHelper helper,
                                                                    Func<IMongoHelper, IClientSessionHandle, CancellationToken, Task<TResult>> callback,
                                                                    TransactionOptions? transactionOptions = null,
                                                                    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(helper);
        ArgumentNullException.ThrowIfNull(callback);

        using var session = await helper.Client.StartSessionAsync(cancellationToken: cancellationToken);
        return await session.WithTransactionAsync(
            async (sessionHandle, token) => await callback.Invoke(helper, sessionHandle, token),
            transactionOptions,
            cancellationToken);
    }

    /// <summary>
    /// Execute code within a MongoDB transaction with automatic retry on transient errors.
    /// </summary>
    /// <remarks>
    /// If the code completes without error the transaction will be committed.
    /// Use <see cref="IClientSessionHandle"/> to abort the transaction when necessary.
    /// When transient errors are encountered the transaction will be retried. This only works if
    /// <paramref name="callback"/> does not suppress <see cref="MongoException"/>s.
    /// </remarks>
    /// <param name="helper">The MongoDB helper instance.</param>
    /// <param name="callback">Code to execute within the transaction.</param>
    /// <param name="transactionOptions">Optional transaction options.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task.</returns>
    public static async Task ExecuteInTransaction(this IMongoHelper helper,
                                                  Func<IMongoHelper, IClientSessionHandle, CancellationToken, Task> callback,
                                                  TransactionOptions? transactionOptions = null,
                                                  CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(helper);
        ArgumentNullException.ThrowIfNull(callback);

        using var session = await helper.Client.StartSessionAsync(cancellationToken: cancellationToken);
        await session.WithTransactionAsync<Object?>(
            async (sessionHandle, token) =>
            {
                await callback.Invoke(helper, sessionHandle, token);
                return null;
            },
            transactionOptions,
            cancellationToken);
    }
}
