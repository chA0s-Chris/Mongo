// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using Chaos.Mongo.Configuration;
using Chaos.Mongo.Queues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;

/// <summary>
/// Hosted service used for automatic lifecycle management.
/// </summary>
public class MongoHostedService : IHostedLifecycleService
{
    private readonly IMongoConfiguratorRunner _configuratorRunner;
    private readonly ILogger<MongoHostedService> _logger;
    private readonly MongoOptions _options;
    private readonly ImmutableArray<IMongoQueue> _queues;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoHostedService"/> class.
    /// </summary>
    /// <param name="queues">The collection of queues to manage.</param>
    /// <param name="configuratorRunner">The configurator runner for executing MongoDB configuration.</param>
    /// <param name="options">The MongoDB options.</param>
    /// <param name="logger">The logger for diagnostic messages.</param>
    public MongoHostedService(IEnumerable<IMongoQueue> queues,
                              IMongoConfiguratorRunner configuratorRunner,
                              IOptions<MongoOptions> options,
                              ILogger<MongoHostedService> logger)
    {
        ArgumentNullException.ThrowIfNull(queues);
        ArgumentNullException.ThrowIfNull(configuratorRunner);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _queues = [..queues];
        _configuratorRunner = configuratorRunner;
        _options = options.Value;
    }

    /// <inheritdoc/>
    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        foreach (var queue in _queues.Where(x => x.QueueDefinition.AutoStartSubscription))
        {
            _logger.LogInformation("Starting subscription for MongoDB queue with payload {Payload}", queue.QueueDefinition.PayloadType.Name);
            await queue.StartSubscriptionAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        if (_options.RunConfiguratorsOnStartup)
        {
            _logger.LogInformation("Running MongoDB configurators on application startup");
            await _configuratorRunner.RunConfiguratorsAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        foreach (var queue in _queues.Where(x => x.QueueDefinition.AutoStartSubscription))
        {
            _logger.LogInformation("Stopping subscription for MongoDB queue with payload {Payload}", queue.QueueDefinition.PayloadType.Name);
            await queue.StopSubscriptionAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
