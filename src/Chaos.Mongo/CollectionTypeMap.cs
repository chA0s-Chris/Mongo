// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using Microsoft.Extensions.Options;
using System.Collections.Frozen;

public class CollectionTypeMap : ICollectionTypeMap
{
    private readonly FrozenDictionary<Type, String> _typeMap;
    private readonly Boolean _useDefaultCollectionNames;

    public CollectionTypeMap(IOptions<MongoOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _useDefaultCollectionNames = options.Value.UseDefaultCollectionNames;
        _typeMap = options.Value.CollectionTypeMap.ToFrozenDictionary();
    }

    public String GetCollectionName(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (_typeMap.TryGetValue(type, out var name))
        {
            return name;
        }

        if (_useDefaultCollectionNames)
        {
            return type.Name;
        }

        throw new KeyNotFoundException($"Type {type} is not mapped and default collection names are not used");
    }
}
