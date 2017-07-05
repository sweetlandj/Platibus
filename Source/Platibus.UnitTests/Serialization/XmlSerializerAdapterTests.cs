using Platibus.Serialization;
using Xunit;

namespace Platibus.UnitTests.Serialization
{
    [Trait("Category", "UnitTests")]
    public class XmlSerializerAdapterTests : SerializerTests
    {
        public XmlSerializerAdapterTests() : base(new XmlSerializerAdapter())
        {
        }
    }
}