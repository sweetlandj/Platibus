using System;
using System.Collections.Generic;
using KellermanSoftware.CompareNetObjects;
using Platibus.Serialization;
using Xunit;

namespace Platibus.UnitTests.Serialization
{
    [Trait("Category", "UnitTests")]
    public abstract class SerializerTests
    {
        protected readonly ISerializer Serializer;

        protected DataContractMessage Message;
        protected string Serialized;
        protected DataContractMessage Deserialized;

        protected SerializerTests(ISerializer serializer)
        {
            Serializer = serializer;
        }

        [Fact]
        public void NullMessagesSerializeToNull()
        {
            GivenNullMessage();
            WhenMessageIsSerialized();
            Assert.Null(Serialized);
        }

        [Fact]
        public void NullStringsDeserializeToNull()
        {
            GivenNullMessage();
            WhenMessageIsDeserialized();
            Assert.Null(Deserialized);
        }

        [Fact]
        public void MessagesShouldRoundTrip()
        {
            GivenDataContractMessage();
            WhenMessageIsSerialized();
            WhenMessageIsDeserialized();
            AssertDeserializedMessageIsTheSame();
        }

        public void GivenNullMessage()
        {
            Message = null;
        }

        public void GivenNullString()
        {
            Serialized = null;
        }

        public void GivenDataContractMessage()
        {
            Message = new DataContractMessage
            {
                GuidValue = Guid.NewGuid(),
                DateValue = DateTime.UtcNow.TruncateMillis(),
                BoolValue = true,
                IntValue = 192873,
                FloatValue = 18197.942934f,
                DoubleValue = 1298199121394823.13912384d,
                DecimalValue = 100.63m,
                StringValue = "Hello, world!",
                NestedMessages = new List<NestedDataContractMessage>
                {
                    new NestedDataContractMessage
                    {
                        GuidValue = Guid.NewGuid(),
                        DateValue = DateTime.UtcNow.AddMinutes(-5).TruncateMillis(),
                        BoolValue = false,
                        IntValue = 291290,
                        FloatValue = 98.35811f,
                        DoubleValue = 128212671812123.59271498d,
                        DecimalValue = 71281.67m,
                        StringValue = "Hello again, world!",
                    }
                }
            };
        }

        public void WhenMessageIsSerialized()
        {
            Serialized = Serializer.Serialize(Message);
        }

        public void WhenMessageIsDeserialized()
        {
            Deserialized = Serializer.Deserialize<DataContractMessage>(Serialized);
        }

        public void AssertDeserializedMessageIsTheSame()
        {
            var result = new CompareLogic().Compare(Message, Deserialized);
            Assert.True(result.AreEqual);
        }
    }
}
