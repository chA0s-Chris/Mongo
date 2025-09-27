// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests.Configuration;

using Chaos.Mongo.Configuration;
using FluentAssertions;
using Moq;
using NUnit.Framework;

public class MongoConfiguratorRunnerTests
{
    [Test]
    public async Task RunConfiguratorsAsync_WhenCancellationRequested_ThrowsOperationCanceled()
    {
        // Arrange
        var helper = new Mock<IMongoHelper>(MockBehavior.Strict);
        var configurator = new Mock<IMongoConfigurator>(MockBehavior.Strict);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var sut = new MongoConfiguratorRunner(helper.Object, [configurator.Object]);

        // Act
        // ReSharper disable once AccessToDisposedClosure
        var act = async () => await sut.RunConfiguratorsAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task RunConfiguratorsAsync_WhenConfiguratorThrows_StopsFurtherExecution()
    {
        // Arrange
        var helper = new Mock<IMongoHelper>(MockBehavior.Strict);
        var first = new Mock<IMongoConfigurator>(MockBehavior.Strict);
        var second = new Mock<IMongoConfigurator>(MockBehavior.Strict);

        first.Setup(x => x.ConfigureAsync(helper.Object, It.IsAny<CancellationToken>()))
             .ThrowsAsync(new InvalidOperationException("boom"));

        // Second should not be called if the first throws
        second.Setup(x => x.ConfigureAsync(It.IsAny<IMongoHelper>(), It.IsAny<CancellationToken>()))
              .Throws(new AssertionException("Second configurator should not be invoked if the first fails."));

        var sut = new MongoConfiguratorRunner(helper.Object,
        [
            first.Object,
            second.Object
        ]);

        // Act
        var act = async () => await sut.RunConfiguratorsAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task RunConfiguratorsAsync_WhenMultipleConfigurators_InvokesEachOnceInOrder()
    {
        // Arrange
        var helper = new Mock<IMongoHelper>(MockBehavior.Strict);

        var first = new Mock<IMongoConfigurator>(MockBehavior.Strict);
        var second = new Mock<IMongoConfigurator>(MockBehavior.Strict);

        var sequence = new MockSequence();
        first.InSequence(sequence)
             .Setup(x => x.ConfigureAsync(helper.Object, It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);
        second.InSequence(sequence)
              .Setup(x => x.ConfigureAsync(helper.Object, It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);

        var sut = new MongoConfiguratorRunner(helper.Object,
        [
            first.Object,
            second.Object
        ]);

        // Act
        await sut.RunConfiguratorsAsync(CancellationToken.None);

        // Assert
        first.Verify(x => x.ConfigureAsync(helper.Object, It.IsAny<CancellationToken>()), Times.Once);
        second.Verify(x => x.ConfigureAsync(helper.Object, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RunConfiguratorsAsync_WhenNoneRegistered_DoesNothing()
    {
        // Arrange
        var helper = new Mock<IMongoHelper>(MockBehavior.Strict);
        var sut = new MongoConfiguratorRunner(helper.Object, new List<IMongoConfigurator>());

        // Act
        var act = async () => await sut.RunConfiguratorsAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
