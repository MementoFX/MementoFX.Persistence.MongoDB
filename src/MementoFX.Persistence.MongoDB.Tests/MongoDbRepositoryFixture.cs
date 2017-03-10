//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SharpTestsEx;
//using Memento.Persistence.MongoDB.Tests.Events;
//using Memento.Messaging;
//using Moq;
//using MongoDB.Driver;
//using Memento.Persistence.MongoDB.Tests.Model;

//namespace Memento.Persistence.MongoDB.Tests
//{
//    [TestClass]
//    public class MongoDbRepositoryFixture
//    {
//        private IMongoDatabase MongoDatabase;
//        private IMongoClient MongoClient;
//        private MongoDbEventStore EventStore;

//        [TestInitialize]
//        public void SetUp()
//        {
//            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["EventStore"].ConnectionString;
//            var databaseName = MongoUrl.Create(connectionString).DatabaseName;
//            MongoClient = new MongoClient(connectionString);
//            MongoDatabase = MongoClient.GetDatabase(databaseName);

//            var bus = new Mock<IEventDispatcher>().Object;
//            var eventStore = new MongoDbEventStore(MongoDatabase, bus);
//            EventStore = eventStore;
//        }

//        [TestCleanup]
//        public void CleanUp()
//        {

//        }

//        [TestMethod]
//        public void Ctor_should_throw_ArgumentNullException_on_null_eventStore_and_value_of_parameter_should_be_eventStore()
//        {
//            Executing.This(() => new MongoDbRepository(null))
//                .Should()
//                .Throw<ArgumentNullException>()
//                .And
//                .ValueOf
//                .ParamName
//                .Should()
//                .Be
//                .EqualTo("eventStore");
//        }

//        [TestMethod]
//        public void Test_EventReplaying_evaluating_CurrentAccountBalance_using_a_stream_containing_past_events_only()
//        {
//            var currentAccountId = Guid.NewGuid();
//            var accountOpening = new AccountOpenedEvent
//            {
//                CurrentAccountId = currentAccountId,
//                Balance = 200
//            };
//            EventStore.Save(accountOpening);

//            var withdrawal1 = new WithdrawalEvent()
//            {
//                CurrentAccountId = currentAccountId,
//                Amount = 100
//            };
//            withdrawal1.SetTimeStamp(DateTime.Now.AddMonths(1));
//            EventStore.Save(withdrawal1);

//            var sut = new MongoDbRepository(EventStore, MongoDatabase);
//            var currentAccount = sut.GetById<CurrentAccount>(currentAccountId, DateTime.Now.AddMonths(2));

//            Assert.AreEqual<decimal>(100, currentAccount.Balance);
//        }

//        [TestMethod]
//        public void Test_EventReplaying_evaluating_CurrentAccountBalance_using_a_stream_containing_both_past_and_future_events()
//        {
//            var currentAccountId = Guid.NewGuid();
//            var accountOpening = new AccountOpenedEvent
//            {
//                CurrentAccountId = currentAccountId,
//                Balance = 200
//            };
//            EventStore.Save(accountOpening);

//            var withdrawal1 = new WithdrawalEvent()
//            {
//                CurrentAccountId = currentAccountId,
//                Amount = 100
//            };
//            withdrawal1.SetTimeStamp(DateTime.Now.AddMonths(1));
//            EventStore.Save(withdrawal1);

//            var withdrawal2 = new WithdrawalEvent()
//            {
//                CurrentAccountId = currentAccountId,
//                Amount = 100
//            };
//            withdrawal2.SetTimeStamp(DateTime.Now.AddMonths(3));
//            EventStore.Save(withdrawal2);

//            var sut = new MongoDbRepository(EventStore, MongoDatabase);
//            var currentAccount = sut.GetById<CurrentAccount>(currentAccountId, DateTime.Now.AddMonths(2));

//            Assert.AreEqual<decimal>(100, currentAccount.Balance);
//        }

//        [TestMethod]
//        public void Test_EventReplaying_evaluating_CurrentAccountBalance_using_a_stream_containing_past_events_only_and_a_different_timeline()
//        {
//            var currentAccountId = Guid.NewGuid();
//            var accountOpening = new AccountOpenedEvent
//            {
//                CurrentAccountId = currentAccountId,
//                Balance = 200
//            };
//            EventStore.Save(accountOpening);

//            var withdrawal1 = new WithdrawalEvent()
//            {
//                CurrentAccountId = currentAccountId,
//                Amount = 100
//            };
//            withdrawal1.SetTimeStamp(DateTime.Now.AddMonths(1));
//            EventStore.Save(withdrawal1);

//            var withdrawal2 = new WithdrawalEvent()
//            {
//                CurrentAccountId = currentAccountId,
//                Amount = 100,
//                TimelineId = Guid.NewGuid()
//            };
//            withdrawal2.SetTimeStamp(DateTime.Now.AddMonths(3));
//            EventStore.Save(withdrawal2);

//            var sut = new MongoDbRepository(EventStore, MongoDatabase);
//            var currentAccount = sut.GetById<CurrentAccount>(currentAccountId, DateTime.Now.AddMonths(3));

//            Assert.AreEqual<decimal>(100, currentAccount.Balance);
//        }

//        [TestMethod]
//        public void Test_Timeline_specific_EventReplaying_evaluating_CurrentAccountBalance_using_a_stream_containing_both_past_and_future_events()
//        {
//            var currentAccountId = Guid.NewGuid();
//            var timelineId = Guid.NewGuid();
//            var accountOpening = new AccountOpenedEvent
//            {
//                CurrentAccountId = currentAccountId,
//                Balance = 200,
//                TimelineId = timelineId
//            };
//            EventStore.Save(accountOpening);

//            var withdrawal1 = new WithdrawalEvent()
//            {
//                CurrentAccountId = currentAccountId,
//                Amount = 100
//            };
//            withdrawal1.SetTimeStamp(DateTime.Now.AddMonths(1));
//            EventStore.Save(withdrawal1);

//            var withdrawal2 = new WithdrawalEvent()
//            {
//                CurrentAccountId = currentAccountId,
//                Amount = 50,
//                TimelineId = timelineId
//            };
//            withdrawal2.SetTimeStamp(DateTime.Now.AddMonths(2));
//            EventStore.Save(withdrawal2);


//            var sut = new MongoDbRepository(EventStore, MongoDatabase);
//            var currentAccount = sut.GetById<CurrentAccount>(currentAccountId, timelineId, DateTime.Now.AddMonths(3));

//            Assert.AreEqual<decimal>(50, currentAccount.Balance);
//        }
//    }
//}
