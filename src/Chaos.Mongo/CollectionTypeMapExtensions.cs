// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

/// <summary>
/// Extension methods for <see cref="ICollectionTypeMap"/>.
/// </summary>
public static class CollectionTypeMapExtensions
{
    /// <summary>
    /// Gets the collection name for the specified type parameter.
    /// </summary>
    /// <typeparam name="T">The CLR type to get the collection name for.</typeparam>
    /// <param name="map">The collection type map instance.</param>
    /// <returns>The MongoDB collection name mapped to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the type is not mapped and default collection names are not enabled.</exception>
    public static String GetCollectionName<T>(this ICollectionTypeMap map)
        => map.GetCollectionName(typeof(T));
}
