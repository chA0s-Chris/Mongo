// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests.Queues;

using Chaos.Mongo.Queues;
using FluentAssertions;
using NUnit.Framework;
using System.IO.Hashing;
using System.Text;

public class MongoQueueCollectionNameGeneratorTests
{
    [Test]
    public void GenerateQueueCollectionName_CalledMultipleTimes_ReturnsSameResult()
    {
        // Arrange
        var generator = new MongoQueueCollectionNameGenerator();
        var payloadType = typeof(TestPayload);

        // Act
        var result1 = generator.GenerateQueueCollectionName(payloadType);
        var result2 = generator.GenerateQueueCollectionName(payloadType);

        // Assert
        result1.Should().Be(result2);
    }

    [Test]
    public void GenerateQueueCollectionName_WhenPayloadTypeIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var generator = new MongoQueueCollectionNameGenerator();

        // Act
        var act = () => generator.GenerateQueueCollectionName(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GenerateQueueCollectionName_WithDifferentTypes_ReturnsDifferentResults()
    {
        // Arrange
        var generator = new MongoQueueCollectionNameGenerator();
        var payloadType1 = typeof(TestPayload);
        var payloadType2 = typeof(AnotherTestPayload);

        // Act
        var result1 = generator.GenerateQueueCollectionName(payloadType1);
        var result2 = generator.GenerateQueueCollectionName(payloadType2);

        // Assert
        result1.Should().NotBe(result2);
        result1.Should().EndWith(payloadType1.Name);
        result2.Should().EndWith(payloadType2.Name);
    }

    [Test]
    public void GenerateQueueCollectionName_WithNestedType_IncludesCorrectName()
    {
        // Arrange
        var generator = new MongoQueueCollectionNameGenerator();
        var payloadType = typeof(NestedContainer.NestedPayload);

        // Act
        var result = generator.GenerateQueueCollectionName(payloadType);

        // Assert
        result.Should().StartWith("_Queue.");
        result.Should().EndWith("NestedPayload");
    }

    [Test]
    public void GenerateQueueCollectionName_WithValidType_IncludesHashOfFullName()
    {
        // Arrange
        var generator = new MongoQueueCollectionNameGenerator();
        var payloadType = typeof(TestPayload);
        var expectedHash = Convert.ToHexString(XxHash3.Hash(Encoding.UTF8.GetBytes(payloadType.FullName!)));

        // Act
        var result = generator.GenerateQueueCollectionName(payloadType);

        // Assert
        result.Should().Contain(expectedHash);
        result.Should().Be($"_Queue.{expectedHash}.{payloadType.Name}");
    }

    [Test]
    public void GenerateQueueCollectionName_WithValidType_ReturnsCorrectFormat()
    {
        // Arrange
        var generator = new MongoQueueCollectionNameGenerator();
        var payloadType = typeof(TestPayload);

        // Act
        var result = generator.GenerateQueueCollectionName(payloadType);

        // Assert
        result.Should().StartWith("_Queue.");
        result.Should().Contain(payloadType.Name);
        result.Should().MatchRegex(@"^_Queue\.[A-F0-9]{16}\.TestPayload$");
    }

    private static class NestedContainer
    {
        public class NestedPayload;
    }

    private class AnotherTestPayload;

    private class TestPayload;
}
