// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests.Integration.Migrations;

using Chaos.Mongo.Migrations;
using Chaos.Testing.Logging;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Testcontainers.MongoDb;

public class MongoMigrationConcurrencyTests
{
    private MongoDbContainer _container;

    [OneTimeSetUp]
    public async Task GetMongoDbContainer() => _container = await MongoDbTestContainer.StartContainerAsync();

    [Test]
    public async Task RunMigrationsAsync_WithConcurrentExecutions_OnlyOneExecutes()
    {
        // Arrange
        var databaseName = $"ConcurrentMigrationTest_{Guid.NewGuid():N}";
        var connectionString = _container.GetConnectionString();
        MigrationTestTracker.Reset();

        var serviceProvider = new ServiceCollection()
                              .AddNUnitTestLogging()
                              .AddMongo(connectionString, databaseName)
                              .WithMigration<ConcurrentTestMigration>()
                              .Services
                              .BuildServiceProvider();

        var runner1 = serviceProvider.GetRequiredService<IMongoMigrationRunner>();
        var runner2 = serviceProvider.GetRequiredService<IMongoMigrationRunner>();

        // Act
        var task1 = Task.Run(async () => await runner1.RunMigrationsAsync());
        var task2 = Task.Run(async () => await runner2.RunMigrationsAsync());

        await Task.WhenAll(task1, task2);

        // Assert
        MigrationTestTracker.ExecutedMigrations.Should().ContainSingle(nameof(ConcurrentTestMigration));
    }

    [Test]
    public async Task RunMigrationsAsync_WithLockExpiration_ThrowsInvalidOperationException()
    {
        // Arrange
        var databaseName = $"LockExpirationTest_{Guid.NewGuid():N}";
        var connectionString = _container.GetConnectionString();
        MigrationTestTracker.Reset();

        var serviceProvider = new ServiceCollection()
                              .AddNUnitTestLogging()
                              .AddMongo(connectionString, databaseName, options =>
                              {
                                  options.MigrationLockLeaseTime = TimeSpan.FromMilliseconds(50);
                              })
                              .WithMigration<VerySlowTestMigration>()
                              .Services
                              .BuildServiceProvider();

        var runner = serviceProvider.GetRequiredService<IMongoMigrationRunner>();

        // Act
        var act = async () => await runner.RunMigrationsAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*lock*expired*");
    }
}
