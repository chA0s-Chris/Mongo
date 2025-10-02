// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Queues;

using System.IO.Hashing;
using System.Text;

public class MongoQueueCollectionNameGenerator : IMongoQueueCollectionNameGenerator
{
    private const String NamePrefix = "_Queue.";

    public Task<String> GenerateQueueCollectionName(Type payloadType)
    {
        ArgumentNullException.ThrowIfNull(payloadType);

        var builder = new StringBuilder(NamePrefix);

        if (!String.IsNullOrEmpty(payloadType.FullName))
        {
            var hash = XxHash3.Hash(Encoding.UTF8.GetBytes(payloadType.FullName));
            var hexString = Convert.ToHexString(hash);

            builder.Append(hexString);
            builder.Append('.');
        }

        builder.Append(payloadType.Name);

        return Task.FromResult(builder.ToString());
    }
}
