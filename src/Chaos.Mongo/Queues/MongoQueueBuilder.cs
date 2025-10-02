// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Queues;

using Microsoft.Extensions.DependencyInjection;

public sealed class MongoQueueBuilder<TPayload>
    where TPayload : class, new()
{
    private readonly IServiceCollection _services;

    public MongoQueueBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _services = services;
    }
}
