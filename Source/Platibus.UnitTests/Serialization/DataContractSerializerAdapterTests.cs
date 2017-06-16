using Platibus.Serialization;

namespace Platibus.UnitTests.Serialization
{
    public class DataContractSerializerAdapterTests : SerializerTests
    {
        public DataContractSerializerAdapterTests() : base(new DataContractSerializerAdapter())
        {
        }
    }
}