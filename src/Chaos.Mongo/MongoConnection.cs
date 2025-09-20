// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

public class MongoConnection : IMongoConnection
{
    public MongoConnection(IMongoClient client, IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(database);
        Client = client;
        Database = database;
    }

    public IMongoClient Client { get; }

    public IMongoDatabase Database { get; }
}
