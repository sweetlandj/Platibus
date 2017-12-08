using System;
using Platibus.SampleMessages.Widgets;

namespace Platibus.SampleApi.Widgets
{
    public static class WidgetMap
    {
        public static Widget ToEntity(WidgetResource resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            var attributes = resource.Attributes ?? new WidgetAttributes();
            return new Widget
            {
                Id = resource.Id,
                PartNumber = attributes.PartNumber,
                Description = attributes.Description,
                Length = attributes.Length.GetValueOrDefault(),
                Width = attributes.Width.GetValueOrDefault(),
                Height = attributes.Height.GetValueOrDefault()
            };
        }

        public static void ApplyUpdates(Widget widget, WidgetResource updates)
        {
            if (updates == null) return;
            var attributeUpdates = updates.Attributes ?? new WidgetAttributes();
            ApplyUpdate(attributeUpdates.PartNumber, v => widget.PartNumber = v);
            ApplyUpdate(attributeUpdates.Description, v => widget.Description = v);
            ApplyUpdate(attributeUpdates.Length, v => widget.Length = v.GetValueOrDefault());
            ApplyUpdate(attributeUpdates.Width, v => widget.Width = v.GetValueOrDefault());
            ApplyUpdate(attributeUpdates.Height, v => widget.Height = v.GetValueOrDefault());
        }

        public static void ApplyUpdate<TValue>(TValue value, Action<TValue> applyUpdate)
        {
            if (value == null) return;
            applyUpdate(value);
        }

        public static WidgetResource ToResource(Widget entity)
        {
            return new WidgetResource
            {
                Id = entity.Id,
                Attributes = new WidgetAttributes
                {
                    PartNumber = entity.PartNumber,
                    Description = entity.Description,
                    Length = entity.Length,
                    Width = entity.Width,
                    Height = entity.Height
                }
            };
        }
    }
}