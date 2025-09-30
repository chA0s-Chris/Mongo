// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests;

using FluentAssertions;
using NUnit.Framework;

public class MongoLockTests
{
    [Test]
    public void Constructor_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange
        var validUntil = DateTime.UtcNow.AddMinutes(5);
        var releaseAction = () => Task.CompletedTask;

        // Act
        var act = () => new MongoLock(String.Empty, validUntil, releaseAction);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithNullId_ShouldThrowArgumentException()
    {
        // Arrange
        var validUntil = DateTime.UtcNow.AddMinutes(5);
        var releaseAction = () => Task.CompletedTask;

        // Act
        var act = () => new MongoLock(null!, validUntil, releaseAction);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithNullReleaseAction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var lockId = "test-lock";
        var validUntil = DateTime.UtcNow.AddMinutes(5);

        // Act
        var act = () => new MongoLock(lockId, validUntil, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WithValidParameters_ShouldInitializeProperties()
    {
        // Arrange
        var lockId = "test-lock";
        var validUntil = DateTime.UtcNow.AddMinutes(5);
        var releaseAction = () => Task.CompletedTask;

        // Act
        var mongoLock = new MongoLock(lockId, validUntil, releaseAction);

        // Assert
        mongoLock.Id.Should().Be(lockId);
        mongoLock.ValidUntil.Should().Be(validUntil);
    }

    [Test]
    public void Constructor_WithWhitespaceId_ShouldThrowArgumentException()
    {
        // Arrange
        var validUntil = DateTime.UtcNow.AddMinutes(5);
        var releaseAction = () => Task.CompletedTask;

        // Act
        var act = () => new MongoLock("   ", validUntil, releaseAction);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public async Task DisposeAsync_ShouldInvokeReleaseAction()
    {
        // Arrange
        var releaseInvoked = false;
        var releaseAction = () =>
        {
            releaseInvoked = true;
            return Task.CompletedTask;
        };
        var mongoLock = new MongoLock("test-lock", DateTime.UtcNow.AddMinutes(5), releaseAction);

        // Act
        await mongoLock.DisposeAsync();

        // Assert
        releaseInvoked.Should().BeTrue();
    }

    [Test]
    public async Task DisposeAsync_WhenCalledMultipleTimes_ShouldInvokeReleaseActionOnlyOnce()
    {
        // Arrange
        var releaseCount = 0;
        var releaseAction = () =>
        {
            releaseCount++;
            return Task.CompletedTask;
        };
        var mongoLock = new MongoLock("test-lock", DateTime.UtcNow.AddMinutes(5), releaseAction);

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
        Func<Task> releaseAction = () => throw new InvalidOperationException("Release failed");
        var mongoLock = new MongoLock("test-lock", DateTime.UtcNow.AddMinutes(5), releaseAction);

        // Act
        var act = async () => await mongoLock.DisposeAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task DisposeAsync_WithAsyncReleaseAction_ShouldAwaitCompletion()
    {
        // Arrange
        var releaseCompleted = false;
        var releaseAction = async () =>
        {
            await Task.Delay(100);
            releaseCompleted = true;
        };
        var mongoLock = new MongoLock("test-lock", DateTime.UtcNow.AddMinutes(5), releaseAction);

        // Act
        await mongoLock.DisposeAsync();

        // Assert
        releaseCompleted.Should().BeTrue();
    }
}
