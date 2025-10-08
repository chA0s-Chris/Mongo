// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests;

using Chaos.Mongo.Queues;
using Chaos.Testing.Logging;
using FluentAssertions;
using Moq;
using NUnit.Framework;

public class MongoHostedServiceTests
{
    [Test]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var queues = new List<IMongoQueue>();

        // Act
        var act = () => new MongoHostedService(queues, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WhenQueuesIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = new NUnitTestLogger<MongoHostedService>();

        // Act
        var act = () => new MongoHostedService(null!, logger);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WithValidParameters_SuccessfullyCreatesInstance()
    {
        // Arrange
        var logger = new NUnitTestLogger<MongoHostedService>();

        // Act
        var service = new MongoHostedService([], logger);

        // Assert
        service.Should().NotBeNull();
    }

    [Test]
    public async Task StartAsync_Always_ReturnsCompletedTask()
    {
        // Arrange
        var logger = new NUnitTestLogger<MongoHostedService>();
        var service = new MongoHostedService([], logger);

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        // Should complete without errors
    }

    [Test]
    public async Task StartedAsync_WithAutoStartQueues_StartsAllSubscriptions()
    {
        // Arrange
        var mockQueue1 = CreateMockQueue(true);
        var mockQueue2 = CreateMockQueue(true);
        var queues = new List<IMongoQueue>
        {
            mockQueue1.Object,
            mockQueue2.Object
        };
        var logger = new NUnitTestLogger<MongoHostedService>();
        var service = new MongoHostedService(queues, logger);

        // Act
        await service.StartedAsync(CancellationToken.None);

        // Assert
        mockQueue1.Verify(x => x.StartSubscriptionAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockQueue2.Verify(x => x.StartSubscriptionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task StartedAsync_WithMixedQueues_OnlyStartsAutoStartQueues()
    {
        // Arrange
        var mockAutoStartQueue = CreateMockQueue(true);
        var mockManualStartQueue = CreateMockQueue(false);
        var queues = new List<IMongoQueue>
        {
            mockAutoStartQueue.Object,
            mockManualStartQueue.Object
        };
        var logger = new NUnitTestLogger<MongoHostedService>();
        var service = new MongoHostedService(queues, logger);

        // Act
        await service.StartedAsync(CancellationToken.None);

        // Assert
        mockAutoStartQueue.Verify(x => x.StartSubscriptionAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockManualStartQueue.Verify(x => x.StartSubscriptionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task StartedAsync_WithNoAutoStartQueues_DoesNotStartAnySubscriptions()
    {
        // Arrange
        var mockQueue1 = CreateMockQueue(false);
        var mockQueue2 = CreateMockQueue(false);
        var queues = new List<IMongoQueue>
        {
            mockQueue1.Object,
            mockQueue2.Object
        };
        var logger = new NUnitTestLogger<MongoHostedService>();
        var service = new MongoHostedService(queues, logger);

        // Act
        await service.StartedAsync(CancellationToken.None);

        // Assert
        mockQueue1.Verify(x => x.StartSubscriptionAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockQueue2.Verify(x => x.StartSubscriptionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task StartedAsync_WithNoQueues_CompletesSuccessfully()
    {
        // Arrange
        var logger = new NUnitTestLogger<MongoHostedService>();
        var service = new MongoHostedService([], logger);

        // Act
        await service.StartedAsync(CancellationToken.None);

        // Assert
        // Should complete without errors
    }

    [Test]
    public async Task StartingAsync_Always_ReturnsCompletedTask()
    {
        // Arrange
        var logger = new NUnitTestLogger<MongoHostedService>();
        var service = new MongoHostedService([], logger);

        // Act
        await service.StartingAsync(CancellationToken.None);

        // Assert
        // Should complete without errors
    }

    [Test]
    public async Task StopAsync_Always_ReturnsCompletedTask()
    {
        // Arrange
        var logger = new NUnitTestLogger<MongoHostedService>();
        var service = new MongoHostedService([], logger);

        // Act
        await service.StopAsync(CancellationToken.None);

        // Assert
        // Should complete without errors
    }

    [Test]
    public async Task StoppedAsync_Always_ReturnsCompletedTask()
    {
        // Arrange
        var logger = new NUnitTestLogger<MongoHostedService>();
        var service = new MongoHostedService([], logger);

        // Act
        await service.StoppedAsync(CancellationToken.None);

        // Assert
        // Should complete without errors
    }

    [Test]
    public async Task StoppingAsync_WithAutoStartQueues_StopsAllSubscriptions()
    {
        // Arrange
        var mockQueue1 = CreateMockQueue(true);
        var mockQueue2 = CreateMockQueue(true);
        var queues = new List<IMongoQueue>
        {
            mockQueue1.Object,
            mockQueue2.Object
        };
        var logger = new NUnitTestLogger<MongoHostedService>();
        var service = new MongoHostedService(queues, logger);

        // Act
        await service.StoppingAsync(CancellationToken.None);

        // Assert
        mockQueue1.Verify(x => x.StopSubscriptionAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockQueue2.Verify(x => x.StopSubscriptionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task StoppingAsync_WithMixedQueues_OnlyStopsAutoStartQueues()
    {
        // Arrange
        var mockAutoStartQueue = CreateMockQueue(true);
        var mockManualStartQueue = CreateMockQueue(false);
        var queues = new List<IMongoQueue>
        {
            mockAutoStartQueue.Object,
            mockManualStartQueue.Object
        };
        var logger = new NUnitTestLogger<MongoHostedService>();
        var service = new MongoHostedService(queues, logger);

        // Act
        await service.StoppingAsync(CancellationToken.None);

        // Assert
        mockAutoStartQueue.Verify(x => x.StopSubscriptionAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockManualStartQueue.Verify(x => x.StopSubscriptionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task StoppingAsync_WithNoAutoStartQueues_DoesNotStopAnySubscriptions()
    {
        // Arrange
        var mockQueue1 = CreateMockQueue(false);
        var mockQueue2 = CreateMockQueue(false);
        var queues = new List<IMongoQueue>
        {
            mockQueue1.Object,
            mockQueue2.Object
        };
        var logger = new NUnitTestLogger<MongoHostedService>();
        var service = new MongoHostedService(queues, logger);

        // Act
        await service.StoppingAsync(CancellationToken.None);

        // Assert
        mockQueue1.Verify(x => x.StopSubscriptionAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockQueue2.Verify(x => x.StopSubscriptionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task StoppingAsync_WithNoQueues_CompletesSuccessfully()
    {
        // Arrange
        var logger = new NUnitTestLogger<MongoHostedService>();
        var service = new MongoHostedService([], logger);

        // Act
        await service.StoppingAsync(CancellationToken.None);

        // Assert
        // Should complete without errors
    }

    private static Mock<IMongoQueue> CreateMockQueue(Boolean autoStart)
    {
        var mockQueue = new Mock<IMongoQueue>();
        var queueDefinition = new MongoQueueDefinition
        {
            CollectionName = "test-queue",
            PayloadType = typeof(TestPayload),
            QueryLimit = 1,
            PayloadHandlerType = typeof(IMongoQueuePayloadHandler<TestPayload>),
            AutoStartSubscription = autoStart
        };

        mockQueue.Setup(x => x.QueueDefinition).Returns(queueDefinition);
        mockQueue.Setup(x => x.StartSubscriptionAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
        mockQueue.Setup(x => x.StopSubscriptionAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        return mockQueue;
    }

    public class TestPayload;
}
