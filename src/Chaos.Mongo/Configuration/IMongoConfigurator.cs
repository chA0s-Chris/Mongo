// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Configuration;

/// <summary>
/// Provides an interface for MongoDB initialization logic (e.g., creating collections, ensuring indexes).
/// </summary>
/// <remarks>
/// Implementations of this interface are used by the <see cref="MongoConfiguratorRunner"/> to perform
/// MongoDB initialization during application startup.
/// </remarks>
public interface IMongoConfigurator
{
    /// <summary>
    /// Executes MongoDB initialization logic (e.g., creating collections, ensuring indexes).
    /// Runs during application startup via a hosted service.
    /// </summary>
    /// <param name="helper">The Mongo helper abstraction provided by this library.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the configuration is finished.</returns>
    Task ConfigureAsync(IMongoHelper helper, CancellationToken cancellationToken = default);
}
