// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Testing.Logging;

using Microsoft.Extensions.Logging;

public record LogMessage(
    DateTime Timestamp,
    LogLevel LogLevel,
    EventId EventId,
    Object? State,
    Exception? Exception,
    String Message);
