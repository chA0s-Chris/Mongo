// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

/// <summary>
/// Representation of a MongoDB connection consisting of the MongoDB client and the current database.
/// </summary>
public interface IMongoConnection
{
    /// <summary>
    /// Reference to the underlying <see cref="IMongoClient"/>.
    /// </summary>
    IMongoClient Client { get; }

    /// <summary>
    /// Reference to the underlying <see cref="IMongoDatabase"/>.
    /// </summary>
    IMongoDatabase Database { get; }
}
