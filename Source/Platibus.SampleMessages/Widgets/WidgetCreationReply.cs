using System.Runtime.Serialization;

namespace Platibus.SampleMessages.Widgets
{
    [DataContract]
    public class WidgetCreationReply : ResponseDocument<WidgetResource>
    {
        public WidgetCreationReply(WidgetResource data) : base(data)
        {
        }

        public WidgetCreationReply()
        {
        }
    }
}
