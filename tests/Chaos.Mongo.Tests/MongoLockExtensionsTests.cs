// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests;

using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

public class MongoLockExtensionsTests
{
    [Test]
    public async Task EnsureValid_AfterDisposal_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var mongoLock = new MongoLock("disposed-lock", timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime, timeProvider, () => Task.CompletedTask);
        await mongoLock.DisposeAsync();

        // Act
        var act = () => mongoLock.EnsureValid();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*disposed-lock*");
    }

    [Test]
    public void EnsureValid_BeforeAndAfterExpiration_ShouldTransitionBehavior()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var mongoLock = new MongoLock("transition-lock", timeProvider.GetUtcNow().AddMilliseconds(100).UtcDateTime, timeProvider, () => Task.CompletedTask);

        // Act & Assert - Valid before expiration
        mongoLock.EnsureValid().Should().BeSameAs(mongoLock);

        // Wait for expiration
        timeProvider.Advance(TimeSpan.FromMilliseconds(150));

        // Act & Assert - Invalid after expiration
        var act = () => mongoLock.EnsureValid();
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void EnsureValid_MultipleLocksWithDifferentStates_ShouldValidateIndependently()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var validLock = new MongoLock("valid-lock", timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime, timeProvider, () => Task.CompletedTask);
        var expiredLock = new MongoLock("expired-lock", timeProvider.GetUtcNow().AddMilliseconds(-100).UtcDateTime, timeProvider, () => Task.CompletedTask);

        // Act & Assert
        validLock.EnsureValid().Should().BeSameAs(validLock);
        var act = () => expiredLock.EnsureValid();
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void EnsureValid_WithExpiredLock_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var mongoLock = new MongoLock("expired-lock", timeProvider.GetUtcNow().AddMilliseconds(-100).UtcDateTime, timeProvider, () => Task.CompletedTask);

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
        var timeProvider = new FakeTimeProvider();
        var mongoLock = new MongoLock("fluent-lock", timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime, timeProvider, () => Task.CompletedTask);

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
        var timeProvider = new FakeTimeProvider();
        var mongoLock = new MongoLock("test-lock", timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime, timeProvider, () => Task.CompletedTask);

        // Act
        var result = mongoLock.EnsureValid();

        // Assert
        result.Should().BeSameAs(mongoLock);
    }
}
