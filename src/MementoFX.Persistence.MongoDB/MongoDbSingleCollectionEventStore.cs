using System;
using System.Collections.Generic;
using System.Linq;
using MementoFX.Messaging;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MementoFX.Persistence.MongoDB
{
    public class MongoDbSingleCollectionEventStore : EventStore
    {
        public static IMongoClient MongoClient { get; private set; }

        public static IMongoCollection<BsonDocument> MongoCollection { get; private set; }

        public static IMongoDatabase MongoDatabase { get; private set; }

        public MongoDbSingleCollectionEventStore(IEventDispatcher eventDispatcher, string connectionString)
            : base(eventDispatcher)
        {
            if (MongoClient == null)
            {
                var databaseName = MongoUrl.Create(connectionString).DatabaseName;
                MongoClient = new MongoClient(connectionString);
                MongoDatabase = MongoClient.GetDatabase(databaseName);
                MongoCollection = MongoDatabase.GetCollection<BsonDocument>("DomainEvents");
            }
        }

        public MongoDbSingleCollectionEventStore(IEventDispatcher eventDispatcher, IMongoDatabase mongoDatabase)
            : base(eventDispatcher)
        {
            if (mongoDatabase == null)
                throw new ArgumentNullException("mongoDatabase");

            MongoDatabase = mongoDatabase;

            MongoClient = mongoDatabase.Client;
            MongoCollection = MongoDatabase.GetCollection<BsonDocument>("DomainEvents");
        }

        public MongoDbSingleCollectionEventStore(IEventDispatcher eventDispatcher, IMongoDatabase mongoDatabase, string collectionName)
            : base(eventDispatcher)
        {
            if (mongoDatabase == null)
                throw new ArgumentNullException("mongoDatabase");

            MongoDatabase = mongoDatabase;

            MongoClient = mongoDatabase.Client;
            MongoCollection = MongoDatabase.GetCollection<BsonDocument>(collectionName ?? "DomainEvents");
        }

        public override IEnumerable<T> Find<T>(Func<T, bool> filter)
        {
            var clrType = typeof(T);
            var events = new List<T>();

            var builder = Builders<BsonDocument>.Filter;

            var clrTypeFilter = builder.Eq("ClrType", clrType.FullName);

            using (var cursor = MongoCollection.FindSync(clrTypeFilter))
            {
                while (cursor.MoveNext())
                {
                    var batch = cursor.Current;

                    foreach (var document in batch)
                    {
                        var evt = BsonSerializer.Deserialize(document["DomainEvent"].AsBsonDocument, clrType);

                        events.Add((T)evt);
                    }
                }
            }

            return events.Where(filter);
        }

        public override IEnumerable<DomainEvent> RetrieveEvents(Guid aggregateId, DateTime pointInTime, IEnumerable<EventMapping> eventDescriptors, Guid? timelineId)
        {
            var events = new List<DomainEvent>();

            var descriptorsGrouped = eventDescriptors
                .GroupBy(k => k.EventType);

            foreach (var descriptorsGroup in descriptorsGrouped)
            {
                var eventType = descriptorsGroup.Key;
                var collectionName = eventType.Name;

                var builder = Builders<BsonDocument>.Filter;
                var filters = new List<FilterDefinition<BsonDocument>>();


                var resultFilter = builder.Eq("ClrType", eventType.FullName);

                for (int i = 0; i < descriptorsGroup.Count(); i++)
                {
                    var eventDescriptor = descriptorsGroup.ElementAt(i);
                    var filterBuilt = builder.Eq("DomainEvent." + eventDescriptor.AggregateIdPropertyName, new BsonBinaryData(aggregateId));
                    filters.Add(filterBuilt);
                }

                resultFilter = resultFilter & Builders<BsonDocument>.Filter.Or(filters);

                resultFilter = resultFilter & builder.Lte("DomainEvent.TimeStamp", pointInTime);

                if (!timelineId.HasValue)
                {
                    resultFilter = resultFilter & builder.Eq("DomainEvent.TimelineId", BsonNull.Value);
                }
                else
                {
                    resultFilter = resultFilter & (builder.Eq("DomainEvent.TimelineId", BsonNull.Value) | builder.Eq("DomainEvent.TimelineId", new BsonBinaryData(timelineId.Value)));
                }

                using (var cursor = MongoCollection.FindSync(resultFilter))
                {
                    while (cursor.MoveNext())
                    {
                        var batch = cursor.Current;

                        foreach (var document in batch)
                        {
                            var evt = BsonSerializer.Deserialize(document["DomainEvent"].AsBsonDocument, eventType);

                            events.Add((DomainEvent)evt);
                        }
                    }
                }
            }

            return events.OrderBy(e => e.TimeStamp);
        }

        protected override void _Save(DomainEvent @event)
        {
            var bsonDocument = new BsonDocument();
            bsonDocument["ClrType"] = @event.GetType().FullName;
            bsonDocument["DomainEvent"] = @event.ToBsonDocument();

            MongoCollection.InsertOne(bsonDocument);
        }
    }
}
