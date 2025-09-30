// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests;

using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

public class MongoLockTests
{
    [Test]
    public void Constructor_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var validUntil = timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime;
        var releaseAction = () => Task.CompletedTask;

        // Act
        var act = () => new MongoLock(String.Empty, validUntil, timeProvider, releaseAction);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithNullId_ShouldThrowArgumentException()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var validUntil = timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime;
        var releaseAction = () => Task.CompletedTask;

        // Act
        var act = () => new MongoLock(null!, validUntil, timeProvider, releaseAction);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithNullReleaseAction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var lockId = "test-lock";
        var validUntil = timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime;

        // Act
        var act = () => new MongoLock(lockId, validUntil, timeProvider, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WithValidParameters_ShouldInitializeProperties()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var lockId = "test-lock";
        var validUntil = timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime;
        var releaseAction = () => Task.CompletedTask;

        // Act
        var mongoLock = new MongoLock(lockId, validUntil, timeProvider, releaseAction);

        // Assert
        mongoLock.Id.Should().Be(lockId);
        mongoLock.ValidUntilUtc.Should().Be(validUntil);
    }

    [Test]
    public void Constructor_WithWhitespaceId_ShouldThrowArgumentException()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var validUntil = timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime;
        var releaseAction = () => Task.CompletedTask;

        // Act
        var act = () => new MongoLock("   ", validUntil, timeProvider, releaseAction);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public async Task DisposeAsync_ShouldInvokeReleaseAction()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var releaseInvoked = false;
        var releaseAction = () =>
        {
            releaseInvoked = true;
            return Task.CompletedTask;
        };
        var mongoLock = new MongoLock("test-lock", timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime, timeProvider, releaseAction);

        // Act
        await mongoLock.DisposeAsync();

        // Assert
        releaseInvoked.Should().BeTrue();
    }

    [Test]
    public async Task DisposeAsync_WhenCalledMultipleTimes_ShouldInvokeReleaseActionOnlyOnce()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var releaseCount = 0;
        var releaseAction = () =>
        {
            releaseCount++;
            return Task.CompletedTask;
        };
        var mongoLock = new MongoLock("test-lock", timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime, timeProvider, releaseAction);

        // Act
        await mongoLock.DisposeAsync();
        await mongoLock.DisposeAsync();
        await mongoLock.DisposeAsync();

        // Assert
        releaseCount.Should().Be(1);
    }

    [Test]
    public async Task DisposeAsync_WhenReleaseActionThrows_ShouldSuppressException()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        Func<Task> releaseAction = () => throw new InvalidOperationException("Release failed");
        var mongoLock = new MongoLock("test-lock", timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime, timeProvider, releaseAction);

        // Act
        var act = async () => await mongoLock.DisposeAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task DisposeAsync_WithAsyncReleaseAction_ShouldAwaitCompletion()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var releaseCompleted = false;
        var releaseAction = () =>
        {
            releaseCompleted = true;
            return Task.CompletedTask;
        };
        var mongoLock = new MongoLock("test-lock", timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime, timeProvider, releaseAction);

        // Act
        await mongoLock.DisposeAsync();

        // Assert
        releaseCompleted.Should().BeTrue();
    }

    [Test]
    public async Task IsValid_AfterDisposal_ShouldReturnFalse()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var mongoLock = new MongoLock("test-lock", timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime, timeProvider, () => Task.CompletedTask);
        mongoLock.IsValid.Should().BeTrue();

        // Act
        await mongoLock.DisposeAsync();

        // Assert
        mongoLock.IsValid.Should().BeFalse();
    }

    [Test]
    public async Task IsValid_AfterMultipleDisposals_ShouldRemainFalse()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var mongoLock = new MongoLock("test-lock", timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime, timeProvider, () => Task.CompletedTask);

        // Act
        await mongoLock.DisposeAsync();
        await mongoLock.DisposeAsync();
        await mongoLock.DisposeAsync();

        // Assert
        mongoLock.IsValid.Should().BeFalse();
    }

    [Test]
    public void IsValid_WhenExpiringDuringTest_ShouldTransitionToFalse()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var mongoLock = new MongoLock("test-lock", timeProvider.GetUtcNow().AddMilliseconds(100).UtcDateTime, timeProvider, () => Task.CompletedTask);
        mongoLock.IsValid.Should().BeTrue();

        // Act - Wait for expiration
        timeProvider.Advance(TimeSpan.FromMilliseconds(150));

        // Assert
        mongoLock.IsValid.Should().BeFalse();
    }

    [Test]
    public void IsValid_WhenLockHasExpired_ShouldReturnFalse()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var mongoLock = new MongoLock("test-lock", timeProvider.GetUtcNow().AddMilliseconds(-100).UtcDateTime, timeProvider, () => Task.CompletedTask);

        // Act & Assert
        mongoLock.IsValid.Should().BeFalse();
    }

    [Test]
    public void IsValid_WhenLockIsNotExpiredAndNotDisposed_ShouldReturnTrue()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var mongoLock = new MongoLock("test-lock", timeProvider.GetUtcNow().AddMinutes(5).UtcDateTime, timeProvider, () => Task.CompletedTask);

        // Act & Assert
        mongoLock.IsValid.Should().BeTrue();
    }
}
