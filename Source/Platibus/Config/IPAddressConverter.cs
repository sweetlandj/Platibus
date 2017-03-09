using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Net;

namespace Platibus.Config
{
    internal class IPAddressConverter : ConfigurationConverterBase
    {
        public override bool CanConvertTo(ITypeDescriptorContext ctx, Type type)
        {
            return type == typeof(string);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext ctx, Type type)
        {
            return type == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var strVal = value as string;
            return string.IsNullOrWhiteSpace(strVal) ? null : IPAddress.Parse(strVal);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null) return null;
            return value.ToString();
        }
    }
}
