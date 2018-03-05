using MementoFX.Messaging;
using MongoDB.Driver;
using Moq;
using SharpTestsEx;
using System;
using System.Reflection;
using System.Threading;
using Xunit;

namespace MementoFX.Persistence.MongoDB.Tests
{

    public class MongoDbEventStoreFixture
    {
        [Fact]
        public void Ctor_should_throw_ArgumentNullException_on_null_mongoDatabase_and_value_of_parameter_should_be_mongoDatabase()
        {
            var eventDispatcher = new Mock<IEventDispatcher>().Object;
            Executing.This(() => new MongoDbEventStore(null, eventDispatcher))
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
            Executing.This(() => new MongoDbEventStore(mongoDataBase, null))
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
            var sut = new MongoDbEventStore(mock, bus);

            Assert.Equal(mock, MongoDbEventStore.MongoDatabase);
        }

        public class DomainEventForTest : DomainEvent
        {
            
        }

        [Fact]
        public void _Save_should_choose_appopriate_InsertOne_from_IMongoCollection()
        {
            var bus = new Mock<IEventDispatcher>().Object;
            var dbMock = new Mock<IMongoDatabase>();
            var collectionMock = new Mock<IMongoCollection<DomainEventForTest>>();
            dbMock.Setup(db => db.GetCollection<DomainEventForTest>(typeof(DomainEventForTest).Name, null))
                .Returns(collectionMock.Object);
            var sut = new MongoDbEventStore(dbMock.Object, bus);
            var method = sut.GetType().GetMethod("_Save", BindingFlags.NonPublic | BindingFlags.Instance);
            var domainEventForTest = new DomainEventForTest();

            method.Invoke(sut, new object[] {domainEventForTest});

            collectionMock.Verify(c => c.InsertOne(domainEventForTest, null, default(CancellationToken)), Times.Once);
        }
    }
}
