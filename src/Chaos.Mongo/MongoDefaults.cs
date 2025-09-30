// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

/// <summary>
/// Provides default configuration values for MongoDB operations.
/// </summary>
public static class MongoDefaults
{
    /// <summary>
    /// The default name of the collection used to store distributed locks.
    /// </summary>
    public const String LockCollectionName = "_locks";

    /// <summary>
    /// The default value indicating whether to use CLR type names as collection names when no mapping is found.
    /// </summary>
    public const Boolean UseDefaultCollectionNames = true;

    /// <summary>
    /// Gets the default lease time for distributed locks.
    /// </summary>
    public static TimeSpan LockLeaseTime => TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets the default delay between lock acquisition retry attempts.
    /// </summary>
    public static TimeSpan RetryDelay => TimeSpan.FromMilliseconds(500);
}
