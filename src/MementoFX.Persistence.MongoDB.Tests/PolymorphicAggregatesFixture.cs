//using MementoFX.Messaging;
//using MementoFX.Persistence.MongoDB.Tests.Model;
//using Xunit;
//using MongoDB.Driver;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MementoFX.Persistence.MongoDB.Tests
//{
//    
//    public class PolymorphicAggregatesFixture
//    {
//        [Fact]
//        public void Test_Polymorphic_aggregate_fetch()
//        {
//            var evtConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["EventStore"].ConnectionString;
//            var evtDatabaseName = MongoUrl.Create(evtConnectionString).DatabaseName;
//            var evtMongoClient = new MongoClient(evtConnectionString);
//            var evtMongoDatabase = evtMongoClient.GetDatabase(evtDatabaseName);

//            var docConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DocumentStore"].ConnectionString;
//            var docDatabaseName = MongoUrl.Create(docConnectionString).DatabaseName;
//            var docMongoClient = new MongoClient(docConnectionString);
//            var docMongoDatabase = docMongoClient.GetDatabase(docDatabaseName);

//            var eventDispatcher = new Mock<IEventDispatcher>().Object;
//            var eventStore = new MongoDbEventStore(evtMongoDatabase, eventDispatcher);
//            var sut = new MongoDbRepository(eventStore, docMongoDatabase);

//            var invoice1 = new IncomingInvoice()
//            {
//                DateOfIssue = DateTime.Now,
//                Number = Guid.NewGuid().ToString(),
//                Price = 101,
//                Taxes = 0,
//                SupplierName = "Acme, inc"
//            };
//            sut.Save(invoice1);

//            var invoice2 = new OutgoingInvoice()
//            {
//                DateOfIssue = DateTime.Now,
//                Number = Guid.NewGuid().ToString(),
//                Price = 101,
//                Taxes = 0,
//                CustomerName = "Acme, ltd"
//            };
//            sut.Save(invoice2);

//            var fetchedInvoice = sut.GetById<Invoice>(invoice1.Id);

//            Assert.IsNotNull(fetchedInvoice);
//            Assert.IsInstanceOfType(fetchedInvoice, typeof(IncomingInvoice));
//            Assert.Equal(invoice1.Id, fetchedInvoice.Id);
//        }
//    }
//}
