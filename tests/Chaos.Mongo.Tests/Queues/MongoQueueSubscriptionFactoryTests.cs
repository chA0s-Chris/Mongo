// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests.Queues;

using Chaos.Mongo.Queues;
using Chaos.Testing.Logging;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

public class MongoQueueSubscriptionFactoryTests
{
    [Test]
    public void Constructor_WhenLoggerFactoryIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockMongoHelper = new Mock<IMongoHelper>();
        var mockPayloadHandlerFactory = new Mock<IMongoQueuePayloadHandlerFactory>();
        var mockPayloadPrioritizer = new Mock<IMongoQueuePayloadPrioritizer>();
        var timeProvider = TimeProvider.System;

        // Act
        var act = () => new MongoQueueSubscriptionFactory(
            mockMongoHelper.Object,
            mockPayloadHandlerFactory.Object,
            mockPayloadPrioritizer.Object,
            timeProvider,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WhenMongoHelperIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockPayloadHandlerFactory = new Mock<IMongoQueuePayloadHandlerFactory>();
        var mockPayloadPrioritizer = new Mock<IMongoQueuePayloadPrioritizer>();
        var timeProvider = TimeProvider.System;
        var loggerFactory = CreateLoggerFactory();

        // Act
        var act = () => new MongoQueueSubscriptionFactory(
            null!,
            mockPayloadHandlerFactory.Object,
            mockPayloadPrioritizer.Object,
            timeProvider,
            loggerFactory);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WhenPayloadHandlerFactoryIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockMongoHelper = new Mock<IMongoHelper>();
        var mockPayloadPrioritizer = new Mock<IMongoQueuePayloadPrioritizer>();
        var timeProvider = TimeProvider.System;
        var loggerFactory = CreateLoggerFactory();

        // Act
        var act = () => new MongoQueueSubscriptionFactory(
            mockMongoHelper.Object,
            null!,
            mockPayloadPrioritizer.Object,
            timeProvider,
            loggerFactory);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WhenPayloadPrioritizerIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockMongoHelper = new Mock<IMongoHelper>();
        var mockPayloadHandlerFactory = new Mock<IMongoQueuePayloadHandlerFactory>();
        var timeProvider = TimeProvider.System;
        var loggerFactory = CreateLoggerFactory();

        // Act
        var act = () => new MongoQueueSubscriptionFactory(
            mockMongoHelper.Object,
            mockPayloadHandlerFactory.Object,
            null!,
            timeProvider,
            loggerFactory);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WithValidParameters_SuccessfullyCreatesInstance()
    {
        // Arrange
        var mockMongoHelper = new Mock<IMongoHelper>();
        var mockPayloadHandlerFactory = new Mock<IMongoQueuePayloadHandlerFactory>();
        var mockPayloadPrioritizer = new Mock<IMongoQueuePayloadPrioritizer>();
        var timeProvider = TimeProvider.System;
        var loggerFactory = CreateLoggerFactory();

        // Act
        var factory = new MongoQueueSubscriptionFactory(
            mockMongoHelper.Object,
            mockPayloadHandlerFactory.Object,
            mockPayloadPrioritizer.Object,
            timeProvider,
            loggerFactory);

        // Assert
        factory.Should().NotBeNull();
    }

    [Test]
    public async Task CreateAndRunAsync_WhenQueueDefinitionIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockMongoHelper = new Mock<IMongoHelper>();
        var mockPayloadHandlerFactory = new Mock<IMongoQueuePayloadHandlerFactory>();
        var mockPayloadPrioritizer = new Mock<IMongoQueuePayloadPrioritizer>();
        var timeProvider = TimeProvider.System;
        var loggerFactory = CreateLoggerFactory();
        var factory = new MongoQueueSubscriptionFactory(
            mockMongoHelper.Object,
            mockPayloadHandlerFactory.Object,
            mockPayloadPrioritizer.Object,
            timeProvider,
            loggerFactory);

        // Act
        var act = async () => await factory.CreateAndRunAsync<TestPayload>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static ILoggerFactory CreateLoggerFactory()
    {
        var services = new ServiceCollection();
        services.AddNUnitTestLogging();
        return services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
    }

    public class TestPayload;
}
