using MementoFX.Messaging;
using MongoDB.Driver;
using Moq;
using SharpTestsEx;
using System;
using Xunit;

namespace MementoFX.Persistence.MongoDB.Tests
{

    public class MongoDbSingleCollectionEventStoreFixture
    {
        [Fact]
        public void Ctor_should_throw_ArgumentNullException_on_null_mongoDatabase_and_value_of_parameter_should_be_mongoDatabase()
        {
            var eventDispatcher = new Mock<IEventDispatcher>().Object;
            Executing.This(() => new MongoDbSingleCollectionEventStore(eventDispatcher, (IMongoDatabase) null))
                .Should()
                .Throw<ArgumentNullException>()
                .And
                .ValueOf
                .ParamName
                .Should()
                .Be
                .EqualTo("mongoDatabase");
        }

        [Fact]
        public void Ctor_should_throw_ArgumentNullException_on_null_eventDispatcher_and_value_of_parameter_should_be_eventDispatcher()
        {
            var mongoDataBase = new Mock<IMongoDatabase>().Object;
            Executing.This(() => new MongoDbSingleCollectionEventStore(null, mongoDataBase))
                .Should()
                .Throw<ArgumentNullException>()
                .And
                .ValueOf
                .ParamName
                .Should()
                .Be
                .EqualTo("eventDispatcher");
        }

        [Fact]
        public void Ctor_should_set_MongoDatabase_field()
        {
            var bus = new Mock<IEventDispatcher>().Object;
            var mock = new Mock<IMongoDatabase>().Object;
            var sut = new MongoDbSingleCollectionEventStore(bus, mock);

            Assert.Equal(mock, MongoDbSingleCollectionEventStore.MongoDatabase);
        }
    }
}
