// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests.Integration;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using NUnit.Framework;
using Testcontainers.MongoDb;

public class MongoHelperExtensionsIntegrationTests
{
    private MongoDbContainer _container;

    [Test]
    public async Task ExecuteInTransaction_ShouldRetryOnTransientErrorAndEventuallyCommitChanges()
    {
        var url = MongoUrl.Create(_container.GetConnectionString());

        var mongoHelper = new ServiceCollection()
                          .AddMongo(url, configure: options =>
                          {
                              options.DefaultDatabase = "ExecuteInTransactionDb";
                              options.AddMapping<TestDocument>("TestDocuments");
                              options.AddMapping<Counter>("Counters");
                          })
                          .BuildServiceProvider()
                          .GetRequiredService<IMongoHelper>();

        var counters = mongoHelper.GetCollection<Counter>();
        var testDocuments = mongoHelper.GetCollection<TestDocument>();

        await counters.InsertOneAsync(new()
        {
            Id = "Test",
            Value = 42
        });

        await testDocuments.InsertOneAsync(new()
        {
            Id = ObjectId.GenerateNewId(),
            Value = 0
        });

        var pass = 1;

        await mongoHelper.ExecuteInTransaction(async (_, session, token) =>
        {
            var currentCounter = await counters.Find(session, x => x.Id == "Test")
                                               .Project(x => x.Value)
                                               .FirstOrDefaultAsync(token);

            currentCounter.Should().Be(42);

            await counters.UpdateOneAsync(session, x => x.Id == "Test",
                                          Builders<Counter>.Update
                                                           .Inc(x => x.Value, 1), cancellationToken: token);

            var documentsToAdd = Enumerable.Range(1, 10)
                                           .Select(x => new TestDocument
                                           {
                                               Id = ObjectId.GenerateNewId(),
                                               Value = x
                                           });

            await testDocuments.InsertManyAsync(session, documentsToAdd, cancellationToken: token);

            pass++;
            if (pass < 5)
            {
                // simulate a transient error that should be retried
                var mongoException = new MongoException("Something went wrong...");
                mongoException.AddErrorLabel("TransientTransactionError");
                throw mongoException;
            }
        });


        var counter = await counters.Find(x => x.Id == "Test")
                                    .Project(x => x.Value)
                                    .FirstAsync();

        counter.Should().Be(43);

        var documentCount = await testDocuments.CountDocumentsAsync(FilterDefinition<TestDocument>.Empty);
        documentCount.Should().Be(11);
    }

    [Test]
    public async Task ExecuteInTransaction_ShouldStopOnApplicationErrorAndRollbackChanges()
    {
        var url = MongoUrl.Create(_container.GetConnectionString());

        var mongoHelper = new ServiceCollection()
                          .AddMongo(url, configure: options =>
                          {
                              options.DefaultDatabase = "ExecuteInTransactionDb";
                              options.AddMapping<TestDocument>("TestDocuments");
                          })
                          .BuildServiceProvider()
                          .GetRequiredService<IMongoHelper>();

        var counters = mongoHelper.GetCollection<Counter>();
        await counters.InsertOneAsync(new()
        {
            Id = "Test",
            Value = 42
        });

        try
        {
            _ = await mongoHelper.ExecuteInTransaction(async (_, session, token) =>
            {
                var result = await counters.UpdateOneAsync(session, x => x.Id == "Test",
                                                           Builders<Counter>.Update
                                                                            .Inc(x => x.Value, 100), cancellationToken: token);

                result.ModifiedCount.Should().Be(1);

                throw new InvalidOperationException("Something went wrong...");
#pragma warning disable CS0162 // Unreachable code detected
                return 0;
#pragma warning restore CS0162 // Unreachable code detected
            });
        }
        catch (InvalidOperationException e)
        {
            e.Message.Should().Be("Something went wrong...");
        }

        var currentCounter = await counters.Find(x => x.Id == "Test")
                                           .Project(x => x.Value)
                                           .FirstOrDefaultAsync();

        currentCounter.Should().Be(42);
    }

    [OneTimeSetUp]
    public async Task GetMongoDbContainer() => _container = await MongoDbTestContainer.StartContainerAsync();

    private class Counter
    {
        [BsonId]
        public required String Id { get; init; }

        public Int32 Value { get; init; }
    }

    private class TestDocument
    {
        [BsonId]
        public ObjectId Id { get; init; }

        public Int32 Value { get; init; }
    }
}
