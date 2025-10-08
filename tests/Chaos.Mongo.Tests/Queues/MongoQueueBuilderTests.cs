// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests.Queues;

using Chaos.Mongo.Queues;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

public class MongoQueueBuilderTests
{
    [Test]
    public void Constructor_WhenServicesIsNull_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new MongoQueueBuilder<TestPayload>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WithValidServices_SuccessfullyCreatesInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Assert
        builder.Should().NotBeNull();
    }

    [Test]
    public void RegisterQueue_CalledTwice_OnlyRegistersOnce()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);
        builder.WithPayloadHandler<TestPayloadHandler>();

        // Act
        builder.RegisterQueue();
        var countAfterFirst = services.Count(s => s.ServiceType == typeof(IMongoQueue<TestPayload>));
        builder.RegisterQueue();
        var countAfterSecond = services.Count(s => s.ServiceType == typeof(IMongoQueue<TestPayload>));

        // Assert
        countAfterFirst.Should().Be(1);
        countAfterSecond.Should().Be(1);
    }

    [Test]
    public void RegisterQueue_WithoutPayloadHandler_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var act = () => builder.RegisterQueue();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Payload handler type or factory must be specified.");
    }

    [Test]
    public void Validate_WhenAlreadyRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IMongoQueue<TestPayload>>(_ => null!);
        var builder = new MongoQueueBuilder<TestPayload>(services);
        builder.WithPayloadHandler<TestPayloadHandler>();

        // Act
        var act = () => builder.RegisterQueue();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("A registration for a MongoDB queue with payload TestPayload already exists.");
    }

    [Test]
    public void Validate_WithBothHandlerTypeAndFactory_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);
        builder.WithPayloadHandler<TestPayloadHandler>();
        builder.WithPayloadHandler(_ => new TestPayloadHandler());

        // Act
        var act = () => builder.RegisterQueue();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Payload handler type and factory cannot be specified together.");
    }

    [Test]
    public void WithAutoStartSubscription_ReturnsBuilderForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var result = builder.WithAutoStartSubscription();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void WithCollectionName_WhenCollectionNameIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var act = () => builder.WithCollectionName(String.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void WithCollectionName_WhenCollectionNameIsNull_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var act = () => builder.WithCollectionName(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void WithCollectionName_WhenCollectionNameIsWhitespace_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var act = () => builder.WithCollectionName("   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void WithCollectionName_WithValidName_ReturnsBuilderForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var result = builder.WithCollectionName("test-queue");

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void WithoutAutoStartSubscription_ReturnsBuilderForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var result = builder.WithoutAutoStartSubscription();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void WithPayloadHandler_GenericVersion_ReturnsBuilderForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var result = builder.WithPayloadHandler<TestPayloadHandler>();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void WithPayloadHandler_WithAbstractType_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var act = () => builder.WithPayloadHandler(typeof(AbstractPayloadHandler));

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Payload handler type must be a non-abstract class implementing *");
    }

    [Test]
    public void WithPayloadHandler_WithFactory_ReturnsBuilderForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var result = builder.WithPayloadHandler(_ => new TestPayloadHandler());

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void WithPayloadHandler_WithFactoryNull_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var act = () => builder.WithPayloadHandler((Func<IServiceProvider, IMongoQueuePayloadHandler<TestPayload>>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void WithPayloadHandler_WithInterface_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var act = () => builder.WithPayloadHandler(typeof(IMongoQueuePayloadHandler<TestPayload>));

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Payload handler type must be a non-abstract class implementing *");
    }

    [Test]
    public void WithPayloadHandler_WithNullType_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var act = () => builder.WithPayloadHandler((Type)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void WithPayloadHandler_WithType_ReturnsBuilderForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var result = builder.WithPayloadHandler(typeof(TestPayloadHandler));

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void WithPayloadHandler_WithWrongInterfaceType_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var act = () => builder.WithPayloadHandler(typeof(WrongPayloadHandler));

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Payload handler type must be a non-abstract class implementing *");
    }

    [Test]
    public void WithQueueLimit_WithNegativeValue_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var act = () => builder.WithQueryLimit(-1);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Query limit must be greater than 0.*");
    }

    [Test]
    public void WithQueueLimit_WithPositiveValue_ReturnsBuilderForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var result = builder.WithQueryLimit(10);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void WithQueueLimit_WithZeroValue_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MongoQueueBuilder<TestPayload>(services);

        // Act
        var act = () => builder.WithQueryLimit(0);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Query limit must be greater than 0.*");
    }

    public abstract class AbstractPayloadHandler : IMongoQueuePayloadHandler<TestPayload>
    {
        public abstract Task HandlePayloadAsync(TestPayload payload, CancellationToken cancellationToken = default);
    }

    public class AnotherPayload;

    public class TestPayload;

    public class TestPayloadHandler : IMongoQueuePayloadHandler<TestPayload>
    {
        public Task HandlePayloadAsync(TestPayload payload, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    public class WrongPayloadHandler : IMongoQueuePayloadHandler<AnotherPayload>
    {
        public Task HandlePayloadAsync(AnotherPayload payload, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
