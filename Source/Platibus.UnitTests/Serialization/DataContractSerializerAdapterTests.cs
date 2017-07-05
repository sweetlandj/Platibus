using Platibus.Serialization;
using Xunit;

namespace Platibus.UnitTests.Serialization
{
    [Trait("Category", "UnitTests")]
    public class DataContractSerializerAdapterTests : SerializerTests
    {
        public DataContractSerializerAdapterTests() : base(new DataContractSerializerAdapter())
        {
        }
    }
}