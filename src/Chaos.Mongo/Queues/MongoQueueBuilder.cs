// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Queues;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides a fluent builder for configuring and registering MongoDB queues.
/// </summary>
/// <typeparam name="TPayload">The type of payload stored in the queue.</typeparam>
public sealed class MongoQueueBuilder<TPayload>
    where TPayload : class, new()
{
    private readonly Type _payloadType = typeof(TPayload);
    private readonly IServiceCollection _services;
    private Boolean? _autoStartSubscription;
    private String? _collectionName;
    private Boolean _isRegistered;
    private Func<IServiceProvider, IMongoQueuePayloadHandler<TPayload>>? _payloadHandlerFactory;
    private Type? _payloadHandlerType;
    private Int32? _queryLimit;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoQueueBuilder{TPayload}"/> class.
    /// </summary>
    /// <param name="services">The service collection to register the queue with.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public MongoQueueBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _services = services;
    }

    public void RegisterQueue()
    {
        if (_isRegistered)
        {
            return;
        }

        Validate();

        var handlerFactory = _payloadHandlerFactory ??
                             (serviceProvider => serviceProvider.GetRequiredService<IMongoQueuePayloadHandler<TPayload>>());

        _services.AddTransient(handlerFactory);

        var queueDefinition = new MongoQueueDefinition
        {
            CollectionName = _collectionName ?? String.Empty,
            PayloadType = _payloadType,
            QueryLimit = _queryLimit ?? MongoDefaults.QueryLimit,
            PayloadHandlerType = typeof(IMongoQueuePayloadHandler<TPayload>),
            AutoStartSubscription = _autoStartSubscription ?? MongoDefaults.AutoStartSubscription
        };

        _services.AddSingleton<IMongoQueue<TPayload>>(serviceProvider =>
        {
            var finalQueueDefinition = queueDefinition;
            if (String.IsNullOrEmpty(queueDefinition.CollectionName))
            {
                var collectionNameGenerator = serviceProvider.GetRequiredService<IMongoQueueCollectionNameGenerator>();
                var collectionName = collectionNameGenerator.GenerateQueueCollectionName(queueDefinition.PayloadType);
                finalQueueDefinition = queueDefinition with
                {
                    CollectionName = collectionName
                };
            }

            var subscriptionFactory = serviceProvider.GetRequiredService<IMongoQueueSubscriptionFactory>();
            var publisher = serviceProvider.GetRequiredService<IMongoQueuePublisher>();
            return new MongoQueue<TPayload>(finalQueueDefinition, subscriptionFactory, publisher);
        });

        _services.AddSingleton<IMongoQueue>(serviceProvider => serviceProvider.GetRequiredService<IMongoQueue<TPayload>>());
        _services.AddHostedService<MongoHostedService>(); // only registered once even if called for multiple queues

        _isRegistered = true;
    }

    public MongoQueueBuilder<TPayload> WithAutoStartSubscription()
    {
        _autoStartSubscription = true;
        return this;
    }

    public MongoQueueBuilder<TPayload> WithCollectionName(String collectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);
        _collectionName = collectionName;
        return this;
    }

    public MongoQueueBuilder<TPayload> WithoutAutoStartSubscription()
    {
        _autoStartSubscription = false;
        return this;
    }

    public MongoQueueBuilder<TPayload> WithPayloadHandler<TPayloadHandler>()
        where TPayloadHandler : class, IMongoQueuePayloadHandler<TPayload>
    {
        _payloadHandlerType = typeof(TPayloadHandler);
        return this;
    }

    public MongoQueueBuilder<TPayload> WithPayloadHandler(Type payloadHandlerType)
    {
        ArgumentNullException.ThrowIfNull(payloadHandlerType);

        if (!payloadHandlerType.IsClass ||
            payloadHandlerType.IsAbstract ||
            !typeof(IMongoQueuePayloadHandler<TPayload>).IsAssignableFrom(payloadHandlerType))
        {
            throw new InvalidOperationException($"Payload handler type must be a non-abstract class implementing {nameof(IMongoQueuePayloadHandler<TPayload>)}.");
        }

        _payloadHandlerType = payloadHandlerType;
        return this;
    }

    public MongoQueueBuilder<TPayload> WithPayloadHandler(Func<IServiceProvider, IMongoQueuePayloadHandler<TPayload>> payloadHandlerFactory)
    {
        ArgumentNullException.ThrowIfNull(payloadHandlerFactory);
        _payloadHandlerFactory = payloadHandlerFactory;
        return this;
    }

    public MongoQueueBuilder<TPayload> WithQueueLimit(Int32 queueLimit)
    {
        if (queueLimit <= 0)
        {
            throw new ArgumentException("Queue limit must be greater than 0.", nameof(queueLimit));
        }

        _queryLimit = queueLimit;
        return this;
    }

    internal void Validate()
    {
        if (_services.Any(s => s.ServiceType == typeof(IMongoQueue<TPayload>)))
        {
            throw new InvalidOperationException($"A registration for a MongoDB queue with payload {typeof(TPayload).Name} already exists.");
        }

        if (_payloadHandlerType is null && _payloadHandlerFactory is null)
        {
            throw new InvalidOperationException("Payload handler type or factory must be specified.");
        }

        if (_payloadHandlerType is not null && _payloadHandlerFactory is not null)
        {
            throw new InvalidOperationException("Payload handler type and factory cannot be specified together.");
        }
    }
}
