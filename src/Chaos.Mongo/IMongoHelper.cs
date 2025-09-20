// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

public interface IMongoHelper : IMongoConnection
{
    /// <summary>
    /// Execute code within a MongoDB transaction with automatic retry on transient errors.
    /// </summary>
    /// <remarks>
    /// If the code completes without error the transaction will be committed.
    /// Use <see cref="IClientSessionHandle"/> to abort the transaction when necessary.
    /// When transient errors are encountered the transaction will be retried. This only works if
    /// <paramref name="callback"/> does not suppress <see cref="MongoException"/>s.
    /// </remarks>
    /// <param name="callback">Code to execute within the transaction.</param>
    /// <param name="transactionOptions">Optional transaction options.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <typeparam name="TResult">Type of the return value.</typeparam>
    /// <returns>Value returned by <paramref name="callback"/>.</returns>
    Task<TResult> ExecuteInTransaction<TResult>(Func<IMongoHelper, IClientSessionHandle, CancellationToken, Task<TResult>> callback,
                                                TransactionOptions? transactionOptions = null,
                                                CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute code within a MongoDB transaction with automatic retry on transient errors.
    /// </summary>
    /// <remarks>
    /// If the code completes without error the transaction will be committed.
    /// Use <see cref="IClientSessionHandle"/> to abort the transaction when necessary.
    /// When transient errors are encountered the transaction will be retried. This only works if
    /// <paramref name="callback"/> does not suppress <see cref="MongoException"/>s.
    /// </remarks>
    /// <param name="callback">Code to execute within the transaction.</param>
    /// <param name="transactionOptions">Optional transaction options.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task.</returns>
    Task ExecuteInTransaction(Func<IMongoHelper, IClientSessionHandle, CancellationToken, Task> callback,
                              TransactionOptions? transactionOptions = null,
                              CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the collection for the specified document type <typeparamref name="TDocument"/>.
    /// </summary>
    /// <typeparam name="TDocument">Type of the document.</typeparam>
    /// <param name="settings">Optional <see cref="MongoCollectionSettings"/>.</param>
    /// <returns>Collection for the type <typeparamref name="TDocument"/>.</returns>
    IMongoCollection<TDocument> GetCollection<TDocument>(MongoCollectionSettings? settings = null);
}
