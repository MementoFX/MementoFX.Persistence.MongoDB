using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Memento.Domain;
using Memento.Messaging;
using System.Reflection;
using System.Threading;

namespace Memento.Persistence.MongoDB
{
    /// <summary>
    /// Provides an implementation of a Memento event store
    /// using MongoDB as the storage
    /// </summary>
    public class MongoDbEventStore : EventStore
    {
        /// <summary>
        /// Gets or sets the reference to the Mongo client
        /// </summary>
        public static IMongoClient MongoClient { get; private set; }

        /// <summary>
        /// Gets or sets the reference to the Mongo database instance
        /// </summary>
        public static IMongoDatabase MongoDatabase { get; private set; }

        /// <summary>
        /// Creates a new instance of the event store
        /// </summary>
        /// <param name="eventDispatcher">The event dispatcher to be used by the instance</param>
        public MongoDbEventStore(IEventDispatcher eventDispatcher)
            : base(eventDispatcher)
        {
            if (MongoClient == null)
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["EventStore"].ConnectionString;
                var databaseName = MongoUrl.Create(connectionString).DatabaseName;
                MongoClient = new MongoClient(connectionString);
                MongoDatabase = MongoClient.GetDatabase(databaseName);
            }
        }

        /// <summary>
        /// Creates a new instance of the event store
        /// </summary>
        /// <param name="mongoDatabase">The document store to be used by the instance</param>
        /// <param name="eventDispatcher">The event dispatcher to be used by the instance</param>
        public MongoDbEventStore(IMongoDatabase mongoDatabase, IEventDispatcher eventDispatcher)
            : base(eventDispatcher)
        {
            if (mongoDatabase == null)
                throw new ArgumentNullException("mongoDatabase");

            MongoDatabase = mongoDatabase;
            MongoClient = mongoDatabase.Client;
        }

        /// <summary>
        /// Retrieves all events of a type which satisfy a requirement
        /// </summary>
        /// <typeparam name="T">The type of the event</typeparam>
        /// <param name="filter">The requirement</param>
        /// <returns>The events which satisfy the given requirement</returns>
        public override IEnumerable<T> Find<T>(Func<T, bool> filter)
        {
            var collectionName = typeof(T).Name;
            var events = MongoDatabase.GetCollection<T>(collectionName).AsQueryable().Where(filter);

            return events;
        }

        /// <summary>
        /// Saves an event into the store
        /// </summary>
        /// <param name="event">The event to be saved</param>
        protected override void _Save(DomainEvent @event)
        {
            var eventType = @event.GetType();
            var collectionName = eventType.Name;
            MethodInfo getCollectionMethod = typeof(IMongoDatabase).GetMethod("GetCollection");
            MethodInfo getCollectionGeneric = getCollectionMethod.MakeGenericMethod(eventType);

            var mongoCollection = getCollectionGeneric.Invoke(MongoDatabase, new object[] { collectionName, null });

            var mongoCollectionType = mongoCollection.GetType();

            var mi = mongoCollectionType.GetMethods().Single(m =>
            {
                if (m.Name != "InsertOne") return false;
                var parameters = m.GetParameters();
                return parameters.Length == 3 &&
                       parameters[0].ParameterType == eventType &&
                       parameters[1].ParameterType == typeof(InsertOneOptions) &&
                       parameters[2].ParameterType == typeof(CancellationToken);
            });

            mi.Invoke(mongoCollection, new object[] { @event, null, null });
        }

        /// <summary>
        /// Retrieves the desired events from the store
        /// </summary>
        /// <typeparam name="T">The aggregate type for which to retrieve the events</typeparam>
        /// <param name="aggregateId">The aggregate id</param>
        /// <param name="pointInTime">The point in time up to which the events have to be retrieved</param>
        /// <param name="eventDescriptors">The descriptors defining the events to be retrieved</param>
        /// <param name="timelineId">The id of the timeline from which to retrieve the events</param>
        /// <returns>The list of the retrieved events</returns>
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

                for (int i = 0; i <descriptorsGroup.Count(); i++)
                {
                    var eventDescriptor = descriptorsGroup.ElementAt(i);
                    var filterBuilt = builder.Eq(eventDescriptor.AggregateIdPropertyName, new BsonBinaryData(aggregateId));
                    filters.Add(filterBuilt);
                }

                var resultFilter = Builders<BsonDocument>.Filter.Or(filters);

                resultFilter = resultFilter & builder.Lte("TimeStamp", pointInTime);

                if (!timelineId.HasValue)
                {
                    resultFilter = resultFilter & builder.Eq("TimelineId", BsonNull.Value);
                }
                else
                {
                    resultFilter = resultFilter & (builder.Eq("TimelineId", BsonNull.Value) | builder.Eq("TimelineId", new BsonBinaryData(timelineId.Value)));
                }

                var collection = MongoDatabase.GetCollection<BsonDocument>(collectionName);

                using (var cursor = collection.FindSync(resultFilter))
                {
                    while (cursor.MoveNext())
                    {
                        var batch = cursor.Current;

                        foreach (var document in batch)
                        {
                            var evt = BsonSerializer.Deserialize(document, eventType);
                            events.Add((DomainEvent)evt);
                        }
                    }
                }
            }

            return events.OrderBy(e => e.TimeStamp);
        }
    }
}
