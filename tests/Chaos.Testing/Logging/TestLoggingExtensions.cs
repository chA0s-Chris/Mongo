// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Testing.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

public static class TestLoggingExtensions
{
    public static ILoggingBuilder AddNUnit(this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, NUnitTestLoggerProvider>());
        return builder;
    }

    public static IServiceCollection AddNUnitTestLogging(this IServiceCollection services)
        => services.AddLogging(builder => builder.AddNUnit());
}
