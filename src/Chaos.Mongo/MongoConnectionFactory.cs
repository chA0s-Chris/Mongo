// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

public class MongoConnectionFactory : IMongoConnectionFactory
{
    private readonly IMongoClientSettingsFactory _clientSettingsFactory;

    public MongoConnectionFactory(IMongoClientSettingsFactory clientSettingsFactory)
    {
        ArgumentNullException.ThrowIfNull(clientSettingsFactory);
        _clientSettingsFactory = clientSettingsFactory;
    }

    public IMongoConnection CreateConnection(MongoUrl url, String? databaseName = null)
    {
        ArgumentNullException.ThrowIfNull(url);

        databaseName ??= url.DatabaseName;
        if (String.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentException("URL doesn't contain a database name and the databaseName parameter is also not set");

        var clientSettings = _clientSettingsFactory.CreateMongoClientSettings(url);
        var mongoClient = new MongoClient(clientSettings);
        var mongoDatabase = mongoClient.GetDatabase(databaseName);

        return new MongoConnection(mongoClient, mongoDatabase);
    }
}
