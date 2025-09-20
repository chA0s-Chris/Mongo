// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

public interface IMongoConnectionFactory
{
    IMongoConnection CreateConnection(MongoUrl url, String? databaseName = null);
}
