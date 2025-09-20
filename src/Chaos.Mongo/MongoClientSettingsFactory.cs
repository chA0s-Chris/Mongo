// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

public class MongoClientSettingsFactory : IMongoClientSettingsFactory
{
    public MongoClientSettings CreateMongoClientSettings(MongoUrl url)
    {
        ArgumentNullException.ThrowIfNull(url);
        return MongoClientSettings.FromUrl(url);
    }
}
