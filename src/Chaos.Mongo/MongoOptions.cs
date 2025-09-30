// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Driver;

public sealed record MongoOptions
{
    public Dictionary<Type, String> CollectionTypeMap { get; init; } = [];

    public String? DefaultDatabase { get; set; }

    public String? HolderId { get; set; }

    public String LockCollectionName { get; set; } = MongoDefaults.LockCollectionName;

    public MongoUrl? Url { get; set; }

    public Boolean UseDefaultCollectionNames { get; set; } = MongoDefaults.UseDefaultCollectionNames;

    public MongoOptions AddMapping<T>(String? collectionName) => AddMapping(typeof(T), collectionName);

    public MongoOptions AddMapping(Type type, String? collectionName)
    {
        ArgumentNullException.ThrowIfNull(type);
        collectionName ??= type.Name;

        CollectionTypeMap.Add(type, collectionName);
        return this;
    }
}
