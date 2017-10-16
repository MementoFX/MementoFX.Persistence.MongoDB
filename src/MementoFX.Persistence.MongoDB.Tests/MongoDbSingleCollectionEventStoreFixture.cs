using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Memento.Messaging;
using SharpTestsEx;
using Moq;
using MongoDB.Driver;

namespace Memento.Persistence.MongoDB.Tests
{
    [TestClass]
    public class MongoDbSingleCollectionEventStoreFixture
    {
        [TestMethod]
        public void Ctor_should_throw_ArgumentNullException_on_null_mongoDatabase_and_value_of_parameter_should_be_mongoDatabase()
        {
            var eventDispatcher = new Mock<IEventDispatcher>().Object;
            Executing.This(() => new MongoDbSingleCollectionEventStore(eventDispatcher, null))
                .Should()
                .Throw<ArgumentNullException>()
                .And
                .ValueOf
                .ParamName
                .Should()
                .Be
                .EqualTo("mongoDatabase");
        }

        [TestMethod]
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

        [TestMethod]
        public void Ctor_should_set_MongoDatabase_field()
        {
            var bus = new Mock<IEventDispatcher>().Object;
            var mock = new Mock<IMongoDatabase>().Object;
            var sut = new MongoDbSingleCollectionEventStore(bus, mock);

            Assert.AreEqual(mock, MongoDbSingleCollectionEventStore.MongoDatabase);
        }
    }
}
