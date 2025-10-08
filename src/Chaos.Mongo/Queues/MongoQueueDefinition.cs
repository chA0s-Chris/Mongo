﻿// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Queues;

public record MongoQueueDefinition
{
    public required Boolean AutoStartSubscription { get; init; }

    /// <summary>
    /// Name of collection.
    /// </summary>
    public required String CollectionName { get; init; }

    /// <summary>
    /// Type of the payload handler.
    /// </summary>
    public required Type PayloadHandlerType { get; init; }

    /// <summary>
    /// Type of the payload.
    /// </summary>
    public required Type PayloadType { get; init; }

    /// <summary>
    /// Number of queue payloads that will be processed sequentially before
    /// the remaining payloads will be sorted again.
    /// </summary>
    public required Int32 QueryLimit { get; init; }
}
