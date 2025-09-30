// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

/// <summary>
/// Provides a mapping between CLR types and MongoDB collection names.
/// </summary>
public interface ICollectionTypeMap
{
    /// <summary>
    /// Gets the collection name for the specified type.
    /// </summary>
    /// <remarks>
    /// If no collection name is mapped for the specified type and default collection names are enabled,
    /// the type name is used as the collection name.
    /// </remarks>
    /// <param name="type">The CLR type to get the collection name for.</param>
    /// <returns>The MongoDB collection name mapped to the specified type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the type is not mapped and default collection names are not enabled.
    /// </exception>
    String GetCollectionName(Type type);
}
