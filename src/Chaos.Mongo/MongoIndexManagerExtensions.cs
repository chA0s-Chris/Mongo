// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

/// <summary>
/// Extensions for <see cref="IMongoIndexManager{TDocument}"/>.
/// </summary>
public static class MongoIndexManagerExtensions
{
    /// <summary>
    /// Creates an index, or updates it if the index already exists with different options or specifications.
    /// If the index exists with conflicting options or key specifications, it will be dropped and recreated.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <param name="indexManager">The index manager.</param>
    /// <param name="model">The create index model.</param>
    /// <param name="options">The options for creating the index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The name of the index that was created or updated.</returns>
    /// <exception cref="MongoCommandException">Thrown when an error occurs that is not related to index conflicts.</exception>
    public static async Task<String?> CreateOneOrUpdateAsync<TDocument>(this IMongoIndexManager<TDocument> indexManager,
                                                                        CreateIndexModel<TDocument> model,
                                                                        CreateOneIndexOptions? options = null,
                                                                        CancellationToken cancellationToken = default)
    {
        try
        {
            return await indexManager.CreateOneAsync(model, options, cancellationToken);
        }
        catch (MongoCommandException e) when (e.CodeName is "IndexOptionsConflict" or "IndexKeySpecsConflict")
        {
            var name = e.Command["indexes"][0]["name"].AsString;
            if (String.IsNullOrEmpty(name))
            {
                throw;
            }

            await indexManager.DropOneAsync(name, cancellationToken);
            return await indexManager.CreateOneAsync(model, options, cancellationToken);
        }
    }

    /// <summary>
    /// Creates an index within a session, or updates it if the index already exists with different options or specifications.
    /// If the index exists with conflicting options or key specifications, it will be dropped and recreated.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <param name="indexManager">The index manager.</param>
    /// <param name="session">The client session.</param>
    /// <param name="model">The create index model.</param>
    /// <param name="options">The options for creating the index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The name of the index that was created or updated.</returns>
    /// <exception cref="MongoCommandException">Thrown when an error occurs that is not related to index conflicts.</exception>
    public static async Task<String?> CreateOneOrUpdateAsync<TDocument>(this IMongoIndexManager<TDocument> indexManager,
                                                                        IClientSessionHandle session,
                                                                        CreateIndexModel<TDocument> model,
                                                                        CreateOneIndexOptions? options = null,
                                                                        CancellationToken cancellationToken = default)
    {
        try
        {
            return await indexManager.CreateOneAsync(session, model, options, cancellationToken);
        }
        catch (MongoCommandException e) when (e.CodeName is "IndexOptionsConflict" or "IndexKeySpecsConflict")
        {
            var name = e.Command["indexes"][0]["name"].AsString;
            if (String.IsNullOrEmpty(name))
            {
                throw;
            }

            await indexManager.DropOneAsync(session, name, cancellationToken);
            return await indexManager.CreateOneAsync(session, model, options, cancellationToken);
        }
    }
}
