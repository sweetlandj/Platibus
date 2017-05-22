using System;

namespace Platibus.SampleApi.Widgets
{
    [Serializable]
    public class WidgetAlreadyExistsException : Exception
    {
        private readonly string _widgetId;

        public string WidgetId { get { return _widgetId; } }

        public WidgetAlreadyExistsException(string widgetId) : base("Widget " + widgetId + " already exists")
        {
            _widgetId = widgetId;
        }
    }
}