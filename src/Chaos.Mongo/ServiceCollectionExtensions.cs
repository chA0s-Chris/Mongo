// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using Chaos.Mongo.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services, Action<MongoOptions>? configure = null)
    {
        var builder = services.AddOptions<MongoOptions>();

        if (configure is not null)
        {
            services.Configure(configure);
        }

        return services.AddMongoInternal(builder);
    }

    public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration, String sectionName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        var builder = services.AddOptions<MongoOptions>();
        builder.Bind(configuration.GetSection(sectionName));

        return services.AddMongoInternal(builder);
    }


    public static IServiceCollection AddMongo(this IServiceCollection services, String connectionString, String? databaseName = null, Action<MongoOptions>? configure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        return services.AddMongo(options =>
        {
            options.Url = new(connectionString);
            options.DefaultDatabase = databaseName;
            configure?.Invoke(options);
        });
    }

    public static IServiceCollection AddMongo(this IServiceCollection services, MongoUrl url, String? databaseName = null, Action<MongoOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(url);

        return services.AddMongo(options =>
        {
            options.Url = url;
            options.DefaultDatabase = databaseName;
            configure?.Invoke(options);
        });
    }

    /// <summary>
    /// Registers a Mongo configurator that will be executed during application startup.
    /// </summary>
    public static IServiceCollection AddMongoConfigurator<T>(this IServiceCollection services)
        where T : class, IMongoConfigurator
    {
        services.AddTransient<IMongoConfigurator, T>();
        return services;
    }

    private static IServiceCollection AddMongoInternal(this IServiceCollection services, OptionsBuilder<MongoOptions> builder)
    {
        services.AddSingleton<IValidateOptions<MongoOptions>, MongoOptionsValidation>();
        builder.ValidateOnStart();

        services.TryAddSingleton<IMongoClientSettingsFactory, MongoClientSettingsFactory>();
        services.TryAddSingleton<IMongoConnectionFactory, MongoConnectionFactory>();
        services.AddSingleton<IMongoConnection>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<MongoOptions>>().Value;
            var url = options.Url ?? throw new InvalidOperationException("MongoOptions.Url is not configured");

            var factory = serviceProvider.GetRequiredService<IMongoConnectionFactory>();
            return factory.CreateConnection(url, options.DefaultDatabase);
        });

        services.AddSingleton<ICollectionTypeMap, CollectionTypeMap>();
        services.AddSingleton<IMongoHelper, MongoHelper>();

        services.AddTransient<IMongoConfiguratorRunner, MongoConfiguratorRunner>();

        return services;
    }
}
