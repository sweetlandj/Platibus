using System;
using System.Runtime.Serialization;

namespace Platibus.SampleMessages.Widgets
{
    [DataContract]
    public class WidgetEvent
    {
        [DataMember(Name = "ts")]
        public DateTime Timestamp { get; set; }

        [DataMember(Name = "requestor")]
        public string Requestor { get; set; }

        [DataMember(Name = "event")]
        public string EventType { get; set; }

        [DataMember(Name = "data")]
        public WidgetResource Data { get; set; }

        public WidgetEvent()
        {
            Timestamp = DateTime.UtcNow;
        }

        public WidgetEvent(string eventType, WidgetResource data, string requestor = null)
        {
            Timestamp = DateTime.UtcNow;
            Requestor = requestor;
            EventType = eventType;
            Data = data;
        }
    }
}
