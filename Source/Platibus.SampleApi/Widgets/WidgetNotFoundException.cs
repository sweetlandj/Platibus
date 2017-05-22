using System;

namespace Platibus.SampleApi.Widgets
{
    [Serializable]
    public class WidgetNotFoundException : Exception
    {
        private readonly string _widgetId;

        public string WidgetId { get { return _widgetId; } }

        public WidgetNotFoundException(string widgetId) : base("Widget " + widgetId + " not found")
        {
            _widgetId = widgetId;
        }
    }
}