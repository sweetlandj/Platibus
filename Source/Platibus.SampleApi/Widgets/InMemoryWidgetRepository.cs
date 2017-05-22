using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Platibus.SampleApi.Widgets
{
    public class InMemoryWidgetRepository : IWidgetRepository
    {
        private readonly ConcurrentDictionary<string, Widget> _widgets = new ConcurrentDictionary<string, Widget>();

        public Task Add(Widget widget)
        {
            if (widget == null) throw new ArgumentNullException("widget");
            if (string.IsNullOrWhiteSpace(widget.Id))
            {
                widget.Id = Guid.NewGuid().ToString();
            }

            if (!_widgets.TryAdd(widget.Id, CopyOf(widget)))
            {
                throw new WidgetAlreadyExistsException(widget.Id);
            }
            return Task.FromResult(0);
        }

        public Task Update(Widget widget)
        {
            if (widget == null)
            {
                throw new ArgumentNullException("widget");
            }

            if (string.IsNullOrWhiteSpace(widget.Id))
            {
                throw new ArgumentException("ID is required");
            }

            _widgets[widget.Id] = CopyOf(widget);
            return Task.FromResult(0);
        }

        public Task<Widget> Get(string id)
        {
            Widget widget;
            _widgets.TryGetValue(id, out widget);
            if (widget == null) throw new WidgetNotFoundException(id);
            return Task.FromResult(CopyOf(widget));
        }

        public Task<IEnumerable<Widget>> List()
        {
            var widgets = _widgets.Values.Select(CopyOf).ToList();
            return Task.FromResult<IEnumerable<Widget>>(widgets);
        }

        public Task Remove(string id)
        {
            Widget widget;
            if (!_widgets.TryRemove(id, out widget))
            {
                throw new WidgetNotFoundException(id);
            }

            return Task.FromResult(0);
        }

        private static Widget CopyOf(Widget widget)
        {
            return new Widget
            {
                Id = widget.Id,
                PartNumber = widget.PartNumber,
                Description = widget.Description,
                Length = widget.Length,
                Width = widget.Width,
                Height = widget.Height
            };
        }
    }
}