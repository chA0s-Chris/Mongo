// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests.Queues;

using Chaos.Mongo.Queues;
using FluentAssertions;
using MongoDB.Driver;
using NUnit.Framework;

public class MongoQueuePayloadPrioritizerTests
{
    [Test]
    public void CreateSortDefinition_CalledMultipleTimes_ReturnsConsistentResults()
    {
        // Arrange
        var prioritizer = new MongoQueuePayloadPrioritizer();
        var serializerRegistry = MongoDB.Bson.Serialization.BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<MongoQueueItem<TestPayload>>();
        var renderContext = new RenderArgs<MongoQueueItem<TestPayload>>(documentSerializer, serializerRegistry);

        // Act
        var result1 = prioritizer.CreateSortDefinition<TestPayload>();
        var result2 = prioritizer.CreateSortDefinition<TestPayload>();
        var rendered1 = result1.Render(renderContext);
        var rendered2 = result2.Render(renderContext);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        rendered1.ToString().Should().Be(rendered2.ToString());
    }

    [Test]
    public void CreateSortDefinition_RenderedDefinition_SortsAscendingById()
    {
        // Arrange
        var prioritizer = new MongoQueuePayloadPrioritizer();
        var sortDefinition = prioritizer.CreateSortDefinition<TestPayload>();
        var serializerRegistry = MongoDB.Bson.Serialization.BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<MongoQueueItem<TestPayload>>();
        var renderContext = new RenderArgs<MongoQueueItem<TestPayload>>(documentSerializer, serializerRegistry);

        // Act
        var rendered = sortDefinition.Render(renderContext);

        // Assert
        rendered.Should().NotBeNull();
        rendered.ToString().Should().Contain("_id");
        rendered.ToString().Should().Contain("1"); // 1 indicates ascending order
    }

    [Test]
    public void CreateSortDefinition_WithDifferentTypes_ReturnsValidSortDefinitions()
    {
        // Arrange
        var prioritizer = new MongoQueuePayloadPrioritizer();

        // Act
        var result1 = prioritizer.CreateSortDefinition<TestPayload>();
        var result2 = prioritizer.CreateSortDefinition<AnotherTestPayload>();

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
    }

    [Test]
    public void CreateSortDefinition_WithValidType_ReturnsNotNull()
    {
        // Arrange
        var prioritizer = new MongoQueuePayloadPrioritizer();

        // Act
        var result = prioritizer.CreateSortDefinition<TestPayload>();

        // Assert
        result.Should().NotBeNull();
    }

    public class AnotherTestPayload;

    public class TestPayload;
}
