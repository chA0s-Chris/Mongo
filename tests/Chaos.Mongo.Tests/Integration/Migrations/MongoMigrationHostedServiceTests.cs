// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests.Integration.Migrations;

using Chaos.Mongo.Migrations;
using Chaos.Testing.Logging;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using NUnit.Framework;
using Testcontainers.MongoDb;

public class MongoMigrationHostedServiceTests
{
    private MongoDbContainer _container;

    [OneTimeSetUp]
    public async Task GetMongoDbContainer() => _container = await MongoDbTestContainer.StartContainerAsync();

    [Test]
    public async Task StartingAsync_WithApplyMigrationsOnStartupDisabled_MigrationsDoNotRun()
    {
        // Arrange
        var databaseName = $"NoAutoRunMigrationTest_{Guid.NewGuid():N}";
        var connectionString = _container.GetConnectionString();
        MigrationTestTracker.Reset();

        var services = new ServiceCollection()
                       .AddNUnitTestLogging()
                       .AddMongo(connectionString, databaseName, options =>
                       {
                           options.ApplyMigrationsOnStartup = false;
                       })
                       .WithMigration<IdempotentTestMigration>()
                       .Services;

        var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>().ToList();

        // Act
        foreach (var hostedService in hostedServices)
        {
            if (hostedService is IHostedLifecycleService lifecycleService)
            {
                await lifecycleService.StartingAsync(CancellationToken.None);
                await lifecycleService.StartedAsync(CancellationToken.None);
            }
            else
            {
                await hostedService.StartAsync(CancellationToken.None);
            }
        }

        // Assert
        MigrationTestTracker.ExecutedMigrations.Should().BeEmpty();

        // Cleanup
        foreach (var hostedService in hostedServices)
        {
            if (hostedService is IHostedLifecycleService lifecycleService)
            {
                await lifecycleService.StoppingAsync(CancellationToken.None);
                await lifecycleService.StoppedAsync(CancellationToken.None);
            }
            else
            {
                await hostedService.StopAsync(CancellationToken.None);
            }
        }
    }

    [Test]
    public async Task StartingAsync_WithApplyMigrationsOnStartupEnabled_MigrationsRunViaHostedService()
    {
        // Arrange
        var databaseName = $"AutoRunMigrationTest_{Guid.NewGuid():N}";
        var connectionString = _container.GetConnectionString();
        MigrationTestTracker.Reset();

        var services = new ServiceCollection()
                       .AddNUnitTestLogging()
                       .AddMongo(connectionString, databaseName, options =>
                       {
                           options.ApplyMigrationsOnStartup = true;
                           options.AddMapping<TestDocument>("TestDocuments");
                       })
                       .WithMigration<Migration_001_CreateTestDocumentsIndex>()
                       .WithMigration<Migration_002_CreateAuditLogsIndex>()
                       .Services;

        var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>().ToList();
        var mongoHelper = serviceProvider.GetRequiredService<IMongoHelper>();

        // Act
        foreach (var hostedService in hostedServices)
        {
            if (hostedService is IHostedLifecycleService lifecycleService)
            {
                await lifecycleService.StartingAsync(CancellationToken.None);
                await lifecycleService.StartedAsync(CancellationToken.None);
            }
            else
            {
                await hostedService.StartAsync(CancellationToken.None);
            }
        }

        // Assert
        MigrationTestTracker.ExecutedMigrations.Should().Contain("001_CreateTestDocumentsIndex");
        MigrationTestTracker.ExecutedMigrations.Should().Contain("002_CreateAuditLogsIndex");

        var historyCollection = mongoHelper.Database.GetCollection<MongoMigrationHistoryItem>("_migrations");
        var historyItems = await historyCollection.Find(x => true).ToListAsync();
        historyItems.Should().HaveCount(2);

        // Cleanup
        foreach (var hostedService in hostedServices)
        {
            if (hostedService is IHostedLifecycleService lifecycleService)
            {
                await lifecycleService.StoppingAsync(CancellationToken.None);
                await lifecycleService.StoppedAsync(CancellationToken.None);
            }
            else
            {
                await hostedService.StopAsync(CancellationToken.None);
            }
        }
    }
}
