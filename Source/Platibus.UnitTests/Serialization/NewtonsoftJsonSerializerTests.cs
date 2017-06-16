using Platibus.Serialization;

namespace Platibus.UnitTests.Serialization
{
    public class NewtonsoftJsonSerializerTests : SerializerTests
    {
        public NewtonsoftJsonSerializerTests() : base(new NewtonsoftJsonSerializer())
        {
        }
    }
}
