// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Configuration;

/// <summary>
/// Runs all registered <see cref="IMongoConfigurator"/> instances to perform MongoDB initialization
/// (e.g., creating collections, ensuring indexes) during application startup.
/// </summary>
/// <remarks>
/// Execution order follows the DI registration order of <see cref="IMongoConfigurator"/> implementations.
/// If none are registered, running is a no-op and completes successfully. The current implementation stops
/// on the first exception thrown by a configurator. The provided <see cref="CancellationToken"/> is honored
/// and may cancel execution before or between configurators.
/// </remarks>
public interface IMongoConfiguratorRunner
{
    /// <summary>
    /// Executes all registered <see cref="IMongoConfigurator"/> instances in order.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task that completes when execution finishes or is canceled.</returns>
    Task RunConfiguratorsAsync(CancellationToken cancellationToken = default);
}
