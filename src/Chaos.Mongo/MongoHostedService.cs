// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using Chaos.Mongo.Queues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

public class MongoHostedService : IHostedLifecycleService
{
    private readonly ILogger<MongoHostedService> _logger;
    private readonly ImmutableArray<IMongoQueue> _queues;

    public MongoHostedService(IEnumerable<IMongoQueue> queues,
                              ILogger<MongoHostedService> logger)
    {
        ArgumentNullException.ThrowIfNull(queues);
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _queues = [..queues];
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        foreach (var queue in _queues.Where(x => x.QueueDefinition.AutoStartSubscription))
        {
            _logger.LogInformation("Starting subscription for MongoDB queue with payload {Payload}", queue.QueueDefinition.PayloadType.Name);
            await queue.StartSubscriptionAsync(cancellationToken);
        }
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        foreach (var queue in _queues.Where(x => x.QueueDefinition.AutoStartSubscription))
        {
            _logger.LogInformation("Stopping subscription for MongoDB queue with payload {Payload}", queue.QueueDefinition.PayloadType.Name);
            await queue.StopSubscriptionAsync(cancellationToken);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
