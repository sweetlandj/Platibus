using Platibus.Serialization;
using Xunit;

namespace Platibus.UnitTests.Serialization
{
    public class NewtonsoftJsonSerializerTests : SerializerTests
    {
        public NewtonsoftJsonSerializerTests() : base(new NewtonsoftJsonSerializer())
        {
        }

        [Fact]
        public void MessageCanBeDeserializedWithDataContractJsonSerializer()
        {
            GivenDataContractMessage();
            WhenMessageIsSerialized();
            WhenMessageIsDeserializedWithDataContractJsonSerializer();
            AssertDeserializedMessageIsTheSame();
        }

        protected void WhenMessageIsDeserializedWithDataContractJsonSerializer()
        {
            Deserialized = new DataContractJsonSerializerAdapter().Deserialize<DataContractMessage>(Serialized);
        }
    }
}
