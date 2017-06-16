using Platibus.Serialization;

namespace Platibus.UnitTests.Serialization
{
    public class XmlSerializerAdapterTests : SerializerTests
    {
        public XmlSerializerAdapterTests() : base(new XmlSerializerAdapter())
        {
        }
    }
}