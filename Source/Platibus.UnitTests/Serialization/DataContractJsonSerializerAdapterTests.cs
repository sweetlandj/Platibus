using Platibus.Serialization;

namespace Platibus.UnitTests.Serialization
{
    public class DataContractJsonSerializerAdapterTests : SerializerTests
    {
        public DataContractJsonSerializerAdapterTests() : base(new DataContractJsonSerializerAdapter())
        {
        }
    }
}