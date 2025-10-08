// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

/// <summary>
/// Configuration options for MongoDB connection and behavior.
/// </summary>
public sealed record MongoOptions
{
    /// <summary>
    /// Gets or initializes the dictionary mapping CLR types to MongoDB collection names.
    /// </summary>
    public Dictionary<Type, String> CollectionTypeMap { get; init; } = [];

    /// <summary>
    /// Gets or sets the default database name to use when not specified in the URL.
    /// </summary>
    public String? DefaultDatabase { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this lock holder instance.
    /// </summary>
    /// <remarks>
    /// If not specified, a new GUID will be generated for each instance.
    /// </remarks>
    public String? HolderId { get; set; }

    /// <summary>
    /// Gets or sets the name of the collection used to store distributed locks.
    /// Defaults to <see cref="MongoDefaults.LockCollectionName"/>.
    /// </summary>
    public String LockCollectionName { get; set; } = MongoDefaults.LockCollectionName;

    /// <summary>
    /// Gets or sets a value indicating whether to automatically run registered <see cref="Configuration.IMongoConfigurator"/> instances on application startup.
    /// Defaults to <see cref="MongoDefaults.RunConfiguratorsOnStartup"/>.
    /// </summary>
    /// <remarks>
    /// When set to <c>true</c>, all registered configurators will be executed automatically during application startup
    /// via the <see cref="MongoHostedService"/>. When <c>false</c>, configurators must be run manually using
    /// <see cref="Configuration.IMongoConfiguratorRunner"/>.
    /// </remarks>
    public Boolean RunConfiguratorsOnStartup { get; set; } = MongoDefaults.RunConfiguratorsOnStartup;

    /// <summary>
    /// Gets or sets the MongoDB connection URL.
    /// </summary>
    public MongoUrl? Url { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use the CLR type name as the collection name when no mapping is found.
    /// Defaults to <see cref="MongoDefaults.UseDefaultCollectionNames"/>.
    /// </summary>
    public Boolean UseDefaultCollectionNames { get; set; } = MongoDefaults.UseDefaultCollectionNames;

    /// <summary>
    /// Adds a type-to-collection name mapping.
    /// </summary>
    /// <typeparam name="T">The CLR type to map.</typeparam>
    /// <param name="collectionName">The MongoDB collection name. If null, the type name is used.</param>
    /// <returns>This <see cref="MongoOptions"/> instance for method chaining.</returns>
    public MongoOptions AddMapping<T>(String? collectionName) => AddMapping(typeof(T), collectionName);

    /// <summary>
    /// Adds a type-to-collection name mapping.
    /// </summary>
    /// <param name="type">The CLR type to map.</param>
    /// <param name="collectionName">The MongoDB collection name. If null, the type name is used.</param>
    /// <returns>This <see cref="MongoOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public MongoOptions AddMapping(Type type, String? collectionName)
    {
        ArgumentNullException.ThrowIfNull(type);
        collectionName ??= type.Name;

        CollectionTypeMap.Add(type, collectionName);
        return this;
    }
}
