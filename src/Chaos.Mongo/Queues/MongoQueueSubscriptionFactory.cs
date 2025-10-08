// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Queues;

using Microsoft.Extensions.Logging;

/// <summary>
/// Provides a factory implementation for creating and starting queue subscriptions.
/// </summary>
public class MongoQueueSubscriptionFactory : IMongoQueueSubscriptionFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMongoHelper _mongoHelper;
    private readonly IMongoQueuePayloadHandlerFactory _payloadHandlerFactory;
    private readonly IMongoQueuePayloadPrioritizer _payloadPrioritizer;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoQueueSubscriptionFactory"/> class.
    /// </summary>
    /// <param name="mongoHelper">The MongoDB helper for database operations.</param>
    /// <param name="payloadHandlerFactory">The factory for creating payload handlers.</param>
    /// <param name="payloadPrioritizer">The prioritizer for sorting queue items.</param>
    /// <param name="timeProvider">The time provider for timestamp operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public MongoQueueSubscriptionFactory(IMongoHelper mongoHelper,
                                         IMongoQueuePayloadHandlerFactory payloadHandlerFactory,
                                         IMongoQueuePayloadPrioritizer payloadPrioritizer,
                                         TimeProvider timeProvider,
                                         ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(mongoHelper);
        ArgumentNullException.ThrowIfNull(payloadHandlerFactory);
        ArgumentNullException.ThrowIfNull(payloadPrioritizer);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _mongoHelper = mongoHelper;
        _payloadHandlerFactory = payloadHandlerFactory;
        _payloadPrioritizer = payloadPrioritizer;
        _timeProvider = timeProvider;
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc/>
    public async Task<IMongoQueueSubscription<TPayload>> CreateAndRunAsync<TPayload>(MongoQueueDefinition queueDefinition)
        where TPayload : class, new()
    {
        ArgumentNullException.ThrowIfNull(queueDefinition);

        var logger = _loggerFactory.CreateLogger<MongoQueueSubscription<TPayload>>();
        var subscription = new MongoQueueSubscription<TPayload>(queueDefinition,
                                                                _mongoHelper,
                                                                _payloadHandlerFactory,
                                                                _payloadPrioritizer,
                                                                _timeProvider,
                                                                logger);
        await subscription.StartAsync();

        return subscription;
    }
}
