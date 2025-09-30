// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

/// <summary>
/// Default implementation of <see cref="IMongoClientSettingsFactory"/> that creates MongoDB client settings from URLs.
/// </summary>
public class MongoClientSettingsFactory : IMongoClientSettingsFactory
{
    /// <inheritdoc/>
    public MongoClientSettings CreateMongoClientSettings(MongoUrl url)
    {
        ArgumentNullException.ThrowIfNull(url);
        return MongoClientSettings.FromUrl(url);
    }
}
