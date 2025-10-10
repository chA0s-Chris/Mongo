// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests.Migrations;

using Chaos.Mongo.Migrations;
using Chaos.Testing.Logging;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

public class MongoMigrationRunnerTests
{
    [Test]
    public void Constructor_WithMigrationsInWrongOrder_SortsThemAlphabetically()
    {
        // Arrange
        var helper = new Mock<IMongoHelper>();
        var options = Options.Create(new MongoOptions());
        var logger = new NUnitTestLogger<MongoMigrationRunner>();
        var timeProvider = new FakeTimeProvider();

        var migration1 = new Mock<IMongoMigration>();
        migration1.SetupGet(x => x.Id).Returns("003_Third");

        var migration2 = new Mock<IMongoMigration>();
        migration2.SetupGet(x => x.Id).Returns("001_First");

        var migration3 = new Mock<IMongoMigration>();
        migration3.SetupGet(x => x.Id).Returns("002_Second");

        // Act
        var sut = new MongoMigrationRunner(
            [migration1.Object, migration2.Object, migration3.Object],
            helper.Object,
            options,
            logger,
            timeProvider);

        // Assert
        var migrations = sut.Migrations;
        migrations.Should().HaveCount(3);
        migrations[0].Id.Should().Be("001_First");
        migrations[1].Id.Should().Be("002_Second");
        migrations[2].Id.Should().Be("003_Third");
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var migration = new Mock<IMongoMigration>();
        migration.SetupGet(x => x.Id).Returns("001_Test");
        var helper = new Mock<IMongoHelper>();
        var options = Options.Create(new MongoOptions());
        var timeProvider = new FakeTimeProvider();

        // Act
        var act = () => new MongoMigrationRunner([migration.Object], helper.Object, options, null!, timeProvider);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void Constructor_WithNullMigrations_ThrowsArgumentNullException()
    {
        // Arrange
        var helper = new Mock<IMongoHelper>(MockBehavior.Strict);
        var options = Options.Create(new MongoOptions());
        var logger = new NUnitTestLogger<MongoMigrationRunner>();
        var timeProvider = new FakeTimeProvider();

        // Act
        var act = () => new MongoMigrationRunner(null!, helper.Object, options, logger, timeProvider);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("migrations");
    }

    [Test]
    public void Constructor_WithNullMongoHelper_ThrowsArgumentNullException()
    {
        // Arrange
        var migration = new Mock<IMongoMigration>();
        migration.SetupGet(x => x.Id).Returns("001_Test");
        var options = Options.Create(new MongoOptions());
        var logger = new NUnitTestLogger<MongoMigrationRunner>();
        var timeProvider = new FakeTimeProvider();

        // Act
        var act = () => new MongoMigrationRunner([migration.Object], null!, options, logger, timeProvider);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("mongoHelper");
    }

    [Test]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var migration = new Mock<IMongoMigration>();
        migration.SetupGet(x => x.Id).Returns("001_Test");
        var helper = new Mock<IMongoHelper>();
        var logger = new NUnitTestLogger<MongoMigrationRunner>();
        var timeProvider = new FakeTimeProvider();

        // Act
        var act = () => new MongoMigrationRunner([migration.Object], helper.Object, null!, logger, timeProvider);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Test]
    public void Constructor_WithNullTimeProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var migration = new Mock<IMongoMigration>();
        migration.SetupGet(x => x.Id).Returns("001_Test");
        var helper = new Mock<IMongoHelper>();
        var options = Options.Create(new MongoOptions());
        var logger = new NUnitTestLogger<MongoMigrationRunner>();

        // Act
        var act = () => new MongoMigrationRunner([migration.Object], helper.Object, options, logger, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("timeProvider");
    }

    [Test]
    public async Task RunMigrationsAsync_WithLockAlreadyHeld_SkipsMigrations()
    {
        // Arrange
        var helper = new Mock<IMongoHelper>(MockBehavior.Strict);
        var database = new Mock<IMongoDatabase>(MockBehavior.Strict);
        var historyCollection = new Mock<IMongoCollection<MongoMigrationHistoryItem>>(MockBehavior.Strict);
        var migration = new Mock<IMongoMigration>(MockBehavior.Strict);
        var options = Options.Create(new MongoOptions());
        var logger = new NUnitTestLogger<MongoMigrationRunner>();
        var timeProvider = new FakeTimeProvider();

        helper.SetupGet(x => x.Database).Returns(database.Object);
        database.Setup(x => x.GetCollection<MongoMigrationHistoryItem>(It.IsAny<String>(), null))
                .Returns(historyCollection.Object);

        helper.Setup(x => x.TryAcquireLockAsync(It.IsAny<String>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((IMongoLock?)null); // Lock is held by another process

        migration.SetupGet(x => x.Id).Returns("001_TestMigration");

        var sut = new MongoMigrationRunner([migration.Object], helper.Object, options, logger, timeProvider);

        // Act
        await sut.RunMigrationsAsync(CancellationToken.None);

        // Assert
        migration.Verify(x => x.ApplyAsync(It.IsAny<IMongoHelper>(), It.IsAny<IClientSessionHandle>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
