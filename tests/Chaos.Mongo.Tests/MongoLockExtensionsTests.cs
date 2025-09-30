// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests;

using FluentAssertions;
using NUnit.Framework;

public class MongoLockExtensionsTests
{
    [Test]
    public async Task EnsureValid_AfterDisposal_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mongoLock = new MongoLock("disposed-lock", DateTime.UtcNow.AddMinutes(5), () => Task.CompletedTask);
        await mongoLock.DisposeAsync();

        // Act
        var act = () => mongoLock.EnsureValid();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*disposed-lock*");
    }

    [Test]
    public async Task EnsureValid_BeforeAndAfterExpiration_ShouldTransitionBehavior()
    {
        // Arrange
        var mongoLock = new MongoLock("transition-lock", DateTime.UtcNow.AddMilliseconds(100), () => Task.CompletedTask);

        // Act & Assert - Valid before expiration
        mongoLock.EnsureValid().Should().BeSameAs(mongoLock);

        // Wait for expiration
        await Task.Delay(150);

        // Act & Assert - Invalid after expiration
        var act = () => mongoLock.EnsureValid();
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void EnsureValid_MultipleLocksWithDifferentStates_ShouldValidateIndependently()
    {
        // Arrange
        var validLock = new MongoLock("valid-lock", DateTime.UtcNow.AddMinutes(5), () => Task.CompletedTask);
        var expiredLock = new MongoLock("expired-lock", DateTime.UtcNow.AddMilliseconds(-100), () => Task.CompletedTask);

        // Act & Assert
        validLock.EnsureValid().Should().BeSameAs(validLock);
        var act = () => expiredLock.EnsureValid();
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void EnsureValid_WithExpiredLock_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mongoLock = new MongoLock("expired-lock", DateTime.UtcNow.AddMilliseconds(-100), () => Task.CompletedTask);

        // Act
        var act = () => mongoLock.EnsureValid();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*expired-lock*");
    }

    [Test]
    public void EnsureValid_WithFluentStyle_ShouldAllowChaining()
    {
        // Arrange
        var mongoLock = new MongoLock("fluent-lock", DateTime.UtcNow.AddMinutes(5), () => Task.CompletedTask);

        // Act
        var result = mongoLock.EnsureValid().EnsureValid().EnsureValid();

        // Assert
        result.Should().BeSameAs(mongoLock);
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void EnsureValid_WithNullLock_ShouldThrowArgumentNullException()
    {
        // Arrange
        IMongoLock? mongoLock = null;

        // Act
        var act = () => mongoLock!.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void EnsureValid_WithValidLock_ShouldReturnLock()
    {
        // Arrange
        var mongoLock = new MongoLock("test-lock", DateTime.UtcNow.AddMinutes(5), () => Task.CompletedTask);

        // Act
        var result = mongoLock.EnsureValid();

        // Assert
        result.Should().BeSameAs(mongoLock);
    }
}
