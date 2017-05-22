using System.Runtime.Serialization;

namespace Platibus.SampleMessages.Widgets
{
    [DataContract]
    public class WidgetCreationRequest : Request<WidgetResource>
    {
    }
}
