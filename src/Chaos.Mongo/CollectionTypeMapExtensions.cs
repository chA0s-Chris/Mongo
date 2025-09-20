// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

public static class CollectionTypeMapExtensions
{
    public static String GetCollectionName<T>(this ICollectionTypeMap map)
        => map.GetCollectionName(typeof(T));
}
