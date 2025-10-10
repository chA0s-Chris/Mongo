// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests;

using FluentAssertions;
using MongoDB.Driver;
using NUnit.Framework;

public class MongoClientSettingsFactoryTests
{
    [Test]
    public void CreateMongoClientSettings_WhenUrlIsNull_ThrowsArgumentNull()
    {
        // Arrange
        var factory = new MongoClientSettingsFactory();

        // Act
        var act = () => factory.CreateMongoClientSettings(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void CreateMongoClientSettings_WithValidUrl_ReturnsSettingsFromUrl()
    {
        // Arrange
        var url = new MongoUrl("mongodb://localhost:27017/testdb");
        var factory = new MongoClientSettingsFactory();

        // Act
        var settings = factory.CreateMongoClientSettings(url);

        // Assert
        settings.Should().NotBeNull();
        settings.Server.Host.Should().Be("localhost");
        settings.Server.Port.Should().Be(27017);
    }
}
