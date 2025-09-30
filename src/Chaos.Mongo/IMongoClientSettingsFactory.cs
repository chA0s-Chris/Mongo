// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

/// <summary>
/// Factory for creating <see cref="MongoClientSettings"/> instances from MongoDB URLs.
/// </summary>
public interface IMongoClientSettingsFactory
{
    /// <summary>
    /// Creates MongoDB client settings from the specified URL.
    /// </summary>
    /// <param name="url">The MongoDB connection URL.</param>
    /// <returns>A configured <see cref="MongoClientSettings"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> is null.</exception>
    MongoClientSettings CreateMongoClientSettings(MongoUrl url);
}
