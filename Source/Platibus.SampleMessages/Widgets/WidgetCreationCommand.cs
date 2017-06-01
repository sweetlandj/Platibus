using System.Runtime.Serialization;

namespace Platibus.SampleMessages.Widgets
{
    [DataContract]
    public class WidgetCreationCommand : RequestDocument<WidgetResource>
    {
        public WidgetCreationCommand(WidgetResource data) : base(data)
        {
        }

        public WidgetCreationCommand()
        {
        }
    }
}
