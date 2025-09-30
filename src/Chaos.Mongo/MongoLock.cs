// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

public class MongoLock : IMongoLock
{
    private readonly Func<Task> _releaseAction;
    private Boolean _disposed;

    public MongoLock(String id, DateTime validUntil, Func<Task> releaseAction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(releaseAction);

        Id = id;
        ValidUntil = validUntil;
        _releaseAction = releaseAction;
    }

    public String Id { get; }

    public DateTime ValidUntil { get; }

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
