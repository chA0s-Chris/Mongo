// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

/// <summary>
/// Represents a distributed lock in MongoDB.
/// </summary>
/// <remarks>
/// Dispose the lock to release it. If not disposed, the lock will automatically expire after the lease time.
/// </remarks>
public interface IMongoLock : IAsyncDisposable
{
    /// <summary>
    /// Gets the unique identifier of the lock.
    /// </summary>
    String Id { get; }

    /// <summary>
    /// Gets the UTC date and time when the lock will automatically expire.
    /// </summary>
    DateTime ValidUntil { get; }
}
