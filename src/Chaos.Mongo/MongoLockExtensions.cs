// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

/// <summary>
/// Extensions for <see cref="IMongoLock"/>.
/// </summary>
public static class MongoLockExtensions
{
    /// <summary>
    /// Ensures that the lock is valid.
    /// </summary>
    /// <param name="mongoLock">The lock to validate.</param>
    /// <returns>The same lock instance if it is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mongoLock"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the lock is not valid.</exception>
    public static IMongoLock EnsureValid(this IMongoLock mongoLock)
    {
        ArgumentNullException.ThrowIfNull(mongoLock);

        if (!mongoLock.IsValid)
        {
            throw new InvalidOperationException($"MongoDB lock {mongoLock.Id}' has expired or been released.");
        }

        return mongoLock;
    }
}
