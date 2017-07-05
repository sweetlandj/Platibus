using Platibus.Serialization;
using Xunit;

namespace Platibus.UnitTests.Serialization
{
    [Trait("Category", "UnitTests")]
    public class DataContractJsonSerializerAdapterTests : SerializerTests
    {
        public DataContractJsonSerializerAdapterTests() : base(new DataContractJsonSerializerAdapter())
        {
        }

        [Fact]
        public void MessageCanBeDeserializedWithNewtonsoftJsonSerializer()
        {
            GivenDataContractMessage();
            WhenMessageIsSerialized();
            WhenMessageIsDeserializedWithNewtonsoftJsonSerializer();
            AssertDeserializedMessageIsTheSame();
        }

        protected void WhenMessageIsDeserializedWithNewtonsoftJsonSerializer()
        {
            Deserialized = new NewtonsoftJsonSerializer().Deserialize<DataContractMessage>(Serialized);
        }
    }
}