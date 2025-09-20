// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests;

using FluentAssertions;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

public class MongoConnectionFactoryTests
{
    [Test]
    public void CreateConnection_WhenDatabaseNameMissing_ThrowsArgumentException()
    {
        // Arrange
        var settingsFactory = new Mock<IMongoClientSettingsFactory>(MockBehavior.Strict);
        var factory = new MongoConnectionFactory(settingsFactory.Object);
        var url = new MongoUrl("mongodb://localhost:27017"); // no database name

        // Act
        var act = () => factory.CreateConnection(url);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*database name*");
    }

    [Test]
    public void CreateConnection_WhenUrlIsNull_ThrowsArgumentNull()
    {
        // Arrange
        var settingsFactory = new Mock<IMongoClientSettingsFactory>(MockBehavior.Strict);
        var factory = new MongoConnectionFactory(settingsFactory.Object);

        // Act
        var act = () => factory.CreateConnection(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void CreateConnection_WithValidUrl_ReturnsConnection()
    {
        // Arrange
        var url = new MongoUrl("mongodb://localhost:27017/mydb");
        var settings = MongoClientSettings.FromUrl(url);

        var settingsFactory = new Mock<IMongoClientSettingsFactory>(MockBehavior.Strict);
        settingsFactory.Setup(f => f.CreateMongoClientSettings(url))
                       .Returns(settings)
                       .Verifiable();

        var factory = new MongoConnectionFactory(settingsFactory.Object);

        // Act
        var connection = factory.CreateConnection(url);

        // Assert
        connection.Should().NotBeNull();
        settingsFactory.Verify(f => f.CreateMongoClientSettings(url), Times.Once);
    }
}
