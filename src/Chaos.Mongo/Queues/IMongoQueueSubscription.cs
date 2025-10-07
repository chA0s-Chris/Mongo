// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Queues;

/// <summary>
/// Represents a queue subscription that monitors and processes queue items.
/// </summary>
/// <typeparam name="TPayload">The type of payload stored in queue items.</typeparam>
public interface IMongoQueueSubscription<TPayload> : IAsyncDisposable
    where TPayload : class, new()
{
    /// <summary>
    /// Gets a value indicating whether the subscription is actively running and not disposed.
    /// </summary>
    Boolean IsActive { get; }

    /// <summary>
    /// Starts the queue subscription by initializing indexes and launching background tasks for monitoring and processing.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the startup operation.</param>
    /// <returns>A task that completes when the subscription has started.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the subscription is already disposed.</exception>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the queue subscription by cancelling background tasks and waiting for them to complete.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token representing the caller's shutdown timeout.</param>
    /// <returns>A task that completes when the subscription has stopped or the timeout expires.</returns>
    Task StopAsync(CancellationToken cancellationToken = default);
}
