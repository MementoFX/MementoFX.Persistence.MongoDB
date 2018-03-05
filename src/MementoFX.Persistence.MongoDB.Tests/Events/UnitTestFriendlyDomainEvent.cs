using System;

namespace MementoFX.Persistence.MongoDB.Tests.Events
{
    public class UnitTestFriendlyDomainEvent : DomainEvent
    {
        public void SetTimeStamp(DateTime pointInTime)
        {
            var type = typeof(DomainEvent);
            var property = type.GetProperty("TimeStamp");
            property.SetValue(this, pointInTime);
        }
    }
}
