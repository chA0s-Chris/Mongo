// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests.Integration;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using NUnit.Framework;
using Testcontainers.MongoDb;

public class MongoHelperLockIntegrationTests
{
    private MongoDbContainer _container;

    [OneTimeSetUp]
    public async Task GetMongoDbContainer() => _container = await MongoDbTestContainer.StartContainerAsync();

    [Test]
    public async Task ReleaseLockAsync_WhenCalledDirectly_ShouldDeleteLockForMatchingHolder()
    {
        // Arrange
        var url = MongoUrl.Create(_container.GetConnectionString());
        var serviceProvider = new ServiceCollection()
                              .AddMongo(url, configure: options =>
                              {
                                  options.DefaultDatabase = "LockTestDb";
                                  options.HolderId = "direct-release-holder";
                              })
                              .BuildServiceProvider();

        var mongoHelper = (MongoHelper)serviceProvider.GetRequiredService<IMongoHelper>();
        var lockCollection = mongoHelper.Database.GetCollection<MongoLockDocument>("_locks");

        await mongoHelper.TryAcquireLockAsync("direct-release-lock");

        // Act
        await mongoHelper.ReleaseLockAsync("direct-release-lock", "direct-release-holder");

        // Assert
        var lockDoc = await lockCollection.Find(x => x.Id == "direct-release-lock").FirstOrDefaultAsync();
        lockDoc.Should().BeNull();
    }

    [Test]
    public async Task ReleaseLockAsync_WhenCalledWithWrongHolder_ShouldNotDeleteLock()
    {
        // Arrange
        var url = MongoUrl.Create(_container.GetConnectionString());
        var serviceProvider = new ServiceCollection()
                              .AddMongo(url, configure: options =>
                              {
                                  options.DefaultDatabase = "LockTestDb";
                                  options.HolderId = "actual-holder";
                              })
                              .BuildServiceProvider();

        var mongoHelper = (MongoHelper)serviceProvider.GetRequiredService<IMongoHelper>();
        var lockCollection = mongoHelper.Database.GetCollection<MongoLockDocument>("_locks");

        await mongoHelper.TryAcquireLockAsync("protected-lock");

        // Act - Try to release with wrong holder ID
        await mongoHelper.ReleaseLockAsync("protected-lock", "wrong-holder");

        // Assert - Lock should still exist
        var lockDoc = await lockCollection.Find(x => x.Id == "protected-lock").FirstOrDefaultAsync();
        lockDoc.Should().NotBeNull();
        lockDoc.Holder.Should().Be("actual-holder");
    }

    [Test]
    public async Task ReleaseLockAsync_WhenLockIsHeld_ShouldDeleteLockDocument()
    {
        // Arrange
        var url = MongoUrl.Create(_container.GetConnectionString());
        var serviceProvider = new ServiceCollection()
                              .AddMongo(url, configure: options =>
                              {
                                  options.DefaultDatabase = "LockTestDb";
                                  options.HolderId = "test-holder";
                              })
                              .BuildServiceProvider();

        var mongoHelper = (MongoHelper)serviceProvider.GetRequiredService<IMongoHelper>();
        var lockCollection = mongoHelper.Database.GetCollection<MongoLockDocument>("_locks");

        await using (var lockInstance = await mongoHelper.TryAcquireLockAsync("release-test-lock"))
        {
            lockInstance.Should().NotBeNull();

            // Verify lock exists in database
            var lockDoc = await lockCollection.Find(x => x.Id == "release-test-lock").FirstOrDefaultAsync();
            lockDoc.Should().NotBeNull();
            lockDoc.Holder.Should().Be("test-holder");
        }

        // Act - DisposeAsync calls ReleaseLockAsync

        // Assert - Lock should be deleted
        var deletedLock = await lockCollection.Find(x => x.Id == "release-test-lock").FirstOrDefaultAsync();
        deletedLock.Should().BeNull();
    }

    [Test]
    public async Task TryAcquireLockAsync_AfterLockRelease_ShouldAllowReacquisition()
    {
        // Arrange
        var url = MongoUrl.Create(_container.GetConnectionString());
        var mongoHelper = new ServiceCollection()
                          .AddMongo(url, configure: options =>
                          {
                              options.DefaultDatabase = "LockTestDb";
                          })
                          .BuildServiceProvider()
                          .GetRequiredService<IMongoHelper>();

        // Acquire and release lock
        await using (var firstLock = await mongoHelper.TryAcquireLockAsync("reacquire-lock"))
        {
            firstLock.Should().NotBeNull();
        }

        // Act - Try to acquire again
        await using var secondLock = await mongoHelper.TryAcquireLockAsync("reacquire-lock");

        // Assert
        secondLock.Should().NotBeNull();
        secondLock.Id.Should().Be("reacquire-lock");
    }

    [Test]
    public async Task TryAcquireLockAsync_MultipleDifferentLocks_ShouldAcquireIndependently()
    {
        // Arrange
        var url = MongoUrl.Create(_container.GetConnectionString());
        var mongoHelper = new ServiceCollection()
                          .AddMongo(url, configure: options =>
                          {
                              options.DefaultDatabase = "LockTestDb";
                          })
                          .BuildServiceProvider()
                          .GetRequiredService<IMongoHelper>();

        // Act
        await using var lock1 = await mongoHelper.TryAcquireLockAsync("lock-1");
        await using var lock2 = await mongoHelper.TryAcquireLockAsync("lock-2");
        await using var lock3 = await mongoHelper.TryAcquireLockAsync("lock-3");

        // Assert
        lock1.Should().NotBeNull();
        lock2.Should().NotBeNull();
        lock3.Should().NotBeNull();
        lock1.Id.Should().Be("lock-1");
        lock2.Id.Should().Be("lock-2");
        lock3.Id.Should().Be("lock-3");
    }

    [Test]
    public async Task TryAcquireLockAsync_WhenLockDoesNotExist_ShouldAcquireLock()
    {
        // Arrange
        var url = MongoUrl.Create(_container.GetConnectionString());
        var mongoHelper = new ServiceCollection()
                          .AddMongo(url, configure: options =>
                          {
                              options.DefaultDatabase = "LockTestDb";
                          })
                          .BuildServiceProvider()
                          .GetRequiredService<IMongoHelper>();

        // Act
        await using var lockInstance = await mongoHelper.TryAcquireLockAsync("test-lock");

        // Assert
        lockInstance.Should().NotBeNull();
        lockInstance.Id.Should().Be("test-lock");
        lockInstance.ValidUntilUtc.Should().BeAfter(DateTime.UtcNow);
    }

    [Test]
    public async Task TryAcquireLockAsync_WhenLockHasExpired_ShouldAcquireLock()
    {
        // Arrange
        var url = MongoUrl.Create(_container.GetConnectionString());
        var mongoHelper = new ServiceCollection()
                          .AddMongo(url, configure: options =>
                          {
                              options.DefaultDatabase = "LockTestDb";
                          })
                          .BuildServiceProvider()
                          .GetRequiredService<IMongoHelper>();

        // Acquire lock with very short lease time
        await using (var expiredLock = await mongoHelper.TryAcquireLockAsync("expired-lock", TimeSpan.FromMilliseconds(1)))
        {
            expiredLock.Should().NotBeNull();
        }

        // Wait for lock to expire
        await Task.Delay(100);

        // Act
        await using var newLock = await mongoHelper.TryAcquireLockAsync("expired-lock");

        // Assert
        newLock.Should().NotBeNull();
        newLock.Id.Should().Be("expired-lock");
    }

    [Test]
    public async Task TryAcquireLockAsync_WhenLockIsHeldByDifferentHolder_ShouldReturnNull()
    {
        // Arrange
        var url = MongoUrl.Create(_container.GetConnectionString());
        var uniqueLockName = $"contended-lock-{Guid.NewGuid()}";

        var helper1 = new ServiceCollection()
                      .AddMongo(url, configure: options =>
                      {
                          options.DefaultDatabase = "LockTestDb";
                          options.HolderId = "holder-1";
                      })
                      .BuildServiceProvider()
                      .GetRequiredService<IMongoHelper>();

        var helper2 = new ServiceCollection()
                      .AddMongo(url, configure: options =>
                      {
                          options.DefaultDatabase = "LockTestDb";
                          options.HolderId = "holder-2";
                      })
                      .BuildServiceProvider()
                      .GetRequiredService<IMongoHelper>();

        // Act
        await using var lock1 = await helper1.TryAcquireLockAsync(uniqueLockName);
        var lock2 = await helper2.TryAcquireLockAsync(uniqueLockName);

        // Assert
        lock1.Should().NotBeNull();
        lock2.Should().BeNull();
    }

    [Test]
    public async Task TryAcquireLockAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var url = MongoUrl.Create(_container.GetConnectionString());
        var mongoHelper = new ServiceCollection()
                          .AddMongo(url, configure: options =>
                          {
                              options.DefaultDatabase = "LockTestDb";
                          })
                          .BuildServiceProvider()
                          .GetRequiredService<IMongoHelper>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = async () => await mongoHelper.TryAcquireLockAsync("cancel-lock", cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task TryAcquireLockAsync_WithCustomLeaseTime_ShouldSetCorrectExpiry()
    {
        // Arrange
        var url = MongoUrl.Create(_container.GetConnectionString());
        var mongoHelper = new ServiceCollection()
                          .AddMongo(url, configure: options =>
                          {
                              options.DefaultDatabase = "LockTestDb";
                          })
                          .BuildServiceProvider()
                          .GetRequiredService<IMongoHelper>();
        var leaseTime = TimeSpan.FromMinutes(10);
        var beforeAcquire = DateTime.UtcNow;

        // Act
        await using var lockInstance = await mongoHelper.TryAcquireLockAsync("custom-lease-lock", leaseTime);

        // Assert
        lockInstance.Should().NotBeNull();
        lockInstance.ValidUntilUtc.Should().BeCloseTo(beforeAcquire.Add(leaseTime), TimeSpan.FromSeconds(2));
    }

    [Test]
    public async Task TryAcquireLockAsync_WithEmptyLockName_ShouldThrowArgumentException()
    {
        // Arrange
        var url = MongoUrl.Create(_container.GetConnectionString());
        var mongoHelper = new ServiceCollection()
                          .AddMongo(url, configure: options =>
                          {
                              options.DefaultDatabase = "LockTestDb";
                          })
                          .BuildServiceProvider()
                          .GetRequiredService<IMongoHelper>();

        // Act
        var act = async () => await mongoHelper.TryAcquireLockAsync(String.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task TryAcquireLockAsync_WithNullLockName_ShouldThrowArgumentException()
    {
        // Arrange
        var url = MongoUrl.Create(_container.GetConnectionString());
        var mongoHelper = new ServiceCollection()
                          .AddMongo(url, configure: options =>
                          {
                              options.DefaultDatabase = "LockTestDb";
                          })
                          .BuildServiceProvider()
                          .GetRequiredService<IMongoHelper>();

        // Act
        var act = async () => await mongoHelper.TryAcquireLockAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
