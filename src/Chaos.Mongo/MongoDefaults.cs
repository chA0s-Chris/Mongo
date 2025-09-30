// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

public static class MongoDefaults
{
    public const String LockCollectionName = "_locks";

    public const Boolean UseDefaultCollectionNames = true;

    public static TimeSpan LockLeaseTime => TimeSpan.FromMinutes(5);

    public static TimeSpan RetryDelay => TimeSpan.FromMicroseconds(500);
}
