// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Testing.Logging;

using Microsoft.Extensions.Logging;

public sealed class NUnitTestLogger : ILogger
{
    private readonly String _categoryName;

    public NUnitTestLogger(String categoryName)
    {
        _categoryName = String.IsNullOrEmpty(categoryName) ? nameof(NUnitTestLogger) : categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
        => DummyDisposable.Instance;

    public Boolean IsEnabled(LogLevel logLevel)
        => NUnit.Framework.TestContext.Progress is not null &&
           logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, String> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter.Invoke(state, exception);
        if (String.IsNullOrEmpty(message))
        {
            return;
        }

        var output = $"{logLevel}: {message}";
        if (exception is not null)
        {
            output += $"{Environment.NewLine}{exception}";
        }

        NUnit.Framework.TestContext.Progress.WriteLine(output);
    }

    private class DummyDisposable : IDisposable
    {
        public static DummyDisposable Instance => new();

        public void Dispose() { }
    }
}
