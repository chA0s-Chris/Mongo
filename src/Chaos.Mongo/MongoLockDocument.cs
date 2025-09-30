// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo;

using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class MongoLockDocument
{
    public String? Holder { get; set; }

    [BsonId]
    public required String Id { get; set; }

    public DateTime LeaseUntilUtc { get; set; }
}
