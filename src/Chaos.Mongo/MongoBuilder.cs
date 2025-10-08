// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using Chaos.Mongo.Configuration;
using Chaos.Mongo.Queues;
using Chaos.Mongo.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Reflection;

/// <summary>
/// Provides a fluent builder for configuring MongoDB services and configurators.
/// </summary>
public class MongoBuilder
{
    private readonly HashSet<Type> _registeredTypes = [];
    private readonly IServiceCollection _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public MongoBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _services = services;
    }

    /// <summary>
    /// Gets the list of configurator types discovered during auto-discovery.
    /// This property is populated after calling <see cref="WithConfiguratorAutoDiscovery"/>.
    /// </summary>
    public ImmutableArray<Type> DiscoveredConfigurators { get; private set; } = [];

    /// <summary>
    /// Gets the underlying <see cref="IServiceCollection"/> to continue service registration.
    /// </summary>
    public IServiceCollection Services => _services;

    /// <summary>
    /// Registers a specific MongoDB configurator.
    /// </summary>
    /// <typeparam name="T">The type of configurator to register.</typeparam>
    /// <returns>This builder instance for method chaining.</returns>
    public MongoBuilder WithConfigurator<T>()
        where T : class, IMongoConfigurator
    {
        var configuratorType = typeof(T);

        if (!_registeredTypes.Contains(configuratorType))
        {
            _services.AddTransient<IMongoConfigurator, T>();
            _registeredTypes.Add(configuratorType);
        }

        return this;
    }

    /// <summary>
    /// Automatically discovers and registers all implementations of <see cref="IMongoConfigurator"/>
    /// in the specified assemblies.
    /// </summary>
    /// <param name="assembliesToScan">
    /// The assemblies to scan for configurator implementations.
    /// If null or empty, scans the calling assembly.
    /// </param>
    /// <returns>This builder instance for method chaining.</returns>
    public MongoBuilder WithConfiguratorAutoDiscovery(IEnumerable<Assembly>? assembliesToScan = null)
    {
        var assemblies = assembliesToScan?.ToList() ?? [];

        // Default to calling assembly if none specified
        if (assemblies.Count == 0)
            assemblies.Add(Assembly.GetCallingAssembly());

        var implementationTypes = ReflectionHelper
                                  .GetInterfaceImplementations(typeof(IMongoConfigurator), assemblies)
                                  .Where(type => !_registeredTypes.Contains(type))
                                  .ToList();

        foreach (var implementationType in implementationTypes)
        {
            _services.AddTransient(typeof(IMongoConfigurator), implementationType);
            _registeredTypes.Add(implementationType);
        }

        DiscoveredConfigurators = [..DiscoveredConfigurators, ..implementationTypes];
        return this;
    }

    /// <summary>
    /// Registers a MongoDB queue with the given payload type and configuration.
    /// </summary>
    /// <typeparam name="TPayload">The type of payload stored in the queue.</typeparam>
    /// <param name="configure">The configuration action for the queue builder.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a registration for a MongoDB queue with the same payload type already exists.</exception>
    public MongoBuilder WithQueue<TPayload>(Action<MongoQueueBuilder<TPayload>> configure)
        where TPayload : class, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        if (_services.Any(s => s.ServiceType == typeof(IMongoQueue<TPayload>)))
        {
            throw new InvalidOperationException($"A registration for a MongoDB queue with payload {typeof(TPayload).Name} already exists.");
        }

        _services.AddMongoQueue();

        var queueBuilder = new MongoQueueBuilder<TPayload>(_services);
        configure.Invoke(queueBuilder);
        queueBuilder.RegisterQueue();

        return this;
    }
}
