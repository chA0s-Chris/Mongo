// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Testing;

using Chaos.Testing.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

public static class TestLogging
{
    private static readonly ConcurrentDictionary<Type, ILogger> _loggers = new();

    public static ILogger GetTestLogger()
        => GetTestLogger<NUnitTestLogger>();

    public static ILogger<T> GetTestLogger<T>()
        => (ILogger<T>)_loggers.GetOrAdd(typeof(T), _ =>
        {
            using var serviceProvider = new ServiceCollection()
                                        .AddNUnitTestLogging()
                                        .BuildServiceProvider();

            return serviceProvider.GetRequiredService<ILoggerFactory>()
                                  .CreateLogger<T>();
        });
}
