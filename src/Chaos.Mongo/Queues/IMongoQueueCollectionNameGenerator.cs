// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Queues;

/// <summary>
/// Defines a generator for creating collection names for queue storage.
/// </summary>
public interface IMongoQueueCollectionNameGenerator
{
    /// <summary>
    /// Generates a unique collection name for the specified payload type.
    /// </summary>
    /// <param name="payloadType">The payload type to generate a collection name for.</param>
    /// <returns>A unique collection name based on the payload type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payloadType"/> is null.</exception>
    String GenerateQueueCollectionName(Type payloadType);
}
