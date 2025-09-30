// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

public interface IMongoHelper : IMongoConnection
{
    /// <summary>
    /// Get the collection for the specified document type <typeparamref name="TDocument"/>.
    /// </summary>
    /// <typeparam name="TDocument">Type of the document.</typeparam>
    /// <param name="settings">Optional <see cref="MongoCollectionSettings"/>.</param>
    /// <returns>Collection for the type <typeparamref name="TDocument"/>.</returns>
    IMongoCollection<TDocument> GetCollection<TDocument>(MongoCollectionSettings? settings = null);
}
