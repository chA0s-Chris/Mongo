// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

/// <summary>
/// Default implementation of <see cref="IMongoLock"/> representing a distributed lock in MongoDB.
/// </summary>
public class MongoLock : IMongoLock
{
    private readonly Func<Task> _releaseAction;
    private readonly TimeProvider _timeProvider;
    private Boolean _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoLock"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the lock.</param>
    /// <param name="validUntilUtc">The UTC date and time when the lock will automatically expire.</param>
    /// <param name="timeProvider">The time provider for getting current time.</param>
    /// <param name="releaseAction">The action to execute when the lock is released.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="releaseAction"/> is null.</exception>
    public MongoLock(String id, DateTime validUntilUtc, TimeProvider timeProvider, Func<Task> releaseAction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(releaseAction);

        Id = id;
        ValidUntilUtc = validUntilUtc;
        _timeProvider = timeProvider;
        _releaseAction = releaseAction;
    }

    /// <inheritdoc/>
    public String Id { get; }

    /// <inheritdoc/>
    public Boolean IsValid => !_disposed && ValidUntilUtc > _timeProvider.GetUtcNow().UtcDateTime;

    /// <inheritdoc/>
    public DateTime ValidUntilUtc { get; }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            await _releaseAction();
        }
        catch
        {
            // Suppress exceptions during dispose to prevent unhandled exceptions
            // Locks will eventually expire anyway
        }
    }
}
